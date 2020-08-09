//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Crema.Services.Domains.Actions;
using Ntreev.Crema.Services.Domains.Serializations;
using Ntreev.Crema.Services.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Ntreev.Crema.Services.Domains
{
    class DomainRestorer
    {
        private readonly DomainContext domainContext;
        private readonly string workingPath;
        private readonly Dictionary<long, DomainCompleteItemSerializationInfo> completedList = new Dictionary<long, DomainCompleteItemSerializationInfo>();
        private readonly List<DomainActionBase> actionList = new List<DomainActionBase>();
        private Dictionary<string, Authentication> authentications;
        private DateTime dateTime;

        public DomainRestorer(DomainContext domainContext, string workingPath)
        {
            this.domainContext = domainContext;
            this.workingPath = workingPath;
        }

        public async Task RestoreAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    await this.DeserializeDomainAsync();
                    this.CollectCompletedActions();
                    this.CollectPostedActions();
                    await this.RestoreDomainAsync();
                });
            }
            catch
            {
                this.Domain = null;
                throw;
            }
            finally
            {
                this.Dispose();
            }
        }

        public Domain Domain { get; private set; }

        private void CollectCompletedActions()
        {
            var domainLogger = this.Domain.Logger;
            foreach (var item in domainLogger.CompletedList)
            {
                this.completedList.Add(item.ID, item);
            }
        }

        private void CollectPostedActions()
        {
            var domainLogger = this.Domain.Logger;
            foreach (var item in domainLogger.PostedList)
            {
                if (this.completedList.ContainsKey(item.ID) == true)
                {
                    var type = Type.GetType(item.Type);
                    var path = Path.Combine(this.workingPath, $"{item.ID}");
                    var action = (DomainActionBase)this.Serializer.Deserialize(path, type, ObjectSerializerSettings.Empty);
                    this.actionList.Add(action);
                }
            }
        }

        private async Task DeserializeDomainAsync()
        {
            var userContext = this.domainContext.CremaHost.UserContext;
            var domainLogger = new DomainLogger(this.domainContext.Serializer, this.workingPath);
            var domainSerializationInfo = domainLogger.DomainInfo;
            var domainType = Type.GetType(domainSerializationInfo.DomainType);
            var domainInfo = domainSerializationInfo.DomainInfo;
            var source = domainLogger.Source;
            var users = await userContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from User user in userContext.Users
                            select new Authentication(new UserAuthenticationProvider(user, true));
                return query.ToArray();
            });
            this.authentications = users.ToDictionary(item => item.ID);
            this.Domain = (Domain)Activator.CreateInstance(domainType, domainSerializationInfo, source);
            this.Domain.Logger = domainLogger;
            this.Domain.Context = this.domainContext;
        }

        private async Task RestoreDomainAsync()
        {
            this.Domain.Host = new DummyDomainHost(this.Domain);
            this.Domain.Logger.IsEnabled = false;

            foreach (var item in this.actionList)
            {
                var authentication = this.authentications[item.UserID];
                try
                {
                    if (!(item is DomainActionBase action))
                        throw new Exception();

                    this.Domain.DateTimeProvider = this.GetTime;
                    this.dateTime = action.AcceptTime;
                    this.Domain.Logger.ID = item.ID;

                    if (item is NewRowAction newRowAction)
                    {
                        await this.Domain.NewRowAsync(authentication, newRowAction.Rows);
                    }
                    else if (item is RemoveRowAction removeRowAction)
                    {
                        await this.Domain.RemoveRowAsync(authentication, removeRowAction.Rows);
                    }
                    else if (item is SetRowAction setRowAction)
                    {
                        await this.Domain.SetRowAsync(authentication, setRowAction.Rows);
                    }
                    else if (item is SetPropertyAction setPropertyAction)
                    {
                        await this.Domain.SetPropertyAsync(authentication, setPropertyAction.PropertyName, setPropertyAction.Value);
                    }
                    else if (item is EnterAction joinAction)
                    {
                        await this.Domain.EnterAsync(authentication, joinAction.AccessType);
                    }
                    else if (item is LeaveAction disjoinAction)
                    {
                        await this.Domain.LeaveAsync(authentication);
                    }
                    else if (item is KickAction kickAction)
                    {
                        await this.Domain.KickAsync(authentication, kickAction.TargetID, kickAction.Comment);
                    }
                    else if (item is SetOwnerAction setOwnerAction)
                    {
                        await this.Domain.SetOwnerAsync(authentication, setOwnerAction.TargetID);
                    }
                    else if (item is BeginUserEditAction beginUserEditAction)
                    {
                        await this.Domain.BeginUserEditAsync(authentication, beginUserEditAction.Location);
                    }
                    else if (item is EndUserEditAction endUserEditAction)
                    {
                        await this.Domain.EndUserEditAsync(authentication);
                    }
                    else
                    {
                        throw new NotImplementedException(item.GetType().Name);
                    }
                }
                finally
                {
                    this.Domain.DateTimeProvider = null;
                    await Task.Delay(1);
                }
            }

            this.Domain.Logger.ID = this.Domain.Logger.PostedList.Count;
            this.Domain.Logger.IsEnabled = true;
            this.Domain.Host = null;
        }

        private DateTime GetTime()
        {
            return this.dateTime;
        }

        private IObjectSerializer Serializer => this.domainContext.Serializer;

        private void Dispose()
        {
            if (this.authentications == null)
                return;
            foreach (var item in this.authentications)
            {
                item.Value.InvokeExpiredEvent(Authentication.SystemID);
            }
        }

        #region DummyDomainHost

        class DummyDomainHost : IDomainHost
        {
            public DummyDomainHost(Domain domain)
            {

            }

            public void Attach(Domain domain)
            {
                throw new NotImplementedException();
            }

            public void Detach()
            {
                throw new NotImplementedException();
            }

            public void ValidateDelete(Authentication authentication, bool isCanceled)
            {
                throw new NotImplementedException();
            }

            public Task DeleteAsync(Authentication authentication, bool isCanceled)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}

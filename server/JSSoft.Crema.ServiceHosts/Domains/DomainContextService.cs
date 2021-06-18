// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Domains
{
    class DomainContextService : CremaServiceItemBase<IDomainContextEventCallback>, IDomainContextService
    {
        private long index = 0;
        private Peer peer;

        public DomainContextService(CremaService service, IDomainContextEventCallback callback)
            : base(service, callback)
        {
            this.UserContext = this.CremaHost.GetService(typeof(IUserContext)) as IUserContext;
            this.DomainContext = this.CremaHost.GetService(typeof(IDomainContext)) as IDomainContext;
            this.DataBaseContext = this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            this.LogService.Debug($"{nameof(DomainContextService)} Constructor");
        }

        public async Task DisposeAsync()
        {
            if (this.peer != null)
            {
                await this.DetachEventHandlersAsync(this.peer.ID);
                this.peer = null;
            }
        }

        public async Task<ResultBase<DomainContextMetaData>> SubscribeAsync(Guid token)
        {
            var value = await this.AttachEventHandlersAsync(token);
            this.peer = Peer.GetPeer(token);
            this.LogService.Debug($"[{token}] {nameof(DomainContextService)} {nameof(SubscribeAsync)}");
            return new ResultBase<DomainContextMetaData>()
            {
                Value = value,
                SignatureDate = new SignatureDateProvider($"{token}")
            };
        }

        public async Task<ResultBase> UnsubscribeAsync(Guid token)
        {
            if (this.peer == null)
                throw new InvalidOperationException();
            if (this.peer.ID != token)
                throw new ArgumentException("invalid token", nameof(token));
            await this.DetachEventHandlersAsync(token);
            this.peer = null;
            this.LogService.Debug($"[{token}] {nameof(DomainContextService)} {nameof(UnsubscribeAsync)}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider($"{token}").Provide()
            };
        }

        public async Task<ResultBase<DomainMetaData[]>> GetMetaDataAsync(Guid dataBaseID)
        {
            var result = new ResultBase<DomainMetaData[]>();
            result.Value = await this.DomainContext.Dispatcher.InvokeAsync(() =>
            {
                var metaData = this.DomainContext.GetMetaData();
                var metaDataList = new List<DomainMetaData>(this.DomainContext.Domains.Count);
                foreach (var item in metaData.Domains)
                {
                    if (item.DomainInfo.DataBaseID == dataBaseID)
                        metaDataList.Add(item);
                }
                return metaDataList.ToArray();
            });
            result.SignatureDate = SignatureDate.Empty;
            return result;
        }

        public async Task<ResultBase<DomainRowInfo[]>> SetRowAsync(Guid authenticationToken, Guid domainID, DomainRowInfo[] rows)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase<DomainRowInfo[]>();
            var domain = await this.GetDomainAsync(domainID);
            var info = await (Task<DomainResultInfo<DomainRowInfo[]>>)domain.SetRowAsync(authentication, rows);
            result.TaskID = info.ID;
            result.Value = info.Value;
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> SetPropertyAsync(Guid authenticationToken, Guid domainID, string propertyName, object value)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase();
            var domain = await this.GetDomainAsync(domainID);
            result.TaskID = await (Task<Guid>)domain.SetPropertyAsync(authentication, propertyName, value);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> BeginUserEditAsync(Guid authenticationToken, Guid domainID, DomainLocationInfo location)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase();
            var domain = await this.GetDomainAsync(domainID);
            await domain.BeginUserEditAsync(authentication, location);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> EndUserEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase();
            var domain = await this.GetDomainAsync(domainID);
            await domain.EndUserEditAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> KickAsync(Guid authenticationToken, Guid domainID, string userID, string comment)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase();
            var domain = await this.GetDomainAsync(domainID);
            result.TaskID = await (Task<Guid>)domain.KickAsync(authentication, userID, comment);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> SetOwnerAsync(Guid authenticationToken, Guid domainID, string userID)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase();
            var domain = await this.GetDomainAsync(domainID);
            await domain.SetOwnerAsync(authentication, userID);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> SetUserLocationAsync(Guid authenticationToken, Guid domainID, DomainLocationInfo location)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase();
            var domain = await this.GetDomainAsync(domainID);
            await domain.SetUserLocationAsync(authentication, location);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainRowInfo[]>> NewRowAsync(Guid authenticationToken, Guid domainID, DomainRowInfo[] rows)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase<DomainRowInfo[]>();
            var domain = await this.GetDomainAsync(domainID);
            var info = await (Task<DomainResultInfo<DomainRowInfo[]>>)domain.NewRowAsync(authentication, rows);
            result.TaskID = info.ID;
            result.Value = info.Value;
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainRowInfo[]>> RemoveRowAsync(Guid authenticationToken, Guid domainID, DomainRowInfo[] rows)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase<DomainRowInfo[]>();
            var domain = await this.GetDomainAsync(domainID);
            var info = await (Task<DomainResultInfo<DomainRowInfo[]>>)domain.RemoveRowAsync(authentication, rows);
            result.TaskID = info.ID;
            result.Value = info.Value;
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<object>> DeleteDomainAsync(Guid authenticationToken, Guid domainID, bool force)
        {
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            var result = new ResultBase<object>();
            var domain = await this.GetDomainAsync(domainID);
            result.TaskID = await (Task<Guid>)domain.DeleteAsync(authentication, force);
            result.Value = domain.Result;
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public IDomainContext DomainContext { get; }

        public IDataBaseContext DataBaseContext { get; }

        public IUserContext UserContext { get; }

        private async Task<IDomain> GetDomainAsync(Guid domainID)
        {
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            if (domain == null)
                throw new DomainNotFoundException(domainID);
            return domain;
        }

        private async Task<DomainContextMetaData> AttachEventHandlersAsync(Guid token)
        {
            var metaData = await this.DomainContext.Dispatcher.InvokeAsync(() =>
            {
                this.DomainContext.Domains.DomainsCreated += Domains_DomainsCreated;
                this.DomainContext.Domains.DomainsDeleted += Domains_DomainsDeleted;
                this.DomainContext.Domains.DomainInfoChanged += Domains_DomainInfoChanged;
                this.DomainContext.Domains.DomainStateChanged += Domains_DomainStateChanged;
                this.DomainContext.Domains.DomainUserAdded += Domains_DomainUserAdded;
                this.DomainContext.Domains.DomainUserRemoved += Domains_DomainUserRemoved;
                this.DomainContext.Domains.DomainUserLocationChanged += Domains_DomainUserLocationChanged;
                this.DomainContext.Domains.DomainUserStateChanged += Domains_DomainUserStateChanged;
                this.DomainContext.Domains.DomainUserEditBegun += Domains_DomainUserEditBegun;
                this.DomainContext.Domains.DomainUserEditEnded += Domains_DomainUserEditEnded;
                this.DomainContext.Domains.DomainOwnerChanged += Domains_DomainOwnerChanged;
                this.DomainContext.Domains.DomainRowAdded += Domains_DomainRowAdded;
                this.DomainContext.Domains.DomainRowChanged += Domains_DomainRowChanged;
                this.DomainContext.Domains.DomainRowRemoved += Domains_DomainRowRemoved;
                this.DomainContext.Domains.DomainPropertyChanged += Domains_DomainPropertyChanged;
                this.DomainContext.TaskCompleted += DomainContext_TaskCompleted;
                return this.DomainContext.GetMetaData();
            });
            this.LogService.Debug($"[{token}] {nameof(DomainContextService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }

        private async Task DetachEventHandlersAsync(Guid token)
        {
            await this.DomainContext.Dispatcher.InvokeAsync(() =>
            {
                this.DomainContext.Domains.DomainsCreated -= Domains_DomainsCreated;
                this.DomainContext.Domains.DomainsDeleted -= Domains_DomainsDeleted;
                this.DomainContext.Domains.DomainInfoChanged -= Domains_DomainInfoChanged;
                this.DomainContext.Domains.DomainStateChanged -= Domains_DomainStateChanged;
                this.DomainContext.Domains.DomainUserRemoved -= Domains_DomainUserRemoved;
                this.DomainContext.Domains.DomainUserAdded -= Domains_DomainUserAdded;
                this.DomainContext.Domains.DomainUserLocationChanged -= Domains_DomainUserLocationChanged;
                this.DomainContext.Domains.DomainUserStateChanged -= Domains_DomainUserStateChanged;
                this.DomainContext.Domains.DomainUserEditBegun -= Domains_DomainUserEditBegun;
                this.DomainContext.Domains.DomainUserEditEnded -= Domains_DomainUserEditEnded;
                this.DomainContext.Domains.DomainOwnerChanged -= Domains_DomainOwnerChanged;
                this.DomainContext.Domains.DomainRowAdded -= Domains_DomainRowAdded;
                this.DomainContext.Domains.DomainRowChanged -= Domains_DomainRowChanged;
                this.DomainContext.Domains.DomainRowRemoved -= Domains_DomainRowRemoved;
                this.DomainContext.Domains.DomainPropertyChanged -= Domains_DomainPropertyChanged;
                this.DomainContext.TaskCompleted -= DomainContext_TaskCompleted;
            });
            this.LogService.Debug($"[{token}] {nameof(DomainContextService)} {nameof(DetachEventHandlersAsync)}");
        }

        private void Domains_DomainsCreated(object sender, DomainsCreatedEventArgs e)
        {
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var metaDatas = e.MetaDatas;
            this.InvokeEvent(() => this.Callback?.OnDomainsCreated(callbackInfo, metaDatas));
        }

        private void Domains_DomainsDeleted(object sender, DomainsDeletedEventArgs e)
        {
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainIDs = e.DomainInfos.Select(item => item.DomainID).ToArray();
            var isCanceleds = e.IsCanceleds;
            var results = e.Results;
            this.InvokeEvent(() => this.Callback?.OnDomainsDeleted(callbackInfo, domainIDs, isCanceleds, results));
        }

        private void Domains_DomainInfoChanged(object sender, DomainEventArgs e)
        {
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var domainInfo = e.DomainInfo;
            this.InvokeEvent(() => this.Callback?.OnDomainInfoChanged(callbackInfo, domainID, domainInfo));
        }

        private void Domains_DomainStateChanged(object sender, DomainEventArgs e)
        {
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var domainState = e.DomainState;
            this.InvokeEvent(() => this.Callback?.OnDomainStateChanged(callbackInfo, domainID, domainState));
        }

        private void Domains_DomainUserAdded(object sender, DomainUserAddedEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var domainUserInfo = e.DomainUserInfo;
            var domainUserState = e.DomainUserState;
            var data = this.peer.Contains(e.UserID) ? e.Data : null;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnUserAdded(callbackInfo, domainID, domainUserInfo, domainUserState, data, taskID));
        }

        private void Domains_DomainUserRemoved(object sender, DomainUserRemovedEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var userID = e.DomainUserInfo.UserID;
            var ownerID = e.OwnerID;
            var removeInfo = e.RemoveInfo;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnUserRemoved(callbackInfo, domainID, userID, ownerID, removeInfo, taskID));
        }

        private void Domains_DomainUserLocationChanged(object sender, DomainUserLocationEventArgs e)
        {
            if (this.peer.Contains(e.UserID) == false)
                return;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var domainLocationInfo = e.DomainLocationInfo;
            this.InvokeEvent(() => this.Callback?.OnUserLocationChanged(callbackInfo, domainID, domainLocationInfo));
        }

        private void Domains_DomainUserStateChanged(object sender, DomainUserEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var domainUserState = e.DomainUserState;
            this.InvokeEvent(() => this.Callback?.OnUserStateChanged(callbackInfo, domainID, domainUserState));
        }

        private void Domains_DomainUserEditBegun(object sender, DomainUserLocationEventArgs e)
        {
            if (this.peer.Contains(e.UserID) == false)
                return;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var domainLocationInfo = e.DomainLocationInfo;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnUserEditBegun(callbackInfo, domainID, domainLocationInfo, taskID));
        }

        private void Domains_DomainUserEditEnded(object sender, DomainUserEventArgs e)
        {
            if (this.peer.Contains(e.UserID) == false)
                return;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnUserEditEnded(callbackInfo, domainID, taskID));
        }

        private void Domains_DomainOwnerChanged(object sender, DomainUserEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var ownerID = e.DomainUserInfo.UserID;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnOwnerChanged(callbackInfo, domainID, ownerID, taskID));
        }

        private void Domains_DomainRowAdded(object sender, DomainRowEventArgs e)
        {
            if (this.peer.Contains(e.UserID) == false)
                return;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnRowAdded(callbackInfo, domainID, rows, taskID));
        }

        private void Domains_DomainRowChanged(object sender, DomainRowEventArgs e)
        {
            if (this.peer.Contains(e.UserID) == false)
                return;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnRowChanged(callbackInfo, domainID, rows, taskID));
        }

        private void Domains_DomainRowRemoved(object sender, DomainRowEventArgs e)
        {
            if (this.peer.Contains(e.UserID) == false)
                return;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnRowRemoved(callbackInfo, domainID, rows, taskID));
        }

        private void Domains_DomainPropertyChanged(object sender, DomainPropertyEventArgs e)
        {
            if (this.peer.Contains(e.UserID) == false)
                return;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var domainID = e.DomainInfo.DomainID;
            var propertyName = e.PropertyName;
            var value = e.Value;
            var taskID = e.TaskID;
            this.InvokeEvent(() => this.Callback?.OnPropertyChanged(callbackInfo, domainID, propertyName, value, taskID));
        }

        private void DomainContext_TaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var taskIDs = e.TaskIDs;
            this.InvokeEvent(() => this.Callback?.OnTaskCompleted(callbackInfo, taskIDs));
        }
    }
}

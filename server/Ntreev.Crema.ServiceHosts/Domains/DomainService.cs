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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts.Domains
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    partial class DomainService : CremaServiceItemBase<IDomainEventCallback>, IDomainService
    {
        private Authentication authentication;

        public DomainService(ICremaHost cremaHost)
            : base(cremaHost.GetService(typeof(ILogService)) as ILogService)
        {
            this.CremaHost = cremaHost;
            this.LogService = cremaHost.GetService(typeof(ILogService)) as ILogService;
            this.UserContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            this.DomainContext = cremaHost.GetService(typeof(IDomainContext)) as IDomainContext;
            this.DataBases = cremaHost.GetService(typeof(IDataBaseCollection)) as IDataBaseCollection;
            this.LogService.Debug($"{nameof(DomainService)} Constructor");
        }

        public async Task<ResultBase<DomainContextMetaData>> SubscribeAsync(Guid authenticationToken)
        {
            var result = new ResultBase<DomainContextMetaData>();
            try
            {
                this.authentication = await this.UserContext.AuthenticateAsync(authenticationToken);
                await this.authentication.AddRefAsync(this);
                this.OwnerID = this.authentication.ID;
                result.Value = await this.AttachEventHandlersAsync();
                result.SignatureDate = this.authentication.SignatureDate;
                this.LogService.Debug($"[{this.OwnerID}] {nameof(DomainService)} {nameof(SubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnsubscribeAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.DetachEventHandlersAsync();
                await this.authentication.RemoveRefAsync(this);
                this.authentication = null;
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
                this.LogService.Debug($"[{this.OwnerID}] {nameof(DomainService)} {nameof(UnsubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DomainMetaData[]>> GetMetaDataAsync(Guid dataBaseID)
        {
            var result = new ResultBase<DomainMetaData[]>();
            try
            {
                result.Value = await this.DomainContext.Dispatcher.InvokeAsync(() =>
                {
                    var metaData = this.DomainContext.GetMetaData(authentication);
                    var metaDataList = new List<DomainMetaData>(this.DomainContext.Domains.Count);
                    foreach (var item in metaData.Domains)
                    {
                        if (item.DomainInfo.DataBaseID == dataBaseID)
                            metaDataList.Add(item);
                    }
                    return metaDataList.ToArray();
                });
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DomainRowInfo[]>> SetRowAsync(Guid domainID, DomainRowInfo[] rows)
        {
            var result = new ResultBase<DomainRowInfo[]>();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                var info = await (Task<DomainResultInfo<DomainRowInfo[]>>)domain.SetRowAsync(this.authentication, rows);
                result.ID = info.ID;
                result.Value = info.Value;
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> SetPropertyAsync(Guid domainID, string propertyName, object value)
        {
            var result = new ResultBase();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                await domain.SetPropertyAsync(this.authentication, propertyName, value);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> BeginUserEditAsync(Guid domainID, DomainLocationInfo location)
        {
            var result = new ResultBase();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                await domain.BeginUserEditAsync(this.authentication, location);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> EndUserEditAsync(Guid domainID)
        {
            var result = new ResultBase();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                await domain.EndUserEditAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> KickAsync(Guid domainID, string userID, string comment)
        {
            var result = new ResultBase();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                result.ID = await (Task<long>)domain.KickAsync(this.authentication, userID, comment);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> SetOwnerAsync(Guid domainID, string userID)
        {
            var result = new ResultBase();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                await domain.SetOwnerAsync(this.authentication, userID);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> SetUserLocationAsync(Guid domainID, DomainLocationInfo location)
        {
            var result = new ResultBase();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                await domain.SetUserLocationAsync(this.authentication, location);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DomainRowInfo[]>> NewRowAsync(Guid domainID, DomainRowInfo[] rows)
        {
            var result = new ResultBase<DomainRowInfo[]>();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                var info = await (Task<DomainResultInfo<DomainRowInfo[]>>)domain.NewRowAsync(this.authentication, rows);
                result.ID = info.ID;
                result.Value = info.Value; 
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DomainRowInfo[]>> RemoveRowAsync(Guid domainID, DomainRowInfo[] rows)
        {
            var result = new ResultBase<DomainRowInfo[]>();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                var info = await (Task<DomainResultInfo<DomainRowInfo[]>>)domain.RemoveRowAsync(this.authentication, rows);
                result.ID = info.ID;
                result.Value = info.Value;
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<object>> DeleteDomainAsync(Guid domainID, bool force)
        {
            var result = new ResultBase<object>();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                result.Value = await domain.DeleteAsync(this.authentication, force);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<bool> IsAliveAsync()
        {
            if (this.authentication == null)
                return false;
            this.LogService.Debug($"[{this.authentication}] {nameof(DomainService)}.{nameof(IsAliveAsync)} : {DateTime.Now}");
            await this.authentication.PingAsync();
            return true;
        }

        public ICremaHost CremaHost { get; }

        public ILogService LogService { get; }

        public IDomainContext DomainContext { get; }

        public IDataBaseCollection DataBases { get; }

        public IUserContext UserContext { get; }

        protected override void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            this.Callback?.OnServiceClosed(signatureDate, closeInfo);
        }

        private async Task<IDomain> GetDomainAsync(Guid domainID)
        {
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            if (domain == null)
                throw new DomainNotFoundException(domainID);
            return domain;
        }

        private async Task<DomainContextMetaData> AttachEventHandlersAsync()
        {
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut;
            });
            await this.DataBases.Dispatcher.InvokeAsync(() =>
            {
                this.DataBases.ItemsResetting += DataBases_ItemsResetting;
                this.DataBases.ItemsReset += DataBases_ItemsReset;
            });
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
                this.DomainContext.Domains.DomainRowRemoved += Domains_DomainRowRemoved;
                this.DomainContext.Domains.DomainPropertyChanged += Domains_DomainPropertyChanged;
                return this.DomainContext.GetMetaData(this.authentication);
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DomainService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }



        private async Task DetachEventHandlersAsync()
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
                this.DomainContext.Domains.DomainRowRemoved -= Domains_DomainRowRemoved;
                this.DomainContext.Domains.DomainPropertyChanged -= Domains_DomainPropertyChanged;
            });
            await this.DataBases.Dispatcher.InvokeAsync(() =>
            {
                this.DataBases.ItemsResetting -= DataBases_ItemsResetting;
                this.DataBases.ItemsReset -= DataBases_ItemsReset;
            });
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DomainService)} {nameof(DetachEventHandlersAsync)}");
        }

        private async void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            var actionUserID = e.UserID;
            var contains = e.Items.Any(item => item.ID == this.authentication.ID);
            var closeInfo = (CloseInfo)e.MetaData;
            var signatureDate = e.SignatureDate;
            if (actionUserID != this.authentication.ID && contains == true)
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
                this.Channel.Abort();
            }
        }

        private void Domains_DomainsCreated(object sender, DomainsCreatedEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var metaDatas = e.MetaDatas;
            for (var i = 0; i < metaDatas.Length; i++)
            {
                if (metaDatas[i].Users.Any(item => item.DomainUserInfo.UserID == this.authentication.ID) == false)
                {
                    metaDatas[i].Data = null;
                }
            }
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDomainsCreated(e.SignatureDate, metaDatas));
        }

        private void Domains_DomainsDeleted(object sender, DomainsDeletedEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainIDs = e.DomainInfos.Select(item => item.DomainID).ToArray();
            var isCanceleds = e.IsCanceleds;
            var results = e.Results;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDomainsDeleted(signatureDate, domainIDs, isCanceleds, results));
        }

        private void Domains_DomainInfoChanged(object sender, DomainEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainInfo = e.DomainInfo;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDomainInfoChanged(signatureDate, domainID, domainInfo));
        }

        private void Domains_DomainStateChanged(object sender, DomainEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainState = e.DomainState;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnDomainStateChanged(signatureDate, domainID, domainState), nameof(Domains_DomainStateChanged));
        }

        private void Domains_DomainUserAdded(object sender, DomainUserEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainUserInfo = e.DomainUserInfo;
            var domainUserState = e.DomainUserState;
            var taskID = e.TaskID;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnUserAdded(signatureDate, domainID, domainUserInfo, domainUserState, taskID));
        }

        //private void Domains_DomainUserChanged(object sender, DomainUserEventArgs e)
        //{
        //    var userID = this.authentication.ID;
        //    var exceptionUserID = e.UserID;
        //    var signatureDate = e.SignatureDate;
        //    var domainID = e.DomainInfo.DomainID;
        //    var domainUserState = e.DomainUserState;
        //    var domainUserInfo = e.DomainUserInfo;
        //    this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnUserChanged(signatureDate, domainID, domainUserInfo, domainUserState));
        //}

        private void Domains_DomainUserRemoved(object sender, DomainUserRemovedEventArgs e)
        {
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var userID = e.DomainUserInfo.UserID;
            var ownerID = e.OwnerID;
            var removeInfo = e.RemoveInfo;
            var taskID = e.TaskID;
            this.InvokeEvent(null, null, () => this.Callback?.OnUserRemoved(signatureDate, domainID, userID, ownerID, removeInfo, taskID));
        }

        private void Domains_DomainUserLocationChanged(object sender, DomainUserLocationEventArgs e)
        {
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainLocationInfo = e.DomainLocationInfo;
            this.InvokeEvent(null, null, () => this.Callback?.OnUserLocationChanged(signatureDate, domainID, domainLocationInfo));
        }

        private void Domains_DomainUserStateChanged(object sender, DomainUserEventArgs e)
        {
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainUserState = e.DomainUserState;
            this.InvokeEvent(null, null, () => this.Callback?.OnUserStateChanged(signatureDate, domainID, domainUserState));
        }

        private void Domains_DomainUserEditBegun(object sender, DomainUserLocationEventArgs e)
        {
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainLocationInfo = e.DomainLocationInfo;
            var taskID = e.TaskID;
            this.InvokeEvent(null, null, () => this.Callback?.OnUserEditBegun(signatureDate, domainID, domainLocationInfo, taskID));
        }

        private void Domains_DomainUserEditEnded(object sender, DomainUserEventArgs e)
        {
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var taskID = e.TaskID;
            this.InvokeEvent(null, null, () => this.Callback?.OnUserEditEnded(signatureDate, domainID, taskID));
        }

        private void Domains_DomainOwnerChanged(object sender, DomainUserEventArgs e)
        {
            throw new NotImplementedException();
        }


        private void Domains_DomainRowAdded(object sender, DomainRowEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            var taskID = e.TaskID;
            this.InvokeEvent(null, null, () => this.Callback?.OnRowAdded(signatureDate, domainID, rows, taskID));
        }

        //private void Domains_DomainRowChanged(object sender, DomainRowEventArgs e)
        //{
        //    var userID = this.authentication.ID;
        //    var exceptionUserID = e.UserID;
        //    var signatureDate = e.SignatureDate;
        //    var domainID = e.DomainInfo.DomainID;
        //    var rows = e.Rows;
        //    var taskID = e.TaskID;
        //    this.InvokeEvent(null, null, () => this.Callback?.OnRowChanged(signatureDate, domainID, taskID, rows));
        //}

        private void Domains_DomainRowRemoved(object sender, DomainRowEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            var taskID = e.TaskID;
            this.InvokeEvent(null, null, () => this.Callback?.OnRowRemoved(signatureDate, domainID, rows, taskID));
        }

        private void Domains_DomainPropertyChanged(object sender, DomainPropertyEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var propertyName = e.PropertyName;
            var value = e.Value;
            var taskID = e.TaskID;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback?.OnPropertyChanged(signatureDate, domainID, propertyName, value, taskID));
        }

        private void DataBases_ItemsResetting(object sender, ItemsEventArgs<IDataBase> e)
        {
            //foreach (var item in e.Items)
            //{
            //    this.resettings.Add(item.ID);
            //}
        }

        private void DataBases_ItemsReset(object sender, ItemsEventArgs<IDataBase> e)
        {
            //foreach (var item in e.Items)
            //{
            //    this.resettings.Remove(item.ID);
            //}
        }

        #region ICremaServiceItem

        protected override async Task OnCloseAsync(bool disconnect)
        {
            if (this.authentication != null)
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
            }
        }

        #endregion
    }
}

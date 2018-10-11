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
using System.Windows.Threading;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts.Domains
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class DomainService : CremaServiceItemBase<IDomainEventCallback>, IDomainService
    {
        private Authentication authentication;
        //private readonly HashSet<Guid> domains = new HashSet<Guid>();
        //private readonly HashSet<Guid> resettings = new HashSet<Guid>();

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
                await this.AttachEventHandlersAsync();
                this.OwnerID = this.authentication.ID;
                result.Value = await this.DomainContext.GetMetaDataAsync(this.authentication);
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

        public async Task<ResultBase<DomainContextMetaData>> GetMetaDataAsync()
        {
            var result = new ResultBase<DomainContextMetaData>();
            try
            {
                result.Value = await this.DomainContext.GetMetaDataAsync(this.authentication);
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
                result.Value = await domain.SetRowAsync(this.authentication, rows);
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

        public async Task<ResultBase<DomainUserInfo>> KickAsync(Guid domainID, string userID, string comment)
        {
            var result = new ResultBase<DomainUserInfo>();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                result.Value = await domain.KickAsync(this.authentication, userID, comment);
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
                result.Value = await domain.NewRowAsync(this.authentication, rows);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> RemoveRowAsync(Guid domainID, DomainRowInfo[] rows)
        {
            var result = new ResultBase();
            try
            {
                var domain = await this.GetDomainAsync(domainID);
                await domain.RemoveRowAsync(this.authentication, rows);
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
            this.Callback.OnServiceClosed(signatureDate, closeInfo);
        }

        private async Task<IDomain> GetDomainAsync(Guid domainID)
        {
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            if (domain == null)
                throw new DomainNotFoundException(domainID);
            return domain;
        }

        private async Task AttachEventHandlersAsync()
        {
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut;
            });
            await this.DomainContext.Dispatcher.InvokeAsync(() =>
            {
                this.DomainContext.Domains.DomainCreated += DomainContext_DomainCreated;
                this.DomainContext.Domains.DomainDeleted += DomainContext_DomainDeleted;
                this.DomainContext.Domains.DomainInfoChanged += DomainContext_DomainInfoChanged;
                this.DomainContext.Domains.DomainStateChanged += DomainContext_DomainStateChanged;
                this.DomainContext.Domains.DomainUserAdded += DomainContext_DomainUserAdded;
                this.DomainContext.Domains.DomainUserRemoved += DomainContext_DomainUserRemoved;
                this.DomainContext.Domains.DomainUserChanged += DomainContext_DomainUserChanged;
                this.DomainContext.Domains.DomainRowAdded += DomainContext_DomainRowAdded;
                this.DomainContext.Domains.DomainRowChanged += DomainContext_DomainRowChanged;
                this.DomainContext.Domains.DomainRowRemoved += DomainContext_DomainRowRemoved;
                this.DomainContext.Domains.DomainPropertyChanged += DomainContext_DomainPropertyChanged;
            });
            await this.DataBases.Dispatcher.InvokeAsync(() =>
            {
                this.DataBases.ItemsResetting += DataBases_ItemsResetting;
                this.DataBases.ItemsReset += DataBases_ItemsReset;
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DomainService)} {nameof(AttachEventHandlersAsync)}");
        }

        private async Task DetachEventHandlersAsync()
        {
            await this.DataBases.Dispatcher.InvokeAsync(() =>
            {
                this.DataBases.ItemsResetting -= DataBases_ItemsResetting;
                this.DataBases.ItemsReset -= DataBases_ItemsReset;
            });
            await this.DomainContext.Dispatcher.InvokeAsync(() =>
            {
                this.DomainContext.Domains.DomainCreated -= DomainContext_DomainCreated;
                this.DomainContext.Domains.DomainDeleted -= DomainContext_DomainDeleted;
                this.DomainContext.Domains.DomainInfoChanged -= DomainContext_DomainInfoChanged;
                this.DomainContext.Domains.DomainStateChanged -= DomainContext_DomainStateChanged;
                this.DomainContext.Domains.DomainUserRemoved -= DomainContext_DomainUserRemoved;
                this.DomainContext.Domains.DomainUserAdded -= DomainContext_DomainUserAdded;
                this.DomainContext.Domains.DomainUserChanged -= DomainContext_DomainUserChanged;
                this.DomainContext.Domains.DomainRowAdded -= DomainContext_DomainRowAdded;
                this.DomainContext.Domains.DomainRowChanged -= DomainContext_DomainRowChanged;
                this.DomainContext.Domains.DomainRowRemoved -= DomainContext_DomainRowRemoved;
                this.DomainContext.Domains.DomainPropertyChanged -= DomainContext_DomainPropertyChanged;
            });
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DomainService)} {nameof(DetachEventHandlersAsync)}");
        }

        private void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            var actionUserID = e.UserID;
            var contains = e.Items.Any(item => item.ID == this.authentication.ID);
            var closeInfo = (CloseInfo)e.MetaData;
            if (actionUserID != this.authentication.ID && contains == true)
            {
                this.InvokeEvent(null, null, () => this.Callback.OnServiceClosed(e.SignatureDate, (CloseInfo)e.MetaData));
            }
        }

        private void DomainContext_DomainCreated(object sender, DomainEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainInfo = e.DomainInfo;
            var domainState = e.DomainState;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnDomainCreated(e.SignatureDate, domainInfo, domainState));
        }

        private void DomainContext_DomainDeleted(object sender, DomainDeletedEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var isCanceled = e.IsCanceled;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnDomainDeleted(signatureDate, domainID, isCanceled, e.Result));
        }

        private void DomainContext_DomainInfoChanged(object sender, DomainEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainInfo = e.DomainInfo;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnDomainInfoChanged(signatureDate, domainID, domainInfo));
        }

        private void DomainContext_DomainStateChanged(object sender, DomainEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainState = e.DomainState;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnDomainStateChanged(signatureDate, domainID, domainState));
        }

        private void DomainContext_DomainUserAdded(object sender, DomainUserEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainUserInfo = e.DomainUserInfo;
            var domainUserState = e.DomainUserState;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUserAdded(signatureDate, domainID, domainUserInfo, domainUserState));
        }

        private void DomainContext_DomainUserChanged(object sender, DomainUserEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainUserState = e.DomainUserState;
            var domainUserInfo = e.DomainUserInfo;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUserChanged(signatureDate, domainID, domainUserInfo, domainUserState));
        }

        private void DomainContext_DomainUserRemoved(object sender, DomainUserRemovedEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var domainUserInfo = e.DomainUserInfo;
            var removeInfo = e.RemoveInfo;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUserRemoved(signatureDate, domainID, domainUserInfo, removeInfo));
        }

        private void DomainContext_DomainRowAdded(object sender, DomainRowEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnRowAdded(signatureDate, domainID, rows));
        }

        private void DomainContext_DomainRowChanged(object sender, DomainRowEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnRowChanged(signatureDate, domainID, rows));
        }

        private void DomainContext_DomainRowRemoved(object sender, DomainRowEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var rows = e.Rows;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnRowRemoved(signatureDate, domainID, rows));
        }

        private void DomainContext_DomainPropertyChanged(object sender, DomainPropertyEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var domainID = e.DomainInfo.DomainID;
            var propertyName = e.PropertyName;
            var value = e.Value;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnPropertyChanged(signatureDate, domainID, propertyName, value));
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

        protected override async Task OnAbortAsync(bool disconnect)
        {
            await this.DetachEventHandlersAsync();
            await CremaService.Dispatcher.InvokeAsync(() =>
            {
                if (disconnect == false)
                {
                    this.Callback?.OnServiceClosed(SignatureDate.Empty, CloseInfo.Empty);
                    try
                    {
                        this.Channel?.Close(TimeSpan.FromSeconds(10));
                    }
                    catch
                    {
                        this.Channel?.Abort();
                    }
                }
                else
                {
                    this.Channel?.Abort();
                }
            });
        }

        #endregion
    }
}

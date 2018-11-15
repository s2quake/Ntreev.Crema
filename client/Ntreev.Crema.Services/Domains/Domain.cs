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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.DomainContextService;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains
{
    abstract class Domain : DomainBase<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomain, IDomainItem, IInfoProvider, IStateProvider
    {
        private readonly HashSet<string> modifiedTableList = new HashSet<string>();

        private EventHandler<DomainUserEventArgs> userAdded;
        private EventHandler<DomainUserRemovedEventArgs> userRemoved;
        private EventHandler<DomainUserLocationEventArgs> userLocationChanged;
        private EventHandler<DomainUserEventArgs> userStateChanged;
        private EventHandler<DomainUserLocationEventArgs> userEditBegun;
        private EventHandler<DomainUserEventArgs> userEditEnded;
        private EventHandler<DomainUserEventArgs> ownerChanged;
        private EventHandler<DomainRowEventArgs> rowAdded;
        private EventHandler<DomainRowEventArgs> rowChanged;
        private EventHandler<DomainRowEventArgs> rowRemoved;
        private EventHandler<DomainPropertyEventArgs> propertyChanged;

        private EventHandler<DomainDeletedEventArgs> deleted;

        public TaskResetEvent<Guid> taskEvent;
        public TaskResetEvent<string> enterEvent;
        public TaskResetEvent<string> leaveEvent;
        private DomainMetaData metaData;

        protected Domain(DomainInfo domainInfo)
        {
            this.Initialize(domainInfo);
            this.Name = domainInfo.DomainID.ToString();
            this.Users = new DomainUserCollection(this);
        }

        public async Task WaitUserEnterAsync(Authentication authentication)
        {
            await this.enterEvent.WaitAsync(authentication.ID);
        }

        public async Task WaitUserLeaveAsync(Authentication authentication)
        {
            await this.leaveEvent.WaitAsync(authentication.ID);
        }

        public async Task<DomainResultInfo<object>> DeleteAsync(Authentication authentication, bool isCanceled)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, isCanceled);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.DeleteDomain(this.ID, isCanceled));
                await this.taskEvent.WaitAsync(result.TaskID);
                return new DomainResultInfo<object>() { ID = result.TaskID, Value = result.Value };
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> BeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.BeginUserEdit(this.ID, location));
                await this.taskEvent.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> EndUserEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.EndUserEdit(this.ID));
                await this.taskEvent.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DomainResultInfo<DomainRowInfo[]>> NewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.NewRow(this.ID, rows));
                await this.taskEvent.WaitAsync(result.TaskID);
                return new DomainResultInfo<DomainRowInfo[]>() { ID = result.TaskID, Value = result.Value };
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DomainResultInfo<DomainRowInfo[]>> SetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetRow(this.ID, rows));
                await this.taskEvent.WaitAsync(result.TaskID);
                return new DomainResultInfo<DomainRowInfo[]>() { ID = result.TaskID, Value = result.Value };
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DomainResultInfo<DomainRowInfo[]>> RemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.RemoveRow(this.ID, rows));
                await this.taskEvent.WaitAsync(result.TaskID);
                return new DomainResultInfo<DomainRowInfo[]>() { ID = result.TaskID, Value = result.Value };
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetPropertyAsync(Authentication authentication, string propertyName, object value)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPropertyAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, propertyName, value);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetProperty(this.ID, propertyName, value));
                await this.taskEvent.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserLocationAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    return this.GetDomainUser(authentication);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetUserLocation(this.ID, location));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    domainUser.DomainLocationInfo = location;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> KickAsync(Authentication authentication, string userID, string comment)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID, comment);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.Kick(this.ID, userID, comment));
                await this.taskEvent.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetOwnerAsync(Authentication authentication, string userID)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetOwnerAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetOwner(this.ID, userID));
                await this.taskEvent.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public DomainMetaData GetMetaData(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            var metaData = new DomainMetaData()
            {
                DomainID = Guid.Parse(this.Name),
                DomainInfo = base.DomainInfo,
                Users = this.Users.Select<DomainUser, DomainUserMetaData>(item => item.GetMetaData(authentication)).ToArray(),
                DomainState = base.DomainState,
                ModifiedTables = this.modifiedTableList.ToArray(),
            };
            if (this.Users.ContainsKey(authentication.ID) == true)
            {
                metaData.Data = this.SerializeSource();
            }
            return metaData;
        }

        public async Task<DomainMetaData> GetMetaDataAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var metaData = new DomainMetaData()
                {
                    DomainID = Guid.Parse(this.Name),
                    DomainInfo = base.DomainInfo,
                    Users = this.Users.Select<DomainUser, DomainUserMetaData>(item => item.GetMetaData(authentication)).ToArray(),
                    DomainState = this.DomainState,
                    ModifiedTables = this.modifiedTableList.ToArray(),
                };
                if (this.Users.ContainsKey(authentication.ID) == true)
                {
                    metaData.Data = this.SerializeSource();
                }
                return metaData;
            });
        }

        public void Initialize(Authentication authentication, DomainMetaData metaData)
        {
            this.Dispatcher.VerifyAccess();

            this.taskEvent = new TaskResetEvent<Guid>(this.Dispatcher);
            this.enterEvent = new TaskResetEvent<string>(this.Dispatcher);
            this.leaveEvent = new TaskResetEvent<string>(this.Dispatcher);
            base.DomainState = metaData.DomainState;
            this.metaData = metaData;
            this.modifiedTableList.Clear();
            foreach (var item in metaData.ModifiedTables)
            {
                this.modifiedTableList.Add(item);
            }

            foreach (var item in metaData.Users)
            {
                if (item.DomainUserState.HasFlag(DomainUserState.Detached) == true)
                    continue;
                var signatureDate = new SignatureDate(item.DomainUserInfo.UserID, authentication.SignatureDate.DateTime);
                var userAuthentication = this.UserContext.Authenticate(signatureDate);
                var domainUser = new DomainUser(this, item.DomainUserInfo, item.DomainUserState, false);
                this.Users.Add(domainUser);
                this.enterEvent.Set(domainUser.ID);
            }

            if (metaData.Data != null)
            {
                this.OnInitialize(metaData.Data);
                this.Logger = new DomainLogger(this)
                {
                    
                };
            }
        }

        public UserContext UserContext => this.CremaHost.UserContext;

        public void Dispose(Authentication authentication, bool isCanceled)
        {
            this.Logger?.Dispose();
            this.Logger = null;
            this.Dispose();
            this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled));
        }

        public object GetService(System.Type serviceType)
        {
            return this.CremaHost.GetService(serviceType);
        }

        public void InvokeDomainInfoChanged(Authentication authentication, DomainInfo domainInfo)
        {
            base.UpdateDomainInfo(domainInfo);
            this.Container.InvokeDomainInfoChangedEvent(authentication, this);
        }

        public void InvokeDomainStateChanged(Authentication authentication, DomainState domainState)
        {
            base.DomainState = domainState;
            this.Container.InvokeDomainStateChangedEvent(authentication, this);
        }

        public async void InvokeDeleteAsync(Authentication authentication, bool isCanceled, object result)
        {
            this.Result = result;
            if (this.Host is IDomainHost host)
            {
                this.Host = null;
                await host.DeleteAsync(authentication, isCanceled);
                this.taskEvent.Set(this.ID);
            }
            var context = this.Context;
            var container = this.Container;
            this.CremaHost.Sign(authentication, authentication.SignatureDate);
            if (this.Logger != null)
                await this.Logger.DisposeAsync();
            this.Logger = null;
            this.Dispose();
            this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled));
            container.InvokeDomainDeletedEvent(authentication, new Domain[] { this }, new bool[] { isCanceled });
            context.deletionEvent.Set(this.ID);
        }

        public async void InvokeUserAddedAsync(Authentication authentication, DomainUserInfo domainUserInfo, DomainUserState domainUserState, byte[] data, Guid taskID)
        {
            var domainUser = new DomainUser(this, domainUserInfo, domainUserState, false);
            this.Users.Add(domainUser);
            if (data != null)
            {
                this.Logger = new DomainLogger(this);
                await this.DataDispatcher.InvokeAsync(() =>
                {
                    this.OnInitialize(data);
                });
            }
            this.OnUserAdded(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserAddedEvent(authentication, this, domainUser, taskID);
            this.taskEvent.Set(taskID);
            this.enterEvent.Set(authentication.ID);
            this.leaveEvent.Reset(authentication.ID);
        }

        public async void InvokeUserRemovedAsync(Authentication authentication, DomainUser domainUser, DomainUser ownerUser, RemoveInfo removeInfo, Guid taskID)
        {
            this.Users.Remove(domainUser.ID);
            this.Users.Owner = ownerUser;
            if (domainUser.ID == this.CremaHost.UserID)
            {
                await this.DataDispatcher.InvokeAsync(() =>
                {
                    this.Logger.Dispose();
                    this.Logger = null;
                });
            }
            this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, removeInfo));
            this.Container?.InvokeDomainUserRemovedEvent(authentication, this, domainUser, removeInfo, taskID);
            this.taskEvent.Set(taskID);
            this.leaveEvent.Set(domainUser.ID);
            this.enterEvent.Reset(authentication.ID);
        }

        public void InvokeUserLocationChanged(Authentication authentication, DomainUser domainUser, DomainLocationInfo domainLocationInfo)
        {
            domainUser.DomainLocationInfo = domainLocationInfo;
            //this.CremaHost.Sign(authentication);
            this.OnUserLocationChanged(new DomainUserLocationEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserLocationChangedEvent(authentication, this, domainUser);
        }

        public void InvokeUserStateChanged(Authentication authentication, DomainUser domainUser, DomainUserState domainUserState)
        {
            domainUser.DomainUserState = domainUserState;
            //this.CremaHost.Sign(authentication);
            this.OnUserStateChanged(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserStateChangedEvent(authentication, this, domainUser);
        }

        public async void InvokeUserEditBegunAsync(Authentication authentication, DomainUser domainUser, DomainLocationInfo domainLocationInfo, Guid taskID)
        {
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.OnBeginUserEdit(domainUser, domainLocationInfo);
            });
            domainUser.DomainLocationInfo = domainLocationInfo;
            domainUser.IsBeingEdited = true;
            //this.CremaHost.Sign(authentication);
            this.OnUserEditBegun(new DomainUserLocationEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserEditBegunEvent(authentication, this, domainUser, taskID);
            this.taskEvent.Set(taskID);
        }

        public async void InvokeUserEditEndedAsync(Authentication authentication, DomainUser domainUser, Guid taskID)
        {
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.OnEndUserEdit(domainUser);
            });
            domainUser.IsBeingEdited = false;
            //this.CremaHost.Sign(authentication);
            this.OnUserEditEnded(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserEditEndedEvent(authentication, this, domainUser, taskID);
            this.taskEvent.Set(taskID);
        }

        public void InvokeOwnerChangedAsync(Authentication authentication, DomainUser domainUser, Guid taskID)
        {
            this.Users.Owner = domainUser;
            this.OnOwnerChanged(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainOwnerChangedEvent(authentication, this, domainUser, taskID);
            this.taskEvent.Set(taskID);
        }

        public async void InvokeRowAddedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, Guid taskID)
        {
            await this.DataDispatcher.InvokeAsync(() =>
            {
                var ss = taskID;
                this.OnNewRow(domainUser, rows, authentication.SignatureDate);
            });
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnRowAdded(new DomainRowEventArgs(authentication, this, rows));
            this.Container?.InvokeDomainRowAddedEvent(authentication, this, taskID, rows);
            this.taskEvent.Set(taskID);
        }

        public async void InvokeRowChangedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, Guid taskID)
        {
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.OnSetRow(domainUser, rows, authentication.SignatureDate);
            });
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnRowChanged(new DomainRowEventArgs(authentication, this, rows));
            this.Container?.InvokeDomainRowChangedEvent(authentication, this, taskID, rows);
            this.taskEvent.Set(taskID);
        }

        public async void InvokeRowRemovedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, Guid taskID)
        {
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.OnRemoveRow(domainUser, rows, authentication.SignatureDate);
            });
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnRowRemoved(new DomainRowEventArgs(authentication, this, rows));
            this.Container?.InvokeDomainRowRemovedEvent(authentication, this, taskID, rows);
            this.taskEvent.Set(taskID);
        }

        public async void InvokePropertyChangedAsync(Authentication authentication, DomainUser domainUser, string propertyName, object value, Guid taskID)
        {
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.OnSetProperty(domainUser, propertyName, value, authentication.SignatureDate);
            });
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnPropertyChanged(new DomainPropertyEventArgs(authentication, this, propertyName, value));
            this.Container?.InvokeDomainPropertyChangedEvent(authentication, this, taskID, propertyName, value);
            this.taskEvent.Set(taskID);
        }

        public void SetDomainHost(Authentication authentication, IDomainHost host)
        {
            this.Dispatcher.VerifyAccess();
            this.Host = host;
            if (this.Host != null)
            {
                base.DomainState |= DomainState.IsActivated;
            }
            else
            {
                base.DomainState &= ~DomainState.IsActivated;
            }
            this.OnDomainStateChanged(new DomainEventArgs(authentication, this));
            this.Container.InvokeDomainStateChangedEvent(authentication, this);
        }

        public void Attach(params Authentication[] authentications)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in authentications)
            {
                if (this.Users.ContainsKey(item.ID) == true)
                {
                    var domainUser = this.Users[item.ID];
                    domainUser.IsOnline = true;
                    this.OnUserStateChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainUserStateChangedEvent(item, this, domainUser);
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
        }

        public void Detach(params Authentication[] authentications)
        {
            foreach (var item in authentications)
            {
                if (this.Users[item.ID] is DomainUser domainUser && domainUser.IsOnline == true)
                {
                    //this.Sign(item, true);
                    //this.InvokeDetach(item, out var domainUser);
                    this.OnUserStateChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainUserStateChangedEvent(item, this, domainUser);
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
        }

        public abstract object Source { get; }

        public object Result { get; set; }

        public IDomainHost Host { get; set; }

        public CremaHost CremaHost => this.Context.CremaHost;

        public Guid ID => Guid.Parse(this.Name);

        public Guid DataBaseID => base.DomainInfo.DataBaseID;

        public string DomainName => base.DomainInfo.ItemType;

        public DomainUserCollection Users { get; }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public CremaDispatcher DataDispatcher => this.Logger?.Dispatcher;

        public DomainLogger Logger { get; set; }

        public string[] ModifiedTables => this.modifiedTableList.OrderBy(item => item).ToArray();

        public event EventHandler<DomainUserEventArgs> UserAdded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userAdded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userAdded -= value;
            }
        }

        public event EventHandler<DomainUserRemovedEventArgs> UserRemoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userRemoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userRemoved -= value;
            }
        }

        public event EventHandler<DomainUserLocationEventArgs> UserLocationChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userLocationChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userLocationChanged -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> UserStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userStateChanged -= value;
            }
        }

        public event EventHandler<DomainUserLocationEventArgs> UserEditBegun
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditBegun += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditBegun -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> UserEditEnded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditEnded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditEnded -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> OwnerChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.ownerChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.ownerChanged -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowAdded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowAdded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowAdded -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowChanged -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowRemoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowRemoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowRemoved -= value;
            }
        }

        public event EventHandler<DomainPropertyEventArgs> PropertyChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.propertyChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.propertyChanged -= value;
            }
        }

        public new event EventHandler<DomainDeletedEventArgs> Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.deleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.deleted -= value;
            }
        }

        public new event EventHandler DomainInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainInfoChanged -= value;
            }
        }

        public new event EventHandler DomainStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainStateChanged -= value;
            }
        }

        protected virtual void OnInitialize(byte[] data)
        {

        }

        protected virtual void OnRelease()
        {

        }

        protected virtual void OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {

        }

        protected virtual void OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {

        }

        protected virtual void OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {

        }

        protected virtual void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDate signatureDate)
        {

        }

        protected virtual void OnBeginUserEdit(DomainUser domainUser, DomainLocationInfo location)
        {

        }

        protected virtual void OnEndUserEdit(DomainUser domainUser)
        {

        }

        protected abstract byte[] SerializeSource();

        protected abstract void DerializeSource(byte[] data);

        protected virtual void OnUserAdded(DomainUserEventArgs e)
        {
            this.userAdded?.Invoke(this, e);
        }

        protected virtual void OnUserRemoved(DomainUserRemovedEventArgs e)
        {
            this.userRemoved?.Invoke(this, e);
        }

        protected virtual void OnUserLocationChanged(DomainUserLocationEventArgs e)
        {
            this.userLocationChanged?.Invoke(this, e);
        }

        protected virtual void OnUserStateChanged(DomainUserEventArgs e)
        {
            this.userStateChanged?.Invoke(this, e);
        }

        protected virtual void OnUserEditBegun(DomainUserLocationEventArgs e)
        {
            this.userEditBegun?.Invoke(this, e);
        }

        protected virtual void OnUserEditEnded(DomainUserEventArgs e)
        {
            this.userEditEnded?.Invoke(this, e);
        }

        protected virtual void OnOwnerChanged(DomainUserEventArgs e)
        {
            this.ownerChanged?.Invoke(this, e);
        }

        protected virtual void OnRowAdded(DomainRowEventArgs e)
        {
            this.rowAdded?.Invoke(this, e);
        }

        protected virtual void OnRowChanged(DomainRowEventArgs e)
        {
            this.rowChanged?.Invoke(this, e);
        }

        protected virtual void OnRowRemoved(DomainRowEventArgs e)
        {
            this.rowRemoved?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(DomainPropertyEventArgs e)
        {
            this.propertyChanged?.Invoke(this, e);
        }

        protected virtual void OnDeleted(DomainDeletedEventArgs e)
        {
            this.deleted?.Invoke(this, e);
        }

        public DomainUser GetDomainUser(string userID)
        {

            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
            if (this.Users.ContainsKey(userID) == false)
                throw new UserNotFoundException(userID);

            return this.Users[userID];
        }

        public DomainUser GetDomainUser(Authentication authentication)
        {

            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
            if (this.Users.ContainsKey(authentication.ID) == false)
                throw new UserNotFoundException(authentication.ID);

            return this.Users[authentication.ID];
        }

        private Task<DomainUser> GetDomainUserAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.Users.ContainsKey(authentication.ID) == false)
                    throw new UserNotFoundException(authentication.ID);
                return this.Users[authentication.ID];
            });
        }

        private void Sign(Authentication authentication, ResultBase result)
        {
            result.Validate(authentication);
        }

        private void Sign<T>(Authentication authentication, ResultBase<T> result)
        {
            result.Validate(authentication);
        }

        private IDomainContextService Service => this.Context.Service;

        #region IDomain

        Task IDomain.DeleteAsync(Authentication authentication, bool isCanceled)
        {
            return this.DeleteAsync(authentication, isCanceled);
        }

        Task IDomain.BeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            return this.BeginUserEditAsync(authentication, location);
        }

        Task IDomain.EndUserEditAsync(Authentication authentication)
        {
            return this.EndUserEditAsync(authentication);
        }

        Task IDomain.NewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.NewRowAsync(authentication, rows);
        }

        Task IDomain.SetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.SetRowAsync(authentication, rows);
        }

        Task IDomain.RemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.RemoveRowAsync(authentication, rows);
        }

        Task IDomain.SetPropertyAsync(Authentication authentication, string propertyName, object value)
        {
            return this.SetPropertyAsync(authentication, propertyName, value);
        }

        Task IDomain.SetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            return this.SetUserLocationAsync(authentication, location);
        }

        Task IDomain.KickAsync(Authentication authentication, string userID, string comment)
        {
            return this.KickAsync(authentication, userID, comment);
        }

        Task IDomain.SetOwnerAsync(Authentication authentication, string userID)
        {
            return this.SetOwnerAsync(authentication, userID);
        }

        IDomainUserCollection IDomain.Users => this.Users;

        DomainInfo IDomain.DomainInfo => base.DomainInfo;

        object IDomain.Host => this.Host;

        #endregion

        #region IDomainItem

        IDomainItem IDomainItem.Parent => this.Category;

        IEnumerable<IDomainItem> IDomainItem.Childs => Enumerable.Empty<IDomainItem>();

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IDataBase) && this.Category != null)
            {
                return this.Category.DataBase;
            }
            return (this.Context as IServiceProvider).GetService(serviceType);
        }

        #endregion

        #region InternalSignatureDateProvider

        protected class InternalSignatureDateProvider : SignatureDateProvider
        {
            private readonly DateTime dateTime;

            public InternalSignatureDateProvider(SignatureDate signatureDate)
                : base(signatureDate.ID)
            {
                this.dateTime = signatureDate.DateTime;
            }

            public DateTime DateTime
            {
                get { return this.dateTime; }
            }

            protected override DateTime GetTime()
            {
                return this.dateTime;
            }
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => base.DomainInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.DomainState;

        #endregion
    }
}

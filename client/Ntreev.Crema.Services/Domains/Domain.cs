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
using Ntreev.Crema.Services.DomainService;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains
{
    abstract class Domain : DomainBase<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomain, IDomainItem, IInfoProvider, IStateProvider
    {
        private bool initialized;
        private readonly HashSet<string> modifiedTableList = new HashSet<string>();

        private EventHandler<DomainUserEventArgs> userAdded;
        private EventHandler<DomainUserEventArgs> userChanged;
        private EventHandler<DomainUserRemovedEventArgs> userRemoved;
        private EventHandler<DomainRowEventArgs> rowAdded;
        private EventHandler<DomainRowEventArgs> rowRemoved;
        private EventHandler<DomainRowEventArgs> rowChanged;
        private EventHandler<DomainPropertyEventArgs> propertyChanged;
        private EventHandler<DomainDeletedEventArgs> deleted;

        protected Domain(DomainInfo domainInfo)
        {
            this.Initialize(domainInfo);
            this.Name = domainInfo.DomainID.ToString();
            this.Users = new DomainUserCollection(this);
        }

        public async Task DeleteAsync(Authentication authentication, bool isCanceled)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, isCanceled);
                });
                var result = await this.Service.DeleteDomainAsync(this.ID, isCanceled);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var container = this.Container;
                    this.CremaHost.Sign(authentication, result);
                    this.Dispose();
                    this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled));
                    container.InvokeDomainDeletedEvent(authentication, this, isCanceled);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task BeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.Service.BeginUserEditAsync(this.ID, location);
                var domainUser = await this.InvokeBeginUserEditAsync(authentication, location);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task EndUserEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.Service.EndUserEditAsync(this.ID);
                var domainUser = await this.InvokeEndUserEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DomainRowInfo[]> NewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.Service.NewRowAsync(this.ID, rows);
                await this.InvokeNewRowAsync(authentication, result.Value);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.IsModified = true;
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowAdded(new DomainRowEventArgs(authentication, this, result.Value));
                    this.Container.InvokeDomainRowAddedEvent(authentication, this, result.Value);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
                return result.Value;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DomainRowInfo[]> SetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.Service.SetRowAsync(this.ID, rows);
                await this.InvokeSetRowAsync(authentication, result.Value);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.IsModified = true;
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowChanged(new DomainRowEventArgs(authentication, this, result.Value));
                    this.Container.InvokeDomainRowChangedEvent(authentication, this, result.Value);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
                return result.Value;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.Service.RemoveRowAsync(this.ID, rows);
                await this.InvokeRemoveRowAsync(authentication, rows);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.IsModified = true;
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowRemoved(new DomainRowEventArgs(authentication, this, rows));
                    this.Container.InvokeDomainRowRemovedEvent(authentication, this, rows);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetPropertyAsync(Authentication authentication, string propertyName, object value)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPropertyAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, propertyName, value);
                });
                var result = await this.Service.SetPropertyAsync(this.ID, propertyName, value);
                await this.InvokeSetPropertyAsync(authentication, propertyName, value);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.OnPropertyChanged(new DomainPropertyEventArgs(authentication, this, propertyName, value));
                    this.Container.InvokeDomainPropertyChangedEvent(authentication, this, propertyName, value);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserLocationAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.Service.SetUserLocationAsync(this.ID, location);
                var domainUser = await this.InvokeSetUserLocationAsync(authentication, location);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DomainUserInfo> KickAsync(Authentication authentication, string userID, string comment)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID, comment);
                });
                var result = await this.Service.KickAsync(this.ID, userID, comment);
                var tuple = await this.InvokeKickAsync(authentication, userID, comment);
                var domainUser = tuple.Item1;
                var removeInfo = tuple.Item2;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, removeInfo));
                    this.Container.InvokeDomainUserRemovedEvent(authentication, this, domainUser, removeInfo);
                });
                return result.Value;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetOwnerAsync(Authentication authentication, string userID)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetOwnerAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID);
                });
                var result = await this.Service.SetOwnerAsync(this.ID, userID);
                var tuple = await this.InvokeSetOwnerAsync(authentication, userID);
                var oldOwner = tuple.Item1;
                var newOwner = tuple.Item2;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.Users.Owner = newOwner;
                    if (oldOwner != null)
                    {
                        this.OnUserChanged(new DomainUserEventArgs(authentication, this, oldOwner));
                        this.Container.InvokeDomainUserChangedEvent(authentication, this, oldOwner);
                    }
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, newOwner));
                    this.Container.InvokeDomainUserChangedEvent(authentication, this, newOwner);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
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
            base.DomainState = metaData.DomainState;
            this.modifiedTableList.Clear();
            foreach (var item in metaData.ModifiedTables)
            {
                this.modifiedTableList.Add(item);
            }

            if (metaData.Data == null)
            {
                foreach (var item in metaData.Users)
                {
                    this.InvokeUserAdded(authentication, item.DomainUserInfo, item.DomainUserState);
                }
            }
            else
            {
                this.OnInitialize(metaData);
                this.DataDispatcher = new CremaDispatcher(this);
                this.initialized = true;
                foreach (var item in metaData.Users)
                {
                    var userInfo = item.DomainUserInfo;
                    if (this.Users.ContainsKey(item.DomainUserInfo.UserID) == false)
                    {
                        var signatureDate = new SignatureDate(item.DomainUserInfo.UserID, authentication.SignatureDate.DateTime);
                        var userAuthentication = this.UserContext.Authenticate(signatureDate);
                        this.InvokeUserAdded(userAuthentication, item.DomainUserInfo, item.DomainUserState);
                    }
                }
            }
        }

        public UserContext UserContext => this.CremaHost.UserContext;

        public async Task ReleaseAsync(Authentication authentication, DomainMetaData metaData)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this.Users.ToArray<DomainUser>())
                {
                    if (metaData.Users.Any(i => i.DomainUserInfo.UserID == item.DomainUserInfo.UserID) == false)
                    {
                        this.InvokeUserRemoved(authentication, item.DomainUserInfo, RemoveInfo.Empty);
                    }
                }
                foreach (var item in metaData.Users)
                {
                    if (item.DomainUserState.HasFlag(DomainUserState.IsOwner) == true)
                    {
                        var master = this.Users[item.DomainUserInfo.UserID];
                        this.Users.Owner = master;
                        this.InvokeUserChanged(authentication, item.DomainUserInfo, item.DomainUserState);
                    }
                }
                this.OnRelease();
                this.initialized = false;
            });
        }

        public void Dispose(Authentication authentication, bool isCanceled)
        {
            this.DataDispatcher?.Dispose();
            this.DataDispatcher = null;
            this.Dispose();
            this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled));
        }

        public Task AttachUserAsync()
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.Users.ContainsKey(this.CremaHost.UserID) == true)
                {
                    var domainUser = this.Users[this.CremaHost.UserID];
                    domainUser.IsOnline = true;
                }
            });
        }

        public Task DetachUserAsync()
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.Users.ContainsKey(this.CremaHost.UserID) == true)
                {
                    var domainUser = this.Users[this.CremaHost.UserID];
                    domainUser.IsOnline = false;
                }
            });
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

        public void InvokeUserAdded(Authentication authentication, DomainUserInfo domainUserInfo, DomainUserState domainUserState)
        {
            var domainUser = new DomainUser(this, domainUserInfo, domainUserState);
            this.Users.Add(domainUser);
            if (domainUser.IsOwner == true)
                this.Users.Owner = domainUser;
            this.OnUserAdded(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container.InvokeDomainUserAddedEvent(authentication, this, domainUser);
        }

        public void InvokeUserChanged(Authentication authentication, DomainUserInfo domainUserInfo, DomainUserState domainUserState)
        {
            var domainUser = this.Users[domainUserInfo.UserID];
            domainUser.SetDomainUserInfo(domainUserInfo);
            domainUser.SetDomainUserState(domainUserState);
            if (domainUser.IsOwner == true)
                this.Users.Owner = domainUser;
            this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
        }

        public void InvokeUserRemoved(Authentication authentication, DomainUserInfo domainUserInfo, RemoveInfo removeInfo)
        {
            var domainUser = this.Users[domainUserInfo.UserID];
            this.Users.Remove(domainUser.ID);
            if (domainUser.IsOwner == true)
                this.Users.Owner = null;
            this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, removeInfo));
            this.Container.InvokeDomainUserRemovedEvent(authentication, this, domainUser, removeInfo);
        }

        public async Task InvokeRowAddedAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            if (this.initialized == false)
                return;

            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnNewRowAsync(domainUser, rows, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
                this.IsModified = true;
                this.OnRowAdded(new DomainRowEventArgs(authentication, this, rows));
                this.Container.InvokeDomainRowAddedEvent(authentication, this, rows);
            });
        }

        public async Task InvokeRowChangedAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            if (this.initialized == false)
                return;

            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnSetRowAsync(domainUser, rows, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
                this.IsModified = true;
                this.OnRowChanged(new DomainRowEventArgs(authentication, this, rows));
                this.Container.InvokeDomainRowChangedEvent(authentication, this, rows);
            });
        }

        public async Task InvokeRowRemovedAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            if (this.initialized == false)
                return;

            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnRemoveRowAsync(domainUser, rows, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
                this.IsModified = true;
                this.OnRowRemoved(new DomainRowEventArgs(authentication, this, rows));
                this.Container.InvokeDomainRowRemovedEvent(authentication, this, rows);
            });
        }

        public async Task InvokePropertyChangedAsync(Authentication authentication, string propertyName, object value)
        {
            if (this.initialized == false)
                return;

            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnSetPropertyAsync(domainUser, propertyName, value, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.IsModified = true;
                this.IsModified = true;
                this.OnPropertyChanged(new DomainPropertyEventArgs(authentication, this, propertyName, value));
                this.Container.InvokeDomainPropertyChangedEvent(authentication, this, propertyName, value);
            });
        }

        public abstract object Source { get; }

        public IDomainHost Host { get; set; }

        public CremaHost CremaHost => this.Context.CremaHost;

        public Guid ID => Guid.Parse(this.Name);

        public Guid DataBaseID => base.DomainInfo.DataBaseID;

        public string DomainName => base.DomainInfo.ItemType;

        public DomainUserCollection Users { get; }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public CremaDispatcher DataDispatcher { get; set; }

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

        public event EventHandler<DomainUserEventArgs> UserChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userChanged -= value;
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

        protected virtual void OnInitialize(DomainMetaData metaData)
        {

        }

        protected virtual void OnRelease()
        {

        }

        protected virtual async Task OnNewRowAsync(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            await Task.Delay(1);
        }

        protected virtual async Task OnSetRowAsync(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            await Task.Delay(1);
        }

        protected virtual async Task OnRemoveRowAsync(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            await Task.Delay(1);
        }

        protected virtual async Task OnSetPropertyAsync(DomainUser domainUser, string propertyName, object value, SignatureDate signatureDate)
        {
            await Task.Delay(1);
        }

        protected virtual async Task OnSetLocationAsync(DomainUser domainUser, DomainLocationInfo location)
        {
            await Task.Delay(1);
        }

        protected virtual async Task OnBeginUserEditAsync(DomainUser domainUser, DomainLocationInfo location)
        {
            await Task.Delay(1);
        }

        protected virtual async Task OnEndUserEditAsync(DomainUser domainUser)
        {
            await Task.Delay(1);
        }

        protected abstract byte[] SerializeSource();

        protected abstract void DerializeSource(byte[] data);

        protected virtual void OnUserAdded(DomainUserEventArgs e)
        {
            this.userAdded?.Invoke(this, e);
        }

        protected virtual void OnUserChanged(DomainUserEventArgs e)
        {
            this.userChanged?.Invoke(this, e);
        }

        protected virtual void OnUserRemoved(DomainUserRemovedEventArgs e)
        {
            this.userRemoved?.Invoke(this, e);
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

        private async Task<DomainUser> InvokeBeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnBeginUserEditAsync(domainUser, location);
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.Location = location;
                domainUser.IsBeingEdited = true;
            });
            return domainUser;
        }

        private async Task<DomainUser> InvokeEndUserEditAsync(Authentication authentication)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnEndUserEditAsync(domainUser);
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.IsBeingEdited = false;
            });
            return domainUser;
        }

        private async Task InvokeNewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnNewRowAsync(domainUser, rows, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
            });
        }

        private async Task InvokeSetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnSetRowAsync(domainUser, rows, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
            });
        }

        private async Task InvokeRemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnRemoveRowAsync(domainUser, rows, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
            });
        }

        private async Task InvokeSetPropertyAsync(Authentication authentication, string propertyName, object value)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnSetPropertyAsync(domainUser, propertyName, value, authentication.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.IsModified = true;
            });
        }

        private async Task<DomainUser> InvokeSetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnSetLocationAsync(domainUser, location);
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.Location = location;
            });
            return domainUser;
        }

        private async Task<(DomainUser, RemoveInfo)> InvokeKickAsync(Authentication authentication, string userID, string comment)
        {
            var removeInfo = new RemoveInfo(RemoveReason.Kick, comment);
            var domainUser = await this.Dispatcher.InvokeAsync(() => this.Users[userID]);
            return (domainUser, removeInfo);
        }

        private async Task<(DomainUser, DomainUser)> InvokeSetOwnerAsync(Authentication authentication, string userID)
        {
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var oldOwner = this.Users.Owner;
                var newOwner = this.Users[userID];
                return (oldOwner, newOwner);
            });
        }

        private DomainUser GetDomainUser(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);

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

        private IDomainService Service => this.Context.Service;

        #region IDomain

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

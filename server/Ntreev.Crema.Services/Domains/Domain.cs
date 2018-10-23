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
using Ntreev.Crema.Services.Domains.Serializations;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains
{
    abstract class Domain : DomainBase<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomain, IDomainItem, IInfoProvider, IStateProvider
    {
        private const string dataKey = "Data";
        private const string usersKey = "Users";
        private byte[] data;
        private Func<DateTime> dateTimeProvider;
        private readonly HashSet<string> modifiedTableList = new HashSet<string>();

        private EventHandler<DomainUserEventArgs> userAdded;
        private EventHandler<DomainUserEventArgs> userChanged;
        private EventHandler<DomainUserRemovedEventArgs> userRemoved;
        private EventHandler<DomainRowEventArgs> rowAdded;
        private EventHandler<DomainRowEventArgs> rowRemoved;
        private EventHandler<DomainRowEventArgs> rowChanged;
        private EventHandler<DomainPropertyEventArgs> propertyChanged;
        private EventHandler<DomainDeletedEventArgs> deleted;

        protected Domain(DomainSerializationInfo serializationInfo, object source)
        {
            this.Source = source;
            this.Initialize(serializationInfo.DomainInfo);
            this.Name = serializationInfo.DomainInfo.DomainID.ToString();
            this.data = this.SerializeSource(source);
            this.Users = new DomainUserCollection(this);
            this.InitializeUsers(serializationInfo.UserInfos);
        }

        protected Domain(string creatorID, object source, Guid dataBaseID, string itemPath, string itemType)
        {
            var signatureDate = new SignatureDate(creatorID, DateTime.UtcNow);
            var domainInfo = new DomainInfo()
            {
                DomainID = Guid.NewGuid(),
                CreationInfo = signatureDate,
                ModificationInfo = signatureDate,
                DataBaseID = dataBaseID,
                ItemPath = itemPath,
                ItemType = itemType,
                DomainType = this.GetType().Name,
            };
            this.Source = source;
            this.Initialize(domainInfo);
            base.DomainState = DomainState.IsActivated;
            this.Name = base.DomainInfo.DomainID.ToString();
            this.Users = new DomainUserCollection(this);
        }

        public async Task<object> DeleteAsync(Authentication authentication, bool isCanceled)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, isCanceled);
                    this.ValidateDelete(authentication, isCanceled);
                });
                if (this.Host != null)
                {
                    return await this.Host.DeleteAsync(authentication, isCanceled, null);
                }
                else
                {
                    await this.Logger?.DisposeAsync(true);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        var container = this.Container;
                        this.Sign(authentication, true);
                        this.Logger = null;
                        this.Dispose();
                        this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled, null));
                        container.InvokeDomainDeletedEvent(authentication, new Domain[] { this }, new bool[] { isCanceled }, new object[] { null });
                    });
                    return null;
                }
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
                    this.ValidateBeginUserEdit(authentication, location);
                });
                var domainUser = await this.InvokeBeginUserEditAsync(authentication, location);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
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
                    this.ValidateEndUserEdit(authentication);
                });
                var domainUser = await this.InvokeEndUserEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
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
                    this.ValidateNewRow(authentication, rows);
                });
                var id = await this.Logger.NewRowAsync(authentication, rows);
                var result = await this.InvokeNewRowAsync(authentication, rows);
                await this.Logger.CompleteAsync(id);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.data = null;
                    this.IsModified = true;
                    this.CremaHost.Sign(authentication);
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowAdded(new DomainRowEventArgs(authentication, this, rows));
                    this.Container.InvokeDomainRowAddedEvent(authentication, this, rows);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
                return result;
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
                    this.ValidateSetRow(authentication, rows);
                });
                var id = await this.Logger.SetRowAsync(authentication, rows);
                var result = await this.InvokeSetRowAsync(authentication, rows);
                await this.Logger.CompleteAsync(id);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.data = null;
                    this.IsModified = true;
                    this.CremaHost.Sign(authentication);
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowChanged(new DomainRowEventArgs(authentication, this, rows));
                    this.Container.InvokeDomainRowChangedEvent(authentication, this, rows);
                    this.Container.InvokeDomainStateChangedEvent(authentication, this);
                    this.Container.InvokeDomainInfoChangedEvent(authentication, this);
                });
                return result;
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
                    this.ValidateRemoveRow(authentication, rows);
                });
                var id = await this.Logger.RemoveRowAsync(authentication, rows);
                await this.InvokeRemoveRowAsync(authentication, rows);
                await this.Logger.CompleteAsync(id);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.data = null;
                    this.IsModified = true;
                    this.CremaHost.Sign(authentication);
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
                    this.ValidateSetProperty(authentication, propertyName, value);
                });
                var id = await this.Logger.SetPropertyAsync(authentication, propertyName, value);
                await this.InvokeSetPropertyAsync(authentication, propertyName, value);
                await this.Logger.CompleteAsync(id);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.data = null;
                    this.IsModified = true;
                    this.CremaHost.Sign(authentication);
                    base.UpdateModificationInfo(authentication.SignatureDate);
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
                    this.ValidateSetLocation(authentication, location);
                });
                var domainUser = await this.InvokeSetUserLocationAsync(authentication, location);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
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
                    this.ValidateKick(authentication, userID, comment);
                });
                var id = await this.Logger.KickAsync(authentication, userID, comment);
                var tuple = await this.InvokeKickAsync(authentication, userID, comment);
                var domainUser = tuple.Item1;
                var removeInfo = tuple.Item2;
                await this.Logger.CompleteAsync(id);
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, removeInfo));
                    this.Container.InvokeDomainUserRemovedEvent(authentication, this, domainUser, removeInfo);
                    return domainUser.DomainUserInfo;
                });
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
                    this.ValidateSetOwner(authentication, userID);
                });
                var id = await this.Logger.SetOwnerAsync(authentication, userID);
                var tuple = await this.InvokeSetOwnerAsync(authentication, userID);
                var oldOwner = tuple.Item1;
                var newOwner = tuple.Item2;
                await this.Logger.CompleteAsync(id);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
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
            if (this.Users.ContainsKey(authentication.ID) == true || authentication.IsSystem == true)
            {
                if (this.data == null)
                    this.data = this.DataDispatcher.Invoke(() => this.SerializeSource(this.Source));
                metaData.Data = this.data;
            }
            return metaData;
        }

        public Task<DomainMetaData> GetMetaDataAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return this.Dispatcher.InvokeAsync(() => this.GetMetaData(authentication));
        }

        public void Dispose(DomainContext domainContext)
        {
            base.Dispose();
        }

        public void Dispose(Authentication authentication, bool isCanceled, object result)
        {
            base.Dispose();
            this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled, result));
        }

        public void Attach(params Authentication[] authentications)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in authentications)
            {
                if (this.Users.ContainsKey(item.ID) == true)
                {
                    this.Sign(item, true);
                    this.InvokeAttach(item, out var domainUser);
                    this.OnUserChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainUserChangedEvent(item, this, domainUser);
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
        }

        //public Task AttachAsync(params Authentication[] authentications)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        foreach (var item in authentications)
        //        {
        //            if (this.Users.ContainsKey(item.ID) == true)
        //            {
        //                this.Sign(item, true);
        //                this.InvokeAttach(item, out var domainUser);
        //                this.OnUserChanged(new DomainUserEventArgs(item, this, domainUser));
        //                this.OnDomainStateChanged(new DomainEventArgs(item, this));
        //                this.Container.InvokeDomainUserChangedEvent(item, this, domainUser);
        //                this.Container.InvokeDomainStateChangedEvent(item, this);
        //            }
        //        }
        //    });
        //}

        public void Detach(params Authentication[] authentications)
        {
            foreach (var item in authentications)
            {
                if (this.Users[item.ID] is DomainUser user && user.IsOnline == true)
                {
                    this.Sign(item, true);
                    this.InvokeDetach(item, out var domainUser);
                    this.OnUserChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainUserChangedEvent(item, this, domainUser);
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
        }

        //public Task DetachAsync(params Authentication[] authentications)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        foreach (var item in authentications)
        //        {
        //            if (this.Users[item.ID] is DomainUser user && user.IsOnline == true)
        //            {
        //                this.Sign(item, true);
        //                this.InvokeDetach(item, out var domainUser);
        //                this.OnUserChanged(new DomainUserEventArgs(item, this, domainUser));
        //                this.OnDomainStateChanged(new DomainEventArgs(item, this));
        //                this.Container.InvokeDomainUserChangedEvent(item, this, domainUser);
        //                this.Container.InvokeDomainStateChangedEvent(item, this);
        //            }
        //        }
        //    });
        //}

        public async Task AddUserAsync(Authentication authentication, DomainAccessType accessType)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.DebugMethod(authentication, this, nameof(AddUserAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, accessType);
                this.ValidateAdd(authentication);
            });
            var id = await this.Logger.JoinAsync(authentication, accessType);
            var domainUser = await this.InvokeAddUserAsync(authentication, accessType);
            await this.Logger.CompleteAsync(id);
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.DomainUserState &= ~DomainUserState.Detached;
                this.CremaHost.Sign(authentication);
                this.OnUserAdded(new DomainUserEventArgs(authentication, this, domainUser));
                this.Container?.InvokeDomainUserAddedEvent(authentication, this, domainUser);
            });
        }

        public async Task RemoveUserAsync(Authentication authentication)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.DebugMethod(authentication, this, nameof(RemoveUserAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                this.ValidateRemove(authentication);
            });
            var id = await this.Logger.DisjoinAsync(authentication, RemoveInfo.Empty);
            var tuple = await this.InvokeRemoveUserAsync(authentication);
            var domainUser = tuple.Item1;
            var isMaster = tuple.Item2;
            await this.Logger.CompleteAsync(id);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.Sign(authentication);
                this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, RemoveInfo.Empty));
                this.Container?.InvokeDomainUserRemovedEvent(authentication, this, domainUser, RemoveInfo.Empty);
                if (isMaster == true && this.Users.Owner != null)
                {
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, this.Users.Owner));
                    this.Container?.InvokeDomainUserChangedEvent(authentication, this, this.Users.Owner);
                }
            });
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
            this.Container?.InvokeDomainStateChangedEvent(authentication, this);
        }

        public async Task SetDomainHostAsync(IDomainHost host)
        {
            Authentication.System.Sign();

            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Host = host;
                if (this.Host != null)
                {
                    base.DomainState |= DomainState.IsActivated;
                }
                else
                {
                    base.DomainState &= ~DomainState.IsActivated;
                }
                this.OnDomainStateChanged(new DomainEventArgs(Authentication.System, this));
            });
            await this.Container.Dispatcher.InvokeAsync(() =>
            {
                this.Container.InvokeDomainStateChangedEvent(Authentication.System, this);
            });
        }

        public DomainSerializationInfo GetSerializationInfo()
        {
            var query = from DomainUser item in this.Users select item.DomainUserInfo;
            var properties = new Dictionary<string, object>();
            var serializationInfo = new DomainSerializationInfo()
            {
                DomainType = this.GetType().AssemblyQualifiedName,
                SourceType = this.Source.GetType().AssemblyQualifiedName,
                DomainInfo = this.DomainInfo,
                UserInfos = query.ToArray(),
            };
            this.OnSerializaing(properties);
            foreach (var item in properties)
            {
                serializationInfo.AddProperty(item.Key, item.Value);
            }
            return serializationInfo;
        }

        public Guid ID
        {
            get => Guid.Parse(this.Name);
            set => this.Name = value.ToString();
        }

        public DomainUserCollection Users { get; }

        public IDomainHost Host { get; set; }

        public Guid DataBaseID => base.DomainInfo.DataBaseID;

        public object Source { get; }

        public new DomainContext Context { get; set; }

        public CremaDispatcher Dispatcher => this.Context.Dispatcher;

        public CremaDispatcher DataDispatcher => this.Logger?.Dispatcher;

        public DomainLogger Logger { get; set; }

        public string ItemPath => base.DomainInfo.ItemPath;

        public Func<DateTime> DateTimeProvider
        {
            get => this.dateTimeProvider ?? this.GetTime;
            set => this.dateTimeProvider = value;
        }

        public CremaHost CremaHost => this.Context.CremaHost;

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
                this.Dispatcher.VerifyAccess();
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

        protected virtual async Task<DomainRowInfo[]> OnNewRowAsync(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            await Task.Delay(1);
            return null;
        }

        protected virtual async Task<DomainRowInfo[]> OnSetRowAsync(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            await Task.Delay(1);
            return null;
        }

        protected virtual async Task OnRemoveRowAsync(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            await Task.Delay(1);
        }

        protected virtual async Task OnSetPropertyAsync(DomainUser domainUser, string propertyName, object value, SignatureDateProvider signatureProvider)
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

        protected abstract byte[] SerializeSource(object source);

        protected abstract object DerializeSource(byte[] data);

        protected virtual void OnSerializaing(IDictionary<string, object> properties)
        {

        }

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

        private void ValidateDelete(Authentication authentication, bool isCanceled)
        {
            var isOwner = this.Users.OwnerUserID == authentication.ID;
            if (authentication.IsAdmin == false && isOwner == false)
                throw new PermissionDeniedException();
            //if (this.Host is IDomainHost domainHost)
            //    domainHost.ValidateDelete(authentication, isCanceled);
        }

        private void ValidateAdd(Authentication authentication)
        {
            if (this.Users.ContainsKey(authentication.ID) == true)
                throw new NotImplementedException();
        }

        private void ValidateRemove(Authentication authentication)
        {
            var domainUser = this.GetDomainUser(authentication);
        }

        private void ValidateNewRow(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSetRow(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateRemoveRow(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSetProperty(Authentication authentication, string propertyName, object value)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSetLocation(Authentication authentication, DomainLocationInfo location)
        {
            var domainUser = this.GetDomainUser(authentication);
        }

        private void ValidateBeginUserEdit(Authentication authentication, DomainLocationInfo location)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateEndUserEdit(Authentication authentication)
        {
            var domainUser = this.GetDomainUser(authentication);
            if (domainUser.CanWrite == false)
                throw new PermissionDeniedException();
        }

        private void ValidateSendMessage(Authentication authentication, string message)
        {
            var domainUser = this.GetDomainUser(authentication);
        }

        private void ValidateKick(Authentication authentication, string userID, string comment)
        {
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            if (this.Users.ContainsKey(userID) == false)
                throw new ArgumentException(string.Format(Resources.Exception_UserIsNotInDomain_Format, userID), nameof(userID));

            if (authentication.ID == userID)
                throw new ArgumentException(Resources.Exception_CannotKickYourself, nameof(userID));

            var domainUser = this.Users[userID];
            if (domainUser.IsOwner == true)
                throw new PermissionDeniedException(Resources.Exception_OwnerCannotKicked);
        }

        private void ValidateSetOwner(Authentication authentication, string userID)
        {
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            if (this.Users.ContainsKey(userID) == false)
                throw new ArgumentException(string.Format(Resources.Exception_UserIsNotInDomain_Format, userID), nameof(userID));
        }

        private void ValidateDomainUser(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new NotImplementedException();

            if (this.Users.ContainsKey(authentication.ID) == false)
                throw new UserNotFoundException(authentication.ID);
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

        private async Task<DomainRowInfo[]> InvokeNewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            rows = await this.OnNewRowAsync(domainUser, rows, authentication.GetSignatureDateProvider());
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
            });
            return rows;
        }

        private async Task<DomainRowInfo[]> InvokeSetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            rows = await this.OnSetRowAsync(domainUser, rows, authentication.GetSignatureDateProvider());
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in rows)
                {
                    this.modifiedTableList.Add(item.TableName);
                }
                domainUser.IsModified = true;
            });
            return rows;
        }

        private async Task InvokeRemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnRemoveRowAsync(domainUser, rows, authentication.GetSignatureDateProvider());
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
            await this.OnSetPropertyAsync(domainUser, propertyName, value, authentication.GetSignatureDateProvider());
            await this.Dispatcher.InvokeAsync(() => domainUser.IsModified = true);
        }

        private async Task<DomainUser> InvokeSetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            await this.OnSetLocationAsync(domainUser, location);
            await this.Dispatcher.InvokeAsync(() => domainUser.Location = location);
            return domainUser;
        }

        private async Task<(DomainUser, RemoveInfo)> InvokeKickAsync(Authentication authentication, string userID, string comment)
        {
            var removeInfo = new RemoveInfo(RemoveReason.Kick, comment);
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var domainUser = this.Users[userID];
                this.Users.Remove(userID);
                if (domainUser.Authentication != null)
                {
                    domainUser.Authentication.Expired -= Authentication_Expired;
                    domainUser.Authentication = null;
                }
                return (domainUser, removeInfo);
            });
        }

        private async Task<(DomainUser, DomainUser)> InvokeSetOwnerAsync(Authentication authentication, string userID)
        {
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var oldOwner = this.Users.Owner;
                var newOwner = this.Users[userID];
                this.Users.Owner = newOwner;
                return (oldOwner, newOwner);
            });
        }

        private void InvokeAttach(Authentication authentication, out DomainUser domainUser)
        {
            domainUser = this.Users[authentication.ID];
            domainUser.IsOnline = true;
            domainUser.Authentication = authentication;
            authentication.Expired += Authentication_Expired;
        }

        private void InvokeDetach(Authentication authentication, out DomainUser domainUser)
        {
            domainUser = this.Users[authentication.ID];
            domainUser.IsOnline = false;
            if (domainUser.Authentication != null)
            {
                domainUser.Authentication.Expired -= Authentication_Expired;
                domainUser.Authentication = null;
            }
        }

        private Task<DomainUser> InvokeAddUserAsync(Authentication authentication, DomainAccessType accessType)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var domainUser = new DomainUser(this, authentication.ID, authentication.Name, accessType)
                {
                    IsOnline = authentication.Types.HasFlag(AuthenticationType.User),
                    DomainUserState = DomainUserState.Detached,
                };
                this.Users.Add(domainUser);
                domainUser.Authentication = authentication;
                authentication.Expired += Authentication_Expired;
                return domainUser;
            });
        }

        private async Task<(DomainUser, bool)> InvokeRemoveUserAsync(Authentication authentication)
        {
            var domainUser = await this.GetDomainUserAsync(authentication);
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var isOwner = domainUser.IsOwner;
                domainUser.Authentication = null;
                authentication.Expired -= Authentication_Expired;
                this.Users.Remove(authentication.ID);
                return (domainUser, isOwner);
            });
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

        private DomainUser GetDomainUser(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            if (this.Users.ContainsKey(authentication.ID) == false)
                throw new UserNotFoundException(authentication.ID);
            return this.Users[authentication.ID];
        }

        private DateTime GetTime()
        {
            return DateTime.UtcNow;
        }

        private void Sign(Authentication authentication, bool defaultProvider)
        {
            if (defaultProvider == false)
            {
                var signatureProvider = new InternalSignatureDateProvider(authentication, this.DateTimeProvider());
                authentication.Sign(signatureProvider.DateTime);
            }
            else
            {
                authentication.Sign();
            }
        }

        private void Authentication_Expired(object sender, EventArgs e)
        {
            if (sender is Authentication authentication)
            {
                this.Dispatcher?.InvokeAsync(() =>
                {
                    var domainUser = this.GetDomainUser(authentication);
                    domainUser.Authentication = null;
                    domainUser.IsOnline = false;
                    authentication.Sign();
                    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                });
            }
        }

        private void InitializeUsers(DomainUserInfo[] users)
        {
            if (users == null)
                return;

            foreach (var item in users)
            {
                this.Users.Add(new DomainUser(this, item.UserID, item.UserName, item.AccessType));
            }
        }

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
            return (base.Context as IServiceProvider).GetService(serviceType);
        }

        #endregion

        #region classes

        class InternalSignatureDateProvider : SignatureDateProvider
        {
            public InternalSignatureDateProvider(Authentication authentication, DateTime dateTime)
                : base(authentication.ID)
            {
                this.DateTime = dateTime;
            }

            public DateTime DateTime { get; }

            protected override DateTime GetTime()
            {
                return this.DateTime;
            }
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => base.DomainInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => base.DomainState;

        #endregion
    }
}

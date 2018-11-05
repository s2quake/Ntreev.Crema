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
        private Func<DateTime> dateTimeProvider;
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
        private long postID;
        private long completionID;

        protected Domain(DomainSerializationInfo serializationInfo, object source)
        {
            this.Source = source;
            this.Initialize(serializationInfo.DomainInfo);
            this.Name = serializationInfo.DomainInfo.DomainID.ToString();
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

        public async Task<Guid> EnterAsync(Authentication authentication, DomainAccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterAsync), this, accessType);
                    this.ValidateAdd(authentication);
                    return new DomainUser(this, authentication.ID, authentication.Name, accessType)
                    {
                        DomainUserState = DomainUserState.Detached,
                        IsOnline = authentication.Types.HasFlag(AuthenticationType.User),
                    };
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeAddUserAsync(authentication, domainUser);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Users.Add(domainUser);
                    domainUser.Authentication = authentication;
                    authentication.Expired += Authentication_Expired;
                    domainUser.DomainUserState &= ~DomainUserState.Detached;
                    this.Logger.Complete(result.ID, this);
                    this.CremaHost.Sign(authentication);
                    this.OnUserAdded(new DomainUserEventArgs(authentication, this, domainUser) { TaskID = taskID });
                    this.Container?.InvokeDomainUserAddedEvent(authentication, this, domainUser, result.Data, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LeaveAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveAsync), this);
                    this.ValidateRemove(authentication);
                    return this.GetDomainUser(authentication);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeRemoveUserAsync(authentication, domainUser);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    domainUser.Authentication = null;
                    authentication.Expired -= Authentication_Expired;
                    this.Users.Remove(domainUser.ID);
                    this.Logger.Complete(result, this);
                    this.CremaHost.Sign(authentication);
                    this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, RemoveInfo.Empty) { TaskID = taskID });
                    this.Container?.InvokeDomainUserRemovedEvent(authentication, this, domainUser, RemoveInfo.Empty, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> DeleteAsync(Authentication authentication, bool isCanceled)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this, isCanceled);
                    this.ValidateDelete(authentication, isCanceled);
                });
                var taskID = Guid.NewGuid();
                var id = await this.Logger.Dispatcher.InvokeAsync(() => this.Logger.Delete(authentication));
                if (this.Host is IDomainHost host)
                {
                    this.Host = null;
                    try
                    {
                        await host.DeleteAsync(authentication, isCanceled);
                    }
                    catch
                    {
                        this.Host = host;
                        throw;
                    }
                }
                await this.Logger?.DisposeAsync(true);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var container = this.Container;
                    this.Sign(authentication, true);
                    this.Logger = null;
                    this.Dispose();
                    this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled) { TaskID = taskID });
                    container.InvokeDomainDeletedEvent(authentication, new Domain[] { this }, new bool[] { isCanceled }, taskID);
                });
                return taskID;
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginUserEditAsync), this);
                    this.ValidateBeginUserEdit(authentication, location);
                    return this.GetDomainUser(authentication);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeBeginUserEditAsync(authentication, domainUser, location);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    domainUser.DomainLocationInfo = location;
                    domainUser.IsBeingEdited = true;
                    this.Logger.Complete(result, this);
                    this.CremaHost.Sign(authentication);
                    this.OnUserEditBegun(new DomainUserLocationEventArgs(authentication, this, domainUser));
                    this.Container?.InvokeDomainUserEditBegunEvent(authentication, this, domainUser, taskID);
                });
                return taskID;
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndUserEditAsync), this);
                    this.ValidateEndUserEdit(authentication);
                    return this.GetDomainUser(authentication);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeEndUserEditAsync(authentication, domainUser);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    domainUser.IsBeingEdited = false;
                    this.Logger.Complete(result, this);
                    this.CremaHost.Sign(authentication);
                    this.OnUserEditEnded(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container?.InvokeDomainUserEditEndedEvent(authentication, this, domainUser, taskID);
                });
                return taskID;
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewRowAsync), this);
                    this.ValidateNewRow(authentication, rows);
                    return this.GetDomainUser(authentication);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeNewRowAsync(authentication, domainUser, rows);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in result.Rows)
                    {
                        this.modifiedTableList.Add(item.TableName);
                    }
                    domainUser.IsModified = true;
                    this.IsModified = true;
                    this.Logger.Complete(result.ID, this);
                    this.CremaHost.Sign(authentication);
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowAdded(new DomainRowEventArgs(authentication, this, result.Rows));
                    this.Container?.InvokeDomainRowAddedEvent(authentication, this, result.Rows, taskID);
                });
                return new DomainResultInfo<DomainRowInfo[]>() { ID = taskID, Value = result.Rows };
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetRowAsync), this);
                    this.ValidateSetRow(authentication, rows);
                    return this.GetDomainUser(authentication);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeSetRowAsync(authentication, domainUser, rows);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in result.Rows)
                    {
                        this.modifiedTableList.Add(item.TableName);
                    }
                    domainUser.IsModified = true;
                    this.IsModified = true;
                    this.Logger.Complete(result.ID, this);
                    this.CremaHost.Sign(authentication);
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowChanged(new DomainRowEventArgs(authentication, this, result.Rows));
                    this.Container?.InvokeDomainRowChangedEvent(authentication, this, result.Rows, taskID);
                });
                return new DomainResultInfo<DomainRowInfo[]>() { ID = taskID, Value = result.Rows };
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveRowAsync), this);
                    this.ValidateRemoveRow(authentication, rows);
                    return this.GetDomainUser(authentication);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeRemoveRowAsync(authentication, domainUser, rows);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in result.Rows)
                    {
                        this.modifiedTableList.Add(item.TableName);
                    }
                    domainUser.IsModified = true;
                    this.IsModified = true;
                    this.Logger.Complete(result.ID, this);
                    this.CremaHost.Sign(authentication);
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnRowRemoved(new DomainRowEventArgs(authentication, this, result.Rows));
                    this.Container?.InvokeDomainRowRemovedEvent(authentication, this, result.Rows, taskID);
                });
                return new DomainResultInfo<DomainRowInfo[]>() { ID = taskID, Value = result.Rows };
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPropertyAsync), this, propertyName, value);
                    this.ValidateSetProperty(authentication, propertyName, value);
                    return this.GetDomainUser(authentication);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeSetPropertyAsync(authentication, domainUser, propertyName, value);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    domainUser.IsModified = true;
                    this.IsModified = true;
                    this.Logger.Complete(result, this);
                    this.CremaHost.Sign(authentication);
                    base.UpdateModificationInfo(authentication.SignatureDate);
                    this.OnPropertyChanged(new DomainPropertyEventArgs(authentication, this, propertyName, value));
                    this.Container?.InvokeDomainPropertyChangedEvent(authentication, this, propertyName, value, taskID);
                });
                return taskID;
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
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserLocationAsync), this);
                    this.ValidateSetLocation(authentication, location);
                    var domainUser = this.GetDomainUser(authentication);
                    domainUser.DomainLocationInfo = location;
                    this.CremaHost.Sign(authentication);
                    this.OnUserLocationChanged(new DomainUserLocationEventArgs(authentication, this, domainUser));
                    this.Container?.InvokeDomainUserLocationChangedEvent(authentication, this, domainUser);
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), this, userID, comment);
                    this.ValidateKick(authentication, userID, comment);
                    return this.GetDomainUser(userID);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeKickAsync(authentication, domainUser, userID, comment);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Users.Remove(userID);
                    if (domainUser.Authentication != null)
                    {
                        domainUser.Authentication.Expired -= Authentication_Expired;
                        domainUser.Authentication = null;
                    }
                    this.Logger.Complete(result.ID, this);
                    this.CremaHost.Sign(authentication);
                    this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, result.RemoveInfo));
                    this.Container?.InvokeDomainUserRemovedEvent(authentication, this, domainUser, result.RemoveInfo, taskID);
                    return domainUser.DomainUserInfo;
                });
                return taskID;
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
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetOwnerAsync), this, userID);
                    this.ValidateSetOwner(authentication, userID);
                    return this.GetDomainUser(userID);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeSetOwnerAsync(authentication, domainUser);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Users.Owner = domainUser;
                    this.Logger.Complete(result, this);
                    this.CremaHost.Sign(authentication);
                    this.OnOwnerChanged(new DomainUserEventArgs(authentication, this, domainUser));
                    this.Container?.InvokeDomainOwnerChangedEvent(authentication, this, domainUser, taskID);
                });
                return taskID;
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
                CompetionID = this.Logger.CompletionID,
        };
            this.DataDispatcher.Invoke(() =>
            {
                if (this.Users.ContainsKey(authentication.ID) == true || authentication.IsSystem == true)
                {
                    metaData.Data = this.SerializeSource(this.Source);
                }
                metaData.PostID = this.Logger.ID;
            });
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

        public void Dispose(Authentication authentication, bool isCanceled)
        {
            base.Dispose();
            this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled));
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
                    this.OnUserStateChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.Container.InvokeDomainUserStateChangedEvent(item, this, domainUser);
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
        }

        public void Detach(params Authentication[] authentications)
        {
            foreach (var item in authentications)
            {
                if (this.Users[item.ID] is DomainUser user && user.IsOnline == true)
                {
                    this.Sign(item, true);
                    this.InvokeDetach(item, out var domainUser);
                    this.OnUserStateChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainUserStateChangedEvent(item, this, domainUser);
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
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

        public object Result { get; set; }

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

        protected virtual DomainRowInfo[] OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            return null;
        }

        protected virtual DomainRowInfo[] OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            return null;
        }

        protected virtual DomainRowInfo[] OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            return null;
        }

        protected virtual void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDateProvider signatureProvider)
        {

        }

        protected virtual void OnBeginUserEdit(DomainUser domainUser, DomainLocationInfo location)
        {

        }

        protected virtual void OnEndUserEdit(DomainUser domainUser)
        {

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

        private Task<long> InvokeBeginUserEditAsync(Authentication authentication, DomainUser domainUser, DomainLocationInfo location)
        {
            return this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.BeginUserEdit(authentication, location);
                this.OnBeginUserEdit(domainUser, location);
                return id;
            });
        }

        private Task<long> InvokeEndUserEditAsync(Authentication authentication, DomainUser domainUser)
        {
            return this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.EndUserEdit(authentication);
                this.OnEndUserEdit(domainUser);
                return id;
            });
        }

        private Task<(long ID, DomainRowInfo[] Rows)> InvokeNewRowAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows)
        {
            return this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.NewRow(authentication, rows);
                var resultRows = this.OnNewRow(domainUser, rows, authentication.GetSignatureDateProvider());
                return (id, resultRows);
            });
        }

        private Task<(long ID, DomainRowInfo[] Rows)> InvokeSetRowAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows)
        {
            return this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.SetRow(authentication, rows);
                var resultRows = this.OnSetRow(domainUser, rows, authentication.GetSignatureDateProvider());
                return (id, resultRows);
            });
        }

        private Task<(long ID, DomainRowInfo[] Rows)> InvokeRemoveRowAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows)
        {
            return this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.RemoveRow(authentication, rows);
                var resultRows = this.OnRemoveRow(domainUser, rows, authentication.GetSignatureDateProvider());
                return (id, resultRows);
            });
        }

        private Task<long> InvokeSetPropertyAsync(Authentication authentication, DomainUser domainUser, string propertyName, object value)
        {
            return this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.SetProperty(authentication, propertyName, value);
                this.OnSetProperty(domainUser, propertyName, value, authentication.GetSignatureDateProvider());
                return id;
            });
        }

        private Task<(long ID, RemoveInfo RemoveInfo)> InvokeKickAsync(Authentication authentication, DomainUser domainUser, string userID, string comment)
        {
            return this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.Kick(authentication, userID, comment);
                return (id, new RemoveInfo(RemoveReason.Kick, comment));
            });
        }

        private async Task<long> InvokeSetOwnerAsync(Authentication authentication, DomainUser domainUser)
        {
            return await this.DataDispatcher.InvokeAsync(() =>
            {
                var id = this.Logger.SetOwner(authentication, domainUser.ID);
                return id;
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

        private async Task<(long ID, byte[] Data)> InvokeAddUserAsync(Authentication authentication, DomainUser domainUser)
        {
            return await this.DataDispatcher.InvokeAsync(() =>
            {
                var id  = this.Logger.Join(authentication, domainUser.DomainUserInfo.AccessType);
                return (id, this.SerializeSource(this.Source));
            });
        }

        private async Task<long> InvokeRemoveUserAsync(Authentication authentication, DomainUser domainUser)
        {
            return await this.DataDispatcher.InvokeAsync(() =>
            {
                return this.Logger.Disjoin(authentication, RemoveInfo.Empty);
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
            return this.GetDomainUser(authentication.ID);
        }

        private DomainUser GetDomainUser(string userID)
        {
            this.Dispatcher.VerifyAccess();
            if (this.Users.ContainsKey(userID) == false)
                throw new UserNotFoundException(userID);
            return this.Users[userID];
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
                    this.OnUserStateChanged(new DomainUserEventArgs(authentication, this, domainUser));
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

        Task IDomain.DeleteAsync(Authentication authentication, bool isCanceled)
        {
            return this.DeleteAsync(authentication, isCanceled);
        }

        Task IDomain.EnterAsync(Authentication authentication, DomainAccessType accessType)
        {
            return this.EnterAsync(authentication, accessType);
        }

        Task IDomain.LeaveAsync(Authentication authentication)
        {
            return this.LeaveAsync(authentication);
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

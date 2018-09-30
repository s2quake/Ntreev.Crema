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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.DataBaseService;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;

namespace Ntreev.Crema.Services.Data
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class DataBase : DataBaseBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext, Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        IDataBaseServiceCallback, IDataBase, ICremaService, IInfoProvider, IStateProvider
    {
        private readonly UserContext userContext;
        private TableContext tableContext;
        private TypeContext typeContext;
        private DataBaseServiceClient service;
        private CremaDispatcher serviceDispatcher;
        private DataBaseMetaData metaData;
        private Timer timer;

        private EventHandler<AuthenticationEventArgs> authenticationEntered;
        private EventHandler<AuthenticationEventArgs> authenticationLeft;

        private readonly HashSet<AuthenticationToken> authentications = new HashSet<AuthenticationToken>();

        public DataBase(CremaHost cremaHost, DataBaseInfo dataBaseInfo)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = cremaHost.Dispatcher;
            this.userContext = cremaHost.UserContext;
            base.Name = dataBaseInfo.Name;
            base.DataBaseInfo = dataBaseInfo;
        }

        public DataBase(CremaHost cremaHost, DataBaseMetaData metaData)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = cremaHost.Dispatcher;
            this.userContext = cremaHost.UserContext;
            base.Name = metaData.DataBaseInfo.Name;
            base.DataBaseInfo = metaData.DataBaseInfo;
            base.DataBaseState = metaData.DataBaseState;
            base.AccessInfo = metaData.AccessInfo;
            base.LockInfo = metaData.LockInfo;

            foreach (var item in metaData.Authentications)
            {
                if (this.userContext.Authenticate(item) is Authentication authentication)
                {
                    this.authentications.Add(authentication);
                }
            }
        }

        public override string ToString()
        {
            return base.Name;
        }

        public AccessType GetAccessType(Authentication authentication)
        {
            this.ValidateExpired();
            return base.GetAccessType(authentication);
        }

        public async Task SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    var result = await this.DataBases.Service.SetPublicAsync(base.Name);
                    this.CremaHost.Sign(authentication, result);
                    base.SetPublic(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetPublicEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetPrivateAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    var result = await this.DataBases.Service.SetPrivateAsync(base.Name);
                    this.CremaHost.Sign(authentication, result);
                    base.SetPrivate(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetPrivateEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMember), this, memberID, accessType);
                    var result = await this.DataBases.Service.AddAccessMemberAsync(base.Name, memberID, accessType);
                    this.CremaHost.Sign(authentication, result);
                    base.AddAccessMember(authentication, memberID, accessType);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsAddAccessMemberEvent(authentication, new IDataBase[] { this }, new string[] { memberID, }, new AccessType[] { accessType, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    var result = await this.DataBases.Service.SetAccessMemberAsync(base.Name, memberID, accessType);
                    this.CremaHost.Sign(authentication, result);
                    base.SetAccessMember(authentication, memberID, accessType);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetAccessMemberEvent(authentication, new IDataBase[] { this }, new string[] { memberID, }, new AccessType[] { accessType, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    var result = await this.DataBases.Service.RemoveAccessMemberAsync(base.Name, memberID);
                    this.CremaHost.Sign(authentication, result);
                    base.RemoveAccessMember(authentication, memberID);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsRemoveAccessMemberEvent(authentication, new IDataBase[] { this }, new string[] { memberID, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task LockAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this);
                    return base.Name;
                });
                var result = await this.DataBases.Service.LockAsync(name, comment);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Lock(authentication, comment);
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBases.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { comment, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task UnlockAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    return base.Name;
                });
                var result = await this.DataBases.Service.UnlockAsync(name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Unlock(authentication);
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBases.InvokeItemsUnlockedEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task LoadAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LoadAsync), this);
                });
                var result = await this.DataBases.Service.LoadAsync(base.Name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.metaData.DataBaseState = DataBaseState.Loaded;
                    base.DataBaseState = DataBaseState.Loaded;
                    base.Load(authentication);
                    this.DataBases.InvokeItemsLoadedEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task UnloadAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnloadAsync), this);
                });
                var result = await this.DataBases.Service.UnloadAsync(base.Name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.authentications.Clear();
                    this.tableContext?.Dispose();
                    this.tableContext = null;
                    this.typeContext?.Dispose();
                    this.typeContext = null;
                    this.metaData.DataBaseState = DataBaseState.None;
                    base.DataBaseState = DataBaseState.None;
                    base.Unload(authentication);
                    this.DataBases.InvokeItemsUnloadedEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task EnterAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    if (this.IsLoaded == false)
                        throw new InvalidOperationException(Resources.Exception_CannotEnter);
                    this.authentications.Add(authentication);
                    if (this.authentications.Any(item => ((Authentication)item).ID == authentication.ID) && this.serviceDispatcher == null)
                    {
                        this.serviceDispatcher = new CremaDispatcher(this);
                        this.service = DataServiceFactory.CreateServiceClient(this.CremaHost.IPAddress, this.CremaHost.ServiceInfos[nameof(DataBaseService)], this);
                        this.service.Open();
                        if (this.service is ICommunicationObject service)
                        {
                            service.Faulted += Service_Faulted;
                        }
                        var result = await this.service.SubscribeAsync(this.CremaHost.AuthenticationToken, base.Name);
#if !DEBUG
                        this.timer = new Timer(30000);
                        this.timer.Elapsed += Timer_Elapsed;
                        this.timer.Start();
#endif
                        this.CremaHost.Sign(authentication, result);

                        this.typeContext = new TypeContext(this, result.Value);
                        this.tableContext = new TableContext(this, result.Value);
                        this.AttachDomainHost();
                        this.CremaHost.AddService(this);
                        this.Dispatcher = this.serviceDispatcher;
                        base.UpdateAccessParent();
                        base.UpdateLockParent();
                        this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                        this.DataBases.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
                    }
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task LeaveAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.authentications.Remove(authentication);

                    if (this.authentications.Any(item => ((Authentication)item).ID == authentication.ID) == false && this.serviceDispatcher != null)
                    {
                        var signatureDate = await this.ReleaseServiceAsync();
                        authentication.SignatureDate = signatureDate;
                        this.DetachDomainHost();
                        this.typeContext.Dispose();
                        this.typeContext = null;
                        this.tableContext.Dispose();
                        this.tableContext = null;
                        this.Dispatcher = this.CremaHost.Dispatcher;
                        this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                        this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
                    }
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RenameAsync(Authentication authentication, string name)
        {
            try
            {
                this.ValidateExpired();
                var oldName = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    return base.Name;
                });
                var result = await this.DataBases.Service.RenameAsync(oldName, name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Name = name;
                    this.metaData.DataBaseInfo = base.DataBaseInfo;
                    this.DataBases.InvokeItemsRenamedEvent(authentication, new DataBase[] { this }, new string[] { oldName });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task DeleteAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    return base.Name;
                });
                var result = await this.DataBases.Service.DeleteAsync(name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.DataBaseState = DataBaseState.None;
                    this.DataBases.InvokeItemsDeletedEvent(authentication, new DataBase[] { this }, new string[] { name });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<bool> ContainsAsync(Authentication authentication)
        {
            return await this.Dispatcher.InvokeAsync(() => this.authentications.Contains(authentication));
        }

        public async Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    var result = await this.DataBases.Service.GetLogAsync(base.Name, revision);
                    this.CremaHost.Sign(authentication, result);
                    return result.Value.ToArray();
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RevertAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RevertAsync), this, revision);
                    var result = await this.DataBases.Service.RevertAsync(base.Name, revision);
                    this.CremaHost.Sign(authentication, result);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ImportAsync), this, comment);
                    var result = await this.Service.ImportDataSetAsync(dataSet, comment);
                    this.CremaHost.Sign(authentication, result);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<CremaDataSet> GetDataSetAsync(Authentication authentication, DataSetType dataSetType, string filterExpression, string revision)
        {
            try
            {
                this.ValidateExpired();
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, dataSetType, filterExpression, revision);
                    var result = await this.Service.GetDataSetAsync(dataSetType, filterExpression, revision);
                    this.CremaHost.Sign(authentication, result);
                    return result.Value;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DataBaseTransaction> BeginTransactionAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    var result = await this.DataBases.Service.BeginTransactionAsync(base.Name);
                    this.CremaHost.Sign(authentication, result);
                    if (this.IsLocked == false)
                    {
                        base.Lock(authentication, $"{this.ID}");
                        this.DataBases.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { $"{this.ID}", });
                    }
                    var transaction = new DataBaseTransaction(authentication, this, this.DataBases.Service);
                    transaction.Disposed += (s, e) =>
                    {
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            if (this.LockInfo.Comment == $"{this.ID}" && this.IsLocked == true)
                            {
                                base.Unlock(authentication);
                                this.DataBases.InvokeItemsUnlockedEvent(authentication, new IDataBase[] { this });
                            }
                        });
                    };
                    return transaction;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        //public void ValidateBeginInDataBase(Authentication authentication)
        //{
        //    this.ValidateDispatcher();
        //    if (authentication != Authentication.System && this.authentications.Contains(authentication) == false)
        //        throw new InvalidOperationException(Resources.Exception_NotInDataBase);
        //}

        //public void ValidateAsyncBeginInDataBase(Authentication authentication)
        //{
        //    if (this.Dispatcher == null)
        //        throw new InvalidOperationException(Resources.Exception_InvalidObject);
        //    if (authentication != Authentication.System && this.authentications.Contains(authentication) == false)
        //        throw new InvalidOperationException(Resources.Exception_NotInDataBase);
        //}

        public void ValidateGetDataSet(Authentication authentication)
        {
            if (this.IsLoaded == false)
                throw new NotImplementedException();
            this.VerifyAccessType(authentication, AccessType.Guest);
        }

        public bool VerifyAccess(Authentication authentication)
        {
            return this.authentications.Contains(authentication);
        }

        public Task<DataBaseMetaData> GetMetaDataAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (authentication == null)
                    throw new ArgumentNullException(nameof(authentication));
                return this.metaData;
            });
        }

        public void SetLoaded(Authentication authentication)
        {
            base.DataBaseState = DataBaseState.Loaded;
            base.Load(authentication);
        }

        public void SetUnloaded(Authentication authentication)
        {
            if (this.serviceDispatcher != null)
            {
                this.DetachDomainHost();
                this.ReleaseServiceAsync();
            }
            this.authentications.Clear();
            this.tableContext?.Dispose();
            this.tableContext = null;
            this.typeContext?.Dispose();
            this.typeContext = null;
            base.DataBaseState = DataBaseState.None;
            base.Unload(authentication);
        }

        public void SetResetting(Authentication authentication)
        {
            System.Diagnostics.Trace.WriteLine("Resetting");
            this.typeContext?.Dispose();
            this.tableContext?.Dispose();
            this.IsResetting = true;
            base.ResettingDataBase(authentication);
            this.DataBases.InvokeItemsResettingEvent(authentication, new IDataBase[] { this, });

            if (this.serviceDispatcher != null)
            {
                this.DetachDomainHost();
            }

            if (this.GetService(typeof(DomainContext)) is DomainContext domainContext)
            {
                var domains = domainContext.Domains.Where<Domain>(item => item.DataBaseID == this.ID).ToArray();
                foreach (var item in domains)
                {
                    item.Dispatcher.Invoke(() => item.Dispose(authentication, true));
                }
            }
        }

        public void SetReset(Authentication authentication, DomainMetaData[] metaDatas)
        {
            var domains = metaDatas.Where(item => item.DomainInfo.DataBaseID == this.ID).ToArray();
            this.CremaHost.DomainContext.AddDomains(domains);
            if (this.serviceDispatcher != null)
            {
                var result = this.service.GetMetaData();
                this.typeContext = new TypeContext(this, result.Value);
                this.tableContext = new TableContext(this, result.Value);
                this.AttachDomainHost();
                base.UpdateLockParent();
                base.UpdateAccessParent();
            }

            this.IsResetting = false;
            base.ResetDataBase(authentication);
            this.DataBases.InvokeItemsResetEvent(authentication, new IDataBase[] { this, });
            System.Diagnostics.Trace.WriteLine("Reset");
        }

        public void SetAuthenticationEntered(Authentication authentication)
        {
            this.authentications.Add(authentication);
            this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
            this.DataBases.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
        }

        public void SetAuthenticationLeft(Authentication authentication)
        {
            this.authentications.Remove(authentication);
            this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
            this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
        }

        public void SetDataBaseInfo(DataBaseInfo dataBaseInfo)
        {
            base.DataBaseInfo = dataBaseInfo;
        }

        public void SetDataBaseState(DataBaseState dataBaseState)
        {
            base.DataBaseState = dataBaseState;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessChangeType changeType, AccessInfo accessInfo)
        {
            if (changeType != AccessChangeType.Public)
                base.AccessInfo = accessInfo;
            else
                base.AccessInfo = AccessInfo.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            base.AccessInfo = accessInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetLockInfo(LockChangeType changeType, LockInfo lockInfo)
        {
            if (this.IsResetting == true)
                return;
            if (changeType == LockChangeType.Lock)
                base.LockInfo = lockInfo;
            else
                base.LockInfo = LockInfo.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetLockInfo(LockInfo lockInfo)
        {
            base.LockInfo = lockInfo;
        }

        public void Invoke(Action action)
        {
            this.Dispatcher.Invoke(action);
        }

        public void Close(CloseInfo closeInfo)
        {
            this.serviceDispatcher?.Invoke(() =>
            {
                this.timer?.Dispose();
                this.timer = null;
                if (this.service != null)
                {
                    try
                    {
                        if (closeInfo.Reason != CloseReason.NoResponding)
                        {
                            this.service.Unsubscribe();
                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                                this.service.Close();
                            else
                                this.service.Abort();
                        }
                        else
                        {
                            this.service.Abort();
                        }
                    }
                    catch
                    {
                        this.service.Abort();
                    }
                    this.service = null;
                }
                this.serviceDispatcher.Dispose();
                this.serviceDispatcher = null;
            });
        }

        public void Delete()
        {
            this.Dispatcher = null;
            base.DataBaseState = DataBaseState.None;
            this.tableContext = null;
            this.typeContext = null;
            this.OnDeleted(EventArgs.Empty);
        }

        public IDomainHost FindDomainHost(Domain domain)
        {
            var itemPath = domain.DomainInfo.ItemPath;
            var itemType = domain.DomainInfo.ItemType;

            if (itemType == nameof(TableContent))
            {
                return new TableContent.TableContentDomainHost(this.tableContext.Tables, domain, itemPath);
            }
            else if (itemType == nameof(NewTableTemplate))
            {
                if (this.tableContext[itemPath] is TableCategory category)
                {
                    return new NewTableTemplate(category);
                }
                else if (this.tableContext[itemPath] is Table table)
                {
                    return new NewTableTemplate(table);
                }
                throw new NotImplementedException();
            }
            else if (itemType == nameof(TableTemplate))
            {
                var table = this.tableContext[itemPath] as Table;
                return table.Template;
            }
            else if (itemType == nameof(NewTypeTemplate))
            {
                var category = this.typeContext[itemPath] as TypeCategory;
                return new NewTypeTemplate(category);
            }
            else if (itemType == nameof(TypeTemplate))
            {
                var type = this.typeContext[itemPath] as Type;
                return type.Template;
            }

            return null;
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(TableContext))
                return this.tableContext;
            else if (serviceType == typeof(TableCategoryCollection))
                return this.tableContext.Categories;
            else if (serviceType == typeof(TableCollection))
                return this.tableContext.Tables;
            else if (serviceType == typeof(TypeContext))
                return this.typeContext;
            else if (serviceType == typeof(TypeCategoryCollection))
                return this.typeContext.Categories;
            else if (serviceType == typeof(TypeCollection))
                return this.typeContext.Types;
            else if (serviceType == typeof(DomainContext))
                return this.CremaHost.DomainContext;
            else
                return this.CremaHost.GetService(serviceType);
        }

        public CremaHost CremaHost { get; }

        public DataBaseCollection DataBases => this.CremaHost.DataBases;

        public TableContext TableContext
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.tableContext;
            }
        }

        public TypeContext TypeContext
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.typeContext;
            }
        }

        public CremaDispatcher Dispatcher { get; private set; }

        public IDataBaseService Service => this.service;

        public new DataBaseInfo DataBaseInfo => base.DataBaseInfo;

        public new DataBaseState DataBaseState => base.DataBaseState;

        public AuthenticationInfo[] AuthenticationInfos => this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();

        public override TypeCategoryBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext> TypeCategory => this.typeContext?.Root;

        public override TableCategoryBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext> TableCategory => this.tableContext?.Root;

        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        public bool IsLoaded => this.DataBaseState.HasFlag(DataBaseState.Loaded);

        public bool IsResetting { get; private set; }

        public new bool IsLocked => base.IsLocked;

        public new bool IsPrivate => base.IsPrivate;

        public Guid ID => base.DataBaseInfo.ID;

        public new event EventHandler Renamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed -= value;
            }
        }

        public new event EventHandler Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted -= value;
            }
        }

        public new event EventHandler Loaded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Loaded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Loaded -= value;
            }
        }

        public new event EventHandler Unloaded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Unloaded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Unloaded -= value;
            }
        }

        public event EventHandler<AuthenticationEventArgs> AuthenticationEntered
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationEntered += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationEntered -= value;
            }
        }

        public event EventHandler<AuthenticationEventArgs> AuthenticationLeft
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationLeft += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationLeft -= value;
            }
        }

        public new event EventHandler Resetting
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Resetting += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Resetting -= value;
            }
        }

        public new event EventHandler Reset
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Reset += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Reset -= value;
            }
        }

        public new event EventHandler DataBaseInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseInfoChanged -= value;
            }
        }

        public new event EventHandler DataBaseStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseStateChanged -= value;
            }
        }

        protected override void OnDataBaseStateChanged(EventArgs e)
        {
            this.metaData.DataBaseState = base.DataBaseState;
            base.OnDataBaseStateChanged(e);
        }

        private void AttachDomainHost()
        {
            var domainContext = this.CremaHost.DomainContext;
            var domains = domainContext.Domains.Where<Domain>(item => item.DataBaseID == this.ID)
                                               .ToArray();

            Authentication.System.Sign();
            foreach (var item in domains)
            {
                try
                {
                    var target = this.FindDomainHost(item);
                    target.RestoreAsync(Authentication.System, item);
                    item.Host = target;
                    item.AttachUser();
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                }
            }
        }

        private void DetachDomainHost()
        {
            var domainContext = this.CremaHost.DomainContext;
            var domains = domainContext.Domains.Where<Domain>(item => item.DataBaseID == this.ID).ToArray();

            foreach (var item in domains)
            {
                item.Host?.DetachAsync();
                item.Host = null;
                item.DetachUser();
            }
        }

        //private void ValidateDispatcher()
        //{
        //    if (this.Dispatcher == null)
        //        throw new InvalidOperationException(Resources.Exception_InvalidObject);
        //    this.Dispatcher?.VerifyAccess();
        //}

        private async Task<SignatureDate> ReleaseServiceAsync()
        {
            var result = await this.service.UnsubscribeAsync();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                this.service.Close();
            else
                this.service.Abort();
            this.timer?.Dispose();
            this.timer = null;
            this.service = null;
            this.serviceDispatcher.Dispose();
            this.serviceDispatcher = null;
            result.Validate();
            this.CremaHost.RemoveService(this);
            return result.SignatureDate;
        }

        private void Service_Faulted(object sender, EventArgs e)
        {
            this.serviceDispatcher.Invoke(() =>
            {
                try
                {
                    this.service.Abort();
                    this.service = null;
                }
                catch
                {

                }
                this.timer?.Dispose();
                this.timer = null;
                this.serviceDispatcher.Dispose();
                this.serviceDispatcher = null;
            });
            this.InvokeAsync(() =>
            {
                this.CremaHost.RemoveService(this);
                this.OnUnloaded(EventArgs.Empty);
            }, nameof(Service_Faulted));
        }

        private async void InvokeAsync(Action action, string callbackName)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(action);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(callbackName);
                this.CremaHost.Error(e);
            }
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.timer?.Stop();
            try
            {
                await this.serviceDispatcher.InvokeAsync(() => this.service.IsAlive());
                this.timer?.Start();
            }
            catch
            {

            }
        }

        //        private void OnEnter(Authentication authentication)
        //        {
        //            if (this.IsLoaded == false)
        //                throw new InvalidOperationException(Resources.Exception_CannotEnter);
        //            this.authentications.Add(authentication);
        //            if (this.authentications.Any(item => ((Authentication)item).ID == authentication.ID) && this.serviceDispatcher == null)
        //            {
        //                this.serviceDispatcher = new CremaDispatcher(this);
        //                var metaData = this.serviceDispatcher.Invoke(() =>
        //                {
        //                    this.service = DataServiceFactory.CreateServiceClient(this.CremaHost.IPAddress, this.CremaHost.ServiceInfos[nameof(DataBaseService)], this);
        //                    this.service.Open();
        //                    if (this.service is ICommunicationObject service)
        //                    {
        //                        service.Faulted += Service_Faulted;
        //                    }
        //                    var result = this.service.Subscribe(this.CremaHost.AuthenticationToken, base.Name);
        //                    result.Validate(authentication);
        //#if !DEBUG
        //                    this.timer = new Timer(30000);
        //                    this.timer.Elapsed += Timer_Elapsed;
        //                    this.timer.Start();
        //#endif
        //                    return result.Value;
        //                });
        //                this.typeContext = new TypeContext(this, metaData);
        //                this.tableContext = new TableContext(this, metaData);
        //                this.AttachDomainHost();
        //                this.CremaHost.AddService(this);
        //                base.UpdateAccessParent();
        //                base.UpdateLockParent();
        //                this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
        //                this.DataBases.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
        //            }
        //        }

        //        private void OnLeave(Authentication authentication)
        //        {
        //            this.authentications.Remove(authentication);

        //            if (this.authentications.Any(item => ((Authentication)item).ID == authentication.ID) == false && this.serviceDispatcher != null)
        //            {
        //                var signatureDate = this.ReleaseService();
        //                authentication.SignatureDate = signatureDate;
        //                this.DetachDomainHost();
        //                this.typeContext.Dispose();
        //                this.typeContext = null;
        //                this.tableContext.Dispose();
        //                this.tableContext = null;
        //                this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
        //                this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
        //            }
        //        }

        #region IDataBase

        async Task<IDataBase> IDataBase.CopyAsync(Authentication authentication, string newDataBaseName, string comment, bool force)
        {
            return await this.DataBases.CopyDataBaseAsync(authentication, this, newDataBaseName, comment, force);
        }

        async Task<ITransaction> IDataBase.BeginTransactionAsync(Authentication authentication)
        {
            return await this.BeginTransactionAsync(authentication);
        }

        ITableContext IDataBase.TableContext => this.TableContext;

        ITypeContext IDataBase.TypeContext => this.TypeContext;

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IDataBase))
                return this;

            if (base.DataBaseState.HasFlag(DataBaseState.Loaded) == true && this.serviceDispatcher != null)
            {
                if (serviceType == typeof(ITableContext))
                    return this.tableContext;
                else if (serviceType == typeof(ITableCategoryCollection))
                    return this.tableContext.Categories;
                else if (serviceType == typeof(ITableCollection))
                    return this.tableContext.Tables;
                else if (serviceType == typeof(ITypeContext))
                    return this.typeContext;
                else if (serviceType == typeof(ITypeCategoryCollection))
                    return this.typeContext.Categories;
                else if (serviceType == typeof(ITypeCollection))
                    return this.typeContext.Types;
            }

            return this.CremaHost.GetService(serviceType);
        }

        #endregion

        #region IDataBaseServiceCallback

        void IDataBaseServiceCallback.OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            this.service.Abort();
            this.service = null;
            this.timer?.Dispose();
            this.timer = null;
            this.serviceDispatcher.Dispose();
            this.serviceDispatcher = null;
            this.InvokeAsync(() =>
            {
                base.DataBaseState = DataBaseState.None;
                this.CremaHost.RemoveService(this);
            }, nameof(IDataBaseServiceCallback.OnServiceClosed));
        }

        void IDataBaseServiceCallback.OnTablesChanged(SignatureDate signatureDate, TableInfo[] tableInfos)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var tables = new Table[tableInfos.Length];
                for (var i = 0; i < tableInfos.Length; i++)
                {
                    var tableInfo = tableInfos[i];
                    var table = this.tableContext.Tables[tableInfo.Name];
                    table.SetTableInfo(tableInfo);
                }
                this.tableContext.Tables.InvokeTablesTemplateChangedEvent(authentication, tables);
            }, nameof(IDataBaseServiceCallback.OnTablesChanged));
        }

        void IDataBaseServiceCallback.OnTablesStateChanged(SignatureDate signatureDate, string[] tableNames, TableState[] states)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var tables = new Table[tableNames.Length];
                for (var i = 0; i < tableNames.Length; i++)
                {
                    var table = this.tableContext.Tables[tableNames[i]];
                    var state = states[i];
                    table.SetTableState(state);
                }
                this.tableContext.Tables.InvokeTablesStateChangedEvent(authentication, tables);
            }, nameof(IDataBaseServiceCallback.OnTablesStateChanged));
        }

        void IDataBaseServiceCallback.OnTableItemsCreated(SignatureDate signatureDate, string[] itemPaths, TableInfo?[] args)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var tableItems = new ITableItem[itemPaths.Length];
                var categories = new List<TableCategory>();
                var tables = new List<Table>();

                for (var i = 0; i < itemPaths.Length; i++)
                {
                    var itemPath = itemPaths[i];
                    if (NameValidator.VerifyCategoryPath(itemPath) == true)
                    {
                        var categoryName = new CategoryName(itemPath);
                        var category = this.tableContext.Categories.Prepare(itemPath);
                        categories.Add(category);
                        tableItems[i] = category;
                    }
                    else
                    {
                        var tableInfo = (TableInfo)args[i];
                        var table = this.tableContext.Tables.AddNew(authentication, tableInfo.Name, tableInfo.CategoryPath);
                        table.Initialize(tableInfo);
                        tables.Add(table);
                        tableItems[i] = table;
                    }
                }

                if (categories.Any() == true)
                {
                    this.tableContext.Categories.InvokeCategoriesCreatedEvent(authentication, categories.ToArray());
                }

                if (tables.Any() == true)
                {
                    this.tableContext.Tables.InvokeTablesCreatedEvent(authentication, tables.ToArray());
                }
            }, nameof(IDataBaseServiceCallback.OnTableItemsCreated));
        }

        void IDataBaseServiceCallback.OnTableItemsRenamed(SignatureDate signatureDate, string[] itemPaths, string[] newNames)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);

                {
                    var items = new List<TableCategory>(itemPaths.Length);
                    var oldNames = new List<string>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is TableCategory == false)
                            continue;

                        var category = tableItem as TableCategory;
                        items.Add(category);
                        oldNames.Add(category.Name);
                        oldPaths.Add(category.Path);
                    }

                    if (items.Any() == true)
                    {
                        for (var i = 0; i < itemPaths.Length; i++)
                        {
                            var tableItem = this.tableContext[itemPaths[i]];
                            if (tableItem is TableCategory == false)
                                continue;

                            var category = tableItem as TableCategory;
                            var categoryName = newNames[i];
                            category.SetName(categoryName);
                        }

                        this.tableContext.Categories.InvokeCategoriesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                    }
                }

                {
                    var items = new List<Table>(itemPaths.Length);
                    var oldNames = new List<string>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is Table == false)
                            continue;

                        var table = tableItem as Table;
                        items.Add(table);
                        oldNames.Add(table.Name);
                        oldPaths.Add(table.Path);
                    }

                    if (items.Any() == true)
                    {
                        for (var i = 0; i < itemPaths.Length; i++)
                        {
                            var tableItem = this.tableContext[itemPaths[i]];
                            if (tableItem is Table == false)
                                continue;

                            var table = tableItem as Table;
                            var tableName = newNames[i];
                            table.SetName(tableName);
                        }

                        this.tableContext.Tables.InvokeTablesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                    }
                }
            }, nameof(IDataBaseServiceCallback.OnTableItemsRenamed));
        }

        void IDataBaseServiceCallback.OnTableItemsMoved(SignatureDate signatureDate, string[] itemPaths, string[] parentPaths)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);

                {
                    var items = new List<TableCategory>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);
                    var oldParentPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is TableCategory == false)
                            continue;

                        var category = tableItem as TableCategory;
                        items.Add(category);
                        oldPaths.Add(category.Path);
                        oldParentPaths.Add(category.Parent.Path);
                    }

                    if (items.Any() == true)
                    {
                        for (var i = 0; i < itemPaths.Length; i++)
                        {
                            var tableItem = this.tableContext[itemPaths[i]];
                            if (tableItem is TableCategory == false)
                                continue;

                            var category = tableItem as TableCategory;
                            var parent = this.tableContext.Categories[parentPaths[i]];
                            category.SetParent(parent);
                        }

                        this.tableContext.Categories.InvokeCategoriesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                    }
                }

                {
                    var items = new List<Table>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);
                    var oldParentPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is Table == false)
                            continue;

                        var table = tableItem as Table;
                        items.Add(table);
                        oldPaths.Add(table.Path);
                        oldParentPaths.Add(table.Category.Path);
                    }

                    if (items.Any() == true)
                    {
                        for (var i = 0; i < itemPaths.Length; i++)
                        {
                            var tableItem = this.tableContext[itemPaths[i]];
                            if (tableItem is Table == false)
                                continue;

                            var table = tableItem as Table;
                            var parent = this.tableContext.Categories[parentPaths[i]];
                            table.SetParent(parent);
                        }

                        this.tableContext.Tables.InvokeTablesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                    }
                }
            }, nameof(IDataBaseServiceCallback.OnTableItemsMoved));
        }

        void IDataBaseServiceCallback.OnTableItemsDeleted(SignatureDate signatureDate, string[] itemPaths)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);

                {
                    var items = new List<TableCategory>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is TableCategory == false)
                            continue;

                        var category = tableItem as TableCategory;
                        items.Add(category);
                        oldPaths.Add(category.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is TableCategory == false)
                            continue;

                        var category = tableItem as TableCategory;
                        category.Dispose();
                    }

                    this.tableContext.Categories.InvokeCategoriesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                }

                {
                    var items = new List<Table>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is Table == false)
                            continue;

                        var table = tableItem as Table;
                        items.Add(table);
                        oldPaths.Add(table.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var tableItem = this.tableContext[itemPaths[i]];
                        if (tableItem is Table == false)
                            continue;

                        var table = tableItem as Table;
                        table.Dispose();
                    }

                    this.tableContext.Tables.InvokeTablesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                }
            }, nameof(IDataBaseServiceCallback.OnTableItemsDeleted));
        }

        void IDataBaseServiceCallback.OnTableItemsAccessChanged(SignatureDate signatureDate, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var tableItems = new ITableItem[accessInfos.Length];
                for (var i = 0; i < accessInfos.Length; i++)
                {
                    var accessInfo = accessInfos[i];
                    var tableItem = this.tableContext[accessInfo.Path];
                    if (tableItem is Table table)
                    {
                        table.SetAccessInfo(changeType, accessInfo);
                    }
                    else if (tableItem is TableCategory category)
                    {
                        category.SetAccessInfo(changeType, accessInfo);
                    }
                    else
                    {
                        throw new NotImplementedException(accessInfo.Path);
                    }
                    tableItems[i] = tableItem as ITableItem;
                }
                switch (changeType)
                {
                    case AccessChangeType.Public:
                        this.TableContext.InvokeItemsSetPublicEvent(authentication, tableItems);
                        break;
                    case AccessChangeType.Private:
                        this.TableContext.InvokeItemsSetPrivateEvent(authentication, tableItems);
                        break;
                    case AccessChangeType.Add:
                        this.TableContext.InvokeItemsAddAccessMemberEvent(authentication, tableItems, memberIDs, accessTypes);
                        break;
                    case AccessChangeType.Set:
                        this.TableContext.InvokeItemsSetAccessMemberEvent(authentication, tableItems, memberIDs, accessTypes);
                        break;
                    case AccessChangeType.Remove:
                        this.TableContext.InvokeItemsRemoveAccessMemberEvent(authentication, tableItems, memberIDs);
                        break;
                }
            }, nameof(IDataBaseServiceCallback.OnTableItemsAccessChanged));
        }

        void IDataBaseServiceCallback.OnTableItemsLockChanged(SignatureDate signatureDate, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var tableItems = new ITableItem[lockInfos.Length];
                for (var i = 0; i < lockInfos.Length; i++)
                {
                    var lockInfo = lockInfos[i];
                    var tableItem = this.tableContext[lockInfo.Path];
                    if (tableItem is Table table)
                    {
                        table.SetLockInfo(changeType, lockInfo);
                    }
                    else if (tableItem is TableCategory category)
                    {
                        category.SetLockInfo(changeType, lockInfo);
                    }
                    else
                    {
                        throw new NotImplementedException(lockInfo.Path);
                    }
                    tableItems[i] = tableItem as ITableItem;
                }
                switch (changeType)
                {
                    case LockChangeType.Lock:
                        this.TableContext.InvokeItemsLockedEvent(authentication, tableItems, comments);
                        break;
                    case LockChangeType.Unlock:
                        this.TableContext.InvokeItemsUnlockedEvent(authentication, tableItems);
                        break;
                }
            }, nameof(IDataBaseServiceCallback.OnTableItemsLockChanged));
        }

        void IDataBaseServiceCallback.OnTypesChanged(SignatureDate signatureDate, TypeInfo[] typeInfos)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var types = new Type[typeInfos.Length];
                for (var i = 0; i < typeInfos.Length; i++)
                {
                    var typeInfo = typeInfos[i];
                    var type = this.typeContext.Types[typeInfo.Name];
                    type.SetTypeInfo(typeInfo);
                }
                this.typeContext.Types.InvokeTypesChangedEvent(authentication, types);
            }, nameof(IDataBaseServiceCallback.OnTypesChanged));
        }

        void IDataBaseServiceCallback.OnTypesStateChanged(SignatureDate signatureDate, string[] typeNames, TypeState[] states)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var types = new Type[typeNames.Length];
                for (var i = 0; i < typeNames.Length; i++)
                {
                    var type = this.typeContext.Types[typeNames[i]];
                    var state = states[i];
                    type.SetTypeState(state);
                }
                this.typeContext.Types.InvokeTypesStateChangedEvent(authentication, types);
            }, nameof(IDataBaseServiceCallback.OnTypesStateChanged));
        }

        void IDataBaseServiceCallback.OnTypeItemsCreated(SignatureDate signatureDate, string[] itemPaths, TypeInfo?[] args)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var typeItems = new ITypeItem[itemPaths.Length];
                var categories = new List<TypeCategory>();
                var types = new List<Type>();

                for (var i = 0; i < itemPaths.Length; i++)
                {
                    var itemPath = itemPaths[i];
                    if (NameValidator.VerifyCategoryPath(itemPath) == true)
                    {
                        var categoryName = new CategoryName(itemPath);
                        var category = this.typeContext.Categories.Prepare(itemPath);
                        categories.Add(category);
                        typeItems[i] = category;
                    }
                    else
                    {
                        var typeInfo = (TypeInfo)args[i];
                        var type = this.typeContext.Types.AddNew(authentication, typeInfo.Name, typeInfo.CategoryPath);
                        type.Initialize(typeInfo);
                        types.Add(type);
                        typeItems[i] = type;
                    }
                }

                if (categories.Any() == true)
                {
                    this.typeContext.Categories.InvokeCategoriesCreatedEvent(authentication, categories.ToArray());
                }

                if (types.Any() == true)
                {
                    this.typeContext.Types.InvokeTypesCreatedEvent(authentication, types.ToArray());
                }
            }, nameof(IDataBaseServiceCallback.OnTypeItemsCreated));
        }

        void IDataBaseServiceCallback.OnTypeItemsRenamed(SignatureDate signatureDate, string[] itemPaths, string[] newNames)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);

                {
                    var items = new List<TypeCategory>(itemPaths.Length);
                    var oldNames = new List<string>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is TypeCategory == false)
                            continue;

                        var category = typeItem as TypeCategory;
                        items.Add(category);
                        oldNames.Add(category.Name);
                        oldPaths.Add(category.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is TypeCategory == false)
                            continue;

                        var category = typeItem as TypeCategory;
                        category.SetName(newNames[i]);
                    }

                    this.typeContext.Categories.InvokeCategoriesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                }

                {
                    var items = new List<Type>(itemPaths.Length);
                    var oldNames = new List<string>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is Type == false)
                            continue;

                        var type = typeItem as Type;
                        items.Add(type);
                        oldNames.Add(type.Name);
                        oldPaths.Add(type.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is Type == false)
                            continue;

                        var type = typeItem as Type;
                        type.SetName(newNames[i]);
                    }

                    this.typeContext.Types.InvokeTypesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                }
            }, nameof(IDataBaseServiceCallback.OnTypeItemsRenamed));
        }

        void IDataBaseServiceCallback.OnTypeItemsMoved(SignatureDate signatureDate, string[] itemPaths, string[] parentPaths)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);

                {
                    var items = new List<TypeCategory>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);
                    var oldParentPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is TypeCategory == false)
                            continue;

                        var category = typeItem as TypeCategory;
                        items.Add(category);
                        oldPaths.Add(category.Path);
                        oldParentPaths.Add(category.Parent.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is TypeCategory == false)
                            continue;

                        var category = typeItem as TypeCategory;
                        var parent = this.typeContext.Categories[parentPaths[i]];
                        category.SetParent(parent);
                    }

                    this.typeContext.Categories.InvokeCategoriesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                }

                {
                    var items = new List<Type>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);
                    var oldParentPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is Type == false)
                            continue;

                        var type = typeItem as Type;
                        items.Add(type);
                        oldPaths.Add(type.Path);
                        oldParentPaths.Add(type.Category.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is Type == false)
                            continue;

                        var type = typeItem as Type;
                        var parent = this.typeContext.Categories[parentPaths[i]];
                        type.SetParent(parent);
                    }

                    this.typeContext.Types.InvokeTypesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                }
            }, nameof(IDataBaseServiceCallback.OnTypeItemsMoved));
        }

        void IDataBaseServiceCallback.OnTypeItemsDeleted(SignatureDate signatureDate, string[] itemPaths)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);

                {
                    var items = new List<TypeCategory>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is TypeCategory == false)
                            continue;

                        var category = typeItem as TypeCategory;
                        items.Add(category);
                        oldPaths.Add(category.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is TypeCategory == false)
                            continue;

                        var category = typeItem as TypeCategory;
                        category.Dispose();
                    }

                    this.typeContext.Categories.InvokeCategoriesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                }

                {
                    var items = new List<Type>(itemPaths.Length);
                    var oldPaths = new List<string>(itemPaths.Length);

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is Type == false)
                            continue;

                        var type = typeItem as Type;
                        items.Add(type);
                        oldPaths.Add(type.Path);
                    }

                    for (var i = 0; i < itemPaths.Length; i++)
                    {
                        var typeItem = this.typeContext[itemPaths[i]];
                        if (typeItem is Type == false)
                            continue;

                        var type = typeItem as Type;
                        type.Dispose();
                    }

                    this.typeContext.Types.InvokeTypesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                }
            }, nameof(IDataBaseServiceCallback.OnTypeItemsDeleted));
        }

        void IDataBaseServiceCallback.OnTypeItemsAccessChanged(SignatureDate signatureDate, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var typeItems = new ITypeItem[accessInfos.Length];
                for (var i = 0; i < accessInfos.Length; i++)
                {
                    var accessInfo = accessInfos[i];
                    var typeItem = this.typeContext[accessInfo.Path];
                    if (typeItem is Type type)
                    {
                        type.SetAccessInfo(changeType, accessInfo);
                    }
                    else if (typeItem is TypeCategory category)
                    {
                        category.SetAccessInfo(changeType, accessInfo);
                    }
                    else
                    {
                        throw new NotImplementedException(accessInfo.Path);
                    }
                    typeItems[i] = typeItem as ITypeItem;
                }
                switch (changeType)
                {
                    case AccessChangeType.Public:
                        this.TypeContext.InvokeItemsSetPublicEvent(authentication, typeItems);
                        break;
                    case AccessChangeType.Private:
                        this.TypeContext.InvokeItemsSetPrivateEvent(authentication, typeItems);
                        break;
                    case AccessChangeType.Add:
                        this.TypeContext.InvokeItemsAddAccessMemberEvent(authentication, typeItems, memberIDs, accessTypes);
                        break;
                    case AccessChangeType.Set:
                        this.TypeContext.InvokeItemsSetAccessMemberEvent(authentication, typeItems, memberIDs, accessTypes);
                        break;
                    case AccessChangeType.Remove:
                        this.TypeContext.InvokeItemsRemoveAccessMemberEvent(authentication, typeItems, memberIDs);
                        break;
                }
            }, nameof(IDataBaseServiceCallback.OnTypeItemsAccessChanged));
        }

        void IDataBaseServiceCallback.OnTypeItemsLockChanged(SignatureDate signatureDate, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            this.InvokeAsync(() =>
            {
                var authentication = this.userContext.Authenticate(signatureDate);
                var typeItems = new ITypeItem[lockInfos.Length];
                for (var i = 0; i < lockInfos.Length; i++)
                {
                    var lockInfo = lockInfos[i];
                    var typeItem = this.typeContext[lockInfo.Path];
                    if (typeItem is Type type)
                    {
                        type.SetLockInfo(changeType, lockInfo);
                    }
                    else if (typeItem is TypeCategory category)
                    {
                        category.SetLockInfo(changeType, lockInfo);
                    }
                    else
                    {
                        throw new NotImplementedException(lockInfo.Path);
                    }
                    typeItems[i] = typeItem as ITypeItem;
                }
                switch (changeType)
                {
                    case LockChangeType.Lock:
                        this.TypeContext.InvokeItemsLockedEvent(authentication, typeItems, comments);
                        break;
                    case LockChangeType.Unlock:
                        this.TypeContext.InvokeItemsUnlockedEvent(authentication, typeItems);
                        break;
                }
            }, nameof(IDataBaseServiceCallback.OnTypeItemsLockChanged));
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => this.DataBaseInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.DataBaseState;

        #endregion
    }
}

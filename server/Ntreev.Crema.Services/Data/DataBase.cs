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
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Data.Serializations;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    partial class DataBase : DataBaseBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext, Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        IDataBase, IInfoProvider, IStateProvider
    {
        private readonly IRepositoryProvider repositoryProvider;
        private readonly IObjectSerializer serializer;
        private readonly string cachePath;
        private TypeContext typeContext;
        private TableContext tableContext;
        private UserContext userContext;
        private CremaDataSet dataSet;
        private DataBaseMetaData metaData;
        private AuthenticationInfo[] authenticationInfos;

        private EventHandler<AuthenticationEventArgs> authenticationEntered;
        private EventHandler<AuthenticationEventArgs> authenticationLeft;

        private HashSet<AuthenticationToken> authentications = new HashSet<AuthenticationToken>();

        public DataBase(CremaHost cremaHost, string name)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = cremaHost.Dispatcher;
            this.repositoryProvider = cremaHost.RepositoryProvider;
            this.serializer = cremaHost.Serializer;
            base.Name = name;
            this.cachePath = cremaHost.GetPath(CremaPath.Caches, DataBaseCollection.DataBasesString);
            this.userContext = this.CremaHost.UserContext;
            this.userContext.Dispatcher.Invoke(() => this.userContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
            this.Initialize();
        }

        public DataBase(CremaHost cremaHost, string name, DataBaseSerializationInfo dataBaseInfo)
            : this(cremaHost, name)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = cremaHost.Dispatcher;
            this.repositoryProvider = cremaHost.RepositoryProvider;
            this.serializer = cremaHost.Serializer;
            base.Name = name;
            this.cachePath = cremaHost.GetPath(CremaPath.Caches, DataBaseCollection.DataBasesString);
            this.userContext = this.CremaHost.UserContext;
            this.userContext.Dispatcher.Invoke(() => this.userContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
            base.DataBaseInfo = (DataBaseInfo)dataBaseInfo;
            this.Initialize();
        }

        public override string ToString()
        {
            return base.Name;
        }

        public AccessType GetAccessType(Authentication authentication)
        {
            this.ValidateBeginInDataBase(authentication);
            return base.GetAccessType(authentication);
        }

        public async Task SetPublicAsync(Authentication authentication)
        {
            try
            {
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                    var signatureDate = await this.InvokeDataBaseSetPublicAsync(authentication);
                    this.Sign(authentication, signatureDate);
                    base.SetPublic(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetPublicEvent(authentication, this.BasePath, new IDataBase[] { this });
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    base.ValidateSetPrivate(authentication);
                    var signatureDate = await this.InvokeDataBaseSetPrivateAsync(authentication);
                    this.Sign(authentication, signatureDate);
                    base.SetPrivate(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetPrivateEvent(authentication, this.BasePath, new IDataBase[] { this });
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                    var signatureDate = await this.InvokeDataBaseAddAccessMemberAsync(authentication, memberID, accessType);
                    this.Sign(authentication, signatureDate);
                    base.AddAccessMember(authentication, memberID, accessType);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsAddAccessMemberEvent(authentication, this.BasePath, new IDataBase[] { this }, new string[] { memberID }, new AccessType[] { accessType });
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                    var signatureDate = await this.InvokeDataBaseSetAccessMemberAsync(authentication, memberID, accessType);
                    this.Sign(authentication, signatureDate);
                    base.SetAccessMember(authentication, memberID, accessType);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetAccessMemberEvent(authentication, this.BasePath, new IDataBase[] { this }, new string[] { memberID }, new AccessType[] { accessType });
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    base.ValidateRemoveAccessMember(authentication, memberID);
                    var signatureDate = await this.InvokeDataBaseRemoveAccessMemberAsync(authentication, memberID);
                    this.Sign(authentication, signatureDate);
                    base.RemoveAccessMember(authentication, memberID);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsRemoveAccessMemberEvent(authentication, this.BasePath, new IDataBase[] { this }, new string[] { memberID });
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
                this.ValidateDispatcher();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this, comment);
                    base.ValidateLock(authentication);
                    base.Lock(authentication, comment);
                    this.Sign(authentication);
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBases.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { comment });
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
                this.ValidateDispatcher();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    base.ValidateUnlock(authentication);
                    base.Unlock(authentication);
                    this.Sign(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LoadAsync), this);
                    this.ValidateLoad(authentication);
                    this.Sign(authentication);
                    var repositorySetting = new RepositorySettings()
                    {
                        BasePath = this.DataBases.RemotePath,
                        RepositoryName = this.Name,
                        WorkingPath = this.BasePath,
                        TransactionPath = this.CremaHost.GetPath(CremaPath.Transactions, $"{this.ID}"),
                        LogService = this.CremaHost,
                    };
                    this.Repository = await Task.Run(() => new DataBaseRepositoryHost(this, this.repositoryProvider.CreateInstance(repositorySetting)));
                    this.Repository.Changed += Repository_Changed;
                    await this.ReadCacheAsync();
                    this.AttachDomainHost();
                    this.Dispatcher = new CremaDispatcher(this);
                    this.metaData.DataBaseState = DataBaseState.IsLoaded;
                    base.DataBaseState = DataBaseState.IsLoaded;
                    base.Load(authentication);
                    base.UpdateLockParent();
                    base.UpdateAccessParent();
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnloadAsync), this);
                    this.ValidateUnload(authentication);
                    this.DetachDomainHost();
                    this.WriteCache();
                    this.tableContext.Dispose();
                    this.tableContext = null;
                    this.typeContext.Dispose();
                    this.typeContext = null;
                    this.DetachUsers();
                    this.Repository.Changed -= Repository_Changed;
                    this.Repository.Dispose();
                    this.Repository = null;
                    this.Dispatcher.Dispose(false);
                    this.Dispatcher = this.CremaHost.Dispatcher;
                    this.metaData.DataBaseState = DataBaseState.None;
                    base.DataBaseState = DataBaseState.None;
                    base.Unload(authentication);
                    this.Sign(authentication);
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateEnter(authentication);
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterAsync), this);
                    this.OnEnter(authentication);
                    this.DataBases.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateLeave(authentication);
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveAsync), this);
                    this.OnLeave(authentication);
                    this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var oldName = base.Name;
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    this.DataBases.InvokeDataBaseRename(authentication, this, name);
                    this.Sign(authentication);
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Delete), this);
                    base.ValidateDelete(authentication);
                    this.DataBases.InvokeDataBaseDelete(authentication, this);
                    base.DataBaseState = DataBaseState.None;
                    this.DeleteCache();
                    this.tableContext = null;
                    this.typeContext = null;
                    this.Sign(authentication);
                    this.DataBases.InvokeItemsDeletedEvent(authentication, new DataBase[] { this }, new string[] { base.Name });
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
                var remotePath = this.CremaHost.GetPath(CremaPath.RepositoryDataBases);
                var logs = await this.CremaHost.RepositoryDispatcher.InvokeAsync(() =>
                {
                    return this.repositoryProvider.GetLog(remotePath, this.Name, revision);
                });
                return logs.ToArray();
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RevertAsync), this, revision);
                    this.DataBases.InvokeDataBaseRevert(authentication, this, revision);
                    this.Sign(authentication);
                });
                var repositoryInfo = await this.CremaHost.RepositoryDispatcher.InvokeAsync(() =>
                {
                    return this.repositoryProvider.GetRepositoryInfo(this.CremaHost.GetPath(CremaPath.RepositoryDataBases), base.Name);
                });
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.DataBaseInfo = new DataBaseInfo()
                    {
                        ID = repositoryInfo.ID,
                        Name = repositoryInfo.Name,
                        Revision = repositoryInfo.Revision,
                        Comment = repositoryInfo.Comment,
                        CreationInfo = repositoryInfo.CreationInfo,
                        ModificationInfo = repositoryInfo.ModificationInfo,
                    };
                    this.DataBases.InvokeItemsRevertedEvent(authentication, new IDataBase[] { this }, new string[] { revision });
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
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateBeginTransaction(authentication);
                    this.Sign(authentication);
                    if (this.IsLocked == false)
                    {
                        this.Lock(authentication, $"{this.ID}");
                    }
                    var transaction = new DataBaseTransaction(authentication, this, this.Repository);
                    transaction.Disposed += (s, e) =>
                    {
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            if (this.LockInfo.Comment == $"{this.ID}" && this.IsLocked == true)
                            {
                                this.Unlock(authentication);
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

        public void ValidateBeginInDataBase(Authentication authentication)
        {
            this.ValidateDispatcher();
            authentication.Verify();
            if (authentication != Authentication.System && this.authentications.Contains(authentication) == false)
                throw new InvalidOperationException(Resources.Exception_NotInDataBase);
        }

        public void ValidateAsyncBeginInDataBase(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            if (authentication != Authentication.System && this.authentications.Contains(authentication) == false)
                throw new InvalidOperationException(Resources.Exception_NotInDataBase);
        }

        public void ValidateGetDataSet(Authentication authentication)
        {
            if (this.IsLoaded == false)
                throw new NotImplementedException();
            this.VerifyAccessType(authentication, AccessType.Guest);
            this.ValidateAsyncBeginInDataBase(authentication);
        }

        public bool VerifyAccess(Authentication authentication)
        {
            return this.authentications.Contains(authentication);
        }

        public void ValidateDirectoryInfo(string path)
        {
            new DirectoryInfo(path);
        }

        public void ValidateFileInfo(string path)
        {
            new FileInfo(path);
        }

        public Task<DataBaseMetaData> GetMetaDataAsync(Authentication authentication)
        {
            return this.Dispatcher.InvokeAsync(() =>
             {
                 if (authentication == null)
                     throw new ArgumentNullException(nameof(authentication));
                 this.ValidateDispatcher();
                 return this.metaData;
             });
        }

        public void Dispose()
        {
            if (this.IsLoaded == true)
            {
                this.WriteCache();
                this.Repository.Dispose();
                this.Dispatcher.Dispose();
            }
            base.DataBaseState = DataBaseState.None;
            this.tableContext = null;
            this.typeContext = null;
        }

        public void ResettingDataBase(Authentication authentication)
        {
            if (this.GetService(typeof(DomainContext)) is DomainContext domainContext)
            {
                this.Sign(authentication);
                if (this.IsLoaded == true)
                    this.DetachDomainHost();

                var domains = domainContext.Domains.Where<Domain>(item => item.DataBaseID == this.ID).ToArray();
                foreach (var item in domains)
                {
                    item.Dispatcher.Invoke(() => item.DeleteAsync(authentication, true));
                }

                this.typeContext?.Dispose();
                this.tableContext?.Dispose();
                base.ResettingDataBase(authentication);
                this.DataBases.InvokeItemsResettingEvent(authentication, new IDataBase[] { this });
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public async Task ResetDataBaseAsync(Authentication authentication, IEnumerable<TypeInfo> typeInfos, IEnumerable<TableInfo> tableInfos)
        {
            this.Sign(authentication);
            this.typeContext = new TypeContext(this, typeInfos);
            this.typeContext.ItemsLockChanged += TypeContext_ItemsLockChanged;
            this.typeContext.ItemsAccessChanged += TypeContext_ItemsAccessChanged;
            this.tableContext = new TableContext(this, tableInfos);
            this.tableContext.ItemsLockChanged += TableContext_ItemsLockChanged;
            this.tableContext.ItemsAccessChanged += TableContext_ItemsAccessChanged;
            base.ResetDataBase(authentication);
            base.UpdateLockParent();
            base.UpdateAccessParent();
            if (this.IsLoaded == true)
            {
                this.AttachDomainHost();
                foreach (var item in this.authentications)
                {
                    this.AttachUsers(authentication);
                }
            }
            var metaDataList = new List<DomainMetaData>();
            foreach (var item in this.CremaHost.DomainContext.Domains)
            {
                if (item.DomainInfo.DataBaseID == this.ID)
                {
                    var metaData = await item.GetMetaDataAsync(authentication);
                    metaDataList.Add(metaData);
                }
            }
            this.DataBases.InvokeItemsResetEvent(authentication, new IDataBase[] { this }, metaDataList.ToArray());
        }

        public IDomainHost FindDomainHost(Domain domain)
        {
            var domainInfo = domain.Dispatcher.Invoke(() => domain.DomainInfo);
            var itemPath = domainInfo.ItemPath;
            var itemType = domainInfo.ItemType;

            if (itemType == nameof(TableContent))
            {
                return new TableContent.TableContentDomainHost(this.tableContext.Tables, null, itemPath);
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

        public Task<CremaDataSet> GetDataSetAsync(Authentication authentication, DataSetType dataSetType, string filterExpression, string revision)
        {
            this.ValidateGetDataSet(authentication);
            this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSet), this, dataSetType, filterExpression, revision);
            switch (dataSetType)
            {
                case DataSetType.All:
                    return this.GetDataSetAsync(authentication, revision, filterExpression, ReadTypes.All);
                case DataSetType.OmitContent:
                    return this.GetDataSetAsync(authentication, revision, filterExpression, ReadTypes.OmitContent);
                case DataSetType.TypeOnly:
                    return this.GetDataSetAsync(authentication, revision, filterExpression, ReadTypes.TypeOnly);
                default:
                    throw new NotImplementedException();
            }
        }

        public async Task<CremaDataSet> GetDataSetAsync(Authentication authentication, string revision, string filterExpression, ReadTypes readType)
        {
            var tempPath = PathUtility.GetTempPath(false);
            try
            {
                var uri = await this.Repository.Dispatcher.InvokeAsync(() => this.Repository.GetUri(this.BasePath, revision));
                var exportPath = await this.Repository.Dispatcher.InvokeAsync(() => this.Repository.Export(uri, tempPath));
                return await Task.Run(() => CremaDataSet.ReadFromDirectory(exportPath, filterExpression, readType));
            }
            finally
            {
                DirectoryUtility.Delete(tempPath);
            }
        }

        public async Task<CremaDataSet> GetDataSet(Authentication authentication, IEnumerable<Table> tables, bool schemaOnly)
        {
            var props = await this.Dispatcher.InvokeAsync(() =>
            {
                var typePaths = tables.SelectMany(item => item.GetTypes())
                                  .Select(item => item.ItemPath)
                                  .Distinct()
                                  .ToArray();
                var tablePaths = tables.SelectMany(item => EnumerableUtility.Friends(item, item.DerivedTables))
                                       .Select(item => item.Parent ?? item)
                                       .Select(item => item.ItemPath)
                                       .Distinct()
                                       .ToArray();

                return new CremaDataSetSerializerSettings(authentication, typePaths, tablePaths)
                {
                    SchemaOnly = schemaOnly,
                };
            });

            return await this.Repository.Dispatcher.InvokeAsync(() => this.Serializer.Deserialize(this.BasePath, typeof(CremaDataSet), props) as CremaDataSet);
        }

        public string BasePath => this.CremaHost.GetPath(CremaPath.DataBases, $"{base.DataBaseInfo.ID}");

        public CremaHost CremaHost { get; }

        public DataBaseCollection DataBases => this.CremaHost.DataBases;

        public TableContext TableContext => this.tableContext;

        public TypeContext TypeContext => this.typeContext;

        public CremaDispatcher Dispatcher { get; private set; }

        public IObjectSerializer Serializer => this.serializer;

        public DataBaseRepositoryHost Repository { get; private set; }

        public new DataBaseInfo DataBaseInfo => base.DataBaseInfo;

        public new DataBaseState DataBaseState => base.DataBaseState;

        public AuthenticationInfo[] AuthenticationInfos => this.authenticationInfos;

        public override TypeCategoryBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext> TypeCategory
        {
            get { return this.typeContext?.Root; }
        }

        public override TableCategoryBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext> TableCategory
        {
            get { return this.tableContext?.Root; }
        }

        public new string Name => base.Name;

        public bool IsLoaded => this.DataBaseState.HasFlag(DataBaseState.IsLoaded);

        public new bool IsLocked => base.IsLocked;

        public new bool IsPrivate => base.IsPrivate;

        public new AccessInfo AccessInfo => base.AccessInfo;

        public new LockInfo LockInfo => base.LockInfo;

        public Guid ID => base.DataBaseInfo.ID;

        public Version Version => this.IsLoaded ? this.Repository.Version : null;

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateSetPrivate(IAuthentication authentication, object target)
        {
            base.OnValidateSetPrivate(authentication, target);

            if (target == this)
            {
                var userID = authentication.ID;
                var userInfo = this.userContext.Dispatcher.Invoke(() => this.userContext.Users[userID].UserInfo);
                if (userInfo.Authority == Authority.Guest)
                {
                    throw new PermissionException();
                }

                if (this.IsLoaded == false)
                    throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            }
        }

        public override void OnValidateSetPublic(IAuthentication authentication, object target)
        {
            base.OnValidateSetPublic(authentication, target);

            if (target == this)
            {
                var userID = authentication.ID;
                var userInfo = this.userContext.Dispatcher.Invoke(() => this.userContext.Users[userID].UserInfo);
                if (userInfo.Authority == Authority.Guest)
                {
                    throw new PermissionException();
                }

                if (this.IsLoaded == false)
                    throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateAddAccessMember(IAuthentication authentication, object target, string memberID, AccessType accessType)
        {
            base.OnValidateAddAccessMember(authentication, target, memberID, accessType);

            if (target == this)
            {
                var userInfo = this.userContext.Dispatcher.Invoke(() => this.userContext.Users[memberID].UserInfo);
                if (userInfo.Authority == Authority.Guest && accessType != AccessType.Guest)
                {
                    throw new PermissionException($"'{memberID}' 은(는) '{Authority.Guest}' 계정이기 때문에 '{accessType}' 권한을 설정할 수 없습니다.");
                }

                if (this.IsLoaded == false)
                    throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateSetAccessMember(IAuthentication authentication, object target, string memberID, AccessType accessType)
        {
            base.OnValidateSetAccessMember(authentication, target, memberID, accessType);

            if (target == this)
            {
                var userInfo = this.userContext.Dispatcher.Invoke(() => this.userContext.Users[memberID].UserInfo);
                if (accessType >= AccessType.Editor && userInfo.Authority == Authority.Guest)
                {
                    throw new PermissionException($"'{memberID}' 은(는) '{Authority.Guest}' 계정이기 때문에 '{accessType}' 권한을 설정할 수 없습니다.");
                }

                if (this.IsLoaded == false)
                    throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            }
        }

        public override void OnValidateRemoveAccessMember(IAuthentication authentication, object target)
        {
            base.OnValidateRemoveAccessMember(authentication, target);

            if (target == this)
            {
                if (this.IsLoaded == false)
                    throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateDelete(IAuthentication authentication, object target)
        {
            if (this.IsLoaded == true)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasBeenLoaded);

            base.OnValidateDelete(authentication, target);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            accessInfo.Path = this.Name;
            base.AccessInfo = accessInfo;
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
                    target.Restore(Authentication.System, item);
                    item.SetDomainHost(target);
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
            var domains = domainContext.Domains.Where<Domain>(item => item.DataBaseID == this.ID)
                                               .ToArray();

            foreach (var item in domains)
            {
                if (item.Host != null)
                {
                    item.Host.Detach();
                }
                item.SetDomainHost(null);
            }
        }

        private void SetDataBaseState(Authentication authentication, DataBaseState dataBaseState)
        {
            this.Dispatcher?.VerifyAccess();
            if (base.DataBaseState == dataBaseState)
                return;
            base.DataBaseState = dataBaseState;
            this.DataBases.InvokeItemsStateChangedEvent(authentication, new IDataBase[] { this });
        }

        private void AttachUsers(Authentication authentication)
        {
            var dataBaseID = this.ID;
            var domainContext = this.CremaHost.DomainContext;

            var query = from Domain item in domainContext.Domains
                        where item.DataBaseID == dataBaseID
                        select item;

            var domains = query.ToArray();
            foreach (var item in domains)
            {
                item.Dispatcher.Invoke(() =>
                {
                    if (item.Users.Contains(authentication.ID) == true)
                        item.Attach(authentication);
                });
            }
        }

        private void DetachUsers()
        {
            foreach (var item in this.authentications.ToArray())
            {
                this.DetachUsers(item);
                if ((Authentication)item is Authentication authentication)
                {
                    authentication.Expired -= Authentication_Expired;
                }
            }
            this.authentications.Clear();
            this.metaData.Authentications = this.authenticationInfos = new AuthenticationInfo[] { };
        }

        private void DetachUsers(Authentication authentication)
        {
            var dataBaseID = this.ID;
            var domainContext = this.CremaHost.DomainContext;

            var query = from Domain item in domainContext.Domains
                        where item.DataBaseID == dataBaseID
                        select item;

            var domains = query.ToArray();
            foreach (var item in domains)
            {
                item.Dispatcher.Invoke(() =>
                {
                    if (item.Users.Contains(authentication.ID) == true)
                    {
                        var user = item.Users[authentication.ID];
                        if (user.IsOnline == true)
                        {
                            item.Detach(authentication);
                        }
                    }
                });
            }
        }

        private void WriteCache()
        {
            var itemPath = FileUtility.Prepare(this.cachePath, $"{this.ID}");
            var dataInfo = new DataBaseDataSerializationInfo()
            {
                Revision = this.Repository.RepositoryInfo.Revision,
                TypeInfos = this.typeContext.Types.Select((Type item) => item.TypeInfo).ToArray(),
                TableInfos = this.tableContext.Tables.Select((Table item) => item.TableInfo).ToArray(),
            };
            this.Serializer.Serialize(itemPath, dataInfo, DataBaseDataSerializationInfo.Settings);
        }

        private void DeleteCache()
        {
            var itemPath = FileUtility.Prepare(this.cachePath, $"{this.ID}");
            var itemPaths = this.Serializer.GetPath(itemPath, typeof(DataBaseDataSerializationInfo), DataBaseDataSerializationInfo.Settings);
            FileUtility.Delete(itemPaths);
        }

        private async Task ReadCacheAsync()
        {
            if (this.CremaHost.NoCache == false)
            {
                try
                {
                    var itemPath = FileUtility.Prepare(this.cachePath, $"{this.ID}");
                    if (this.Serializer.Exists(itemPath, typeof(DataBaseDataSerializationInfo), DataBaseDataSerializationInfo.Settings) == true)
                    {
                        var dataInfo = (DataBaseDataSerializationInfo)this.Serializer.Deserialize(itemPath, typeof(DataBaseDataSerializationInfo), DataBaseDataSerializationInfo.Settings);
                        if (this.Repository.RepositoryInfo.Revision == dataInfo.Revision)
                        {
                            await this.ResetDataBaseAsync(Authentication.System, dataInfo.TypeInfos, dataInfo.TableInfos);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                    this.CremaHost.Error($"'{this.Name}' cache is crashed.");
                }
            }

            {
                this.CremaHost.Debug($"begin read database : '{this.Name}'");
                this.dataSet = CremaDataSet.ReadFromDirectory(this.BasePath);
                this.CremaHost.Debug($"end read database : '{this.Name}'");

                var typeInfos = dataSet.Types.Select(item => item.TypeInfo);
                var tableInfos = dataSet.Tables.Select(item => item.TableInfo);
                await this.ResetDataBaseAsync(Authentication.System, typeInfos, tableInfos);
            }
        }

        private void Initialize()
        {
            var remotePath = this.CremaHost.GetPath(CremaPath.RepositoryDataBases);
            var revision = this.repositoryProvider.GetRevision(remotePath, base.Name);

            if (base.DataBaseInfo.Revision != revision)
            {
                this.CremaHost.Debug($"initialize database : {base.Name}");
                var repositoryInfo = this.repositoryProvider.GetRepositoryInfo(remotePath, base.Name);
                var itemList = this.repositoryProvider.GetRepositoryItemList(remotePath, this.Name);
                var categories = from item in itemList
                                 where item.EndsWith(PathUtility.Separator) == true
                                 select item;
                var items = from item in itemList
                            where item.EndsWith(PathUtility.Separator) == false
                            where item.StartsWith(PathUtility.Separator + CremaSchema.TypeDirectory + PathUtility.Separator)
                                  || item.StartsWith(PathUtility.Separator + CremaSchema.TableDirectory + PathUtility.Separator)
                            select FileUtility.RemoveExtension(item);

                var allItems = items.Distinct()
                                    .Concat(categories)
                                    .OrderBy(item => item)
                                    .ToArray();

                base.DataBaseInfo = new DataBaseInfo()
                {
                    ID = repositoryInfo.ID,
                    Name = repositoryInfo.Name,
                    Revision = repositoryInfo.Revision,
                    Comment = repositoryInfo.Comment,
                    Paths = CategoryName.MakeItemList(allItems),
                    CreationInfo = repositoryInfo.CreationInfo,
                    ModificationInfo = repositoryInfo.ModificationInfo,
                };
            }

            this.ReadAccessInfo();
        }

        private void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            this.CremaHost.Dispatcher.InvokeAsync(() =>
            {
                if (e.UserID == this.LockInfo.UserID)
                {
                    this.Unlock(Authentication.System);
                }
            });
        }

        private void Repository_Changed(object sender, EventArgs e)
        {
            this.Sign(Authentication.System);
            var dataBaseInfo = new DataBaseInfo(base.DataBaseInfo)
            {
                Revision = this.Repository.RepositoryInfo.Revision,
                ModificationInfo = this.Repository.RepositoryInfo.ModificationInfo,
                Paths = this.GetItemPaths().ToArray(),
                TypesHashValue = CremaDataSet.GenerateHashValue((from Type item in this.TypeContext.Types select item.TypeInfo).ToArray()),
                TablesHashValue = CremaDataSet.GenerateHashValue((from Table item in this.TableContext.Tables select item.TableInfo).ToArray()),
            };
            this.dataSet = null;
            this.metaData.TypeCategories = this.typeContext.GetCategoryMetaDatas();
            this.metaData.Types = this.typeContext.GetTypeMetaDatas();
            this.metaData.TableCategories = this.tableContext.GetCategoryMetaDatas();
            this.metaData.Tables = this.tableContext.GetTableMetaDatas();
            base.DataBaseInfo = this.metaData.DataBaseInfo = dataBaseInfo;
            this.DataBases.InvokeItemsChangedEvent(Authentication.System, new IDataBase[] { this });
        }

        private void Authentication_Expired(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                var authentication = sender as Authentication;
                if (this.IsLoaded == true && this.VerifyAccess(authentication) == true)
                {
                    this.OnLeave(authentication);
                    this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
                }
            });
        }

        private Task<SignatureDate> InvokeDataBaseSetPublicAsync(Authentication authentication)
        {
            var message = EventMessageBuilder.SetPublicDataBase(authentication, this.Name);
            var itemPath = this.BasePath + Path.DirectorySeparatorChar;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    var itemPaths = this.Serializer.GetPath(itemPath, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
                    this.Repository.DeleteRange(itemPaths);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private Task<SignatureDate> InvokeDataBaseSetPrivateAsync(Authentication authentication)
        {
            var accessInfo = this.AccessInfo;
            var message = EventMessageBuilder.SetPrivateDataBase(authentication, this.Name);
            var itemPath = this.BasePath + Path.DirectorySeparatorChar;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    accessInfo.SetPrivate(this.GetType().Name, signatureDate);
                    var itemPaths = this.Serializer.Serialize(itemPath, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
                    this.Repository.AddRange(itemPaths);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private Task<SignatureDate> InvokeDataBaseAddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            var accessInfo = this.AccessInfo;
            var message = EventMessageBuilder.AddAccessMemberToDataBase(authentication, this.Name, memberID, accessType);
            var itemPath = this.BasePath + Path.DirectorySeparatorChar;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    accessInfo.Add(signatureDate, memberID, accessType);
                    this.Serializer.Serialize(itemPath, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private Task<SignatureDate> InvokeDataBaseSetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            var accessInfo = this.AccessInfo;
            var message = EventMessageBuilder.SetAccessMemberOfDataBase(authentication, this.Name, memberID, accessType);
            var itemPath = this.BasePath + Path.DirectorySeparatorChar;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    accessInfo.Set(signatureDate, memberID, accessType);
                    this.Serializer.Serialize(itemPath, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private Task<SignatureDate> InvokeDataBaseRemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            var accessInfo = this.AccessInfo;
            var message = EventMessageBuilder.RemoveAccessMemberFromDataBase(authentication, this.Name, memberID);
            var itemPath = this.BasePath + Path.DirectorySeparatorChar;
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var signatureDate = authentication.Sign();
                    accessInfo.Remove(signatureDate, memberID);
                    this.Serializer.Serialize(itemPath, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
                    this.Repository.Commit(authentication, message);
                    return signatureDate;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private void ValidateDispatcher()
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            //this.Dispatcher.VerifyAccess();
        }

        private void ValidateEnter(Authentication authentication)
        {
            if (this.IsLoaded == false)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            if (this.authentications.Contains(authentication) == true)
                throw new ArgumentException(Resources.Exception_AlreadyInDataBase, nameof(authentication));
            if (this.VerifyAccessType(authentication, AccessType.Guest) == false)
                throw new PermissionDeniedException();
        }

        private void ValidateLeave(Authentication authentication)
        {
            if (this.IsLoaded == false)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
        }

        private void ValidateLoad(Authentication authentication)
        {
            if (this.IsLoaded == true)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasBeenLoaded);
            if (authentication.IsSystem == false && authentication.IsAdmin == false)
                throw new PermissionDeniedException();
            if (this.VerifyAccessType(authentication, AccessType.Master) == false)
                throw new PermissionDeniedException();
        }

        private void ValidateUnload(Authentication authentication)
        {
            if (this.IsLoaded == false)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            if (authentication.IsSystem == false && authentication.IsAdmin == false)
                throw new PermissionDeniedException();
            if (this.VerifyAccessType(authentication, AccessType.Master) == false)
                throw new PermissionDeniedException();
        }

        private void ValidateBeginTransaction(Authentication authentication)
        {
            if (this.IsLoaded == false)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasNotBeenLoaded);
            if (authentication.IsSystem == false && authentication.IsAdmin == false)
                throw new PermissionDeniedException();
            if (this.VerifyAccessType(authentication, AccessType.Owner) == false)
                throw new PermissionDeniedException();
        }

        private void OnEnter(Authentication authentication)
        {
            this.Sign(authentication);
            this.authentications.Add(authentication);
            this.authenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
            authentication.Expired += Authentication_Expired;
            this.SetDataBaseState(authentication, this.DataBaseState | DataBaseState.HasWorker);
            this.AttachUsers(authentication);
            this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
        }

        private void OnLeave(Authentication authentication)
        {
            this.Sign(authentication);
            this.authentications.Remove(authentication);
            this.authenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
            authentication.Expired -= Authentication_Expired;
            this.DetachUsers(authentication);
            if (this.authentications.Any() == false)
            {
                this.SetDataBaseState(authentication, this.DataBaseState & ~DataBaseState.HasWorker);
            }
            this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
        }

        private void ReadAccessInfo()
        {
            var itemPath = this.BasePath + Path.DirectorySeparatorChar;
            try
            {
                if (this.serializer.Exists(itemPath, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings) == true)
                {
                    var accessInfo = (AccessSerializationInfo)this.serializer.Deserialize(itemPath, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
                    this.SetAccessInfo((AccessInfo)accessInfo);
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        private void Sign(Authentication authentication)
        {
            authentication.Sign();
        }

        private void Sign(Authentication authentication, SignatureDate signatureDate)
        {
            authentication.Sign(signatureDate.DateTime);
        }

        private void TypeContext_ItemsLockChanged(object sender, ItemsEventArgs<ITypeItem> e)
        {
            this.metaData.TypeCategories = this.typeContext.GetCategoryMetaDatas();
            this.metaData.Types = this.typeContext.GetTypeMetaDatas();
        }

        private void TypeContext_ItemsAccessChanged(object sender, ItemsEventArgs<ITypeItem> e)
        {
            this.metaData.TypeCategories = this.typeContext.GetCategoryMetaDatas();
            this.metaData.Types = this.typeContext.GetTypeMetaDatas();
        }

        private void TableContext_ItemsLockChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            this.metaData.TableCategories = this.tableContext.GetCategoryMetaDatas();
            this.metaData.Tables = this.tableContext.GetTableMetaDatas();
        }

        private void TableContext_ItemsAccessChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            this.metaData.TableCategories = this.tableContext.GetCategoryMetaDatas();
            this.metaData.Tables = this.tableContext.GetTableMetaDatas();
        }

        private IEnumerable<string> GetItemPaths()
        {
            yield return PathUtility.Separator;
            yield return PathUtility.Separator + CremaSchema.TypeDirectory + PathUtility.Separator;
            foreach (var item in this.typeContext)
            {
                yield return PathUtility.Separator + CremaSchema.TypeDirectory + item.Path;
            }
            yield return PathUtility.Separator + CremaSchema.TableDirectory + PathUtility.Separator;
            foreach (var item in this.tableContext)
            {
                yield return PathUtility.Separator + CremaSchema.TableDirectory + item.Path;
            }
        }

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

            if (base.DataBaseState.HasFlag(DataBaseState.IsLoaded) == true)
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

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => this.DataBaseInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.DataBaseState;

        #endregion
    }
}

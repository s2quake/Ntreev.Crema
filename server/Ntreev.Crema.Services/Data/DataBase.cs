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
        private DataBaseMetaData metaData = DataBaseMetaData.Empty;
        private AuthenticationInfo[] authenticationInfos;

        private EventHandler<AuthenticationEventArgs> authenticationEntered;
        private EventHandler<AuthenticationEventArgs> authenticationLeft;

        private HashSet<AuthenticationToken> authentications = new HashSet<AuthenticationToken>();

        public DataBase(DataBaseCollection dataBases, string name)
        {
            this.DataBases = dataBases;
            this.Dispatcher = dataBases.Dispatcher;
            this.repositoryProvider = this.CremaHost.RepositoryProvider;
            this.serializer = this.CremaHost.Serializer;
            base.Name = name;
            this.cachePath = this.CremaHost.GetPath(CremaPath.Caches, DataBaseCollection.DataBasesString);
            this.userContext = this.CremaHost.UserContext;
            this.userContext.Dispatcher.Invoke(() => this.userContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
            this.Initialize();
        }

        public DataBase(DataBaseCollection dataBases, string name, DataBaseSerializationInfo dataBaseInfo)
            : this(dataBases, name)
        {
            this.DataBases = dataBases;
            this.Dispatcher = dataBases.Dispatcher;
            this.repositoryProvider = this.CremaHost.RepositoryProvider;
            this.serializer = this.CremaHost.Serializer;
            base.Name = name;
            this.cachePath = this.CremaHost.GetPath(CremaPath.Caches, DataBaseCollection.DataBasesString);
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
            this.ValidateExpired();
            return base.GetAccessType(authentication);
        }

        public async Task SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                });
                var signatureDate = await this.InvokeDataBaseSetPublicAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, signatureDate);
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    base.ValidateSetPrivate(authentication);
                });
                var accessInfo = await this.InvokeDataBaseSetPrivateAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                });
                var accessInfo = await this.InvokeDataBaseAddAccessMemberAsync(authentication, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                });
                var accessInfo = await this.InvokeDataBaseSetAccessMemberAsync(authentication, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    base.ValidateRemoveAccessMember(authentication, memberID);
                });
                var accessInfo = await this.InvokeDataBaseRemoveAccessMemberAsync(authentication, memberID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this, comment);
                    base.ValidateLock(authentication);
                    base.Lock(authentication, comment);
                    this.CremaHost.Sign(authentication);
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    base.ValidateUnlock(authentication);
                    base.Unlock(authentication);
                    this.CremaHost.Sign(authentication);
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
                var repositorySetting = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LoadAsync), this);
                    this.ValidateLoad(authentication);
                    base.DataBaseState = DataBaseState.Loading;
                    return new RepositorySettings()
                    {
                        BasePath = this.DataBases.RemotePath,
                        RepositoryName = this.Name,
                        WorkingPath = this.BasePath,
                        TransactionPath = this.CremaHost.GetPath(CremaPath.Transactions, $"{this.ID}"),
                        LogService = this.CremaHost,
                    };
                });
                var repository = await Task.Run(() => this.repositoryProvider.CreateInstance(repositorySetting));
                this.Dispatcher = new CremaDispatcher(this);
                this.Repository = new DataBaseRepositoryHost(this, repository);
                //this.Repository.Changed += Repository_Changed;
                var cache = await this.ReadCacheAsync(repository.RepositoryInfo);
                await this.ResetDataBaseAsync(authentication, cache.Item1, cache.Item2);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.DataBaseState = DataBaseState.Loaded;
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnloadAsync), this);
                    this.ValidateUnload(authentication);
                    base.DataBaseState = DataBaseState.Unloading;
                });
                await this.DetachDomainHostAsync();
                await this.WriteCacheAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.tableContext.Dispose();
                    this.tableContext = null;
                    this.typeContext.Dispose();
                    this.typeContext = null;
                    this.ClearAuthentications();
                    //this.Repository.Changed -= Repository_Changed;
                    this.Repository.Dispose();
                    this.Repository = null;
                    this.Dispatcher.Dispose(false);
                    this.Dispatcher = this.CremaHost.Dispatcher;
                    base.DataBaseState = DataBaseState.Unloaded;
                    base.Unload(authentication);
                    this.CremaHost.Sign(authentication);
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateEnter(authentication);
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterAsync), this);
                });
                await this.AttachUsersAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.authentications.Add(authentication);
                    this.authenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
                    authentication.Expired += Authentication_Expired;
                    this.CremaHost.Sign(authentication);
                    this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
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
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateLeave(authentication);
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveAsync), this);
                });
                await this.DetachUsersAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.authentications.Remove(authentication);
                    this.authenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
                    authentication.Expired -= Authentication_Expired;
                    this.CremaHost.Sign(authentication);
                    this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
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
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    base.ValidateRename(authentication, name);
                    return (this.DataBaseInfo, base.Name);
                });
                var result = await this.DataBases.InvokeDataBaseRenameAsync(authentication, tuple.DataBaseInfo, name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Name = name;
                    this.metaData.DataBaseInfo = base.DataBaseInfo;
                    this.DataBases.InvokeItemsRenamedEvent(authentication, new DataBase[] { this }, new string[] { tuple.Name });
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
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    base.ValidateDelete(authentication);
                    return (this.DataBaseInfo, base.Name);
                });
                var result = await this.DataBases.InvokeDataBaseDeleteAsync(authentication, tuple.DataBaseInfo);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.DataBaseState = DataBaseState.None;
                    this.DeleteCache();
                    this.DataBases.InvokeItemsDeletedEvent(authentication, new DataBase[] { this }, new string[] { tuple.Name });
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
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RevertAsync), this, revision);
                    return base.Name;
                });
                var result = await this.DataBases.InvokeDataBaseRevertAsync(authentication, name, revision);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.DataBaseInfo = new DataBaseInfo()
                    {
                        ID = result.ID,
                        Name = result.Name,
                        Revision = result.Revision,
                        Comment = result.Comment,
                        CreationInfo = result.CreationInfo,
                        ModificationInfo = result.ModificationInfo,
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
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateBeginTransaction(authentication);
                    this.CremaHost.Sign(authentication);
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
            this.ValidateExpired();
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (authentication == null)
                    throw new ArgumentNullException(nameof(authentication));
                return this.metaData;
            });
        }

        public async Task DisposeAsync()
        {
            if (this.IsLoaded == true)
            {
                await this.WriteCacheAsync();
                this.Repository.Dispose();
                this.Dispatcher.Dispose();
            }
            base.DataBaseState = DataBaseState.None;
            this.tableContext = null;
            this.typeContext = null;
        }

        public async Task ResettingDataBaseAsync(Authentication authentication)
        {
            var isLoaded = await this.Dispatcher.InvokeAsync(() => this.IsLoaded);
            if (isLoaded == true)
                await this.DetachDomainHostAsync();

            var domains = await this.DomainContext.GetDomainsAsync(this.ID);
            foreach (var item in domains)
            {
                await item.DeleteAsync(authentication, true);
            }
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.typeContext?.Dispose();
                this.tableContext?.Dispose();
                base.ResettingDataBase(authentication);
                this.DataBases.InvokeItemsResettingEvent(authentication, new IDataBase[] { this });
            });
        }

        public async Task ResetDataBaseAsync(Authentication authentication, TypeInfo[] typeInfos, TableInfo[] tableInfos)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.Sign(authentication);
                this.typeContext = new TypeContext(this, typeInfos);
                this.typeContext.ItemsCreated += TypeContext_ItemsCreated;
                this.typeContext.ItemsRenamed += TypeContext_ItemsRenamed;
                this.typeContext.ItemsMoved += TypeContext_ItemsMoved;
                this.typeContext.ItemsDeleted += TypeContext_ItemsDeleted;
                this.typeContext.ItemsChanged += TypeContext_ItemsChanged;
                this.typeContext.ItemsLockChanged += TypeContext_ItemsLockChanged;
                this.typeContext.ItemsAccessChanged += TypeContext_ItemsAccessChanged;
                this.tableContext = new TableContext(this, tableInfos);
                this.tableContext.ItemsCreated += TableContext_ItemsCreated;
                this.tableContext.ItemsRenamed += TableContext_ItemsRenamed;
                this.tableContext.ItemsMoved += TableContext_ItemsMoved;
                this.tableContext.ItemsDeleted += TableContext_ItemsDeleted;
                this.tableContext.ItemsChanged += TableContext_ItemsChanged;
                this.tableContext.ItemsLockChanged += TableContext_ItemsLockChanged;
                this.tableContext.ItemsAccessChanged += TableContext_ItemsAccessChanged;
                this.metaData.TypeCategories = this.typeContext.GetCategoryMetaDatas();
                this.metaData.Types = this.typeContext.GetTypeMetaDatas();
                this.metaData.TableCategories = this.tableContext.GetCategoryMetaDatas();
                this.metaData.Tables = this.tableContext.GetTableMetaDatas();
                base.ResetDataBase(authentication);
                base.UpdateLockParent();
                base.UpdateAccessParent();
            });

            var domains = await this.DomainContext.GetDomainsAsync(this.ID);
            var metaDataList = new List<DomainMetaData>(domains.Length);
            var authentications = this.authentications.Select(item => (Authentication)item).ToArray();
            foreach (var item in domains)
            {
                try
                {
                    var target = await this.FindDomainHostAsync(item);
                    await target.RestoreAsync(Authentication.System, item);
                    await item.SetDomainHostAsync(target);
                    await item.AttachAsync(authentications);
                    var metaData = await item.GetMetaDataAsync(authentication);
                    metaDataList.Add(metaData);
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                }
            }

            await this.Dispatcher.InvokeAsync(() =>
            {
                this.DataBases.InvokeItemsResetEvent(authentication, new IDataBase[] { this }, metaDataList.ToArray());
            });
        }

        private void TableContext_ItemsCreated(object sender, ItemsCreatedEventArgs<ITableItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TableContext_ItemsRenamed(object sender, ItemsRenamedEventArgs<ITableItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TableContext_ItemsMoved(object sender, ItemsMovedEventArgs<ITableItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TableContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<ITableItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TableContext_ItemsChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TypeContext_ItemsCreated(object sender, ItemsCreatedEventArgs<ITypeItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TypeContext_ItemsRenamed(object sender, ItemsRenamedEventArgs<ITypeItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TypeContext_ItemsMoved(object sender, ItemsMovedEventArgs<ITypeItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TypeContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<ITypeItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        private void TypeContext_ItemsChanged(object sender, ItemsEventArgs<ITypeItem> e)
        {
            this.RefreshDataBaseInfo();
        }

        public Task<IDomainHost> FindDomainHostAsync(Domain domain)
        {
            var domainInfo = domain.DomainInfo;
            var itemPath = domainInfo.ItemPath;
            var itemType = domainInfo.ItemType;

            return this.Dispatcher.InvokeAsync<IDomainHost>(() =>
            {
                if (itemType == nameof(TableContent))
                {
                    return new TableContent.TableContentDomainHost(this.tableContext.Tables, itemPath);
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
            });
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

        public CremaHost CremaHost => this.DataBases.CremaHost;

        public DataBaseCollection DataBases { get; }

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

        public bool IsLoaded => this.DataBaseState.HasFlag(DataBaseState.Loaded);

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

        [EditorBrowsable(EditorBrowsableState.Never)]
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
        public override void OnValidateRename(IAuthentication authentication, object target, string oldName, string newName)
        {
            base.OnValidateRename(authentication, target, oldName, newName);
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsLoaded == true)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasBeenLoaded);
            var dataBasePath = Path.Combine(Path.GetDirectoryName(this.BasePath), newName);
            if (DirectoryUtility.Exists(dataBasePath) == true)
                throw new ArgumentException(string.Format(Resources.Exception_ExistsPath_Format, newName), nameof(newName));
            if (this.DataBases.ContainsKey(newName) == true)
                throw new ArgumentException(string.Format(Resources.Exception_DataBaseIsAlreadyExisted_Format, newName), nameof(newName));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateDelete(IAuthentication authentication, object target)
        {
            base.OnValidateDelete(authentication, target);
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsLoaded == true)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasBeenLoaded);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            accessInfo.Path = this.Name;
            base.AccessInfo = accessInfo;
        }

        protected override void OnDataBaseStateChanged(EventArgs e)
        {
            this.metaData.DataBaseState = base.DataBaseState;
            base.OnDataBaseStateChanged(e);
        }

        private async Task AttachDomainHostAsync()
        {
            var domains = await this.DomainContext.GetDomainsAsync(this.ID);
            foreach (var item in domains)
            {
                try
                {
                    var target = await this.FindDomainHostAsync(item);
                    await target.RestoreAsync(Authentication.System, item);
                    await item.SetDomainHostAsync(target);
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                }
            }
        }

        private async Task DetachDomainHostAsync()
        {
            var authentications = this.authentications.Select(item => (Authentication)item).ToArray();
            var domains = await this.DomainContext.GetDomainsAsync(this.ID);
            foreach (var item in domains)
            {
                await item.DetachAsync(authentications);
                await item.Host.DetachAsync();
                await item.SetDomainHostAsync(null);
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

        private async Task AttachUsersAsync(Authentication authentication)
        {
            var domains = await this.DomainContext.GetDomainsAsync(this.ID);
            foreach (var item in domains)
            {
                await item.AttachAsync(authentication);
            }
        }

        private void ClearAuthentications()
        {
            foreach (var item in this.authentications.ToArray())
            {
                if ((Authentication)item is Authentication authentication)
                {
                    authentication.Expired -= Authentication_Expired;
                }
            }
            this.authentications.Clear();
            this.metaData.Authentications = this.authenticationInfos = new AuthenticationInfo[] { };
        }

        private Task DetachUsersAsync(Authentication authentication)
        {
            return this.DomainContext.DetachUsersAsync(authentication, this.ID);
        }

        private async Task WriteCacheAsync()
        {
            var itemPath = FileUtility.Prepare(this.cachePath, $"{this.ID}");
            var repositoryInfo = await this.Repository.Dispatcher.InvokeAsync(() => this.Repository.RepositoryInfo);
            var dataInfo = await this.Dispatcher.InvokeAsync(() =>
            {
                return new DataBaseDataSerializationInfo()
                {
                    Revision = this.Repository.RepositoryInfo.Revision,
                    TypeInfos = this.typeContext.Types.Select((Type item) => item.TypeInfo).ToArray(),
                    TableInfos = this.tableContext.Tables.Select((Table item) => item.TableInfo).ToArray(),
                };
            });
            await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Serializer.Serialize(itemPath, dataInfo, DataBaseDataSerializationInfo.Settings);
            });
        }

        private void DeleteCache()
        {
            var itemPath = FileUtility.Prepare(this.cachePath, $"{this.ID}");
            var itemPaths = this.Serializer.GetPath(itemPath, typeof(DataBaseDataSerializationInfo), DataBaseDataSerializationInfo.Settings);
            FileUtility.Delete(itemPaths);
        }

        private Task<(TypeInfo[], TableInfo[])> ReadCacheAsync(RepositoryInfo repositoryInfo)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.CremaHost.NoCache == false)
                {
                    try
                    {
                        var itemPath = FileUtility.Prepare(this.cachePath, $"{this.ID}");
                        if (this.Serializer.Exists(itemPath, typeof(DataBaseDataSerializationInfo), DataBaseDataSerializationInfo.Settings) == true)
                        {
                            var dataInfo = (DataBaseDataSerializationInfo)this.Serializer.Deserialize(itemPath, typeof(DataBaseDataSerializationInfo), DataBaseDataSerializationInfo.Settings);
                            if (repositoryInfo.Revision == dataInfo.Revision)
                            {
                                return (dataInfo.TypeInfos, dataInfo.TableInfos);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.CremaHost.Error(e);
                        this.CremaHost.Error($"'{this.Name}' cache is crashed.");
                    }
                }

                this.CremaHost.Debug($"begin read database : '{this.Name}'");
                this.dataSet = CremaDataSet.ReadFromDirectory(this.BasePath);
                this.CremaHost.Debug($"end read database : '{this.Name}'");

                var typeInfos = dataSet.Types.Select(item => item.TypeInfo).ToArray();
                var tableInfos = dataSet.Tables.Select(item => item.TableInfo).ToArray();
                return (typeInfos, tableInfos);
            });
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

            this.metaData.DataBaseInfo = base.DataBaseInfo;
            this.metaData.DataBaseState = base.DataBaseState;
            this.metaData.AccessInfo = base.AccessInfo;
            this.metaData.LockInfo = base.LockInfo;

            this.ReadAccessInfo();
        }

        private async void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            await this.CremaHost.Dispatcher.InvokeAsync(() =>
            {
                if (e.UserID == this.LockInfo.UserID)
                {
                    this.Unlock(Authentication.System);
                }
            });
        }

        //private async void Repository_Changed(object sender, EventArgs e)
        //{

        //}

        private async void RefreshDataBaseInfo()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.Sign(Authentication.System);
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
            });
        }

        private async void Authentication_Expired(object sender, EventArgs e)
        {
            if (sender is Authentication authentication)
            {
                var value = await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.IsLoaded == true && this.VerifyAccess(authentication) == true)
                    {
                        this.CremaHost.Sign(authentication);
                        this.authentications.Remove(authentication);
                        this.authenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
                        authentication.Expired -= Authentication_Expired;
                        return true;
                    }
                    return false;
                });
                if (value == true)
                {
                    await this.DetachUsersAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                        this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
                    });
                }
            }
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

        private Task<AccessInfo> InvokeDataBaseSetPrivateAsync(Authentication authentication)
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
                    return accessInfo;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private Task<AccessInfo> InvokeDataBaseAddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
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
                    return accessInfo;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private Task<AccessInfo> InvokeDataBaseSetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
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
                    return accessInfo;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        private Task<AccessInfo> InvokeDataBaseRemoveAccessMemberAsync(Authentication authentication, string memberID)
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
                    return accessInfo;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        //private void ValidateDispatcher()
        //{
        //    if (this.Dispatcher == null)
        //        throw new InvalidOperationException(Resources.Exception_InvalidObject);
        //    //this.Dispatcher.VerifyAccess();
        //}

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

        private void ValidateRevert(Authentication authentication, string revision)
        {
            if (authentication.IsSystem == false && authentication.IsAdmin == false)
                throw new PermissionDeniedException();
            if (this.IsLoaded == true)
                throw new InvalidOperationException(Resources.Exception_LoadedDataBaseCannotRevert);
        }

        //private async Task OnEnterAsync(Authentication authentication)
        //{
        //    this.CremaHost.Sign(authentication);
        //    this.authentications.Add(authentication);
        //    this.authenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
        //    authentication.Expired += Authentication_Expired;
        //    await this.AttachUsersAsync(authentication);
        //    this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
        //}

        //private async Task OnLeaveAsync(Authentication authentication)
        //{
        //    this.CremaHost.Sign(authentication);
        //    this.authentications.Remove(authentication);
        //    this.authenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
        //    authentication.Expired -= Authentication_Expired;
        //    await this.DetachUsersAsync(authentication);
        //    this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
        //}

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

        private DomainContext DomainContext => this.CremaHost.DomainContext;

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

            if (base.DataBaseState.HasFlag(DataBaseState.Loaded) == true)
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

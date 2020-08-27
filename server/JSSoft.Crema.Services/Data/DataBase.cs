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

using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data.Serializations;
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users;
using JSSoft.Library.IO;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    partial class DataBase : DataBaseBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext, Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        IDataBase, IInfoProvider, IStateProvider
    {
        public static readonly string TypePathPrefix = PathUtility.Separator + CremaSchema.TypeDirectory;
        public static readonly string TablePathPrefix = PathUtility.Separator + CremaSchema.TableDirectory;

        private readonly IRepositoryProvider repositoryProvider;
        private readonly string cachePath;
        private CremaDataSet dataSet;
        private DataBaseMetaData metaData = DataBaseMetaData.Empty;
        private EventHandler<AuthenticationEventArgs> authenticationEntered;
        private EventHandler<AuthenticationEventArgs> authenticationLeft;
        private TaskCompletedEventHandler taskCompleted;

        private readonly HashSet<AuthenticationToken> authentications = new HashSet<AuthenticationToken>();

        public DataBase(DataBaseContext dataBaseContext, string name)
        {
            this.DataBaseContext = dataBaseContext;
            this.Dispatcher = dataBaseContext.Dispatcher;
            this.repositoryProvider = this.CremaHost.RepositoryProvider;
            this.Serializer = this.CremaHost.Serializer;
            base.Name = name;
            this.cachePath = this.CremaHost.GetPath(CremaPath.Caches, DataBaseContext.DataBasesString);
            this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
            this.Initialize();
        }

        public DataBase(DataBaseContext dataBaseContext, string name, DataBaseSerializationInfo dataBaseInfo)
        {
            this.DataBaseContext = dataBaseContext;
            this.Dispatcher = dataBaseContext.Dispatcher;
            this.repositoryProvider = this.CremaHost.RepositoryProvider;
            this.Serializer = this.CremaHost.Serializer;
            base.Name = name;
            this.cachePath = this.CremaHost.GetPath(CremaPath.Caches, DataBaseContext.DataBasesString);
            this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
            base.DataBaseInfo = (DataBaseInfo)dataBaseInfo;
            this.Initialize();
        }

        public override string ToString()
        {
            return base.Name;
        }

        public void InvokeTaskCompletedEvent(Authentication authentication, Guid taskID)
        {
            this.OnTaskCompleted(new TaskCompletedEventArgs(authentication, taskID));
        }

        public AccessType GetAccessType(Authentication authentication)
        {
            this.ValidateExpired();
            return base.GetAccessType(authentication);
        }

        public async Task<Guid> SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                    var name = base.Name;
                    var accessInfo = base.AccessInfo;
                    return (name, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeDataBaseSetPublicAsync(authentication, tuple.accessInfo);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBaseContext.InvokeItemsSetPublicEvent(authentication, this.BasePath, new IDataBase[] { this });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetPrivateAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    base.ValidateSetPrivate(authentication);
                    var dataBaseInfo = base.DataBaseInfo;
                    var accessInfo = base.AccessInfo;
                    return (dataBaseInfo, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeDataBaseSetPrivateAsync(authentication, tuple.dataBaseInfo, tuple.accessInfo);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBaseContext.InvokeItemsSetPrivateEvent(authentication, this.BasePath, new IDataBase[] { this });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                    var dataBaseInfo = base.DataBaseInfo;
                    var accessInfo = base.AccessInfo;
                    return (dataBaseInfo, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeDataBaseAddAccessMemberAsync(authentication, tuple.dataBaseInfo, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBaseContext.InvokeItemsAddAccessMemberEvent(authentication, this.BasePath, new IDataBase[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                    var dataBaseInfo = base.DataBaseInfo;
                    var accessInfo = base.AccessInfo;
                    return (dataBaseInfo, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeDataBaseSetAccessMemberAsync(authentication, tuple.dataBaseInfo, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBaseContext.InvokeItemsSetAccessMemberEvent(authentication, this.BasePath, new IDataBase[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    base.ValidateRemoveAccessMember(authentication, memberID);
                    var dataBaseInfo = base.DataBaseInfo;
                    var accessInfo = base.AccessInfo;
                    return (dataBaseInfo, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.InvokeDataBaseRemoveAccessMemberAsync(authentication, tuple.dataBaseInfo, tuple.accessInfo, memberID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBaseContext.InvokeItemsRemoveAccessMemberEvent(authentication, this.BasePath, new IDataBase[] { this }, new string[] { memberID });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LockAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this, comment);
                    base.ValidateLock(authentication);
                    var taskID = Guid.NewGuid();
                    base.Lock(authentication, comment);
                    this.CremaHost.Sign(authentication);
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBaseContext.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { comment });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> UnlockAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    base.ValidateUnlock(authentication);
                    var taskID = Guid.NewGuid();
                    base.Unlock(authentication);
                    this.CremaHost.Sign(authentication);
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBaseContext.InvokeItemsUnlockedEvent(authentication, new IDataBase[] { this });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LoadAsync(Authentication authentication)
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
                        BasePath = this.DataBaseContext.RemotePath,
                        RepositoryName = this.Name,
                        WorkingPath = this.BasePath,
                        TransactionPath = this.CremaHost.GetPath(CremaPath.Transactions, $"{this.ID}"),
                        LogService = this.CremaHost,
                    };
                });
                var taskID = Guid.NewGuid();
                var repository = await Task.Run(() => this.repositoryProvider.CreateInstance(repositorySetting));
                this.Dispatcher = new CremaDispatcher(this);
                this.Repository = new DataBaseRepositoryHost(this, repository);
                var cache = await this.ReadCacheAsync(repository.RepositoryInfo);

                await this.ResetDataBaseAsync(authentication, cache.Item1, cache.Item2);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.DataBaseState = DataBaseState.Loaded;
                    base.Load(authentication);
                    base.UpdateLockParent();
                    base.UpdateAccessParent();
                    this.CremaHost.Sign(authentication);
                    this.DataBaseContext.InvokeItemsLoadedEvent(authentication, new IDataBase[] { this });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> UnloadAsync(Authentication authentication)
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
                var taskID = Guid.NewGuid();
                await this.WriteCacheAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DetachDomainHost();
                    this.TableContext.Dispose();
                    this.TableContext = null;
                    this.TypeContext.Dispose();
                    this.TypeContext = null;
                    this.ClearAuthentications();
                    this.Repository.Dispose();
                    this.Repository = null;
                    this.Dispatcher.Dispose();
                    this.Dispatcher = this.DataBaseContext.Dispatcher;
                    base.DataBaseState = DataBaseState.Unloaded;
                    base.Unload(authentication);
                    this.CremaHost.Sign(authentication);
                    this.DataBaseContext.InvokeItemsUnloadedEvent(authentication, new IDataBase[] { this });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> EnterAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateEnter(authentication);
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterAsync), this);
                });
                var taskID = Guid.NewGuid();
                await this.DomainContext.AttachUsersAsync(authentication, this.ID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.authentications.Add(authentication);
                    this.AuthenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
                    authentication.Expired += Authentication_Expired;
                    this.CremaHost.Sign(authentication);
                    this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                    this.DataBaseContext.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateLeave(authentication);
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveAsync), this);
                });
                var taskID = Guid.NewGuid();
                await this.DomainContext.DetachUsersAsync(authentication, this.ID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.authentications.Remove(authentication);
                    this.AuthenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
                    authentication.Expired -= Authentication_Expired;
                    this.CremaHost.Sign(authentication);
                    this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                    this.DataBaseContext.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RenameAsync(Authentication authentication, string name)
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
                var taskID = Guid.NewGuid();
                await this.DataBaseContext.InvokeDataBaseRenameAsync(authentication, tuple.DataBaseInfo, name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Name = name;
                    this.CremaHost.Sign(authentication);
                    this.metaData.DataBaseInfo = base.DataBaseInfo;
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBaseContext.InvokeItemsRenamedEvent(authentication, new DataBase[] { this }, new string[] { tuple.Name });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> DeleteAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    base.ValidateDelete(authentication);
                    return (base.DataBaseInfo, base.Name);
                });
                var taskID = Guid.NewGuid();
                await this.DataBaseContext.InvokeDataBaseDeleteAsync(authentication, tuple.DataBaseInfo);
                await this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.DataBaseState = DataBaseState.None;
                    this.CremaHost.Sign(authentication);
                    this.DeleteCache();
                    this.DataBaseContext.InvokeItemsDeletedEvent(authentication, new DataBase[] { this }, new string[] { tuple.Name });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public bool Contains(Authentication authentication)
        {
            return this.authentications.Contains(authentication);
        }

        public async Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    this.ValidateGetLog(authentication);
                    var remotePath = this.IsLoaded == true ? this.BasePath : this.CremaHost.GetPath(CremaPath.RepositoryDataBases);
                    return (remotePath, base.Name);
                });
                var logs = await this.CremaHost.RepositoryDispatcher.InvokeAsync(() =>
                {
                    return this.repositoryProvider.GetLog(tuple.remotePath, tuple.Name, revision);
                });
                return logs.ToArray();
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RevertAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RevertAsync), this, revision);
                    this.ValidateRevert(authentication, revision);
                    return base.Name;
                });
                var taskID = Guid.NewGuid();
                var result = await this.DataBaseContext.InvokeDataBaseRevertAsync(authentication, name, revision);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.DataBaseInfo = new DataBaseInfo()
                    {
                        ID = result.ID,
                        Name = result.Name,
                        Revision = result.Revision,
                        Comment = result.Comment,
                        CreationInfo = result.CreationInfo,
                        ModificationInfo = result.ModificationInfo,
                    };
                    this.CremaHost.Sign(authentication);
                    this.DataBaseContext.InvokeItemsRevertedEvent(authentication, new IDataBase[] { this }, new string[] { revision });
                    this.DataBaseContext.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void LockForTransaction(Authentication authentication, Guid transactionID)
        {
            if (this.IsLocked == false)
            {
                this.Lock(authentication, $"{transactionID}");
                this.DataBaseContext.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { $"{transactionID}", });
            }
        }

        public void UnlockForTransaction(Authentication authentication, Guid transactionID)
        {
            if (this.LockInfo.Comment == $"{transactionID}" && this.IsLocked == true)
            {
                this.Unlock(authentication);
                this.DataBaseContext.InvokeItemsUnlockedEvent(authentication, new IDataBase[] { this });
            }
        }

        public async Task<DataBaseTransaction> BeginTransactionAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var transaction = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateBeginTransaction(authentication);

                    return new DataBaseTransaction(authentication, this, this.Repository);
                });
                transaction.BeginAsync(authentication);
                return transaction;
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

        public DataBaseMetaData GetMetaData(Authentication authentication)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            return this.metaData;
        }

        public async Task DisposeAsync()
        {
            await this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut);
            if (this.IsLoaded == true)
            {
                await this.WriteCacheAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ClearAuthentications();
                    this.TableContext.Dispose();
                    this.TypeContext.Dispose();
                    this.Repository.Dispose();
                    this.Dispatcher.Dispose();
                });
            }
            base.DataBaseState = DataBaseState.None;
            this.TableContext = null;
            this.TypeContext = null;
        }

        public async Task ResettingDataBaseAsync(Authentication authentication)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.IsLoaded == true)
                    this.DetachDomainHost();

                this.TypeContext?.Dispose();
                this.TypeContext = null;
                this.TableContext?.Dispose();
                this.TableContext = null;
                base.ResettingDataBase(authentication);
                this.DataBaseContext.InvokeItemsResettingEvent(authentication, new IDataBase[] { this });
            });
        }

        public async Task ResetDataBaseAsync(Authentication authentication, TypeInfo[] typeInfos, TableInfo[] tableInfos)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.Sign(authentication);
                this.TypeContext = new TypeContext(this, typeInfos);
                this.TypeContext.ItemsCreated += TypeContext_ItemsCreated;
                this.TypeContext.ItemsRenamed += TypeContext_ItemsRenamed;
                this.TypeContext.ItemsMoved += TypeContext_ItemsMoved;
                this.TypeContext.ItemsDeleted += TypeContext_ItemsDeleted;
                this.TypeContext.ItemsChanged += TypeContext_ItemsChanged;
                this.TypeContext.ItemsLockChanged += TypeContext_ItemsLockChanged;
                this.TypeContext.ItemsAccessChanged += TypeContext_ItemsAccessChanged;
                this.TableContext = new TableContext(this, tableInfos);
                this.TableContext.ItemsCreated += TableContext_ItemsCreated;
                this.TableContext.ItemsRenamed += TableContext_ItemsRenamed;
                this.TableContext.ItemsMoved += TableContext_ItemsMoved;
                this.TableContext.ItemsDeleted += TableContext_ItemsDeleted;
                this.TableContext.ItemsChanged += TableContext_ItemsChanged;
                this.TableContext.ItemsLockChanged += TableContext_ItemsLockChanged;
                this.TableContext.ItemsAccessChanged += TableContext_ItemsAccessChanged;
                this.metaData.TypeCategories = this.TypeContext.GetCategoryMetaDatas();
                this.metaData.Types = this.TypeContext.GetTypeMetaDatas();
                this.metaData.TableCategories = this.TableContext.GetCategoryMetaDatas();
                this.metaData.Tables = this.TableContext.GetTableMetaDatas();
                base.ResetDataBase(authentication);
                base.UpdateLockParent();
                base.UpdateAccessParent();

                this.AttachDomainHost();
                this.DataBaseContext.InvokeItemsResetEvent(authentication, new IDataBase[] { this }, new DataBaseMetaData[] { this.GetMetaData(authentication) });
            });
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(TableContext))
                return this.TableContext;
            else if (serviceType == typeof(TableCategoryCollection))
                return this.TableContext.Categories;
            else if (serviceType == typeof(TableCollection))
                return this.TableContext.Tables;
            else if (serviceType == typeof(TypeContext))
                return this.TypeContext;
            else if (serviceType == typeof(TypeCategoryCollection))
                return this.TypeContext.Categories;
            else if (serviceType == typeof(TypeCollection))
                return this.TypeContext.Types;
            else if (serviceType == typeof(DomainContext))
                return this.CremaHost.DomainContext;
            else
                return this.CremaHost.GetService(serviceType);
        }

        public Task<CremaDataSet> GetDataSetAsync(Authentication authentication, DataSetType dataSetType, string filterExpression, string revision)
        {
            this.ValidateGetDataSet(authentication);
            this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, dataSetType, filterExpression, revision);
            return dataSetType switch
            {
                DataSetType.All => this.GetDataSetAsync(authentication, revision, filterExpression, ReadTypes.All),
                DataSetType.OmitContent => this.GetDataSetAsync(authentication, revision, filterExpression, ReadTypes.OmitContent),
                DataSetType.TypeOnly => this.GetDataSetAsync(authentication, revision, filterExpression, ReadTypes.TypeOnly),
                _ => throw new NotImplementedException(),
            };
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
            var fullPaths = await this.Dispatcher.InvokeAsync(() =>
            {
                var typePaths = tables.SelectMany(item => item.GetTypes())
                                  .Select(item => item.FullPath)
                                  .Distinct()
                                  .ToArray();
                var tablePaths = tables.SelectMany(item => EnumerableUtility.Friends(item, item.DerivedTables))
                                       .Select(item => item.Parent ?? item)
                                       .Select(item => item.FullPath)
                                       .Distinct()
                                       .ToArray();
                return typePaths.Concat(tablePaths).ToArray();
            });

            return await this.Repository.Dispatcher.InvokeAsync(() => this.Repository.ReadDataSet(authentication, fullPaths, schemaOnly));
        }

        public string BasePath => this.CremaHost.GetPath(CremaPath.DataBases, $"{base.DataBaseInfo.ID}");

        public CremaHost CremaHost => this.DataBaseContext.CremaHost;

        public DataBaseContext DataBaseContext { get; }

        public TableContext TableContext { get; private set; }

        public TypeContext TypeContext { get; private set; }

        public UserContext UserContext => this.CremaHost.UserContext;

        public CremaDispatcher Dispatcher { get; private set; }

        public IObjectSerializer Serializer { get; }

        public DataBaseRepositoryHost Repository { get; private set; }

        public new DataBaseInfo DataBaseInfo => base.DataBaseInfo;

        public new DataBaseState DataBaseState
        {
            get => base.DataBaseState;
            set => base.DataBaseState = value;
        }

        public AuthenticationInfo[] AuthenticationInfos { get; private set; }

        public override TypeCategoryBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext> TypeCategory => this.TypeContext?.Root;

        public override TableCategoryBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext> TableCategory => this.TableContext?.Root;

        public new string Name => base.Name;

        public bool IsLoaded => base.DataBaseState == DataBaseState.Loaded;

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

        public event TaskCompletedEventHandler TaskCompleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateSetPrivate(IAuthentication authentication, object target)
        {
            base.OnValidateSetPrivate(authentication, target);

            if (target == this)
            {
                var userID = authentication.ID;
                var userInfo = this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users[userID].UserInfo);
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
                var userInfo = this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users[userID].UserInfo);
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
                var userInfo = this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users[memberID].UserInfo);
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
                var userInfo = this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users[memberID].UserInfo);
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
            if (this.DataBaseContext.ContainsKey(newName) == true)
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

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs e)
        {
            this.taskCompleted?.Invoke(this, e);
        }

        private void AttachDomainHost()
        {
            var domains = this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.GetDomains(this.ID));
            var authentications = this.authentications.Select(item => (Authentication)item).ToArray();
            var domainHostByDomain = this.FindDomainHosts(domains);
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
                domainHost.Attach(domain);
            }
            this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.AttachDomainHost(authentications, domainHostByDomain));
        }

        private void DetachDomainHost()
        {
            var domains = this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.GetDomains(this.ID));
            var authentications = this.authentications.Select(item => (Authentication)item).ToArray();
            var domainHostByDomain = domains.ToDictionary(item => item, item => item.Host);
            this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.DetachDomainHost(authentications, domainHostByDomain));
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
                domainHost.Detach();
            }
        }

        private void SetDataBaseState(Authentication authentication, DataBaseState dataBaseState)
        {
            this.Dispatcher?.VerifyAccess();
            if (base.DataBaseState == dataBaseState)
                return;
            base.DataBaseState = dataBaseState;
            this.DataBaseContext.InvokeItemsStateChangedEvent(authentication, new IDataBase[] { this });
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
            this.metaData.Authentications = this.AuthenticationInfos = new AuthenticationInfo[] { };
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
                    TypeInfos = this.TypeContext.Types.Select((Type item) => item.TypeInfo).ToArray(),
                    TableInfos = this.TableContext.Tables.Select((Table item) => item.TableInfo).ToArray(),
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
                var basePath = this.CremaHost.GetPath(CremaPath.DataBases, $"{repositoryInfo.ID}");
                var itemList = new string[] { };
                if (Directory.Exists(basePath) == true)
                {
                    repositoryInfo = this.repositoryProvider.GetRepositoryInfo(basePath, base.Name);
                    itemList = this.repositoryProvider.GetRepositoryItemList(basePath, base.Name);
                }
                else
                {
                    itemList = this.repositoryProvider.GetRepositoryItemList(remotePath, base.Name);
                }

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
            this.metaData.DataBaseInfo = base.DataBaseInfo;
            this.metaData.DataBaseState = base.DataBaseState;
            this.metaData.AccessInfo = base.AccessInfo;
            this.metaData.LockInfo = base.LockInfo;
        }

        private async void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (e.UserID == this.LockInfo.UserID)
                {
                    this.Unlock(Authentication.System);
                }
            });
        }

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
                this.metaData.TypeCategories = this.TypeContext.GetCategoryMetaDatas();
                this.metaData.Types = this.TypeContext.GetTypeMetaDatas();
                this.metaData.TableCategories = this.TableContext.GetCategoryMetaDatas();
                this.metaData.Tables = this.TableContext.GetTableMetaDatas();
                base.DataBaseInfo = this.metaData.DataBaseInfo = dataBaseInfo;
                this.DataBaseContext.InvokeItemsChangedEvent(Authentication.System, new IDataBase[] { this });
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
                        this.AuthenticationInfos = this.metaData.Authentications = this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();
                        authentication.Expired -= Authentication_Expired;
                        return true;
                    }
                    return false;
                });
                if (value == true)
                {
                    await this.DomainContext.DetachUsersAsync(authentication, this.ID);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                        this.DataBaseContext.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
                    });
                }
            }
        }

        private Task<AccessInfo> InvokeDataBaseSetPublicAsync(Authentication authentication, AccessInfo accessInfo)
        {
            var message = EventMessageBuilder.SetPublicDataBase(authentication, this.Name);
            var repositoryPath = new RepositoryPath(this, PathUtility.Separator);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    accessInfo.SetPublic();
                    var itemPaths = this.Serializer.GetPath(repositoryPath.Path, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
                    this.Repository.DeleteRange(itemPaths);
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

        private Task<AccessInfo> InvokeDataBaseSetPrivateAsync(Authentication authentication, DataBaseInfo dataBaseInfo, AccessInfo accessInfo)
        {
            var message = EventMessageBuilder.SetPrivateDataBase(authentication, dataBaseInfo.Name);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var repositoryPath = new RepositoryPath(this, PathUtility.Separator);
                    var signatureDate = authentication.Sign();
                    accessInfo.SetPrivate(dataBaseInfo.Name, signatureDate);
                    var itemPaths = this.Serializer.Serialize(repositoryPath.Path, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
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

        private Task<AccessInfo> InvokeDataBaseAddAccessMemberAsync(Authentication authentication, DataBaseInfo dataBaseInfo, AccessInfo accessInfo, string memberID, AccessType accessType)
        {
            var message = EventMessageBuilder.AddAccessMemberToDataBase(authentication, dataBaseInfo.Name, memberID, accessType);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var repositoryPath = new RepositoryPath(this, PathUtility.Separator);
                    var signatureDate = authentication.Sign();
                    accessInfo.Add(signatureDate, memberID, accessType);
                    this.Serializer.Serialize(repositoryPath.Path, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
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

        private Task<AccessInfo> InvokeDataBaseSetAccessMemberAsync(Authentication authentication, DataBaseInfo dataBaseInfo, AccessInfo accessInfo, string memberID, AccessType accessType)
        {
            var message = EventMessageBuilder.SetAccessMemberOfDataBase(authentication, dataBaseInfo.Name, memberID, accessType);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var repositoryPath = new RepositoryPath(this, PathUtility.Separator);
                    var signatureDate = authentication.Sign();
                    accessInfo.Set(signatureDate, memberID, accessType);
                    this.Serializer.Serialize(repositoryPath.Path, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
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

        private Task<AccessInfo> InvokeDataBaseRemoveAccessMemberAsync(Authentication authentication, DataBaseInfo dataBaseInfo, AccessInfo accessInfo, string memberID)
        {
            var message = EventMessageBuilder.RemoveAccessMemberFromDataBase(authentication, dataBaseInfo.Name, memberID);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var repositoryPath = new RepositoryPath(this, PathUtility.Separator);
                    var signatureDate = authentication.Sign();
                    accessInfo.Remove(signatureDate, memberID);
                    this.Serializer.Serialize(repositoryPath.Path, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
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
            if (base.DataBaseState != DataBaseState.None)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasBeenLoaded);
            if (authentication.IsSystem == false && authentication.IsAdmin == false)
                throw new PermissionDeniedException();
            if (this.VerifyAccessType(authentication, AccessType.Master) == false)
                throw new PermissionDeniedException();
        }

        private void ValidateUnload(Authentication authentication)
        {
            if (base.DataBaseState != DataBaseState.Loaded)
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

        private void ValidateGetLog(Authentication authentication)
        {
            if (this.VerifyAccessType(authentication, AccessType.Guest) == false)
                throw new PermissionDeniedException();
        }

        private void ValidateRevert(Authentication authentication, string revision)
        {
            if (authentication.IsSystem == false && authentication.IsAdmin == false)
                throw new PermissionDeniedException();
            if (base.DataBaseState != DataBaseState.Unloaded)
                throw new InvalidOperationException(Resources.Exception_LoadedDataBaseCannotRevert);
        }

        private void ReadAccessInfo()
        {
            var itemPath = this.BasePath + Path.DirectorySeparatorChar;
            try
            {
                if (this.Serializer.Exists(itemPath, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings) == true)
                {
                    var accessInfo = (AccessSerializationInfo)this.Serializer.Deserialize(itemPath, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
                    this.SetAccessInfo((AccessInfo)accessInfo);
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        private IDictionary<Domain, IDomainHost> FindDomainHosts(Domain[] domains)
        {
            var dictionary = new Dictionary<Domain, IDomainHost>(domains.Length);
            foreach (var item in domains)
            {
                dictionary.Add(item, this.FindDomainHost(item));
            }
            return dictionary;
        }

        private IDomainHost FindDomainHost(Domain domain)
        {
            var domainInfo = domain.DomainInfo;
            var itemPath = domainInfo.ItemPath;
            var itemType = domainInfo.ItemType;

            if (itemType == nameof(TableContent))
            {
                return new TableContent.TableContentGroup(this.TableContext.Tables, itemPath);
            }
            else if (itemType == nameof(NewTableTemplate))
            {
                if (this.TableContext[itemPath] is TableCategory category)
                {
                    return new NewTableTemplate(category);
                }
                else if (this.TableContext[itemPath] is Table table)
                {
                    return new NewTableTemplate(table);
                }
                throw new NotImplementedException();
            }
            else if (itemType == nameof(TableTemplate))
            {
                var table = this.TableContext[itemPath] as Table;
                return table.Template;
            }
            else if (itemType == nameof(NewTypeTemplate))
            {
                var category = this.TypeContext[itemPath] as TypeCategory;
                return new NewTypeTemplate(category);
            }
            else if (itemType == nameof(TypeTemplate))
            {
                var type = this.TypeContext[itemPath] as Type;
                return type.Template;
            }

            return null;
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

        private void TypeContext_ItemsLockChanged(object sender, ItemsEventArgs<ITypeItem> e)
        {
            this.metaData.TypeCategories = this.TypeContext.GetCategoryMetaDatas();
            this.metaData.Types = this.TypeContext.GetTypeMetaDatas();
        }

        private void TypeContext_ItemsAccessChanged(object sender, ItemsEventArgs<ITypeItem> e)
        {
            this.metaData.TypeCategories = this.TypeContext.GetCategoryMetaDatas();
            this.metaData.Types = this.TypeContext.GetTypeMetaDatas();
        }

        private void TableContext_ItemsLockChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            this.metaData.TableCategories = this.TableContext.GetCategoryMetaDatas();
            this.metaData.Tables = this.TableContext.GetTableMetaDatas();
        }

        private void TableContext_ItemsAccessChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            this.metaData.TableCategories = this.TableContext.GetCategoryMetaDatas();
            this.metaData.Tables = this.TableContext.GetTableMetaDatas();
        }

        private IEnumerable<string> GetItemPaths()
        {
            yield return PathUtility.Separator;
            foreach (var item in this.TypeContext)
            {
                yield return PathUtility.Separator + CremaSchema.TypeDirectory + item.Path;
            }
            foreach (var item in this.TableContext)
            {
                yield return PathUtility.Separator + CremaSchema.TableDirectory + item.Path;
            }
        }

        private DomainContext DomainContext => this.CremaHost.DomainContext;

        #region IDataBase

        Task IDataBase.LoadAsync(Authentication authentication)
        {
            return this.LoadAsync(authentication);
        }

        Task IDataBase.UnloadAsync(Authentication authentication)
        {
            return this.UnloadAsync(authentication);
        }

        Task IDataBase.EnterAsync(Authentication authentication)
        {
            return this.EnterAsync(authentication);
        }

        Task IDataBase.LeaveAsync(Authentication authentication)
        {
            return this.LeaveAsync(authentication);
        }

        Task IDataBase.RenameAsync(Authentication authentication, string name)
        {
            return this.RenameAsync(authentication, name);
        }

        Task IDataBase.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        Task IDataBase.RevertAsync(Authentication authentication, string revision)
        {
            return this.RevertAsync(authentication, revision);
        }

        Task IDataBase.ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            return this.ImportAsync(authentication, dataSet, comment);
        }

        async Task<IDataBase> IDataBase.CopyAsync(Authentication authentication, string newDataBaseName, string comment, bool force)
        {
            return await this.DataBaseContext.CopyDataBaseAsync(authentication, this, newDataBaseName, comment, force);
        }

        async Task<ITransaction> IDataBase.BeginTransactionAsync(Authentication authentication)
        {
            return await this.BeginTransactionAsync(authentication);
        }

        ITableContext IDataBase.TableContext => this.TableContext;

        ITypeContext IDataBase.TypeContext => this.TypeContext;

        #endregion

        #region IAccessible

        Task IAccessible.SetPublicAsync(Authentication authentication)
        {
            return this.SetPublicAsync(authentication);
        }

        Task IAccessible.SetPrivateAsync(Authentication authentication)
        {
            return this.SetPrivateAsync(authentication);
        }

        Task IAccessible.AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            return this.AddAccessMemberAsync(authentication, memberID, accessType);
        }

        Task IAccessible.SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            return this.SetAccessMemberAsync(authentication, memberID, accessType);
        }

        Task IAccessible.RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            return this.RemoveAccessMemberAsync(authentication, memberID);
        }

        #endregion

        #region ILockable

        Task ILockable.LockAsync(Authentication authentication, string comment)
        {
            return this.LockAsync(authentication, comment);
        }

        Task ILockable.UnlockAsync(Authentication authentication)
        {
            return this.UnlockAsync(authentication);
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IDataBase))
                return this;

            if (base.DataBaseState.HasFlag(DataBaseState.Loaded) == true)
            {
                if (serviceType == typeof(ITableContext))
                    return this.TableContext;
                else if (serviceType == typeof(ITableCategoryCollection))
                    return this.TableContext.Categories;
                else if (serviceType == typeof(ITableCollection))
                    return this.TableContext.Tables;
                else if (serviceType == typeof(ITypeContext))
                    return this.TypeContext;
                else if (serviceType == typeof(ITypeCategoryCollection))
                    return this.TypeContext.Categories;
                else if (serviceType == typeof(ITypeCollection))
                    return this.TypeContext.Types;
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

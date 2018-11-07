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
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TableContext : ItemContext<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        ITableContext
    {
        private DataBase dataBase;
        private readonly DataBaseRepositoryHost repository;

        private ItemsCreatedEventHandler<ITableItem> itemsCreated;
        private ItemsRenamedEventHandler<ITableItem> itemsRenamed;
        private ItemsMovedEventHandler<ITableItem> itemsMoved;
        private ItemsDeletedEventHandler<ITableItem> itemsDeleted;
        private ItemsEventHandler<ITableItem> itemsChanged;
        private ItemsEventHandler<ITableItem> itemsAccessChanged;
        private ItemsEventHandler<ITableItem> itemsLockChanged;

        public TableContext(DataBase dataBase, IEnumerable<TableInfo> tableInfos)
        {
            this.dataBase = dataBase;
            this.repository = dataBase.Repository;
            this.CremaHost.Debug(Resources.Message_TableContextInitialize);
            this.BasePath = Path.Combine(dataBase.BasePath, CremaSchema.TableDirectory);
            this.Initialize(tableInfos);
            this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
            this.CremaHost.Debug(Resources.Message_TableContextIsCreated);
        }

        public void Dispose()
        {
            this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut);
            this.dataBase = null;
        }

        public Task<AccessInfo> InvokeTableItemSetPublicAsync(Authentication authentication, string tableItemPath, AccessInfo accessInfo)
        {
            var message = EventMessageBuilder.SetPublicTableItem(authentication, tableItemPath);
            var repositoryPath = new RepositoryPath(this, tableItemPath);
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

        // TODO: 잠금 설정과 해제 처리를 외부에서 (단 예외시는 바로 해제 처리)
        public Task<AccessInfo> InvokeTableItemSetPrivateAsync(Authentication authentication, string tableItemPath, AccessInfo accessInfo)
        {
            var message = EventMessageBuilder.SetPrivateTableItem(authentication, tableItemPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var repositoryPath = new RepositoryPath(this, tableItemPath);
                    var signatureDate = authentication.Sign();
                    accessInfo.SetPrivate(tableItemPath, signatureDate);
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

        public Task<AccessInfo> InvokeTableItemAddAccessMemberAsync(Authentication authentication, string tableItemPath, AccessInfo accessInfo, string memberID, AccessType accessType)
        {
            var message = EventMessageBuilder.AddAccessMemberToTableItem(authentication, tableItemPath, memberID, accessType);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var repositoryPath = new RepositoryPath(this, tableItemPath);
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

        public Task<AccessInfo> InvokeTableItemSetAccessMemberAsync(Authentication authentication, string tableItemPath, AccessInfo accessInfo, string memberID, AccessType accessType)
        {
            var message = EventMessageBuilder.SetAccessMemberOfTableItem(authentication, tableItemPath, memberID, accessType);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var repositoryPath = new RepositoryPath(this, tableItemPath);
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

        public Task<AccessInfo> InvokeTableItemRemoveAccessMemberAsync(Authentication authentication, string tableItemPath, AccessInfo accessInfo, string memberID)
        {
            var message = EventMessageBuilder.RemoveAccessMemberFromTableItem(authentication, tableItemPath, memberID);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var respositoryPath = new RepositoryPath(this, tableItemPath);
                    var signatureDate = authentication.Sign();
                    accessInfo.Remove(signatureDate, memberID);
                    this.Serializer.Serialize(respositoryPath.Path, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
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

        public void InvokeItemsSetPublicEvent(Authentication authentication, ITableItem[] items, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeItemsSetPublicEvent), items);
            var message = EventMessageBuilder.SetPublicTableItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Public);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public void InvokeItemsSetPrivateEvent(Authentication authentication, ITableItem[] items, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeItemsSetPrivateEvent), items);
            var message = EventMessageBuilder.SetPrivateTableItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Private);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public void InvokeItemsAddAccessMemberEvent(Authentication authentication, ITableItem[] items, string[] memberIDs, AccessType[] accessTypes, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeItemsAddAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.AddAccessMemberToTableItem(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Add, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public void InvokeItemsSetAccessMemberEvent(Authentication authentication, ITableItem[] items, string[] memberIDs, AccessType[] accessTypes, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeItemsSetAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.SetAccessMemberOfTableItem(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Set, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public void InvokeItemsRemoveAccessMemberEvent(Authentication authentication, ITableItem[] items, string[] memberIDs, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeItemsRemoveAccessMemberEvent), items, memberIDs);
            var message = EventMessageBuilder.RemoveAccessMemberFromTableItem(authentication, items, memberIDs);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Remove, memberIDs);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public void InvokeItemsLockedEvent(Authentication authentication, ITableItem[] items, string[] comments, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeItemsLockedEvent), items, comments);
            var message = EventMessageBuilder.LockTableItem(authentication, items, comments);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Lock, comments);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public void InvokeItemsUnlockedEvent(Authentication authentication, ITableItem[] items, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeItemsUnlockedEvent), items);
            var message = EventMessageBuilder.UnlockTableItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Unlock);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public void InvokeItemsCreatedEvent(Authentication authentication, ITableItem[] items, object[] args, object metaData, Guid taskID)
        {
            this.OnItemsCreated(new ItemsCreatedEventArgs<ITableItem>(authentication, items, args, metaData) { TaskID = taskID });
        }

        public void InvokeItemsRenamedEvent(Authentication authentication, ITableItem[] items, string[] oldNames, string[] oldPaths, object metaData, Guid taskID)
        {
            this.OnItemsRenamed(new ItemsRenamedEventArgs<ITableItem>(authentication, items, oldNames, oldPaths, metaData) { TaskID = taskID });
        }

        public void InvokeItemsMovedEvent(Authentication authentication, ITableItem[] items, string[] oldPaths, string[] oldParentPaths, object metaData, Guid taskID)
        {
            this.OnItemsMoved(new ItemsMovedEventArgs<ITableItem>(authentication, items, oldPaths, oldParentPaths, metaData) { TaskID = taskID });
        }

        public void InvokeItemsDeletedEvent(Authentication authentication, ITableItem[] items, string[] itemPaths, object metaData, Guid taskID)
        {
            this.OnItemsDeleted(new ItemsDeletedEventArgs<ITableItem>(authentication, items, itemPaths, metaData) { TaskID = taskID });
        }

        public void InvokeItemsChangedEvent(Authentication authentication, ITableItem[] items, object metaData, Guid taskID)
        {
            this.OnItemsChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData) { TaskID = taskID });
        }

        public LogInfo[] GetTableLog(string path, string revision)
        {
            var repositoryPath = new RepositoryPath(this, path);
            var files = repositoryPath.GetFiles();
            return this.Repository.GetLog(files, revision);
        }

        public LogInfo[] GetCategoryLog(string path, string revision)
        {
            var repositoryPath = new RepositoryPath(this, path);
            var files = repositoryPath.GetFiles();
            return this.Repository.GetLog(files, revision);
        }

        public CategoryMetaData[] GetCategoryMetaDatas()
        {
            var query = from TableCategory item in this.Categories
                        orderby item.Path
                        select item.MetaData;

            return query.ToArray();
        }

        public TableMetaData[] GetTableMetaDatas()
        {
            var query = from Table item in this.Tables
                        orderby item.Path
                        select item.MetaData;

            return query.ToArray();
        }

        //public string GenerateCategoryPath(string parentPath, string name)
        //{
        //    var value = new CategoryName(parentPath, name);
        //    return this.GenerateCategoryPath(value.Path);
        //}

        //public string GenerateCategoryPath(string categoryPath)
        //{
        //    NameValidator.ValidateCategoryPath(categoryPath);
        //    var baseUri = new Uri(this.BasePath);
        //    var uri = new Uri(baseUri + categoryPath);
        //    return uri.LocalPath;
        //}

        //public string GenerateTablePath(string categoryPath, string name)
        //{
        //    return Path.Combine(this.GenerateCategoryPath(categoryPath), name);
        //}

        //public string GeneratePath(string path)
        //{
        //    if (NameValidator.VerifyCategoryPath(path) == true)
        //        return this.GenerateCategoryPath(path);
        //    var itemName = new ItemName(path);
        //    return this.GenerateTablePath(itemName.CategoryPath, itemName.Name);
        //}

        //public string[] GetFiles(string itemPath)
        //{
        //    var directoryName = Path.GetDirectoryName(itemPath);
        //    var name = Path.GetFileName(itemPath);
        //    var files = Directory.GetFiles(directoryName, $"{name}.*").Where(item => Path.GetFileNameWithoutExtension(item) == name).ToArray();
        //    return files;
        //}

        public void ValidateTablePath(string categoryPath, string name, Table templatedParent)
        {
            var repositoryPath = new RepositoryPath(this, categoryPath + name);
            var templatedItemPath = templatedParent != null ? new RepositoryPath(this, templatedParent.Path).Path : null;
            var settings = new CremaDataTableSerializerSettings(repositoryPath.Path, templatedItemPath);
            var itemPaths = this.Serializer.GetPath(this.DataBase.BasePath, typeof(CremaDataTable), settings);
            foreach (var item in itemPaths)
            {
                this.DataBase.ValidateFileInfo(item);
            }
        }

        public void ValidateCategoryPath(string categoryPath)
        {
            var repositoryPath = new RepositoryPath(this, categoryPath);
            this.DataBase.ValidateDirectoryInfo(repositoryPath.Path);
        }

        public void LockItems(Authentication authentication, string[] itemPaths, string comment)
        {
            this.Dispatcher?.VerifyAccess();
            this.ValidateLockItems(authentication, itemPaths);
            var items = itemPaths.Select(item => this[item] as ITableItem).ToArray();

            foreach (var item in items)
            {
                if (item is Table table)
                    table.LockInternal(authentication, comment);
                else if (item is TableCategory category)
                    category.LockInternal(authentication, comment);
            }

            authentication.Sign();
            this.OnItemsLockChanged(new ItemsEventArgs<ITableItem>(authentication, items));
        }

        public void UnlockItems(Authentication authentication, string userID)
        {
            this.Dispatcher?.VerifyAccess();
            this.ValidateUnlockItems(authentication, userID);
            this.CremaHost.Sign(authentication);
            var query = from ITableItem item in this
                        let lockInfo = item.LockInfo
                        where lockInfo.IsLocked == true && lockInfo.IsInherited == false && lockInfo.IsOwner(userID)
                        select item;

            var items = query.ToArray();
            if (items.Any() == false)
                return;

            foreach (var item in items)
            {
                if (item is Table table)
                    table.UnlockInternal(authentication);
                else if (item is TableCategory category)
                    category.UnlockInternal(authentication);
            }

            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Unlock);
            this.OnItemsLockChanged(new ItemsEventArgs<ITableItem>(authentication, items, metaData));
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public TableCollection Tables => this.Items;

        public DataBase DataBase
        {
            get
            {
                if (this.dataBase == null)
                    throw new InvalidOperationException(Resources.Exception_InvalidObject);
                return this.dataBase;
            }
        }

        public CremaHost CremaHost => this.DataBase.CremaHost;

        public UserContext UserContext => this.CremaHost.UserContext;

        public CremaDispatcher Dispatcher => this.dataBase?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public string BasePath { get; }

        public event ItemsCreatedEventHandler<ITableItem> ItemsCreated
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsCreated += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<ITableItem> ItemsRenamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsRenamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<ITableItem> ItemsMoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsMoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<ITableItem> ItemsDeleted

        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsDeleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsDeleted -= value;
            }
        }

        public event ItemsEventHandler<ITableItem> ItemsChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsChanged -= value;
            }
        }

        public event ItemsEventHandler<ITableItem> ItemsAccessChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsAccessChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsAccessChanged -= value;
            }
        }

        public event ItemsEventHandler<ITableItem> ItemsLockChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsLockChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.itemsLockChanged -= value;
            }
        }

        protected virtual void OnItemsCreated(ItemsCreatedEventArgs<ITableItem> e)
        {
            this.itemsCreated?.Invoke(this, e);
        }

        protected virtual void OnItemsRenamed(ItemsRenamedEventArgs<ITableItem> e)
        {
            this.itemsRenamed?.Invoke(this, e);
        }

        protected virtual void OnItemsMoved(ItemsMovedEventArgs<ITableItem> e)
        {
            this.itemsMoved?.Invoke(this, e);
        }

        protected virtual void OnItemsDeleted(ItemsDeletedEventArgs<ITableItem> e)
        {
            this.itemsDeleted?.Invoke(this, e);
        }

        protected virtual void OnItemsChanged(ItemsEventArgs<ITableItem> e)
        {
            this.itemsChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsAccessChanged(ItemsEventArgs<ITableItem> e)
        {
            this.itemsAccessChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsLockChanged(ItemsEventArgs<ITableItem> e)
        {
            this.itemsLockChanged?.Invoke(this, e);
        }

        private void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            this.Dispatcher.InvokeAsync(() =>
            {
                if (this.dataBase == null)
                    return;
                foreach (var item in userIDs)
                {
                    this.UnlockItems(Authentication.System, item);
                }
            });
        }

        private void ValidateLockItems(Authentication authentication, string[] itemPaths)
        {
            var items = itemPaths.Select(item => this[item]);

            foreach (var item in items)
            {
                if (item is IPermission permission)
                {
                    if (permission.VerifyAccessType(authentication, AccessType.Master) == false)
                        throw new PermissionDeniedException();
                }

                {

                    var tableItem = item as ITableItem;
                    if (tableItem.IsLocked == true && tableItem.LockInfo.Path == tableItem.Path)
                        throw new PermissionException(string.Format(Resources.Exception_ItemIsAlreadyLocked_Format, item.Path));
                }
            }
        }

        private void ValidateUnlockItems(Authentication authentication, string userID)
        {
            foreach (ITableItem item in this)
            {
                if (item.LockInfo.UserID == userID && item.VerifyAccessType(authentication, AccessType.Master) == false)
                {
                    throw new PermissionDeniedException();
                }
            }
        }

        private void Initialize(IEnumerable<TableInfo> tableInfos)
        {
            this.CremaHost.Debug(Resources.Message_LoadTables);
            var directories = DirectoryUtility.GetAllDirectories(this.BasePath);
            foreach (var item in directories)
            {
                var categoryName = CategoryName.Create(UriUtility.MakeRelativeOfDirectory(this.BasePath, item));
                this.Categories.Prepare(categoryName.Path);
            }

            foreach (var item in tableInfos.OrderBy(i => i.Name))
            {
                var table = this.Tables.AddNew(Authentication.System, item.Name, item.CategoryPath);
                table.Initialize(item);
            }

            foreach (var item in tableInfos.Where(i => i.TemplatedParent != string.Empty))
            {
                var table = this.Tables[item.Name];
                table.TemplatedParent = this.Tables[item.TemplatedParent];
                if (table.TemplatedParent == null)
                {
                    throw new Exception();
                }
            }

            var itemPaths = this.Serializer.GetItemPaths(this.BasePath, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
            foreach (var item in itemPaths)
            {
                var accessInfo = (AccessSerializationInfo)this.Serializer.Deserialize(item, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
                var tableItem = this.GetTableItemByItemPath(item);
                if (tableItem is Table table)
                {
                    table.SetAccessInfo((AccessInfo)accessInfo);
                }
                else if (tableItem is TableCategory category)
                {
                    category.SetAccessInfo((AccessInfo)accessInfo);
                }
            }
            this.CremaHost.Debug(Resources.Message_TableLoadingIsCompleted);
        }

        private ITableItem GetTableItemByItemPath(string itemPath)
        {
            var isCategory = itemPath.EndsWith($"{Path.DirectorySeparatorChar}");
            var directory = Path.GetDirectoryName(itemPath);
            var relativeUri = UriUtility.MakeRelativeOfDirectory(this.BasePath, itemPath);
            var segments = StringUtility.Split(relativeUri, PathUtility.SeparatorChar, true);
            var path = isCategory == true ? (string)CategoryName.Create(segments) : ItemName.Create(segments);
            return this[path] as ITableItem;
        }

        private DataBaseRepositoryHost Repository => this.DataBase.Repository;

        #region ITableContext

        bool ITableContext.Contains(string itemPath)
        {
            return this.Contains(itemPath);
        }

        ITableCollection ITableContext.Tables => this.Tables;

        ITableCategoryCollection ITableContext.Categories => this.Categories;

        ITableCategory ITableContext.Root => this.Root;

        ITableItem ITableContext.this[string itemPath] => this[itemPath] as ITableItem;

        #endregion

        #region IEnumerable

        IEnumerator<ITableItem> IEnumerable<ITableItem>.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as ITableItem;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as ITableItem;
            }
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion
    }
}

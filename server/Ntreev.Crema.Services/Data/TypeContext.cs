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
    class TypeContext : TypeContextBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext>,
        ITypeContext
    {
        private DataBase dataBase;
        private readonly DataBaseRepositoryHost repository;

        private ItemsCreatedEventHandler<ITypeItem> itemsCreated;
        private ItemsRenamedEventHandler<ITypeItem> itemsRenamed;
        private ItemsMovedEventHandler<ITypeItem> itemsMoved;
        private ItemsDeletedEventHandler<ITypeItem> itemsDeleted;
        private ItemsEventHandler<ITypeItem> itemsChanged;
        private ItemsEventHandler<ITypeItem> itemsAccessChanged;
        private ItemsEventHandler<ITypeItem> itemsLockChanged;

        public TypeContext(DataBase dataBase, IEnumerable<TypeInfo> typeInfos)
        {
            this.dataBase = dataBase;
            this.repository = dataBase.Repository;
            this.BasePath = Path.Combine(dataBase.BasePath, CremaSchema.TypeDirectory);
            this.Initialize(typeInfos);
            this.CremaHost.UserContext.Dispatcher.Invoke(() => this.CremaHost.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
            this.CremaHost.Debug("TypeContext is created.");
        }

        public void Dispose()
        {
            var userContext = this.CremaHost.UserContext;
            this.dataBase = null;
            userContext.Dispatcher.Invoke(() => userContext.Users.UsersLoggedOut -= Users_UsersLoggedOut);
        }

        public Task<SignatureDate> InvokeTypeItemSetPublicAsync(Authentication authentication, string typeItemPath)
        {
            var message = EventMessageBuilder.SetPublicTypeItem(authentication, typeItemPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var itemPath = this.GeneratePath(typeItemPath);
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

        public Task<AccessInfo> InvokeTypeItemSetPrivateAsync(Authentication authentication, string typeItemPath)
        {
            var message = EventMessageBuilder.SetPrivateTypeItem(authentication, typeItemPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var accessInfo = AccessInfo.Empty;
                    var itemPath = this.GeneratePath(typeItemPath);
                    var signatureDate = authentication.Sign();
                    accessInfo.SetPrivate(typeof(ITypeItem).Name, signatureDate);
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

        public Task<AccessInfo> InvokeTypeItemAddAccessMemberAsync(Authentication authentication, string typeItemPath, AccessInfo accessInfo, string memberID, AccessType accessType)
        {
            var message = EventMessageBuilder.AddAccessMemberToTypeItem(authentication, typeItemPath, memberID, accessType);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var itemPath = this.GeneratePath(typeItemPath);
                    var signatureDate = authentication.Sign();
                    accessInfo.Add(signatureDate, memberID, accessType);
                    this.Serializer.Serialize(itemPath, (AccessSerializationInfo)accessInfo, AccessSerializationInfo.Settings);
                    this.Repository.Commit(authentication, message); accessInfo.Add(authentication.SignatureDate, memberID, accessType);
                    return accessInfo;
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task<AccessInfo> InvokeTypeItemSetAccessMemberAsync(Authentication authentication, string typeItemPath, AccessInfo accessInfo, string memberID, AccessType accessType)
        {
            var message = EventMessageBuilder.SetAccessMemberOfTypeItem(authentication, typeItemPath, memberID, accessType);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var itemPath = this.GeneratePath(typeItemPath);
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

        public Task<AccessInfo> InvokeTypeItemRemoveAccessMemberAsync(Authentication authentication, string typeItemPath, AccessInfo accessInfo, string memberID)
        {
            var message = EventMessageBuilder.RemoveAccessMemberFromTypeItem(authentication, typeItemPath, memberID);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var itemPath = this.GeneratePath(typeItemPath);
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

        public void InvokeItemsSetPublicEvent(Authentication authentication, ITypeItem[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPublicEvent), items);
            var message = EventMessageBuilder.SetPublicTypeItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Public);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsSetPrivateEvent(Authentication authentication, ITypeItem[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPrivateEvent), items);
            var message = EventMessageBuilder.SetPrivateTypeItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Private);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsAddAccessMemberEvent(Authentication authentication, ITypeItem[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsAddAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.AddAccessMemberToTypeItem(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Add, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsSetAccessMemberEvent(Authentication authentication, ITypeItem[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.SetAccessMemberOfTypeItem(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Set, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsRemoveAccessMemberEvent(Authentication authentication, ITypeItem[] items, string[] memberIDs)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsRemoveAccessMemberEvent), items, memberIDs);
            var message = EventMessageBuilder.RemoveAccessMemberFromTypeItem(authentication, items, memberIDs);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Remove, memberIDs);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<ITypeItem>(authentication, items, new object[] { AccessChangeType.Remove, memberIDs, }));
        }

        public void InvokeItemsLockedEvent(Authentication authentication, ITypeItem[] items, string[] comments)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsLockedEvent), items, comments);
            var message = EventMessageBuilder.LockTypeItem(authentication, items, comments);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Lock, comments);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsUnlockedEvent(Authentication authentication, ITypeItem[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsUnlockedEvent), items);
            var message = EventMessageBuilder.UnlockTypeItem(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Unlock);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public void InvokeItemsCreatedEvent(Authentication authentication, ITypeItem[] items, object[] args, object metaData)
        {
            this.OnItemsCreated(new ItemsCreatedEventArgs<ITypeItem>(authentication, items, args, metaData));
        }

        public void InvokeItemsRenamedEvent(Authentication authentication, ITypeItem[] items, string[] oldNames, string[] oldPaths, object metaData)
        {
            this.OnItemsRenamed(new ItemsRenamedEventArgs<ITypeItem>(authentication, items, oldNames, oldPaths, metaData));
        }

        public void InvokeItemsMovedEvent(Authentication authentication, ITypeItem[] items, string[] oldPaths, string[] oldParentPaths, object metaData)
        {
            this.OnItemsMoved(new ItemsMovedEventArgs<ITypeItem>(authentication, items, oldPaths, oldParentPaths, metaData));
        }

        public void InvokeItemsDeleteEvent(Authentication authentication, ITypeItem[] items, string[] itemPaths, object metaData)
        {
            this.OnItemsDeleted(new ItemsDeletedEventArgs<ITypeItem>(authentication, items, itemPaths, metaData));
        }

        public void InvokeItemsChangedEvent(Authentication authentication, ITypeItem[] items, object metaData)
        {
            this.OnItemsChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public LogInfo[] GetTypeLog(string itemPath, string revision)
        {
            var files = this.GetFiles(itemPath);
            return this.Repository.GetLog(files, revision);
        }

        public LogInfo[] GetCategoryLog(string localPath, string revision)
        {
            return this.Repository.GetLog(new string[] { localPath }, revision);
        }

        public CategoryMetaData[] GetCategoryMetaDatas()
        {
            var query = from TypeCategory item in this.Categories
                        orderby item.Path
                        select item.MetaData;

            return query.ToArray();
        }

        public TypeMetaData[] GetTypeMetaDatas()
        {
            var query = from Type item in this.Types
                        orderby item.Path
                        select item.MetaData;

            return query.ToArray();
        }

        public string GenerateCategoryPath(string parentPath, string name)
        {
            var categoryName = new CategoryName(parentPath, name);
            return this.GenerateCategoryPath(categoryName.Path);
        }

        public string GenerateCategoryPath(string categoryPath)
        {
            NameValidator.ValidateCategoryPath(categoryPath);
            var baseUri = new Uri(this.BasePath);
            var uri = new Uri(baseUri + categoryPath);
            return uri.LocalPath;
        }

        public string GenerateTypePath(string categoryPath, string name)
        {
            NameValidator.ValidateCategoryPath(categoryPath);
            return PathUtility.ConvertFromUri(this.BasePath + categoryPath + name);
        }

        public string GeneratePath(string path)
        {
            if (NameValidator.VerifyCategoryPath(path) == true)
                return this.GenerateCategoryPath(path);
            var itemName = new ItemName(path);
            return this.GenerateTypePath(itemName.CategoryPath, itemName.Name);
        }

        public string[] GetFiles(string itemPath)
        {
            var directoryName = Path.GetDirectoryName(itemPath);
            var name = Path.GetFileName(itemPath);
            var files = Directory.GetFiles(directoryName, $"{name}.*").Where(item => Path.GetFileNameWithoutExtension(item) == name).ToArray();
            return files;
        }

        public void ValidateTypePath(string categoryPath, string name)
        {
            var itemPath = this.GenerateTypePath(categoryPath, name);
            var settings = ObjectSerializerSettings.Empty;
            var itemPaths = this.Serializer.GetPath(itemPath, typeof(CremaDataType), settings);
            foreach (var item in itemPaths)
            {
                this.DataBase.ValidateFileInfo(item);
            }
        }

        public void ValidateCategoryPath(string categoryPath)
        {
            this.DataBase.ValidateDirectoryInfo(this.GenerateCategoryPath(categoryPath));
        }

        public void UnlockItems(Authentication authentication, string userID)
        {
            this.ValidateUnlockItems(authentication, userID);

            var query = from ITypeItem item in this
                        where item.IsLocked
                        let lockInfo = item.LockInfo
                        where lockInfo.Path == item.Path && lockInfo.UserID == userID
                        select item;

            var items = query.ToArray();
            if (items.Any() == false)
                return;

            foreach (var item in items)
            {
                if (item is Type type)
                    type.UnlockInternal(authentication);
                else if (item is TypeCategory category)
                    category.UnlockInternal(authentication);
            }

            authentication.Sign();
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Unlock);
            this.OnItemsLockChanged(new ItemsEventArgs<ITypeItem>(authentication, items, metaData));
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public TypeCollection Types => this.Items;

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

        public CremaDispatcher Dispatcher => this.dataBase?.Dispatcher;

        public string BasePath { get; }

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public event ItemsCreatedEventHandler<ITypeItem> ItemsCreated
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

        public event ItemsRenamedEventHandler<ITypeItem> ItemsRenamed
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

        public event ItemsMovedEventHandler<ITypeItem> ItemsMoved
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

        public event ItemsDeletedEventHandler<ITypeItem> ItemsDeleted
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

        public event ItemsEventHandler<ITypeItem> ItemsChanged
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

        public event ItemsEventHandler<ITypeItem> ItemsAccessChanged
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

        public event ItemsEventHandler<ITypeItem> ItemsLockChanged
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

        protected virtual void OnItemsCreated(ItemsCreatedEventArgs<ITypeItem> e)
        {
            this.itemsCreated?.Invoke(this, e);
        }

        protected virtual void OnItemsRenamed(ItemsRenamedEventArgs<ITypeItem> e)
        {
            this.itemsRenamed?.Invoke(this, e);
        }

        protected virtual void OnItemsMoved(ItemsMovedEventArgs<ITypeItem> e)
        {
            this.itemsMoved?.Invoke(this, e);
        }

        protected virtual void OnItemsDeleted(ItemsDeletedEventArgs<ITypeItem> e)
        {
            this.itemsDeleted?.Invoke(this, e);
        }

        protected virtual void OnItemsChanged(ItemsEventArgs<ITypeItem> e)
        {
            this.itemsChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsAccessChanged(ItemsEventArgs<ITypeItem> e)
        {
            this.itemsAccessChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsLockChanged(ItemsEventArgs<ITypeItem> e)
        {
            this.itemsLockChanged?.Invoke(this, e);
        }

        protected override IEnumerable<ITableInfoProvider> GetTables()
        {
            return this.DataBase.TableContext.Tables;
        }

        private void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            this.CremaHost.Dispatcher.InvokeAsync(() =>
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
                var permission = item as IPermission;

                if (permission.VerifyAccessType(authentication, AccessType.Master) == false)
                    throw new PermissionDeniedException();
            }
        }

        private void ValidateUnlockItems(Authentication authentication, string userID)
        {
            foreach (ITypeItem item in this)
            {
                if (item.LockInfo.UserID == userID && item.VerifyAccessType(authentication, AccessType.Master) == false)
                {
                    throw new PermissionDeniedException();
                }
            }
        }

        private void Initialize(IEnumerable<TypeInfo> typeInfos)
        {
            this.CremaHost.Debug("Load Types");
            var directories = DirectoryUtility.GetAllDirectories(this.BasePath);
            foreach (var item in directories)
            {
                var categoryName = CategoryName.Create(UriUtility.MakeRelativeOfDirectory(this.BasePath, item));
                this.Categories.Prepare(categoryName.Path);
            }

            foreach (var item in typeInfos)
            {
                var type = this.Types.AddNew(Authentication.System, item.Name, item.CategoryPath);
                type.Initialize(item);
            }

            var itemPaths = this.Serializer.GetItemPaths(this.BasePath, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
            foreach (var item in itemPaths)
            {
                var accessInfo = (AccessSerializationInfo)this.Serializer.Deserialize(item, typeof(AccessSerializationInfo), AccessSerializationInfo.Settings);
                var tableItem = this.GetTypeItemByItemPath(item);
                if (tableItem is Table table)
                {
                    table.SetAccessInfo((AccessInfo)accessInfo);
                }
                else if (tableItem is TableCategory category)
                {
                    category.SetAccessInfo((AccessInfo)accessInfo);
                }
            }
            this.CremaHost.Debug("TypeLoadingCompleted.");
        }

        private ITypeItem GetTypeItemByItemPath(string itemPath)
        {
            var isCategory = itemPath.EndsWith($"{Path.DirectorySeparatorChar}");
            var directory = Path.GetDirectoryName(itemPath);
            var relativeUri = UriUtility.MakeRelativeOfDirectory(this.BasePath, itemPath);
            var segments = StringUtility.Split(relativeUri, PathUtility.SeparatorChar, true);
            var path = isCategory == true ? (string)CategoryName.Create(segments) : ItemName.Create(segments);
            return this[path] as ITypeItem;
        }

        private DataBaseRepositoryHost Repository => this.DataBase.Repository;

        #region ITypeContext

        Task<bool> ITypeContext.ContainsAsync(string itemPath)
        {
            return this.Dispatcher.InvokeAsync(() => this.Contains(itemPath));
        }

        ITypeCollection ITypeContext.Types => this.Types;

        ITypeCategoryCollection ITypeContext.Categories => this.Categories;

        ITypeCategory ITypeContext.Root => this.Root;

        ITypeItem ITypeContext.this[string itemPath] => this[itemPath] as ITypeItem;

        #endregion

        #region IEnumerable

        IEnumerator<ITypeItem> IEnumerable<ITypeItem>.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as ITypeItem;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as ITypeItem;
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

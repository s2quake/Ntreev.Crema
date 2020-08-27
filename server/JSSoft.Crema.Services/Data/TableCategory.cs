// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    // TODO: 카테고리에도 상태를 추가하는 방법을 생각해야 할것 같음. CategoryState.HasNew, 
    class TableCategory : TableCategoryBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        ITableCategory, ITableItem
    {
        private readonly List<NewTableTemplate> templateList = new List<NewTableTemplate>();

        public TableCategory()
        {

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
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTableItemSetPublicAsync(authentication, tuple.path, tuple.accessInfo);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsSetPublicEvent(authentication, new ITableItem[] { this });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
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
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTableItemSetPrivateAsync(authentication, tuple.path, tuple.accessInfo);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsSetPrivateEvent(authentication, new ITableItem[] { this });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
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
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTableItemAddAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsAddAccessMemberEvent(authentication, new ITableItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
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
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTableItemSetAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsSetAccessMemberEvent(authentication, new ITableItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
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
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTableItemRemoveAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsRemoveAccessMemberEvent(authentication, new ITableItem[] { this }, new string[] { memberID });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
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
                    var lockInfo = new LockInfo()
                    {
                        Path = this.Path,
                        ParentPath = string.Empty,
                        SignatureDate = new SignatureDate(authentication.ID),
                        Comment = comment
                    };
                    base.LockInfo = lockInfo;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsLockedEvent(authentication, new ITableItem[] { this }, new string[] { comment });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
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
                    var lockInfo = LockInfo.Empty;
                    base.LockInfo = lockInfo;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsUnlockedEvent(authentication, new ITableItem[] { this });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });
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
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    var targetName = new CategoryName(base.Path) { Name = name };
                    return (items, oldNames, oldPaths, path, targetName);
                });
                var taskID = Guid.NewGuid();
                var dataSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                using var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet);
                await this.Container.InvokeCategoryRenameAsync(authentication, tuple.path, name, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Rename(authentication, name);
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeCategoriesRenamedEvent(authentication, tuple.items, tuple.oldNames, tuple.oldPaths, dataSet);
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> MoveAsync(Authentication authentication, string parentPath)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, parentPath);
                    base.ValidateMove(authentication, parentPath);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldParentPaths = items.Select(item => item.Parent.Path).ToArray();
                    var path = base.Path;
                    var targetName = new CategoryName(parentPath, base.Name);
                    return (items, oldPaths, oldParentPaths, path, targetName);
                });
                var taskID = Guid.NewGuid();
                var dataSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                using var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet);
                await this.Container.InvokeCategoryMoveAsync(authentication, tuple.path, parentPath, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Move(authentication, parentPath);
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeCategoriesMovedEvent(authentication, tuple.items, tuple.oldPaths, tuple.oldParentPaths, dataSet);
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
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
                var container = this.Container;
                var repository = this.Repository;
                var cremaHost = this.CremaHost;
                var dataBase = this.DataBase;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    base.ValidateDelete(authentication);
                    this.CremaHost.Sign(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldPaths, path);
                });
                var taskID = Guid.NewGuid();
                var dataSet = await this.ReadDataForPathAsync(authentication, new CategoryName(tuple.path));
                using var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet);
                await this.Container.InvokeCategoryDeleteAsync(authentication, tuple.path, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Delete(authentication);
                    cremaHost.Sign(authentication);
                    container.InvokeCategoriesDeletedEvent(authentication, tuple.items, tuple.oldPaths);
                    dataBase.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<NewTableTemplate> NewTableAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var template = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewTableAsync), this);
                    return new NewTableTemplate(this);
                });
                await template.BeginEditAsync(authentication);
                return template;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<CremaDataSet> GetDataSetAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, revision);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    return this.Repository.GetTableCategoryData(this.Serializer, this.Path, revision);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    return this.Context.GetCategoryLog(this.Path, revision);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<FindResultInfo[]> FindAsync(Authentication authentication, string text, FindOptions options)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(FindAsync), this, text, options);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    if (this.GetService(typeof(DataFindService)) is DataFindService service)
                    {
                        return service.Dispatcher.Invoke(() => service.FindFromTable(this.DataBase.ID, new string[] { base.Path }, text, options));
                    }
                    throw new NotImplementedException();
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public async Task<CremaDataSet> ReadDataForPathAsync(Authentication authentication, CategoryName targetName)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var targetItemPaths = new string[]
                {
                    DataBase.TablePathPrefix + targetName.ParentPath,
                    DataBase.TablePathPrefix + targetName,
                };
                var items = EnumerableUtility.FamilyTree(this as ITableItem, item => item.Childs);
                var itemPaths = items.Select(item => DataBase.TablePathPrefix + item.Path).ToArray();
                var baseTables = items.Where(item => item is Table).Select(item => item as Table).ToArray();

                if (baseTables.Distinct().Count() != baseTables.Length)
                {
                    System.Diagnostics.Debugger.Launch();
                }
                var tableNames = baseTables.Select(item => item.Name).ToArray();
                var derivedTables = baseTables.SelectMany(item => item.DerivedTables).ToArray();
                var tables = baseTables.Concat(derivedTables).Distinct().ToArray();
                var types = tables.SelectMany(item => item.GetTypes()).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.FullPath).ToArray();
                var tableItemPaths = tables.Select(item => item.FullPath).ToArray();
                itemPaths = itemPaths.Concat(typeItemPaths).Concat(tableItemPaths).Concat(targetItemPaths).Distinct().ToArray();
                return (itemPaths, tableNames);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(authentication, this, nameof(ReadDataForPathAsync), tuple.itemPaths);
                var dataSet = this.Repository.ReadDataSet(authentication, tuple.itemPaths);
                dataSet.ExtendedProperties["TableNames"] = tuple.tableNames;
                return dataSet;
            });
        }

        public async Task<CremaDataSet> ReadDataForNewTemplateAsync(Authentication authentication)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var typeCollection = this.GetService(typeof(TypeCollection)) as TypeCollection;
                var types = typeCollection.ToArray<Type>();
                var typePaths = types.Select(item => item.FullPath).ToArray();
                var itemPaths = new string[] { this.FullPath };
                var fullPaths = itemPaths.Concat(typePaths).ToArray();
                return (fullPaths, typePaths, itemPaths);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(authentication, this, nameof(ReadDataForNewTemplateAsync), tuple.itemPaths);
                var dataSet = this.Repository.ReadDataSet(authentication, tuple.fullPaths);
                dataSet.SetItemPaths(tuple.itemPaths);
                return dataSet;
            });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateRename(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateRename(authentication, target, oldPath, newPath);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotRenameOnCreateTable);
            var categoryName = new CategoryName(Regex.Replace(this.Path, $"^{oldPath}", newPath));
            this.Context.ValidateCategoryPath(categoryName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateMove(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateMove(authentication, target, oldPath, newPath);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotMoveOnCreateTable);
            var tables = EnumerableUtility.Descendants<IItem, Table>(this as IItem, item => item.Childs);
            if (tables.Where(item => item.TableState != TableState.None).Any() == true)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingEdited_Format, string.Join(", ", tables.Select(item => item.Name))));
            var categoryName = new CategoryName(Regex.Replace(this.Path, $"^{oldPath}", newPath));
            this.Context.ValidateCategoryPath(categoryName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateDelete(IAuthentication authentication, object target)
        {
            base.OnValidateDelete(authentication, target);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteOnCreateTable);
            var tables = EnumerableUtility.Descendants<IItem, Table>(this as IItem, item => item.Childs).ToArray();
            if (tables.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeletePathWithItems);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            accessInfo.Path = this.Path;
            base.AccessInfo = accessInfo;
        }

        public void LockInternal(Authentication authentication, string comment)
        {
            base.Lock(authentication, comment);
        }

        public void UnlockInternal(Authentication authentication)
        {
            base.Unlock(authentication);
        }

        public void Attach(NewTableTemplate template)
        {
            template.EditCanceled += (s, e) => this.templateList.Remove(template);
            template.EditEnded += (s, e) => this.templateList.Remove(template);
            this.templateList.Add(template);
        }

        public CremaHost CremaHost => this.Context.DataBase.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public string BasePath => base.Path;

        public new string Name => base.Name;

        public new string Path => base.Path;

        public new bool IsLocked => base.IsLocked;

        public new bool IsPrivate => base.IsPrivate;

        public new AccessInfo AccessInfo => base.AccessInfo;

        public new LockInfo LockInfo => base.LockInfo;

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

        public new event EventHandler Moved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved -= value;
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

        public new event EventHandler LockChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.LockChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.LockChanged -= value;
            }
        }

        public new event EventHandler AccessChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.AccessChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.AccessChanged -= value;
            }
        }

        #region ITableCategory

        Task ITableCategory.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task ITableCategory.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task ITableCategory.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        async Task<ITableCategory> ITableCategory.AddNewCategoryAsync(Authentication authentication, string name)
        {
            return await this.Container.AddNewAsync(authentication, name, base.Path);
        }

        async Task<ITableTemplate> ITableCategory.NewTableAsync(Authentication authentication)
        {
            return await this.NewTableAsync(authentication);
        }

        ITableCategory ITableCategory.Parent => this.Parent;

        IContainer<ITableCategory> ITableCategory.Categories => this.Categories;

        IContainer<ITable> ITableCategory.Tables => this.Tables;

        #endregion

        #region ITableItem

        Task ITableItem.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task ITableItem.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task ITableItem.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        ITableItem ITableItem.Parent => this.Parent;

        IEnumerable<ITableItem> ITableItem.Childs
        {
            get
            {
                foreach (var item in this.Categories)
                {
                    yield return item;
                }
                foreach (var item in this.Items)
                {
                    if (item.Parent == null)
                        yield return item;
                }
            }
        }

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
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion
    }
}

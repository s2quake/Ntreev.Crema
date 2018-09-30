﻿//Released under the MIT License.
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
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
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

        public async Task SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                    this.CremaHost.Sign(authentication);
                    await this.Context.InvokeTableItemSetPublicAsync(authentication, this.Path);
                    base.SetPublic(authentication);
                    this.Context.InvokeItemsSetPublicEvent(authentication, new ITableItem[] { this });
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
                    base.ValidateSetPrivate(authentication);
                    var accessInfo = await this.Context.InvokeTableItemSetPrivateAsync(authentication, this.Path);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
                    base.SetPrivate(authentication);
                    this.Context.InvokeItemsSetPrivateEvent(authentication, new ITableItem[] { this });
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
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                    var accessInfo = await this.Context.InvokeTableItemAddAccessMemberAsync(authentication, this.Path, this.AccessInfo, memberID, accessType);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
                    base.AddAccessMember(authentication, memberID, accessType);
                    this.Context.InvokeItemsAddAccessMemberEvent(authentication, new ITableItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
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
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                    var accessInfo = await this.Context.InvokeTableItemSetAccessMemberAsync(authentication, this.Path, this.AccessInfo, memberID, accessType);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
                    base.SetAccessMember(authentication, memberID, accessType);
                    this.Context.InvokeItemsSetAccessMemberEvent(authentication, new ITableItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
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
                    base.ValidateRemoveAccessMember(authentication, memberID);
                    var accessInfo = await this.Context.InvokeTableItemRemoveAccessMemberAsync(authentication, this.Path, this.AccessInfo, memberID);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
                    base.RemoveAccessMember(authentication, memberID);
                    this.Context.InvokeItemsRemoveAccessMemberEvent(authentication, new ITableItem[] { this }, new string[] { memberID });
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
                    this.CremaHost.Sign(authentication);
                    base.Lock(authentication, comment);
                    this.Context.InvokeItemsLockedEvent(authentication, new ITableItem[] { this }, new string[] { comment });
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
                    this.CremaHost.Sign(authentication);
                    base.Unlock(authentication);
                    this.Context.InvokeItemsUnlockedEvent(authentication, new ITableItem[] { this });
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
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldNames, oldPaths, path);
                });
                var dataSet = await this.ReadDataForPathAsync(authentication);
                var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, false);
                var signatureDate = await this.Container.InvokeCategoryRenameAsync(authentication, tuple.path, name, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, signatureDate);
                    base.Rename(authentication, name);
                    this.Container.InvokeCategoriesRenamedEvent(authentication, tuple.items, tuple.oldNames, tuple.oldPaths, dataSet);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task MoveAsync(Authentication authentication, string parentPath)
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
                    return (items, oldPaths, oldParentPaths, path);
                });
                var dataSet = await this.ReadDataForPathAsync(authentication);
                var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, false);
                var signatureDate = await this.Container.InvokeCategoryMoveAsync(authentication, tuple.path, parentPath, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, signatureDate);
                    base.Move(authentication, parentPath);
                    this.Container.InvokeCategoriesMovedEvent(authentication, tuple.items, tuple.oldPaths, tuple.oldParentPaths, dataSet);
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
                    this.CremaHost.Sign(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldPaths, path);
                });
                var dataSet = await this.ReadDataForPathAsync(authentication);
                var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, false);
                var signatureDate = await this.Container.InvokeCategoryDeleteAsync(authentication, tuple.path, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var container = this.Container;
                    base.Delete(authentication);
                    container.InvokeCategoriesDeletedEvent(authentication, tuple.items, tuple.oldPaths);
                });
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
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewTableAsync), this);
                    var template = new NewTableTemplate(this);
                    await template.BeginEditAsync(authentication);
                    return template;
                });
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
                    return this.Repository.GetTableCategoryData(this.Serializer, this.ItemPath, revision);
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
                    return this.Context.GetCategoryLog(this.ItemPath, revision);
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

        /// <summary>
        /// 폴더내에 모든 테이블과 상속된 테이블을 읽어들입니다.
        /// </summary>
        public async Task<CremaDataSet> ReadDataForPathAsync(Authentication authentication)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var items = EnumerableUtility.FamilyTree(this as ITableItem, item => item.Childs);
                var itemPaths = items.Select(item => this.Context.GeneratePath(item.Path)).ToArray();
                var baseTables = items.Where(item => item is Table).Select(item => item as Table).ToArray();

                if(baseTables.Distinct().Count() != baseTables.Length)
                {
                    System.Diagnostics.Debugger.Launch();
                }
                var tableNames = baseTables.Select(item => item.Name).ToArray();
                var derivedTables = baseTables.SelectMany(item => item.DerivedTables).ToArray();
                var tables = baseTables.Concat(derivedTables).Distinct().ToArray();
                var types = tables.SelectMany(item => item.GetTypes()).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.ItemPath).ToArray();
                var tableItemPaths = tables.Select(item => item.ItemPath).ToArray();
                var props = new CremaDataSetSerializerSettings(authentication, typeItemPaths, tableItemPaths);
                var itemPath = this.ItemPath;
                itemPaths = itemPaths.Concat(typeItemPaths).Concat(tableItemPaths).Distinct().ToArray();
                return (itemPaths, props, itemPath, tableNames);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.itemPaths);
                var dataSet = this.Serializer.Deserialize(tuple.itemPath, typeof(CremaDataSet), tuple.props) as CremaDataSet;
                dataSet.ExtendedProperties[nameof(DataBaseSet.ItemPaths)] = tuple.itemPaths;
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
                var typePaths = types.Select(item => item.ItemPath).ToArray();
                var props = new CremaDataSetSerializerSettings(authentication, typePaths, null);
                var itemPath = this.ItemPath;
                var itemPaths = new string[] { itemPath };
                return (itemPaths, props, itemPath);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.itemPaths);
                var dataSet = this.Serializer.Deserialize(tuple.itemPath, typeof(CremaDataSet), tuple.props) as CremaDataSet;
                dataSet.ExtendedProperties[nameof(DataBaseSet.ItemPaths)] = tuple.itemPaths;
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

        public string ItemPath => this.Context.GenerateCategoryPath(base.Path);

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

        private Task<CremaDataSet> ReadDataAsync(Authentication authentication, IEnumerable<Table> tables)
        {
            var typePaths = tables.SelectMany(item => item.GetTypes())
                                  .Select(item => item.ItemPath)
                                  .Distinct()
                                  .ToArray();
            var tablePaths = tables.Select(item => item.ItemPath).Distinct().ToArray();
            var props = new CremaDataSetSerializerSettings(authentication, typePaths, tablePaths);
            return this.Repository.Dispatcher.InvokeAsync(() => this.Serializer.Deserialize(this.ItemPath, typeof(CremaDataSet), props) as CremaDataSet);
        }

        #region ITableCategory

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

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion
    }
}

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
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    // TODO: 상태를 더 추가하는 방법을 생각해야 할것 같음. TableState.HasContent, 
    class Table : TableBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        ITable, ITableItem, IInfoProvider, IStateProvider
    {
        private readonly List<NewTableTemplate> templateList = new List<NewTableTemplate>();

        public Table()
        {
            this.Template = new TableTemplate(this);
            this.Content = new TableContent(this);
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                    return base.Path;
                });
                var result = await this.Context.InvokeTableItemSetPublicAsync(authentication, path);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    base.ValidateSetPrivate(authentication);
                    return base.Path;
                });
                var result = await this.Context.InvokeTableItemSetPrivateAsync(authentication, path);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result.SignatureDate);
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
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var result = await this.Context.InvokeTableItemAddAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result.SignatureDate);
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
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var result = await this.Context.InvokeTableItemSetAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result.SignatureDate);
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
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    base.ValidateRemoveAccessMember(authentication, memberID);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var result = await this.Context.InvokeTableItemRemoveAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result.SignatureDate);
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
                    var items = EnumerableUtility.FamilyTree(this, item => item.Childs).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var tableInfo = base.TableInfo;
                    var targetName = new ItemName(base.Path) { Name = name };
                    return (items, oldNames, oldPaths, tableInfo, targetName);
                });
                var dataSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, false);
                var result = await this.Container.InvokeTableRenameAsync(authentication, tuple.tableInfo, name, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Rename(authentication, name);
                    this.Container.InvokeTablesRenamedEvent(authentication, tuple.items, tuple.oldNames, tuple.oldPaths, dataSet);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task MoveAsync(Authentication authentication, string categoryPath)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, categoryPath);
                    base.ValidateMove(authentication, categoryPath);
                    var items = EnumerableUtility.FamilyTree(this, item => item.Childs).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldCategoryPaths = items.Select(item => item.Category.Path).ToArray();
                    var tableInfo = base.TableInfo;
                    var targetName = new ItemName(categoryPath, base.Name);
                    return (items, oldPaths, oldCategoryPaths, tableInfo, targetName);
                });
                var dataSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, false);
                var result = await this.Container.InvokeTableMoveAsync(authentication, tuple.tableInfo, categoryPath, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Move(authentication, categoryPath);
                    this.Container.InvokeTablesMovedEvent(authentication, tuple.items, tuple.oldPaths, tuple.oldCategoryPaths, dataSet);
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
                var container = this.Container;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    base.ValidateDelete(authentication);
                    var items = EnumerableUtility.FamilyTree(this, item => item.Childs).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var tableInfo = base.TableInfo;
                    return (items, oldPaths, tableInfo);
                });
                var dataSet = await this.ReadDataForPathAsync(authentication, new ItemName(tuple.tableInfo.Path));
                var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, false);
                var result = await container.InvokeTableDeleteAsync(authentication, tuple.tableInfo, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Delete(authentication);
                    container.InvokeTablesDeletedEvent(authentication, tuple.items, tuple.oldPaths);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<Table> CopyAsync(Authentication authentication, string newTableName, string categoryPath, bool copyContent)
        {
            return this.Container.CopyAsync(authentication, this, newTableName, categoryPath, copyContent);
        }

        public Task<Table> InheritAsync(Authentication authentication, string newTableName, string categoryPath, bool copyContent)
        {
            return this.Container.InheritAsync(authentication, this, newTableName, categoryPath, copyContent);
        }

        public async Task<NewTableTemplate> NewChildAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var template = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewChildAsync), this);
                    this.ValidateNewChild(authentication);
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

        public Task<CremaDataSet> GetDataSetAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, revision);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    return this.Repository.GetTableData(this.Serializer, this.ItemPath, this.TemplatedParent?.ItemPath, revision);
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
                this.ValidateExpired();
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    return this.Context.GetTableLog(this.ItemPath, revision);
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

        public string ItemPath => this.Context.GenerateTablePath(this.Category.BasePath, base.Name);

        public bool IsTypeUsed(string typePath)
        {
            foreach (var item in base.TableInfo.Columns)
            {
                if (item.DataType == typePath)
                    return true;
            }

            return false;
        }

        public IEnumerable<Type> GetTypes()
        {
            var types = this.GetService(typeof(TypeCollection)) as TypeCollection;
            var query = from item in base.TableInfo.Columns
                        where NameValidator.VerifyItemPath(item.DataType)
                        let itemName = new ItemName(item.DataType)
                        select types[itemName.Name, itemName.CategoryPath];
            return query.Distinct();
        }

        public IEnumerable<Table> GetRelations()
        {
            var table = this;
            while (table.Parent != null)
            {
                table = table.Parent;
            };

            return EnumerableUtility.FamilyTree(table, item => item.Childs);
        }

        public void ValidateIsNotBeingEdited()
        {
            if (this.IsBeingEdited == true)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingSetup_Format, base.Name));
            if (this.Content.Domain != null)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingEdited_Format, base.Name));
        }

        public void ValidateHasNotBeingEditedType()
        {
            var typeContext = this.GetService(typeof(TypeContext)) as TypeContext;

            foreach (var item in base.TableInfo.Columns)
            {
                if (NameValidator.VerifyItemPath(item.DataType) == false)
                    continue;
                var type = typeContext[item.DataType] as Type;
                if (type.IsBeingEdited == true)
                    throw new InvalidOperationException(string.Format(Resources.Exception_TypeIsBeingEdited_Format, type.Name));
            }
        }

        public async Task<CremaDataSet> ReadDataForContentAsync(Authentication authentication, ItemName targetName)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var targetItemPaths = new string[]
                {
                    this.Context.GenerateCategoryPath(targetName.CategoryPath),
                    this.Context.GeneratePath(targetName),
                };
                var tables = this.GetRelations().Distinct().OrderBy(item => item.Name).ToArray();
                var types = tables.SelectMany(item => item.GetTypes()).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.ItemPath).ToArray();
                var tableItemPaths = tables.Select(item => item.ItemPath).ToArray();
                var itemPaths = typeItemPaths.Concat(tableItemPaths).Concat(targetItemPaths).Distinct().ToArray();
                var props = new CremaDataSetSerializerSettings(authentication, typeItemPaths, tableItemPaths);
                var itemPath = this.ItemPath;
                return (itemPaths, props, itemPath);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.itemPaths);
                var dataSet = this.Serializer.Deserialize(tuple.itemPath, typeof(CremaDataSet), tuple.props) as CremaDataSet;
                dataSet.SetItemPaths(tuple.itemPaths);
                return dataSet;
            });
        }

        public async Task<CremaDataSet> ReadDataForCopyAsync(Authentication authentication, ItemName targetName)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var targetItemPaths = new string[]
                {
                    this.Context.GenerateCategoryPath(targetName.CategoryPath),
                    this.Context.GeneratePath(targetName),
                };
                var tables = this.CollectChilds().OrderBy(item => item.Name).ToArray();
                var types = tables.SelectMany(item => item.GetTypes()).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.ItemPath).ToArray();
                var tableItemPaths = tables.Select(item => item.ItemPath).ToArray();
                var itemPaths = typeItemPaths.Concat(tableItemPaths).Concat(targetItemPaths).Distinct().ToArray();
                var props = new CremaDataSetSerializerSettings(authentication, typeItemPaths, tableItemPaths);
                var itemPath = this.ItemPath;
                return (itemPaths, props, itemPath);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.itemPaths);
                var dataSet = this.Serializer.Deserialize(tuple.itemPath, typeof(CremaDataSet), tuple.props) as CremaDataSet;
                dataSet.SetItemPaths(tuple.itemPaths);
                return dataSet;
            });
        }

        public async Task<CremaDataSet> ReadDataForPathAsync(Authentication authentication, ItemName targetName)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var targetItemPaths = new string[]
                {
                    this.Context.GenerateCategoryPath(targetName.CategoryPath),
                    this.Context.GeneratePath(targetName),
                };
                var typeCollection = this.GetService(typeof(TypeCollection)) as TypeCollection;
                var tables = this.Collect().OrderBy(item => item.Name).ToArray();
                var types = tables.SelectMany(item => item.GetTypes()).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.ItemPath).ToArray();
                var tableItemPaths = tables.Select(item => item.ItemPath).ToArray();
                var itemPaths = typeItemPaths.Concat(tableItemPaths).Concat(targetItemPaths).Distinct().ToArray();
                var props = new CremaDataSetSerializerSettings(authentication, typeItemPaths, tableItemPaths);
                var itemPath = this.ItemPath;
                return (itemPaths, props, itemPath);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.itemPaths);
                var dataSet = this.Serializer.Deserialize(tuple.itemPath, typeof(CremaDataSet), tuple.props) as CremaDataSet;
                dataSet.SetItemPaths(tuple.itemPaths);
                return dataSet;
            });
        }

        public async Task<CremaDataSet> ReadDataForTemplateAsync(Authentication authentication, bool allTypes)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var typeCollection = this.GetService(typeof(TypeCollection)) as TypeCollection;
                var tables = this.Collect().OrderBy(item => item.Name).ToArray();
                var types = allTypes == true ? typeCollection.ToArray<Type>() : tables.SelectMany(item => item.GetTypes()).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.ItemPath).ToArray();
                var tableItemPaths = tables.Select(item => item.ItemPath).ToArray();
                var itemPaths = typeItemPaths.Concat(tableItemPaths).ToArray();
                var props = new CremaDataSetSerializerSettings(authentication, typeItemPaths, tableItemPaths);
                var itemPath = this.ItemPath;
                return (itemPaths, props, itemPath);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.itemPaths);
                var dataSet = this.Serializer.Deserialize(tuple.itemPath, typeof(CremaDataSet), tuple.props) as CremaDataSet;
                dataSet.SetItemPaths(tuple.itemPaths);
                return dataSet;
            });
        }

        private IEnumerable<Table> Collect()
        {
            yield return this;

            foreach (var item in this.Childs)
            {
                foreach (var i in item.Collect())
                {
                    yield return i;
                }
            }

            foreach (var item in this.DerivedTables)
            {
                yield return item;
            }
        }

        private IEnumerable<Table> CollectChilds()
        {
            yield return this;

            foreach (var item in this.Childs)
            {
                foreach (var i in item.CollectChilds())
                {
                    yield return i;
                }
            }
        }

        public void ValidateNewChild(Authentication authentication)
        {
            if (this.TemplatedParent != null)
                throw new InvalidOperationException(Resources.Exception_InheritedTableCannotNewChild);
            this.ValidateAccessType(authentication, AccessType.Master);
            this.OnValidateNewChild(authentication, this);
        }

        public void ValidateLockInternal(Authentication authentication)
        {
            base.ValidateLock(authentication);
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
            template.EditCanceled += Template_EditCanceled;
            template.EditEnded += Template_EditEnded;
            this.templateList.Add(template);
        }

        public TableTemplate Template { get; }

        public TableContent Content { get; }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public new string Name => base.Name;

        public new string TableName => base.TableName;

        public new string Path => base.Path;

        public new bool IsLocked => base.IsLocked;

        public new bool IsPrivate => base.IsPrivate;

        public new AccessInfo AccessInfo => base.AccessInfo;

        public new LockInfo LockInfo => base.LockInfo;

        public new TableInfo TableInfo => base.TableInfo;

        public new TableState TableState => base.TableState;

        public new TagInfo Tags => base.Tags;

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

        public new event EventHandler TableInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.TableInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.TableInfoChanged -= value;
            }
        }

        public new event EventHandler TableStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.TableStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.TableStateChanged -= value;
            }
        }

        private void Template_EditEnded(object sender, EventArgs e)
        {
            this.templateList.Remove(sender as NewTableTemplate);
        }

        private void Template_EditCanceled(object sender, EventArgs e)
        {
            this.templateList.Remove(sender as NewTableTemplate);
        }

        #region Invisibles

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateLock(IAuthentication authentication, object target)
        {
            base.OnValidateLock(authentication, target);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateUnlock(IAuthentication authentication, object target)
        {
            base.OnValidateUnlock(authentication, target);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateSetPublic(IAuthentication authentication, object target)
        {
            base.OnValidateSetPublic(authentication, target);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateSetPrivate(IAuthentication authentication, object target)
        {
            base.OnValidateSetPrivate(authentication, target);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateAddAccessMember(IAuthentication authentication, object target, string memberID, AccessType accessType)
        {
            base.OnValidateAddAccessMember(authentication, target, memberID, accessType);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateRemoveAccessMember(IAuthentication authentication, object target)
        {
            base.OnValidateRemoveAccessMember(authentication, target);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateRename(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateRename(authentication, target, oldPath, newPath);
            if (target == this)
            {
                var itemName = new ItemName(newPath);
                if (this.TableName == itemName.Name)
                    throw new ArgumentException(Resources.Exception_SameName, nameof(newPath));
            }
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotRenameOnCreateChildTable);
            this.ValidateIsNotBeingEdited();
            this.ValidateHasNotBeingEditedType();

            if (this.Parent == null)
            {
                var itemName = new ItemName(Regex.Replace(this.Path, $"^{oldPath}", newPath));
                this.Context.ValidateTablePath(itemName.CategoryPath, itemName.Name, this.TemplatedParent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateMove(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateMove(authentication, target, oldPath, newPath);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotMoveOnCreateChildTable);
            this.ValidateIsNotBeingEdited();
            this.ValidateHasNotBeingEditedType();

            if (this.Parent == null)
            {
                var itemName = new ItemName(Regex.Replace(this.Path, $"^{oldPath}", newPath));
                this.Context.ValidateTablePath(itemName.CategoryPath, itemName.Name, this.TemplatedParent);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateDelete(IAuthentication authentication, object target)
        {
            base.OnValidateDelete(authentication, target);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteOnCreateChildTable);
            this.ValidateIsNotBeingEdited();
            this.ValidateHasNotBeingEditedType();

            if (this.IsBaseTemplate == true && this.Parent == null && target == this)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteBaseTemplateTable);
            if (this.Childs.Any() == true)
                throw new InvalidOperationException("Cannot delete table with childs");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnValidateNewChild(IAuthentication authentication, object target)
        {
            this.ValidateIsNotBeingEdited();
            this.ValidateHasNotBeingEditedType();

            foreach (var item in this.Childs)
            {
                item.OnValidateNewChild(authentication, target);
            }

            foreach (var item in this.DerivedTables)
            {
                item.OnValidateNewChild(authentication, null);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnValidateRevert(IAuthentication authentication, object target)
        {
            this.ValidateIsNotBeingEdited();
            this.ValidateHasNotBeingEditedType();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            accessInfo.Path = this.Path;
            base.AccessInfo = accessInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetTableState(TableState tableState)
        {
            base.TableState = tableState;
        }

        #endregion

        #region ITable

        async Task<ITable> ITable.CopyAsync(Authentication authentication, string newTableName, string categoryPath, bool copyContent)
        {
            return await this.CopyAsync(authentication, newTableName, categoryPath, copyContent);
        }

        async Task<ITable> ITable.InheritAsync(Authentication authentication, string newTableName, string categoryPath, bool copyContent)
        {
            return await this.InheritAsync(authentication, newTableName, categoryPath, copyContent);
        }

        async Task<ITableTemplate> ITable.NewTableAsync(Authentication authentication)
        {
            return await this.NewChildAsync(authentication);
        }

        ITable ITable.TemplatedParent => this.TemplatedParent;

        ITableCategory ITable.Category => this.Category;

        ITable ITable.Parent => this.Parent;

        ITableTemplate ITable.Template => this.Template;

        ITableContent ITable.Content => this.Content;

        IContainer<ITable> ITable.Childs => this.Childs;

        IContainer<ITable> ITable.DerivedTables => this.DerivedTables;

        #endregion

        #region ITableItem

        ITableItem ITableItem.Parent
        {
            get
            {
                if (this.Parent == null)
                    return this.Category;
                return this.Parent;
            }
        }

        IEnumerable<ITableItem> ITableItem.Childs => this.Childs;

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => this.TableInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.TableState;

        #endregion
    }
}

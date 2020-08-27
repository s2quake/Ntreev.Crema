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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 0612

namespace JSSoft.Crema.Services.Data
{
    class TableCollection : TableCollectionBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        ITableCollection
    {
        private ItemsCreatedEventHandler<ITable> tablesCreated;
        private ItemsRenamedEventHandler<ITable> tablesRenamed;
        private ItemsMovedEventHandler<ITable> tablesMoved;
        private ItemsDeletedEventHandler<ITable> tablesDeleted;
        private ItemsChangedEventHandler<ITable> tablesChanged;
        private ItemsEventHandler<ITable> tablesStateChanged;

        public TableCollection()
        {

        }

        public Table AddNew(Authentication authentication, string name, string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            if (NameValidator.VerifyName(name) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, name), nameof(name));
            return this.BaseAddNew(name, categoryPath, authentication);
        }

        public async Task<Table[]> AddNewAsync(Authentication authentication, DataBaseSet dataBaseSet)
        {
            var dataTables = dataBaseSet.TablesToCreate;
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in dataTables)
                {
                    this.ValidateAddNew(item.Name, item.CategoryPath, authentication);
                }
            });
            var dataSet = dataBaseSet.DataSet;
            var itemPaths = dataTables.Select(item => item.FullPath).ToArray();
            var tableList = new List<Table>(dataTables.Length);
            var tablePaths = dataTables.Select(item => item.Path).ToArray();
            await this.InvokeTableCreateAsync(authentication, tablePaths, dataBaseSet);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in dataTables)
                {
                    var table = this.AddNew(authentication, item.Name, item.CategoryPath);
                    if (item.TemplatedParentName != string.Empty)
                        table.TemplatedParent = this[item.TemplatedParentName];
                    table.Initialize(item.TableInfo);
                    tableList.Add(table);
                }
                this.InvokeTablesCreatedEvent(authentication, tableList.ToArray(), dataSet);
            });
            return tableList.ToArray();
        }

        public async Task<Table[]> InheritAsync(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyContent)
        {
            try
            {
                this.ValidateExpired();
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(InheritAsync), this, table, newTableName, categoryPath, copyContent);
                    this.ValidateInherit(authentication, table, newTableName, categoryPath, copyContent);
                    return table.Path;
                });
                var taskID = GuidUtility.FromName(nameof(InheritAsync) + categoryPath + newTableName + copyContent);
                var itemName = new ItemName(path);
                var targetName = new ItemName(categoryPath, newTableName);
                var dataSet = await table.ReadDataForCopyAsync(authentication, categoryPath);
                var dataTable = dataSet.Tables[itemName.Name, itemName.CategoryPath];
                var dataTables = dataSet.Tables.ToArray();
                var newDataTable = dataTable.Inherit(targetName, copyContent);
                newDataTable.CategoryPath = categoryPath;
                using var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, DataBaseSetOptions.AllowTableCreation);
                var tables = await this.AddNewAsync(authentication, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() => this.DataBase.InvokeTaskCompletedEvent(authentication, taskID));
                return tables;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        // TODO: 상속된 테이블 복사시 에러가 발생
        public async Task<Table[]> CopyAsync(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyContent)
        {
            try
            {
                this.ValidateExpired();
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CopyAsync), this, table, newTableName, categoryPath, copyContent);
                    this.ValidateCopy(authentication, table, newTableName, categoryPath, copyContent);
                    return table.Path;
                });
                var taskID = GuidUtility.FromName(nameof(CopyAsync) + categoryPath + newTableName + copyContent);
                var itemName = new ItemName(path);
                var targetName = new ItemName(categoryPath, newTableName);
                var dataSet = await table.ReadDataForCopyAsync(authentication, categoryPath);
                var dataTable = dataSet.Tables[itemName.Name, itemName.CategoryPath];
                var dataTables = dataSet.Tables.ToArray();
                var newDataTable = dataTable.Copy(targetName, copyContent);
                newDataTable.CategoryPath = categoryPath;
                using var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, DataBaseSetOptions.AllowTableCreation);
                var tables = await this.AddNewAsync(authentication, dataBaseSet);
                await this.Dispatcher.InvokeAsync(() => this.DataBase.InvokeTaskCompletedEvent(authentication, taskID));
                return tables;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<CremaDataSet> ReadDataForContentAsync(Authentication authentication, Table[] tables)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var types = tables.SelectMany(item => item.GetTypes()).Distinct().ToArray();
                var typePaths = types.Select(item => item.FullPath).ToArray();
                var tablePaths = tables.Select(item => item.FullPath).ToArray();
                var fullPaths = typePaths.Concat(tablePaths).ToArray();
                return (tablePaths, typePaths, fullPaths);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(authentication, this, nameof(ReadDataForContentAsync), tuple.tablePaths);
                var dataSet = this.Repository.ReadDataSet(authentication, tuple.fullPaths);
                dataSet.SetItemPaths(tuple.tablePaths);
                return dataSet;
            });
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public Task InvokeTableCreateAsync(Authentication authentication, string[] tablePaths, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.CreateTable(authentication, tablePaths);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.CreateTable(dataBaseSet, tablePaths);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTableRenameAsync(Authentication authentication, TableInfo tableInfo, string newName, DataBaseSet dataBaseSet)
        {
            var name = tableInfo.ParentName == string.Empty ? newName : tableInfo.ParentName + "." + newName;
            var message = EventMessageBuilder.RenameTable(authentication, tableInfo.Name, name);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.RenameTable(dataBaseSet, tableInfo.Path, name);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTableMoveAsync(Authentication authentication, TableInfo tableInfo, string newCategoryPath, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.MoveTable(authentication, tableInfo.Name, newCategoryPath, tableInfo.CategoryPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.MoveTable(dataBaseSet, tableInfo.Path, newCategoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTableDeleteAsync(Authentication authentication, TableInfo tableInfo, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.DeleteTable(authentication, tableInfo.Name);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.DeleteTable(dataBaseSet, tableInfo.Path);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTableEndContentEditAsync(Authentication authentication, Table[] tables, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.ChangeTableContent(authentication, tables);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.ModifyTable(dataBaseSet);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTableEndTemplateEditAsync(Authentication authentication, TableInfo tableInfo, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.ChangeTableTemplate(authentication, tableInfo.Name);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.ModifyTable(dataBaseSet);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public void InvokeTablesCreatedEvent(Authentication authentication, Table[] tables, CremaDataSet dataSet)
        {
            var args = tables.Select(item => (object)item.TableInfo).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesCreatedEvent), tables);
            var message = EventMessageBuilder.CreateTable(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesCreated(new ItemsCreatedEventArgs<ITable>(authentication, tables, args, dataSet));
            this.Context.InvokeItemsCreatedEvent(authentication, tables, args, dataSet);
        }

        public void InvokeTablesRenamedEvent(Authentication authentication, Table[] tables, string[] oldNames, string[] oldPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesRenamedEvent), tables, oldNames, oldPaths);
            var message = EventMessageBuilder.RenameTable(authentication, tables, oldNames);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesRenamed(new ItemsRenamedEventArgs<ITable>(authentication, tables, oldNames, oldPaths, dataSet));
            this.Context.InvokeItemsRenamedEvent(authentication, tables, oldNames, oldPaths, dataSet);
        }

        public void InvokeTablesMovedEvent(Authentication authentication, Table[] tables, string[] oldPaths, string[] oldCategoryPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesMovedEvent), tables, oldPaths, oldCategoryPaths);
            var message = EventMessageBuilder.MoveTable(authentication, tables, oldCategoryPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesMoved(new ItemsMovedEventArgs<ITable>(authentication, tables, oldPaths, oldCategoryPaths, dataSet));
            this.Context.InvokeItemsMovedEvent(authentication, tables, oldPaths, oldCategoryPaths, dataSet);
        }

        public void InvokeTablesDeletedEvent(Authentication authentication, Table[] tables, string[] oldPaths)
        {
            var dataSet = CremaDataSet.Create(new SignatureDateProvider(authentication.ID));
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesDeletedEvent), oldPaths);
            var message = EventMessageBuilder.DeleteTable(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesDeleted(new ItemsDeletedEventArgs<ITable>(authentication, tables, oldPaths, dataSet));
            this.Context.InvokeItemsDeletedEvent(authentication, tables, oldPaths, dataSet);
        }

        public void InvokeTablesStateChangedEvent(Authentication authentication, Table[] tables)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeTablesStateChangedEvent), tables);
            this.OnTablesStateChanged(new ItemsEventArgs<ITable>(authentication, tables));
        }

        public void InvokeTablesTemplateChangedEvent(Authentication authentication, Table[] tables, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesTemplateChangedEvent), tables);
            var message = EventMessageBuilder.ChangeTableTemplate(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesChanged(new ItemsChangedEventArgs<ITable>(authentication, tables, dataSet, DomainItemType.TableTemplate));
            this.Context.InvokeItemsChangedEvent(authentication, tables, dataSet);
        }

        public void InvokeTablesContentChangedEvent(Authentication authentication, Table[] tables, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesContentChangedEvent), tables);
            var message = EventMessageBuilder.ChangeTableContent(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesChanged(new ItemsChangedEventArgs<ITable>(authentication, tables, dataSet, DomainItemType.TableContent));
            this.Context.InvokeItemsChangedEvent(authentication, tables, dataSet);
        }

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<ITable> TablesCreated
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesCreated += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<ITable> TablesRenamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesRenamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<ITable> TablesMoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesMoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<ITable> TablesDeleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesDeleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesDeleted -= value;
            }
        }

        public event ItemsChangedEventHandler<ITable> TablesChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesChanged -= value;
            }
        }

        public event ItemsEventHandler<ITable> TablesStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.tablesStateChanged -= value;
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.CollectionChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.CollectionChanged -= value;
            }
        }

        protected override void ValidateAddNew(string name, string categoryPath, object validation)
        {
            base.ValidateAddNew(name, categoryPath, validation);

            if (NameValidator.VerifyName(name) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, name), nameof(name));

            if (validation is Authentication authentication)
            {
                var category = this.GetCategory(categoryPath);
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                category.ValidateAccessType(authentication, AccessType.Master);
            }
        }

        protected virtual void OnTablesCreated(ItemsCreatedEventArgs<ITable> e)
        {
            this.tablesCreated?.Invoke(this, e);
        }

        protected virtual void OnTablesRenamed(ItemsRenamedEventArgs<ITable> e)
        {
            this.tablesRenamed?.Invoke(this, e);
        }

        protected virtual void OnTablesMoved(ItemsMovedEventArgs<ITable> e)
        {
            this.tablesMoved?.Invoke(this, e);
        }

        protected virtual void OnTablesDeleted(ItemsDeletedEventArgs<ITable> e)
        {
            this.tablesDeleted?.Invoke(this, e);
        }

        protected virtual void OnTablesStateChanged(ItemsEventArgs<ITable> e)
        {
            this.tablesStateChanged?.Invoke(this, e);
        }

        protected virtual void OnTablesChanged(ItemsChangedEventArgs<ITable> e)
        {
            this.tablesChanged?.Invoke(this, e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher?.VerifyAccess();
            base.OnCollectionChanged(e);
        }

        protected override Table NewItem(params object[] args)
        {
            return new Table();
        }

        private void ValidateInherit(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyXml)
        {
            table.ValidateAccessType(authentication, AccessType.Master);

            if (this.Contains(newTableName) == true)
                throw new ArgumentException(Resources.Exception_SameTableNameExist, nameof(newTableName));
            if (table.Parent != null)
                throw new InvalidOperationException(Resources.Exception_ChildTableCannotInherit);
            if (table.TemplatedParent != null)
                throw new InvalidOperationException(Resources.Exception_InheritedTableCannotInherit);

            NameValidator.ValidateCategoryPath(categoryPath);

            if (this.Context.Categories.Contains(categoryPath) == false)
                throw new CategoryNotFoundException(categoryPath);

            var category = this.Context.Categories[categoryPath];
            category.ValidateAccessType(authentication, AccessType.Master);

            if (copyXml == true)
            {
                foreach (var item in EnumerableUtility.Friends(table, table.Childs))
                {
                    item.ValidateHasNotBeingEditedType();
                }
            }
        }

        private void ValidateCopy(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyXml)
        {
            table.ValidateAccessType(authentication, AccessType.Master);

            if (this.Contains(newTableName) == true)
                throw new ArgumentException(Resources.Exception_SameTableNameExist, nameof(newTableName));
            if (table.Parent != null)
                throw new InvalidOperationException(Resources.Exception_ChildTableCannotCopy);

            NameValidator.ValidateCategoryPath(categoryPath);

            if (this.Context.Categories.Contains(categoryPath) == false)
                throw new CategoryNotFoundException(categoryPath);

            var category = this.Context.Categories[categoryPath];
            category.ValidateAccessType(authentication, AccessType.Master);

            if (copyXml == true)
            {
                foreach (var item in EnumerableUtility.Friends(table, table.Childs))
                {
                    item.ValidateHasNotBeingEditedType();
                }
            }
        }

        #region ITableCollection

        bool ITableCollection.Contains(string tableName)
        {
            return this.Contains(tableName);
        }

        ITable ITableCollection.this[string tableName] => this[tableName];

        #endregion

        #region IEnumerable

        IEnumerator<ITable> IEnumerable<ITable>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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

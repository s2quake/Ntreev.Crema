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
using JSSoft.Crema.ServiceHosts.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

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
            if (NameValidator.VerifyName(name) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, name), nameof(name));
            return this.BaseAddNew(name, categoryPath, authentication);
        }

        public Task<Table[]> AddNewAsync(Authentication authentication, TableInfo[] tableInfos)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var tableList = new List<Table>(tableInfos.Length);
                foreach (var item in tableInfos)
                {
                    var table = this.AddNew(authentication, item.Name, item.CategoryPath);
                    if (item.TemplatedParent != string.Empty)
                        table.TemplatedParent = this[item.TemplatedParent];
                    table.Initialize(item);
                }
                this.InvokeTablesCreatedEvent(authentication, tableList.ToArray());
                return tableList.ToArray();
            });
        }

        public async Task<Table[]> InheritAsync(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyContent)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(InheritAsync), this, table, newTableName, categoryPath, copyContent);
                    return table.Name;
                });
                var taskID = GuidUtility.FromName(nameof(InheritAsync) + categoryPath + newTableName + copyContent);
                var result = await this.Service.InheritTableAsync(authentication.Token, name, newTableName, categoryPath, copyContent);
                var tableInfos = result.Value;
                await this.DataBase.WaitAsync(taskID);
                return await this.Dispatcher.InvokeAsync(() => tableInfos.Select(item => this[item.Name]).ToArray());

            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Table[]> CopyAsync(Authentication authentication, Table table, string newTableName, string categoryPath, bool copyContent)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CopyAsync), this, table, newTableName, categoryPath, copyContent);
                    return table.Name;
                });
                var taskID = GuidUtility.FromName(nameof(CopyAsync) + categoryPath + newTableName + copyContent);
                var result = await this.Service.CopyTableAsync(authentication.Token, name, newTableName, categoryPath, copyContent);
                var tableInfos = result.Value;
                await this.DataBase.WaitAsync(taskID);
                return await this.Dispatcher.InvokeAsync(() => tableInfos.Select(item => this[item.Name]).ToArray());
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

        public void InvokeTablesCreatedEvent(Authentication authentication, Table[] tables)
        {
            var args = tables.Select(item => (object)item.TableInfo).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesCreatedEvent), tables);
            var message = EventMessageBuilder.CreateTable(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesCreated(new ItemsCreatedEventArgs<ITable>(authentication, tables, args));
            this.Context.InvokeItemsCreatedEvent(authentication, tables, args);
        }

        public void InvokeTablesRenamedEvent(Authentication authentication, Table[] tables, string[] oldNames, string[] oldPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesRenamedEvent), tables, oldNames, oldPaths);
            var message = EventMessageBuilder.RenameTable(authentication, tables, oldNames);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesRenamed(new ItemsRenamedEventArgs<ITable>(authentication, tables, oldNames, oldPaths));
            this.Context.InvokeItemsRenamedEvent(authentication, tables, oldNames, oldPaths);
        }

        public void InvokeTablesMovedEvent(Authentication authentication, Table[] tables, string[] oldPaths, string[] oldCategoryPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesMovedEvent), tables, oldPaths, oldCategoryPaths);
            var message = EventMessageBuilder.MoveTable(authentication, tables, oldCategoryPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesMoved(new ItemsMovedEventArgs<ITable>(authentication, tables, oldPaths, oldCategoryPaths));
            this.Context.InvokeItemsMovedEvent(authentication, tables, oldPaths, oldCategoryPaths);
        }

        public void InvokeTablesDeletedEvent(Authentication authentication, Table[] tables, string[] oldPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesDeletedEvent), oldPaths);
            var message = EventMessageBuilder.DeleteTable(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesDeleted(new ItemsDeletedEventArgs<ITable>(authentication, tables, oldPaths));
            this.Context.InvokeItemsDeleteEvent(authentication, tables, oldPaths);
        }

        public void InvokeTablesStateChangedEvent(Authentication authentication, Table[] tables)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeTablesStateChangedEvent), tables);
            this.OnTablesStateChanged(new ItemsEventArgs<ITable>(authentication, tables));
        }

        public void InvokeTablesTemplateChangedEvent(Authentication authentication, Table[] tables)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesTemplateChangedEvent), tables);
            var message = EventMessageBuilder.ChangeTableTemplate(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesChanged(new ItemsChangedEventArgs<ITable>(authentication, tables, null, DomainItemType.TableTemplate));
            this.Context.InvokeItemsChangedEvent(authentication, tables);
        }

        public void InvokeTablesContentChangedEvent(Authentication authentication, Table[] tables)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTablesContentChangedEvent), tables);
            var message = EventMessageBuilder.ChangeTableContent(authentication, tables);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTablesChanged(new ItemsChangedEventArgs<ITable>(authentication, tables, null, DomainItemType.TableContent));
            this.Context.InvokeItemsChangedEvent(authentication, tables);
        }

        public IDataBaseService Service => this.Context.Service;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

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

        private void Sign(Authentication authentication, ResultBase result)
        {
            result.Validate(authentication);
        }

        private void Sign<T>(Authentication authentication, ResultBase<T> result)
        {
            result.Validate(authentication);
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

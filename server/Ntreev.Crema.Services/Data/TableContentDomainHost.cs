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
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Domains;
using Ntreev.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    partial class TableContent
    {
        private TableContentDomainHost domainHost;

        public string[] Editors => this.domainHost != null ? this.domainHost.Editors : new string[] { };

        public string Owner => this.domainHost != null ? this.domainHost.Owner : string.Empty;

        public class TableContentDomainHost : IDomainHost, IEnumerable<ITableContent>
        {
            private readonly string path;
            private Domain domain;
            private DataBaseSet dataBaseSet;
            private CremaDataSet dataSet;
            private string[] itemPaths;
            private string[] editors;
            private string owner;

            public TableContentDomainHost(TableCollection container, Table[] tables)
            {
                this.Container = container;
                this.Tables = tables;
                this.Contents = tables.Select(item => item.Content).ToArray();
                this.path = string.Join("|", tables.Select(item => item.Path));
            }

            public TableContentDomainHost(TableCollection container, string itemPath)
            {
                var items = StringUtility.Split(itemPath, '|');
                var tableList = new List<Table>(items.Length);
                var dataBase = container.DataBase;
                foreach (var item in items)
                {
                    if (dataBase.TableContext[item] is Table table)
                    {
                        tableList.Add(table);
                    }
                }
                this.Container = container;
                this.Tables = tableList.ToArray();
                this.Contents = tableList.Select(item => item.Content).ToArray();
                this.path = itemPath;
            }

            public void InvokeEditBegunEvent(EventArgs e)
            {
                foreach (var item in this.Contents)
                {
                    item.OnEditBegun(e);
                }
            }

            public void InvokeEditEndedEvent(EventArgs e)
            {
                foreach (var item in this.Contents)
                {
                    item.OnEditEnded(e);
                }
            }

            public void InvokeEditCanceledEvent(EventArgs e)
            {
                foreach (var item in this.Contents)
                {
                    item.OnEditCanceled(e);
                }
            }

            public void Release()
            {
                foreach (var item in this.Contents)
                {
                    item.domainHost = null;
                }
            }

            public async Task BeginContentAsync(Authentication authentication)
            {
                try
                {
                    this.dataSet = await this.Container.ReadDataForContentAsync(authentication, this.Tables);
                    this.domain = new TableContentDomain(authentication, dataSet, this.DataBase, this.path, typeof(TableContent).Name, this);
                    this.dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, false, false);
                    this.itemPaths = this.dataSet.GetItemPaths();
                    await this.DomainContext.AddAsync(authentication, this.domain, this.DataBase);
                }
                catch
                {
                    if (this.dataBaseSet != null)
                        await this.Repository.UnlockAsync(this.dataBaseSet.ItemPaths);
                    this.dataSet = null;
                    this.domain = null;
                    this.dataBaseSet = null;
                    throw;
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.Domain = this.domain;
                        item.domainHost = this;
                        item.DataTable = dataSet.Tables[item.Table.Name, item.Table.Category.Path];
                        item.Table.TableState = TableState.IsBeingEdited;
                        item.IsModified = domain.ModifiedTables.Contains(item.dataTable.Name);
                    }
                });
                await this.domain.Dispatcher.InvokeAsync(this.AttachDomainEvent);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public async Task<TableInfo[]> EndContentAsync(Authentication authentication, TableInfo[] tableInfos)
            {
                if (this.domain.IsModified == true)
                {
                    await this.Container.InvokeTableEndContentEditAsync(authentication, this.Tables, this.dataBaseSet);
                }
                else
                {
                    await this.Repository.UnlockAsync(this.dataBaseSet.ItemPaths);
                }
                tableInfos = this.Contents.Where(item => item.IsModified).Select(item => item.DataTable.TableInfo).ToArray();
                if (this.domain != null)
                {
                    await this.domain.Dispatcher.InvokeAsync(this.DetachDomainEvent);
                    await this.DomainContext.RemoveAsync(authentication, this.domain, false, tableInfos);
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var tables = this.Contents.Where(item => item.IsModified).Select(item => item.Table).ToArray();
                    foreach (var item in this.Contents)
                    {
                        if (item.IsModified == true)
                        {
                            var tableInfo = item.DataTable.TableInfo;
                            item.Table.UpdateContent(item.DataTable.TableInfo);
                        }
                        item.Domain = null;
                        item.IsModified = false;
                        item.DataTable = null;
                        item.Table.TableState = TableState.None;
                    }
                    this.editors = null;
                    this.owner = null;
                    if (tables.Any() == true)
                        this.Container.InvokeTablesContentChangedEvent(authentication, tables, dataSet);
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
                return tableInfos;
            }

            public async Task CancelContentAsync(Authentication authentication)
            {
                if (this.domain != null)
                {
                    await this.domain.Dispatcher.InvokeAsync(this.DetachDomainEvent);
                    await this.DomainContext.RemoveAsync(authentication, this.domain, true, null);
                }
                await this.Repository.UnlockAsync(this.itemPaths);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.Domain = null;
                        item.IsModified = false;
                        item.DataTable = null;
                        item.Table.TableState = TableState.None;
                    }
                    this.editors = null;
                    this.owner = null;
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public void EnterContent(Authentication authentication)
            {

            }

            public void LeaveContent(Authentication authentication)
            {

            }

            public void SetServiceState(ServiceState serviceState)
            {
                foreach (var item in this.Contents)
                {
                    item.ServiceState = serviceState;
                }
            }

            public Table[] Tables { get; }

            public TableContent[] Contents { get; }

            public DataBase DataBase => this.Container.DataBase;

            public string[] Editors => this.editors ?? new string[] { };

            public string Owner => this.owner ?? string.Empty;

            public TableCollection Container { get; }

            private async void Domain_RowAdded(object sender, DomainRowEventArgs e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var query = from row in e.Rows
                                join content in this.Contents on row.TableName equals content.dataTable.Name
                                select content;
                    foreach (var item in query)
                    {
                        item.IsModified = true;
                        item.OnChanged(e);
                    }
                });
            }

            private async void Domain_RowChanged(object sender, DomainRowEventArgs e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var query = from row in e.Rows
                                join content in this.Contents on row.TableName equals content.dataTable.Name
                                select content;
                    foreach (var item in query)
                    {
                        item.IsModified = true;
                        item.OnChanged(e);
                    }
                });
            }

            private async void Domain_RowRemoved(object sender, DomainRowEventArgs e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var query = from row in e.Rows
                                join content in this.Contents on row.TableName equals content.dataTable.Name
                                select content;
                    foreach (var item in query)
                    {
                        item.IsModified = true;
                        item.OnChanged(e);
                    }
                });
            }

            private void Domain_PropertyChanged(object sender, DomainPropertyEventArgs e)
            {

            }

            private async void Domain_UserAdded(object sender, DomainUserEventArgs e)
            {
                this.RefreshEditors();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.OnEditorsChanged(e);
                    }
                });
            }

            private void Domain_OwnerChanged(object sender, DomainUserEventArgs e)
            {
                this.owner = this.domain.Users.OwnerUserID;

            }

            private async void Domain_UserChanged(object sender, DomainUserEventArgs e)
            {
                this.RefreshEditors();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.OnEditorsChanged(e);
                    }
                });
            }

            private async void Domain_UserRemoved(object sender, DomainUserRemovedEventArgs e)
            {
                this.RefreshEditors();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.OnEditorsChanged(e);
                    }
                });
            }

            int refcount;

            private void AttachDomainEvent()
            {
                this.domain.Dispatcher.VerifyAccess();
                if (refcount != 0)
                {
                    System.Diagnostics.Debugger.Launch();
                }
                this.domain.RowAdded += Domain_RowAdded;
                this.domain.RowChanged += Domain_RowChanged;
                this.domain.RowRemoved += Domain_RowRemoved;
                this.domain.PropertyChanged += Domain_PropertyChanged;
                this.domain.UserAdded += Domain_UserAdded;
                this.domain.OwnerChanged += Domain_OwnerChanged;
                this.domain.UserRemoved += Domain_UserRemoved;
                refcount++;
            }

            

            private void DetachDomainEvent()
            {
                this.domain.Dispatcher.VerifyAccess();
                this.domain.RowAdded -= Domain_RowAdded;
                this.domain.RowChanged -= Domain_RowChanged;
                this.domain.RowRemoved -= Domain_RowRemoved;
                this.domain.PropertyChanged -= Domain_PropertyChanged;
                this.domain.UserAdded -= Domain_UserAdded;
                this.domain.OwnerChanged -= Domain_OwnerChanged;
                this.domain.UserRemoved -= Domain_UserRemoved;
                if (refcount != 1)
                {
                    System.Diagnostics.Debugger.Launch();
                }
                refcount--;
            }

            private void RefreshEditors()
            {
                this.domain.Dispatcher.VerifyAccess();
                this.editors = (from DomainUser item in this.domain.Users select item.ID).ToArray();
                this.owner = this.domain.Users.OwnerUserID;
            }

            private CremaDispatcher Dispatcher => this.Container.Dispatcher;

            private DomainContext DomainContext => this.Container.GetService(typeof(DomainContext)) as DomainContext;

            private DataBaseRepositoryHost Repository => this.Container.Repository;

            #region IDomainHost

            void IDomainHost.Detach()
            {
                this.domain.Dispatcher.Invoke(this.DetachDomainEvent);
                this.domain.Host = null;
                this.domain = null;
                foreach (var item in this.Contents)
                {
                    item.Domain = null;
                    item.dataTable = null;
                }
            }

            void IDomainHost.Attach(Domain domain)
            {
                this.dataSet = domain.Source as CremaDataSet;
                this.domain = domain;
                this.domain.Host = this;
                this.dataBaseSet = DataBaseSet.Create(this.DataBase, dataSet, false, false);
                this.itemPaths = this.dataSet.GetItemPaths();
                this.Repository.Dispatcher.Invoke(() => this.Repository.Lock(this.itemPaths));
                foreach (var item in this.Contents)
                {
                    item.domainHost = this;
                    item.Domain = domain;
                    item.DataTable = dataSet.Tables[item.Table.Name, item.Table.Category.Path];
                    item.Table.TableState = TableState.IsBeingEdited;
                    item.ServiceState = ServiceState.Opened;
                    item.IsModified = domain.ModifiedTables.Contains(item.dataTable.Name);
                }
                this.domain.Dispatcher.Invoke(this.AttachDomainEvent);
            }

            async Task<object> IDomainHost.DeleteAsync(Authentication authentication, bool isCanceled, object result)
            {
                if (isCanceled == false)
                {
                    var args = new DomainDeletedEventArgs(authentication, this.domain, isCanceled, result);
                    result = await this.EndContentAsync(authentication, result as TableInfo[]);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.SetServiceState(ServiceState.Closed);
                        this.InvokeEditEndedEvent(args);
                    });
                    return result;
                }
                else
                {
                    var args = new DomainDeletedEventArgs(authentication, this.domain, isCanceled, null);
                    await this.CancelContentAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.SetServiceState(ServiceState.Closed);
                        this.InvokeEditCanceledEvent(args);
                    });
                    return null;
                }
            }

            #endregion

            #region IEnumerable

            IEnumerator<ITableContent> IEnumerable<ITableContent>.GetEnumerator()
            {
                foreach (var item in this.Contents)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var item in this.Contents)
                {
                    yield return item;
                }
            }

            #endregion
        }
    }
}

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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    partial class TableContent
    {
        private TableContentGroup domainHost;

        public string[] Editors => this.domainHost != null ? this.domainHost.Editors : new string[] { };

        public string Owner => this.domainHost != null ? this.domainHost.Owner : string.Empty;

        public class TableContentGroup : IDomainHost, ITableContentGroup
        {
            private readonly string path;
            private Domain domain;
            private DataBaseSet dataBaseSet;
            private CremaDataSet dataSet;
            private string[] itemPaths;
            private string[] editors;
            private string owner;

            public TableContentGroup(TableCollection container, Table[] tables)
            {
                this.Container = container;
                this.Tables = tables;
                this.Contents = tables.Select(item => item.Content).ToArray();
                this.path = string.Join("|", tables.Select(item => item.Path));
            }

            public TableContentGroup(TableCollection container, string itemPath)
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

            public DomainAccessType GetAccessType(Authentication authentication)
            {
                var accessType = DomainAccessType.ReadWrite;
                foreach (var item in this.Contents)
                {
                    var itemAccessType = item.GetAccessType(authentication);
                    if (itemAccessType < accessType)
                    {
                        itemAccessType = accessType;
                    }
                }
                return accessType;
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
                    this.dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet);
                    this.itemPaths = this.dataSet.GetItemPaths();
                    await this.DomainContext.AddAsync(authentication, this.domain, this.DataBase);
                }
                catch
                {
                    if (this.dataBaseSet != null)
                        await this.Repository.UnlockAsync(authentication, this, nameof(BeginContentAsync), this.dataBaseSet.ItemPaths);
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

            public async Task EndContentAsync(Authentication authentication)
            {
                if (this.domain.IsModified == true)
                {
                    await this.Container.InvokeTableEndContentEditAsync(authentication, this.Tables, this.dataBaseSet);
                }
                var tableInfos = this.Contents.Where(item => item.IsModified).Select(item => item.DataTable.TableInfo).ToArray();
                var tableInfoByName = tableInfos.ToDictionary(item => item.Name);
                var taskID = this.domain.ID;
                this.domain.Result = tableInfos;
                if (this.domain.Host != null)
                {
                    await this.domain.Dispatcher.InvokeAsync(this.DetachDomainEvent);
                    await this.DomainContext.RemoveAsync(authentication, this.domain, false);
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var tables = this.Contents.Where(item => item.IsModified).Select(item => item.Table).ToArray();
                    foreach (var item in this.Contents)
                    {
                        item.Domain = null;
                        item.IsModified = false;
                        item.DataTable = null;
                        if (tableInfoByName.ContainsKey(item.Table.Name))
                            item.Table.UpdateContent(tableInfoByName[item.Table.Name]);
                        item.Table.TableState = TableState.None;
                    }
                    this.editors = null;
                    this.owner = null;
                    this.Container.InvokeTablesContentChangedEvent(authentication, tables, dataSet);
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
                await this.Repository.UnlockAsync(authentication, this, nameof(EndContentAsync), this.dataBaseSet.ItemPaths);
            }

            public async Task CancelContentAsync(Authentication authentication)
            {
                if (this.domain.Host != null)
                {
                    await this.domain.Dispatcher.InvokeAsync(this.DetachDomainEvent);
                    await this.DomainContext.RemoveAsync(authentication, this.domain, true);
                }
                await this.Repository.UnlockAsync(authentication, this, nameof(CancelContentAsync), this.itemPaths);
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

            public CremaDispatcher Dispatcher => this.Container.Dispatcher;

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



            private DomainContext DomainContext => this.Container.GetService(typeof(DomainContext)) as DomainContext;

            private DataBaseRepositoryHost Repository => this.Container.Repository;

            private CremaHost CremaHost => this.Container.CremaHost;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void ValidateEndEdit(Authentication authentication)
            {
                var isAdmin = authentication.Types.HasFlag(AuthenticationType.Administrator);
                foreach (var item in this.Contents)
                {
                    item.OnValidateEndEdit(authentication, item);
                }
                this.domain.Dispatcher?.Invoke(() =>
                {
                    var isOwner = this.domain.Users.OwnerUserID == authentication.ID;
                    if (isAdmin == false && isOwner == false)
                        throw new NotImplementedException();
                });
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void ValidateEnter(Authentication authentication)
            {
                foreach (var item in this.Contents)
                {
                    item.OnValidateEnter(authentication, item);
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void ValidateLeave(Authentication authentication)
            {
                foreach (var item in this.Contents)
                {
                    item.OnValidateLeave(authentication, item);
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void ValidateCancelEdit(Authentication authentication)
            {
                var isAdmin = authentication.Types.HasFlag(AuthenticationType.Administrator);
                foreach (var item in this.Contents)
                {
                    item.OnValidateCancelEdit(authentication, item);
                }
                this.domain.Dispatcher.Invoke(() =>
                {
                    if (isAdmin == false && this.domain.Users.Owner.ID != authentication.ID)
                        throw new NotImplementedException();
                });
            }

            #region ITableContentGroup

            public async Task EndEditAsync(Authentication authentication)
            {
                try
                {
                    this.ValidateExpired();
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.DebugMethod(authentication, this, nameof(EndEditAsync), this.Tables.Select(item => item.Name).ToArray());
                        this.ValidateEndEdit(authentication);
                        this.SetServiceState(ServiceState.Closing);
                    });
                    try
                    {
                        await this.EndContentAsync(authentication);
                    }
                    catch
                    {
                        this.SetServiceState(ServiceState.Open);
                        throw;
                    }
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.Sign(authentication);
                        this.SetServiceState(ServiceState.Closed);
                        this.InvokeEditEndedEvent(EventArgs.Empty);
                        this.Release();
                    });
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                    throw;
                }
            }

            public async Task CancelEditAsync(Authentication authentication)
            {
                try
                {
                    this.ValidateExpired();
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.DebugMethod(authentication, this, nameof(CancelEditAsync), this.Tables.Select(item => item.Name).ToArray());
                        this.ValidateCancelEdit(authentication);
                        this.SetServiceState(ServiceState.Closing);
                    });
                    try
                    {
                        await this.CancelContentAsync(authentication);
                    }
                    catch
                    {
                        this.SetServiceState(ServiceState.Open);
                        throw;
                    }
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.Sign(authentication);
                        this.SetServiceState(ServiceState.Closed);
                        this.InvokeEditCanceledEvent(EventArgs.Empty);
                        this.Release();
                    });
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                    throw;
                }
            }

            public async Task EnterEditAsync(Authentication authentication)
            {
                try
                {
                    this.ValidateExpired();
                    var accessType = await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.DebugMethod(authentication, this, nameof(EnterEditAsync), this.Tables.Select(item => item.Name).ToArray());
                        this.ValidateEnter(authentication);
                        return this.GetAccessType(authentication);
                    });
                    await this.domain.EnterAsync(authentication, accessType);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.Sign(authentication);
                        this.EnterContent(authentication);
                    });
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                    throw;
                }
            }

            public async Task LeaveEditAsync(Authentication authentication)
            {
                try
                {
                    this.ValidateExpired();
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.DebugMethod(authentication, this, nameof(LeaveEditAsync), this.Tables.Select(item => item.Name).ToArray());
                        this.ValidateLeave(authentication);
                    });
                    await this.domain.LeaveAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.Sign(authentication);
                        this.LeaveContent(authentication);
                    });
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                    throw;
                }
            }

            IDomain ITableContentGroup.Domain => this.domain;

            ITable[] ITableContentGroup.Tables => this.Tables;

            #endregion

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
                this.dataBaseSet = DataBaseSet.Create(this.DataBase, dataSet, DataBaseSetOptions.None);
                this.itemPaths = this.dataSet.GetItemPaths();
                this.Repository.Dispatcher.Invoke(() => this.Repository.Lock(Authentication.System, this, nameof(IDomainHost.Attach), this.itemPaths));
                foreach (var item in this.Contents)
                {
                    item.domainHost = this;
                    item.Domain = domain;
                    item.DataTable = dataSet.Tables[item.Table.Name, item.Table.Category.Path];
                    item.Table.TableState = TableState.IsBeingEdited;
                    item.ServiceState = ServiceState.Open;
                    item.IsModified = domain.ModifiedTables.Contains(item.dataTable.Name);
                }
                this.domain.Dispatcher.Invoke(this.AttachDomainEvent);
            }

            async Task IDomainHost.DeleteAsync(Authentication authentication, bool isCanceled)
            {
                var domain = this.domain;
                if (isCanceled == false)
                {
                    await this.EndContentAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.SetServiceState(ServiceState.Closed);
                        this.InvokeEditEndedEvent(new DomainDeletedEventArgs(authentication, domain, isCanceled));
                    });
                }
                else
                {
                    await this.CancelContentAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.SetServiceState(ServiceState.Closed);
                        this.InvokeEditCanceledEvent(new DomainDeletedEventArgs(authentication, domain, isCanceled));
                    });
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

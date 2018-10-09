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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.DataBaseService;
using Ntreev.Crema.Services.Domains;
using Ntreev.Library;

namespace Ntreev.Crema.Services.Data
{
    partial class TableContent
    {
        private TableContentDomainHost domainHost;

        public class TableContentDomainHost : IDomainHost
        {
            private Domain domain;

            private string masterUserID;

            public TableContentDomainHost(TableCollection container)
            {
                this.Container = container;
            }

            public TableContentDomainHost(TableCollection container, Domain domain, string itemPath)
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
                this.domain = domain;
                foreach (var item in this.Contents)
                {
                    item.domainHost = this;
                }
            }

            public Task AttachDomainEventAsync()
            {
                return this.domain.Dispatcher.InvokeAsync(() =>
                {
                    this.domain.Deleted += Domain_Deleted;
                    this.domain.RowAdded += Domain_RowAdded;
                    this.domain.RowChanged += Domain_RowChanged;
                    this.domain.RowRemoved += Domain_RowRemoved;
                    this.domain.PropertyChanged += Domain_PropertyChanged;
                    this.domain.UserChanged += Domain_UserChanged;
                });
            }

            public Task DetachDomainEventAsync()
            {
                return this.domain.Dispatcher.InvokeAsync(() =>
                {
                    this.domain.Deleted -= Domain_Deleted;
                    this.domain.RowAdded -= Domain_RowAdded;
                    this.domain.RowChanged -= Domain_RowChanged;
                    this.domain.RowRemoved -= Domain_RowRemoved;
                    this.domain.PropertyChanged -= Domain_PropertyChanged;
                    this.domain.UserChanged -= Domain_UserChanged;
                });
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

            public async Task<SignatureDate> BeginContentAsync(Authentication authentication, string name)
            {
                var result = await this.Service.BeginTableContentEditAsync(name);
                if (this.domain == null)
                {
                    this.domain = await this.DomainContext.CreateAsync(authentication, result.Value);
                    this.domain.Host = this;
                }
                var domainInfo = this.domain.DomainInfo;
                var items = StringUtility.Split(domainInfo.ItemPath, '|');
                var tableList = new List<Table>(items.Length);
                foreach (var item in items)
                {
                    if (this.DataBase.TableContext[item] is Table table)
                    {
                        tableList.Add(table);
                    }
                }

                this.Tables = tableList.ToArray();
                this.Contents = tableList.Select(item => item.Content).ToArray();
                foreach (var item in this.Contents)
                {
                    item.domain = domain;
                    item.domainHost = this;
                    item.Table.SetTableState(TableState.IsBeingEdited);
                }
                await this.AttachDomainEventAsync();
                await this.Dispatcher.InvokeAsync(() => this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables));
                return result.SignatureDate;
            }

            public async Task EndContentAsync(Authentication authentication, string name)
            {
                var tableInfoByName = new Dictionary<string, TableInfo>();
                if (name != null)
                {
                    var result = await this.Service.EndTableContentEditAsync(name);
                    var tableInfos = result.Value;
                    foreach (var item in tableInfos)
                    {
                        tableInfoByName.Add(item.Name, item);
                    }
                    if (this.domain != null)
                    {
                        await this.DetachDomainEventAsync();
                        await this.DomainContext.DeleteAsync(authentication, this.domain, false);
                    }
                }
                await this.Container.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.domain = null;
                        item.IsModified = false;
                        item.dataTable = null;
                        if (tableInfoByName.ContainsKey(item.Table.Name))
                            item.Table.UpdateContent(tableInfoByName[item.Table.Name]);
                        item.Table.SetTableState(TableState.None);
                    }

                    this.Container.InvokeTablesContentChangedEvent(authentication, this.Tables);
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public async Task CancelContentAsync(Authentication authentication, string name)
            {
                if (name != null)
                {
                    var result = await this.Service.CancelTableContentEditAsync(name);
                    if (this.domain != null)
                    {
                        await this.DetachDomainEventAsync();
                        await this.DomainContext.DeleteAsync(authentication, this.domain, true);
                    }
                }
                foreach (var item in this.Contents)
                {
                    item.domain = null;
                    item.IsModified = false;
                    item.dataTable = null;
                    item.Table.SetTableState(TableState.None);
                }
                this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
            }

            public async Task EnterContentAsync(Authentication authentication, string name)
            {
                var result = await this.Service.EnterTableContentEditAsync(name);
                this.domain.Initialize(authentication, result.Value);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var dataSet = domain.Source as CremaDataSet;
                    foreach (var item in this.Contents)
                    {
                        var tableState = item.Table.TableState;
                        item.DataTable = dataSet?.Tables[item.Table.Name, item.Table.Category.Path];
                        if (dataSet != null)
                            tableState |= TableState.IsMember;
                        if (this.masterUserID == authentication.ID)
                            tableState |= TableState.IsOwner;
                        item.Table.SetTableState(tableState);
                    }
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public async Task LeaveContentAsync(Authentication authentication, string name)
            {
                var result = await this.Service.LeaveTableContentEditAsync(name);
                await this.domain.ReleaseAsync(authentication, result.Value);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.dataTable = null;
                        item.Table.SetTableState(item.Table.TableState & ~TableState.IsMember);
                    }
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public Table[] Tables { get; private set; }

            public TableContent[] Contents { get; private set; }

            public DataBase DataBase => this.Container.DataBase;

            public TableCollection Container { get; }

            private async void Domain_Deleted(object sender, DomainDeletedEventArgs e)
            {
                if (e.IsCanceled == false)
                {
                    await this.EndContentAsync(e.Authentication, null);
                    await this.Dispatcher.InvokeAsync(() => this.InvokeEditEndedEvent(e));
                }
                else
                {
                    await this.CancelContentAsync(e.Authentication, null);
                    await this.Dispatcher.InvokeAsync(() => this.InvokeEditCanceledEvent(e));
                }
            }

            private void Domain_RowAdded(object sender, DomainRowEventArgs e)
            {
                this.Dispatcher.InvokeAsync(() =>
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

            private void Domain_RowChanged(object sender, DomainRowEventArgs e)
            {
                this.Dispatcher.InvokeAsync(() =>
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

            private void Domain_RowRemoved(object sender, DomainRowEventArgs e)
            {
                this.Dispatcher.InvokeAsync(() =>
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

            private void Domain_UserChanged(object sender, DomainUserEventArgs e)
            {
                if (this.masterUserID == this.domain.Users.OwnerUserID)
                    return;

                this.masterUserID = this.domain.Users.OwnerUserID;
                foreach (var item in this.Contents)
                {
                    var tableState = item.Table.TableState;
                    if (this.masterUserID == this.domain.CremaHost.UserID)
                        tableState |= TableState.IsOwner;
                    else
                        tableState &= ~TableState.IsOwner;
                    item.Table.SetTableState(tableState);
                }
                Authentication.System.Sign();
                this.Container.InvokeTablesStateChangedEvent(Authentication.System, this.Tables);
            }

            private CremaDispatcher Dispatcher => this.Container.Dispatcher;

            private DomainContext DomainContext => this.Container.GetService(typeof(DomainContext)) as DomainContext;

            public IDataBaseService Service => this.Container.Service;

            #region IDomainHost

            async Task IDomainHost.DetachAsync()
            {
                await this.DetachDomainEventAsync();
                this.domain = null;
                foreach (var item in this.Contents)
                {
                    item.domain = null;
                    item.dataTable = null;
                }
            }

            async Task IDomainHost.RestoreAsync(Authentication authentication, Domain domain)
            {
                var dataSet = domain.Source as CremaDataSet;
                this.domain = domain;
                this.masterUserID = this.domain.Users.OwnerUserID;
                foreach (var item in this.Contents)
                {
                    var tableState = TableState.IsBeingEdited;
                    item.domainHost = this;
                    item.domain = domain;
                    if (dataSet != null)
                    {
                        item.dataTable = dataSet.Tables[item.Table.Name, item.Table.Category.Path];
                        if (dataSet != null)
                            tableState |= TableState.IsMember;
                        if (this.masterUserID == authentication.ID)
                            tableState |= TableState.IsOwner;
                    }
                    item.Table.SetTableState(tableState);
                    item.IsModified = domain.ModifiedTables.Contains(item.Table.Name);
                }
                await this.AttachDomainEventAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                    this.InvokeEditBegunEvent(EventArgs.Empty);
                });

            }

            #endregion
        }
    }
}

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
using System.Collections;
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
        private TableContentGroup domainHost;

        public string[] Editors => this.domainHost != null ? this.domainHost.Editors : new string[] { };

        public string Owner => this.domainHost != null ? this.domainHost.Owner : string.Empty;

        public class TableContentGroup : IDomainHost, ITableContentGroup
        {
            private Domain domain;

            private string[] editors;
            private string owner;

            public TableContentGroup(TableCollection container)
            {
                this.Container = container;
            }

            public TableContentGroup(TableCollection container, Domain domain, string itemPath)
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

            public async Task EndEditAsync(Authentication authentication)
            {
                try
                {
                    this.ValidateExpired();
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.DebugMethod(authentication, this, nameof(EndEditAsync), this.Tables.Select(item => item.Name).ToArray());
                    });
                    await this.EndContentAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.InvokeEditEndedEvent(EventArgs.Empty);
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
                    });
                    await this.CancelContentAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.InvokeEditCanceledEvent(EventArgs.Empty);
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
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.DebugMethod(authentication, this, nameof(EnterEditAsync), this.Tables.Select(item => item.Name).ToArray());
                    });
                    await this.EnterContentAsync(authentication);
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
                    });
                    await this.LeaveContentAsync(authentication);
                }
                catch (Exception e)
                {
                    this.CremaHost.Error(e);
                    throw;
                }
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

            public async Task<SignatureDate> BeginContentAsync(Authentication authentication, string name)
            {
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.BeginTableContentEdit(name));
                this.CremaHost.Sign(authentication, result);
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
                    item.Domain = domain;
                    item.domainHost = this;
                    item.Table.TableState = TableState.IsBeingEdited;
                }

                await this.Dispatcher.InvokeAsync(() => this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables));
                return result.SignatureDate;
            }

            private async Task EndDomainAsync()
            {
                try
                {
                    this.domain.Host = null;
                    await this.CremaHost.InvokeServiceAsync(() => this.Service.EndTableContentEdit(this.domain.ID));
                    await this.DomainContext.WaitDeleteAsync(this.domain);
                }
                catch
                {
                    this.domain.Host = this;
                    throw;
                }
            }

            private async Task CancelDomainAsync()
            {
                try
                {
                    this.domain.Host = null;
                    await this.CremaHost.InvokeServiceAsync(() => this.Service.CancelTableContentEdit(this.domain.ID));
                    await this.DomainContext.WaitDeleteAsync(this.domain);
                }
                catch
                {
                    this.domain.Host = this;
                    throw;
                }
            }

            public async Task EndContentAsync(Authentication authentication)
            {
                if (this.domain.Host != null)
                {
                    await this.EndDomainAsync();
                }
                var tableInfos = this.domain.Result as TableInfo[];
                var tableInfoByName = tableInfos.ToDictionary(item => item.Name);

                await this.Container.Dispatcher.InvokeAsync(() =>
                {
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
                    this.Container.InvokeTablesContentChangedEvent(authentication, this.Tables);
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public async Task CancelContentAsync(Authentication authentication)
            {
                if (this.domain.Host != null)
                {
                    await this.CancelDomainAsync();
                }
                await this.Container.Dispatcher.InvokeAsync(() =>
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

            public async Task EnterContentAsync(Authentication authentication)
            {
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.EnterTableContentEdit(this.domain.ID));
                await this.domain.Dispatcher.InvokeAsync(this.AttachDomainEvent);
                await this.domain.WaitUserEnterAsync(authentication);
                await this.domain.DataDispatcher.InvokeAsync(() =>
                {
                    var dataSet = domain.Source as CremaDataSet;
                    foreach (var item in this.Contents)
                    {
                        item.DataTable = dataSet?.Tables[item.Table.Name, item.Table.Category.Path];
                    }
                });
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public async Task LeaveContentAsync(Authentication authentication)
            {
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.LeaveTableContentEdit(this.domain.ID));
                await this.domain.WaitUserLeaveAsync(authentication);
                await this.domain.Dispatcher.InvokeAsync(this.DetachDomainEvent);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.Contents)
                    {
                        item.DataTable = null;
                    }
                    this.Container.InvokeTablesStateChangedEvent(authentication, this.Tables);
                });
            }

            public Table[] Tables { get; private set; }

            public TableContent[] Contents { get; private set; }

            public DataBase DataBase => this.Container.DataBase;

            public string[] Editors => this.editors ?? new string[] { };

            public string Owner => this.owner ?? string.Empty;

            public CremaHost CremaHost => this.Container.CremaHost;

            public TableCollection Container { get; }

            public CremaDispatcher Dispatcher => this.Container.Dispatcher;

            private void Domain_RowAdded(object sender, DomainRowEventArgs e)
            {
                this.Dispatcher.InvokeAsync(() =>
                {
                    var query = from row in e.Rows
                                join content in this.Contents on row.TableName equals content.Table.Name
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
                                join content in this.Contents on row.TableName equals content.Table.Name
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

            private async void Domain_OwnerChanged(object sender, DomainUserEventArgs e)
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

            private void AttachDomainEvent()
            {
                this.domain.Dispatcher.VerifyAccess();
                this.domain.RowAdded += Domain_RowAdded;
                this.domain.RowChanged += Domain_RowChanged;
                this.domain.RowRemoved += Domain_RowRemoved;
                this.domain.PropertyChanged += Domain_PropertyChanged;
                this.domain.UserAdded += Domain_UserAdded;
                this.domain.OwnerChanged += Domain_OwnerChanged;
                this.domain.UserRemoved += Domain_UserRemoved;
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
            }

            private void RefreshEditors()
            {
                this.domain.Dispatcher.VerifyAccess();
                this.editors = (from DomainUser item in this.domain.Users select item.ID).ToArray();
                this.owner = this.domain.Users.OwnerUserID;
            }

            private DomainContext DomainContext => this.Container.GetService(typeof(DomainContext)) as DomainContext;

            public IDataBaseService Service => this.Container.Service;

            #region ITableContentGroup

            IDomain ITableContentGroup.Domain => this.domain;

            ITable[] ITableContentGroup.Tables => this.Tables;

            #endregion

            #region IDomainHost

            void IDomainHost.Detach()
            {
                this.Dispatcher.VerifyAccess();
                if (this.domain.Source != null)
                    this.domain.Dispatcher.Invoke(this.DetachDomainEvent);
                this.domain.Host = null;
                this.domain = null;
                foreach (var item in this.Contents)
                {
                    item.Domain = null;
                    item.DataTable = null;
                }
            }

            void IDomainHost.Attach(Domain domain)
            {
                this.Dispatcher.VerifyAccess();
                var dataSet = domain.Source as CremaDataSet;
                this.domain = domain;
                this.domain.Host = this;
                foreach (var item in this.Contents)
                {
                    item.domainHost = this;
                    item.Domain = domain;
                    item.DataTable = dataSet?.Tables[item.Table.Name, item.Table.Category.Path];
                    item.Table.TableState = TableState.IsBeingEdited;
                    item.IsModified = domain.ModifiedTables.Contains(item.Table.Name);
                }
                if (this.domain.Source != null)
                {
                    this.domain.Dispatcher.Invoke(this.AttachDomainEvent);
                    this.domain.Dispatcher.Invoke(this.RefreshEditors);
                }
            }

            async Task IDomainHost.DeleteAsync(Authentication authentication, bool isCanceled)
            {
                var domain = this.domain;
                if (isCanceled == false)
                {
                    await this.EndContentAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.InvokeEditEndedEvent(new DomainDeletedEventArgs(authentication, domain, isCanceled));
                    });
                }
                else
                {
                    await this.CancelContentAsync(authentication);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
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

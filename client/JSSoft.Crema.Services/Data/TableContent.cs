﻿// Released under the MIT License.
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
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    partial class TableContent : ITableContent
    {
        private CremaDataTable dataTable;
        private DataTable internalTable;

        private List<TableRow> items;

        private EventHandler editBegun;
        private EventHandler editEnded;
        private EventHandler editCanceled;
        private EventHandler changed;
        private EventHandler editorsChanged;

        public TableContent(Table table)
        {
            this.Table = table;
        }

        public override string ToString()
        {
            return this.Table.ToString();
        }

        public Task<TableRow> FindAsync(Authentication authentication, params object[] keys)
        {
            return this.Domain.DataDispatcher.InvokeAsync(() =>
            {
                var row = this.internalTable.Rows.Find(keys);
                if (row == null)
                    return null;
                return this.dataTable.ExtendedProperties[row] as TableRow;
            });
        }

        public Task<TableRow[]> SelectAsync(Authentication authentication, string filterExpression)
        {
            return this.Domain.DataDispatcher.InvokeAsync(() =>
            {
                var rows = this.internalTable.Select(filterExpression);
                var rowList = new List<TableRow>(rows.Length);
                foreach (var item in rows)
                {
                    rowList.Add(this.dataTable.ExtendedProperties[item] as TableRow);
                }
                return rowList.ToArray();
            });
        }

        public async Task BeginEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginEditAsync), this.Table);
                    return this.Table.Name;
                });
                var domainHost = new TableContentGroup(this.Container);
                var signatureDate = await domainHost.BeginContentAsync(authentication, name);
                this.domainHost = domainHost;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, signatureDate);
                    this.domainHost.InvokeEditBegunEvent(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task EndEditAsync(Authentication authentication)
        {
            return this.domainHost.EndEditAsync(authentication);
        }

        public Task CancelEditAsync(Authentication authentication)
        {
            return this.domainHost.CancelEditAsync(authentication);
        }

        public Task EnterEditAsync(Authentication authentication)
        {
            return this.domainHost.EnterEditAsync(authentication);
        }

        public Task LeaveEditAsync(Authentication authentication)
        {
            return this.domainHost.LeaveEditAsync(authentication);
        }



        public async Task ClearAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ClearAsync), this.Table);
                    return this.Table.Name;
                });
                var rowInfo = new DomainRowInfo()
                {
                    TableName = name,
                    Keys = DomainRowInfo.ClearKey,
                };
                await this.Domain.RemoveRowAsync(authentication, new DomainRowInfo[] { rowInfo });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<TableRow> AddNewAsync(Authentication authentication, string relationID)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewAsync));
                    this.ValidateAddNew(authentication);
                });
                var row = await this.Domain.DataDispatcher.InvokeAsync(() => new TableRow(this, this.dataTable.DefaultView.Table, relationID));
                return row;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task EndNewAsync(Authentication authentication, TableRow row)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndNewAsync));
                });
                await row.EndNewAsync(authentication);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Domain Domain { get; private set; }

        public IPermission Permission => this.Table;

        public Table Table { get; }

        public CremaHost CremaHost => this.Table.CremaHost;

        public DataBase DataBase => this.Table.DataBase;

        public CremaDispatcher Dispatcher => this.Table.Dispatcher;

        public int Count => this.dataTable.Rows.Count;

        public CremaDataTable DataTable
        {
            get => this.dataTable;
            set
            {
                if (value != null)
                {
                    this.dataTable = value;
                    this.internalTable = this.dataTable.DefaultView.Table;
                    this.items = new List<TableRow>(this.internalTable.Rows.Count);
                    for (var i = 0; i < this.internalTable.Rows.Count; i++)
                    {
                        var dataRow = this.internalTable.Rows[i];
                        var row = new TableRow(this, dataRow);
                        this.items.Add(row);
                        this.internalTable.ExtendedProperties[dataRow] = row;
                    }
                    this.internalTable.RowDeleted += InternalTable_RowDeleted;
                    this.internalTable.RowChanged += InternalTable_RowChanged;
                }
                else
                {
                    this.dataTable = null;
                    this.internalTable = null;
                    this.items = null;
                }
            }
        }

        private async void InternalTable_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var row = this.internalTable.ExtendedProperties[e.Row] as TableRow;
                this.items.Remove(row);
                this.internalTable.ExtendedProperties.Remove(e.Row);
            });
        }

        private async void InternalTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var row = new TableRow(this, e.Row);
                    this.items.Add(row);
                    this.internalTable.ExtendedProperties[e.Row] = row;
                });
            }
        }

        public DomainContext DomainContext => this.Table.CremaHost.DomainContext;

        public IEnumerable<TableContent> Childs
        {
            get
            {
                if (this.Table != null)
                {
                    foreach (var item in this.Table.Childs)
                    {
                        yield return item.Content;
                    }
                }
            }
        }

        public bool IsModified { get; private set; }

        public ServiceState ServiceState { get; private set; }

        public event EventHandler EditBegun
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.editBegun += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.editBegun -= value;
            }
        }

        public event EventHandler EditEnded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.editEnded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.editEnded -= value;
            }
        }

        public event EventHandler EditCanceled
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.editCanceled += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.editCanceled -= value;
            }
        }

        public event EventHandler Changed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.changed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.changed -= value;
            }
        }

        public event EventHandler EditorsChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.editorsChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.editorsChanged -= value;
            }
        }

        protected virtual void OnEditBegun(EventArgs e)
        {
            this.editBegun?.Invoke(this, e);
        }

        protected virtual void OnEditEnded(EventArgs e)
        {
            this.editEnded?.Invoke(this, e);
        }

        protected virtual void OnEditCanceled(EventArgs e)
        {
            this.editCanceled?.Invoke(this, e);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            this.changed?.Invoke(this, e);
        }

        protected virtual void OnEditorsChanged(EventArgs e)
        {
            this.editorsChanged?.Invoke(this, e);
        }

        private void ValidateAddNew(Authentication authentication)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (this.Domain == null)
                throw new InvalidOperationException("domain is null");
        }

        private TableCollection Container => this.Table.Container;

        public IDataBaseService Service => this.Table.Service;

        #region ITableContent

        async Task<ITableRow> ITableContent.AddNewAsync(Authentication authentication, string relationID)
        {
            return await this.AddNewAsync(authentication, relationID);
        }

        Task ITableContent.EndNewAsync(Authentication authentication, ITableRow row)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));
            if (row is TableRow == false)
                throw new ArgumentException(Resources.Exception_InvalidObject, nameof(row));
            return this.EndNewAsync(authentication, row as TableRow);
        }

        async Task<ITableRow> ITableContent.FindAsync(Authentication authentication, params object[] keys)
        {
            return await this.FindAsync(authentication, keys);
        }

        async Task<ITableRow[]> ITableContent.SelectAsync(Authentication authentication, string filterExpression)
        {
            return await this.SelectAsync(authentication, filterExpression);
        }

        IDomain ITableContent.Domain => this.Domain;

        ITable ITableContent.Table => this.Table;

        ITable[] ITableContent.Tables => this.domainHost != null ? this.domainHost.Tables : new ITable[] { };

        #endregion

        #region IEnumerable

        IEnumerator<ITableRow> IEnumerable<ITableRow>.GetEnumerator()
        {
            return (this.items ?? Enumerable.Empty<ITableRow>()).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this.items ?? Enumerable.Empty<ITableRow>()).GetEnumerator();
        }

        #endregion
    }
}

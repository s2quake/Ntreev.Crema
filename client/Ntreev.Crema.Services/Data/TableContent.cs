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
using Ntreev.Crema.Services.DataBaseService;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    partial class TableContent : ITableContent
    {
        private Domain domain;
        private CremaDataTable dataTable;
        private DataTable internalTable;

        //private readonly Dictionary<DataRow, TableRow> rows = new Dictionary<DataRow, TableRow>();
        private readonly HashSet<DataRow> rowsToAdd = new HashSet<DataRow>();

        private EventHandler editBegun;
        private EventHandler editEnded;
        private EventHandler editCanceled;
        private EventHandler changed;

        public TableContent(Table table)
        {
            this.Table = table;
        }

        public override string ToString()
        {
            return this.Table.ToString();
        }

        protected Task AddAsync(TableRow row)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.dataTable.ExtendedProperties[row.Row] = row;
                this.rowsToAdd.Remove(row.Row);
            });
        }

        protected void Clear()
        {
            //this.rows.Clear();
        }

        public Task<TableRow> FindAsync(Authentication authentication, params object[] keys)
        {
            return this.Domain.Dispatcher.InvokeAsync(() =>
            {
                var row = this.internalTable.Rows.Find(keys);
                if (row == null)
                    return null;
                return this.dataTable.ExtendedProperties[row] as TableRow;
            });
        }

        public Task<TableRow[]> SelectAsync(Authentication authentication, string filterExpression)
        {
            return this.Domain.Dispatcher.InvokeAsync(() =>
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
                var domainHost = new TableContentDomainHost(this.Container);
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

        public async Task EndEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndEditAsync), this.Table);
                    return this.Table.Name;
                });
                await this.domainHost.EndContentAsync(authentication, name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.domainHost.InvokeEditEndedEvent(EventArgs.Empty);
                    this.domainHost = null;
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
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CancelEditAsync), this.Table);
                    return this.Table.Name;
                });
                await this.domainHost.CancelContentAsync(authentication, name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    //this.CremaHost.Sign(authentication, result);
                    this.domainHost.InvokeEditCanceledEvent(EventArgs.Empty);
                    this.domainHost = null;
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
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterEditAsync), this.Table);
                    return this.Table.Name;
                });
                await this.domainHost.EnterContentAsync(authentication, name);
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
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveEditAsync), this.Table);
                    return this.Table.Name;
                });
                await this.domainHost.LeaveContentAsync(authentication, name);
                //    var result = await Task.Run(() => this.Service.LeaveTableContentEdit(this.Table.Name));
                //    this.CremaHost.Sign(authentication, result);
                //    this.domain.Dispatcher.Invoke(() => this.domain.Release(authentication, result.GetValue()));
                //    this.domainHost.LeaveContent(authentication);
                //});
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
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
                await this.domain.RemoveRowAsync(authentication, new DomainRowInfo[] { rowInfo });
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
                var row = await this.domain.Dispatcher.InvokeAsync(() => new TableRow(this, this.dataTable.DefaultView.Table, relationID));
                await this.Dispatcher.InvokeAsync(() => this.rowsToAdd.Add(row.Row));
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
                await this.AddAsync(row);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Domain Domain => this.domain;

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
                this.dataTable = value;
                this.internalTable = this.dataTable.DefaultView.Table;
                foreach (DataRow item in this.internalTable.Rows)
                {
                    this.dataTable.ExtendedProperties[item] = new TableRow(this, item);
                }
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

        private void ValidateAddNew(Authentication authentication)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (this.domain == null)
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

        // TODO: this.items 로 변경이 가능한지 확인(서버랑 코드가 다름)
        IEnumerator<ITableRow> IEnumerable<ITableRow>.GetEnumerator()
        {
            foreach (DataRow item in this.internalTable.Rows)
            {
                yield return this.dataTable.ExtendedProperties[item] as TableRow;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (DataRow item in this.internalTable.Rows)
            {
                yield return this.dataTable.ExtendedProperties[item] as TableRow;
            }
        }

        #endregion
    }
}

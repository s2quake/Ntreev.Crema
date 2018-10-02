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
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    partial class TableContent : TableContentBase, ITableContent
    {
        private readonly Table table;
        private Domain domain;
        private CremaDataTable dataTable;

        private EventHandler editBegun;
        private EventHandler editEnded;
        private EventHandler editCanceled;
        private EventHandler changed;

        private bool isModified;

        public TableContent(Table table)
        {
            this.table = table;
        }

        public override string ToString()
        {
            return this.table.ToString();
        }

        public async Task BeginEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginEditAsync), this.Table);
                    return this.table.Name;
                });
                var result = await this.Service.BeginTableContentEditAsync(name);
                if (this.domain == null)
                {
                    this.domain = await this.DomainContext.CreateAsync(authentication, result.Value);
                    this.domain.Host = new TableContentDomainHost(this.Container, this.domain, this.domain.DomainInfo.ItemPath);
                    await this.domainHost.AttachDomainEventAsync();
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.domainHost.BeginContent(authentication, this.domain);
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
                    return this.table.Name;
                });
                var result = await this.Service.EndTableContentEditAsync(name);
                if (this.domain != null)
                {
                    await this.domainHost.DetachDomainEventAsync();
                    await this.domain.DisposeAsync(authentication, false);
                }
                else
                {
                    await this.DomainContext.DeleteAsync(authentication, this.domain, false);
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.domainHost.EndContent(authentication, result.Value);
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
                    return this.table.Name;
                });
                var result = await this.Service.CancelTableContentEditAsync(name);
                if (this.domain != null)
                {
                    await this.domainHost.DetachDomainEventAsync();
                    await this.domain.DisposeAsync(authentication, true);
                }
                else
                {
                    await this.DomainContext.DeleteAsync(authentication, this.domain, true);
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.domainHost.CancelContent(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterEditAsync), this.Table);
                    var result = await this.Service.EnterTableContentEditAsync(this.table.Name);
                    this.CremaHost.Sign(authentication, result);
                    this.domain.Dispatcher.Invoke(() => this.domain.Initialize(authentication, result.Value));
                    this.domainHost.EnterContent(authentication, this.domain);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveEditAsync), this.Table);
                    var result = await this.Service.LeaveTableContentEditAsync(this.table.Name);
                    this.CremaHost.Sign(authentication, result);
                    this.domain.Dispatcher.Invoke(() => this.domain.Release(authentication, result.Value));
                    this.domainHost.LeaveContent(authentication);
                });
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ClearAsync), this.Table);
                    var rowInfo = new DomainRowInfo()
                    {
                        TableName = this.table.Name,
                        Keys = DomainRowInfo.ClearKey,
                    };
                    await this.domain.RemoveRowAsync(authentication, new DomainRowInfo[] { rowInfo });
                });
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
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.domain == null)
                        throw new InvalidOperationException(Resources.Exception_NotBeingEdited);
                    var view = this.dataTable.DefaultView;
                    return new TableRow(this, view.Table);
                });
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    if (this.domain == null)
                        throw new InvalidOperationException(Resources.Exception_NotBeingEdited);
                    await row.EndNewAsync(authentication);
                    this.Add(row);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public override Domain Domain => this.domain;

        public IPermission Permission => this.table;

        public Table Table { get; }

        public override CremaHost CremaHost => this.table.CremaHost;

        public override DataBase DataBase => this.table.DataBase;

        public override CremaDispatcher Dispatcher => this.table.Dispatcher;

        public int Count => this.dataTable.Rows.Count;

        public override CremaDataTable DataTable => this.dataTable;

        public DomainContext DomainContext => this.table.CremaHost.DomainContext;

        public IEnumerable<TableContent> Childs
        {
            get
            {
                if (this.table != null)
                {
                    foreach (var item in this.table.Childs)
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

        private TableCollection Container => this.table.Container;

        public IDataBaseService Service => this.table.Service;

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
            this.Dispatcher?.VerifyAccess();
            return this.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            this.Dispatcher?.VerifyAccess();
            return this.GetEnumerator();
        }

        #endregion
    }
}

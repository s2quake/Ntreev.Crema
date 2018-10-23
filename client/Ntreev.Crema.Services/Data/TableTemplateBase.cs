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
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    abstract class TableTemplateBase : ITableTemplate, IDomainHost
    {
        private TableTemplateDomain domain;
        private DataTable table;

        private readonly HashSet<DataRow> rowsToAdd = new HashSet<DataRow>();
        private List<TableColumn> items;

        private EventHandler editBegun;
        private EventHandler editEnded;
        private EventHandler editCanceled;
        private EventHandler changed;

        public abstract AccessType GetAccessType(Authentication authentication);

        public async Task<TableColumn> AddNewAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewAsync));
                });
                var column = await TableColumn.CreateAsync(authentication, this, this.TemplateSource.View.Table);
                await this.Dispatcher.InvokeAsync(() => this.rowsToAdd.Add(column.Row));
                return column;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task EndNewAsync(Authentication authentication, TableColumn column)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndNewAsync));
                });
                await column.EndNewAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.items.Add(column);
                    this.rowsToAdd.Remove(column.Row);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task BeginEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginEditAsync));
                });
                await this.OnBeginEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnEditBegun(EventArgs.Empty);
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndEditAsync));
                });
                await this.OnEndEditAsync(authentication, this.domain.ID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnEditEnded(EventArgs.Empty);
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
                    this.CremaHost.DebugMethod(authentication, this, nameof(CancelEditAsync));
                });
                await this.OnCancelEditAsync(authentication, this.domain.ID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnEditCanceled(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetTableNameAsync(Authentication authentication, string value)
        {
            return this.domain.SetPropertyAsync(authentication, CremaSchema.TableName, value);
        }

        public Task SetTagsAsync(Authentication authentication, TagInfo value)
        {
            return this.domain.SetPropertyAsync(authentication, CremaSchema.Tags, (string)value);
        }

        public Task SetCommentAsync(Authentication authentication, string value)
        {
            return this.domain.SetPropertyAsync(authentication, CremaSchema.Comment, value);
        }

        public bool Contains(string columnName)
        {
            if (this.items == null)
                return false;
            return this.items.Any(item => item.Name == columnName);
        }

        public bool IsNew { get; set; }

        public Domain Domain => this.domain;

        public abstract IPermission Permission { get; }

        public int Count => this.items.Count;

        public abstract DomainContext DomainContext { get; }

        public abstract string ItemPath { get; }

        public abstract CremaHost CremaHost { get; }

        public abstract object Target { get; }

        public abstract DataBase DataBase { get; }

        public abstract IDispatcherObject DispatcherObject { get; }

        public CremaDispatcher Dispatcher => this.DispatcherObject.Dispatcher;

        public string TableName => this.TemplateSource.TableName;

        public TagInfo Tags => this.TemplateSource.Tags;

        public string Comment => this.TemplateSource.Comment;

        public TableColumn this[string columnName] => this.items.FirstOrDefault(item => item.Name == columnName);

        public string[] SelectableTypes => this.TemplateSource.Types;

        public IEnumerable<TableColumn> PrimaryKey
        {
            get
            {
                foreach (var item in this.items)
                {
                    if (item.IsNew == true)
                        continue;
                    if (item.IsKey == true)
                        yield return item;
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

        protected virtual async Task OnBeginEditAsync(Authentication authentication)
        {
            var result = await this.BeginDomainAsync(authentication);
            this.CremaHost.Sign(authentication, result);

            var metaData = result.GetValue();
            this.domain = await this.DomainContext.CreateAsync(authentication, metaData) as TableTemplateDomain;
            this.domain.IsNew = this.IsNew;
            this.domain.Host = this;
            this.TemplateSource = this.domain.TemplateSource;

            this.table = this.TemplateSource.View.Table;
            this.items = new List<TableColumn>(this.table.Rows.Count);
            for (var i = 0; i < this.table.Rows.Count; i++)
            {
                var item = this.table.Rows[i];
                this.items.Add(new TableColumn(this, item));
            }
            this.table.RowDeleted += Table_RowDeleted;
            this.table.RowChanged += Table_RowChanged;

            //await this.DomainContext.Domains.AddAsync(authentication, this.domain, this.DataBase);
            //await this.domain.AddUserAsync(authentication, DomainAccessType.ReadWrite);
            await this.AttachDomainEventAsync();
        }

        protected virtual async Task<TableInfo[]> OnEndEditAsync(Authentication authentication, object args)
        {
            var tableInfos = await this.EndDomainAsync(authentication, args);
            if (this.domain != null)
            {
                await this.DetachDomainEventAsync();
                await this.DomainContext.Domains.RemoveAsync(authentication, this.domain, false, tableInfos);
                this.domain = null;
            }
            if (this.table != null)
            {
                this.table.RowDeleted -= Table_RowDeleted;
                this.table.RowChanged -= Table_RowChanged;
            }
            this.IsModified = false;
            this.table = null;
            this.items = null;
            this.rowsToAdd.Clear();
            return tableInfos;
        }

        protected virtual async Task OnCancelEditAsync(Authentication authentication, object args)
        {
            var result = await this.CancelDomainAsync(authentication, args);
            this.CremaHost.Sign(authentication, result);
            if (args is Guid)
            {
                await this.DetachDomainEventAsync();
                await this.DomainContext.Domains.RemoveAsync(authentication, this.domain, true, null);
                this.domain = null;
            }
            if (this.table != null)
            {
                this.table.RowDeleted -= Table_RowDeleted;
                this.table.RowChanged -= Table_RowChanged;
            }
            this.IsModified = false;
            this.table = null;
            this.items = null;
            this.rowsToAdd.Clear();
        }

        protected virtual void OnAttach(Domain domain)
        {
            this.TemplateSource = domain.Source as CremaTemplate;
            this.domain = domain as TableTemplateDomain;

            if (this.TemplateSource != null)
            {
                this.table = this.TemplateSource.View.Table;
                this.items = new List<TableColumn>(this.table.Rows.Count);
                for (var i = 0; i < this.table.Rows.Count; i++)
                {
                    var item = this.table.Rows[i];
                    this.items.Add(new TableColumn(this, item));
                }
                this.table.RowDeleted += Table_RowDeleted;
                this.table.RowChanged += Table_RowChanged;
            }
            this.IsModified = this.domain.IsModified;
            this.AttachDomainEvent();
        }

        protected virtual void OnDetach()
        {
            this.DetachDomainEvent();
            this.domain = null;
        }

        protected CremaTemplate TemplateSource { get; private set; }

        protected abstract Task<ResultBase<DomainMetaData>> BeginDomainAsync(Authentication authentication);

        protected abstract Task<TableInfo[]> EndDomainAsync(Authentication authentication, object args);

        protected abstract Task<ResultBase> CancelDomainAsync(Authentication authentication, object args);

        private async void Table_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var column = this.items.FirstOrDefault(item => item.Row == e.Row);
                this.items.Remove(column);
            });
        }

        private async void Table_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.rowsToAdd.Contains(e.Row) == false)
                    {
                        this.items.Add(new TableColumn(this, e.Row));
                    }
                });
            }
        }

        //private async void Domain_Deleted(object sender, DomainDeletedEventArgs e)
        //{
        //    var isCanceled = e.IsCanceled;
        //    if (isCanceled == false)
        //    {
        //        await this.OnEndEditAsync(e.Authentication);
        //        await this.Dispatcher.InvokeAsync(() => this.OnEditEnded(e));
        //    }
        //    else
        //    {
        //        await this.OnCancelEditAsync(e.Authentication);
        //        await this.Dispatcher.InvokeAsync(() => this.OnEditCanceled(e));
        //    }
        //}

        private async void Domain_RowAdded(object sender, DomainRowEventArgs e)
        {
            this.IsModified = this.domain.IsModified;
            await this.Dispatcher.InvokeAsync(() => this.OnChanged(e));
        }

        private async void Domain_RowChanged(object sender, DomainRowEventArgs e)
        {
            this.IsModified = this.domain.IsModified;
            await this.Dispatcher.InvokeAsync(() => this.OnChanged(e));
        }

        private async void Domain_RowRemoved(object sender, DomainRowEventArgs e)
        {
            this.IsModified = this.domain.IsModified;
            await this.Dispatcher.InvokeAsync(() => this.OnChanged(e));
        }

        private async void Domain_PropertyChanged(object sender, DomainPropertyEventArgs e)
        {
            this.IsModified = this.domain.IsModified;
            await this.Dispatcher.InvokeAsync(() => this.OnChanged(e));
        }

        private void AttachDomainEvent()
        {
            this.domain.Dispatcher.Invoke(() =>
            {
                this.domain.RowAdded += Domain_RowAdded;
                this.domain.RowChanged += Domain_RowChanged;
                this.domain.RowRemoved += Domain_RowRemoved;
                this.domain.PropertyChanged += Domain_PropertyChanged;
            });
        }

        private void DetachDomainEvent()
        {
            this.domain.Dispatcher.Invoke(() =>
            {
                this.domain.RowAdded -= Domain_RowAdded;
                this.domain.RowChanged -= Domain_RowChanged;
                this.domain.RowRemoved -= Domain_RowRemoved;
                this.domain.PropertyChanged -= Domain_PropertyChanged;
            });
        }

        private Task AttachDomainEventAsync()
        {
            return this.domain.Dispatcher.InvokeAsync(() =>
            {
                //this.domain.Deleted += Domain_Deleted;
                this.domain.RowAdded += Domain_RowAdded;
                this.domain.RowChanged += Domain_RowChanged;
                this.domain.RowRemoved += Domain_RowRemoved;
                this.domain.PropertyChanged += Domain_PropertyChanged;
            });
        }

        private Task DetachDomainEventAsync()
        {
            return this.domain.Dispatcher.InvokeAsync(() =>
            {
                //this.domain.Deleted -= Domain_Deleted;
                this.domain.RowAdded -= Domain_RowAdded;
                this.domain.RowChanged -= Domain_RowChanged;
                this.domain.RowRemoved -= Domain_RowRemoved;
                this.domain.PropertyChanged -= Domain_PropertyChanged;
            });
        }

        #region ITableTemplate

        async Task<ITableColumn> ITableTemplate.AddNewAsync(Authentication authentication)
        {
            return await this.AddNewAsync(authentication);
        }

        Task ITableTemplate.EndNewAsync(Authentication authentication, ITableColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (column is TableColumn == false)
                throw new ArgumentException(Resources.Exception_InvalidObject, nameof(column));
            return this.EndNewAsync(authentication, column as TableColumn);
        }

        object ITableTemplate.Target => this.Target;

        IDomain ITableTemplate.Domain => this.Domain;

        ITableColumn ITableTemplate.this[string columnName] => this[columnName];

        IEnumerable<ITableColumn> ITableTemplate.PrimaryKey => this.PrimaryKey;

        #endregion

        #region IDomainHost

        void IDomainHost.Attach(Domain domain)
        {
            this.OnAttach(domain);
            this.OnEditBegun(EventArgs.Empty);
        }

        void IDomainHost.Detach()
        {
            this.OnDetach();
        }

        async Task<object> IDomainHost.DeleteAsync(Authentication authentication, bool isCanceled, object result)
        {
            if (isCanceled == false)
            {
                var args = new DomainDeletedEventArgs(authentication, this.domain, isCanceled, result);
                result = await this.OnEndEditAsync(authentication, result);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnEditEnded(args);
                });
                return result;
            }
            else
            {
                var args = new DomainDeletedEventArgs(authentication, this.domain, isCanceled, null);
                await this.OnCancelEditAsync(authentication, result);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnEditCanceled(args);
                });
                return null;
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator<ITableColumn> IEnumerable<ITableColumn>.GetEnumerator()
        {
            return this.items?.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.items?.GetEnumerator();
        }

        #endregion
    }
}

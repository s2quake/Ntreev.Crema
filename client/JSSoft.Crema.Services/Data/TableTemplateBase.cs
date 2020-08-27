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

        private List<TableColumn> items;

        private EventHandler editBegun;
        private EventHandler editEnded;
        private EventHandler editCanceled;
        private EventHandler changed;

        private EventHandler editorsChanged;

        private string editor;

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
                await this.OnEndEditAsync(authentication);
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
                await this.OnCancelEditAsync(authentication);
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

        public string Editor => this.editor ?? string.Empty;

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

        protected virtual async Task OnBeginEditAsync(Authentication authentication)
        {
            var result = await this.BeginDomainAsync(authentication);
            this.CremaHost.Sign(authentication, result);

            var metaData = result.Value;
            this.domain = await this.DomainContext.CreateAsync(authentication, metaData) as TableTemplateDomain;
            this.domain.IsNew = this.IsNew;
            this.domain.Host = this;
            await this.domain.WaitUserEnterAsync(authentication);
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
            await this.domain.Dispatcher.InvokeAsync(this.AttachDomainEvent);
            await this.domain.Dispatcher.InvokeAsync(this.RefreshEditors);
        }

        protected virtual async Task OnEndEditAsync(Authentication authentication)
        {
            if (this.domain.Host != null)
            {
                await this.EndDomainAsync(authentication);
            }
            this.domain = null;
            if (this.table != null)
            {
                this.table.RowDeleted -= Table_RowDeleted;
                this.table.RowChanged -= Table_RowChanged;
            }
            this.IsModified = false;
            this.table = null;
            this.items = null;
            this.editor = null;
        }

        protected virtual async Task OnCancelEditAsync(Authentication authentication)
        {
            if (this.domain.Host != null)
            {
                await this.CancelDomainAsync(authentication);
            }
            this.domain = null;
            if (this.table != null)
            {
                this.table.RowDeleted -= Table_RowDeleted;
                this.table.RowChanged -= Table_RowChanged;
            }
            this.IsModified = false;
            this.table = null;
            this.items = null;
            this.editor = null;
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
                this.IsModified = this.domain.IsModified;
                this.domain.Dispatcher.Invoke(this.AttachDomainEvent);
                this.domain.Dispatcher.Invoke(this.RefreshEditors);
            }
        }

        protected virtual void OnDetach()
        {
            if (this.TemplateSource != null)
            {
                this.domain.Dispatcher.Invoke(this.DetachDomainEvent);
            }
            this.domain = null;
        }

        protected CremaTemplate TemplateSource { get; private set; }

        protected abstract Task<ResultBase<DomainMetaData>> OnBeginDomainAsync(Authentication authentication);

        protected abstract Task<ResultBase<TableInfo[]>> OnEndDomainAsync(Authentication authentication);

        protected abstract Task<ResultBase> OnCancelDomainAsync(Authentication authentication);

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
                    this.items.Add(new TableColumn(this, e.Row));
                });
            }
        }

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

        private async void Domain_UserAdded(object sender, DomainUserEventArgs e)
        {
            this.RefreshEditors();
            await this.Dispatcher.InvokeAsync(() => this.OnEditorsChanged(e));
        }

        private async void Domain_OwnerChanged(object sender, DomainUserEventArgs e)
        {
            this.RefreshEditors();
            await this.Dispatcher.InvokeAsync(() => this.OnEditorsChanged(e));
        }

        private async void Domain_UserRemoved(object sender, DomainUserRemovedEventArgs e)
        {
            this.RefreshEditors();
            await this.Dispatcher.InvokeAsync(() => this.OnEditorsChanged(e));
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
            this.editor = this.domain.Users.OwnerUserID;
        }

        private Task<ResultBase<DomainMetaData>> BeginDomainAsync(Authentication authentication)
        {
            return this.OnBeginDomainAsync(authentication);
        }

        private async Task EndDomainAsync(Authentication authentication)
        {
            try
            {
                this.domain.Host = null;
                await this.OnEndDomainAsync(authentication);
                await this.DomainContext.WaitDeleteAsync(this.domain);
            }
            catch
            {
                this.domain.Host = this;
                throw;
            }
        }

        private async Task CancelDomainAsync(Authentication authentication)
        {
            try
            {
                this.domain.Host = null;
                await this.OnCancelDomainAsync(authentication);
                await this.DomainContext.WaitDeleteAsync(this.domain);
            }
            catch
            {
                this.domain.Host = this;
                throw;
            }
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
        }

        void IDomainHost.Detach()
        {
            this.OnDetach();
        }

        async Task IDomainHost.DeleteAsync(Authentication authentication, bool isCanceled)
        {
            var domain = this.domain;
            if (isCanceled == false)
            {
                await this.OnEndEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnEditEnded(new DomainDeletedEventArgs(authentication, domain, isCanceled));
                });
            }
            else
            {
                await this.OnCancelEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnEditCanceled(new DomainDeletedEventArgs(authentication, domain, isCanceled));
                });
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

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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    partial class TableContent : TableContentBase, ITableContent
    {
        private Domain domain;
        private CremaDataTable dataTable;

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

        public async Task BeginEditAsync(Authentication authentication)
        {
            try
            {
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginEditAsync), this.Table);
                    this.ValidateBeginEdit(authentication);
                    this.Sign(authentication);
                    if (this.domain == null)
                    {
                        var dataSet = await this.Table.ReadEditableDataAsync(authentication);
                        var tables = this.Table.GetRelations();
                        var itemPath = string.Join("|", tables.Select(item => item.Path));
                        this.domain = new TableContentDomain(authentication, dataSet, this.Table.DataBase, itemPath, this.GetType().Name);
                        this.domain.Host = new TableContentDomainHost(this.Container, this.domain, itemPath);
                        this.DomainContext.Domains.Add(authentication, this.domain, this.DataBase);
                    }
                    await this.domainHost.BeginContentAsync(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndEditAsync), this.Table);
                    this.ValidateEndEdit(authentication);
                    this.Sign(authentication);
                    await this.domainHost.EndContentAsync(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CancelEditAsync), this.Table);
                    this.ValidateCancelEdit(authentication);
                    this.Sign(authentication);
                    await this.domainHost.CancelContentAsync(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterEditAsync), this.Table);
                    this.ValidateEnter(authentication);
                    this.Sign(authentication);
                    var accessType = this.GetAccessType(authentication);
                    await this.domain.AddUserAsync(authentication, accessType);
                    this.domainHost.EnterContent(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveEditAsync), this.Table);
                    this.ValidateLeave(authentication);
                    this.Sign(authentication);
                    await this.domain.RemoveUserAsync(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ClearAsync), this.Table);
                    var rowInfo = new DomainRowInfo()
                    {
                        TableName = this.Table.Name,
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
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.domain == null)
                        throw new InvalidOperationException(Resources.Exception_TableIsNotBeingEdited);
                    var view = this.dataTable.DefaultView;
                    return new TableRow(this, view.Table, relationID);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    if (this.domain == null)
                        throw new InvalidOperationException(Resources.Exception_TableIsNotBeingEdited);
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ValidateBeginEdit(Authentication authentication)
        {
            var isAdmin = authentication.Types.HasFlag(AuthenticationType.Administrator);
            var items = this.Table.GetRelations().Distinct().OrderBy(item => item.Name);
            foreach (var item in items)
            {
                item.Content.OnValidateBeginEdit(authentication, this);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ValidateEndEdit(Authentication authentication)
        {
            var isAdmin = authentication.Types.HasFlag(AuthenticationType.Administrator);
            if (this.domain == null)
                throw new NotImplementedException();
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
            foreach (var item in this.Relations)
            {
                item.OnValidateEnter(authentication, this);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ValidateLeave(Authentication authentication)
        {
            foreach (var item in this.Relations)
            {
                item.OnValidateLeave(authentication, this);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ValidateCancelEdit(Authentication authentication)
        {
            var isAdmin = authentication.Types.HasFlag(AuthenticationType.Administrator);
            if (this.domain == null)
                throw new NotImplementedException();
            this.domain.Dispatcher.Invoke(() =>
            {
                if (isAdmin == false && this.domain.Users.Owner.ID != authentication.ID)
                    throw new NotImplementedException();
            });

            foreach (var item in this.Relations)
            {
                item.OnValidateCancelEdit(authentication, this);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnValidateBeginEdit(Authentication authentication, object target)
        {
            this.Table.ValidateAccessType(authentication, AccessType.Guest);

            if (this.Table.DataBase.Version.Major != CremaSchema.MajorVersion || this.DataBase.Version.Minor != CremaSchema.MinorVersion)
                throw new InvalidOperationException("database version is low.");

            if (this.domain != null)
                throw new NotImplementedException();

            this.Table.ValidateHasNotBeingEditedType();

            if (this.Table.Template.IsBeingEdited == true)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingEdited_Format, this.Table.Name));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnValidateEnter(Authentication authentication, object target)
        {
            this.Table.ValidateAccessType(authentication, AccessType.Guest);

            if (this.domain == null)
                throw new NotImplementedException();

            this.Table.ValidateHasNotBeingEditedType();

            if (this.Table.Template.IsBeingEdited == true)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingEdited_Format, this.Table.Name));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnValidateLeave(Authentication authentication, object target)
        {
            this.Table.ValidateAccessType(authentication, AccessType.Guest);

            if (this.domain == null)
                throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnValidateCancelEdit(Authentication authentication, object target)
        {
            if (this.domain == null)
                throw new NotImplementedException();
        }

        public override Domain Domain => this.domain;

        public IPermission Permission => this.Table;

        public Table Table { get; }

        public override CremaHost CremaHost => this.Table.CremaHost;

        public override DataBase DataBase => this.Table.DataBase;

        public override IDispatcherObject DispatcherObject => this.Table;

        public int Count => this.dataTable.Rows.Count;

        public override CremaDataTable DataTable => this.dataTable;

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

        private DomainAccessType GetAccessType(Authentication authentication)
        {
            if (this.Table.VerifyAccessType(authentication, AccessType.Editor))
                return DomainAccessType.ReadWrite;
            else if (this.Table.VerifyAccessType(authentication, AccessType.Guest))
                return DomainAccessType.Read;
            throw new PermissionDeniedException();
        }

        protected void Sign(Authentication authentication)
        {
            authentication.Sign();
        }

        private TableCollection Container => this.Table.Container;

        private IEnumerable<TableContent> Relations => this.Table.GetRelations().Select(item => item.Content);

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
            return this.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}

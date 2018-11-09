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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    abstract class TypeTemplateBase : ITypeTemplate, IDomainHost
    {
        private TypeDomain domain;
        private DataTable table;

        private List<TypeMember> items;

        private EventHandler editBegun;
        private EventHandler editEnded;
        private EventHandler editCanceled;
        private EventHandler changed;
        private EventHandler editorsChanged;

        private string editor;

        public abstract AccessType GetAccessType(Authentication authentication);

        public async Task<TypeMember> AddNewAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewAsync));
                    this.ValidateAddNew(authentication);
                });
                var member = await TypeMember.CreateAsync(authentication, this, this.TypeSource.View.Table);
                return member;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task EndNewAsync(Authentication authentication, TypeMember member)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndNewAsync));
                });
                await member.EndNewAsync(authentication);
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
                    this.ValidateBeginEdit(authentication);
                    this.ServiceState = ServiceState.Opening;
                });
                try
                {
                    await this.OnBeginEditAsync(authentication);
                }
                catch
                {
                    await this.Dispatcher.InvokeAsync(() => this.ServiceState = ServiceState.None);
                    throw;
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.ServiceState = ServiceState.Opened;
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
                    this.ValidateEndEdit(authentication);
                    this.ServiceState = ServiceState.Closing;
                });
                try
                {
                    await this.OnEndEditAsync(authentication);
                }
                catch
                {
                    await this.Dispatcher.InvokeAsync(() => this.ServiceState = ServiceState.Opened);
                    throw;
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.ServiceState = ServiceState.Closed;
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
                    this.ValidateCancelEdit(authentication);
                    this.ServiceState = ServiceState.Closing;
                });
                try
                {
                    await this.OnCancelEditAsync(authentication);
                }
                catch
                {
                    await this.Dispatcher.InvokeAsync(() => this.ServiceState = ServiceState.Opened);
                    throw;
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.ServiceState = ServiceState.Closed;
                    this.OnEditCanceled(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void ValidateAddNew(Authentication authentication)
        {
            if (this.ServiceState != ServiceState.Opened)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
            this.OnValidateAddNew(authentication, this);
        }

        public void ValidateBeginEdit(Authentication authentication)
        {
            if (this.domain != null)
                throw new InvalidOperationException(Resources.Exception_ItIsAlreadyBeingEdited);
            if (this.ServiceState != ServiceState.None)
                throw new InvalidOperationException(Resources.Exception_ItIsAlreadyBeingEdited);
            this.ValidateAccessType(authentication, AccessType.Developer);
            this.OnValidateBeginEdit(authentication, this);
        }

        public void ValidateEndEdit(Authentication authentication)
        {
            if (this.domain == null)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
            if (this.ServiceState != ServiceState.Opened)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
            this.ValidateAccessType(authentication, AccessType.Developer);
            this.OnValidateEndEdit(authentication, this);
        }

        public void ValidateCancelEdit(Authentication authentication)
        {
            if (this.domain == null || this.domain.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
            if (this.ServiceState != ServiceState.Opened)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
            this.ValidateAccessType(authentication, AccessType.Developer);
            this.OnValidateCancelEdit(authentication, this);
        }

        public Task SetTypeNameAsync(Authentication authentication, string value)
        {
            return this.domain.SetPropertyAsync(authentication, CremaSchema.TypeName, value);
        }

        public Task SetTagsAsync(Authentication authentication, TagInfo value)
        {
            return this.domain.SetPropertyAsync(authentication, CremaSchema.Tags, (string)value);
        }

        public Task SetIsFlagAsync(Authentication authentication, bool value)
        {
            return this.domain.SetPropertyAsync(authentication, CremaSchema.IsFlag, value);
        }

        public Task SetCommentAsync(Authentication authentication, string value)
        {
            return this.domain.SetPropertyAsync(authentication, CremaSchema.Comment, value);
        }

        public bool Contains(string memberName)
        {
            if (this.items == null)
                return false;
            return this.items.Any(item => item.Name == memberName);
        }

        public bool IsNew { get; set; }

        public Domain Domain => this.domain;

        public abstract IPermission Permission { get; }

        public int Count => this.items.Count;

        public string Editor => this.editor ?? string.Empty;

        public abstract DomainContext DomainContext { get; }

        public abstract string Path { get; }

        public abstract CremaHost CremaHost { get; }

        public abstract IType Type { get; }

        public abstract DataBase DataBase { get; }

        public abstract IDispatcherObject DispatcherObject { get; }

        public CremaDispatcher Dispatcher => this.DispatcherObject.Dispatcher;

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public string TypeName => this.TypeSource.Name;

        public bool IsFlag => this.TypeSource.IsFlag;

        public string Comment => this.TypeSource.Comment;

        public TypeMember this[string memberName] => this.items.FirstOrDefault(item => item.Name == memberName);

        public bool IsModified { get; private set; }

        public ServiceState ServiceState { get; private set; }

        public string[] ItemPaths { get; private set; }

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
            this.TypeSource = await this.CreateSourceAsync(authentication);
            this.domain = new TypeDomain(authentication, this.TypeSource, this.DataBase, this.Path, this.GetType().Name)
            {
                IsNew = this.IsNew,
                Host = this
            };
            this.ItemPaths = this.domain.ItemPaths;
            await this.DomainContext.AddAsync(authentication, this.domain, this.DataBase);

            this.table = this.TypeSource.View.Table;
            this.items = new List<TypeMember>(this.table.Rows.Count);
            for (var i = 0; i < this.table.Rows.Count; i++)
            {
                var item = this.table.Rows[i];
                this.items.Add(new TypeMember(this, item));
            }
            this.table.RowDeleted += Table_RowDeleted;
            this.table.RowChanged += Table_RowChanged;

            await this.domain.EnterAsync(authentication, DomainAccessType.ReadWrite);
            await this.domain.Dispatcher.InvokeAsync(this.AttachDomainEvent);
            await this.domain.Dispatcher.InvokeAsync(this.RefreshEditors);
        }

        protected virtual async Task OnEndEditAsync(Authentication authentication)
        {
            if (this.domain.Host != null)
            {
                await this.domain.Dispatcher.InvokeAsync(this.DetachDomainEvent);
                await this.DomainContext.RemoveAsync(authentication, this.domain, false);
            }
            this.domain = null;
            this.table.RowDeleted -= Table_RowDeleted;
            this.table.RowChanged -= Table_RowChanged;
            this.IsModified = false;
            this.table = null;
            this.items = null;
            this.editor = null;
        }

        protected virtual async Task OnCancelEditAsync(Authentication authentication)
        {
            if (this.domain.Host != null)
            {
                await this.domain.Dispatcher.InvokeAsync(this.DetachDomainEvent);
                await this.DomainContext.RemoveAsync(authentication, this.domain, true);
            }
            this.domain = null;
            this.table.RowDeleted -= Table_RowDeleted;
            this.table.RowChanged -= Table_RowChanged;
            this.IsModified = false;
            this.table = null;
            this.items = null;
            this.editor = null;
        }

        protected virtual void OnAttach(Domain domain)
        {
            this.TypeSource = domain.Source as CremaDataType;
            this.domain = domain as TypeDomain;
            this.ItemPaths = this.domain.ItemPaths;
            this.Repository.Dispatcher.Invoke(() => this.Repository.Lock(Authentication.System, this, nameof(OnAttach), this.ItemPaths));
            if (this.TypeSource != null)
            {
                this.table = this.TypeSource.View.Table;
                this.items = new List<TypeMember>(this.table.Rows.Count);
                for (var i = 0; i < this.table.Rows.Count; i++)
                {
                    var item = this.table.Rows[i];
                    this.items.Add(new TypeMember(this, item));
                }
                this.table.RowDeleted += Table_RowDeleted;
                this.table.RowChanged += Table_RowChanged;
            }

            this.IsModified = this.domain.IsModified;
            this.domain.Dispatcher.Invoke(this.AttachDomainEvent);
            this.domain.Dispatcher.Invoke(this.RefreshEditors);
            this.ServiceState = ServiceState.Opened;
        }

        protected virtual void OnDetach()
        {
            this.domain.Dispatcher.Invoke(this.DetachDomainEvent);
            this.domain = null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnValidateAddNew(Authentication authentication, object target)
        {

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnValidateBeginEdit(Authentication authentication, object target)
        {

        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnValidateEndEdit(Authentication authentication, object target)
        {
            if (this.TypeSource.Members.Any() == false)
                throw new InvalidOperationException(Resources.Exception_AtLeastOneMemberInType);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnValidateCancelEdit(Authentication authentication, object target)
        {

        }

        protected CremaDataType TypeSource { get; private set; }

        protected abstract Task<CremaDataType> CreateSourceAsync(Authentication authentication);

        private async void Table_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var member = this.items.FirstOrDefault(item => item.Row == e.Row);
                if (member == null)
                    System.Diagnostics.Debugger.Launch();
                this.items.Remove(member);
            });
        }

        private async void Table_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.items.Add(new TypeMember(this, e.Row));
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

        #region ITypeTemplate

        async Task<ITypeMember> ITypeTemplate.AddNewAsync(Authentication authentication)
        {
            return await this.AddNewAsync(authentication);
        }

        async Task ITypeTemplate.EndNewAsync(Authentication authentication, ITypeMember member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (member is TypeMember == false)
                throw new ArgumentException(nameof(member));
            await this.EndNewAsync(authentication, member as TypeMember);
        }

        IType ITypeTemplate.Type => this.Type;

        IDomain ITypeTemplate.Domain => this.Domain;

        ITypeMember ITypeTemplate.this[string columnName] => this[columnName];

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

        async Task IDomainHost.DeleteAsync(Authentication authentication, bool isCanceled)
        {
            var domain = this.domain;
            if (isCanceled == false)
            {
                await this.Dispatcher.InvokeAsync(() => this.ValidateEndEdit(authentication));
                await this.OnEndEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = ServiceState.Closed;
                    this.OnEditEnded(new DomainDeletedEventArgs(authentication, domain, isCanceled));
                });
            }
            else
            {
                await this.Dispatcher.InvokeAsync(() => this.ValidateCancelEdit(authentication));
                await this.OnCancelEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = ServiceState.Closed;
                    this.OnEditCanceled(new DomainDeletedEventArgs(authentication, domain, isCanceled));
                });
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator<ITypeMember> IEnumerable<ITypeMember>.GetEnumerator()
        {
            return (this.items ?? Enumerable.Empty<ITypeMember>()).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this.items ?? Enumerable.Empty<ITypeMember>()).GetEnumerator();
        }

        #endregion
    }
}

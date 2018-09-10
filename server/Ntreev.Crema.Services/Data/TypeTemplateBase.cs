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

        private readonly List<TypeMember> members = new List<TypeMember>();

        private EventHandler editBegun;
        private EventHandler editEnded;
        private EventHandler editCanceled;
        private EventHandler changed;

        private bool isModified;

        public Task<TypeMember> AddNewAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    return new TypeMember(this, this.TypeSource.View.Table);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task EndNewAsync(Authentication authentication, TypeMember member)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(async () =>
                {
                    this.table.RowChanged -= Table_RowChanged;
                    try
                    {
                        await member.EndNewAsync(authentication);
                        this.members.Add(member);
                    }
                    finally
                    {
                        this.table.RowChanged += Table_RowChanged;
                    }
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task BeginEditAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginEditAsync));
                    this.ValidateBeginEdit(authentication);
                    this.Sign(authentication);
                    this.OnBeginEdit(authentication);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndEditAsync));
                    this.ValidateEndEdit(authentication);
                    this.Sign(authentication);
                    await this.OnEndEditAsync(authentication);
                    this.OnEditEnded(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task CancelEditAsync(Authentication authentication)
        {
            try
            {
                return this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CancelEditAsync));
                    this.ValidateCancelEdit(authentication);
                    this.Sign(authentication);
                    this.OnCancelEdit(authentication);
                    this.OnEditCanceled(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void ValidateBeginEdit(Authentication authentication)
        {
            if (this.domain != null)
                throw new InvalidOperationException(Resources.Exception_ItIsAlreadyBeingEdited);
            this.OnValidateBeginEdit(authentication, this);
        }

        public void ValidateEndEdit(Authentication authentication)
        {
            if (this.domain == null)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
            this.OnValidateEndEdit(authentication, this);
        }

        public void ValidateCancelEdit(Authentication authentication)
        {
            if (this.domain == null)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
            this.OnValidateCancelEdit(authentication, this);
        }

        public Task SetTypeNameAsync(Authentication authentication, string value)
        {
            try
            {
                return this.domain.SetPropertyAsync(authentication, CremaSchema.TypeName, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetTagsAsync(Authentication authentication, TagInfo value)
        {
            try
            {
                return this.domain.SetPropertyAsync(authentication, CremaSchema.Tags, (string)value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetIsFlagAsync(Authentication authentication, bool value)
        {
            try
            {
                return this.domain.SetPropertyAsync(authentication, CremaSchema.IsFlag, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetCommentAsync(Authentication authentication, string value)
        {
            try
            {
                return this.domain.SetPropertyAsync(authentication, CremaSchema.Comment, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<bool> ContainsAsync(string memberName)
        {
            return this.Dispatcher.InvokeAsync(() => this.members.Any(item => item.Name == memberName));
        }

        public bool IsNew { get; set; }

        public Domain Domain => this.domain;

        public abstract IPermission Permission { get; }

        public int Count => this.members.Count;

        public abstract DomainContext DomainContext { get; }

        public abstract string ItemPath { get; }

        public abstract CremaHost CremaHost { get; }

        public abstract IType Type { get; }

        public abstract DataBase DataBase { get; }

        public abstract IDispatcherObject DispatcherObject { get; }

        public CremaDispatcher Dispatcher => this.domain != null ? this.domain.Dispatcher : this.DispatcherObject.Dispatcher;

        public string TypeName => this.TypeSource.Name;

        public bool IsFlag => this.TypeSource.IsFlag;

        public string Comment => this.TypeSource.Comment;

        public TypeMember this[string memberName] => this.members.FirstOrDefault(item => item.Name == memberName);

        public bool IsModified => this.isModified;

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

        protected virtual void OnBeginEdit(Authentication authentication)
        {
            this.TypeSource = this.CreateSource(authentication);
            this.domain = new TypeDomain(authentication, this.TypeSource, this.DataBase, this.ItemPath, this.GetType().Name)
            {
                IsNew = this.IsNew,
                Host = this
            };

            this.table = this.TypeSource.View.Table;
            for (var i = 0; i < this.table.Rows.Count; i++)
            {
                var item = this.table.Rows[i];
                this.members.Add(new TypeMember(this, item));
            }
            this.table.RowDeleted += Table_RowDeleted;
            this.table.RowChanged += Table_RowChanged;

            this.DomainContext.Domains.Add(authentication, this.domain, this.DataBase);
            this.domain.Dispatcher.Invoke(async () =>
            {
                this.AttachDomainEvent();
                await this.domain.AddUserAsync(authentication, DomainAccessType.ReadWrite);
            });
        }

        protected virtual async Task OnEndEditAsync(Authentication authentication)
        {
            this.DetachDomainEvent();
            this.domain.Dispose(authentication, false);
            this.domain = null;
            if (this.table != null)
            {
                this.table.RowDeleted -= Table_RowDeleted;
                this.table.RowChanged -= Table_RowChanged;
            }
            this.isModified = false;
            this.table = null;
            this.members.Clear();
            await Task.Delay(0);
        }

        protected virtual void OnCancelEdit(Authentication authentication)
        {
            this.DetachDomainEvent();
            this.domain.Dispose(authentication, true);
            this.domain = null;
            if (this.table != null)
            {
                this.table.RowDeleted -= Table_RowDeleted;
                this.table.RowChanged -= Table_RowChanged;
            }
            this.isModified = false;
            this.table = null;
            this.members.Clear();
        }

        protected virtual void OnRestore(Domain domain)
        {
            this.TypeSource = domain.Source as CremaDataType;
            this.domain = domain as TypeDomain;

            if (this.TypeSource != null)
            {
                this.table = this.TypeSource.View.Table;
                for (var i = 0; i < this.table.Rows.Count; i++)
                {
                    var item = this.table.Rows[i];
                    this.members.Add(new TypeMember(this, item));
                }
                this.table.RowDeleted += Table_RowDeleted;
                this.table.RowChanged += Table_RowChanged;
            }

            this.domain.Dispatcher.Invoke(() =>
            {
                this.isModified = this.domain.IsModified;
                this.AttachDomainEvent();
            });
        }

        protected virtual void OnDetached()
        {
            this.domain.Dispatcher.Invoke(() =>
            {
                this.DetachDomainEvent();
            });
            this.domain = null;
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

        protected abstract CremaDataType CreateSource(Authentication authentication);

        protected void Sign(Authentication authentication)
        {
            authentication.Sign();
        }

        private void Table_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            var column = this.members.FirstOrDefault(item => item.Row == e.Row);
            this.members.Remove(column);
        }

        private void Table_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (e.Action == DataRowAction.Add)
            {
                this.Dispatcher.InvokeAsync(() => this.members.Add(new TypeMember(this, e.Row)));
            }
        }

        private async void Domain_Deleted(object sender, DomainDeletedEventArgs e)
        {
            if (e.IsCanceled == false)
            {
                await this.OnEndEditAsync(e.Authentication);
                this.OnEditEnded(e);
            }
            else
            {
                this.OnCancelEdit(e.Authentication);
                this.OnEditCanceled(e);
            }
        }

        private void Domain_RowAdded(object sender, DomainRowEventArgs e)
        {
            this.isModified = this.domain.IsModified;
            this.OnChanged(e);
        }

        private void Domain_RowChanged(object sender, DomainRowEventArgs e)
        {
            this.isModified = this.domain.IsModified;
            this.OnChanged(e);
        }

        private void Domain_RowRemoved(object sender, DomainRowEventArgs e)
        {
            this.isModified = this.domain.IsModified;
            this.OnChanged(e);
        }

        private void Domain_PropertyChanged(object sender, DomainPropertyEventArgs e)
        {
            this.isModified = this.domain.IsModified;
            this.OnChanged(e);
        }

        private void AttachDomainEvent()
        {
            this.domain.Deleted += Domain_Deleted;
            this.domain.RowAdded += Domain_RowAdded;
            this.domain.RowChanged += Domain_RowChanged;
            this.domain.RowRemoved += Domain_RowRemoved;
            this.domain.PropertyChanged += Domain_PropertyChanged;
        }

        private void DetachDomainEvent()
        {
            this.domain.Deleted -= Domain_Deleted;
            this.domain.RowAdded -= Domain_RowAdded;
            this.domain.RowChanged -= Domain_RowChanged;
            this.domain.RowRemoved -= Domain_RowRemoved;
            this.domain.PropertyChanged -= Domain_PropertyChanged;
        }

        private void ValidateDispatcher(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
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

        IType ITypeTemplate.Type
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.Type;
            }
        }

        IDomain ITypeTemplate.Domain
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.Domain;
            }
        }

        ITypeMember ITypeTemplate.this[string columnName]
        {
            get { return this[columnName]; }
        }

        #endregion

        #region IDomainHost

        void IDomainHost.Restore(Authentication authentication, Domain domain)
        {
            this.OnRestore(domain);
            this.OnEditBegun(EventArgs.Empty);
        }

        void IDomainHost.Detach()
        {
            this.OnDetached();
        }

        void IDomainHost.ValidateDelete(Authentication authentication, bool isCanceled)
        {
            if (isCanceled == false)
            {
                this.Dispatcher.Invoke(() => this.ValidateEndEdit(authentication));
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator<ITypeMember> IEnumerable<ITypeMember>.GetEnumerator()
        {
            return this.members.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.members.GetEnumerator();
        }

        #endregion
    }
}

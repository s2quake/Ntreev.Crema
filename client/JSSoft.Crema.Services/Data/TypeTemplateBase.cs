// Released under the MIT License.
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
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
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

        public string TypeName => this.TypeSource.Name;

        public bool IsFlag => this.TypeSource.IsFlag;

        public string Comment => this.TypeSource.Comment;

        public TypeMember this[string memberName] => this.items.FirstOrDefault(item => item.Name == memberName);

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
            this.domain = await this.DomainContext.CreateAsync(authentication, metaData) as TypeDomain;
            this.domain.IsNew = this.IsNew;
            this.domain.Host = this;
            await this.domain.WaitUserEnterAsync(authentication);
            this.TypeSource = this.domain.Source as CremaDataType;

            this.table = this.TypeSource.View.Table;
            this.items = new List<TypeMember>(this.table.Rows.Count);
            for (var i = 0; i < this.table.Rows.Count; i++)
            {
                var item = this.table.Rows[i];
                this.items.Add(new TypeMember(this, item));
            }
            this.table.RowDeleted += Table_RowDeleted;
            this.table.RowChanged += Table_RowChanged;

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
            this.TypeSource = domain.Source as CremaDataType;
            this.domain = domain as TypeDomain;
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
                this.IsModified = this.domain.IsModified;
                this.domain.Dispatcher.Invoke(this.AttachDomainEvent);
                this.domain.Dispatcher.Invoke(this.RefreshEditors);
            }
        }

        protected virtual void OnDetached()
        {
            if (this.TypeSource != null)
            {
                this.domain.Dispatcher.Invoke(this.DetachDomainEvent);
            }
            this.domain = null;
        }

        protected CremaDataType TypeSource { get; private set; }

        protected abstract Task<ResultBase<DomainMetaData>> OnBeginDomainAsync(Authentication authentication);

        protected abstract Task<ResultBase<TypeInfo[]>> OnEndDomainAsync(Authentication authentication);

        protected abstract Task<ResultBase> OnCancelDomainAsync(Authentication authentication);

        private async void Table_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var member = this.items.FirstOrDefault(item => item.Row == e.Row);
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

        //private async void Domain_Deleted(object sender, DomainDeletedEventArgs e)
        //{
        //    if (e.IsCanceled == false)
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


        //private Task<ResultBase<DomainMetaData>> BeginDomainAsync(Authentication authentication)
        //{
        //    return this.OnBeginDomainAsync(authentication);
        //}

        //private async Task EndDomainAsync(Authentication authentication)
        //{
        //    try
        //    {
        //        this.domain.Host = null;
        //        await this.OnEndDomainAsync(authentication);
        //        await this.DomainContext.WaitDeleteAsync(this.domain);
        //    }
        //    catch
        //    {
        //        this.domain.Host = this;
        //        throw;
        //    }
        //}

        //private async Task CancelDomainAsync(Authentication authentication)
        //{
        //    try
        //    {
        //        this.domain.Host = null;
        //        await this.OnCancelDomainAsync(authentication);
        //        await this.DomainContext.WaitDeleteAsync(this.domain);
        //    }
        //    catch
        //    {
        //        this.domain.Host = this;
        //        throw;
        //    }
        //}


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
        }

        void IDomainHost.Detach()
        {
            this.OnDetached();
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

        IEnumerator<ITypeMember> IEnumerable<ITypeMember>.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        #endregion
    }
}

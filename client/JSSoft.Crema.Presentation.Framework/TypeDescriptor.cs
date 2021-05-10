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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System;

namespace JSSoft.Crema.Presentation.Framework
{
    public class TypeDescriptor : DescriptorBase, ITypeItemDescriptor, ITypeDescriptor, IPermissionDescriptor, ILockableDescriptor, IAccessibleDescriptor
    {
        private IType type;

        public TypeDescriptor(Authentication authentication, ITypeDescriptor descriptor, bool isSubscriptable, object owner)
            : base(authentication, descriptor.Target, descriptor, isSubscriptable)
        {
            this.type = descriptor.Target;
            this.Owner = owner ?? this;
        }

        public TypeDescriptor(Authentication authentication, IType type, DescriptorTypes descriptorTypes, object owner)
            : base(authentication, type, descriptorTypes)
        {
            this.type = type;
            this.Owner = owner ?? this;
            this.type.Dispatcher.VerifyAccess();
            this.TypeInfo = type.TypeInfo;
            this.TypeState = type.TypeState;
            if (this.type.TypeInfo.IsFlag == true)
                this.TypeAttribute |= TypeAttribute.IsFlag;
            this.LockInfo = type.LockInfo;
            this.AccessInfo = type.AccessInfo;
            this.AccessType = type.GetAccessType(this.authentication);
            this.TemplateDescriptor = new TypeTemplateDescriptor(authentication, type.Template, descriptorTypes);

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.type.Deleted += Type_Deleted;
                this.type.LockChanged += Type_LockChanged;
                this.type.AccessChanged += Type_AccessChanged;
                this.type.TypeInfoChanged += Type_TypeInfoChanged;
                this.type.TypeStateChanged += Type_TypeStateChanged;
            }
        }

        [DescriptorProperty]
        public string Name => this.TypeInfo.Name;

        [DescriptorProperty]
        public string TypeName => this.TypeInfo.Name;

        [DescriptorProperty]
        public string Path => this.TypeInfo.CategoryPath + this.TypeInfo.Name;

        [DescriptorProperty]
        public string DisplayName => this.TypeInfo.Name;

        [DescriptorProperty]
        public TypeInfo TypeInfo { get; private set; } = TypeInfo.Default;

        [DescriptorProperty]
        public TypeState TypeState { get; private set; }

        [DescriptorProperty]
        public TypeAttribute TypeAttribute { get; private set; }

        [DescriptorProperty]
        public bool IsBeingEdited => TypeDescriptorUtility.IsBeingEdited(this.authentication, this);

        [DescriptorProperty]
        public bool IsContentEditor => TypeDescriptorUtility.IsBeingEdited(this.authentication, this) && this.TemplateDescriptor != null && this.TemplateDescriptor.Editor == this.authentication.ID;

        [DescriptorProperty]
        public bool IsFlag => TypeDescriptorUtility.IsFlag(this.authentication, this);

        [DescriptorProperty]
        public LockInfo LockInfo { get; private set; } = LockInfo.Empty;

        [DescriptorProperty]
        public AccessInfo AccessInfo { get; private set; } = AccessInfo.Empty;

        [DescriptorProperty]
        public AccessType AccessType { get; private set; }

        [DescriptorProperty]
        public bool IsLocked => LockableDescriptorUtility.IsLocked(this.authentication, this);

        [DescriptorProperty]
        public bool IsLockInherited => LockableDescriptorUtility.IsLockInherited(this.authentication, this);

        [DescriptorProperty]
        public bool IsLockOwner => LockableDescriptorUtility.IsLockOwner(this.authentication, this);

        [DescriptorProperty]
        public bool IsPrivate => AccessibleDescriptorUtility.IsPrivate(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessInherited => AccessibleDescriptorUtility.IsAccessInherited(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessOwner => AccessibleDescriptorUtility.IsAccessOwner(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessMember => AccessibleDescriptorUtility.IsAccessMember(this.authentication, this);

        [DescriptorProperty]
        public TypeTemplateDescriptor TemplateDescriptor { get; }

        protected async override void OnDisposed(EventArgs e)
        {
            if (this.referenceTarget == null && this.type != null)
            {
                await this.type.Dispatcher.InvokeAsync(() =>
                {
                    if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                    {
                        this.type.Deleted -= Type_Deleted;
                        this.type.LockChanged -= Type_LockChanged;
                        this.type.AccessChanged -= Type_AccessChanged;
                        this.type.TypeInfoChanged -= Type_TypeInfoChanged;
                        this.type.TypeStateChanged -= Type_TypeStateChanged;
                    }
                });
            }
            base.OnDisposed(e);
        }

        protected object Owner { get; }

        private void Type_Deleted(object sender, EventArgs e)
        {
            this.type = null;
            this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDisposed(EventArgs.Empty);
            });
        }

        private async void Type_LockChanged(object sender, EventArgs e)
        {
            this.LockInfo = this.type.LockInfo;
            this.AccessType = this.type.GetAccessType(this.authentication);
            await this.RefreshAsync();
        }

        private async void Type_AccessChanged(object sender, EventArgs e)
        {
            this.AccessInfo = this.type.AccessInfo;
            this.AccessType = this.type.GetAccessType(this.authentication);
            await this.RefreshAsync();
        }

        private async void Type_TypeInfoChanged(object sender, EventArgs e)
        {
            this.TypeInfo = this.type.TypeInfo;
            if (this.TypeInfo.IsFlag == true)
                this.TypeAttribute |= TypeAttribute.IsFlag;
            else
                this.TypeAttribute &= ~TypeAttribute.IsFlag;
            await this.RefreshAsync();
        }

        private async void Type_TypeStateChanged(object sender, EventArgs e)
        {
            this.TypeState = this.type.TypeState;
            await this.RefreshAsync();
        }

        #region ITypeItemDescriptor

        ITypeItem ITypeItemDescriptor.Target => this.type as ITypeItem;

        #endregion

        #region ITypeDescriptor

        IType ITypeDescriptor.Target => this.type;

        #endregion

        #region ILockableDescriptor

        ILockable ILockableDescriptor.Target => this.type as ILockable;

        #endregion

        #region IAccessibleDescriptor

        IAccessible IAccessibleDescriptor.Target => this.type as IAccessible;

        #endregion

        #region IPermissionDescriptor

        IPermission IPermissionDescriptor.Target => this.type as IPermission;

        #endregion
    }
}

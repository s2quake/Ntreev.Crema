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
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework
{
    public class TypeListItemBase : DescriptorListItemBase<TypeDescriptor>, ITypeDescriptor, IAccessibleDescriptor, IPermissionDescriptor, ILockableDescriptor, IPartImportsSatisfiedNotification
    {
        public TypeListItemBase(Authentication authentication, IType type, object owner)
            : base(authentication, new TypeDescriptor(authentication, type, DescriptorTypes.IsSubscriptable, owner), owner)
        {

        }

        public TypeListItemBase(Authentication authentication, ITypeDescriptor descriptor, object owner)
            : base(authentication, new TypeDescriptor(authentication, descriptor, true, owner), owner)
        {

        }

        public async Task LockAsync()
        {
            await LockableDescriptorUtility.LockAsync(this.authentication, this.descriptor);
        }

        public async Task UnlockAsync()
        {
            await LockableDescriptorUtility.UnlockAsync(this.authentication, this.descriptor);
        }

        public async Task SetPrivateAsync()
        {
            await AccessibleDescriptorUtility.SetPrivateAsync(this.authentication, this.descriptor);
        }

        public async Task SetPublicAsync()
        {
            await AccessibleDescriptorUtility.SetPublicAsync(this.authentication, this.descriptor);
        }

        public async Task SetAuthorityAsync()
        {
            await AccessibleDescriptorUtility.SetAuthorityAsync(this.authentication, this.descriptor);
        }

        [DescriptorProperty]
        public bool CanLock => LockableDescriptorUtility.CanLock(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanUnlock => LockableDescriptorUtility.CanUnlock(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanSetPrivate => AccessibleDescriptorUtility.CanSetPrivate(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanSetPublic => AccessibleDescriptorUtility.CanSetPublic(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanSetAuthority => AccessibleDescriptorUtility.CanSetAuthority(this.authentication, this.descriptor);

        public string Name => this.descriptor.Name;

        public string TypeName => this.descriptor.TypeName;

        public string Path => this.descriptor.Path;

        public TypeInfo TypeInfo => this.descriptor.TypeInfo;

        public TypeState TypeState => this.descriptor.TypeState;

        public TypeAttribute TypeAttribute => this.descriptor.TypeAttribute;

        public bool IsBeingEdited => this.descriptor.IsBeingEdited;

        public bool IsContentEditor => this.descriptor.IsContentEditor;

        public bool IsFlag => this.descriptor.IsFlag;

        public LockInfo LockInfo => this.descriptor.LockInfo;

        public AccessInfo AccessInfo => this.descriptor.AccessInfo;

        public AccessType AccessType => this.descriptor.AccessType;

        public bool IsLocked => this.descriptor.IsLocked;

        public bool IsLockInherited => this.descriptor.IsLockInherited;

        public bool IsLockOwner => this.descriptor.IsLockOwner;

        public bool IsPrivate => this.descriptor.IsPrivate;

        public bool IsAccessInherited => this.descriptor.IsAccessInherited;

        public bool IsAccessOwner => this.descriptor.IsAccessOwner;

        public bool IsAccessMember => this.descriptor.IsAccessMember;

        public override string DisplayName => this.descriptor.TypeName;

        #region IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {

        }

        #endregion

        #region ITypeObject

        IType ITypeDescriptor.Target => this.descriptor.Target as IType;

        #endregion

        #region ILockableDescriptor

        ILockable ILockableDescriptor.Target => this.descriptor.Target as ILockable;

        #endregion

        #region IAccessibleDescriptor

        IAccessible IAccessibleDescriptor.Target => this.descriptor.Target as IAccessible;

        #endregion

        #region IPermissionDescriptor

        IPermission IPermissionDescriptor.Target => this.descriptor.Target as IPermission;

        #endregion
    }
}
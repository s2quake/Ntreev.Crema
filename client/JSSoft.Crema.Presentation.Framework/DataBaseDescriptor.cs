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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System;

namespace JSSoft.Crema.Presentation.Framework
{
    public class DataBaseDescriptor : DescriptorBase, IDataBaseDescriptor, IPermissionDescriptor, ILockableDescriptor, IAccessibleDescriptor
    {
        private IDataBase dataBase;
        private DataBaseInfo dataBaseInfo = DataBaseInfo.Empty;
        private LockInfo lockInfo = LockInfo.Empty;
        private AccessInfo accessInfo = AccessInfo.Empty;

        public DataBaseDescriptor(Authentication authentication, DataBaseInfo dataBaseInfo)
            : base(authentication, null, DescriptorTypes.None)
        {
            this.dataBaseInfo = dataBaseInfo;
            this.Owner = this;
        }

        public DataBaseDescriptor(Authentication authentication, IDataBaseDescriptor descriptor, bool isSubscriptable, object owner)
            : base(authentication, descriptor.Target, descriptor, isSubscriptable)
        {
            this.dataBase = descriptor.Target;
            this.Owner = owner ?? this;
        }

        public DataBaseDescriptor(Authentication authentication, IDataBase dataBase, DescriptorTypes descriptorTypes, object owner)
            : base(authentication, dataBase, descriptorTypes)
        {
            this.dataBase = dataBase;
            this.Owner = owner ?? this;
            this.dataBase.Dispatcher.VerifyAccess();
            this.dataBaseInfo = dataBase.DataBaseInfo;
            this.DataBaseState = dataBase.DataBaseState;
            this.AuthenticationInfos = dataBase.AuthenticationInfos;
            this.lockInfo = dataBase.LockInfo;
            this.accessInfo = dataBase.AccessInfo;
            this.AccessType = dataBase.GetAccessType(authentication);

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.dataBase.Renamed += DataBase_Renamed;
                this.dataBase.Deleted += DataBase_Deleted;
                this.dataBase.Loaded += DataBase_Loaded;
                this.dataBase.Unloaded += DataBase_Unloaded;
                this.dataBase.AuthenticationEntered += DataBase_AuthenticationEntered;
                this.dataBase.AuthenticationLeft += DataBase_AuthenticationLeft;
                this.dataBase.DataBaseInfoChanged += DataBase_DataBaseInfoChanged;
                this.dataBase.DataBaseStateChanged += DataBase_DataBaseStateChanged;
                this.dataBase.AccessChanged += DataBase_AccessChanged;
                this.dataBase.LockChanged += DataBase_LockChanged;
            }
        }

        [DescriptorProperty]
        public string Name => this.dataBaseInfo.Name;

        [DescriptorProperty]
        public string Path => this.dataBaseInfo.Name;

        [DescriptorProperty]
        public Guid DataBaseID => this.dataBaseInfo.ID;

        [DescriptorProperty(nameof(dataBaseInfo))]
        public DataBaseInfo DataBaseInfo => this.dataBaseInfo;

        [DescriptorProperty]
        public DataBaseState DataBaseState { get; private set; }

        [DescriptorProperty]
        public AuthenticationInfo[] AuthenticationInfos { get; private set; }

        [DescriptorProperty(nameof(lockInfo))]
        public LockInfo LockInfo => this.lockInfo;

        [DescriptorProperty(nameof(accessInfo))]
        public AccessInfo AccessInfo => this.accessInfo;

        [DescriptorProperty]
        public AccessType AccessType { get; private set; }

        [DescriptorProperty]
        public bool IsLoaded => DataBaseDescriptorUtility.IsLoaded(Authenticator.Current, this);

        [DescriptorProperty]
        public bool IsLocked => this.authentication != null && LockableDescriptorUtility.IsLocked(this.authentication, this);

        [DescriptorProperty]
        public bool IsLockInherited => this.authentication != null && LockableDescriptorUtility.IsLockInherited(this.authentication, this);

        [DescriptorProperty]
        public bool IsLockOwner => this.authentication != null && LockableDescriptorUtility.IsLockOwner(this.authentication, this);

        [DescriptorProperty]
        public bool IsPrivate => this.authentication != null && AccessibleDescriptorUtility.IsPrivate(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessInherited => this.authentication != null && AccessibleDescriptorUtility.IsAccessInherited(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessOwner => this.authentication != null && AccessibleDescriptorUtility.IsAccessOwner(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessMember => this.authentication != null && AccessibleDescriptorUtility.IsAccessMember(this.authentication, this);

        protected async override void OnDisposed(EventArgs e)
        {
            if (this.referenceTarget == null && this.dataBase != null)
            {
                await this.dataBase.Dispatcher.InvokeAsync(() =>
                {
                    if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                    {
                        this.dataBase.Renamed -= DataBase_Renamed;
                        this.dataBase.Deleted -= DataBase_Deleted;
                        this.dataBase.Loaded -= DataBase_Loaded;
                        this.dataBase.Unloaded -= DataBase_Unloaded;
                        this.dataBase.AuthenticationEntered -= DataBase_AuthenticationEntered;
                        this.dataBase.AuthenticationLeft -= DataBase_AuthenticationLeft;
                        this.dataBase.DataBaseInfoChanged -= DataBase_DataBaseInfoChanged;
                        this.dataBase.DataBaseStateChanged -= DataBase_DataBaseStateChanged;
                        this.dataBase.AccessChanged -= DataBase_AccessChanged;
                        this.dataBase.LockChanged -= DataBase_LockChanged;
                    }
                });
            }
            base.OnDisposed(e);
        }

        protected object Owner { get; }

        private async void DataBase_Renamed(object sender, EventArgs e)
        {
            this.dataBaseInfo = this.dataBase.DataBaseInfo;
            await this.RefreshAsync();
        }

        private void DataBase_Deleted(object sender, EventArgs e)
        {
            this.dataBase = null;
            this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDisposed(EventArgs.Empty);
            });
        }

        private async void DataBase_Loaded(object sender, EventArgs e)
        {
            this.DataBaseState = this.dataBase.DataBaseState;
            await this.RefreshAsync();
        }

        private async void DataBase_Unloaded(object sender, EventArgs e)
        {
            this.DataBaseState = this.dataBase.DataBaseState;
            this.AuthenticationInfos = this.dataBase.AuthenticationInfos;
            await this.RefreshAsync();
        }

        private async void DataBase_AuthenticationEntered(object sender, AuthenticationEventArgs e)
        {
            this.AuthenticationInfos = this.dataBase.AuthenticationInfos;
            await this.RefreshAsync();
        }

        private async void DataBase_AuthenticationLeft(object sender, AuthenticationEventArgs e)
        {
            this.AuthenticationInfos = this.dataBase.AuthenticationInfos;
            await this.RefreshAsync();
        }

        private async void DataBase_DataBaseInfoChanged(object sender, EventArgs e)
        {
            this.dataBaseInfo = this.dataBase.DataBaseInfo;
            await this.RefreshAsync();
        }

        private async void DataBase_DataBaseStateChanged(object sender, EventArgs e)
        {
            this.DataBaseState = this.dataBase.DataBaseState;
            await this.RefreshAsync();
        }

        private async void DataBase_LockChanged(object sender, EventArgs e)
        {
            this.lockInfo = this.dataBase.LockInfo;
            this.AccessType = this.dataBase.GetAccessType(this.authentication);
            await this.RefreshAsync();
        }

        private async void DataBase_AccessChanged(object sender, EventArgs e)
        {
            this.accessInfo = this.dataBase.AccessInfo;
            this.AccessType = this.dataBase.GetAccessType(this.authentication);
            await this.RefreshAsync();
        }

        #region IDataBaseDescriptor

        IDataBase IDataBaseDescriptor.Target => this.dataBase as IDataBase;

        #endregion

        #region ILockableDescriptor

        ILockable ILockableDescriptor.Target => this.dataBase as ILockable;

        #endregion

        #region IAccessibleDescriptor

        IAccessible IAccessibleDescriptor.Target => this.dataBase as IAccessible;

        #endregion

        #region IPermissionDescriptor

        IPermission IPermissionDescriptor.Target => this.dataBase as IPermission;

        #endregion
    }
}

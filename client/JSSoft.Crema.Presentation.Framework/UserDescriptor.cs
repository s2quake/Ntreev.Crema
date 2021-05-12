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
    public class UserDescriptor : DescriptorBase, IUserDescriptor, IUserItemDescriptor
    {
        private IUser user;
        private UserInfo userInfo;
        private BanInfo banInfo = BanInfo.Empty;

        public UserDescriptor(Authentication authentication, IUserDescriptor descriptor, bool isSubscriptable, object owner)
            : base(authentication, descriptor.Target, descriptor, isSubscriptable)
        {
            this.user = descriptor.Target;
            this.Owner = owner ?? this;
        }

        public UserDescriptor(Authentication authentication, IUser user, DescriptorTypes descriptorTypes, object owner)
            : base(authentication, user, descriptorTypes)
        {
            this.user = user;
            this.Owner = owner ?? this;
            this.user.Dispatcher.VerifyAccess();
            this.userInfo = this.user.UserInfo;
            this.UserState = this.user.UserState;
            this.banInfo = this.user.BanInfo;

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.user.Deleted += User_Deleted;
                this.user.UserInfoChanged += User_UserInfoChanged;
                this.user.UserStateChanged += User_UserStateChanged;
                this.user.UserBanInfoChanged += User_UserBanInfoChanged;
            }
        }

        [DescriptorProperty]
        public string UserID => this.userInfo.ID;

        [DescriptorProperty]
        public string DisplayName => this.userInfo.ID + " [" + this.userInfo.Name + "]";

        [DescriptorProperty(nameof(userInfo))]
        public UserInfo UserInfo => this.userInfo;

        [DescriptorProperty]
        public UserState UserState { get; private set; }

        [DescriptorProperty(nameof(banInfo))]
        public BanInfo BanInfo => this.banInfo;

        [DescriptorProperty]
        public bool IsOnline => UserDescriptorUtility.IsOnline(this.authentication, this);

        [DescriptorProperty]
        public bool IsBanned => UserDescriptorUtility.IsBanned(this.authentication, this);

        [DescriptorProperty]
        public bool IsAdmin => UserDescriptorUtility.IsAdmin(this.authentication, this);

        [DescriptorProperty]
        public bool IsMember => UserDescriptorUtility.IsMember(this.authentication, this);

        [DescriptorProperty]
        public bool IsGuest => UserDescriptorUtility.IsGuest(this.authentication, this);

        protected async override void OnDisposed(EventArgs e)
        {
            if (this.referenceTarget == null && this.user != null)
            {
                await this.user.Dispatcher.InvokeAsync(() =>
                {
                    if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                    {
                        this.user.Deleted -= User_Deleted;
                        this.user.UserInfoChanged -= User_UserInfoChanged;
                        this.user.UserStateChanged -= User_UserStateChanged;
                        this.user.UserBanInfoChanged -= User_UserBanInfoChanged;
                    }
                });
            }
            base.OnDisposed(e);
        }

        protected object Owner { get; }

        private void User_Deleted(object sender, EventArgs e)
        {
            this.user = null;
            this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDisposed(EventArgs.Empty);
            });
        }

        private async void User_UserInfoChanged(object sender, EventArgs e)
        {
            this.userInfo = this.user.UserInfo;
            await this.RefreshAsync();
        }

        private async void User_UserStateChanged(object sender, EventArgs e)
        {
            this.UserState = this.user.UserState;
            await this.RefreshAsync();
        }

        private async void User_UserBanInfoChanged(object sender, EventArgs e)
        {
            this.banInfo = this.user.BanInfo;
            await this.RefreshAsync();
        }

        #region IUserItemDescriptor

        IUserItem IUserItemDescriptor.Target => this.user as IUserItem;

        string IUserItemDescriptor.Name => this.userInfo.ID;

        string IUserItemDescriptor.Path => this.userInfo.CategoryPath + this.userInfo.ID;

        #endregion

        #region IUserDescriptor

        IUser IUserDescriptor.Target => this.user;

        #endregion
    }
}

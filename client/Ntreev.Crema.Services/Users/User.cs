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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.UserService;
using Ntreev.Library.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    class User : UserBase<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUser, IUserItem, IInfoProvider, IStateProvider
    {
        private Authentication authentication;

        public User()
        {

        }

        public Task RenameAsync(Authentication authentication, string newName)
        {
            throw new NotSupportedException();
        }

        public async Task MoveAsync(Authentication authentication, string categoryPath)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, categoryPath);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldCategoryPaths = items.Select(item => item.Category.Path).ToArray();
                    var result = await this.Service.MoveUserItemAsync(this.Path, categoryPath);
                    this.CremaHost.Sign(authentication, result);
                    base.Move(authentication, categoryPath);
                    this.Container.InvokeUsersMovedEvent(authentication, items, oldPaths, oldCategoryPaths);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task DeleteAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var container = this.Container;
                    var result = await this.Service.DeleteUserItemAsync(this.Path);
                    this.CremaHost.Sign(authentication, result);
                    base.Delete(authentication);
                    container.InvokeUsersDeletedEvent(authentication, items, oldPaths);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task KickAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), this, comment);
                    var users = new User[] { this };
                    var comments = Enumerable.Repeat(comment, users.Length).ToArray();
                    var result = await this.Service.KickAsync(this.ID, comment ?? string.Empty);
                    this.CremaHost.Sign(authentication, result);
                    this.IsOnline = false;
                    this.Container.InvokeUsersKickedEvent(authentication, users, comments);
                    this.Container.InvokeUsersStateChangedEvent(authentication, users);
                    this.Container.InvokeUsersLoggedOutEvent(authentication, users, new CloseInfo(CloseReason.Kicked, comment));
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task BanAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BanAsync), this, comment);
                    var users = new User[] { this };
                    var comments = Enumerable.Repeat(comment, users.Length).ToArray();
                    var result = await this.Service.BanAsync(this.ID, comment ?? string.Empty);
                    this.CremaHost.Sign(authentication, result);
                    base.Ban(authentication, result.Value);
                    this.Container.InvokeUsersBannedEvent(authentication, users, comments);
                    if (this.IsOnline == true)
                    {
                        this.IsOnline = false;
                        this.Container.InvokeUsersStateChangedEvent(authentication, users);
                        this.Container.InvokeUsersLoggedOutEvent(authentication, users, new CloseInfo(CloseReason.Banned, comment));
                    }
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task UnbanAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnbanAsync), this);
                    var users = new User[] { this };
                    var result = await this.Service.UnbanAsync(this.ID);
                    this.CremaHost.Sign(authentication, result);
                    base.Unban(authentication);
                    this.Container.InvokeUsersUnbannedEvent(authentication, users);
                    this.Container.InvokeUsersStateChangedEvent(authentication, users);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task ChangeUserInfoAsync(Authentication authentication, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ChangeUserInfoAsync), this, userName, authority);
                    if (this.ID == authentication.ID && password == null && newPassword != null)
                        throw new ArgumentNullException(nameof(password));
                    if (newPassword == null && password != null)
                        throw new ArgumentNullException(nameof(newPassword));
                    var p1 = password == null ? null : UserContext.Encrypt(this.ID, password);
                    var p2 = newPassword == null ? null : UserContext.Encrypt(this.ID, newPassword);
                    var result = await this.Service.ChangeUserInfoAsync(this.UserInfo.ID, p1, p2, userName, authority);
                    this.CremaHost.Sign(authentication, result);
                    base.UpdateUserInfo(result.Value);
                    this.Container.InvokeUsersChangedEvent(authentication, new User[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SendMessageAsync(Authentication authentication, string message)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SendMessageAsync), this, message);
                    var result = await this.Service.SendMessageAsync(this.UserInfo.ID, message);
                    this.CremaHost.Sign(authentication, result);
                    this.Container.InvokeSendMessageEvent(authentication, this, message);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetUserInfo(UserInfo userInfo)
        {
            this.UpdateUserInfo(userInfo);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetUserState(UserState userState)
        {
            base.UserState = userState;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetBanInfo(BanChangeType changeType, BanInfo banInfo)
        {
            if (changeType == BanChangeType.Ban)
                base.BanInfo = banInfo;
            else
                base.BanInfo = BanInfo.Empty;
        }

        public string ID
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.Name;
            }
        }

        public string UserName
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.UserInfo.Name;
            }
        }

        public new string Path
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.Path;
            }
        }

        public new Authority Authority
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.Authority;
            }
        }

        public new UserInfo UserInfo
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.UserInfo;
            }
        }

        public new UserState UserState
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.UserState;
            }
        }

        public new BanInfo BanInfo
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.BanInfo;
            }
        }

        public bool IsBanned => this.BanInfo.Path != string.Empty;

        public Authentication Authentication => authentication;

        public IUserService Service => this.Context.Service;

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.CremaHost.Dispatcher;

        public new event EventHandler Renamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed -= value;
            }
        }

        public new event EventHandler Moved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved -= value;
            }
        }

        public new event EventHandler Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted -= value;
            }
        }

        public new event EventHandler UserInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.UserInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.UserInfoChanged -= value;
            }
        }

        public new event EventHandler UserStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.UserStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.UserStateChanged -= value;
            }
        }

        public new event EventHandler UserBanInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.UserBanInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.UserBanInfoChanged -= value;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.authentication = new Authentication(new UserAuthenticationProvider(this));
        }

        #region IUser

        string IUser.ID => this.ID;

        IUserCategory IUser.Category
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.Category;
            }
        }

        #endregion

        #region IUserItem

        string IUserItem.Name => this.ID;

        IUserItem IUserItem.Parent
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return this.Category;
            }
        }

        IEnumerable<IUserItem> IUserItem.Childs
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return Enumerable.Empty<IUserItem>();
            }
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.Context as IUserContext).GetService(serviceType);
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => this.UserInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.UserState;

        #endregion
    }
}

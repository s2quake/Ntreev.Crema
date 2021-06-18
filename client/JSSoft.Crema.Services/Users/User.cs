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

using JSSoft.Crema.ServiceHosts.Users;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Users
{
    class User : UserBase<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUser, IUserItem, IInfoProvider, IStateProvider
    {
        public User()
        {

        }

        public Task RenameAsync(Authentication authentication, string newName)
        {
            throw new NotSupportedException();
        }

        public async Task<Guid> MoveAsync(Authentication authentication, string categoryPath)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                 {
                     this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, categoryPath);
                     var items = EnumerableUtility.One(this).ToArray();
                     var oldPaths = items.Select(item => item.Path).ToArray();
                     var oldCategoryPaths = items.Select(item => item.Category.Path).ToArray();
                     var path = base.Path;
                     return (items, oldPaths, oldCategoryPaths, path);
                 });
                var result = await this.Service.MoveUserItemAsync(authentication.Token, this.Path, categoryPath);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> DeleteAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var context = this.Context;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldPaths, path);
                });
                var result = await this.Service.DeleteUserItemAsync(authentication.Token, tuple.path);
                await context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> KickAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), this, comment);
                    var items = new User[] { this };
                    var comments = Enumerable.Repeat(comment, items.Length).ToArray();
                    var id = this.ID;
                    return (items, comments, id);
                });
                var result = await this.Service.KickAsync(authentication.Token, tuple.id, comment ?? string.Empty);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> BanAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BanAsync), this, comment);
                    var items = new User[] { this };
                    var comments = Enumerable.Repeat(comment, items.Length).ToArray();
                    var id = this.ID;
                    return (items, comments, id);
                });
                var result = await this.Service.BanAsync(authentication.Token, tuple.id, comment ?? string.Empty);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> UnbanAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnbanAsync), this);
                    var items = new User[] { this };
                    var id = this.ID;
                    return (items, id);
                });
                var result = await this.Service.UnbanAsync(authentication.Token, tuple.id);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetUserNameAsync(Authentication authentication, SecureString password, string userName)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (password is null)
                    throw new ArgumentNullException(nameof(password));
                if (userName is null)
                    throw new ArgumentNullException(nameof(userName));

                this.ValidateExpired();
                var userInfo = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserNameAsync), this, userName);
                    return base.UserInfo;
                });
                // if (userInfo.ID == authentication.ID && password == null && newPassword != null)
                //     throw new ArgumentNullException(nameof(password));
                // if (newPassword == null && password != null)
                //     throw new ArgumentNullException(nameof(newPassword));
                var encryptedPassword = UserContext.Encrypt(userInfo.ID, password);
                // var p2 = newPassword == null ? null : UserContext.Encrypt(userInfo.ID, newPassword);
                var result = await this.Service.SetUserNameAsync(authentication.Token, userInfo.ID, encryptedPassword, userName);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetPasswordAsync(Authentication authentication, SecureString password, SecureString newPassword)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (password is null)
                    throw new ArgumentNullException(nameof(password));
                if (newPassword is null)
                    throw new ArgumentNullException(nameof(newPassword));

                this.ValidateExpired();
                var userInfo = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPasswordAsync), this);
                    return base.UserInfo;
                });
                var encryptedPassword1 = UserContext.Encrypt(userInfo.ID, password);
                var encryptedPassword2 = UserContext.Encrypt(userInfo.ID, newPassword);
                var result = await this.Service.SetPasswordAsync(authentication.Token, userInfo.ID, encryptedPassword1, encryptedPassword2);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> ResetPasswordAsync(Authentication authentication)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                this.ValidateExpired();
                var userInfo = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ResetPasswordAsync), this);
                    return base.UserInfo;
                });
                var result = await this.Service.ResetPasswordAsync(authentication.Token, userInfo.ID);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SendMessageAsync(Authentication authentication, string message)
        {
            try
            {
                this.ValidateExpired();
                var userInfo = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SendMessageAsync), this, message);
                    return base.UserInfo;
                });
                var result = await this.Service.SendMessageAsync(authentication.Token, userInfo.ID, message);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
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

        public string ID => this.Name;

        public string UserName => this.UserInfo.Name;

        public new string Path => base.Path;

        public new Authority Authority => base.Authority;

        public new UserInfo UserInfo => base.UserInfo;

        public new UserState UserState => base.UserState;

        public new BanInfo BanInfo => base.BanInfo;

        public bool IsBanned => this.BanInfo.Path != string.Empty;

        public Authentication Authentication { get; private set; }

        public IUserContextService Service => this.Context.Service;

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

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
            this.Authentication = new Authentication(new UserAuthenticationProvider(this));
        }

        protected override void OnDetached()
        {
            base.OnDetached();
            this.Authentication.InvokeExpiredEvent();
            this.Authentication = null;
        }

        #region IUser

        Task IUser.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task IUser.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        Task IUser.SetUserNameAsync(Authentication authentication, SecureString password, string userName)
        {
            return this.SetUserNameAsync(authentication, password, userName);
        }

        Task IUser.SetPasswordAsync(Authentication authentication, SecureString password, SecureString newPassword)
        {
            return this.SetPasswordAsync(authentication, password, newPassword);
        }

        Task IUser.ResetPasswordAsync(Authentication authentication)
        {
            return this.ResetPasswordAsync(authentication);
        }


        Task IUser.KickAsync(Authentication authentication, string comment)
        {
            return this.KickAsync(authentication, comment);
        }

        Task IUser.BanAsync(Authentication authentication, string comment)
        {
            return this.BanAsync(authentication, comment);
        }

        Task IUser.UnbanAsync(Authentication authentication)
        {
            return this.UnbanAsync(authentication);
        }

        Task IUser.SendMessageAsync(Authentication authentication, string message)
        {
            return this.SendMessageAsync(authentication, message);
        }

        string IUser.ID => this.ID;

        IUserCategory IUser.Category => this.Category;

        #endregion

        #region IUserItem

        Task IUserItem.MoveAsync(Authentication authentication, string parentPath)
        {
            return this.MoveAsync(authentication, parentPath);
        }

        Task IUserItem.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        string IUserItem.Name => this.ID;

        IUserItem IUserItem.Parent => this.Category;

        IEnumerable<IUserItem> IUserItem.Childs => Enumerable.Empty<IUserItem>();

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

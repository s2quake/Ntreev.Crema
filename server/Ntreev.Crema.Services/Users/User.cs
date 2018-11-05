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
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
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
                    this.ValidateMove(authentication, categoryPath);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldCategoryPaths = items.Select(item => item.Category.Path).ToArray();
                    var userInfo = this.UserInfo;
                    var targetName = new ItemName(categoryPath, base.Name);
                    return (items, oldPaths, oldCategoryPaths, userInfo, targetName);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeUserMoveAsync(authentication, tuple.userInfo, categoryPath, userContextSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.Move(authentication, categoryPath);
                    this.Container.InvokeUsersMovedEvent(authentication, tuple.items, tuple.oldPaths, tuple.oldCategoryPaths, taskID);
                });
                await this.Repository.UnlockAsync(userContextSet.Paths);
                return taskID;
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
                var container = this.Container;
                var repository = this.Repository;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    this.ValidateDelete(authentication);
                    this.CremaHost.Sign(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var userInfo = base.UserInfo;
                    return (items, oldPaths, userInfo);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForPathAsync(authentication, new ItemName(tuple.userInfo.Path));
                var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeUserDeleteAsync(authentication, tuple.userInfo, userContextSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.Delete(authentication);
                    container.InvokeUsersDeletedEvent(authentication, tuple.items, tuple.oldPaths, taskID);
                });
                await repository.UnlockAsync(userContextSet.Paths);
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Authentication> LoginAsync(SecureString password)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(Authentication.System, this, nameof(LoginAsync), this);
                    this.ValidateLogin(password);
                    var users = new User[] { this };
                    var authentication = new Authentication(new UserAuthenticationProvider(this), Guid.NewGuid());
                    var taskID = GuidUtility.FromName(this.ID);
                    if (this.Authentication != null)
                    {
                        var message = "다른 기기에서 동일한 아이디로 접속하였습니다.";
                        var closeInfo = new CloseInfo(CloseReason.Reconnected, message);
                        this.Authentication.InvokeExpiredEvent(this.ID, message);
                        this.Container.InvokeUsersLoggedOutEvent(this.Authentication, users, closeInfo, taskID);
                    }
                    this.Authentication = authentication;
                    this.IsOnline = true;
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeUsersStateChangedEvent(this.Authentication, users);
                    this.Container.InvokeUsersLoggedInEvent(this.Authentication, users, taskID);
                    return this.Authentication;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LogoutAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LogoutAsync), this);
                    this.ValidateLogout(authentication);
                    var users = new User[] { this };
                    var taskID = Guid.NewGuid();
                    this.Authentication.InvokeExpiredEvent(authentication.ID, string.Empty);
                    this.Authentication = null;
                    this.IsOnline = false;
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeUsersStateChangedEvent(authentication, users);
                    this.Container.InvokeUsersLoggedOutEvent(authentication, users, CloseInfo.Empty, taskID);
                    return taskID;
                });
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
                    this.ValidateBan(authentication, comment);
                    var items = EnumerableUtility.One(this).ToArray();
                    var comments = Enumerable.Repeat(comment, items.Length).ToArray();
                    var userInfo = base.UserInfo;
                    return (items, comments, userInfo);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForChangeAsync(authentication);
                var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeUserBanAsync(authentication, tuple.userInfo, userContextSet, comment);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var userInfo = userContextSet.GetUserInfo(this.Path);
                    base.Ban(authentication, (BanInfo)userInfo.BanInfo);
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeUsersBannedEvent(authentication, tuple.items, tuple.comments, taskID);
                    if (this.IsOnline == true)
                    {
                        this.Authentication.InvokeExpiredEvent(authentication.ID, comment);
                        this.Authentication = null;
                        this.IsOnline = false;
                        this.Container.InvokeUsersStateChangedEvent(authentication, tuple.items);
                        this.Container.InvokeUsersLoggedOutEvent(authentication, tuple.items, new CloseInfo(CloseReason.Banned, comment), taskID);
                    }
                });
                await this.Repository.UnlockAsync(userContextSet.Paths);
                return taskID;
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
                    this.ValidateUnban(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var userInfo = base.UserInfo;
                    return (items, userInfo);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForChangeAsync(authentication);
                var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeUserUnbanAsync(authentication, tuple.userInfo, userContextSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    base.Unban(authentication);
                    this.Container.InvokeUsersUnbannedEvent(authentication, tuple.items, taskID);
                });
                await this.Repository.UnlockAsync(userContextSet.Paths);
                return taskID;
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
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), this, comment);
                    this.ValidateKick(authentication, comment);
                    this.CremaHost.Sign(authentication);
                    var taskID = Guid.NewGuid();
                    var items = new User[] { this };
                    var comments = Enumerable.Repeat(comment, items.Length).ToArray();
                    this.IsOnline = false;
                    this.Authentication.InvokeExpiredEvent(authentication.ID, comment);
                    this.Authentication = null;
                    this.Container.InvokeUsersKickedEvent(authentication, items, comments, taskID);
                    this.Container.InvokeUsersStateChangedEvent(authentication, items);
                    this.Container.InvokeUsersLoggedOutEvent(authentication, items, new CloseInfo(CloseReason.Kicked, comment), Guid.Empty);
                    return taskID;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> ChangeUserInfoAsync(Authentication authentication, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ChangeUserInfoAsync), this, userName, authority);
                    this.ValidateUserInfoChange(authentication, password, newPassword, userName, authority);
                    this.CremaHost.Sign(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var userInfo = base.UserInfo;
                    return (items, userInfo);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForChangeAsync(authentication);
                var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeUserChangeAsync(authentication, tuple.userInfo, userContextSet, password, newPassword, userName, authority);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var userInfo = userContextSet.GetUserInfo(this.Path);
                    this.CremaHost.Sign(authentication);
                    this.Password = UserContext.StringToSecureString(userInfo.Password);
                    base.UpdateUserInfo((UserInfo)userInfo);
                    this.Container.InvokeUsersChangedEvent(authentication, tuple.items, taskID);
                });
                await this.Repository.UnlockAsync(userContextSet.Paths);
                return taskID;
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
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SendMessageAsync), this, message);
                    this.ValidateSendMessage(authentication, message);
                    var taskID = Guid.NewGuid();
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeSendMessageEvent(authentication, this, message, taskID);
                    return taskID;
                });

            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public bool VerifyPassword(SecureString password)
        {
            return UserContext.SecureStringToString(this.Password) == UserContext.SecureStringToString(password).Encrypt();
        }

        public async Task<UserSet> ReadDataForChangeAsync(Authentication authentication)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var paths = new string[]
                {
                    this.Path
                };
                var path = this.Path;
                return (paths, path);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.paths);
                var userInfo = this.Repository.Read(tuple.path);
                var dataSet = new UserSet()
                {
                    ItemPaths = tuple.paths,
                    Infos = new UserSerializationInfo[] { userInfo },
                    SignatureDateProvider = new SignatureDateProvider(authentication.ID),
                };
                return dataSet;
            });
        }

        public async Task<UserSet> ReadDataForPathAsync(Authentication authentication, ItemName targetName)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var items = new string[]
                {
                    targetName.CategoryPath,
                    targetName,
                    this.Path
                };
                var paths = items.Distinct().ToArray();
                var path = this.Path;
                return (paths, path);
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(tuple.paths);
                var userInfo = (UserSerializationInfo)this.Repository.Read(tuple.path);
                var dataSet = new UserSet()
                {
                    ItemPaths = tuple.paths,
                    Infos = new UserSerializationInfo[] { userInfo },
                    SignatureDateProvider = new SignatureDateProvider(authentication.ID),
                };
                return dataSet;
            });
        }

        public Authentication Authentication { get; set; }

        public string ID => base.Name;

        public string UserName => base.UserInfo.Name;

        public new string Path => base.Path;

        public new Authority Authority => base.Authority;

        public new UserInfo UserInfo => base.UserInfo;

        public new UserState UserState => base.UserState;

        public new BanInfo BanInfo => base.BanInfo;

        public bool IsBanned => this.BanInfo.Path != string.Empty;

        public SecureString Password { get; set; }

        public UserSerializationInfo SerializationInfo
        {
            get
            {
                var userInfo = (UserSerializationInfo)base.UserInfo;
                userInfo.Password = UserContext.SecureStringToString(this.Password);
                userInfo.BanInfo = (BanSerializationInfo)base.BanInfo;
                return userInfo;
            }
        }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public CremaHost CremaHost => this.Context.CremaHost;

        public UserRepositoryHost Repository => this.Context.Repository;

        public IObjectSerializer Serializer => this.Context.Serializer;

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

        protected override void OnDeleted(EventArgs e)
        {
            if (this.IsOnline == true)
            {
                throw new InvalidOperationException(Resources.Exception_UserIDIsConnecting);
            }

            base.OnDeleted(e);
        }

        private void ValidateUpdate(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
            {
                throw new PermissionDeniedException();
            }
        }

        private void ValidateUserInfoChange(Authentication authentication, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            if (this.ID != authentication.ID)
            {
                var isAdmin = authentication.Types.HasFlag(AuthenticationType.Administrator);

                if (base.UserInfo.ID == Authentication.AdminID)
                    throw new InvalidOperationException(Resources.Exception_AdminCanChangeAdminInfo);

                if (authority.HasValue == true && this.IsOnline == true)
                    throw new InvalidOperationException(Resources.Exception_OnlineUserAuthorityCannotChanged);

                if (newPassword != null)
                {
                    if (isAdmin == false)
                    {
                        if (this.VerifyPassword(password) == false)
                            throw new ArgumentException(Resources.Exception_IncorrectPassword, nameof(password));
                        if (this.VerifyPassword(newPassword) == true)
                            throw new ArgumentException(Resources.Exception_CannotChangeToOldPassword, nameof(newPassword));
                    }
                    else
                    {
                        if (this.VerifyPassword(newPassword) == true)
                            throw new ArgumentException(Resources.Exception_CannotChangeToOldPassword, nameof(newPassword));
                    }
                }
            }
            else
            {
                if (newPassword != null)
                {
                    if (newPassword == null || this.VerifyPassword(password) == false)
                        throw new ArgumentException(Resources.Exception_IncorrectPassword, nameof(password));
                    if (this.VerifyPassword(newPassword) == true)
                        throw new ArgumentException(Resources.Exception_CannotChangeToOldPassword, nameof(newPassword));
                }
                if (authority.HasValue == true)
                    throw new ArgumentException(Resources.Exception_CannotChangeYourAuthority);
            }
        }

        private void ValidateSendMessage(Authentication authentication, string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (message == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringCannotSend, nameof(message));
            if (this.IsOnline == false)
                throw new InvalidOperationException(Resources.Exception_CannotSendMessageToOfflineUser);
        }

        private void ValidateMove(Authentication authentication, string categoryPath)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (this.Authentication != authentication)
            {
                if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                {
                    throw new PermissionDeniedException();
                }
            }

            base.ValidateMove(authentication, categoryPath);
        }

        private void ValidateDelete(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (this.IsOnline == true)
                throw new InvalidOperationException(Resources.Exception_LoggedInUserCannotDelete);

            if (this.ID == authentication.ID)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteYourself);

            if (base.UserInfo.ID == Authentication.AdminID)
                throw new InvalidOperationException(Resources.Exception_AdminCannotDeleted);

            base.ValidateDelete();
        }

        private void ValidateLogin(SecureString password)
        {
            if (this.IsBanned == true)
                throw new InvalidOperationException(Resources.Exception_BannedUserCannotLogin);
        }

        private void ValidateLogout(Authentication authentication)
        {
            if (this.IsOnline == false)
                throw new InvalidOperationException(Resources.Exception_UserIsNotLoggedIn);
            if (authentication.ID != this.ID)
                throw new PermissionDeniedException();
        }

        private void ValidateBan(Authentication authentication, string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));
            if (comment == string.Empty)
                throw new ArgumentNullException(nameof(comment), Resources.Exception_EmptyStringIsNotAllowed);
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsBanned == true)
                throw new InvalidOperationException(Resources.Exception_UserIsAlreadyBanned);
            if (this.Authority == Authority.Admin)
                throw new PermissionDeniedException(Resources.Exception_AdminCannotBanned);
            if (authentication.ID == this.ID)
                throw new PermissionDeniedException(Resources.Exception_CannotBanYourself);
        }

        private void ValidateUnban(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsBanned == false)
                throw new InvalidOperationException(Resources.Exception_UserIsNotBanned);
        }

        private void ValidateKick(Authentication authentication, string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));
            if (comment == string.Empty)
                throw new ArgumentNullException(nameof(comment), Resources.Exception_EmptyStringIsNotAllowed);
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.IsOnline == false)
                throw new InvalidOperationException(Resources.Exception_OfflineUserCannotKicked);
            if (authentication.ID == this.ID)
                throw new PermissionDeniedException(Resources.Exception_CannotKickYourself);
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

        Task IUser.ChangeUserInfoAsync(Authentication authentication, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            return this.ChangeUserInfoAsync(authentication, password, newPassword, userName, authority);
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

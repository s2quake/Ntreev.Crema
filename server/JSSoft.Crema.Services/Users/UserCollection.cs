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
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users.Arguments;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Users
{
    class UserCollection : ItemContainer<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUserCollection
    {
        private ItemsCreatedEventHandler<IUser> usersCreated;
        private ItemsMovedEventHandler<IUser> usersMoved;
        private ItemsDeletedEventHandler<IUser> usersDeleted;
        private ItemsEventHandler<IUser> usersStateChanged;
        private ItemsEventHandler<IUser> usersChanged;
        private ItemsEventHandler<IUser> usersLoggedIn;
        private ItemsEventHandler<IUser> usersLoggedOut;
        private ItemsEventHandler<IUser> usersKicked;
        private ItemsEventHandler<IUser> usersBanChanged;
        private EventHandler<MessageEventArgs> messageReceived;

        public UserCollection()
        {

        }

        public User AddNew(string name, string categoryPath)
        {
            return this.BaseAddNew(name, categoryPath, null);
        }

        // public UserSet ReadDataForPath(Authentication authentication, string userPath, string[] lockPaths)
        // {
        //     var userInfo = (UserSerializationInfo)this.Repository.Read(userPath);
        //     var dataSet = new UserSet()
        //     {
        //         ItemPaths = lockPaths,
        //         Infos = new UserSerializationInfo[] { userInfo },
        //         SignatureDateProvider = new SignatureDateProvider(authentication.ID),
        //     };
        //     return dataSet;
        // }

        public async Task<User> AddNewAsync(Authentication authentication, string userID, string categoryPath, SecureString password, string userName, Authority authority)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (userID is null)
                    throw new ArgumentNullException(nameof(userID));
                if (categoryPath is null)
                    throw new ArgumentNullException(nameof(categoryPath));
                if (password is null)
                    throw new ArgumentNullException(nameof(password));
                if (userName is null)
                    throw new ArgumentNullException(nameof(userName));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewAsync), this, userID, categoryPath, userName, authority);
                    this.ValidateUserCreate(authentication, userID, categoryPath, password, userName, authority);
                    return new UserCreateArguments(userID, categoryPath, password, userName, authority);
                });
                var userInfo = await this.InvokeUserCreateAsync(authentication, args);
                var newUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    var user = this.BaseAddNew(userID, categoryPath, null);
                    var items = new[] { user };
                    user.Initialize((UserInfo)userInfo, (BanInfo)userInfo.BanInfo);
                    user.Password = UserContext.StringToSecureString(userInfo.Password);
                    user.Guid = Guid.NewGuid();
                    this.CremaHost.Sign(authentication);
                    this.InvokeUsersCreatedEvent(authentication, items);
                    this.Context.InvokeTaskCompletedEvent(authentication, user.Guid);
                    return user;
                });
                return newUser;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<UserSerializationInfo> InvokeUserCreateAsync(Authentication authentication, UserCreateArguments args)
        {
            var userPath = args.UserPath;
            var userPaths = new[] { args.UserPath };
            var userNames = new[] { args.UserName };
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var message = EventMessageBuilder.CreateUser(authentication, userPaths, userNames);
            var repository = this.Repository;
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserMoveAsync), lockPaths);
                try
                {
                    var userSet = args.Create(authentication);
                    var userContextSet = new UserContextSet(context, userSet, true);
                    repository.CreateUser(userContextSet, userPaths);
                    repository.Commit(authentication, message);
                    return userContextSet.GetUserInfo(userPath);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeUserMoveAsync(Authentication authentication, UserMoveArguments args)
        {
            var userInfo = args.UserInfo;
            var userID = userInfo.ID;
            var userName = userInfo.Name;
            var categoryPath = userInfo.CategoryPath;
            var newCategoryPath = args.NewCategoryPath;
            var userPath = args.UserPath;
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.MoveUser(authentication, userID, userName, categoryPath, newCategoryPath);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserMoveAsync), lockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.MoveUser(userContextSet, userInfo.Path, newCategoryPath);
                    repository.Commit(authentication, message);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeUserDeleteAsync(Authentication authentication, UserDeleteArguments args)
        {
            var userInfo = args.UserInfo;
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.DeleteUser(authentication, userInfo.ID);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserDeleteAsync), lockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.DeleteUser(userContextSet, userInfo.Path);
                    repository.Commit(authentication, message);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        // public UserSet ReadDataForChange(Authentication authentication, string userPath, string[] lockPaths)
        // {
        //     var userInfo = this.Repository.Read(userPath);
        //     var dataSet = new UserSet()
        //     {
        //         ItemPaths = lockPaths,
        //         Infos = new[] { userInfo },
        //         SignatureDateProvider = new SignatureDateProvider(authentication.ID),
        //     };
        //     return dataSet;
        // }

        public Task<UserInfo> InvokeUserNameSetAsync(Authentication authentication, UserSetNameArguments args)
        {
            var userInfo = args.UserInfo;
            var userName = args.UserName;
            var userPath = args.UserPath;
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.SetUserName(authentication, userInfo.ID, userInfo.Name);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserNameSetAsync), lockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.ModifyUser(userContextSet, userPath, userName);
                    repository.Commit(authentication, message);
                    return (UserInfo)userContextSet.GetUserInfo(userPath);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task<UserSerializationInfo> InvokeUserPasswordSetAsync(Authentication authentication, UserSetPasswordArguments args)
        {
            var password = args.Password;
            var userInfo = args.UserInfo;
            var userPath = args.UserPath;
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.SetPassword(authentication, userInfo.ID, userInfo.Name);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserPasswordSetAsync), lockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.ModifyUser(userContextSet, userPath, password);
                    repository.Commit(authentication, message);
                    return userContextSet.GetUserInfo(userPath);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task<UserSerializationInfo> InvokeUserPasswordResetAsync(Authentication authentication, UserResetPasswordArguments args)
        {
            var password = args.Password;
            var userInfo = args.UserInfo;
            var userPath = args.UserPath;
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.ResetPassword(authentication, userInfo.ID, userInfo.Name);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserPasswordSetAsync), lockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.ModifyUser(userContextSet, userPath, password);
                    repository.Commit(authentication, message);
                    return userContextSet.GetUserInfo(userPath);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task<UserSerializationInfo> InvokeUserBanAsync(Authentication authentication, UserBanArguments args)
        {
            var userInfo = args.UserInfo;
            var comment = args.Comment;
            var userPath = args.UserPath;
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.BanUser(authentication, userInfo.ID, userInfo.Name, comment);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserBanAsync), lockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.BanUser(userContextSet, userPath, comment);
                    repository.Commit(authentication, message);
                    return userContextSet.GetUserInfo(userPath);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task<UserSerializationInfo> InvokeUserUnbanAsync(Authentication authentication, UserUnbanArguments args)
        {
            var userInfo = args.UserInfo;
            var userPath = args.UserPath;
            var lockPaths = args.LockPaths;
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.UnbanUser(authentication, userInfo.ID, userInfo.Name);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeUserUnbanAsync), lockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.UnbanUser(userContextSet, userPath);
                    repository.Commit(authentication, message);
                    return userContextSet.GetUserInfo(userPath);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public void InvokeUsersCreatedEvent(Authentication authentication, User[] users)
        {
            var args = users.Select(item => (object)item.UserInfo).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersCreatedEvent), users);
            var message = EventMessageBuilder.CreateUser(authentication, users);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersCreated(new ItemsCreatedEventArgs<IUser>(authentication, users, args));
            this.Context.InvokeItemsCreatedEvent(authentication, users, args);
        }

        public void InvokeUsersMovedEvent(Authentication authentication, User[] users, string[] oldPaths, string[] oldCategoryPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersMovedEvent), users, oldPaths, oldCategoryPaths);
            var message = EventMessageBuilder.MoveUser(authentication, users, oldCategoryPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersMoved(new ItemsMovedEventArgs<IUser>(authentication, users, oldPaths, oldCategoryPaths));
            this.Context.InvokeItemsMovedEvent(authentication, users, oldPaths, oldCategoryPaths);
        }

        public void InvokeUsersDeletedEvent(Authentication authentication, User[] users, string[] itemPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersDeletedEvent), itemPaths);
            var message = EventMessageBuilder.DeleteUser(authentication, users);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersDeleted(new ItemsDeletedEventArgs<IUser>(authentication, users, itemPaths));
            this.Context.InvokeItemsDeleteEvent(authentication, users, itemPaths);
        }

        public void InvokeUsersChangedEvent(Authentication authentication, User[] users)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersChangedEvent), users);
            var message = EventMessageBuilder.SetUserName(authentication, users);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersChanged(new ItemsEventArgs<IUser>(authentication, users));
            this.Context.InvokeItemsChangedEvent(authentication, users);
        }

        public void InvokeUsersStateChangedEvent(Authentication authentication, User[] users)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeUsersStateChangedEvent), users);
            this.OnUsersStateChanged(new ItemsEventArgs<IUser>(authentication, users));
        }

        public void InvokeUsersLoggedInEvent(Authentication authentication, User[] users)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersLoggedInEvent), users);
            var comment = EventMessageBuilder.LoginUser(authentication, users);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnUsersLoggedIn(new ItemsEventArgs<IUser>(authentication, users));
        }

        public void InvokeUsersLoggedOutEvent(Authentication authentication, User[] users, CloseInfo closeInfo)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersLoggedOutEvent), users, closeInfo.Reason, closeInfo.Message);
            var comment = EventMessageBuilder.LogoutUser(authentication, users);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnUsersLoggedOut(new ItemsEventArgs<IUser>(authentication, users, closeInfo));
        }

        public void InvokeUsersKickedEvent(Authentication authentication, User[] users, string[] comments)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersKickedEvent), users, comments);
            var comment = EventMessageBuilder.KickUser(authentication, users, comments);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnUsersKicked(new ItemsEventArgs<IUser>(authentication, users, comments));
        }

        public void InvokeUsersBannedEvent(Authentication authentication, User[] users, string[] comments)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersBannedEvent), users, comments);
            var message = EventMessageBuilder.BanUser(authentication, users, comments);
            var metaData = EventMetaDataBuilder.Build(users, BanChangeType.Ban, comments);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersBanChanged(new ItemsEventArgs<IUser>(authentication, users, metaData));
            this.Context.InvokeItemsChangedEvent(authentication, users);
        }

        public void InvokeUsersUnbannedEvent(Authentication authentication, User[] users)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersUnbannedEvent), users);
            var message = EventMessageBuilder.UnbanUser(authentication, users);
            var metaData = EventMetaDataBuilder.Build(users, BanChangeType.Unban);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersBanChanged(new ItemsEventArgs<IUser>(authentication, users, metaData));
            this.Context.InvokeItemsChangedEvent(authentication, users);
        }

        public void InvokeSendMessageEvent(Authentication authentication, User user, string message)
        {
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeSendMessageEvent), user, message);
            var comment = EventMessageBuilder.SendMessage(authentication, user, message);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnMessageReceived(new MessageEventArgs(authentication, new IUser[] { user }, message, MessageType.None));
        }

        public void InvokeNotifyMessageEvent(Authentication authentication, User[] users, string message)
        {
            var target = users.Any() == false ? "all users" : string.Join(",", users.Select(item => item.ID).ToArray());
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeNotifyMessageEvent), target, message);
            var comment = EventMessageBuilder.NotifyMessage(authentication, users, message);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnMessageReceived(new MessageEventArgs(authentication, users, message, MessageType.Notification));
        }

        // public UserSet CreateDataForCreate(Authentication authentication, string userID, string categoryPath, SecureString password, string userName, Authority authority, string[] lockPaths)
        // {
        //     var userInfo = new UserSerializationInfo()
        //     {
        //         ID = userID,
        //         Password = UserContext.SecureStringToString(password).Encrypt(),
        //         Name = userName,
        //         Authority = authority,
        //         CategoryPath = categoryPath,
        //     };
        //     var dataSet = new UserSet()
        //     {
        //         ItemPaths = lockPaths,
        //         Infos = new UserSerializationInfo[] { userInfo },
        //         SignatureDateProvider = new SignatureDateProvider(authentication.ID),
        //     };
        //     return dataSet;
        //     // });
        // }

        public UserRepositoryHost Repository => this.Context.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.Context.Dispatcher;

        public IObjectSerializer Serializer => this.Context.Serializer;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<IUser> UsersCreated
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersCreated += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersCreated -= value;
            }
        }

        public event ItemsMovedEventHandler<IUser> UsersMoved
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersMoved += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<IUser> UsersDeleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersDeleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersDeleted -= value;
            }
        }

        public event ItemsEventHandler<IUser> UsersStateChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersStateChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersStateChanged -= value;
            }
        }

        public event ItemsEventHandler<IUser> UsersChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersChanged -= value;
            }
        }

        public event ItemsEventHandler<IUser> UsersLoggedIn
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersLoggedIn += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersLoggedIn -= value;
            }
        }

        public event ItemsEventHandler<IUser> UsersLoggedOut
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersLoggedOut += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersLoggedOut -= value;
            }
        }

        public event ItemsEventHandler<IUser> UsersKicked
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersKicked += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersKicked -= value;
            }
        }

        public event ItemsEventHandler<IUser> UsersBanChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersBanChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersBanChanged -= value;
            }
        }

        public event EventHandler<MessageEventArgs> MessageReceived
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.messageReceived += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.messageReceived -= value;
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                base.CollectionChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                base.CollectionChanged -= value;
            }
        }

        protected virtual void OnUsersCreated(ItemsCreatedEventArgs<IUser> e)
        {
            this.usersCreated?.Invoke(this, e);
        }

        protected virtual void OnUsersMoved(ItemsMovedEventArgs<IUser> e)
        {
            this.usersMoved?.Invoke(this, e);
        }

        protected virtual void OnUsersDeleted(ItemsDeletedEventArgs<IUser> e)
        {
            this.usersDeleted?.Invoke(this, e);
        }

        protected virtual void OnUsersStateChanged(ItemsEventArgs<IUser> e)
        {
            this.usersStateChanged?.Invoke(this, e);
        }

        protected virtual void OnUsersChanged(ItemsEventArgs<IUser> e)
        {
            this.usersChanged?.Invoke(this, e);
        }

        protected virtual void OnUsersLoggedIn(ItemsEventArgs<IUser> e)
        {
            this.usersLoggedIn?.Invoke(this, e);
        }

        protected virtual void OnUsersLoggedOut(ItemsEventArgs<IUser> e)
        {
            this.usersLoggedOut?.Invoke(this, e);
        }

        protected virtual void OnUsersKicked(ItemsEventArgs<IUser> e)
        {
            this.usersKicked?.Invoke(this, e);
        }

        protected virtual void OnUsersBanChanged(ItemsEventArgs<IUser> e)
        {
            this.usersBanChanged?.Invoke(this, e);
        }

        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            this.messageReceived?.Invoke(this, e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher?.VerifyAccess();
            base.OnCollectionChanged(e);
        }

        private void ValidateUserCreate(Authentication authentication, string userID, string categoryPath, SecureString password, string userName, Authority authority)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (password == null)
                throw new ArgumentNullException(nameof(password), Resources.Exception_InvalidPassword);

            var category = this.GetCategory(categoryPath);
            if (category == null)
                throw new CategoryNotFoundException(categoryPath);

            if (userID == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringIsNotAllowed, nameof(userID));

            if (this.Contains(userID) == true)
                throw new ArgumentException(Resources.Exception_UserIDisAlreadyResitered, nameof(userID));

            if (VerifyName(userID) == false)
                throw new ArgumentException(Resources.Exception_InvalidUserID, nameof(userID));

            if (userName == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringIsNotAllowed, nameof(userName));

            if (authority == Authority.None)
                throw new ArgumentException(nameof(authority));
        }

        private static bool VerifyName(string name)
        {
            if (NameValidator.VerifyName(name) == false)
                return false;
            return IdentifierValidator.Verify(name);
        }

        #region IUserCollection

        bool IUserCollection.Contains(string userID)
        {
            if (userID is null)
                throw new ArgumentNullException(nameof(userID));

            this.Dispatcher.VerifyAccess();
            return base.Contains(userID);
        }

        IUser IUserCollection.this[string userID]
        {
            get
            {
                if (userID is null)
                    throw new ArgumentNullException(nameof(userID));

                this.Dispatcher.VerifyAccess();
                if (userID == string.Empty)
                    throw new ArgumentException(Resources.Exception_EmptyStringIsNotAllowed);
                if (this.Contains(userID) == false)
                    throw new UserNotFoundException(userID);
                return base[userID];
            }
        }

        #endregion

        #region IReadOnlyCollection<IUser>

        int IReadOnlyCollection<IUser>.Count
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return this.Count;
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator<IUser> IEnumerable<IUser>.GetEnumerator()
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in this)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in this)
            {
                yield return item;
            }
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.Context as IUserContext).GetService(serviceType);
        }

        #endregion
    }
}

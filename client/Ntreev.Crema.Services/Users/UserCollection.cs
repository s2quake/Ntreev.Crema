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
using Ntreev.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    class UserCollection : ItemContainer<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUserCollection
    {
        private ItemsCreatedEventHandler<IUser> usersCreated;
        private ItemsRenamedEventHandler<IUser> usersRenamed;
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
        public User BaseAddNew(string name, string categoryPath)
        {
            return base.BaseAddNew(name, categoryPath, null);
        }

        public async Task<User> AddNewAsync(Authentication authentication, string userID, string categoryPath, SecureString password, string userName, Authority authority)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewAsync), this, userID, categoryPath, userName, authority);
                });
                var result = await this.Service.NewUserAsync(userID, categoryPath, UserContext.Encrypt(userID, password), userName, authority);
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    var user = this.BaseAddNew(userID, categoryPath);
                    user.Initialize(result.Value, BanInfo.Empty);
                    this.InvokeUsersCreatedEvent(authentication, new User[] { user });
                    return user;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
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

        public void InvokeUsersRenamedEvent(Authentication authentication, User[] users, string[] oldNames, string[] oldPaths)
        {

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
            var message = EventMessageBuilder.ChangeUserInfo(authentication, users);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersChanged(new ItemsEventArgs<IUser>(authentication, users));
        }

        public void InvokeUsersStateChangedEvent(Authentication authentication, User[] users)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeUsersStateChangedEvent), users);
            this.OnUsersStateChanged(new ItemsEventArgs<IUser>(authentication, users));
        }

        public void InvokeUsersLoggedInEvent(Authentication authentication, User[] users)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeUsersLoggedInEvent), users);
            this.CremaHost.Info(EventMessageBuilder.LoginUser(authentication, users));
            this.OnUsersLoggedIn(new ItemsEventArgs<IUser>(authentication, users));
        }

        public void InvokeUsersLoggedOutEvent(Authentication authentication, User[] users, CloseInfo closeInfo)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersLoggedOutEvent), users, closeInfo.Reason, closeInfo.Message);
            var message = EventMessageBuilder.LogoutUser(authentication, users);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnUsersLoggedOut(new ItemsEventArgs<IUser>(authentication, users));
        }

        public void InvokeUsersKickedEvent(Authentication authentication, User[] users, string[] comments)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeUsersKickedEvent), users, comments);
            var message = EventMessageBuilder.KickUser(authentication, users, comments);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
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
            this.OnMessageReceived(new MessageEventArgs(authentication, new IUser[] { user, }, message, MessageType.None));
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

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.Context.Dispatcher;

        public IUserService Service => this.Context.Service;

        public new int Count
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return base.Count;
            }
        }

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

        public event ItemsRenamedEventHandler<IUser> UsersRenamed
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.usersRenamed += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.usersRenamed -= value;
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

        protected virtual void OnUsersRenamed(ItemsRenamedEventArgs<IUser> e)
        {
            this.usersRenamed?.Invoke(this, e);
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

        #region IUserCollection

        Task<bool> IUserCollection.ContainsAsync(string userID)
        {
            return this.Dispatcher.InvokeAsync(() => base.Contains(userID));
        }

        IUser IUserCollection.this[string userID] => this[userID];

        #endregion

        #region IEnumerable

        IEnumerator<IUser> IEnumerable<IUser>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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

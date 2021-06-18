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
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Users
{
    class UserContext : ItemContext<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUserContextEventCallback, IUserContext
    {
        private ItemsCreatedEventHandler<IUserItem> itemsCreated;
        private ItemsRenamedEventHandler<IUserItem> itemsRenamed;
        private ItemsMovedEventHandler<IUserItem> itemsMoved;
        private ItemsDeletedEventHandler<IUserItem> itemsDeleted;
        private ItemsEventHandler<IUserItem> itemsChanged;
        private TaskCompletedEventHandler taskCompleted;

        private readonly Dictionary<string, Authentication> customAuthentications = new();
        private readonly TaskResetEvent<Guid> taskEvent;
        private readonly IndexedDispatcher callbackEvent;
        private Guid subscribeToken;

        public UserContext(CremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = new CremaDispatcher(this);
            this.taskEvent = new TaskResetEvent<Guid>(this.Dispatcher);
            this.callbackEvent = new IndexedDispatcher(this);
        }

        public void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            this.callbackEvent.Dispose();
        }

        public async Task InitializeAsync(Guid subscribeToken)
        {
            var result = await this.Service.SubscribeAsync(subscribeToken);
            await this.Dispatcher.InvokeAsync(() =>
            {
                var metaData = result.Value;
                foreach (var item in metaData.Categories)
                {
                    if (item == this.Root.Path)
                        continue;
                    this.Categories.Prepare(item);
                }
                foreach (var item in metaData.Users)
                {
                    var itemName = new ItemName(item.Path);
                    var user = this.Users.BaseAddNew(itemName.Name, itemName.CategoryPath);
                    user.Initialize(item.UserInfo, item.BanInfo);
                    user.SetUserState(item.UserState);
                }
                this.subscribeToken = subscribeToken;
            });
            this.ReleaseHandle.Reset();
        }

        public async Task ReleaseAsync()
        {
            if (this.Service != null)
                await this.Service.UnsubscribeAsync(this.subscribeToken);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.subscribeToken = Guid.Empty;
                this.Clear();
            });
            this.ReleaseHandle.Set();
        }

        public Task WaitAsync(Guid taskID)
        {
            return this.taskEvent.WaitAsync(taskID);
        }

        public static byte[] Encrypt(string userID, SecureString value)
        {
            return StringToSecureString(SecureStringToString());

            string SecureStringToString()
            {
                var valuePtr = IntPtr.Zero;
                try
                {
                    valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                    return Marshal.PtrToStringUni(valuePtr);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                }
            }

            byte[] StringToSecureString(string text)
            {
                return Encoding.UTF8.GetBytes(StringUtility.Encrypt(text, userID));
            }
        }

        public async Task LoginAsync(string userID, Guid authenticationToken)
        {
            var user = await this.Dispatcher.InvokeAsync(() => this.Users[userID]);
            await user.LoginAsync(authenticationToken);
        }

        public Authentication Authenticate(SignatureDate signatureDate)
        {
            return this.Dispatcher.Invoke(() => this.AuthenticateInternal(signatureDate));
        }

        private Authentication AuthenticateInternal(SignatureDate signatureDate)
        {
            if (signatureDate.ID == Authentication.SystemID)
            {
                Authentication.System.SignatureDate = signatureDate;
                return Authentication.System;
            }

            var user = this.Users[signatureDate.ID];
            if (user != null)
            {
                user.Authentication.SignatureDate = signatureDate;
                return user.Authentication;
            }

            if (this.customAuthentications.ContainsKey(signatureDate.ID) == false)
            {
                this.customAuthentications.Add(signatureDate.ID, new Authentication(new AuthenticationSignatureDateProvider(signatureDate)));
            }

            var authentication = this.customAuthentications[signatureDate.ID];
            authentication.SignatureDate = signatureDate;
            return authentication;
        }

        public async Task<Authentication> AuthenticateAsync(Guid authenticationToken)
        {
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var query = from User item in this.Users
                            let authentication = item.Authentication
                            where authentication != null && authentication.Token == authenticationToken
                            select item;

                if (query.Any() == true)
                    return query.First().Authentication;

                if (authenticationToken == Authentication.System.Token)
                    return Authentication.System;

                return null;
            });
        }

        public async Task DisauthenticateAsync(Authentication authentication)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (authentication == null)
                    throw new ArgumentNullException(nameof(authentication));
                // if (authentication.ID != this.CurrentUser.ID)
                //     throw new InvalidOperationException();
                // this.CurrentUser.Authentication.InvokeExpiredEvent();
            });
        }

        public Task<Authentication> AuthenticateAsync(SignatureDate signatureDate)
        {
            return this.Dispatcher.InvokeAsync(() => this.Authenticate(signatureDate));
        }

        public Authentication Authenticate(AuthenticationInfo authenticationInfo)
        {
            var user = this.Users[authenticationInfo.ID];
            if (user != null)
                return user.Authentication;
            return null;
        }

        public void InvokeItemsCreatedEvent(Authentication authentication, IUserItem[] items, object[] args)
        {
            this.OnItemsCreated(new ItemsCreatedEventArgs<IUserItem>(authentication, items, args));
        }

        public void InvokeItemsRenamedEvent(Authentication authentication, IUserItem[] items, string[] oldNames, string[] oldPaths)
        {
            this.OnItemsRenamed(new ItemsRenamedEventArgs<IUserItem>(authentication, items, oldNames, oldPaths));
        }

        public void InvokeItemsMovedEvent(Authentication authentication, IUserItem[] items, string[] oldPaths, string[] oldParentPaths)
        {
            this.OnItemsMoved(new ItemsMovedEventArgs<IUserItem>(authentication, items, oldPaths, oldParentPaths));
        }

        public void InvokeItemsDeleteEvent(Authentication authentication, IUserItem[] items, string[] itemPaths)
        {
            this.OnItemsDeleted(new ItemsDeletedEventArgs<IUserItem>(authentication, items, itemPaths));
        }

        public void InvokeItemsChangedEvent(Authentication authentication, IUserItem[] items)
        {
            this.OnItemsChanged(new ItemsEventArgs<IUserItem>(authentication, items));
        }

        public void InvokeTaskCompletedEvent(Authentication authentication, Guid taskID)
        {
            this.OnTaskCompleted(new TaskCompletedEventArgs(authentication, taskID));
        }

        public UserContextMetaData GetMetaData()
        {
            this.Dispatcher.VerifyAccess();

            var metaData = new UserContextMetaData();
            {
                var query = from UserCategory item in this.Categories
                            orderby item.Path
                            select item.Path;

                metaData.Categories = query.ToArray();
            }

            {
                var query = from User item in this.Users
                            orderby item.Category.Path
                            select new UserMetaData()
                            {
                                Path = item.Path,
                                UserInfo = item.UserInfo,
                                UserState = item.UserState,
                                BanInfo = item.BanInfo,
                            };
                metaData.Users = query.ToArray();
            }

            return metaData;
        }

        public async Task<Guid> NotifyMessageAsync(Authentication authentication, string[] userIDs, string message)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NotifyMessageAsync), this, userIDs, message);
                });
                var result = await this.Service.NotifyMessageAsync(authentication.Token, userIDs, message);
                await this.WaitAsync(result.TaskID);
                return result.TaskID;

            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public IUserContextService Service { get; set; }

        public UserCollection Users => this.Items;

        public CremaHost CremaHost { get; }

        public CremaDispatcher Dispatcher { get; set; }

        public ManualResetEvent ReleaseHandle { get; } = new ManualResetEvent(false);

        public event ItemsCreatedEventHandler<IUserItem> ItemsCreated
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsCreated += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<IUserItem> ItemsRenamed
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsRenamed += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<IUserItem> ItemsMoved
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsMoved += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<IUserItem> ItemsDeleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsDeleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsDeleted -= value;
            }
        }

        public event ItemsEventHandler<IUserItem> ItemsChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsChanged -= value;
            }
        }

        public event TaskCompletedEventHandler TaskCompleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted -= value;
            }
        }

        protected virtual void OnItemsCreated(ItemsCreatedEventArgs<IUserItem> e)
        {
            this.itemsCreated?.Invoke(this, e);
        }

        protected virtual void OnItemsRenamed(ItemsRenamedEventArgs<IUserItem> e)
        {
            this.itemsRenamed?.Invoke(this, e);
        }

        protected virtual void OnItemsMoved(ItemsMovedEventArgs<IUserItem> e)
        {
            this.itemsMoved?.Invoke(this, e);
        }

        protected virtual void OnItemsDeleted(ItemsDeletedEventArgs<IUserItem> e)
        {
            this.itemsDeleted?.Invoke(this, e);
        }

        protected virtual void OnItemsChanged(ItemsEventArgs<IUserItem> e)
        {
            this.itemsChanged?.Invoke(this, e);
        }

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs e)
        {
            this.taskCompleted?.Invoke(this, e);
        }

        #region IUserContextEventCallback

        async void IUserContextEventCallback.OnUsersChanged(CallbackInfo callbackInfo, UserInfo[] userInfos)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        var users = new User[userInfos.Length];
                        for (var i = 0; i < userInfos.Length; i++)
                        {
                            var userInfo = userInfos[i];
                            var user = this.Users[userInfo.ID];
                            user.SetUserInfo(userInfo);
                            users[i] = user;
                        }
                        this.Users.InvokeUsersChangedEvent(authentication, users);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUsersStateChanged(CallbackInfo callbackInfo, string[] userIDs, UserState[] states)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        var users = new User[userIDs.Length];
                        for (var i = 0; i < userIDs.Length; i++)
                        {
                            var user = this.Users[userIDs[i]];
                            user.SetUserState(states[i]);
                            users[i] = user;
                        }
                        this.Users.InvokeUsersStateChangedEvent(authentication, users);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUserItemsCreated(CallbackInfo callbackInfo, string[] itemPaths, UserInfo?[] args)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        var userItems = new IUserItem[itemPaths.Length];
                        var categories = new List<UserCategory>(itemPaths.Length);
                        var users = new List<User>(itemPaths.Length);
                        for (var i = 0; i < itemPaths.Length; i++)
                        {
                            var itemPath = itemPaths[i];
                            if (NameValidator.VerifyCategoryPath(itemPath) == true)
                            {
                                var categoryName = new CategoryName(itemPath);
                                var category = this.Categories.Prepare(itemPath);
                                categories.Add(category);
                                userItems[i] = category;
                            }
                            else
                            {
                                var userInfo = (UserInfo)args[i];
                                var user = this.Users.BaseAddNew(userInfo.ID, userInfo.CategoryPath);
                                user.Initialize(userInfo, BanInfo.Empty);
                                users.Add(user);
                                userItems[i] = user;
                            }
                        }
                        if (categories.Any() == true)
                        {
                            this.Categories.InvokeCategoriesCreatedEvent(authentication, categories.ToArray());
                        }
                        if (users.Any() == true)
                        {
                            this.Users.InvokeUsersCreatedEvent(authentication, users.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUserItemsRenamed(CallbackInfo callbackInfo, string[] itemPaths, string[] newNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        {
                            var items = new List<UserCategory>(itemPaths.Length);
                            var oldNames = new List<string>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is UserCategory == false)
                                    continue;

                                var category = userItem as UserCategory;
                                items.Add(category);
                                oldNames.Add(category.Name);
                                oldPaths.Add(category.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is UserCategory == false)
                                    continue;

                                var category = userItem as UserCategory;
                                category.InternalSetName(newNames[i]);
                            }

                            this.Categories.InvokeCategoriesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                        }

                        {
                            var items = new List<User>(itemPaths.Length);
                            var oldNames = new List<string>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is User == false)
                                    continue;

                                var user = userItem as User;
                                items.Add(user);
                                oldNames.Add(user.Name);
                                oldPaths.Add(user.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is User == false)
                                    continue;

                                var user = userItem as User;
                                user.Name = newNames[i];
                            }

                            this.Users.InvokeUsersRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUserItemsMoved(CallbackInfo callbackInfo, string[] itemPaths, string[] parentPaths)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        {
                            var items = new List<UserCategory>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);
                            var oldParentPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is UserCategory == false)
                                    continue;

                                var category = userItem as UserCategory;
                                items.Add(category);
                                oldPaths.Add(category.Path);
                                oldParentPaths.Add(category.Parent.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is UserCategory == false)
                                    continue;

                                var category = userItem as UserCategory;
                                category.Parent = this.Categories[parentPaths[i]];
                            }

                            this.Categories.InvokeCategoriesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                        }

                        {
                            var items = new List<User>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);
                            var oldParentPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is User == false)
                                    continue;

                                var user = userItem as User;
                                items.Add(user);
                                oldPaths.Add(user.Path);
                                oldParentPaths.Add(user.Category.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is User == false)
                                    continue;

                                var user = userItem as User;
                                user.Category = this.Categories[parentPaths[i]];
                            }

                            this.Users.InvokeUsersMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUserItemsDeleted(CallbackInfo callbackInfo, string[] itemPaths)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        {
                            var items = new List<UserCategory>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is UserCategory == false)
                                    continue;

                                var category = userItem as UserCategory;
                                items.Add(category);
                                oldPaths.Add(category.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is UserCategory == false)
                                    continue;

                                var category = userItem as UserCategory;
                                category.Dispose();
                            }

                            this.Categories.InvokeCategoriesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                        }

                        {
                            var items = new List<User>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is User == false)
                                    continue;

                                var user = userItem as User;
                                items.Add(user);
                                oldPaths.Add(user.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var userItem = this[itemPaths[i]];
                                if (userItem is User == false)
                                    continue;

                                var user = userItem as User;
                                user.Dispose();
                            }
                            this.Users.InvokeUsersDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUsersLoggedIn(CallbackInfo callbackInfo, string[] userIDs)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        var users = new User[userIDs.Length];
                        for (var i = 0; i < userIDs.Length; i++)
                        {
                            var user = this.Users[userIDs[i]];
                            user.IsOnline = true;
                            users[i] = user;
                        }
                        this.Users.InvokeUsersLoggedInEvent(authentication, users);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUsersLoggedOut(CallbackInfo callbackInfo, string[] userIDs, CloseInfo closeInfo)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        var users = new User[userIDs.Length];
                        for (var i = 0; i < userIDs.Length; i++)
                        {
                            var user = this.Users[userIDs[i]];
                            user.IsOnline = false;
                            users[i] = user;
                        }
                        this.Users.InvokeUsersLoggedOutEvent(authentication, users, closeInfo);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUsersKicked(CallbackInfo callbackInfo, string[] userIDs, string[] comments)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        var users = new User[userIDs.Length];
                        for (var i = 0; i < userIDs.Length; i++)
                        {
                            var user = this.Users[userIDs[i]];
                            user.IsOnline = false;
                            users[i] = user;
                        }
                        this.Users.InvokeUsersKickedEvent(authentication, users, comments);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnUsersBanChanged(CallbackInfo callbackInfo, BanInfo[] banInfos, BanChangeType changeType, string[] comments)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        var users = new User[banInfos.Length];
                        for (var i = 0; i < banInfos.Length; i++)
                        {
                            var banInfo = banInfos[i];
                            var user = this[banInfo.Path] as User;
                            user.SetBanInfo(changeType, banInfo);
                            users[i] = user;
                        }
                        switch (changeType)
                        {
                            case BanChangeType.Ban:
                                this.Users.InvokeUsersBannedEvent(authentication, users, comments);
                                break;
                            case BanChangeType.Unban:
                                this.Users.InvokeUsersUnbannedEvent(authentication, users);
                                break;
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnMessageReceived(CallbackInfo callbackInfo, string[] userIDs, string message, MessageType messageType)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.AuthenticateInternal(callbackInfo.SignatureDate);
                        if (messageType == MessageType.None)
                        {
                            foreach (var item in userIDs)
                            {
                                var user = this.Users[item];
                                this.Users.InvokeSendMessageEvent(authentication, user, message);
                            }
                        }
                        else if (messageType == MessageType.Notification)
                        {
                            var users = new User[userIDs.Length];
                            for (var i = 0; i < userIDs.Length; i++)
                            {
                                var user = this.Users[userIDs[i]];
                                users[i] = user;
                            }
                            this.Users.InvokeNotifyMessageEvent(authentication, users, message);
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IUserContextEventCallback.OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.taskEvent.Set(taskIDs);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        #endregion

        #region IUserContext

        Task IUserContext.NotifyMessageAsync(Authentication authentication, string[] userIDs, string message)
        {
            return this.NotifyMessageAsync(authentication, userIDs, message);
        }

        bool IUserContext.Contains(string itemPath)
        {
            return this.Contains(itemPath);
        }

        IUserItem IUserContext.this[string itemPath] => this[itemPath] as IUserItem;

        IUserCategory IUserContext.Root => this.Root;

        #endregion

        #region IEnumerable

        IEnumerator<IUserItem> IEnumerable<IUserItem>.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as IUserItem;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as IUserItem;
            }
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.CremaHost as ICremaHost).GetService(serviceType);
        }

        #endregion
    }
}

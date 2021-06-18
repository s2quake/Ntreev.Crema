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
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Users
{
    class UserContextService : CremaServiceItemBase<IUserContextEventCallback>, IUserContextService
    {
        private Peer peer;
        private long index = 0;

        public UserContextService(CremaService service, IUserContextEventCallback callback)
            : base(service, callback)
        {
            this.UserContext = this.CremaHost.GetService(typeof(IUserContext)) as IUserContext;
            this.UserCollection = this.CremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            this.LogService.Debug($"{nameof(UserContextService)} Constructor");
        }

        public async Task DisposeAsync()
        {
            if (this.peer != null)
            {
                await this.DetachEventHandlersAsync(this.peer.ID);
                this.peer = null;
            }
        }

        public async Task<ResultBase<UserContextMetaData>> SubscribeAsync(Guid token)
        {
            var value = await this.AttachEventHandlersAsync(token);
            this.peer = Peer.GetPeer(token);
            this.LogService.Debug($"[{token}] {nameof(UserContextService)} {nameof(SubscribeAsync)}");
            return new ResultBase<UserContextMetaData>()
            {
                Value = value,
                SignatureDate = new SignatureDateProvider($"{token}")
            };
        }

        public async Task<ResultBase> UnsubscribeAsync(Guid token)
        {
            if (this.peer == null)
                throw new InvalidOperationException();
            if (this.peer.ID != token)
                throw new ArgumentException("invalid token", nameof(token));
            await this.DetachEventHandlersAsync(token);
            this.peer = null;
            this.LogService.Debug($"[{token}] {nameof(UserContextService)} {nameof(UnsubscribeAsync)}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider($"{token}")
            };
        }

        public async Task<ResultBase<UserInfo>> NewUserAsync(Guid authenticationToken, string userID, string categoryPath, byte[] password, string userName, Authority authority)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<UserInfo>();
            var category = await this.GetCategoryAsync(categoryPath);
            var user = await category.AddNewUserAsync(authentication, userID, ToSecureString(userID, password), userName, authority);
            result.TaskID = GuidUtility.FromName(categoryPath + userID);
            result.Value = user.UserInfo;
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> NewUserCategoryAsync(Guid authenticationToken, string categoryPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var categoryName = new CategoryName(categoryPath);
            var category = await this.GetCategoryAsync(categoryName.ParentPath);
            await category.AddNewCategoryAsync(authentication, categoryName.Name);
            result.TaskID = GuidUtility.FromName(categoryPath);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> RenameUserItemAsync(Guid authenticationToken, string itemPath, string newName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var item = await this.GetUserItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)item.RenameAsync(authentication, newName);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> MoveUserItemAsync(Guid authenticationToken, string itemPath, string parentPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var item = await this.GetUserItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)item.MoveAsync(authentication, parentPath);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> DeleteUserItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var item = await this.GetUserItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)item.DeleteAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<UserInfo>> SetUserNameAsync(Guid authenticationToken, string userID, byte[] password, string userName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<UserInfo>();
            var p1 = password == null ? null : ToSecureString(userID, password);
            var user = await this.GetUserAsync(userID);
            result.TaskID = await (Task<Guid>)user.SetUserNameAsync(authentication, p1, userName);
            result.Value = await user.Dispatcher.InvokeAsync(() => user.UserInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<UserInfo>> SetPasswordAsync(Guid authenticationToken, string userID, byte[] password, byte[] newPassword)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<UserInfo>();
            var p1 = password == null ? null : ToSecureString(userID, password);
            var p2 = newPassword == null ? null : ToSecureString(userID, newPassword);
            var user = await this.GetUserAsync(userID);
            result.TaskID = await (Task<Guid>)user.SetPasswordAsync(authentication, p1, p2);
            result.Value = await user.Dispatcher.InvokeAsync(() => user.UserInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<UserInfo>> ResetPasswordAsync(Guid authenticationToken, string userID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<UserInfo>();
            var user = await this.GetUserAsync(userID);
            result.TaskID = await (Task<Guid>)user.ResetPasswordAsync(authentication);
            result.Value = await user.Dispatcher.InvokeAsync(() => user.UserInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> KickAsync(Guid authenticationToken, string userID, string comment)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var user = await this.GetUserAsync(userID);
            result.TaskID = await (Task<Guid>)user.KickAsync(authentication, comment);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<BanInfo>> BanAsync(Guid authenticationToken, string userID, string comment)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<BanInfo>();
            var user = await this.GetUserAsync(userID);
            result.TaskID = await (Task<Guid>)user.BanAsync(authentication, comment);
            result.Value = await user.Dispatcher.InvokeAsync(() => user.BanInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> UnbanAsync(Guid authenticationToken, string userID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var user = await this.GetUserAsync(userID);
            result.TaskID = await (Task<Guid>)user.UnbanAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> SendMessageAsync(Guid authenticationToken, string userID, string message)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var user = await this.GetUserAsync(userID);
            result.TaskID = await (Task<Guid>)user.SendMessageAsync(authentication, message);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> NotifyMessageAsync(Guid authenticationToken, string[] userIDs, string message)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            result.TaskID = await (Task<Guid>)this.UserContext.NotifyMessageAsync(authentication, userIDs, message);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public IUserContext UserContext { get; set; }

        public IUserCollection UserCollection { get; set; }

        private async Task<UserContextMetaData> AttachEventHandlersAsync(Guid token)
        {
            var metaData = await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserCollection.UsersStateChanged += UserCollection_UsersStateChanged;
                this.UserCollection.UsersChanged += UserCollection_UsersChanged;
                this.UserContext.ItemsCreated += UserContext_ItemsCreated;
                this.UserContext.ItemsRenamed += UserContext_ItemsRenamed;
                this.UserContext.ItemsMoved += UserContext_ItemsMoved;
                this.UserContext.ItemsDeleted += UserContext_ItemsDeleted;
                this.UserCollection.UsersLoggedIn += UserCollection_UsersLoggedIn;
                this.UserCollection.UsersLoggedOut += UserCollection_UsersLoggedOut;
                this.UserCollection.UsersKicked += UserCollection_UsersKicked;
                this.UserCollection.UsersBanChanged += UserCollection_UsersBanChanged;
                this.UserCollection.MessageReceived += UserContext_MessageReceived;
                this.UserContext.TaskCompleted += UserContext_TaskCompleted;
                return this.UserContext.GetMetaData();
            });
            this.LogService.Debug($"[{token}] {nameof(UserContextService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }

        private async Task DetachEventHandlersAsync(Guid token)
        {
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserCollection.UsersStateChanged -= UserCollection_UsersStateChanged;
                this.UserCollection.UsersChanged -= UserCollection_UsersChanged;
                this.UserContext.ItemsCreated -= UserContext_ItemsCreated;
                this.UserContext.ItemsRenamed -= UserContext_ItemsRenamed;
                this.UserContext.ItemsMoved -= UserContext_ItemsMoved;
                this.UserContext.ItemsDeleted -= UserContext_ItemsDeleted;
                this.UserCollection.UsersLoggedIn -= UserCollection_UsersLoggedIn;
                this.UserCollection.UsersLoggedOut -= UserCollection_UsersLoggedOut;
                this.UserCollection.UsersKicked -= UserCollection_UsersKicked;
                this.UserCollection.UsersBanChanged -= UserCollection_UsersBanChanged;
                this.UserCollection.MessageReceived -= UserContext_MessageReceived;
                this.UserContext.TaskCompleted -= UserContext_TaskCompleted;
            });
            this.LogService.Debug($"[{token}] {nameof(UserContextService)} {nameof(DetachEventHandlersAsync)}");
        }

        private void UserCollection_UsersStateChanged(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var states = e.Items.Select(item => item.UserState).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersStateChanged(callbackInfo, userIDs, states));
        }

        private void UserCollection_UsersChanged(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = e.Items.Select(item => item.UserInfo).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersChanged(callbackInfo, values));
        }

        private void UserContext_ItemsCreated(object sender, Services.ItemsCreatedEventArgs<IUserItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.Items.Select(item => item.Path).ToArray();
            var arguments = e.Arguments.Select(item => item is UserInfo userInfo ? (UserInfo?)userInfo : null).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUserItemsCreated(callbackInfo, itemPaths, arguments));
        }

        private void UserContext_ItemsRenamed(object sender, Services.ItemsRenamedEventArgs<IUserItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUserItemsRenamed(callbackInfo, oldPaths, itemNames));
        }

        private void UserContext_ItemsMoved(object sender, Services.ItemsMovedEventArgs<IUserItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var parentPaths = e.Items.Select(item => item.Parent.Path).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUserItemsMoved(callbackInfo, oldPaths, parentPaths));
        }

        private void UserContext_ItemsDeleted(object sender, Services.ItemsDeletedEventArgs<IUserItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.ItemPaths;
            this.InvokeEvent(() => this.Callback?.OnUserItemsDeleted(callbackInfo, itemPaths));
        }

        private void UserContext_MessageReceived(object sender, MessageEventArgs e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var message = e.Message;
            var messageType = e.MessageType;

            this.InvokeEvent(() => this.Callback?.OnMessageReceived(callbackInfo, userIDs, message, messageType));
        }

        private void UserCollection_UsersLoggedIn(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersLoggedIn(callbackInfo, userIDs));
        }

        private void UserCollection_UsersLoggedOut(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var closeInfo = (CloseInfo)e.MetaData;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersLoggedOut(callbackInfo, userIDs, closeInfo));
        }

        private void UserCollection_UsersKicked(object sender, ItemsEventArgs<IUser> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var comments = e.MetaData as string[];
            this.InvokeEvent(() => this.Callback?.OnUsersKicked(callbackInfo, userIDs, comments));
        }

        private void UserCollection_UsersBanChanged(object sender, ItemsEventArgs<IUser> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = new BanInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var lockInfo = item.BanInfo;
                if (item.BanInfo.Path != item.Path)
                {
                    lockInfo = BanInfo.Empty;
                    lockInfo.Path = item.Path;
                }
                values[i] = lockInfo;
            }
            var metaData = e.MetaData as object[];
            var changeType = (BanChangeType)metaData[0];
            var comments = metaData[1] as string[];
            this.InvokeEvent(() => this.Callback?.OnUsersBanChanged(callbackInfo, values, changeType, comments));
        }

        private void UserContext_TaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var taskIDs = e.TaskIDs;
            this.InvokeEvent(() => this.Callback?.OnTaskCompleted(callbackInfo, taskIDs));
        }

        private static SecureString ToSecureString(string userID, byte[] password)
        {
            var text = Encoding.UTF8.GetString(password);
            return StringToSecureString(StringUtility.Decrypt(text, userID));
        }

        private static SecureString StringToSecureString(string value)
        {
            var secureString = new SecureString();
            foreach (var item in value)
            {
                secureString.AppendChar(item);
            }
            return secureString;
        }

        private Task<IUserItem> GetUserItemAsync(string itemPath) => this.UserContext.GetUserItemAsync(itemPath);

        private Task<IUser> GetUserAsync(string userID) => this.UserContext.GetUserAsync(userID);

        private Task<IUserCategory> GetCategoryAsync(string categoryPath) => this.UserContext.GetUserCategoryAsync(categoryPath);
    }
}

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
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Users
{
    class UserContextService : CremaServiceItemBase<IUserContextEventCallback>, IUserContextService
    {
        private Authentication authentication;
        private long index = 0;

        public UserContextService(CremaService service, IUserContextEventCallback callback)
            : base(service, callback)
        {
            this.UserContext = this.CremaHost.GetService(typeof(IUserContext)) as IUserContext;
            this.LogService.Debug($"{nameof(UserContextService)} Constructor");
        }

        public async Task DisposeAsync()
        {
            if (this.authentication != null)
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
            }
        }

        public async Task<ResultBase<UserContextMetaData>> SubscribeAsync(Guid authenticationToken)
        {
            var result = new ResultBase<UserContextMetaData>();
            try
            {
                this.authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
                this.OwnerID = this.authentication.ID;
                result.Value = await this.AttachEventHandlersAsync();
                result.SignatureDate = this.authentication.SignatureDate;
                this.LogService.Debug($"[{this.OwnerID}] {nameof(UserContextService)} {nameof(SubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnsubscribeAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
                this.LogService.Debug($"[{this.OwnerID}] {nameof(UserContextService)} {nameof(UnsubscribeAsync)}");
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<UserInfo>> NewUserAsync(string userID, string categoryPath, byte[] password, string userName, Authority authority)
        {
            var result = new ResultBase<UserInfo>();
            try
            {
                var category = await this.GetCategoryAsync(categoryPath);
                var user = await category.AddNewUserAsync(this.authentication, userID, ToSecureString(userID, password), userName, authority);
                result.TaskID = GuidUtility.FromName(categoryPath + userID);
                result.Value = user.UserInfo;
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> NewUserCategoryAsync(string categoryPath)
        {
            var result = new ResultBase();
            try
            {
                var categoryName = new CategoryName(categoryPath);
                var category = await this.GetCategoryAsync(categoryName.ParentPath);
                await category.AddNewCategoryAsync(this.authentication, categoryName.Name);
                result.TaskID = GuidUtility.FromName(categoryPath);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> RenameUserItemAsync(string itemPath, string newName)
        {
            var result = new ResultBase();
            try
            {
                var item = await this.GetUserItemAsync(itemPath);
                result.TaskID = await (Task<Guid>)item.RenameAsync(this.authentication, newName);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> MoveUserItemAsync(string itemPath, string parentPath)
        {
            var result = new ResultBase();
            try
            {
                var item = await this.GetUserItemAsync(itemPath);
                result.TaskID = await (Task<Guid>)item.MoveAsync(this.authentication, parentPath);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> DeleteUserItemAsync(string itemPath)
        {
            var result = new ResultBase();
            try
            {
                var item = await this.GetUserItemAsync(itemPath);
                result.TaskID = await (Task<Guid>)item.DeleteAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<UserInfo>> ChangeUserInfoAsync(string userID, byte[] password, byte[] newPassword, string userName, Authority? authority)
        {
            var result = new ResultBase<UserInfo>();
            try
            {
                var p1 = password == null ? null : ToSecureString(userID, password);
                var p2 = newPassword == null ? null : ToSecureString(userID, newPassword);
                var user = await this.GetUserAsync(userID);
                result.TaskID = await (Task<Guid>)user.ChangeUserInfoAsync(this.authentication, p1, p2, userName, authority);
                result.Value = await user.Dispatcher.InvokeAsync(() => user.UserInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> KickAsync(string userID, string comment)
        {
            var result = new ResultBase();
            try
            {
                var user = await this.GetUserAsync(userID);
                result.TaskID = await (Task<Guid>)user.KickAsync(this.authentication, comment);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<BanInfo>> BanAsync(string userID, string comment)
        {
            var result = new ResultBase<BanInfo>();
            try
            {
                var user = await this.GetUserAsync(userID);
                result.TaskID = await (Task<Guid>)user.BanAsync(this.authentication, comment);
                result.Value = await user.Dispatcher.InvokeAsync(() => user.BanInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> UnbanAsync(string userID)
        {
            var result = new ResultBase();
            try
            {
                var user = await this.GetUserAsync(userID);
                result.TaskID = await (Task<Guid>)user.UnbanAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> SendMessageAsync(string userID, string message)
        {
            var result = new ResultBase();
            try
            {
                var user = await this.GetUserAsync(userID);
                result.TaskID = await (Task<Guid>)user.SendMessageAsync(this.authentication, message);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> NotifyMessageAsync(string[] userIDs, string message)
        {
            var result = new ResultBase();
            try
            {
                result.TaskID = await (Task<Guid>)this.UserContext.NotifyMessageAsync(this.authentication, userIDs, message);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<bool> IsAliveAsync()
        {
            if (this.authentication == null)
                return false;
            this.LogService.Debug($"[{this.authentication}] {nameof(UserContextService)}.{nameof(IsAliveAsync)} : {DateTime.Now}");
            await Task.Delay(1);
            return true;
        }

        public IUserContext UserContext { get; set; }

        protected override async Task OnCloseAsync(bool disconnect)
        {
            if (this.authentication != null)
            {
                await this.DetachEventHandlersAsync();
                if (disconnect == false)
                    await this.CremaHost.LogoutAsync(this.authentication);
                this.authentication = null;
            }
        }

        protected override void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = signatureDate };
            this.Callback?.OnServiceClosed(callbackInfo, closeInfo);
        }

        private async Task<UserContextMetaData> AttachEventHandlersAsync()
        {
            var metaData = await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersStateChanged += Users_UsersStateChanged;
                this.UserContext.Users.UsersChanged += Users_UsersChanged;
                this.UserContext.ItemsCreated += UserContext_ItemsCreated;
                this.UserContext.ItemsRenamed += UserContext_ItemsRenamed;
                this.UserContext.ItemsMoved += UserContext_ItemsMoved;
                this.UserContext.ItemsDeleted += UserContext_ItemsDeleted;
                this.UserContext.Users.UsersLoggedIn += Users_UsersLoggedIn;
                this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut;
                this.UserContext.Users.UsersKicked += Users_UsersKicked;
                this.UserContext.Users.UsersBanChanged += Users_UsersBanChanged;
                this.UserContext.Users.MessageReceived += UserContext_MessageReceived;
                this.UserContext.TaskCompleted += UserContext_TaskCompleted;
                return this.UserContext.GetMetaData(this.authentication);
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(UserContextService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }

        private async Task DetachEventHandlersAsync()
        {
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersStateChanged -= Users_UsersStateChanged;
                this.UserContext.Users.UsersChanged -= Users_UsersChanged;
                this.UserContext.ItemsCreated -= UserContext_ItemsCreated;
                this.UserContext.ItemsRenamed -= UserContext_ItemsRenamed;
                this.UserContext.ItemsMoved -= UserContext_ItemsMoved;
                this.UserContext.ItemsDeleted -= UserContext_ItemsDeleted;
                this.UserContext.Users.UsersLoggedIn -= Users_UsersLoggedIn;
                this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;
                this.UserContext.Users.UsersKicked -= Users_UsersKicked;
                this.UserContext.Users.UsersBanChanged -= Users_UsersBanChanged;
                this.UserContext.Users.MessageReceived -= UserContext_MessageReceived;
                this.UserContext.TaskCompleted -= UserContext_TaskCompleted;
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(UserContextService)} {nameof(DetachEventHandlersAsync)}");
        }

        private void Users_UsersStateChanged(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var states = e.Items.Select(item => item.UserState).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersStateChanged(callbackInfo, userIDs, states));
        }

        private void Users_UsersChanged(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = e.Items.Select(item => item.UserInfo).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersChanged(callbackInfo, values));
        }

        private void UserContext_ItemsCreated(object sender, Services.ItemsCreatedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.Items.Select(item => item.Path).ToArray();
            var arguments = e.Arguments.Select(item => item is UserInfo userInfo ? (UserInfo?)userInfo : null).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUserItemsCreated(callbackInfo, itemPaths, arguments));
        }

        private void UserContext_ItemsRenamed(object sender, Services.ItemsRenamedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUserItemsRenamed(callbackInfo, oldPaths, itemNames));
        }

        private void UserContext_ItemsMoved(object sender, Services.ItemsMovedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var parentPaths = e.Items.Select(item => item.Parent.Path).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUserItemsMoved(callbackInfo, oldPaths, parentPaths));
        }

        private void UserContext_ItemsDeleted(object sender, Services.ItemsDeletedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.ItemPaths;
            this.InvokeEvent(() => this.Callback?.OnUserItemsDeleted(callbackInfo, itemPaths));
        }

        private void UserContext_MessageReceived(object sender, MessageEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var message = e.Message;
            var messageType = e.MessageType;

            this.InvokeEvent(() => this.Callback?.OnMessageReceived(callbackInfo, userIDs, message, messageType));
        }

        private void Users_UsersLoggedIn(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersLoggedIn(callbackInfo, userIDs));
        }

        private void Users_UsersLoggedOut(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var actionUserID = e.UserID;
            var contains = e.Items.Any(item => item.ID == this.authentication.ID);
            var closeInfo = (CloseInfo)e.MetaData;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            this.InvokeEvent(() => this.Callback?.OnUsersLoggedOut(callbackInfo, userIDs, closeInfo));
        }

        private void Users_UsersKicked(object sender, ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var comments = e.MetaData as string[];
            this.InvokeEvent(() => this.Callback?.OnUsersKicked(callbackInfo, userIDs, comments));
        }

        private void Users_UsersBanChanged(object sender, ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
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

        private Task<IUserItem> GetUserItemAsync(string itemPath)
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var item = this.UserContext[itemPath];
                if (item == null)
                    throw new ItemNotFoundException(itemPath);
                return item;
            });
        }

        private Task<IUser> GetUserAsync(string userID)
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var user = this.UserContext.Users[userID];
                if (user == null)
                    throw new UserNotFoundException(userID);
                return user;
            });
        }

        private Task<IUserCategory> GetCategoryAsync(string categoryPath)
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var category = this.UserContext.Categories[categoryPath];
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                return category;
            });
        }
    }
}

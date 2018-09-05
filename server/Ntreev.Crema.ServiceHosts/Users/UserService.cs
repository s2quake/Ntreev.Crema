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
using Ntreev.Library.ObjectModel;
using Ntreev.Library;
using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Security;
using System.Runtime.InteropServices;
using Ntreev.Crema.ServiceHosts.Properties;

namespace Ntreev.Crema.ServiceHosts.Users
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class UserService : CremaServiceItemBase<IUserEventCallback>, IUserService, ICremaServiceItem
    {
        private readonly ICremaHost cremaHost;
        private readonly CremaService cremaService;
        private readonly ILogService logService;
        private readonly IUserContext userContext;

        private Authentication authentication;

        public UserService(ICremaHost cremaHost, CremaService cremaService)
            : base(cremaHost.GetService(typeof(ILogService)) as ILogService)
        {
            this.cremaHost = cremaHost;
            this.cremaService = cremaService;
            this.logService = cremaHost.GetService(typeof(ILogService)) as ILogService;
            this.userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            this.logService.Debug($"{nameof(UserService)} Constructor");
        }

        public ResultBase DefinitionType(SignatureDate p1)
        {
            return new ResultBase();
        }

        public async Task<ResultBase<UserContextMetaData>> SubscribeAsync(string userID, byte[] password, string version, string platformID, string culture)
        {
            var result = new ResultBase<UserContextMetaData>();
            try
            {
                var serverVersion = typeof(ICremaHost).Assembly.GetName().Version;
                var clientVersion = new Version(version);

                if (clientVersion < serverVersion)
                    throw new ArgumentException(Resources.Exception_LowerVersion, nameof(version));

                this.authentication = await this.userContext.LoginAsync(userID, ToSecureString(userID, password));
                this.authentication.AddRef(this, (a) =>
                {
                    this.userContext.LogoutAsync(a).Wait();
                });

                this.OwnerID = this.authentication.ID;
                this.AttachEventHandlers();
                this.logService.Debug($"[{this.OwnerID}] {nameof(UserService)} {nameof(SubscribeAsync)}");

                result.Value = await this.userContext.GetMetaDataAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                this.OwnerID = $"{userID} - failed";
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        private async void AuthenticationUtility_Disconnected(object sender, EventArgs e)
        {
            var authentication = sender as Authentication;
            if (this.authentication != null && this.authentication == authentication)
            {
                await this.userContext.LogoutAsync(this.authentication);
            }
        }

        public async Task<ResultBase> UnsubscribeAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.userContext.Dispatcher.InvokeAsync(this.DetachEventHandlers);
                this.authentication.RemoveRef(this);
                await this.userContext.LogoutAsync(this.authentication);
                this.authentication = null;
                this.logService.Debug($"[{this.OwnerID}] {nameof(UserService)} {nameof(UnsubscribeAsync)}");
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> ShutdownAsync(int milliseconds, ShutdownType shutdownType, string message)
        {
            var result = new ResultBase();
            try
            {
                await this.cremaHost.ShutdownAsync(this.authentication, milliseconds, shutdownType, message);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> CancelShutdownAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.cremaHost.CancelShutdownAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
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
                var categoryName = new Ntreev.Library.ObjectModel.CategoryName(categoryPath);
                var category = await this.GetCategoryAsync(categoryName.ParentPath);
                await category.AddNewCategoryAsync(this.authentication, categoryName.Name);
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
                await item.RenameAsync(this.authentication, newName);
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
                await item.MoveAsync(this.authentication, parentPath);
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
                await item.DeleteAsync(this.authentication);
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
                await user.ChangeUserInfoAsync(this.authentication, p1, p2, userName, authority);
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
                await user.KickAsync(this.authentication, comment);
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
                await user.BanAsync(this.authentication, comment);
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
                await user.UnbanAsync(this.authentication);
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
                await user.SendMessageAsync(this.authentication, message);
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
                await this.userContext.NotifyMessageAsync(this.authentication, userIDs, message);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public bool IsAlive()
        {
            if (this.authentication == null)
                return false;
            this.logService.Debug($"[{this.authentication}] {nameof(UserService)}.{nameof(IsAlive)} : {DateTime.Now}");
            this.authentication.Ping();
            return true;
        }

        protected override void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);
            this.userContext.Dispatcher.Invoke(() =>
            {
                if (this.authentication != null)
                {
                    this.DetachEventHandlers();
                    if (this.authentication.RemoveRef(this) == 0)
                    {
                        this.userContext.LogoutAsync(this.authentication).Wait();
                    }
                    this.authentication = null;
                }
            });
        }

        protected override void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            this.Callback.OnServiceClosed(signatureDate, closeInfo);
        }

        private void AttachEventHandlers()
        {
            this.userContext.Dispatcher.VerifyAccess();

            this.userContext.Users.UsersStateChanged += Users_UsersStateChanged;
            this.userContext.Users.UsersChanged += Users_UsersChanged;
            this.userContext.ItemsCreated += UserContext_ItemsCreated;
            this.userContext.ItemsRenamed += UserContext_ItemsRenamed;
            this.userContext.ItemsMoved += UserContext_ItemsMoved;
            this.userContext.ItemsDeleted += UserContext_ItemsDeleted;
            this.userContext.Users.UsersLoggedIn += Users_UsersLoggedIn;
            this.userContext.Users.UsersLoggedOut += Users_UsersLoggedOut;
            this.userContext.Users.UsersKicked += Users_UsersKicked;
            this.userContext.Users.UsersBanChanged += Users_UsersBanChanged;
            this.userContext.Users.MessageReceived += UserContext_MessageReceived;

            this.logService.Debug($"[{this.OwnerID}] {nameof(UserService)} {nameof(AttachEventHandlers)}");
        }

        private void DetachEventHandlers()
        {
            this.userContext.Dispatcher.VerifyAccess();

            this.userContext.Users.UsersStateChanged -= Users_UsersStateChanged;
            this.userContext.Users.UsersChanged -= Users_UsersChanged;
            this.userContext.ItemsCreated -= UserContext_ItemsCreated;
            this.userContext.ItemsRenamed -= UserContext_ItemsRenamed;
            this.userContext.ItemsMoved -= UserContext_ItemsMoved;
            this.userContext.ItemsDeleted -= UserContext_ItemsDeleted;
            this.userContext.Users.UsersLoggedIn -= Users_UsersLoggedIn;
            this.userContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;
            this.userContext.Users.UsersKicked -= Users_UsersKicked;
            this.userContext.Users.UsersBanChanged -= Users_UsersBanChanged;
            this.userContext.Users.MessageReceived -= UserContext_MessageReceived;

            this.logService.Debug($"[{this.OwnerID}] {nameof(UserService)} {nameof(DetachEventHandlers)}");
        }

        private void Users_UsersStateChanged(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var states = e.Items.Select(item => item.UserState).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUsersStateChanged(e.SignatureDate, userIDs, states));
        }

        private void Users_UsersChanged(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var values = e.Items.Select(item => item.UserInfo).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUsersChanged(signatureDate, values));
        }

        private void UserContext_ItemsCreated(object sender, Services.ItemsCreatedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemPaths = e.Items.Select(item => item.Path).ToArray();
            var arguments = e.Arguments.Select(item => item is UserInfo userInfo ? (UserInfo?)userInfo : null).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUserItemsCreated(signatureDate, itemPaths, arguments));
        }

        private void UserContext_ItemsRenamed(object sender, Services.ItemsRenamedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var oldPaths = e.OldPaths;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUserItemsRenamed(signatureDate, oldPaths, itemNames));
        }

        private void UserContext_ItemsMoved(object sender, Services.ItemsMovedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var oldPaths = e.OldPaths;
            var parentPaths = e.Items.Select(item => item.Parent.Path).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUserItemsMoved(signatureDate, oldPaths, parentPaths));
        }

        private void UserContext_ItemsDeleted(object sender, Services.ItemsDeletedEventArgs<IUserItem> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var itemPaths = e.ItemPaths;
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUserItemsDeleted(signatureDate, itemPaths));
        }

        private void UserContext_MessageReceived(object sender, MessageEventArgs e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var message = e.Message;
            var messageType = e.MessageType;

            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnMessageReceived(signatureDate, userIDs, message, messageType));
        }

        private void Users_UsersLoggedIn(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUsersLoggedIn(signatureDate, userIDs));
        }

        private void Users_UsersLoggedOut(object sender, Services.ItemsEventArgs<IUser> e)
        {
            var actionUserID = e.UserID;
            var contains = e.Items.Any(item => item.ID == this.authentication.ID);
            var closeInfo = (CloseInfo)e.MetaData;
            if (actionUserID != this.authentication.ID && contains == true)
            {
                var signatureDate = e.SignatureDate;
                this.DetachEventHandlers();
                this.InvokeEvent(null, null, () => this.Callback.OnServiceClosed(signatureDate, closeInfo));
            }
            else
            {
                var userID = this.authentication.ID;
                var exceptionUserID = e.UserID;
                var signatureDate = e.SignatureDate;
                var userIDs = e.Items.Select(item => item.ID).ToArray();
                this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUsersLoggedOut(signatureDate, userIDs));
            }
        }

        private void Users_UsersKicked(object sender, ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var comments = e.MetaData as string[];
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUsersKicked(signatureDate, userIDs, comments));
        }

        private void Users_UsersBanChanged(object sender, ItemsEventArgs<IUser> e)
        {
            var userID = this.authentication.ID;
            var exceptionUserID = e.UserID;
            var signatureDate = e.SignatureDate;
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
            this.InvokeEvent(userID, exceptionUserID, () => this.Callback.OnUsersBanChanged(signatureDate, values, changeType, comments));
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

        private ResultBase<T> Invoke<T>(Func<T> func)
        {
            var result = new ResultBase<T>();
            try
            {
                result.Value = this.userContext.Dispatcher.Invoke(func);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        private ResultBase Invoke(Action action)
        {
            var result = new ResultBase();
            try
            {
                this.userContext.Dispatcher.Invoke(action);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        private Task<IUserItem> GetUserItemAsync(string itemPath)
        {
            return this.userContext.Dispatcher.InvokeAsync(() =>
            {
                var item = this.userContext[itemPath];
                if (item == null)
                    throw new ItemNotFoundException(itemPath);
                return item;
            });
        }

        private Task<IUser> GetUserAsync(string userID)
        {
            return this.userContext.Dispatcher.InvokeAsync(() =>
            {
                var user = this.userContext.Users[userID];
                if (user == null)
                    throw new UserNotFoundException(userID);
                return user;
            });
        }

        private Task<IUserCategory> GetCategoryAsync(string categoryPath)
        {
            return this.userContext.Dispatcher.InvokeAsync(() =>
            {
                var category = this.userContext.Categories[categoryPath];
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                return category;
            });
        }

        #region ICremaServiceItem

        void ICremaServiceItem.Abort(bool disconnect)
        {
            this.userContext.Dispatcher.Invoke(() =>
            {
                this.DetachEventHandlers();
                this.authentication = null;
            });

            CremaService.Dispatcher.Invoke(() =>
            {
                if (disconnect == false)
                {
                    this.Callback?.OnServiceClosed(SignatureDate.Empty, CloseInfo.Empty);
                    try
                    {
                        this.Channel?.Close(TimeSpan.FromSeconds(10));
                    }
                    catch
                    {
                        this.Channel?.Abort();
                    }
                }
                else
                {
                    this.Channel?.Abort();
                }
            });
        }

        #endregion
    }
}

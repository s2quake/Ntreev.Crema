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

using Ntreev.Library;
using Ntreev.Crema.Services;
using Ntreev.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ntreev.Crema.ServiceModel;
using System.ComponentModel;
using Newtonsoft.Json;
using Ntreev.Library.IO;
using System.Diagnostics;
using Ntreev.Crema.Commands.Consoles.Properties;
using Ntreev.Crema.Commands.Consoles.Serializations;

namespace Ntreev.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceDescription("Resources", IsShared = true)]
    class UserCommand : ConsoleCommandMethodBase
    {
        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        public UserCommand(ICremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(IsOnline), nameof(IsBanned))]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        public async Task ListAsync()
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var metaData = await this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.GetMetaData(authentication));
            var query = from item in metaData.Users
                        let userID = item.UserInfo.ID
                        orderby userID
                        where StringUtility.GlobMany(userID, FilterProperties.FilterExpression)
                        where this.IsOnline == false || item.UserState == UserState.Online
                        where this.IsBanned == false || item.BanInfo.Path == item.Path
                        select new TerminalUserItem(userID, item.BanInfo, item.UserState);

            var metaItems = query.ToArray();
            this.CommandContext.WriteList(metaItems);
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(Message))]
        public async Task KickAsync([CommandCompletion(nameof(GetOnlineUserIDs))]string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.KickAsync(authentication, this.Message);
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(Message))]
        public async Task BanAsync([CommandCompletion(nameof(GetUnbannedUserIDs))]string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.BanAsync(authentication, this.Message);
        }

        [CommandMethod]
        public async Task UnbanAsync([CommandCompletion(nameof(GetBannedUserIDs))]string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.UnbanAsync(authentication);
        }

        [CommandMethod]
        public async Task PasswordAsync([CommandCompletion(nameof(GetUserIDs))]string userID)
        {
            var user = await this.GetUserAsync(userID);
            var password1 = this.CommandContext.ReadSecureString("Password1:");
            var password2 = this.CommandContext.ReadSecureString("Password2:");
            ConsoleCommandContextBase.Validate(password1, password2);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.ChangeUserInfoAsync(authentication, null, password1, null, null);
        }

        [CommandMethod]
        public async Task RenameAsync([CommandCompletion(nameof(GetUserIDs))]string userID, string newName = null)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            var newValue = newName ?? this.CommandContext.ReadString("NewName:");
            await user.ChangeUserInfoAsync(authentication, null, null, newValue, null);
        }

        [CommandMethod]
        public async Task MoveAsync([CommandCompletion(nameof(GetUserIDs))]string userID, string categoryPath)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.MoveAsync(authentication, categoryPath);
        }

        [CommandMethod("authority")]
        public async Task SetAuthorityAsync([CommandCompletion(nameof(GetUserIDs))]string userID, Authority authority)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.ChangeUserInfoAsync(authentication, null, null, null, authority);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task InfoAsync([CommandCompletion(nameof(GetUserIDs))]string userID)
        {
            var user = await this.GetUserAsync(userID);
            var userInfo = await user.Dispatcher.InvokeAsync(() => user.UserInfo);
            this.CommandContext.WriteObject(userInfo.ToDictionary(), FormatProperties.Format);
        }

        [ConsoleModeOnly]
        [CommandMethod]
        [CommandMethodProperty(nameof(CategoryPath))]
        public async Task CreateAsync()
        {
            var schema = JsonSchemaUtility.CreateSchema(typeof(JsonUserInfo));
            schema.SetEnums(nameof(JsonUserInfo.CategoryPath), this.GetCategoryPaths());

            var userInfo = JsonUserInfo.Default;
            userInfo.CategoryPath = this.CategoryPath;
            if (JsonEditorHost.TryEdit(ref userInfo, schema) == false)
                return;

            var category = await this.GetCategoryAsync(this.CategoryPath ?? this.CommandContext.Path);
            var userID = userInfo.UserID;
            var password = StringUtility.ToSecureString(userInfo.Password);
            var userName = userInfo.UserName;
            var authority = (Authority)Enum.Parse(typeof(Authority), userInfo.Authority);
            var authentication = this.CommandContext.GetAuthentication(this);
            await category.AddNewUserAsync(authentication, userID, password, userName, authority);
        }

        [CommandMethod]
        public async Task DeleteAsync([CommandCompletion(nameof(GetUserIDs))]string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            if (this.CommandContext.ConfirmToDelete() == true)
            {
                await user.DeleteAsync(authentication);
            }
        }

        [CommandMethod("message")]
        public async Task SendMessageAsync([CommandCompletion(nameof(GetUserIDs))]string userID, string message)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.SendMessageAsync(authentication, message);
        }

        [CommandProperty("online", 'o')]
        public bool IsOnline
        {
            get; set;
        }

        [CommandProperty("banned")]
        public bool IsBanned
        {
            get; set;
        }

        [CommandProperty('m', true, IsRequired = true, IsExplicit = true)]
        public string Message
        {
            get; set;
        }

        [CommandProperty]
        [CommandCompletion(nameof(GetCategoryPaths))]
        public string CategoryPath
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.Drive is UsersConsoleDrive;

        protected IUserContext UserContext
        {
            get { return this.cremaHost.GetService(typeof(IUserContext)) as IUserContext; }
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

        private string[] GetUserIDs()
        {
            return this.UserContext.Dispatcher.Invoke(() =>
            {
                var query = from item in this.UserContext.Users
                            orderby item.ID
                            select item.ID;
                return query.ToArray();
            });
        }

        private string[] GetCategoryPaths()
        {
            return this.UserContext.Dispatcher.Invoke(() =>
            {
                var query = from item in this.UserContext.Categories
                            orderby item.Path
                            select item.Path;
                return query.ToArray();
            });
        }

        private string[] GetOnlineUserIDs()
        {
            return this.UserContext.Dispatcher.Invoke(() =>
            {
                var query = from item in this.UserContext.Users
                            where item.UserState.HasFlag(UserState.Online)
                            select item.ID;
                return query.ToArray();
            });
        }

        private string[] GetUnbannedUserIDs()
        {
            return this.UserContext.Dispatcher.Invoke(() =>
            {
                var query = from item in this.UserContext.Users
                            where item.BanInfo.Path != item.Path
                            select item.ID;
                return query.ToArray();
            });
        }

        private string[] GetBannedUserIDs()
        {
            return this.UserContext.Dispatcher.Invoke(() =>
            {
                var query = from item in this.UserContext.Users
                            where item.BanInfo.Path == item.Path
                            select item.ID;
                return query.ToArray();
            });
        }

        #region classes

        class TerminalUserItem : TerminalTextItem
        {
            private string userID;
            private readonly BanInfo banInfo;
            private readonly UserState userState;

            public TerminalUserItem(string userID, BanInfo banInfo, UserState userState)
                : base(userID)
            {
                this.userID = userID;
                this.banInfo = banInfo;
                this.userState = userState;
            }

            protected override void OnDraw(TextWriter writer, string text)
            {
                if (this.banInfo.Path != string.Empty)
                {
                    using (TerminalColor.SetForeground(ConsoleColor.Red))
                    {
                        base.OnDraw(writer, text);
                    }
                }
                else if (this.userState != UserState.Online)
                {
                    //using (TerminalColor.SetForeground(ConsoleColor.Gray))
                    {
                        base.OnDraw(writer, text);
                    }
                }
                else
                {
                    using (TerminalColor.SetForeground(ConsoleColor.Blue))
                    {
                        base.OnDraw(writer, text);
                    }
                }

            }
        }

        #endregion
    }
}

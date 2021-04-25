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

using JSSoft.Crema.Commands.Consoles.Properties;
using JSSoft.Crema.Commands.Consoles.Serializations;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Commands;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
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
            var sb = new StringBuilder();
            var authentication = this.CommandContext.GetAuthentication(this);
            var metaData = await this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.GetMetaData(authentication));
            var query = from item in metaData.Users
                        let userID = item.UserInfo.ID
                        orderby userID
                        where StringUtility.GlobMany(userID, FilterProperties.FilterExpression)
                        where this.IsOnline == false || item.UserState == UserState.Online
                        where this.IsBanned == false || item.BanInfo.Path == item.Path
                        select FormatUserID(userID, item.BanInfo, item.UserState);

            sb.AppendLine(query);
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(Message))]
        public async Task KickAsync([CommandCompletion(nameof(GetOnlineUserIDsAsync))] string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.KickAsync(authentication, this.Message);
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(Message))]
        public async Task BanAsync([CommandCompletion(nameof(GetUnbannedUserIDsAsync))] string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.BanAsync(authentication, this.Message);
        }

        [CommandMethod]
        public async Task UnbanAsync([CommandCompletion(nameof(GetBannedUserIDsAsync))] string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.UnbanAsync(authentication);
        }

        [CommandMethod]
        public async Task PasswordAsync([CommandCompletion(nameof(GetUserIDsAsync))] string userID)
        {
            var user = await this.GetUserAsync(userID);
            var password1 = this.CommandContext.ReadSecureString("Password1:");
            var password2 = this.CommandContext.ReadSecureString("Password2:");
            ConsoleCommandContextBase.Validate(password1, password2);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.ChangeUserInfoAsync(authentication, null, password1, null, null);
        }

        [CommandMethod]
        public async Task RenameAsync([CommandCompletion(nameof(GetUserIDsAsync))] string userID, string newName = null)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            var newValue = newName ?? this.CommandContext.ReadString("NewName:");
            await user.ChangeUserInfoAsync(authentication, null, null, newValue, null);
        }

        [CommandMethod]
        public async Task MoveAsync([CommandCompletion(nameof(GetUserIDsAsync))] string userID, [CommandCompletion(nameof(GetCategoryPathsAsync))] string categoryPath)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.MoveAsync(authentication, categoryPath);
        }

        [CommandMethod("authority")]
        public async Task SetAuthorityAsync([CommandCompletion(nameof(GetUserIDsAsync))] string userID, Authority authority)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.ChangeUserInfoAsync(authentication, null, null, null, authority);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task InfoAsync([CommandCompletion(nameof(GetUserIDsAsync))] string userID)
        {
            var sb = new StringBuilder();
            var user = await this.GetUserAsync(userID);
            var userInfo = await user.Dispatcher.InvokeAsync(() => user.UserInfo);
            var props = userInfo.ToDictionary();
            var format = FormatProperties.Format;
            sb.AppendLine(props, format);
            await this.Out.WriteAsync(sb.ToString());
        }

        [ConsoleModeOnly]
        [CommandMethod]
        [CommandMethodProperty(nameof(CategoryPath))]
        public async Task CreateAsync()
        {
            var schema = JsonSchemaUtility.CreateSchema(typeof(JsonUserInfo));
            schema.SetEnums(nameof(JsonUserInfo.CategoryPath), await this.GetCategoryPathsAsync());

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
        public async Task DeleteAsync([CommandCompletion(nameof(GetUserIDsAsync))] string userID)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            if (this.CommandContext.ConfirmToDelete() == true)
            {
                await user.DeleteAsync(authentication);
            }
        }

        [CommandMethod("message")]
        public async Task SendMessageAsync([CommandCompletion(nameof(GetUserIDsAsync))] string userID, string message)
        {
            var user = await this.GetUserAsync(userID);
            var authentication = this.CommandContext.GetAuthentication(this);
            await user.SendMessageAsync(authentication, message);
        }

        [CommandPropertySwitch("online", 'o')]
        public bool IsOnline
        {
            get; set;
        }

        [CommandPropertySwitch("banned", 'b')]
        public bool IsBanned
        {
            get; set;
        }

        [CommandPropertyRequired('m', AllowName = true, IsExplicit = true)]
        public string Message
        {
            get; set;
        }

        [CommandProperty]
        [CommandCompletion(nameof(GetCategoryPathsAsync))]
        public string CategoryPath
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.Drive is UsersConsoleDrive;

        protected IUserContext UserContext => this.cremaHost.GetService(typeof(IUserContext)) as IUserContext;

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

        private Task<string[]> GetUserIDsAsync()
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.UserContext.Users
                            orderby item.ID
                            select item.ID;
                return query.ToArray();
            });
        }

        private Task<string[]> GetCategoryPathsAsync()
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.UserContext.Categories
                            orderby item.Path
                            select item.Path;
                return query.ToArray();
            });
        }

        private Task<string[]> GetOnlineUserIDsAsync()
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.UserContext.Users
                            where item.UserState.HasFlag(UserState.Online)
                            select item.ID;
                return query.ToArray();
            });
        }

        private Task<string[]> GetUnbannedUserIDsAsync()
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.UserContext.Users
                            where item.BanInfo.Path != item.Path
                            select item.ID;
                return query.ToArray();
            });
        }

        private Task<string[]> GetBannedUserIDsAsync()
        {
            return this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.UserContext.Users
                            where item.BanInfo.Path == item.Path
                            select item.ID;
                return query.ToArray();
            });
        }

        private static string FormatUserID(string userID, BanInfo banInfo, UserState userState)
        {
            if (banInfo.Path != string.Empty)
            {
                return TerminalStrings.Foreground(userID, TerminalColor.Red);
            }
            else if (userState != UserState.Online)
            {
                return TerminalStrings.Foreground(userID, TerminalColor.BrightBlack);
            }
            else
            {
                return userID;
            }
        }
    }
}

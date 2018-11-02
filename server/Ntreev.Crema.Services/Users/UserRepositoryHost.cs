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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using Ntreev.Library.IO;

namespace Ntreev.Crema.Services.Users
{
    class UserRepositoryHost : RepositoryHost
    {
        private readonly HashSet<string> users = new HashSet<string>();

        public UserRepositoryHost(UserContext userContext, IRepository repository)
            : base(repository)
        {
            this.UserContext = userContext;
            this.RefreshItems();
        }

        public void RefreshItems()
        {
            var itemPaths = this.Serializer.GetItemPaths(this.UserContext.BasePath, typeof(UserSerializationInfo), ObjectSerializerSettings.Empty);
            this.users.Clear();
            foreach (var item in itemPaths)
            {
                this.users.Add(Path.GetFileName(item));
            }
        }

        public void Commit(Authentication authentication, string comment)
        {
            this.Dispatcher.VerifyAccess();
            var props = new List<LogPropertyInfo>
            {
                //new LogPropertyInfo() { Key = LogPropertyInfo.BranchRevisionKey, Value = $"{this.RepositoryInfo.BranchRevision}"},
            };

            try
            {
                base.Commit(authentication, comment, props.ToArray());
                this.RefreshItems();
            }
            catch
            {
                throw;
            }
        }

        public void CreateUserCategory(string itemPath)
        {
            var directoryName = PathUtility.GetDirectoryName(itemPath);
            if (Directory.Exists(directoryName) == false)
                throw new DirectoryNotFoundException();
            if (Directory.Exists(itemPath) == true)
                throw new IOException();
            Directory.CreateDirectory(itemPath);
            this.Add(itemPath);
        }

        public void RenameUserCategory(UserBaseSet userBaseSet, string categoryPath, string newCategoryPath)
        {
            userBaseSet.SetUserCategoryPath(categoryPath, newCategoryPath);
        }

        public void MoveUserCategory(UserBaseSet userBaseSet, string categoryPath, string newCategoryPath)
        {
            userBaseSet.SetUserCategoryPath(categoryPath, newCategoryPath);
        }

        public void DeleteUserCategory(UserBaseSet userBaseSet, string categoryPath)
        {
            userBaseSet.DeleteUserCategory(categoryPath);
        }

        public void CreateUser(UserBaseSet userBaseSet, string[] userPaths)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in userPaths)
            {
                var name = Path.GetFileName(item);
                if (this.users.Contains(name) == true)
                    throw new ItemAlreadyExistsException(item);
            }
            userBaseSet.CreateUser();
        }

        public void MoveUser(UserBaseSet userBaseSet, string userPath, string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            userBaseSet.MoveUser(userPath, categoryPath);
        }

        public void DeleteUser(UserBaseSet userBaseSet, string userPath)
        {
            this.Dispatcher.VerifyAccess();
            userBaseSet.DeleteUser(userPath);
        }

        public void ModifyUser(UserBaseSet userBaseSet, string userPath, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            this.Dispatcher.VerifyAccess();
            userBaseSet.ModifyUser(userPath, password, newPassword, userName, authority);
        }

        public void BanUser(UserBaseSet userBaseSet, string userPath, string comment)
        {
            this.Dispatcher.VerifyAccess();
            userBaseSet.BanUser(userPath, comment);
        }

        public void UnbanUser(UserBaseSet userBaseSet, string userPath)
        {
            this.Dispatcher.VerifyAccess();
            userBaseSet.UnbanUser(userPath);
        }

        public override CremaHost CremaHost => this.UserContext.CremaHost;

        private UserContext UserContext { get; }

        private IObjectSerializer Serializer => this.UserContext.Serializer;
    }
}

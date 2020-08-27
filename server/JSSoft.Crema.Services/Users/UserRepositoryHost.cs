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
using JSSoft.Crema.Services.Users.Serializations;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace JSSoft.Crema.Services.Users
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

        public UserSerializationInfo Read(string path)
        {
            var repositoryPath = new RepositoryPath(this.UserContext.BasePath, path);
            return this.Serializer.Deserialize<UserSerializationInfo>(repositoryPath, ObjectSerializerSettings.Empty);
        }

        public void Write(string path, UserSerializationInfo userInfo, bool isNew)
        {
            var repositoryItemPath = new RepositoryPath(this.UserContext.BasePath, path);
            if (isNew == true)
            {
                repositoryItemPath.ValidateNotExists(this.Serializer, typeof(UserSerializationInfo));
            }
            this.Serializer.Serialize(repositoryItemPath, userInfo, ObjectSerializerSettings.Empty);
        }

        public void Commit(Authentication authentication, string comment)
        {
            this.Dispatcher.VerifyAccess();
            var props = new List<LogPropertyInfo> { };

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

        public void CreateUserCategory(string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            var repositoryPath = new RepositoryPath(this.UserContext.BasePath, categoryPath);
            var parentPath = repositoryPath.ParentPath;
            if (parentPath.IsExists == false)
                throw new DirectoryNotFoundException();
            if (repositoryPath.IsExists == true)
                throw new IOException();
            Directory.CreateDirectory(repositoryPath.Path);
            this.Add(repositoryPath.Path);
        }

        public void RenameUserCategory(UserContextSet userContextSet, string categoryPath, string newCategoryPath)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.SetUserCategoryPath(categoryPath, newCategoryPath);
        }

        public void MoveUserCategory(UserContextSet userContextSet, string categoryPath, string newCategoryPath)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.SetUserCategoryPath(categoryPath, newCategoryPath);
        }

        public void DeleteUserCategory(UserContextSet userContextSet, string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.DeleteUserCategory(categoryPath);
        }

        public void CreateUser(UserContextSet userContextSet, string[] userPaths)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in userPaths)
            {
                var name = Path.GetFileName(item);
                if (this.users.Contains(name) == true)
                    throw new ItemAlreadyExistsException(item);
            }
            userContextSet.CreateUser();
        }

        public void MoveUser(UserContextSet userContextSet, string userPath, string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.MoveUser(userPath, categoryPath);
        }

        public void DeleteUser(UserContextSet userContextSet, string userPath)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.DeleteUser(userPath);
        }

        public void ModifyUser(UserContextSet userContextSet, string userPath, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.ModifyUser(userPath, password, newPassword, userName, authority);
        }

        public void BanUser(UserContextSet userContextSet, string userPath, string comment)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.BanUser(userPath, comment);
        }

        public void UnbanUser(UserContextSet userContextSet, string userPath)
        {
            this.Dispatcher.VerifyAccess();
            userContextSet.UnbanUser(userPath);
        }

        public override CremaHost CremaHost => this.UserContext.CremaHost;

        private UserContext UserContext { get; }

        private IObjectSerializer Serializer => this.UserContext.Serializer;
    }
}

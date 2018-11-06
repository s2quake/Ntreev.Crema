using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    class UserContextSet
    {
        private readonly Dictionary<string, UserSerializationInfo> users = new Dictionary<string, UserSerializationInfo>();
        private readonly Dictionary<string, UserSerializationInfo> usersToCreate = new Dictionary<string, UserSerializationInfo>();
        private readonly Dictionary<string, UserSerializationInfo> usersToDelete = new Dictionary<string, UserSerializationInfo>();
        private readonly SignatureDateProvider signatureDateProvider;

        private UserContextSet(UserContext userContext, UserSet userSet, bool userCreation)
        {
            this.UserContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            this.UserContext.Dispatcher.VerifyAccess();
            this.Paths = userSet.ItemPaths;
            try
            {
                foreach (var item in userSet.Infos)
                {
                    var user = userContext.Users[item.ID, item.CategoryPath];
                    if (user == null)
                    {
                        if (userCreation == false)
                        {
                            throw new UserNotFoundException(item.ID);
                        }
                        else
                        {
                            this.usersToCreate.Add(item.Path, item);
                        }
                    }
                    else
                    {
                        this.users.Add(item.Path, item);
                    }
                }
                this.signatureDateProvider = userSet.SignatureDateProvider;
            }
            catch
            {
                this.Repository.Dispatcher.Invoke(() => this.Repository.Unlock(userSet.ItemPaths));
                throw;
            }
        }

        public static Task<UserContextSet> CreateAsync(UserContext userContext, UserSet userSet, bool userCreation)
        {
            return userContext.Dispatcher.InvokeAsync(() => new UserContextSet(userContext, userSet, userCreation));
        }

        public void SetUserCategoryPath(string categoryPath, string newCategoryPath)
        {
            var itemPath1 = new RepositoryPath(this.UserContext.BasePath, categoryPath);
            var itemPath2 = new RepositoryPath(this.UserContext.BasePath, newCategoryPath);

            if (itemPath1.IsExists == false)
                throw new DirectoryNotFoundException();
            if (itemPath2.IsExists == true)
                throw new IOException();

            var signatureDate = this.signatureDateProvider.Provide();
            foreach (var item in this.users.ToArray())
            {
                var path = item.Key;
                var userInfo = item.Value;
                if (userInfo.CategoryPath.StartsWith(categoryPath) == false)
                    continue;

                var userInfoCategoryPath = Regex.Replace(userInfo.CategoryPath, "^" + categoryPath, newCategoryPath);
                var repositoryPath1 = new RepositoryPath(this.UserContext.BasePath, path);
                var repositoryPath2 = new RepositoryPath(this.UserContext.BasePath, userInfoCategoryPath + userInfo.ID);
                repositoryPath1.ValidateExists(this.Serializer, typeof(UserSerializationInfo));
                repositoryPath2.ValidateNotExists(this.Serializer, typeof(UserSerializationInfo));
                userInfo.CategoryPath = userInfoCategoryPath;
                userInfo.ModificationInfo = signatureDate;
                this.users[path] = userInfo;
            }

            this.Serialize();
            this.Repository.Move(itemPath1, itemPath2);
        }

        public void DeleteUserCategory(string categoryPath)
        {
            var itemPath = new RepositoryPath(this.UserContext.BasePath, categoryPath);
            this.Repository.Delete(itemPath);
        }

        public void CreateUser()
        {
            foreach (var item in this.usersToCreate.ToArray())
            {
                var path = item.Key;
                var userInfo = item.Value;
                userInfo.CreationInfo = userInfo.ModificationInfo = this.signatureDateProvider.Provide();
                this.usersToCreate[path] = userInfo;
            }
            this.Serialize();
            foreach (var item in this.usersToCreate)
            {
                var repositoryPath = new RepositoryPath(this.UserContext.BasePath, item.Key);
                this.Repository.Add(repositoryPath);
            }
        }

        public void RenameUser(string typePath, string typeName)
        {
            throw new NotImplementedException();
        }

        public void MoveUser(string userPath, string categoryPath)
        {
            var userInfo = this.users[userPath];
            var repositoryPath1 = new RepositoryPath(this.UserContext.BasePath, userInfo.Path);
            var repositoryPath2 = new RepositoryPath(this.UserContext.BasePath, categoryPath + userInfo.ID);
            repositoryPath1.ValidateExists(this.Serializer, typeof(UserSerializationInfo));
            repositoryPath2.ValidateNotExists(this.Serializer, typeof(UserSerializationInfo));
            userInfo.CategoryPath = categoryPath;
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.Serialize();
            this.Repository.Move(repositoryPath1, repositoryPath2);
        }

        public void DeleteUser(string userPath)
        {
            var userInfo = this.users[userPath];
            this.users.Remove(userPath);
            this.usersToDelete.Add(userPath, userInfo);
            var repositoryPaths = this.usersToDelete.Keys.Select(item => new RepositoryPath(this.UserContext.BasePath, item)).ToArray();
            this.Repository.DeleteRange(repositoryPaths);
        }

        public void ModifyUser(string userPath, SecureString password, SecureString newPassword, string userName, Authority? authority)
        {
            var userInfo = this.users[userPath];
            if (newPassword != null)
                userInfo.Password = UserContext.SecureStringToString(newPassword).Encrypt();
            if (userName != null)
                userInfo.Name = userName;
            if (authority.HasValue)
                userInfo.Authority = authority.Value;
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public void BanUser(string userPath, string comment)
        {
            var userInfo = this.users[userPath];
            var banInfo = new BanSerializationInfo()
            {
                Path = userInfo.Path,
                Comment = comment,
                SignatureDate = this.signatureDateProvider.Provide(),
            };
            userInfo.BanInfo = banInfo;
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public void UnbanUser(string userPath)
        {
            var userInfo = this.users[userPath];
            userInfo.BanInfo = BanSerializationInfo.Empty;
            this.users[userPath] = userInfo;
            this.Serialize();
        }

        public UserSerializationInfo GetUserInfo(string path)
        {
            if (this.users.ContainsKey(path) == true)
                return this.users[path];
            if (this.usersToCreate.ContainsKey(path) == true)
                return this.usersToCreate[path];
            throw new NotImplementedException();
        }

        public string[] Paths { get; }

        private void Serialize()
        {
            foreach (var item in this.users)
            {
                var path = item.Key;
                var userInfo = item.Value;
                var itemPath1 = new RepositoryPath(this.UserContext.BasePath, path);
                var itemPath2 = new RepositoryPath(this.UserContext.BasePath, userInfo.Path);

                itemPath1.ValidateExists(this.Serializer, typeof(UserSerializationInfo));
                if (itemPath1 != itemPath2)
                {
                    itemPath2.ValidateNotExists(this.Serializer, typeof(UserSerializationInfo));
                }
                this.Repository.Write(path, userInfo, false);
            }

            foreach (var item in this.usersToCreate)
            {
                var path = item.Key;
                var userInfo = item.Value;
                this.Repository.Write(path, userInfo, true);
            }
        }

        private UserRepositoryHost Repository => this.UserContext.Repository;

        private IObjectSerializer Serializer => this.UserContext.Serializer;

        private UserContext UserContext { get; }
    }
}

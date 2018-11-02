using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using Ntreev.Library.IO;
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
    class UserBaseSet
    {
        private readonly Dictionary<string, UserSerializationInfo> users = new Dictionary<string, UserSerializationInfo>();
        private readonly Dictionary<string, UserSerializationInfo> usersToCreate = new Dictionary<string, UserSerializationInfo>();
        private readonly Dictionary<string, UserSerializationInfo> usersToDelete = new Dictionary<string, UserSerializationInfo>();
        private readonly string[] itemPaths;
        private readonly SignatureDateProvider signatureDateProvider;

        private UserBaseSet(UserContext userContext, UserSet userSet, bool typeCreation)
        {
            this.UserContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            this.UserContext.Dispatcher.VerifyAccess();
            this.itemPaths = userSet.ItemPaths;
            foreach (var item in userSet.Infos)
            {
                var user = userContext.Users[item.ID, item.CategoryPath];
                if (user == null)
                {
                    if (typeCreation == false)
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

        public static Task<UserBaseSet> CreateAsync(UserContext userContext, UserSet userSet, bool userCreation)
        {
            return userContext.Dispatcher.InvokeAsync(() => new UserBaseSet(userContext, userSet, userCreation));
        }

        //public static UserBaseSet Create(UserContext dataBase, CremaDataSet dataSet, bool typeCreation, bool tableCreation)
        //{
        //    return new UserBaseSet(dataBase, dataSet, typeCreation, tableCreation);
        //}

        public void SetUserCategoryPath(string categoryPath, string newCategoryPath)
        {
            var itemPath1 = this.UserContext.GenerateCategoryPath(categoryPath);
            var itemPath2 = this.UserContext.GenerateCategoryPath(newCategoryPath);

            if (Directory.Exists(itemPath1) == false)
                throw new DirectoryNotFoundException();
            if (Directory.Exists(itemPath2) == true)
                throw new IOException();

            //var typeNames = DataSet.ExtendedProperties["TypeNames"] as string[];
            //var typeNameList = typeNames.ToList();
            //var files = DirectoryUtility.GetAllFiles(itemPath1, "*.xml");
            //var sss = files.Select(item => Path.GetFileNameWithoutExtension(item)).ToList();

            foreach (var item in this.users.ToArray())
            {
                var path = item.Key;
                var userInfo = item.Value;
                if (userInfo.CategoryPath.StartsWith(categoryPath) == false)
                    continue;

                this.ValidateUserExists(path);
                userInfo.CategoryPath = Regex.Replace(userInfo.CategoryPath, "^" + categoryPath, newCategoryPath);
                this.ValidateUserNotExists(path);
                //typeNameList.Remove(userInfo.ID);
            }

            this.Serialize();
            this.Repository.Move(itemPath1, itemPath2);
        }

        public void DeleteUserCategory(string categoryPath)
        {
            var itemPath = this.UserContext.GenerateCategoryPath(categoryPath);
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
            this.AddUsersRepositoryPath();
        }

        public void RenameUser(string typePath, string typeName)
        {
            //var dataType = this.types.First(item => item.Path == typePath);
            //this.ValidateTypeExists(dataType.Path);
            //dataType.TypeName = typeName;
            //this.ValidateTypeNotExists(dataType.Path);
            //this.Serialize();
            //this.MoveUsersRepositoryPath();
            throw new NotImplementedException();
        }

        public void MoveUser(string userPath, string categoryPath)
        {
            var userInfo = this.users[userPath];
            this.ValidateUserExists(userInfo.Path);
            userInfo.CategoryPath = categoryPath;
            userInfo.ModificationInfo = this.signatureDateProvider.Provide();
            this.users[userPath] = userInfo;
            this.ValidateUserNotExists(userInfo.Path);
            this.Serialize();
            this.MoveUsersRepositoryPath();
        }

        public void DeleteUser(string userPath)
        {
            var userInfo = this.users[userPath];
            this.users.Remove(userPath);
            this.usersToDelete.Add(userPath, userInfo);
            this.ValidateUserExists(userPath);
            this.DeleteUsersRepositoryPath();
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

        //public static void Modify(CremaDataSet dataSet, UserContext dataBase)
        //{
        //    var dataBaseSet = new UserBaseSet(dataBase, dataSet, false, false);
        //    dataBaseSet.Serialize();
        //}

        //public DataBaseItemState GetTypeState(CremaDataType dataType)
        //{
        //    if (dataType.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
        //    {
        //        var typeInfo = (TypeInfo)dataType.ExtendedProperties[typeof(TypeInfo)];
        //        var itemPath1 = this.UserContext.GeneratePath(dataType.Path);
        //        var itemPath2 = this.UserContext.GeneratePath(typeInfo.Path);

        //        if (dataType.Name != typeInfo.Name)
        //        {
        //            return DataBaseItemState.Rename;
        //        }
        //        else if (itemPath1 != itemPath2)
        //        {
        //            return DataBaseItemState.Move;
        //        }
        //        else
        //        {
        //            return DataBaseItemState.None;
        //        }
        //    }
        //    else
        //    {
        //        return DataBaseItemState.Create;
        //    }
        //}

        //public IReadOnlyDictionary<string, UserSerializationInfo> Users => this.users;

        public UserSerializationInfo GetUserInfo(string path)
        {
            if (this.users.ContainsKey(path) == true)
                return this.users[path];
            if (this.usersToCreate.ContainsKey(path) == true)
                return this.usersToCreate[path];
            throw new NotImplementedException();
        }

        //public CremaDataSet DataSet { get; }

        public string[] ItemPaths => this.itemPaths;

        private void Serialize()
        {
            foreach (var item in this.users)
            {
                var path = item.Key;
                var userInfo = item.Value;
                var itemPath1 = this.UserContext.GeneratePath(path);
                var itemPath2 = this.UserContext.GeneratePath(userInfo.Path);

                if (itemPath1 != itemPath2)
                {
                    this.ValidateUserNotExists(userInfo.Path);
                    this.ValidateUserExists(path);
                }
                else
                {
                    this.ValidateUserExists(path);
                }

                this.Serializer.Serialize(itemPath1, userInfo, ObjectSerializerSettings.Empty);
            }

            foreach (var item in this.usersToCreate)
            {
                var path = item.Key;
                var userInfo = item.Value;
                var itemPath = this.UserContext.GenerateUserPath(userInfo.CategoryPath, userInfo.ID);

                this.ValidateUserNotExists(path);

                var sss = Path.GetFileName(itemPath);
                var files = DirectoryUtility.GetAllFiles(this.UserContext.BasePath, sss + ".*");
                if (files.Any())
                {
                    System.Diagnostics.Debugger.Launch();
                }

                this.Serializer.Serialize(itemPath, userInfo, ObjectSerializerSettings.Empty);
            }
        }

        private void AddUsersRepositoryPath()
        {
            foreach (var item in this.usersToCreate)
            {
                this.AddRepositoryPath(item.Value);
            }
        }

        private void MoveUsersRepositoryPath()
        {
            foreach (var item in this.users)
            {
                var path = item.Key;
                var userInfo = item.Value;
                if (userInfo.Path == path)
                    continue;

                this.MoveRepositoryPath(userInfo, path);
            }
        }

        private void DeleteUsersRepositoryPath()
        {
            foreach (var item in this.usersToDelete)
            {
                var path = item.Key;
                var userInfo = item.Value;
                this.DeleteRepositoryPath(userInfo, path);
            }
        }

        private void AddRepositoryPath(UserSerializationInfo userInfo)
        {
            var itemPath = this.UserContext.GenerateUserPath(userInfo.CategoryPath, userInfo.ID);
            var files = this.UserContext.GetFiles(itemPath);
            var status = this.Repository.Status(files);

            foreach (var item in status)
            {
                if (item.Status == RepositoryItemStatus.Untracked)
                {
                    this.Repository.Add(item.Path);
                }
            }
        }

        private void MoveRepositoryPath(UserSerializationInfo userInfo, string userPath)
        {
            var itemPath = this.UserContext.GeneratePath(userPath);
            var files = this.UserContext.GetFiles(itemPath);

            for (var i = 0; i < files.Length; i++)
            {
                var path1 = files[i];
                var extension = Path.GetExtension(path1);
                var path2 = this.UserContext.GeneratePath(userInfo.Path) + extension;
                this.Repository.Move(path1, path2);
            }
        }

        private void DeleteRepositoryPath(UserSerializationInfo dataType, string typePath)
        {
            var itemPath = this.UserContext.GeneratePath(typePath);
            var files = this.UserContext.GetFiles(itemPath);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
            this.Repository.DeleteRange(files);
        }

        private void ValidateUserExists(string typePath)
        {
            var itemPath = this.UserContext.GeneratePath(typePath);
            var files = this.Serializer.GetPath(itemPath, typeof(UserSerializationInfo), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
        }

        private void ValidateUserNotExists(string typePath)
        {
            var itemPath = this.UserContext.GeneratePath(typePath);
            var files = this.Serializer.GetPath(itemPath, typeof(UserSerializationInfo), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == true)
                    throw new FileNotFoundException();
            }
        }

        private UserRepositoryHost Repository => this.UserContext.Repository;

        private IObjectSerializer Serializer => this.UserContext.Serializer;

        private UserContext UserContext { get; }
    }
}

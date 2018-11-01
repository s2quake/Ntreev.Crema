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
        public UserRepositoryHost(UserContext userContext, IRepository repository)
            : base(repository)
        {
            this.UserContext = userContext;
        }

        public void CreateCategory(Authentication authentication, string itemPath)
        {
            var parentItemPath = PathUtility.GetDirectoryName(itemPath);
            if (DirectoryUtility.Exists(parentItemPath) == false)
                throw new DirectoryNotFoundException();
            Directory.CreateDirectory(itemPath);
            this.Add(itemPath);
        }

        public void RenameCategory(Authentication authentication, UserSerializationInfo[] userInfos, string categoryPath, string newCategoryPath)
        {
            var itemPath1 = this.UserContext.GenerateCategoryPath(categoryPath);
            var itemPath2 = this.UserContext.GenerateCategoryPath(newCategoryPath);

            if (DirectoryUtility.Exists(itemPath1) == false)
                throw new DirectoryNotFoundException();

            for (var i = 0; i < userInfos.Length; i++)
            {
                var item = userInfos[i];
                if (item.CategoryPath.StartsWith(categoryPath) == false)
                    continue;
                var itemPath = this.UserContext.GenerateUserPath(item.CategoryPath, item.ID);
                item.CategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
                this.Serializer.Serialize(itemPath, item, ObjectSerializerSettings.Empty);
            }
            this.Move(itemPath1, itemPath2);
        }

        public void MoveCategory(Authentication authentication, UserSerializationInfo[] userInfos, string categoryPath, string newCategoryPath)
        {
            var itemPath1 = this.UserContext.GenerateCategoryPath(categoryPath);
            var itemPath2 = this.UserContext.GenerateCategoryPath(newCategoryPath);

            if (DirectoryUtility.Exists(itemPath1) == false)
                throw new DirectoryNotFoundException();

            for (var i = 0; i < userInfos.Length; i++)
            {
                var item = userInfos[i];
                if (item.CategoryPath.StartsWith(categoryPath) == false)
                    continue;
                var itemPath = this.UserContext.GenerateUserPath(item.CategoryPath, item.ID);
                item.CategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
                this.Serializer.Serialize(itemPath, item, ObjectSerializerSettings.Empty);
            }
            this.Move(itemPath1, itemPath2);
        }

        public void CreateUser(UserSerializationInfo userInfo)
        {
            var itemPath = this.UserContext.GenerateUserPath(userInfo.CategoryPath, userInfo.ID);
            var directoryPath = Path.GetDirectoryName(itemPath);
            if (DirectoryUtility.Exists(directoryPath) == false)
                throw new DirectoryNotFoundException();
            var files = this.UserContext.GetFiles(itemPath);
            if (files.Any() == true)
                throw new IOException();

            var itemPaths = this.Serializer.Serialize(itemPath, userInfo, ObjectSerializerSettings.Empty);
            this.AddRange(itemPaths);
        }

        public void MoveUser(UserDataSet dataSet, string path, string categoryPath)
        {
            var userInfo = dataSet.Infos.First(item => item.Path == path);
            this.ValidateUserExists(userInfo.Path);
            userInfo.CategoryPath = categoryPath;
            this.ValidateUserNotExists(userInfo.Path);
            this.Serialize();
            this.MoveRepositoryPath();

            var itemPath = this.UserContext.GeneratePath(path);
            var categoryItemPath = this.UserContext.GeneratePath(categoryPath);
            var directoryPath = Path.GetDirectoryName(itemPath);
            if (DirectoryUtility.Exists(directoryPath) == false)
                throw new DirectoryNotFoundException();
            if (DirectoryUtility.Exists(categoryItemPath) == false)
                throw new DirectoryNotFoundException();
            var files = this.UserContext.GetFiles(itemPath);
            if (files.Any() == false)
                throw new FileNotFoundException();

            files = this.Serializer.Serialize(itemPath, serializationInfo, ObjectSerializerSettings.Empty);
            for (var i = 0; i < files.Length; i++)
            {
                var path1 = files[i];
                var extension = Path.GetExtension(path1);
                var path2 = this.UserContext.GeneratePath(serializationInfo.CategoryPath + serializationInfo.ID) + extension;
                this.Move(path1, path2);
            }
        }

        public void DeleteUser(string itemPath)
        {
            var files = this.UserContext.GetFiles(itemPath);
            if (files.Any() == false)
                throw new FileNotFoundException();
            this.DeleteRange(files);
        }

        public void ModifyUser(string itemPath, UserSerializationInfo serializationInfo)
        {
            var directoryPath = Path.GetDirectoryName(itemPath);
            if (DirectoryUtility.Exists(directoryPath) == false)
                throw new DirectoryNotFoundException();
            var files = this.UserContext.GetFiles(itemPath);
            if (files.Any() == false)
                throw new FileNotFoundException();
            this.Serializer.Serialize(itemPath, serializationInfo, ObjectSerializerSettings.Empty);
        }

        public override CremaHost CremaHost => this.UserContext.CremaHost;

        private void MoveRepositoryPath(UserSerializationInfo userSerializationInfo, string path)
        {
            var itemPath = this.UserContext.GeneratePath(path);
            var files = this.UserContext.GetFiles(itemPath);

            for (var i = 0; i < files.Length; i++)
            {
                var path1 = files[i];
                var extension = Path.GetExtension(path1);
                var path2 = this.UserContext.GeneratePath(userSerializationInfo.CategoryPath + userSerializationInfo.ID) + extension;
                this.Move(path1, path2);
            }
        }

        private void ValidateUserExists(string path)
        {
            var itemPath = this.UserContext.GeneratePath(path);
            var files = this.Serializer.GetPath(itemPath, typeof(UserSerializationInfo), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
        }

        private void ValidateUserNotExists(string path)
        {
            var itemPath = this.UserContext.GeneratePath(path);
            var files = this.Serializer.GetPath(itemPath, typeof(UserSerializationInfo), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == true)
                    throw new FileNotFoundException();
            }
        }

        private UserContext UserContext { get; }

        private IObjectSerializer Serializer => this.UserContext.Serializer;
    }
}

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

using Ntreev.Crema.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Ntreev.Library.IO;
using Ntreev.Crema.ServiceModel;
using Ntreev.Library.ObjectModel;
using System.Threading.Tasks;

namespace Ntreev.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleDrive))]
    public sealed class UsersConsoleDrive : ConsoleDriveBase
    {
        [Import]
        private Lazy<ICremaHost> cremaHost = null;

        private string path;

        internal UsersConsoleDrive()
            : base("users")
        {

        }

        public override string[] GetPaths()
        {
            return this.UserContext.Dispatcher.Invoke(() => this.UserContext.Select(item => item.Path).ToArray());
        }

        public override async Task<object> GetObjectAsync(Authentication authentication, string path)
        {
            return await this.GetObjectAsync(path);
        }

        public Task<IUser> GetUserAsync(string userID)
        {
            return this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.Users[userID]);
        }

        public string[] GetUserList()
        {
            return this.UserContext.Dispatcher.Invoke(() => this.UserContext.Users.Select(item => item.ID).ToArray());
        }

        protected override async Task OnCreateAsync(Authentication authentication, string path, string name)
        {
            var category = await this.GetObjectAsync(path) as IUserCategory;
            if (category == null)
                throw new CategoryNotFoundException(path);
            await category.AddNewCategoryAsync(authentication, name);
        }

        protected override async Task OnMoveAsync(Authentication authentication, string path, string newPath)
        {
            var sourceObject = await this.GetObjectAsync(path);

            if (sourceObject is IUser sourceUser)
            {
                await this.MoveUserAsync(authentication, sourceUser, newPath);
            }
            else if (sourceObject is IUserCategory sourceUserCategory)
            {
                await this.MoveUserCategoryAsync(authentication, sourceUserCategory, newPath);
            }
            else
            {
                throw new ItemNotFoundException(path);
            }
        }

        protected override async Task OnDeleteAsync(Authentication authentication, string path)
        {
            var userItem = await this.GetObjectAsync(path) as IUserItem;
            if (userItem == null)
                throw new ItemNotFoundException(path);
            await userItem.DeleteAsync(authentication);
        }

        protected override Task OnSetPathAsync(Authentication authentication, string path)
        {
            return Task.Run(() =>
            {
                this.path = path;
            });
        }
        
        private async Task MoveUserCategoryAsync(Authentication authentication, IUserCategory sourceCategory, string destPath)
        {
            var destObject = await this.GetObjectAsync(destPath);
            //var dataBase = sourceCategory.GetService(typeof(IDataBase)) as IDataBase;
            var users = sourceCategory.GetService(typeof(IUserCollection)) as IUserCollection;

            //if (destPath.DataBaseName != dataBase.Name)
            //    throw new InvalidOperationException($"cannot move to : {destPath}");
            //if (destPath.Context != CremaSchema.UserDirectory)
            //    throw new InvalidOperationException($"cannot move to : {destPath}");
            if (destObject is IUser)
                throw new InvalidOperationException($"cannot move to : {destPath}");

            if (destObject is IUserCategory destCategory)
            {
                if (sourceCategory.Parent != destCategory)
                    await sourceCategory.MoveAsync(authentication, destCategory.Path);
            }
            else
            {
                if (NameValidator.VerifyCategoryPath(destPath) == true)
                    throw new InvalidOperationException($"cannot move to : {destPath}");
                var itemName = new ItemName(destPath);
                var categories = sourceCategory.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
                if (await categories.ContainsAsync(itemName.CategoryPath) == false)
                    throw new InvalidOperationException($"cannot move to : {destPath}");
                if (sourceCategory.Name != itemName.Name && await users.ContainsAsync(itemName.Name) == true)
                    throw new InvalidOperationException($"cannot move to : {destPath}");
                if (sourceCategory.Parent.Path != itemName.CategoryPath)
                    await sourceCategory.MoveAsync(authentication, itemName.CategoryPath);
                if (sourceCategory.Name != itemName.Name)
                    await sourceCategory.RenameAsync(authentication, itemName.Name);
            }
        }

        private async Task MoveUserAsync(Authentication authentication, IUser sourceUser, string destPath)
        {
            var destObject = await this.GetObjectAsync(destPath);
            var users = sourceUser.GetService(typeof(IUserCollection)) as IUserCollection;

            if (destObject is IUserCategory destCategory)
            {
                if (sourceUser.Category != destCategory)
                    await sourceUser.MoveAsync(authentication, destCategory.Path);
            }
            else
            {
                if (NameValidator.VerifyCategoryPath(destPath) == true)
                    throw new InvalidOperationException($"cannot move to : {destPath}");
                var itemName = new ItemName(destPath);
                var categories = sourceUser.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
                if (await categories.ContainsAsync(itemName.CategoryPath) == false)
                    throw new InvalidOperationException($"cannot move to : {destPath}");
                if (sourceUser.ID != itemName.Name)
                    throw new InvalidOperationException($"cannot move to : {destPath}");
                if (sourceUser.Category.Path != itemName.CategoryPath)
                    await sourceUser.MoveAsync(authentication, itemName.CategoryPath);
            }
        }

        private Task<IUserItem> GetObjectAsync(string path)
        {
            return this.UserContext.Dispatcher.InvokeAsync(GetObject);

            IUserItem GetObject()
            {
                if (NameValidator.VerifyCategoryPath(path) == true)
                {
                    return this.UserContext[path];
                }
                else
                {
                    var itemName = new ItemName(path);
                    var category = this.UserContext.Categories[itemName.CategoryPath];
                    if (category.Categories.ContainsKey(itemName.Name) == true)
                        return category.Categories[itemName.Name] as IUserItem;
                    if (category.Users.ContainsKey(itemName.Name) == true)
                        return category.Users[itemName.Name] as IUserItem;
                    return null;
                }
            }
        }

        private IUserContext UserContext => this.cremaHost.Value.GetService(typeof(IUserContext)) as IUserContext;
    }
}

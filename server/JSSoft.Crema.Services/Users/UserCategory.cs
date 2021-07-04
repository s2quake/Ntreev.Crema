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
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users.Arguments;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Users
{
    class UserCategory : UserCategoryBase<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUserCategory, IUserItem
    {
        public async Task<Guid> RenameAsync(Authentication authentication, string name)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (name is null)
                    throw new ArgumentNullException(nameof(name));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    this.ValidateRename(authentication, name);
                    return new UserCategoryRenameArguments(this, name);
                });
                var taskID = Guid.NewGuid();
                var items = args.Items;
                var oldNames = args.OldNames;
                var oldPaths = args.OldPaths;
                await this.Container.InvokeCategoryRenameAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Name = name;
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeCategoriesRenamedEvent(authentication, items, oldNames, oldPaths);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> MoveAsync(Authentication authentication, string parentPath)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (parentPath is null)
                    throw new ArgumentNullException(nameof(parentPath));

                this.ValidateExpired();
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, parentPath);
                    this.ValidateMove(authentication, parentPath);
                    return new UserCategoryMoveArguments(this, parentPath);
                });
                var taskID = Guid.NewGuid();
                await this.Container.InvokeCategoryMoveAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Parent = this.Container[parentPath];
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeCategoriesMovedEvent(authentication, args.Items, args.OldPaths, args.OldParentPaths);
                    this.Context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> DeleteAsync(Authentication authentication)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                this.ValidateExpired();
                var container = this.Container;
                var repository = this.Repository;
                var cremaHost = this.CremaHost;
                var context = this.Context;
                var args = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    this.ValidateDelete(authentication);
                    return new UserCategoryDeleteArguments(this);
                });
                var taskID = Guid.NewGuid();
                await this.Container.InvokeCategoryDeleteAsync(authentication, args);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Dispose();
                    cremaHost.Sign(authentication);
                    container.InvokeCategoriesDeletedEvent(authentication, args.Items, args.OldPaths);
                    context.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<UserCategory> AddNewCategoryAsync(Authentication authentication, string name)
        {
            return this.Container.AddNewAsync(authentication, name, base.Path);
        }

        public Task<User> AddNewUserAsync(Authentication authentication, string userID, SecureString password, string userName, Authority authority)
        {
            return this.Context.Users.AddNewAsync(authentication, userID, base.Path, password, userName, authority);
        }

        public void ValidateRename(Authentication authentication, string name)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (base.Name == name)
                throw new ArgumentException(Resources.Exception_SameName, nameof(name));
            base.ValidateRename(name);
        }

        public void ValidateMove(Authentication authentication, string parentPath)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (this.Parent == null)
                throw new InvalidOperationException("root cannot move.");
            if (this.Parent.Path == parentPath)
                throw new ArgumentException(Resources.Exception_CannotMoveToSamePath, nameof(parentPath));
            if (parentPath == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringIsNotAllowed);
            var parent = this.Container[parentPath];
            if (parent == null)
                throw new CategoryNotFoundException(parentPath);

            base.ValidateMove(parent);
        }

        public void ValidateDelete(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            base.ValidateDelete();
            if (this.Items.Any() == true || this.Categories.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeletePathWithItems);
        }

        // private (string[] userPaths, string[] lockPaths) GetPathForData(CategoryName targetName)
        // {
        //     var targetPaths = new string[]
        //     {
        //         targetName.ParentPath,
        //         targetName,
        //     };
        //     var items = EnumerableUtility.FamilyTree(this as IUserItem, item => item.Childs);
        //     var users = items.Where(item => item is User).Select(item => item as User).ToArray();
        //     var userPaths = users.Select(item => item.Path).ToArray();
        //     var itemPaths = items.Select(item => item.Path).ToArray();
        //     var lockPaths = itemPaths.Concat(targetPaths).Distinct().OrderBy(item => item).ToArray();
        //     return (userPaths, lockPaths);
        // }

        // public async Task<UserSet> ReadDataForPathAsync(Authentication authentication, CategoryName targetName)
        // {
        //     var tuple = await this.Dispatcher.InvokeAsync(() =>
        //     {
        //         var targetPaths = new string[]
        //         {
        //             targetName.ParentPath,
        //             targetName,
        //         };
        //         var items = EnumerableUtility.FamilyTree(this as IUserItem, item => item.Childs);
        //         var users = items.Where(item => item is User).Select(item => item as User).ToArray();
        //         var userPaths = users.Select(item => item.Path).ToArray();
        //         var itemPaths = items.Select(item => item.Path).ToArray();
        //         var paths = itemPaths.Concat(targetPaths).Distinct().OrderBy(item => item).ToArray();
        //         return (userPaths, paths);
        //     });
        //     return await this.Repository.Dispatcher.InvokeAsync((Func<UserSet>)(() =>
        //     {
        //         this.Repository.Lock(authentication, this, nameof(ReadDataForPathAsync), tuple.paths);
        //         var userInfoList = new List<UserSerializationInfo>(tuple.userPaths.Length);
        //         foreach (var item in tuple.userPaths)
        //         {
        //             var userInfo = this.Repository.Read(item);
        //             userInfoList.Add(userInfo);
        //         }
        //         var dataSet = new UserSet()
        //         {
        //             ItemPaths = tuple.paths,
        //             Infos = userInfoList.ToArray(),
        //             SignatureDateProvider = new SignatureDateProvider(authentication.ID),
        //         };
        //         return dataSet;
        //     }));
        // }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public CremaHost CremaHost => this.Context.CremaHost;

        public UserRepositoryHost Repository => this.Context.Repository;

        public IObjectSerializer Serializer => this.Context.Serializer;

        public new string Name => base.Name;

        public new string Path => base.Path;

        public new event EventHandler Renamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed -= value;
            }
        }

        public new event EventHandler Moved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Moved -= value;
            }
        }

        public new event EventHandler Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted -= value;
            }
        }

        #region IUserCategory

        Task IUserCategory.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task IUserCategory.MoveAsync(Authentication authentication, string parentPath)
        {
            return this.MoveAsync(authentication, parentPath);
        }

        Task IUserCategory.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        async Task<IUserCategory> IUserCategory.AddNewCategoryAsync(Authentication authentication, string name)
        {
            return await this.AddNewCategoryAsync(authentication, name);
        }

        async Task<IUser> IUserCategory.AddNewUserAsync(Authentication authentication, string userID, SecureString password, string userName, Authority authority)
        {
            return await this.AddNewUserAsync(authentication, userID, password, userName, authority);
        }

        IUserCategory IUserCategory.Parent => this.Parent;

        IContainer<IUser> IUserCategory.Users
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return this.Items;
            }
        }

        IContainer<IUserCategory> IUserCategory.Categories
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return this.Categories;
            }
        }

        #endregion

        #region IUserItem

        Task IUserItem.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task IUserItem.MoveAsync(Authentication authentication, string parentPath)
        {
            return this.MoveAsync(authentication, parentPath);
        }

        Task IUserItem.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        IUserItem IUserItem.Parent => this.Parent;

        IEnumerable<IUserItem> IUserItem.Childs
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                foreach (var item in this.Categories)
                {
                    yield return item;
                }
                foreach (var item in this.Items)
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.Context as IUserContext).GetService(serviceType);
        }

        #endregion
    }
}

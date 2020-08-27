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
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    class UserCategory : UserCategoryBase<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUserCategory, IUserItem
    {
        public async Task<Guid> RenameAsync(Authentication authentication, string name)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    this.ValidateRename(authentication, name);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    var targetName = new CategoryName(base.Path) { Name = name };
                    return (items, oldNames, oldPaths, path, targetName);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                using var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeCategoryRenameAsync(authentication, tuple.path, name, userContextSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.Name = name;
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeCategoriesRenamedEvent(authentication, tuple.items, tuple.oldNames, tuple.oldPaths);
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
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, parentPath);
                    this.ValidateMove(authentication, parentPath);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldParentPaths = items.Select(item => item.Parent.Path).ToArray();
                    var path = base.Path;
                    var targetName = new CategoryName(parentPath, base.Name);
                    return (items, oldPaths, oldParentPaths, path, targetName);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                using var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeCategoryMoveAsync(authentication, tuple.path, parentPath, userContextSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Parent = this.Container[parentPath];
                    this.CremaHost.Sign(authentication);
                    this.Container.InvokeCategoriesMovedEvent(authentication, tuple.items, tuple.oldPaths, tuple.oldParentPaths);
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
                this.ValidateExpired();
                var container = this.Container;
                var repository = this.Repository;
                var cremaHost = this.CremaHost;
                var context = this.Context;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    this.ValidateDelete(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldPaths, path);
                });
                var taskID = Guid.NewGuid();
                var userSet = await this.ReadDataForPathAsync(authentication, new CategoryName(tuple.path));
                using var userContextSet = await UserContextSet.CreateAsync(this.Context, userSet, false);
                await this.Container.InvokeCategoryDeleteAsync(authentication, tuple.path, userContextSet);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Dispose();
                    cremaHost.Sign(authentication);
                    container.InvokeCategoriesDeletedEvent(authentication, tuple.items, tuple.oldPaths);
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
            if (this.Parent.Path == parentPath)
                throw new ArgumentException(Resources.Exception_CannotMoveToSamePath, nameof(parentPath));
            var parent = this.Container[parentPath];
            if (parent == null)
                throw new CategoryNotFoundException(parentPath);

            base.ValidateMove(parent);
        }

        public void ValidateDelete(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
            {
                throw new PermissionDeniedException();
            }

            base.ValidateDelete();

            if (EnumerableUtility.Descendants<IItem, IUser>(this as IItem, item => item.Childs).Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeletePathWithItems);
        }

        public async Task<UserSet> ReadDataForPathAsync(Authentication authentication, CategoryName targetName)
        {
            var tuple = await this.Dispatcher.InvokeAsync(() =>
            {
                var targetPaths = new string[]
                {
                    targetName.ParentPath,
                    targetName,
                };
                var items = EnumerableUtility.FamilyTree(this as IUserItem, item => item.Childs);
                var users = items.Where(item => item is User).Select(item => item as User).ToArray();
                var userPaths = users.Select(item => item.Path).ToArray();
                var itemPaths = items.Select(item => item.Path).ToArray();
                var paths = itemPaths.Concat(targetPaths).Distinct().OrderBy(item => item).ToArray();
                return (userPaths, paths);
            });
            return await this.Repository.Dispatcher.InvokeAsync((Func<UserSet>)(() =>
            {
                this.Repository.Lock(authentication, this, nameof(ReadDataForPathAsync), tuple.paths);
                var userInfoList = new List<UserSerializationInfo>(tuple.userPaths.Length);
                foreach (var item in tuple.userPaths)
                {
                    var userInfo = this.Repository.Read(item);
                    userInfoList.Add(userInfo);
                }
                var dataSet = new UserSet()
                {
                    ItemPaths = tuple.paths,
                    Infos = userInfoList.ToArray(),
                    SignatureDateProvider = new SignatureDateProvider(authentication.ID),
                };
                return dataSet;
            }));
        }

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

        IContainer<IUser> IUserCategory.Users => this.Items;

        IContainer<IUserCategory> IUserCategory.Categories => this.Categories;

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

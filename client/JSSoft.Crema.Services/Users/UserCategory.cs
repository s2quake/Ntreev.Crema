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

using JSSoft.Crema.ServiceHosts.Users;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library.Linq;
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
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldNames, oldPaths, path);
                });
                var result = await this.Service.RenameUserItemAsync(tuple.path, name);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
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
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldParentPaths = items.Select(item => item.Parent.Path).ToArray();
                    var path = base.Path;
                    return (items, oldPaths, oldParentPaths, path);
                });
                var result = await this.Service.MoveUserItemAsync(tuple.path, parentPath);
                await this.Context.WaitAsync(result.TaskID);
                return result.TaskID;
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
                var context = this.Context;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldPaths, path);
                });
                var result = await this.Service.DeleteUserItemAsync(tuple.path);
                await context.WaitAsync(result.TaskID);
                return result.TaskID;
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

        public void InternalSetName(string name)
        {
            base.Name = name;
        }

        public IUserContextService Service => this.Context.Service;

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

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

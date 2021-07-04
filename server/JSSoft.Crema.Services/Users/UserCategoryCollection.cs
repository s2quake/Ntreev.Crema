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
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Users
{
    class UserCategoryCollection : CategoryContainer<User, UserCategory, UserCollection, UserCategoryCollection, UserContext>,
        IUserCategoryCollection
    {
        private ItemsCreatedEventHandler<IUserCategory> categoriesCreated;
        private ItemsRenamedEventHandler<IUserCategory> categoriesRenamed;
        private ItemsMovedEventHandler<IUserCategory> categoriesMoved;
        private ItemsDeletedEventHandler<IUserCategory> categoriesDeleted;

        public UserCategoryCollection()
        {

        }

        public async Task<UserCategory> AddNewAsync(Authentication authentication, string name, string parentPath)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (name is null)
                    throw new ArgumentNullException(nameof(name));
                if (parentPath is null)
                    throw new ArgumentNullException(nameof(parentPath));

                this.ValidateExpired();
                var categoryName = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewAsync), this, name, parentPath);
                    this.ValidateAddNew(authentication, name, parentPath);
                    return new CategoryName(parentPath, name);
                });
                await this.InvokeCategoryCreateAsync(authentication, categoryName);
                var result = await this.Dispatcher.InvokeAsync(() =>
                {
                    var category = this.BaseAddNew(name, parentPath, authentication);
                    category.Guid = Guid.NewGuid();
                    this.CremaHost.Sign(authentication);
                    this.InvokeCategoriesCreatedEvent(authentication, new UserCategory[] { category });
                    this.Context.InvokeTaskCompletedEvent(authentication, category.Guid);
                    return category;
                });
                return result;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task InvokeCategoryCreateAsync(Authentication authentication, string categoryPath)
        {
            var context = this.Context;
            var repository = this.Repository;
            var itemPaths = new string[] { categoryPath };
            var message = EventMessageBuilder.CreateUserCategory(authentication, categoryPath);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeCategoryCreateAsync), itemPaths);
                try
                {
                    var userContextSet = UserContextSet.CreateEmpty(authentication, context, itemPaths);
                    repository.CreateUserCategory(categoryPath);
                    repository.Commit(authentication, message);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeCategoryRenameAsync(Authentication authentication, UserCategoryRenameArguments args)
        {
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.RenameUserCategory(authentication, args.CategoryPath, args.Name);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeCategoryRenameAsync), args.LockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.RenameUserCategory(userContextSet, args.CategoryPath, args.NewCategoryPath);
                    repository.Commit(authentication, message);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeCategoryMoveAsync(Authentication authentication, UserCategoryMoveArguments args)
        {
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.MoveUserCategory(authentication, args.CategoryPath, args.ParentPath, args.NewParentPath);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeCategoryMoveAsync), args.LockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.MoveUserCategory(userContextSet, args.CategoryPath, args.NewCategoryPath);
                    repository.Commit(authentication, message);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeCategoryDeleteAsync(Authentication authentication, UserCategoryDeleteArguments args)
        {
            var context = this.Context;
            var repository = this.Repository;
            var message = EventMessageBuilder.DeleteUserCategory(authentication, args.CategoryPath);
            return repository.Dispatcher.InvokeAsync(() =>
            {
                using var locker = new RepositoryHostLock(repository, authentication, this, nameof(InvokeCategoryDeleteAsync), args.LockPaths);
                try
                {
                    var userSet = args.Read(authentication, repository);
                    var userContextSet = new UserContextSet(context, userSet, false);
                    repository.DeleteUserCategory(userContextSet, args.CategoryPath);
                    repository.Commit(authentication, message);
                }
                catch
                {
                    repository.Revert();
                    throw;
                }
            });
        }

        public void InvokeCategoriesCreatedEvent(Authentication authentication, UserCategory[] categories)
        {
            var args = categories.Select(item => (object)null).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesCreatedEvent), categories);
            var message = EventMessageBuilder.CreateUserCategory(authentication, categories);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesCreated(new ItemsCreatedEventArgs<IUserCategory>(authentication, categories, args));
            this.Context.InvokeItemsCreatedEvent(authentication, categories, args);
        }

        public void InvokeCategoriesRenamedEvent(Authentication authentication, UserCategory[] categories, string[] oldNames, string[] oldPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesRenamedEvent), categories, oldNames, oldPaths);
            var message = EventMessageBuilder.RenameUserCategory(authentication, categories, oldPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesRenamed(new ItemsRenamedEventArgs<IUserCategory>(authentication, categories, oldNames, oldPaths));
            this.Context.InvokeItemsRenamedEvent(authentication, categories, oldNames, oldPaths);
        }

        public void InvokeCategoriesMovedEvent(Authentication authentication, UserCategory[] categories, string[] oldPaths, string[] oldParentPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesMovedEvent), categories, oldPaths, oldParentPaths);
            var message = EventMessageBuilder.MoveUserCategory(authentication, categories, oldPaths, oldParentPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesMoved(new ItemsMovedEventArgs<IUserCategory>(authentication, categories, oldPaths, oldParentPaths));
            this.Context.InvokeItemsMovedEvent(authentication, categories, oldPaths, oldParentPaths);
        }

        public void InvokeCategoriesDeletedEvent(Authentication authentication, UserCategory[] categories, string[] categoryPaths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesDeletedEvent), categoryPaths);
            var message = EventMessageBuilder.DeleteUserCategory(authentication, categories);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesDeleted(new ItemsDeletedEventArgs<IUserCategory>(authentication, categories, categoryPaths));
            this.Context.InvokeItemsDeleteEvent(authentication, categories, categoryPaths);
        }

        public UserRepositoryHost Repository => this.Context.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.Context.Serializer;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<IUserCategory> CategoriesCreated
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesCreated += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<IUserCategory> CategoriesRenamed
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesRenamed += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<IUserCategory> CategoriesMoved
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesMoved += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<IUserCategory> CategoriesDeleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesDeleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.categoriesDeleted -= value;
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                base.CollectionChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                base.CollectionChanged -= value;
            }
        }

        protected virtual void OnCategoriesCreated(ItemsCreatedEventArgs<IUserCategory> e)
        {
            this.categoriesCreated?.Invoke(this, e);
        }

        protected virtual void OnCategoriesRenamed(ItemsRenamedEventArgs<IUserCategory> e)
        {
            this.categoriesRenamed?.Invoke(this, e);
        }

        protected virtual void OnCategoriesMoved(ItemsMovedEventArgs<IUserCategory> e)
        {
            this.categoriesMoved?.Invoke(this, e);
        }

        protected virtual void OnCategoriesDeleted(ItemsDeletedEventArgs<IUserCategory> e)
        {
            this.categoriesDeleted?.Invoke(this, e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher?.VerifyAccess();
            base.OnCollectionChanged(e);
        }

        private void ValidateAddNew(Authentication authentication, string name, string parentPath)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            base.ValidateAddNew(name, parentPath, null);
        }

        #region IUserCategoryCollection

        bool IUserCategoryCollection.Contains(string categoryPath)
        {
            if (categoryPath is null)
                throw new ArgumentNullException(nameof(categoryPath));

            this.Dispatcher.VerifyAccess();
            return this.Contains(categoryPath);
        }

        IUserCategory IUserCategoryCollection.Root => this.Root;

        IUserCategory IUserCategoryCollection.this[string categoryPath]
        {
            get
            {
                if (categoryPath is null)
                    throw new ArgumentNullException(nameof(categoryPath));

                this.Dispatcher.VerifyAccess();
                if (categoryPath == string.Empty)
                    throw new ArgumentException(Resources.Exception_EmptyStringIsNotAllowed);
                if (this.Contains(categoryPath) == false)
                    throw new CategoryNotFoundException(categoryPath);
                return base[categoryPath];
            }
        }

        #endregion

        #region IReadOnlyCollection<IUserCategory>

        int IReadOnlyCollection<IUserCategory>.Count
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return this.Count;
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator<IUserCategory> IEnumerable<IUserCategory>.GetEnumerator()
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in this)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in this)
            {
                yield return item;
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

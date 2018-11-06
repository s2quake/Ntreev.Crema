﻿//Released under the MIT License.
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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TypeCategoryCollection : CategoryContainer<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext>,
        ITypeCategoryCollection
    {
        private ItemsCreatedEventHandler<ITypeCategory> categoriesCreated;
        private ItemsRenamedEventHandler<ITypeCategory> categoriesRenamed;
        private ItemsMovedEventHandler<ITypeCategory> categoriesMoved;
        private ItemsDeletedEventHandler<ITypeCategory> categoriesDeleted;

        public TypeCategoryCollection()
        {

        }

        public async Task<TypeCategory> AddNewAsync(Authentication authentication, string name, string parentPath)
        {
            try
            {
                this.ValidateExpired();
                var categoryName = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewAsync), this, name, parentPath);
                    this.ValidateAddNew(authentication, name, parentPath);
                    return new CategoryName(parentPath, name);
                });
                var taskID = GuidUtility.FromName(categoryName);
                var itemPath = await this.InvokeCategoryCreateAsync(authentication, categoryName);
                var result = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    var category = this.BaseAddNew(name, parentPath, authentication);
                    var items = EnumerableUtility.One(category).ToArray();
                    this.InvokeCategoriesCreatedEvent(authentication, items, taskID);
                    return category;
                });
                await this.Repository.UnlockAsync(itemPath);
                return result;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public Task<string> InvokeCategoryCreateAsync(Authentication authentication, string categoryPath)
        {
            var message = EventMessageBuilder.CreateTypeCategory(authentication, categoryPath);
            var itemPath = this.Context.GenerateCategoryPath(categoryPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.Lock(itemPath);
                    this.Repository.CreateTypeCategory(itemPath);
                    this.Repository.Commit(authentication, message);
                    return itemPath;
                }
                catch
                {
                    this.Repository.Revert();
                    this.Repository.Unlock(itemPath);
                    throw;
                }
            });
        }

        public Task InvokeCategoryRenameAsync(Authentication authentication, string categoryPath, string name, DataBaseSet dataBaseSet)
        {
            var newCategoryPath = new CategoryName(categoryPath) { Name = name, };
            var message = EventMessageBuilder.RenameTypeCategory(authentication, categoryPath, newCategoryPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.RenameTypeCategory(dataBaseSet, categoryPath, newCategoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    this.Repository.Unlock(dataBaseSet.ItemPaths);
                    throw;
                }
            });
        }

        public Task InvokeCategoryMoveAsync(Authentication authentication, string categoryPath, string parentPath, DataBaseSet dataBaseSet)
        {
            var categoryName = new CategoryName(categoryPath);
            var newCategoryPath = new CategoryName(parentPath, categoryName.Name);
            var message = EventMessageBuilder.MoveTypeCategory(authentication, categoryPath, categoryName.ParentPath, parentPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.MoveTypeCategory(dataBaseSet, categoryPath, newCategoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    this.Repository.Unlock(dataBaseSet.ItemPaths);
                    throw;
                }
            });
        }

        public Task InvokeCategoryDeleteAsync(Authentication authentication, string categoryPath, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.DeleteTableCategory(authentication, categoryPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.DeleteTypeCategory(dataBaseSet, categoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    this.Repository.Unlock(dataBaseSet.ItemPaths);
                    throw;
                }
            });
        }

        public void InvokeCategoriesCreatedEvent(Authentication authentication, TypeCategory[] categories, Guid taskID)
        {
            var args = categories.Select(item => (object)null).ToArray();
            var dataSet = CremaDataSet.Create(new SignatureDateProvider(authentication.ID));
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeCategoriesCreatedEvent), categories);
            var message = EventMessageBuilder.CreateTypeCategory(authentication, categories);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesCreated(new ItemsCreatedEventArgs<ITypeCategory>(authentication, categories, args, dataSet) { TaskID = taskID });
            this.Context.InvokeItemsCreatedEvent(authentication, categories, args, dataSet, taskID);
        }

        public void InvokeCategoriesRenamedEvent(Authentication authentication, TypeCategory[] categories, string[] oldNames, string[] oldPaths, CremaDataSet dataSet, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeCategoriesRenamedEvent), categories, oldNames, oldPaths);
            var message = EventMessageBuilder.RenameTypeCategory(authentication, categories, oldPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesRenamed(new ItemsRenamedEventArgs<ITypeCategory>(authentication, categories, oldNames, oldPaths, dataSet) { TaskID = taskID });
            this.Context.InvokeItemsRenamedEvent(authentication, categories, oldNames, oldPaths, dataSet, taskID);
        }

        public void InvokeCategoriesMovedEvent(Authentication authentication, TypeCategory[] categories, string[] oldPaths, string[] oldParentPaths, CremaDataSet dataSet, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeCategoriesMovedEvent), categories, oldPaths, oldParentPaths);
            var message = EventMessageBuilder.MoveTypeCategory(authentication, categories, oldPaths, oldParentPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesMoved(new ItemsMovedEventArgs<ITypeCategory>(authentication, categories, oldPaths, oldParentPaths, dataSet) { TaskID = taskID });
            this.Context.InvokeItemsMovedEvent(authentication, categories, oldPaths, oldParentPaths, dataSet, taskID);
        }

        public void InvokeCategoriesDeletedEvent(Authentication authentication, TypeCategory[] categories, string[] categoryPaths, CremaDataSet dataSet, Guid taskID)
        {
            var eventLog = EventLogBuilder.BuildMany(taskID, authentication, this, nameof(InvokeCategoriesDeletedEvent), categories, categoryPaths);
            var message = EventMessageBuilder.DeleteTypeCategory(authentication, categories);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesDeleted(new ItemsDeletedEventArgs<ITypeCategory>(authentication, categories, categoryPaths, dataSet) { TaskID = taskID });
            this.Context.InvokeItemsDeleteEvent(authentication, categories, categoryPaths, dataSet, taskID);
        }

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<ITypeCategory> CategoriesCreated
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesCreated += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<ITypeCategory> CategoriesRenamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesRenamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<ITypeCategory> CategoriesMoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesMoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<ITypeCategory> CategoriesDeleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesDeleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.categoriesDeleted -= value;
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.CollectionChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.CollectionChanged -= value;
            }
        }

        protected virtual void OnCategoriesCreated(ItemsCreatedEventArgs<ITypeCategory> e)
        {
            this.categoriesCreated?.Invoke(this, e);
        }

        protected virtual void OnCategoriesRenamed(ItemsRenamedEventArgs<ITypeCategory> e)
        {
            this.categoriesRenamed?.Invoke(this, e);
        }

        protected virtual void OnCategoriesMoved(ItemsMovedEventArgs<ITypeCategory> e)
        {
            this.categoriesMoved?.Invoke(this, e);
        }

        protected virtual void OnCategoriesDeleted(ItemsDeletedEventArgs<ITypeCategory> e)
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
            base.ValidateAddNew(name, parentPath, null);
            var parent = this[parentPath];
            parent.ValidateAccessType(authentication, AccessType.Master);

            var path = this.Context.GenerateCategoryPath(parentPath, name);
            if (Directory.Exists(path) == true)
                throw new InvalidOperationException(Resources.Exception_SameNamePathExists);
        }

        #region ITypeCategoryCollection

        bool ITypeCategoryCollection.Contains(string categoryPath)
        {
            return this.Contains(categoryPath);
        }

        ITypeCategory ITypeCategoryCollection.Root => this.Root;

        ITypeCategory ITypeCategoryCollection.this[string categoryPath] => this[categoryPath];

        #endregion

        #region IEnumerable

        IEnumerator<ITypeCategory> IEnumerable<ITypeCategory>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion
    }
}

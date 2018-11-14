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

using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml.Schema;
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
    class TableCategoryCollection : CategoryContainer<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        ITableCategoryCollection
    {
        private ItemsCreatedEventHandler<ITableCategory> categoriesCreated;
        private ItemsRenamedEventHandler<ITableCategory> categoriesRenamed;
        private ItemsMovedEventHandler<ITableCategory> categoriesMoved;
        private ItemsDeletedEventHandler<ITableCategory> categoriesDeleted;

        public TableCategoryCollection()
        {

        }

        public async Task<TableCategory> AddNewAsync(Authentication authentication, string name, string parentPath)
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
                var fullPaths = new string[] { PathUtility.Separator + CremaSchema.TableDirectory + categoryName };
                await this.Repository.LockAsync(authentication, this, nameof(AddNewAsync), fullPaths);
                using (var dataBaseSet = await DataBaseSet.CreateEmptyAsync(this.DataBase, fullPaths))
                {
                    await this.InvokeCategoryCreateAsync(authentication, categoryName);
                    var result = await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.Sign(authentication);
                        var category = this.BaseAddNew(name, parentPath, authentication);
                        var items = EnumerableUtility.One(category).ToArray();
                        this.InvokeCategoriesCreatedEvent(authentication, items);
                        return category;
                    });
                    return result;
                }
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

        public Task InvokeCategoryCreateAsync(Authentication authentication, string categoryPath)
        {
            var message = EventMessageBuilder.CreateTableCategory(authentication, categoryPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.CreateTableCategory(categoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeCategoryRenameAsync(Authentication authentication, string categoryPath, string name, DataBaseSet dataBaseSet)
        {
            var newCategoryPath = new CategoryName(categoryPath) { Name = name };
            var message = EventMessageBuilder.RenameTableCategory(authentication, categoryPath, newCategoryPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.RenameTableCategory(dataBaseSet, categoryPath, newCategoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeCategoryMoveAsync(Authentication authentication, string categoryPath, string parentPath, DataBaseSet dataBaseSet)
        {
            var categoryName = new CategoryName(categoryPath);
            var newCategoryPath = new CategoryName(parentPath, categoryName.Name);
            var message = EventMessageBuilder.MoveTableCategory(authentication, categoryPath, categoryName.ParentPath, parentPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.MoveTableCategory(dataBaseSet, categoryPath, newCategoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
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
                    this.Repository.DeleteTableCategory(dataBaseSet, categoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public void InvokeCategoriesCreatedEvent(Authentication authentication, TableCategory[] categories)
        {
            var args = categories.Select(item => (object)null).ToArray();
            var dataSet = CremaDataSet.Create(new SignatureDateProvider(authentication.ID));
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesCreatedEvent), categories);
            var message = EventMessageBuilder.CreateTableCategory(authentication, categories);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesCreated(new ItemsCreatedEventArgs<ITableCategory>(authentication, categories, args));
            this.Context.InvokeItemsCreatedEvent(authentication, categories, args, dataSet);
        }

        public void InvokeCategoriesRenamedEvent(Authentication authentication, TableCategory[] categories, string[] oldNames, string[] oldPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesRenamedEvent), categories, oldNames, oldPaths);
            var message = EventMessageBuilder.RenameTableCategory(authentication, categories, oldNames);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesRenamed(new ItemsRenamedEventArgs<ITableCategory>(authentication, categories, oldNames, oldPaths, dataSet));
            this.Context.InvokeItemsRenamedEvent(authentication, categories, oldNames, oldPaths, dataSet);
        }

        public void InvokeCategoriesMovedEvent(Authentication authentication, TableCategory[] categories, string[] oldPaths, string[] oldParentPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesMovedEvent), categories, oldPaths, oldParentPaths);
            var message = EventMessageBuilder.MoveTableCategory(authentication, categories, oldPaths, oldParentPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesMoved(new ItemsMovedEventArgs<ITableCategory>(authentication, categories, oldPaths, oldParentPaths, dataSet));
            this.Context.InvokeItemsMovedEvent(authentication, categories, oldPaths, oldParentPaths, dataSet);
        }

        public void InvokeCategoriesDeletedEvent(Authentication authentication, TableCategory[] categories, string[] categoryPaths)
        {
            var dataSet = CremaDataSet.Create(new SignatureDateProvider(authentication.ID));
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeCategoriesDeletedEvent), categoryPaths);
            var message = EventMessageBuilder.DeleteTableCategory(authentication, categories);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnCategoriesDeleted(new ItemsDeletedEventArgs<ITableCategory>(authentication, categories, categoryPaths, dataSet));
            this.Context.InvokeItemsDeletedEvent(authentication, categories, categoryPaths, dataSet);
        }

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<ITableCategory> CategoriesCreated
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

        public event ItemsRenamedEventHandler<ITableCategory> CategoriesRenamed
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

        public event ItemsMovedEventHandler<ITableCategory> CategoriesMoved
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

        public event ItemsDeletedEventHandler<ITableCategory> CategoriesDeleted
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

        protected virtual void OnCategoriesCreated(ItemsCreatedEventArgs<ITableCategory> e)
        {
            this.categoriesCreated?.Invoke(this, e);
        }

        protected virtual void OnCategoriesRenamed(ItemsRenamedEventArgs<ITableCategory> e)
        {
            this.categoriesRenamed?.Invoke(this, e);
        }

        protected virtual void OnCategoriesMoved(ItemsMovedEventArgs<ITableCategory> e)
        {
            this.categoriesMoved?.Invoke(this, e);
        }

        protected virtual void OnCategoriesDeleted(ItemsDeletedEventArgs<ITableCategory> e)
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

            var repositoryPath = new RepositoryPath(this.Context, parentPath + name);
            if (repositoryPath.IsExists == true)
                throw new InvalidOperationException(Resources.Exception_SameNamePathExists);
        }

        #region ITableCategoryCollection

        bool ITableCategoryCollection.Contains(string categoryPath)
        {
            return this.Contains(categoryPath);
        }

        ITableCategory ITableCategoryCollection.Root => this.Root;

        ITableCategory ITableCategoryCollection.this[string categoryPath] => this[categoryPath];

        #endregion

        #region IEnumerable

        IEnumerator<ITableCategory> IEnumerable<ITableCategory>.GetEnumerator()
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

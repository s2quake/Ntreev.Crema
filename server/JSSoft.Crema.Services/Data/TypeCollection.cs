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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class TypeCollection : ItemContainer<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext>, ITypeCollection
    {
        private ItemsCreatedEventHandler<IType> typesCreated;
        private ItemsRenamedEventHandler<IType> typesRenamed;
        private ItemsMovedEventHandler<IType> typesMoved;
        private ItemsDeletedEventHandler<IType> typesDeleted;
        private ItemsEventHandler<IType> typesStateChanged;
        private ItemsChangedEventHandler<IType> typesChanged;

        public TypeCollection()
        {

        }

        public Type AddNew(Authentication authentication, string name, string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            if (NameValidator.VerifyName(name) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, name), nameof(name));
            return this.BaseAddNew(name, categoryPath, authentication);
        }

        public async Task<Type[]> AddNewAsync(Authentication authentication, DataBaseSet dataBaseSet)
        {
            var dataTypes = dataBaseSet.TypesToCreate;
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in dataTypes)
                {
                    this.ValidateAddNew(item.Name, item.CategoryPath, authentication);
                }
            });
            var dataSet = dataBaseSet.DataSet;
            var itemPaths = dataTypes.Select(item => item.FullPath).ToArray();
            var typeList = new List<Type>(dataTypes.Length);
            var typePaths = dataTypes.Select(item => item.Path).ToArray();
            await this.InvokeTypeCreateAsync(authentication, typePaths, dataBaseSet);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in dataTypes)
                {
                    var type = this.BaseAddNew(item.Name, item.CategoryPath, authentication);
                    type.Initialize(item.TypeInfo);
                    typeList.Add(type);
                }
                this.InvokeTypesCreatedEvent(authentication, typeList.ToArray(), dataSet);
            });
            return typeList.ToArray();
        }

        public async Task<Type> CopyAsync(Authentication authentication, string typeName, string newTypeName, string categoryPath)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CopyAsync), typeName, newTypeName, categoryPath);
                    this.ValidateCopy(authentication, typeName, newTypeName);
                    return (this[typeName], this[typeName].Path);
                });
                var type = tuple.Item1;
                var path = tuple.Path;
                var taskID = GuidUtility.FromName(categoryPath + newTypeName);
                var itemName = new ItemName(tuple.Path);
                var targetName = new ItemName(categoryPath, newTypeName);
                var dataSet = await type.ReadDataForCopyAsync(authentication, categoryPath);
                var dataType = dataSet.Types[itemName.Name, itemName.CategoryPath];
                var newDataType = dataType.Copy(targetName);
                using var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, DataBaseSetOptions.AllowTypeCreation);
                var typePaths = new string[] { categoryPath + newTypeName };
                await this.InvokeTypeCreateAsync(authentication, typePaths, dataBaseSet);
                var result = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    var newType = this.BaseAddNew(newTypeName, categoryPath, authentication);
                    newType.Initialize(newDataType.TypeInfo);
                    this.InvokeTypesCreatedEvent(authentication, new Type[] { newType }, dataSet);
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                    return newType;
                });
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

        public Task InvokeTypeCreateAsync(Authentication authentication, string[] typePaths, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.CreateType(authentication, typePaths);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.CreateType(dataBaseSet, typePaths);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTypeRenameAsync(Authentication authentication, TypeInfo typeInfo, string newName, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.RenameType(authentication, typeInfo.Name, newName);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.RenameType(dataBaseSet, typeInfo.Path, newName);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTypeMoveAsync(Authentication authentication, TypeInfo typeInfo, string newCategoryPath, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.MoveType(authentication, typeInfo.Name, newCategoryPath, typeInfo.CategoryPath);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.MoveType(dataBaseSet, typeInfo.Path, newCategoryPath);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTypeDeleteAsync(Authentication authentication, TypeInfo typeInfo, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.DeleteType(authentication, typeInfo.Name);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.DeleteType(dataBaseSet, typeInfo.Path);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public Task InvokeTypeEndTemplateEditAsync(Authentication authentication, string typeName, DataBaseSet dataBaseSet)
        {
            var message = EventMessageBuilder.ChangeTypeTemplate(authentication, typeName);
            return this.Repository.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Repository.ModifyType(dataBaseSet);
                    this.Repository.Commit(authentication, message);
                }
                catch
                {
                    this.Repository.Revert();
                    throw;
                }
            });
        }

        public void InvokeTypesCreatedEvent(Authentication authentication, Type[] types, CremaDataSet dataSet)
        {
            var args = types.Select(item => (object)item.TypeInfo).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTypesCreatedEvent), types);
            var message = EventMessageBuilder.CreateType(authentication, types);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTypesCreated(new ItemsCreatedEventArgs<IType>(authentication, types, args, dataSet));
            this.Context.InvokeItemsCreatedEvent(authentication, types, args, dataSet);
        }

        public void InvokeTypesRenamedEvent(Authentication authentication, Type[] types, string[] oldNames, string[] oldPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTypesRenamedEvent), types, oldNames, oldPaths);
            var message = EventMessageBuilder.RenameType(authentication, types, oldNames);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTypesRenamed(new ItemsRenamedEventArgs<IType>(authentication, types, oldNames, oldPaths));
            this.Context.InvokeItemsRenamedEvent(authentication, types, oldNames, oldPaths, dataSet);
        }

        public void InvokeTypesMovedEvent(Authentication authentication, Type[] types, string[] oldPaths, string[] oldCategoryPaths, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTypesMovedEvent), types, oldPaths, oldCategoryPaths);
            var message = EventMessageBuilder.MoveType(authentication, types, oldCategoryPaths);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTypesMoved(new ItemsMovedEventArgs<IType>(authentication, types, oldPaths, oldCategoryPaths));
            this.Context.InvokeItemsMovedEvent(authentication, types, oldPaths, oldCategoryPaths, dataSet);
        }

        public void InvokeTypesDeletedEvent(Authentication authentication, Type[] types, string[] oldPaths)
        {
            var dataSet = CremaDataSet.Create(new SignatureDateProvider(authentication.ID));
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTypesDeletedEvent), oldPaths);
            var message = EventMessageBuilder.DeleteType(authentication, types);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTypesDeleted(new ItemsDeletedEventArgs<IType>(authentication, types, oldPaths));
            this.Context.InvokeItemsDeleteEvent(authentication, types, oldPaths, dataSet);
        }

        public void InvokeTypesChangedEvent(Authentication authentication, Type[] types, CremaDataSet dataSet)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeTypesChangedEvent), types);
            var message = EventMessageBuilder.ChangeTypeTemplate(authentication, types);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnTypesChanged(new ItemsChangedEventArgs<IType>(authentication, types, dataSet, DomainItemType.TypeTemplate));
            this.Context.InvokeItemsChangedEvent(authentication, types, dataSet);
        }

        public void InvokeTypesStateChangedEvent(Authentication authentication, Type[] types)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeTypesStateChangedEvent), types);
            this.OnTypesStateChanged(new ItemsEventArgs<IType>(authentication, types));
        }

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<IType> TypesCreated
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.typesCreated += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.typesCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<IType> TypesRenamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.typesRenamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.typesRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<IType> TypesMoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.typesMoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.typesMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<IType> TypesDeleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.typesDeleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.typesDeleted -= value;
            }
        }

        public event ItemsEventHandler<IType> TypesStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.typesStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.typesStateChanged -= value;
            }
        }

        public event ItemsChangedEventHandler<IType> TypesChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.typesChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.typesChanged -= value;
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

        protected override void ValidateAddNew(string name, string categoryPath, object validation)
        {
            base.ValidateAddNew(name, categoryPath, validation);

            if (NameValidator.VerifyName(name) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, name), nameof(name));

            if (validation is Authentication authentication)
            {
                var category = this.GetCategory(categoryPath);
                category.ValidateAccessType(authentication, AccessType.Master);
            }
        }

        protected virtual void OnTypesCreated(ItemsCreatedEventArgs<IType> e)
        {
            this.typesCreated?.Invoke(this, e);
        }

        protected virtual void OnTypesRenamed(ItemsRenamedEventArgs<IType> e)
        {
            this.typesRenamed?.Invoke(this, e);
        }

        protected virtual void OnTypesMoved(ItemsMovedEventArgs<IType> e)
        {
            this.typesMoved?.Invoke(this, e);
        }

        protected virtual void OnTypesDeleted(ItemsDeletedEventArgs<IType> e)
        {
            this.typesDeleted?.Invoke(this, e);
        }

        protected virtual void OnTypesStateChanged(ItemsEventArgs<IType> e)
        {
            this.typesStateChanged?.Invoke(this, e);
        }

        protected virtual void OnTypesChanged(ItemsChangedEventArgs<IType> e)
        {
            this.typesChanged?.Invoke(this, e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher?.VerifyAccess();
            base.OnCollectionChanged(e);
        }

        private void ValidateCopy(Authentication authentication, string typeName, string newTypeName)
        {
            if (this.Contains(typeName) == false)
                throw new TypeNotFoundException(typeName);

            if (this.Contains(newTypeName) == true)
                throw new ArgumentException(Resources.Exception_SameTypeNameExist, nameof(newTypeName));
        }

        #region ITypeCollection

        bool ITypeCollection.Contains(string typeName)
        {
            return this.Contains(typeName);
        }

        IType ITypeCollection.this[string typeName] => this[typeName];

        #endregion

        #region IEnumerable

        IEnumerator<IType> IEnumerable<IType>.GetEnumerator()
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

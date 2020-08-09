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
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class Type : TypeBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext>,
        IType, ITypeItem, IInfoProvider, IStateProvider
    {
        public Type()
        {
            this.Template = new TypeTemplate(this);
        }

        public AccessType GetAccessType(Authentication authentication)
        {
            this.ValidateExpired();
            return base.GetAccessType(authentication);
        }

        public async Task<Guid> SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTypeItemSetPublicAsync(authentication, tuple.path, tuple.accessInfo);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsSetPublicEvent(authentication, new ITypeItem[] { this });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetPrivateAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    base.ValidateSetPrivate(authentication);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTypeItemSetPrivateAsync(authentication, tuple.path, tuple.accessInfo);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsSetPrivateEvent(authentication, new ITypeItem[] { this });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTypeItemAddAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsAddAccessMemberEvent(authentication, new ITypeItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTypeItemSetAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID, accessType);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsSetAccessMemberEvent(authentication, new ITypeItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    base.ValidateRemoveAccessMember(authentication, memberID);
                    var path = base.Path;
                    var accessInfo = base.AccessInfo;
                    return (path, accessInfo);
                });
                var taskID = Guid.NewGuid();
                var result = await this.Context.InvokeTypeItemRemoveAccessMemberAsync(authentication, tuple.path, tuple.accessInfo, memberID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    base.AccessInfo = result;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsRemoveAccessMemberEvent(authentication, new ITypeItem[] { this }, new string[] { memberID });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                });
                return taskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LockAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this, comment);
                    base.ValidateLock(authentication);
                    var taskID = Guid.NewGuid();
                    var lockInfo = new LockInfo()
                    {
                        Path = this.Path,
                        ParentPath = string.Empty,
                        SignatureDate = new SignatureDate(authentication.ID),
                        Comment = comment
                    };
                    base.LockInfo = lockInfo;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsLockedEvent(authentication, new ITypeItem[] { this }, new string[] { comment });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> UnlockAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    base.ValidateUnlock(authentication);
                    var taskID = Guid.NewGuid();
                    var lockInfo = LockInfo.Empty;
                    base.LockInfo = lockInfo;
                    this.CremaHost.Sign(authentication);
                    this.Context.InvokeItemsUnlockedEvent(authentication, new ITypeItem[] { this });
                    this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                    return taskID;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RenameAsync(Authentication authentication, string name)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    base.ValidateRename(authentication, name);
                    var items = new Type[] { this };
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var typeInfo = base.TypeInfo;
                    var targetName = new ItemName(base.Path) { Name = name };
                    return (items, oldNames, oldPaths, typeInfo, targetName);
                });
                var taskID = Guid.NewGuid();
                var dataSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                using (var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet))
                {
                    await this.Container.InvokeTypeRenameAsync(authentication, tuple.typeInfo, name, dataBaseSet);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.Sign(authentication);
                        base.Rename(authentication, name);
                        this.Container.InvokeTypesRenamedEvent(authentication, tuple.items, tuple.oldNames, tuple.oldPaths, dataSet);
                        this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                    });
                    return taskID;
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> MoveAsync(Authentication authentication, string categoryPath)
        {
            try
            {
                this.ValidateExpired();
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Move), this, categoryPath);
                    base.ValidateMove(authentication, categoryPath);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldCategoryPaths = items.Select(item => item.Category.Path).ToArray();
                    var typeInfo = base.TypeInfo;
                    var targetName = new ItemName(categoryPath, base.Name);
                    return (items, oldPaths, oldCategoryPaths, typeInfo, targetName);
                });
                var taskID = Guid.NewGuid();
                var dataSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                using (var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet))
                {
                    await this.Container.InvokeTypeMoveAsync(authentication, tuple.typeInfo, categoryPath, dataBaseSet);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.CremaHost.Sign(authentication);
                        base.Move(authentication, categoryPath);
                        this.Container.InvokeTypesMovedEvent(authentication, tuple.items, tuple.oldPaths, tuple.oldCategoryPaths, dataSet);
                        this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
                    });
                    return taskID;
                }
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
                var dataBase = this.DataBase;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Delete), this);
                    base.ValidateDelete(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var typeInfo = base.TypeInfo;
                    var targetName = new ItemName(base.Path);
                    return (items, oldPaths, typeInfo, targetName);
                });
                var taskID = Guid.NewGuid();
                var dataSet = await this.ReadDataForPathAsync(authentication, tuple.targetName);
                using (var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet))
                {
                    await this.Container.InvokeTypeDeleteAsync(authentication, tuple.typeInfo, dataBaseSet);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        base.Delete(authentication);
                        cremaHost.Sign(authentication);
                        container.InvokeTypesDeletedEvent(authentication, tuple.items, tuple.oldPaths);
                        dataBase.InvokeTaskCompletedEvent(authentication, taskID);
                    });
                    return taskID;
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<CremaDataSet> GetDataSetAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, revision);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    return this.Repository.GetTypeData(this.Serializer, this.Path, revision);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    return this.Context.GetTypeLog(this.Path, revision);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<FindResultInfo[]> FindAsync(Authentication authentication, string text, FindOptions options)
        {
            try
            {
                this.ValidateExpired();
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(FindAsync), this, text, options);
                    this.ValidateAccessType(authentication, AccessType.Guest);
                    this.CremaHost.Sign(authentication);
                    if (this.GetService(typeof(DataFindService)) is DataFindService service)
                    {
                        return service.Dispatcher.Invoke(() => service.FindFromType(this.DataBase.ID, new string[] { base.Path }, text, options));
                    }
                    throw new NotImplementedException();
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<CremaDataSet> ReadDataForCopyAsync(Authentication authentication, string categoryPath)
        {
            var fullPaths = await this.Dispatcher.InvokeAsync(() =>
            {
                var itemPaths = new string[]
                {
                    DataBase.TypePathPrefix + categoryPath,
                    this.FullPath
                };
                return itemPaths;
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(authentication, this, nameof(ReadDataForCopyAsync), fullPaths);
                return this.Repository.ReadDataSet(authentication, fullPaths);
            });
        }

        public async Task<CremaDataSet> ReadDataForTypeTemplateAsync(Authentication authentication)
        {
            var fullPaths = await this.Dispatcher.InvokeAsync(() =>
            {
                var tables = this.GetTables();
                var types = tables.SelectMany(item => item.GetTypes()).Concat(new Type[] { this }).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.FullPath).ToArray();
                var tableItemPaths = tables.Select(item => item.FullPath).ToArray();
                var itemPaths = typeItemPaths.Concat(tableItemPaths).ToArray();
                return itemPaths;
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(authentication, this, nameof(MoveAsync), fullPaths);
                return this.Repository.ReadDataSet(authentication, fullPaths);
            });
        }

        public async Task<CremaDataSet> ReadDataForPathAsync(Authentication authentication, ItemName targetName)
        {
            var fullPaths = await this.Dispatcher.InvokeAsync(() =>
            {
                var targetItemPaths = new string[]
                {
                    DataBase.TypePathPrefix + targetName.CategoryPath,
                    DataBase.TypePathPrefix + targetName,
                };
                var tables = this.GetTables();
                var types = tables.SelectMany(item => item.GetTypes()).Concat(new Type[] { this }).Distinct().ToArray();
                var typeItemPaths = types.Select(item => item.FullPath).ToArray();
                var tableItemPaths = tables.Select(item => item.FullPath).ToArray();
                var itemPaths = typeItemPaths.Concat(tableItemPaths).Concat(targetItemPaths).Distinct().ToArray();
                return itemPaths;
            });
            return await this.Repository.Dispatcher.InvokeAsync(() =>
            {
                this.Repository.Lock(authentication, this, nameof(ReadDataForPathAsync), fullPaths);
                return this.Repository.ReadDataSet(authentication, fullPaths);
            });
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public IEnumerable<Table> GetTables()
        {
            var tables = this.GetService(typeof(TableCollection)) as TableCollection;
            var query = from Table item in tables
                        from column in item.TableInfo.Columns
                        where this.Path == column.DataType
                        select item;
            return query.Distinct();
        }

        //public string ItemPath => this.Context.GenerateTypePath(this.Category.BasePath, base.Name);

        public IEnumerable<Table> ReferencedTables
        {
            get
            {
                var tables = this.GetService(typeof(TableCollection)) as TableCollection;
                foreach (var item in tables)
                {
                    if (item.IsTypeUsed(this.Path))
                    {
                        yield return item;
                        if (item.Parent != null)
                        {
                            yield return item.Parent;
                            foreach (var i in item.Parent.Childs)
                            {
                                yield return i;
                            }
                        }
                    }
                }
            }
        }

        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public void SetAccessInfo(AccessInfo accessInfo)
        //{
        //    accessInfo.Path = this.Path;
        //    base.AccessInfo = accessInfo;
        //}

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetTypeState(TypeState typeState)
        {
            base.TypeState = typeState;
        }

        public void ValidateLockInternal(Authentication authentication)
        {
            base.ValidateLock(authentication);
        }

        public void LockInternal(Authentication authentication, string comment)
        {
            base.Lock(authentication, comment);
        }

        public void UnlockInternal(Authentication authentication)
        {
            base.Unlock(authentication);
        }

        public TypeTemplate Template { get; }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public new string Name => base.Name;

        public new string Path => base.Path;

        public new bool IsLocked => base.IsLocked;

        public new bool IsPrivate => base.IsPrivate;

        public new AccessInfo AccessInfo => base.AccessInfo;

        public new LockInfo LockInfo => base.LockInfo;

        public new TypeInfo TypeInfo => base.TypeInfo;

        //public new TypeState TypeState => base.TypeState;

        public new TagInfo Tags => base.Tags;

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

        public new event EventHandler LockChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.LockChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.LockChanged -= value;
            }
        }

        public new event EventHandler AccessChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.AccessChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.AccessChanged -= value;
            }
        }

        public new event EventHandler TypeInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.TypeInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.TypeInfoChanged -= value;
            }
        }

        public new event EventHandler TypeStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.TypeStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.TypeStateChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateLock(IAuthentication authentication, object target)
        {
            base.OnValidateLock(authentication, target);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateUnlock(IAuthentication authentication, object target)
        {
            base.OnValidateUnlock(authentication, target);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateSetPublic(IAuthentication authentication, object target)
        {
            base.OnValidateSetPublic(authentication, target);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateSetPrivate(IAuthentication authentication, object target)
        {
            base.OnValidateSetPrivate(authentication, target);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateAddAccessMember(IAuthentication authentication, object target, string memberID, AccessType accessType)
        {
            base.OnValidateAddAccessMember(authentication, target, memberID, accessType);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateRemoveAccessMember(IAuthentication authentication, object target)
        {
            base.OnValidateRemoveAccessMember(authentication, target);
            this.ValidateIsNotBeingEdited();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateRename(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateRename(authentication, target, oldPath, newPath);
            this.ValidateIsNotBeingEdited();
            this.ValidateUsingTables(authentication);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateMove(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateMove(authentication, target, oldPath, newPath);
            this.ValidateIsNotBeingEdited();
            this.ValidateUsingTables(authentication);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateDelete(IAuthentication authentication, object target)
        {
            base.OnValidateDelete(authentication, target);
            this.ValidateIsNotBeingEdited();
            this.ValidateUsingTables(authentication);

            var tables = this.GetService(typeof(TableCollection)) as TableCollection;
            var query = from Table item in tables
                        where item.IsTypeUsed(base.Path)
                        select item;
            if (query.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteTypeWithUsedTables);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            accessInfo.Path = this.Path;
            base.AccessInfo = accessInfo;
        }

        public void ValidateUsingTables(IAuthentication authentication)
        {
            var tables = this.GetService(typeof(TableCollection)) as TableCollection;

            var query = from Table item in tables
                        where item.IsTypeUsed(base.Path)
                        select item;

            foreach (var item in query.Distinct())
            {
                this.ValidateUsingTable(authentication, item);
            }
        }

        public void ValidateIsNotBeingEdited()
        {
            if (this.TypeState == TypeState.IsBeingEdited)
                throw new InvalidOperationException(string.Format(Resources.Exception_TypeIsBeingEdited_Format, base.Name));
            if (this.Template.Domain != null)
                throw new InvalidOperationException(string.Format(Resources.Exception_TypeIsBeingEdited_Format, base.Name));
        }

        public void ValidateIsBeingEdited()
        {
            if (this.TypeState != TypeState.IsBeingEdited)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
        }

        private void ValidateUsingTable(IAuthentication authentication, Table table)
        {
            if (table.TableState == TableState.IsBeingEdited)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingEdited_Format, table.Name));
            if (table.TableState == TableState.IsBeingSetup)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingSetup_Format, table.Name));
            if (table.VerifyAccessType(authentication, AccessType.Master) == false)
                throw new PermissionException();
        }

        #region IType

        Task IType.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task IType.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task IType.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        async Task<IType> IType.CopyAsync(Authentication authentication, string newTypeName, string categoryPath)
        {
            return await this.Container.CopyAsync(authentication, base.Name, newTypeName, categoryPath);
        }

        ITypeCategory IType.Category => this.Category;

        ITypeTemplate IType.Template => this.Template;

        #endregion

        #region ITypeItem

        Task ITypeItem.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task ITypeItem.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task ITypeItem.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        ITypeItem ITypeItem.Parent => this.Category;

        IEnumerable<ITypeItem> ITypeItem.Childs => Enumerable.Empty<ITypeItem>();

        #endregion

        #region IAccessible

        Task IAccessible.SetPublicAsync(Authentication authentication)
        {
            return this.SetPublicAsync(authentication);
        }

        Task IAccessible.SetPrivateAsync(Authentication authentication)
        {
            return this.SetPrivateAsync(authentication);
        }

        Task IAccessible.AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            return this.AddAccessMemberAsync(authentication, memberID, accessType);
        }

        Task IAccessible.SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            return this.SetAccessMemberAsync(authentication, memberID, accessType);
        }

        Task IAccessible.RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            return this.RemoveAccessMemberAsync(authentication, memberID);
        }

        #endregion

        #region ILockable

        Task ILockable.LockAsync(Authentication authentication, string comment)
        {
            return this.LockAsync(authentication, comment);
        }

        Task ILockable.UnlockAsync(Authentication authentication)
        {
            return this.UnlockAsync(authentication);
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => this.TypeInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.TypeState;

        #endregion
    }
}

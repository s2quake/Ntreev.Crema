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
using Ntreev.Library.Linq;
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

        public async Task SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                    var result = await this.Context.InvokeTypeItemSetPublicAsync(authentication, this);
                    this.CremaHost.Sign(authentication, result);
                    base.SetPublic(authentication);
                    this.Context.InvokeItemsSetPublicEvent(authentication, new ITypeItem[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetPrivateAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    base.ValidateSetPrivate(authentication);
                    var result = await this.Context.InvokeTypeItemSetPrivateAsync(authentication, this, AccessInfo.Empty);
                    this.CremaHost.Sign(authentication, result.SignatureDate);
                    base.SetPrivate(authentication);
                    this.Context.InvokeItemsSetPrivateEvent(authentication, new ITypeItem[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                    var signature = await this.Context.InvokeTypeItemAddAccessMemberAsync(authentication, this, this.AccessInfo, memberID, accessType);
                    this.CremaHost.Sign(authentication, signature.SignatureDate);
                    base.AddAccessMember(authentication, memberID, accessType);
                    this.Context.InvokeItemsAddAccessMemberEvent(authentication, new ITypeItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                    var result = await this.Context.InvokeTypeItemSetAccessMemberAsync(authentication, this, this.AccessInfo, memberID, accessType);
                    this.CremaHost.Sign(authentication, result.SignatureDate);
                    base.SetAccessMember(authentication, memberID, accessType);
                    this.Context.InvokeItemsSetAccessMemberEvent(authentication, new ITypeItem[] { this }, new string[] { memberID }, new AccessType[] { accessType });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    base.ValidateRemoveAccessMember(authentication, memberID);
                    var result = await this.Context.InvokeTypeItemRemoveAccessMemberAsync(authentication, this, this.AccessInfo, memberID);
                    this.CremaHost.Sign(authentication, result.SignatureDate);
                    base.RemoveAccessMember(authentication, memberID);
                    this.Context.InvokeItemsRemoveAccessMemberEvent(authentication, new ITypeItem[] { this }, new string[] { memberID });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task LockAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this, comment);
                    base.ValidateLock(authentication);
                    this.CremaHost.Sign(authentication);
                    base.Lock(authentication, comment);
                    this.Context.InvokeItemsLockedEvent(authentication, new ITypeItem[] { this }, new string[] { comment });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task UnlockAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    base.ValidateUnlock(authentication);
                    this.CremaHost.Sign(authentication);
                    base.Unlock(authentication);
                    this.Context.InvokeItemsUnlockedEvent(authentication, new ITypeItem[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RenameAsync(Authentication authentication, string name)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    base.ValidateRename(authentication, name);
                    var items = new Type[] { this };
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var dataSet = await this.ReadDataForPathAsync(authentication);
                    var dataBaseSet = new DataBaseSet(this.DataBase, dataSet, false);
                    var result = await this.Container.InvokeTypeRenameAsync(authentication, base.TypeInfo, name, dataBaseSet);
                    this.CremaHost.Sign(authentication, result);
                    base.Rename(authentication, name);
                    this.Container.InvokeTypesRenamedEvent(authentication, items, oldNames, oldPaths, dataSet);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task MoveAsync(Authentication authentication, string categoryPath)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Move), this, categoryPath);
                    base.ValidateMove(authentication, categoryPath);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldCategoryPaths = items.Select(item => item.Category.Path).ToArray();
                    var dataSet = await this.ReadDataForPathAsync(authentication);
                    var dataBaseSet = new DataBaseSet(this.DataBase, dataSet, false);
                    var result = await this.Container.InvokeTypeMoveAsync(authentication, base.TypeInfo, categoryPath, dataBaseSet);
                    this.CremaHost.Sign(authentication, result);
                    base.Move(authentication, categoryPath);
                    this.Container.InvokeTypesMovedEvent(authentication, items, oldPaths, oldCategoryPaths, dataSet);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task DeleteAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Delete), this);
                    base.ValidateDelete(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var container = this.Container;
                    var dataSet = await this.ReadDataForPathAsync(authentication);
                    var dataBaseSet = new DataBaseSet(this.DataBase, dataSet, false);
                    var result = await container.InvokeTypeDeleteAsync(authentication, base.TypeInfo, dataBaseSet);
                    this.CremaHost.Sign(authentication, result);
                    base.Delete(authentication);
                    container.InvokeTypesDeletedEvent(authentication, items, oldPaths);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<Type> CopyAsync(Authentication authentication, string newTypeName, string categoryPath)
        {
            return this.Container.CopyAsync(authentication, base.Name, newTypeName, categoryPath);
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
                    return this.Repository.GetTypeData(this.Serializer, this.ItemPath, revision);
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
                    return this.Context.GetTypeLog(this.ItemPath, revision);
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

        public Task<CremaDataSet> ReadDataAsync(Authentication authentication)
        {
            var props = new CremaDataSetSerializerSettings(new string[] { this.ItemPath }, null);
            return this.Repository.Dispatcher.InvokeAsync(() => this.Serializer.Deserialize(this.ItemPath, typeof(CremaDataSet), props) as CremaDataSet);
        }

        public Task<CremaDataSet> ReadAllDataAsync(Authentication authentication)
        {
            var tables = this.ReferencedTables.ToArray();
            var typeFiles = tables.SelectMany(item => item.GetTypes())
                                  .Concat(EnumerableUtility.One(this))
                                  .Select(item => item.ItemPath)
                                  .Distinct()
                                  .ToArray();
            var tableFiles = tables.Select(item => item.ItemPath)
                                   .Distinct()
                                   .ToArray();

            var props = new CremaDataSetSerializerSettings(authentication, typeFiles, tableFiles);
            return this.Repository.Dispatcher.InvokeAsync(() => this.Serializer.Deserialize(this.ItemPath, typeof(CremaDataSet), props) as CremaDataSet);
        }

        public Task<CremaDataSet> ReadDataForPathAsync(Authentication authentication)
        {
            var tables = this.ReferencedTables.ToArray();
            var typeFiles = tables.SelectMany(item => item.GetTypes())
                                  .Concat(EnumerableUtility.One(this))
                                  .Select(item => item.ItemPath)
                                  .Distinct()
                                  .ToArray();
            var tableFiles = tables.Select(item => item.ItemPath)
                                   .Distinct()
                                   .ToArray();

            var props = new CremaDataSetSerializerSettings(authentication, typeFiles, tableFiles);
            return this.Repository.Dispatcher.InvokeAsync(() => this.Serializer.Deserialize(this.ItemPath, typeof(CremaDataSet), props) as CremaDataSet);
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public string ItemPath => this.Context.GenerateTypePath(this.Category.BasePath, base.Name);

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            accessInfo.Path = this.Path;
            base.AccessInfo = accessInfo;
        }

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

        public new TypeState TypeState => base.TypeState;

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
            if (this.IsBeingEdited == true)
                throw new InvalidOperationException(string.Format(Resources.Exception_TypeIsBeingEdited_Format, base.Name));
            if (this.Template.Domain != null)
                throw new InvalidOperationException(string.Format(Resources.Exception_TypeIsBeingEdited_Format, base.Name));
        }

        public void ValidateIsBeingEdited()
        {
            if (this.IsBeingEdited == false)
                throw new InvalidOperationException(Resources.Exception_TypeIsNotBeingEdited);
        }

        private void ValidateUsingTable(IAuthentication authentication, Table table)
        {
            if (table.TableState.HasFlag(TableState.IsBeingEdited) == true)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingEdited_Format, table.Name));
            if (table.TableState.HasFlag(TableState.IsBeingSetup) == true)
                throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingSetup_Format, table.Name));
            if (table.VerifyAccessType(authentication, AccessType.Master) == false)
                throw new PermissionException();
        }

        #region IType

        async Task<IType> IType.CopyAsync(Authentication authentication, string newTypeName, string categoryPath)
        {
            return await this.CopyAsync(authentication, newTypeName, categoryPath);
        }

        ITypeCategory IType.Category => this.Category;

        ITypeTemplate IType.Template => this.Template;

        #endregion

        #region ITypeItem

        ITypeItem ITypeItem.Parent => this.Category;

        IEnumerable<ITypeItem> ITypeItem.Childs => Enumerable.Empty<ITypeItem>();

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

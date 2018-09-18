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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TypeCategory : TypeCategoryBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext>,
        ITypeCategory, ITypeItem
    {
        private readonly List<NewTypeTemplate> templateList = new List<NewTypeTemplate>();

        public TypeCategory()
        {

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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    base.ValidateSetPublic(authentication);
                    var signatureDate = await this.Context.InvokeTypeItemSetPublicAsync(authentication, this);
                    this.CremaHost.Sign(authentication, signatureDate);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    base.ValidateSetPrivate(authentication);
                    var accessInfo = await this.Context.InvokeTypeItemSetPrivateAsync(authentication, this, AccessInfo.Empty);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    base.ValidateAddAccessMember(authentication, memberID, accessType);
                    var accessInfo = await this.Context.InvokeTypeItemAddAccessMemberAsync(authentication, this, this.AccessInfo, memberID, accessType);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    base.ValidateSetAccessMember(authentication, memberID, accessType);
                    var accessInfo = await this.Context.InvokeTypeItemSetAccessMemberAsync(authentication, this, this.AccessInfo, memberID, accessType);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    base.ValidateRemoveAccessMember(authentication, memberID);
                    var accessInfo = await this.Context.InvokeTypeItemRemoveAccessMemberAsync(authentication, this, this.AccessInfo, memberID);
                    this.CremaHost.Sign(authentication, accessInfo.SignatureDate);
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
                    this.Context.InvokeItemsLockedEvent(authentication, new ITypeItem[] { this, }, new string[] { comment });
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
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var dataSet = await this.ReadAllDataAsync(authentication, true);
                    var dataBaseSet = new DataBaseSet(this.DataBase, dataSet);
                    var signatureDate = await this.Container.InvokeCategoryRenameAsync(authentication, this.Path, name, dataBaseSet);
                    this.CremaHost.Sign(authentication, signatureDate);
                    base.Rename(authentication, name);
                    this.Container.InvokeCategoriesRenamedEvent(authentication, items, oldNames, oldPaths, dataSet);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task MoveAsync(Authentication authentication, string parentPath)
        {
            try
            {
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(MoveAsync), this, parentPath);
                    base.ValidateMove(authentication, parentPath);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldParentPaths = items.Select(item => item.Parent.Path).ToArray();
                    var dataSet = await this.ReadAllDataAsync(authentication, true);
                    var dataBaseSet = new DataBaseSet(this.DataBase, dataSet);
                    var signatureDate = await this.Container.InvokeCategoryMoveAsync(authentication, this.Path, parentPath, dataBaseSet);
                    this.CremaHost.Sign(authentication, signatureDate);
                    base.Move(authentication, parentPath);
                    this.Container.InvokeCategoriesMovedEvent(authentication, items, oldPaths, oldParentPaths, dataSet);
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
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    base.ValidateDelete(authentication);
                    this.CremaHost.Sign(authentication);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var container = this.Container;
                    var dataSet = await this.ReadAllDataAsync(authentication, true);
                    var dataBaseSet = new DataBaseSet(this.DataBase, dataSet);
                    var signatureDate = await container.InvokeCategoryDeleteAsync(authentication, this.Path, dataBaseSet);
                    this.CremaHost.Sign(authentication, signatureDate);
                    base.Delete(authentication);
                    container.InvokeCategoriesDeletedEvent(authentication, items, oldPaths, dataSet);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task<TypeCategory> AddNewCategoryAsync(Authentication authentication, string name)
        {
            return this.Container.AddNewAsync(authentication, name, base.Path);
        }

        public async Task<NewTypeTemplate> NewTypeAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewTypeAsync), this);
                    var template = new NewTypeTemplate(this);
                    await template.BeginEditAsync(authentication);
                    return template;
                });
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
                    return this.Repository.GetTypeCategoryData(this.Serializer, this.ItemPath, revision);
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
                    return this.Context.GetCategoryLog(this.ItemPath, revision);
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

        /// <summary>
        /// 폴더내에 모든 타입과 타입이 사용하고 있는 테이블, 상속된 테이블을 읽어들입니다.
        /// </summary>
        public Task<CremaDataSet> ReadAllDataAsync(Authentication authentication, bool recursive)
        {
            var types = CollectTypes();
            var tables = CollectTables();
            var typePaths = types.Select(item => item.ItemPath).ToArray();
            var tablePaths = tables.Select(item => item.ItemPath)
                                   .Distinct()
                                   .ToArray();

            var props = new CremaDataSetSerializerSettings(authentication, typePaths, tablePaths);
            return this.Repository.Dispatcher.InvokeAsync(() => this.Serializer.Deserialize(this.ItemPath, typeof(CremaDataSet), props) as CremaDataSet);

            Type[] CollectTypes()
            {
                if (recursive == true)
                {
                    return EnumerableUtility.Descendants<IItem, Type>(this as IItem, item => item.Childs).ToArray();
                }
                return this.Types.ToArray();
            }

            Table[] CollectTables()
            {
                var tableContext = this.GetService(typeof(TableContext)) as TableContext;
                var query = from table in EnumerableUtility.Descendants<IItem, Table>(tableContext.Root as IItem, item => item.Childs)
                            from type in types
                            where table.IsTypeUsed(type.Path)
                            select table;

                return query.ToArray();
            }
        }

        /// <summary>
        /// 폴더내의 타입들을 읽어들입니다.
        /// </summary>
        public Task<CremaDataSet> ReadDataAsync(Authentication authentication)
        {
            return this.ReadDataAsync(authentication, false);
        }

        public Task<CremaDataSet> ReadDataAsync(Authentication authentication, bool recursive)
        {
            var types = CollectTypes();
            var typePaths = types.Select(item => item.ItemPath).ToArray();
            var info = new CremaDataSetSerializerSettings(authentication, typePaths, null);
            return this.Repository.Dispatcher.InvokeAsync(() => this.Serializer.Deserialize(this.ItemPath, typeof(CremaDataSet), info) as CremaDataSet);

            Type[] CollectTypes()
            {
                if (recursive == true)
                {
                    return EnumerableUtility.Descendants<IItem, Type>(this as IItem, item => item.Childs).ToArray();
                }
                return this.Types.ToArray();
            }
        }

        public object GetService(System.Type serviceType)
        {
            return this.DataBase.GetService(serviceType);
        }

        public string ItemPath => this.Context.GenerateCategoryPath(base.Path);

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IObjectSerializer Serializer => this.DataBase.Serializer;

        public DataBaseRepositoryHost Repository => this.DataBase.Repository;

        public string BasePath => base.Path;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateRename(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateRename(authentication, target, oldPath, newPath);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotRenameOnCreateType);
            this.ValidateUsingTables(authentication);
            var categoryName = new CategoryName(Regex.Replace(this.Path, $"^{oldPath}", newPath));
            this.Context.ValidateCategoryPath(categoryName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateMove(IAuthentication authentication, object target, string oldPath, string newPath)
        {
            base.OnValidateMove(authentication, target, oldPath, newPath);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotMoveOnCreateType);
            this.ValidateUsingTables(authentication);
            var categoryName = new CategoryName(Regex.Replace(this.Path, $"^{oldPath}", newPath));
            this.Context.ValidateCategoryPath(categoryName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateDelete(IAuthentication authentication, object target)
        {
            base.OnValidateDelete(authentication, target);
            if (this.templateList.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeleteOnCreateType);
            var types = EnumerableUtility.Descendants<IItem, Type>(this as IItem, item => item.Childs).ToArray();
            if (types.Any() == true)
                throw new InvalidOperationException(Resources.Exception_CannotDeletePathWithItems);

            this.ValidateUsingTables(authentication);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            accessInfo.Path = this.Path;
            base.AccessInfo = accessInfo;
        }

        public void Attach(NewTypeTemplate template)
        {
            template.EditCanceled += (s, e) => this.templateList.Remove(template);
            template.EditEnded += (s, e) => this.templateList.Remove(template);
            this.templateList.Add(template);
        }

        public void LockInternal(Authentication authentication, string comment)
        {
            base.Lock(authentication, comment);
        }

        public void UnlockInternal(Authentication authentication)
        {
            base.Unlock(authentication);
        }

        public new string Name => base.Name;

        public new string Path => base.Path;

        public new bool IsLocked => base.IsLocked;

        public new bool IsPrivate => base.IsPrivate;

        public new AccessInfo AccessInfo => base.AccessInfo;

        public new LockInfo LockInfo => base.LockInfo;

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

        private void ValidateUsingTables(IAuthentication authentication)
        {
            var tables = this.GetService(typeof(TableCollection)) as TableCollection;
            var types = EnumerableUtility.Descendants<IItem, Type>(this as IItem, item => item.Childs).ToArray();
            {
                var query = from Table item in tables
                            from type in types
                            where item.IsTypeUsed(type.Path) && item.VerifyAccessType(authentication, AccessType.Master) == false
                            select item;

                if (query.Any() == true)
                    throw new PermissionDeniedException(string.Format(Resources.Exception_ItemsPermissionDenined_Format, string.Format(", ", query)));
            }

            {
                var query = from Table item in tables
                            from type in types
                            where item.IsTypeUsed(type.Path) && item.TableState != TableState.None
                            select item.Name;

                if (query.Any() == true)
                    throw new InvalidOperationException(string.Format(Resources.Exception_TableIsBeingEdited_Format, string.Join(", ", query)));
            }
        }

        #region ITypeCategory

        async Task<ITypeCategory> ITypeCategory.AddNewCategoryAsync(Authentication authentication, string name)
        {
            return await this.AddNewCategoryAsync(authentication, name);
        }

        async Task<ITypeTemplate> ITypeCategory.NewTypeAsync(Authentication authentication)
        {
            return await this.NewTypeAsync(authentication);
        }

        ITypeCategory ITypeCategory.Parent => this.Parent;

        IContainer<ITypeCategory> ITypeCategory.Categories => this.Categories;

        IContainer<IType> ITypeCategory.Types => this.Types;

        #endregion

        #region ITypeItem

        ITypeItem ITypeItem.Parent => this.Parent;

        IEnumerable<ITypeItem> ITypeItem.Childs
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
            return (this.DataBase as IDataBase).GetService(serviceType);
        }

        #endregion
    }
}

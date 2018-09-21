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
using Ntreev.Crema.Services.DataBaseService;
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
    class TypeCategory : TypeCategoryBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext>,
        ITypeCategory, ITypeItem
    {
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
                this.ValidateExpired();
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    var result = await this.Service.SetPublicTypeItemAsync(base.Path);
                    this.CremaHost.Sign(authentication, result);
                    base.SetPublic(authentication);
                    this.Context.InvokeItemsSetPublicEvent(authentication, new ITypeItem[] { this, });
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
                    var result = await this.Service.SetPrivateTypeItemAsync(base.Path);
                    this.CremaHost.Sign(authentication, result);
                    base.SetPrivate(authentication);
                    this.Context.InvokeItemsSetPrivateEvent(authentication, new ITypeItem[] { this, });
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
                    var result = await this.Service.AddAccessMemberTypeItemAsync(base.Path, memberID, accessType);
                    this.CremaHost.Sign(authentication, result);
                    base.AddAccessMember(authentication, memberID, accessType);
                    this.Context.InvokeItemsAddAccessMemberEvent(authentication, new ITypeItem[] { this, }, new string[] { memberID, }, new AccessType[] { accessType, });
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
                    var result = await this.Service.SetAccessMemberTypeItemAsync(base.Path, memberID, accessType);
                    this.CremaHost.Sign(authentication, result);
                    base.SetAccessMember(authentication, memberID, accessType);
                    this.Context.InvokeItemsSetAccessMemberEvent(authentication, new ITypeItem[] { this, }, new string[] { memberID, }, new AccessType[] { accessType, });
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
                    var result = await this.Service.RemoveAccessMemberTypeItemAsync(base.Path, memberID);
                    this.CremaHost.Sign(authentication, result);
                    base.RemoveAccessMember(authentication, memberID);
                    this.Context.InvokeItemsRemoveAccessMemberEvent(authentication, new ITypeItem[] { this, }, new string[] { memberID, });
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this, comment);
                    var result = await this.Service.LockTypeItemAsync(base.Path, comment);
                    this.CremaHost.Sign(authentication, result);
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
                await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    var result = await this.Service.UnlockTypeItemAsync(base.Path);
                    this.CremaHost.Sign(authentication, result);
                    base.Unlock(authentication);
                    this.Context.InvokeItemsUnlockedEvent(authentication, new ITypeItem[] { this, });
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
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var result = await this.Service.RenameTypeItemAsync(base.Path, name);
                    this.CremaHost.Sign(authentication, result);
                    base.Rename(authentication, name);
                    this.Container.InvokeCategoriesRenamedEvent(authentication, items, oldNames, oldPaths);
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
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var oldParentPaths = items.Select(item => item.Parent.Path).ToArray();
                    var result = await this.Service.MoveTypeItemAsync(base.Path, parentPath);
                    this.CremaHost.Sign(authentication, result);
                    base.Move(authentication, parentPath);
                    this.Container.InvokeCategoriesMovedEvent(authentication, items, oldPaths, oldParentPaths);
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

                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var container = this.Container;
                    var result = await this.Service.DeleteTypeItemAsync(base.Path);
                    this.CremaHost.Sign(authentication, result);
                    base.Delete(authentication);
                    container.InvokeCategoriesDeletedEvent(authentication, items, oldPaths);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
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
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, revision);
                    var result = await this.Service.GetTypeItemDataSetAsync(base.Path, revision);
                    this.CremaHost.Sign(authentication, result);
                    return result.Value;
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
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    var result = await this.Service.GetTypeItemLogAsync(base.Path, revision);
                    this.CremaHost.Sign(authentication, result);
                    return result.Value ?? new LogInfo[] { };
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
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(FindAsync), this, text, options);
                    var result = await this.Service.FindTypeItemAsync(base.Path, text, options);
                    this.CremaHost.Sign(authentication, result);
                    return result.Value ?? new FindResultInfo[] { };
                });
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetName(string name)
        {
            base.Name = name;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetParent(TypeCategory parent)
        {
            base.Parent = parent;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessChangeType changeType, AccessInfo accessInfo)
        {
            if (changeType != AccessChangeType.Public)
                base.AccessInfo = accessInfo;
            else
                base.AccessInfo = AccessInfo.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            base.AccessInfo = accessInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetLockInfo(LockChangeType changeType, LockInfo lockInfo)
        {
            if (changeType == LockChangeType.Lock)
                base.LockInfo = lockInfo;
            else
                base.LockInfo = LockInfo.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetLockInfo(LockInfo lockInfo)
        {
            base.LockInfo = lockInfo;
        }

        public CremaHost CremaHost => this.Context.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IDataBaseService Service => this.Context.Service;

        public new string Name
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.Name;
            }
        }

        public new string Path
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.Path;
            }
        }

        public new bool IsLocked
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.IsLocked;
            }
        }

        public new bool IsPrivate
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.IsPrivate;
            }
        }

        public new AccessInfo AccessInfo
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.AccessInfo;
            }
        }

        public new LockInfo LockInfo
        {
            get
            {
                this.Dispatcher?.VerifyAccess();
                return base.LockInfo;
            }
        }

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

        #region ITypeCategory

        async Task<ITypeCategory> ITypeCategory.AddNewCategoryAsync(Authentication authentication, string name)
        {
            return await this.Container.AddNewAsync(authentication, name, base.Path);
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

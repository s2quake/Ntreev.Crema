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
    class TableCategory : TableCategoryBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        ITableCategory, ITableItem
    {
        public TableCategory()
        {

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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetPublicTableItem(path));
                await this.DataBase.WaitAsync(result.TaskID);
                return result.TaskID;
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetPrivateTableItem(path));
                await this.DataBase.WaitAsync(result.TaskID);
                return result.TaskID;
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMemberAsync), this, memberID, accessType);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.AddAccessMemberTableItem(path, memberID, accessType));
                await this.DataBase.WaitAsync(result.TaskID);
                return result.TaskID;
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetAccessMemberTableItem(path, memberID, accessType));
                await this.DataBase.WaitAsync(result.TaskID);
                return result.TaskID;
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.RemoveAccessMemberTableItem(path, memberID));
                await this.DataBase.WaitAsync(result.TaskID);
                return result.TaskID;
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(Lock), this, comment);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.LockTableItem(path, comment));
                await this.DataBase.WaitAsync(result.TaskID);
                return result.TaskID;
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.UnlockTableItem(path));
                await this.DataBase.WaitAsync(result.TaskID);
                return result.TaskID;
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
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldNames = items.Select(item => item.Name).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldNames, oldPaths, path);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.RenameTableItem(tuple.path, name));
                await this.DataBase.WaitAsync(result.TaskID);
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
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.MoveTableItem(tuple.path, parentPath));
                await this.DataBase.WaitAsync(result.TaskID);
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
                var dataBase = this.DataBase;
                var tuple = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    var items = EnumerableUtility.One(this).ToArray();
                    var oldPaths = items.Select(item => item.Path).ToArray();
                    var path = base.Path;
                    return (items, oldPaths, path);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.DeleteTableItem(tuple.path));
                await dataBase.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<NewTableTemplate> NewTableAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var template = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewTableAsync), this);
                    return new NewTableTemplate(this);
                });
                await template.BeginEditAsync(authentication);
                return template;
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, revision);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.GetTableItemDataSet(path, revision));
                return await this.Dispatcher.InvokeAsync(() =>
                {
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
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.GetTableItemLog(path, revision));
                return await this.Dispatcher.InvokeAsync(() =>
                {
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

        public async Task<FindResultInfo[]> FindAsync(Authentication authentication, string text, FindOptions options)
        {
            try
            {
                this.ValidateExpired();
                var path = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(FindAsync), this, text, options);
                    return base.Path;
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.FindTableItem(path, text, options));
                return await this.Dispatcher.InvokeAsync(() =>
                {
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
        public void SetParent(TableCategory parent)
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

        public CremaHost CremaHost => this.Context.DataBase.CremaHost;

        public DataBase DataBase => this.Context.DataBase;

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public IDataBaseService Service => this.Context.Service;

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

        #region ITableCategory

        Task ITableCategory.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task ITableCategory.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task ITableCategory.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        async Task<ITableCategory> ITableCategory.AddNewCategoryAsync(Authentication authentication, string name)
        {
            return await this.Container.AddNewAsync(authentication, name, base.Path);
        }

        async Task<ITableTemplate> ITableCategory.NewTableAsync(Authentication authentication)
        {
            return await this.NewTableAsync(authentication);
        }

        ITableCategory ITableCategory.Parent => this.Parent;

        IContainer<ITableCategory> ITableCategory.Categories => this.Categories;

        IContainer<ITable> ITableCategory.Tables => this.Tables;

        #endregion

        #region ITableItem

        Task ITableItem.RenameAsync(Authentication authentication, string newName)
        {
            return this.RenameAsync(authentication, newName);
        }

        Task ITableItem.MoveAsync(Authentication authentication, string categoryPath)
        {
            return this.MoveAsync(authentication, categoryPath);
        }

        Task ITableItem.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        ITableItem ITableItem.Parent => this.Parent;

        IEnumerable<ITableItem> ITableItem.Childs
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
    }
}

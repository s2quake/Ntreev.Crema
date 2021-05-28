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

using JSSoft.Crema.ServiceHosts.Domains;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Users;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Domains
{
    class DomainContext : ItemContext<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomainContextEventCallback, IDomainContext, IServiceProvider
    {
        private ItemsCreatedEventHandler<IDomainItem> itemsCreated;
        private ItemsRenamedEventHandler<IDomainItem> itemsRenamed;
        private ItemsMovedEventHandler<IDomainItem> itemsMoved;
        private ItemsDeletedEventHandler<IDomainItem> itemsDeleted;
        private TaskCompletedEventHandler taskCompleted;

        public readonly TaskResetEvent<Guid> creationEvent;
        public readonly TaskResetEvent<Guid> deletionEvent;
        public readonly IndexedDispatcher callbackEvent;

        public DomainContext(CremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = new CremaDispatcher(this);
            this.creationEvent = new TaskResetEvent<Guid>(this.Dispatcher);
            this.deletionEvent = new TaskResetEvent<Guid>(this.Dispatcher);
            this.callbackEvent = new IndexedDispatcher(this);
        }

        public void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            this.callbackEvent.Dispose();
        }

        public async Task InitializeAsync(Guid authenticationToken)
        {
            var result = await this.Service.SubscribeAsync(authenticationToken);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Initialize(result.Value);
            });
            //  TODO: 데이터 베이스 변경은 서버에서 내용을 받아 갱신하는것으로 개선하는것이 좋을것 같음
            await this.CremaHost.DataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.DataBaseContext.ItemsCreated += DataBaseContext_ItemsCreated;
                this.CremaHost.DataBaseContext.ItemsRenamed += DataBaseContext_ItemsRenamed;
                this.CremaHost.DataBaseContext.ItemsDeleted += DataBaseContext_ItemDeleted;
            });
            this.ReleaseHandle.Reset();
        }

        public async Task ReleaseAsync()
        {
            if (this.Service != null)
                await this.Service.UnsubscribeAsync();
            await Task.Delay(100);
            var tasks = await this.Dispatcher.InvokeAsync(() =>
            {
                var taskList = new List<Task>(this.Domains.Count);
                foreach (var item in this.Domains.ToArray<Domain>())
                {
                    if (item.Logger != null)
                    {
                        taskList.Add(item.Logger.DisposeAsync());
                    }
                }
                return taskList.ToArray();
            });
            await Task.WhenAll(tasks);
            await this.Dispatcher.InvokeAsync(this.Clear);
            this.ReleaseHandle.Set();
        }

        public void InvokeItemsCreatedEvent(Authentication authentication, IDomainItem[] items, object[] args)
        {
            this.OnItemsCreated(new ItemsCreatedEventArgs<IDomainItem>(authentication, items, args));
        }

        public void InvokeItemsRenamedEvent(Authentication authentication, IDomainItem[] items, string[] oldNames, string[] oldPaths)
        {
            this.OnItemsRenamed(new ItemsRenamedEventArgs<IDomainItem>(authentication, items, oldNames, oldPaths));
        }

        public void InvokeItemsMovedEvent(Authentication authentication, IDomainItem[] items, string[] oldPaths, string[] oldParentPaths)
        {
            this.OnItemsMoved(new ItemsMovedEventArgs<IDomainItem>(authentication, items, oldPaths, oldParentPaths));
        }

        public void InvokeItemsDeleteEvent(Authentication authentication, IDomainItem[] items, string[] itemPaths)
        {
            this.OnItemsDeleted(new ItemsDeletedEventArgs<IDomainItem>(authentication, items, itemPaths));
        }

        public void InvokeTaskCompletedEvent(Authentication authentication, Guid taskID)
        {
            this.OnTaskCompleted(new TaskCompletedEventArgs(authentication, taskID));
        }

        public async Task<Domain> CreateAsync(Authentication authentication, DomainMetaData metaData)
        {
            await this.creationEvent.WaitAsync(metaData.DomainID);
            return await this.Dispatcher.InvokeAsync(() => this.GetDomain(metaData.DomainID));
        }

        public async Task WaitDeleteAsync(Domain domain)
        {
            await this.deletionEvent.WaitAsync(domain.ID);
        }

        public async Task DeleteAsync(Authentication authentication, Domain domain)
        {
            await this.deletionEvent.WaitAsync(domain.ID);
        }

        public DomainContextMetaData GetMetaData(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            return new DomainContextMetaData()
            {
                DomainCategories = this.Categories.GetMetaData(authentication),
                Domains = this.Domains.GetMetaData(authentication),
            };
        }

        public void AttachDomainHost(Authentication[] authentications, IDictionary<Domain, IDomainHost> domainHostByDomain)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
                if (domain.Category == null)
                    continue;
                domain.SetDomainHost(Authentication.System, domainHost);
                domain.Attach(authentications);
            }
        }

        public void DetachDomainHost(Authentication[] authentications, IDictionary<Domain, IDomainHost> domainHostByDomain)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
                domain.Detach(authentications);
                domain.SetDomainHost(Authentication.System, null);
            }
        }

        public void DeleteDomains(Authentication authentication, Guid dataBaseID)
        {
            this.Dispatcher.Invoke(() =>
            {
                var domains = this.GetDomains(dataBaseID);
                foreach (var item in domains)
                {
                    item.Dispose(authentication, true);
                }
            });
        }

        public Domain[] GetDomains(Guid dataBaseID)
        {
            this.Dispatcher.VerifyAccess();
            var domainList = new List<Domain>(this.Domains.Count);
            foreach (var item in this.Domains)
            {
                if (item.DataBaseID == dataBaseID)
                {
                    domainList.Add(item);
                }
            }
            return domainList.ToArray();
        }

        public Domain GetDomain(Guid domainID)
        {
            return this.Domains[domainID];
        }

        public CremaHost CremaHost { get; }

        public UserContext UserContext => this.CremaHost.UserContext;

        public DomainCollection Domains => this.Items;

        public CremaDispatcher Dispatcher { get; set; }

        public CremaDispatcher CallbackDispatcher { get; set; }

        public IDomainContextService Service { get; set; }

        public string UserID => this.CremaHost.UserID;

        public ManualResetEvent ReleaseHandle { get; } = new ManualResetEvent(false);

        public event ItemsCreatedEventHandler<IDomainItem> ItemsCreated
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsCreated += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<IDomainItem> ItemsRenamed
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsRenamed += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsRenamed -= value;
            }
        }

        public event ItemsMovedEventHandler<IDomainItem> ItemsMoved
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsMoved += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsMoved -= value;
            }
        }

        public event ItemsDeletedEventHandler<IDomainItem> ItemsDeleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsDeleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsDeleted -= value;
            }
        }

        public event TaskCompletedEventHandler TaskCompleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted -= value;
            }
        }

        protected virtual void OnItemsCreated(ItemsCreatedEventArgs<IDomainItem> e)
        {
            this.itemsCreated?.Invoke(this, e);
        }

        protected virtual void OnItemsRenamed(ItemsRenamedEventArgs<IDomainItem> e)
        {
            this.itemsRenamed?.Invoke(this, e);
        }

        protected virtual void OnItemsMoved(ItemsMovedEventArgs<IDomainItem> e)
        {
            this.itemsMoved?.Invoke(this, e);
        }

        protected virtual void OnItemsDeleted(ItemsDeletedEventArgs<IDomainItem> e)
        {
            this.itemsDeleted?.Invoke(this, e);
        }

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs e)
        {
            this.taskCompleted?.Invoke(this, e);
        }

        private void Initialize(DomainContextMetaData metaData)
        {
            foreach (var item in metaData.DomainCategories)
            {
                if (item != this.Root.Path)
                {
                    var category = this.Categories.AddNew(item);
                    if (category.Parent == this.Root)
                    {
                        category.DataBase = this.CremaHost.DataBaseContext[category.Name];
                    }
                }
            }

            foreach (var item in metaData.Domains)
            {
                var domainInfo = item.DomainInfo;
                var authentication = this.UserContext.Authenticate(domainInfo.CreationInfo);
                var domain = this.Domains.AddDomain(authentication, domainInfo);
                domain.Initialize(authentication, item);
            }
        }

        private async void DataBaseContext_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
            var authentication = await this.UserContext.AuthenticateAsync(e.SignatureDate);
            await this.Dispatcher.InvokeAsync(() =>
            {
                var categoryList = new List<DomainCategory>(e.Items.Length);
                var categoryNameList = new List<string>(e.Items.Length);
                var categoryPathList = new List<string>(e.Items.Length);
                for (var i = 0; i < e.Items.Length; i++)
                {
                    var dataBase = e.Items[i];
                    var categoryName = CategoryName.Create(dataBase.Name);
                    var category = this.Categories.AddNew(categoryName);
                    category.DataBase = dataBase;
                    categoryList.Add(category);
                }
                this.Categories.InvokeCategoriesCreatedEvent(authentication, categoryList.ToArray());
            });

        }

        private async void DataBaseContext_ItemsRenamed(object sender, ItemsRenamedEventArgs<IDataBase> e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var categoryList = new List<DomainCategory>(e.Items.Length);
                var categoryNameList = new List<string>(e.Items.Length);
                var categoryPathList = new List<string>(e.Items.Length);
                for (var i = 0; i < e.Items.Length; i++)
                {
                    var oldName = e.OldNames[i];
                    var newName = e.Items[i].Name;
                    var category = this.Root.Categories[oldName];
                    var categoryName = category.Name;
                    var categoryPath = category.Path;
                    category.Name = newName;
                    categoryList.Add(category);
                    categoryNameList.Add(categoryName);
                    categoryPathList.Add(categoryPath);
                }
                Authentication.System.Sign();
                this.Categories.InvokeCategoriesRenamedEvent(Authentication.System, categoryList.ToArray(), categoryNameList.ToArray(), categoryPathList.ToArray());
            });
        }

        private async void DataBaseContext_ItemDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var categoryList = new List<DomainCategory>(e.Items.Length);
                var categoryPathList = new List<string>(e.Items.Length);
                foreach (var item in e.Items)
                {
                    this.DeleteDomains(item);
                    var category = this.Root.Categories[item.Name];
                    var categoryPath = category.Path;
                    category.Dispose();
                    categoryList.Add(category);
                    categoryPathList.Add(categoryPath);
                }
                Authentication.System.Sign();
                this.Categories.InvokeCategoriesDeletedEvent(Authentication.System, categoryList.ToArray(), categoryPathList.ToArray());
            });
        }

        private void DeleteDomains(IDataBase dataBase)
        {
            foreach (var item in this.Domains.ToArray<Domain>())
            {
                if (item.DataBaseID == dataBase.ID)
                {
                    item.Dispose();
                }
            }
        }

        #region IDomainContextEventCallback

        async void IDomainContextEventCallback.OnDomainsCreated(CallbackInfo callbackInfo, DomainMetaData[] metaDatas)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        this.Domains.AddDomain(authentication, metaDatas);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnDomainsDeleted(CallbackInfo callbackInfo, Guid[] domainIDs, bool[] isCanceleds, object[] results)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domainHostList = new List<IDomainHost>(domainIDs.Length);
                        var domainHostCanceledList = new List<bool>(domainIDs.Length);
                        var domainHostResultList = new List<object>(domainIDs.Length);

                        var domainList = new List<Domain>(domainIDs.Length);
                        var domainCanceledList = new List<bool>(domainIDs.Length);
                        var domainResultList = new List<object>(domainIDs.Length);

                        for (var i = 0; i < domainIDs.Length; i++)
                        {
                            var domainID = domainIDs[i];
                            var isCanceled = isCanceleds[i];
                            var result = results[i];
                            var domain = this.GetDomain(domainID);
                            domain.InvokeDeleteAsync(authentication, isCanceled, result);
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnDomainInfoChanged(CallbackInfo callbackInfo, Guid domainID, DomainInfo domainInfo)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        domain.InvokeDomainInfoChanged(authentication, domainInfo);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnDomainStateChanged(CallbackInfo callbackInfo, Guid domainID, DomainState domainState)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        domain.InvokeDomainStateChanged(authentication, domainState);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnUserAdded(CallbackInfo callbackInfo, Guid domainID, DomainUserInfo domainUserInfo, DomainUserState domainUserState, byte[] data, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        domain.InvokeUserAddedAsync(authentication, domainUserInfo, domainUserState, data, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnUserRemoved(CallbackInfo callbackInfo, Guid domainID, string userID, string ownerID, RemoveInfo removeInfo, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(userID);
                        var ownerUser = ownerID != null ? domain.GetDomainUser(ownerID) : null;
                        domain.InvokeUserRemovedAsync(authentication, domainUser, ownerUser, removeInfo, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnUserLocationChanged(CallbackInfo callbackInfo, Guid domainID, DomainLocationInfo domainLocationInfo)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeUserLocationChanged(authentication, domainUser, domainLocationInfo);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnUserStateChanged(CallbackInfo callbackInfo, Guid domainID, DomainUserState domainUserState)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeUserStateChanged(authentication, domainUser, domainUserState);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnUserEditBegun(CallbackInfo callbackInfo, Guid domainID, DomainLocationInfo domainLocationInfo, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeUserEditBegunAsync(authentication, domainUser, domainLocationInfo, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnUserEditEnded(CallbackInfo callbackInfo, Guid domainID, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeUserEditEndedAsync(authentication, domainUser, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnOwnerChanged(CallbackInfo callbackInfo, Guid domainID, string ownerID, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeOwnerChangedAsync(authentication, domainUser, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnRowAdded(CallbackInfo callbackInfo, Guid domainID, DomainRowInfo[] rows, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeRowAddedAsync(authentication, domainUser, rows, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnRowChanged(CallbackInfo callbackInfo, Guid domainID, DomainRowInfo[] rows, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeRowChangedAsync(authentication, domainUser, rows, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnRowRemoved(CallbackInfo callbackInfo, Guid domainID, DomainRowInfo[] rows, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokeRowRemovedAsync(authentication, domainUser, rows, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnPropertyChanged(CallbackInfo callbackInfo, Guid domainID, string propertyName, object value, Guid taskID)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var domain = this.GetDomain(domainID);
                        var domainUser = domain.GetDomainUser(authentication);
                        domain.InvokePropertyChangedAsync(authentication, domainUser, propertyName, value, taskID);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainContextEventCallback.OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        //this.taskEvent.Set(taskIDs);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        #endregion

        #region IDomainContext

        bool IDomainContext.Contains(string itemPath)
        {
            return this.Contains(itemPath);
        }

        IDomainCollection IDomainContext.Domains => this.Domains;

        IDomainCategoryCollection IDomainContext.Categories => this.Categories;

        IDomainItem IDomainContext.this[string itemPath] => this[itemPath] as IDomainItem;

        IDomainCategory IDomainContext.Root => this.Root;

        #endregion

        #region IEnumerable

        IEnumerator<IDomainItem> IEnumerable<IDomainItem>.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as IDomainItem;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in this)
            {
                yield return item as IDomainItem;
            }
        }

        #endregion

        #region IServiceProvider 

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.CremaHost as ICremaHost).GetService(serviceType);
        }

        #endregion
    }
}

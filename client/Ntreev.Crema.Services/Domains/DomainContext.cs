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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Data;
using Ntreev.Crema.Services.DomainService;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;

namespace Ntreev.Crema.Services.Domains
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class DomainContext : ItemContext<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomainServiceCallback, IDomainContext, IServiceProvider, ICremaService
    {
        private readonly UserContext userContext;
        private DomainServiceClient service;
        private Timer timer;

        private ItemsCreatedEventHandler<IDomainItem> itemsCreated;
        private ItemsRenamedEventHandler<IDomainItem> itemsRenamed;
        private ItemsMovedEventHandler<IDomainItem> itemsMoved;
        private ItemsDeletedEventHandler<IDomainItem> itemsDeleted;

        public DomainContext(CremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
            this.userContext = cremaHost.UserContext;
            this.Dispatcher = new CremaDispatcher(this);
        }

        public async Task InitializeAsync(string address, Guid authenticationToken, ServiceInfo serviceInfo)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var binding = CremaHost.CreateBinding(serviceInfo);
                var endPointAddress = new EndpointAddress($"net.tcp://{address}:{serviceInfo.Port}/DomainService");
                var instanceContext = new InstanceContext(this);

                this.service = new DomainServiceClient(instanceContext, binding, endPointAddress);
                this.service.Open();
                if (this.service is ICommunicationObject service)
                {
                    service.Faulted += Service_Faulted;
                }

                var result = this.service.Subscribe(authenticationToken);
#if !DEBUG
                this.timer = new Timer(30000);
                this.timer.Elapsed += Timer_Elapsed;
                this.timer.Start();
#endif
                var metaData = result.GetValue();
                this.Initialize(metaData);
                this.CremaHost.DataBases.Dispatcher.Invoke(() =>
                {
                    this.CremaHost.DataBases.ItemsCreated += DataBases_ItemsCreated;
                    this.CremaHost.DataBases.ItemsRenamed += DataBases_ItemsRenamed;
                    this.CremaHost.DataBases.ItemsDeleted += DataBases_ItemDeleted;
                });
                this.CremaHost.AddService(this);
            });
        }

        public async Task<DomainMetaData[]> RestoreAsync(Authentication authentication, DataBase dataBase)
        {
            var result = await Task.Run(() => this.service.GetMetaData());
            this.CremaHost.Sign(authentication, result);

            var metaData = result.Value;
            var metaDataList = new List<DomainMetaData>(metaData.Domains.Length);
            foreach (var item in metaData.Domains)
            {
                if (item.DomainInfo.DataBaseID == dataBase.ID)
                {
                    metaDataList.Add(item);
                }
            }
            return metaDataList.ToArray();
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

        public Task<Domain> CreateAsync(Authentication authentication, DomainMetaData metaData)
        {
            return this.Domains.CreateAsync(authentication, metaData);
        }

        public Task DeleteAsync(Authentication authentication, Domain domain, bool isCanceled, object result)
        {
            return this.Domains.DeleteAsync(authentication, domain, isCanceled, result);
        }

        public async Task AddDomainsAsync(DomainMetaData[] metaDatas)
        {
            foreach (var item in metaDatas)
            {
                var authentication = await this.userContext.AuthenticateAsync(item.DomainInfo.CreationInfo);
                var domain = await this.Domains.AddDomainAsync(authentication, item.DomainInfo);
                if (domain == null)
                    continue;


                await this.Dispatcher.InvokeAsync(() => domain.Initialize(Authentication.System, item));
            }
        }

        public async Task CloseAsync(CloseInfo closeInfo)
        {
            this.timer?.Dispose();
            this.timer = null;
            if (this.service != null)
            {
                try
                {
                    if (closeInfo.Reason != CloseReason.NoResponding)
                    {
                        await Task.Run(() => this.service.Unsubscribe());
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            this.service.Close();
                        else
                            this.service.Abort();
                    }
                    else
                    {
                        this.service.Abort();
                    }
                }
                catch
                {
                    this.service.Abort();
                }
                this.service = null;
            }
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
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

        public async Task<DomainContextMetaData> GetMetaDataAsync(Authentication authentication)
        {
            var domains = await this.Domains.GetMetaDataAsync(authentication);
            return await this.Dispatcher.InvokeAsync(() =>
            {
                return new DomainContextMetaData()
                {
                    DomainCategories = this.Categories.GetMetaData(authentication),
                    Domains = domains,
                };
            });
        }

        public void AttachDomainHost(Authentication[] authentications, IDictionary<Domain, IDomainHost> domainHostByDomain)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (var item in domainHostByDomain)
                {
                    var domain = item.Key;
                    var domainHost = item.Value;
                    domain.SetDomainHost(Authentication.System, domainHost);
                    domain.Attach(authentications);
                }
            });
        }

        public void DetachDomainHost(Authentication[] authentications, IDictionary<Domain, IDomainHost> domainHostByDomain)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (var item in domainHostByDomain)
                {
                    var domain = item.Key;
                    var domainHost = item.Value;
                    domain.Detach(authentications);
                    domain.SetDomainHost(Authentication.System, null);
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

        public Task<Domain[]> GetDomainsAsync(Guid dataBaseID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var domainList = new List<Domain>(this.Domains.Count);
                foreach (var item in this.Domains)
                {
                    if (item.DataBaseID == dataBaseID)
                    {
                        domainList.Add(item);
                    }
                }
                return domainList.ToArray();
            });
        }

        public Task<Domain> GetDomainAsync(Guid domainID)
        {
            return this.Dispatcher.InvokeAsync(() => this.Domains[domainID]);
        }

        public Domain GetDomain(Guid domainID)
        {
            return this.Domains[domainID];
        }

        public CremaHost CremaHost { get; }

        public DomainCollection Domains => this.Items;

        public CremaDispatcher Dispatcher { get; set; }

        public CremaDispatcher CallbackDispatcher { get; set; }

        public IDomainService Service => this.service;

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

        private void Initialize(DomainContextMetaData metaData)
        {
            foreach (var item in metaData.DomainCategories)
            {
                if (item != this.Root.Path)
                {
                    var category = this.Categories.AddNew(item);
                    if (category.Parent == this.Root)
                    {
                        category.DataBase = this.CremaHost.DataBases[category.Name];
                    }
                }
            }

            foreach (var item in metaData.Domains)
            {
                var domainInfo = item.DomainInfo;
                var authentication = this.userContext.Authenticate(domainInfo.CreationInfo);
                var domain = this.Domains.AddDomain(authentication, domainInfo);
                domain.Initialize(authentication, item);
            }
        }

        private async void DataBases_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
            var authentication = await this.userContext.AuthenticateAsync(e.SignatureDate);
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

        private void DataBases_ItemsRenamed(object sender, ItemsRenamedEventArgs<IDataBase> e)
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
        }

        private void DataBases_ItemDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
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

        private async void Service_Faulted(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.service.Abort();
                    this.service = null;
                }
                catch
                {

                }
                this.timer?.Dispose();
                this.timer = null;
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
            await this.CremaHost.RemoveServiceAsync(this);
        }

        //private async void InvokeAsync(Action action, string callbackName)
        //{
        //    var count = 0;
        //_Invoke:
        //    try
        //    {

        //        await this.Dispatcher.InvokeAsync(action);
        //    }
        //    catch (NullReferenceException e)
        //    {
        //        await Task.Delay(1);
        //        if (count == 0)
        //        {
        //            count++;
        //            goto _Invoke;
        //        }
        //        this.CremaHost.Error(callbackName);
        //        this.CremaHost.Error(e);
        //    }
        //    catch (Exception e)
        //    {
        //        this.CremaHost.Error(callbackName);
        //        this.CremaHost.Error(e);
        //    }
        //}

        //private async void InvokeAsync<T>(Func<T> action, string callbackName)
        //{
        //    var count = 0;
        //_Invoke:
        //    try
        //    {

        //        await this.Dispatcher.InvokeAsync(action);
        //    }
        //    catch (NullReferenceException e)
        //    {
        //        await Task.Delay(1);
        //        if (count == 0)
        //        {
        //            count++;
        //            goto _Invoke;
        //        }
        //        this.CremaHost.Error(callbackName);
        //        this.CremaHost.Error(e);
        //    }
        //    catch (Exception e)
        //    {
        //        this.CremaHost.Error(callbackName);
        //        this.CremaHost.Error(e);
        //    }
        //}

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.timer?.Stop();
            try
            {
                await Task.Run(() => this.service.IsAlive());
                this.timer?.Start();
            }
            catch
            {

            }
        }

        #region IDomainServiceCallback

        async void IDomainServiceCallback.OnDomainCreated(SignatureDate signatureDate, DomainInfo domainInfo, DomainState domainState)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                await this.Domains.AddDomainAsync(authentication, domainInfo);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnDomainDeleted(SignatureDate signatureDate, Guid domainID, bool isCanceled, object result)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                var domain = await this.Dispatcher.InvokeAsync(() => this.GetDomain(domainID));
                if (domain.Host != null)
                {
                    await domain.Host.DeleteAsync(authentication, isCanceled, result);
                }
                else
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        domain.Dispose(authentication, isCanceled, null);
                        this.Domains.InvokeDomainDeletedEvent(authentication, domain, isCanceled, null);
                    });
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnDomainInfoChanged(SignatureDate signatureDate, Guid domainID, DomainInfo domainInfo)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var domain = this.GetDomain(domainID);
                    domain.InvokeDomainInfoChanged(authentication, domainInfo);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnDomainStateChanged(SignatureDate signatureDate, Guid domainID, DomainState domainState)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var domain = this.GetDomain(domainID);
                    domain.InvokeDomainStateChanged(authentication, domainState);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnUserAdded(SignatureDate signatureDate, Guid domainID, DomainUserInfo domainUserInfo, DomainUserState domainUserState)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var domain = this.GetDomain(domainID);
                    domain.InvokeUserAdded(authentication, domainUserInfo, domainUserState);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnUserRemoved(SignatureDate signatureDate, Guid domainID, DomainUserInfo domainUserInfo, RemoveInfo removeInfo)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var domain = this.GetDomain(domainID);
                    domain.InvokeUserRemoved(authentication, domainUserInfo, removeInfo);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnUserChanged(SignatureDate signatureDate, Guid domainID, DomainUserInfo domainUserInfo, DomainUserState domainUserState)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var domain = this.GetDomain(domainID);
                    domain.InvokeUserChanged(authentication, domainUserInfo, domainUserState);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnRowAdded(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                var domain = await this.GetDomainAsync(domainID);
                await domain.InvokeRowAddedAsync(authentication, rows);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnRowChanged(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                var domain = await this.GetDomainAsync(domainID);
                await domain.InvokeRowChangedAsync(authentication, rows);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnRowRemoved(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                var domain = await this.GetDomainAsync(domainID);
                await domain.InvokeRowRemovedAsync(authentication, rows);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnPropertyChanged(SignatureDate signatureDate, Guid domainID, string propertyName, object value)
        {
            try
            {
                var authentication = await this.userContext.AuthenticateAsync(signatureDate);
                var domain = await this.GetDomainAsync(domainID);
                await domain.InvokePropertyChangedAsync(authentication, propertyName, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDomainServiceCallback.OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            this.service.Close();
            this.service = null;
            this.timer?.Dispose();
            this.timer = null;
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            await this.CremaHost.RemoveServiceAsync(this);
        }

        #endregion

        #region IDomainContext

        Task<bool> IDomainContext.ContainsAsync(string itemPath)
        {
            return this.Dispatcher.InvokeAsync(() => this.Contains(itemPath));
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

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
        private DomainServiceClient service;
        private Timer timer;

        private ItemsCreatedEventHandler<IDomainItem> itemsCreated;
        private ItemsRenamedEventHandler<IDomainItem> itemsRenamed;
        private ItemsMovedEventHandler<IDomainItem> itemsMoved;
        private ItemsDeletedEventHandler<IDomainItem> itemsDeleted;

        public DomainContext(CremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
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
                var metaData = result.Value;
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

        //public Task DeleteDomainsAsync(Authentication authentication, Guid dataBaseID)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        var domains = this.GetDomains(dataBaseID);
        //        var itemPaths = domains.Select(item => item.Path).ToArray();
        //        foreach (var item in domains)
        //        {
        //            item.Dispose(authentication, true, null);
        //        }
        //        this.InvokeItemsDeleteEvent(authentication, domains, itemPaths);
        //    });
        //}

        //public async Task RestoreAsync(Authentication authentication, Guid dataBaseID)
        //{
        //    var result = await this.CremaHost.InvokeServiceAsync(() => this.service.GetMetaData(dataBaseID));
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        this.CremaHost.Sign(authentication, result);
        //        this.Restore(authentication, result.Value);
        //    });


        //    //var metaData = result.Value;
        //    //var metaDataList = new List<DomainMetaData>(metaData.Domains.Length);
        //    //foreach (var item in metaData.Domains)
        //    //{
        //    //    if (item.DomainInfo.DataBaseID == dataBase.ID)
        //    //    {
        //    //        metaDataList.Add(item);
        //    //    }
        //    //}
        //    //return metaDataList.ToArray();
        //    //return null;
        //}

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

        public Domain Create(Authentication authentication, DomainMetaData metaData)
        {
            return this.Domains.Create(authentication, metaData);
        }

        public Task<Domain> CreateAsync(Authentication authentication, DomainMetaData metaData)
        {
            return this.Domains.CreateAsync(authentication, metaData);
        }

        public Task DeleteAsync(Authentication authentication, Domain domain, bool isCanceled, object result)
        {
            return this.Domains.DeleteAsync(authentication, domain, isCanceled, result);
        }

        //public async Task AddDomainsAsync(DomainMetaData[] metaDatas)
        //{
        //    foreach (var item in metaDatas)
        //    {
        //        var authentication = await this.UserContext.AuthenticateAsync(item.DomainInfo.CreationInfo);
        //        var domain = await this.Domains.AddDomainAsync(authentication, item.DomainInfo);
        //        if (domain == null)
        //            continue;

        //        await this.Dispatcher.InvokeAsync(() => domain.Initialize(Authentication.System, item));
        //    }
        //}

        public async Task CloseAsync(CloseInfo closeInfo)
        {
            this.service.Unsubscribe();
            this.service.Close();
            this.service = null;
            this.timer?.Dispose();
            this.timer = null;

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
            await this.Dispatcher.DisposeAsync();
            this.Dispatcher = null;
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

        //public async Task<DomainContextMetaData> GetMetaDataAsync(Authentication authentication)
        //{
        //    var domains = await this.Domains.GetMetaDataAsync(authentication);
        //    return await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        return new DomainContextMetaData()
        //        {
        //            DomainCategories = this.Categories.GetMetaData(authentication),
        //            Domains = domains,
        //        };
        //    });
        //}

        public void AttachDomainHost(Authentication[] authentications, IDictionary<Domain, IDomainHost> domainHostByDomain)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
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
                    item.Dispose(authentication, true, null);
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

        //public Task<Domain[]> GetDomainsAsync(Guid dataBaseID)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        var domainList = new List<Domain>(this.Domains.Count);
        //        foreach (var item in this.Domains)
        //        {
        //            if (item.DataBaseID == dataBaseID)
        //            {
        //                domainList.Add(item);
        //            }
        //        }
        //        return domainList.ToArray();
        //    });
        //}

        //public Task<Domain> GetDomainAsync(Guid domainID)
        //{
        //    return this.Dispatcher.InvokeAsync(() => this.Domains[domainID]);
        //}

        public Domain GetDomain(Guid domainID)
        {
            return this.Domains[domainID];
        }

        public CremaHost CremaHost { get; }

        public UserContext UserContext => this.CremaHost.UserContext;

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
                var authentication = this.UserContext.Authenticate(domainInfo.CreationInfo);
                var domain = this.Domains.AddDomain(authentication, domainInfo);
                domain.Initialize(authentication, item);
            }
        }

        private void Restore(Authentication authentication, DomainMetaData[] metaDatas)
        {
            foreach (var item in metaDatas)
            {
                var domainInfo = item.DomainInfo;
                var domain = this.Domains[item.DomainID];
                domain.Initialize(authentication, item);
            }
        }

        private async void DataBases_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
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

        private async void DataBases_ItemsRenamed(object sender, ItemsRenamedEventArgs<IDataBase> e)
        {
            var authentication = await this.UserContext.AuthenticateAsync(e.SignatureDate);
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
                this.Categories.InvokeCategoriesRenamedEvent(authentication, categoryList.ToArray(), categoryNameList.ToArray(), categoryPathList.ToArray());
            });
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
            await await this.Dispatcher.InvokeAsync(async() =>
            {
                this.service.Abort();
                this.service = null;
                this.timer?.Dispose();
                this.timer = null;
                var tasks = await this.Dispatcher.InvokeAsync(() =>
                {
                    var taskList = new List<Task>(this.Domains.Count);
                    foreach (var item in this.Domains.ToArray<Domain>())
                    {
                        if (item.DataDispatcher != null)
                        {
                            taskList.Add(item.DataDispatcher.DisposeAsync());
                        }
                    }
                    return taskList.ToArray();
                });
                await Task.WhenAll(tasks);
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
            this.CremaHost.RemoveServiceAsync(this);
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.timer?.Stop();
            try
            {
                await this.Dispatcher.InvokeAsync(() => this.service.IsAlive());
                this.timer?.Start();
            }
            catch
            {

            }
        }

        #region IDomainServiceCallback

        void IDomainServiceCallback.OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            this.Dispatcher.Invoke(async () =>
            {
                this.service.Close();
                this.service = null;
                this.timer?.Dispose();
                this.timer = null;
                var tasks = await this.Dispatcher.InvokeAsync(() =>
                {
                    var taskList = new List<Task>(this.Domains.Count);
                    foreach (var item in this.Domains.ToArray<Domain>())
                    {
                        if (item.DataDispatcher != null)
                        {
                            taskList.Add(item.DataDispatcher.DisposeAsync());
                        }
                    }
                    return taskList.ToArray();
                });
                await Task.WhenAll(tasks);
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
            this.CremaHost.RemoveServiceAsync(this);
        }

        void IDomainServiceCallback.OnDomainsCreated(SignatureDate signatureDate, DomainMetaData[] metaDatas)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    this.Domains.AddDomain(authentication, metaDatas);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnDomainsDeleted(SignatureDate signatureDate, Guid[] domainIDs, bool[] isCanceleds, object[] results)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domainHostList = new List<IDomainHost>(domainIDs.Length);
                    var domainList = new List<Domain>(domainIDs.Length);
                    for (var i = 0; i < domainIDs.Length; i++)
                    {
                        var domainID = domainIDs[i];
                        var isCanceled = isCanceleds[i];
                        var result = results[i];
                        var domain = this.GetDomain(domainID);
                        var domainHost = domain.Host;
                        domain.Dispose(authentication, isCanceled, null);
                        domainHostList.Add(domainHost);
                        domainList.Add(domain);
                    }
                    this.Domains.InvokeDomainDeletedEvent(authentication, domainList.ToArray(), isCanceleds, results);
                    for (var i = 0; i < domainIDs.Length; i++)
                    {
                        var domainHost = domainHostList[i];
                        var isCanceled = isCanceleds[i];
                        var result = results[i];
                        if (domainHost != null)
                            domainHost.DeleteAsync(authentication, isCanceled, result);
                    }
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnDomainInfoChanged(SignatureDate signatureDate, Guid domainID, DomainInfo domainInfo)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    domain.InvokeDomainInfoChanged(authentication, domainInfo);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnDomainStateChanged(SignatureDate signatureDate, Guid domainID, DomainState domainState)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    domain.InvokeDomainStateChanged(authentication, domainState);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnUserAdded(SignatureDate signatureDate, Guid domainID, DomainUserInfo domainUserInfo, DomainUserState domainUserState, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    await domain.InvokeUserAddedAsync(authentication, domainUserInfo, domainUserState, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnUserRemoved(SignatureDate signatureDate, Guid domainID, string userID, string ownerID, RemoveInfo removeInfo, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(userID);
                    var ownerUser = domain.GetDomainUser(ownerID);
                    await domain.InvokeUserRemovedAsync(authentication, domainUser, ownerUser, removeInfo, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnUserLocationChanged(SignatureDate signatureDate, Guid domainID, DomainLocationInfo domainLocationInfo)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    domain.InvokeUserLocationChanged(authentication, domainUser, domainLocationInfo);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnUserStateChanged(SignatureDate signatureDate, Guid domainID, DomainUserState domainUserState)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    domain.InvokeUserStateChanged(authentication, domainUser, domainUserState);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnUserEditBegun(SignatureDate signatureDate, Guid domainID, DomainLocationInfo domainLocationInfo, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    await domain.InvokeUserEditBegunAsync(authentication, domainUser, domainLocationInfo, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnUserEditEnded(SignatureDate signatureDate, Guid domainID, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    await domain.InvokeUserEditEndedAsync(authentication, domainUser, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnOwnerChanged(SignatureDate signatureDate, Guid domainID, string ownerID, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    await domain.InvokeOwnerChangedAsync(authentication, domainUser, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        //void IDomainServiceCallback.OnUserChanged(SignatureDate signatureDate, Guid domainID, DomainUserInfo domainUserInfo, DomainUserState domainUserState)
        //{
        //    try
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            var authentication = this.UserContext.Authenticate(signatureDate);
        //            var domain = this.GetDomain(domainID);
        //            domain.InvokeUserChanged(authentication, domainUserInfo, domainUserState);
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        this.CremaHost.Error(e);
        //    }
        //}

        void IDomainServiceCallback.OnRowAdded(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    await domain.InvokeRowAddedAsync(authentication, domainUser, rows, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnRowChanged(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    await domain.InvokeRowChangedAsync(authentication, domainUser, rows, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnRowRemoved(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    await domain.InvokeRowRemovedAsync(authentication, domainUser, rows, id);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDomainServiceCallback.OnPropertyChanged(SignatureDate signatureDate, Guid domainID, string propertyName, object value, long id)
        {
            try
            {
                this.Dispatcher.Invoke(async () =>
                {
                    var authentication = this.UserContext.Authenticate(signatureDate);
                    var domain = this.GetDomain(domainID);
                    var domainUser = domain.GetDomainUser(authentication);
                    await domain.InvokePropertyChangedAsync(authentication, domainUser, propertyName, value, id);
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

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
using Ntreev.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains
{
    class DomainCollection : ItemContainer<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomainCollection
    {
        private EventHandler<DomainsCreatedEventArgs> domainsCreated;
        private EventHandler<DomainsDeletedEventArgs> domainsDeleted;
        private EventHandler<DomainEventArgs> domainInfoChanged;
        private EventHandler<DomainEventArgs> domainStateChanged;
        private EventHandler<DomainUserAddedEventArgs> domainUserAdded;
        private EventHandler<DomainUserRemovedEventArgs> domainUserRemoved;
        private EventHandler<DomainUserLocationEventArgs> domainUserLocationChanged;
        private EventHandler<DomainUserEventArgs> domainUserStateChanged;
        private EventHandler<DomainUserLocationEventArgs> domainUserEditBegun;
        private EventHandler<DomainUserEventArgs> domainUserEditEnded;
        private EventHandler<DomainUserEventArgs> domainOwnerChanged;
        private EventHandler<DomainRowEventArgs> domainRowAdded;
        private EventHandler<DomainRowEventArgs> domainRowChanged;
        private EventHandler<DomainRowEventArgs> domainRowRemoved;
        private EventHandler<DomainPropertyEventArgs> domainPropertyChanged;

        public void InvokeDomainCreatedEvent(Authentication authentication, Domain[] domains)
        {
            var domainInfos = domains.Select(item => (object)item.DomainInfo).ToArray();
            var metaDatas = domains.Select(item => item.GetMetaData(authentication)).ToArray();
            var args = new DomainsCreatedEventArgs(authentication, domains, metaDatas);
            foreach (var item in domains)
            {
                var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainCreatedEvent), item);
                var comment = EventMessageBuilder.BeginDomain(authentication, item);
                this.CremaHost.Debug(eventLog);
                this.CremaHost.Info(comment);
            }
            this.OnDomainsCreated(args);
            this.Context.InvokeItemsCreatedEvent(authentication, domains, domainInfos);
        }

        public void InvokeDomainDeletedEvent(Authentication authentication, Domain[] domains, bool[] isCanceleds, long taksID)
        {
            var itemPaths = domains.Select(item => item.Path).ToArray();
            var args = new DomainsDeletedEventArgs(authentication, domains, isCanceleds) { TaskID = taksID };
            for (var i = 0; i < domains.Length; i++)
            {
                var item = domains[i];
                var isCanceled = isCanceleds[i];
                var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainDeletedEvent), item, isCanceled);
                var comment = isCanceled == false ? EventMessageBuilder.EndDomain(authentication, item) : EventMessageBuilder.CancelDomain(authentication, item);
                this.CremaHost.Debug(eventLog);
                this.CremaHost.Info(comment);
            }
            this.OnDomainsDeleted(args);
            this.Context.InvokeItemsDeleteEvent(authentication, domains, itemPaths);
        }

        public void InvokeDomainUserAddedEvent(Authentication authentication, Domain domain, DomainUser domainUser, long id)
        {
            var args = new DomainUserAddedEventArgs(authentication, domain, domainUser, null) { TaskID = id };
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainUserAddedEvent), domain, domainUser);
            var comment = EventMessageBuilder.EnterDomainUser(authentication, domain);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnDomainUserAdded(args);
        }

        public void InvokeDomainUserLocationChangedEvent(Authentication authentication, Domain domain, DomainUser domainUser)
        {
            var args = new DomainUserLocationEventArgs(authentication, domain, domainUser);
            this.OnDomainUserLocationChanged(args);
        }

        public void InvokeDomainUserStateChangedEvent(Authentication authentication, Domain domain, DomainUser domainUser)
        {
            var args = new DomainUserEventArgs(authentication, domain, domainUser);
            this.OnDomainUserStateChanged(args);
        }

        public void InvokeDomainUserEditBegunEvent(Authentication authentication, Domain domain, DomainUser domainUser, long id)
        {
            var args = new DomainUserLocationEventArgs(authentication, domain, domainUser) { TaskID = id };
            this.OnDomainUserEditBegun(args);
        }

        public void InvokeDomainUserEditEndedEvent(Authentication authentication, Domain domain, DomainUser domainUser, long id)
        {
            var args = new DomainUserEventArgs(authentication, domain, domainUser) { TaskID = id };
            this.OnDomainUserEditEnded(args);

        }

        public void InvokeDomainOwnerChangedEvent(Authentication authentication, Domain domain, DomainUser domainUser, long id)
        {
            var args = new DomainUserEventArgs(authentication, domain, domainUser) { TaskID = id };
            this.OnDomainOwnerChanged(args);
        }

        public void InvokeDomainRowAddedEvent(Authentication authentication, Domain domain, long id, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows) { TaskID = id };
            this.OnDomainRowAdded(args);
        }

        public void InvokeDomainRowChangedEvent(Authentication authentication, Domain domain, long id, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows) { TaskID = id };
            this.OnDomainRowChanged(args);
        }

        public void InvokeDomainRowRemovedEvent(Authentication authentication, Domain domain, long id, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows) { TaskID = id };
            this.OnDomainRowRemoved(args);
        }

        public void InvokeDomainPropertyChangedEvent(Authentication authentication, Domain domain, long id, string propertyName, object value)
        {
            var args = new DomainPropertyEventArgs(authentication, domain, propertyName, value) { TaskID = id };
            this.OnDomainPropertyChanged(args);
        }


        public void InvokeDomainUserRemovedEvent(Authentication authentication, Domain domain, DomainUser domainUser, RemoveInfo removeInfo, long id)
        {
            var args = new DomainUserRemovedEventArgs(authentication, domain, domainUser, removeInfo) { TaskID = id };
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainUserRemovedEvent), domain, domainUser, removeInfo.Reason, removeInfo.Message);
            var comment = removeInfo.Reason == RemoveReason.Kick
                ? EventMessageBuilder.KickDomainUser(authentication, domain, domainUser)
                : EventMessageBuilder.LeaveDomainUser(authentication, domain);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnDomainUserRemoved(args);
        }

        public void InvokeDomainInfoChangedEvent(Authentication authentication, Domain domain)
        {
            var args = new DomainEventArgs(authentication, domain);
            this.OnDomainInfoChanged(args);
        }

        public void InvokeDomainStateChangedEvent(Authentication authentication, Domain domain)
        {
            var args = new DomainEventArgs(authentication, domain);
            this.OnDomainStateChanged(args);
        }

        public Domain Create(Authentication authentication, DomainMetaData metaData)
        {
            return this.Dispatcher.Invoke(() =>
            {
                var domain = this[metaData.DomainID];

                if (domain == null)
                {
                    if (metaData.DomainInfo.DomainType == typeof(TableContentDomain).Name)
                    {
                        domain = new TableContentDomain(metaData.DomainInfo);
                    }
                    else if (metaData.DomainInfo.DomainType == typeof(TableTemplateDomain).Name)
                    {
                        domain = new TableTemplateDomain(metaData.DomainInfo);
                    }
                    else if (metaData.DomainInfo.DomainType == typeof(TypeDomain).Name)
                    {
                        domain = new TypeDomain(metaData.DomainInfo);
                    }

                    this.Add(domain);
                    domain.Category = this.Context.Categories.Prepare(metaData.DomainInfo.CategoryPath);
                    this.InvokeDomainCreatedEvent(authentication, new Domain[] { domain });
                    domain.Initialize(authentication, metaData);
                }
                else
                {
                    domain.Initialize(authentication, metaData);
                }

                return domain;
            });
        }

        public async Task<Domain> CreateAsync(Authentication authentication, DomainMetaData metaData)
        {
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var domain = this[metaData.DomainID];
                if (domain == null)
                {
                    if (metaData.DomainInfo.DomainType == typeof(TableContentDomain).Name)
                    {
                        domain = new TableContentDomain(metaData.DomainInfo);
                    }
                    else if (metaData.DomainInfo.DomainType == typeof(TableTemplateDomain).Name)
                    {
                        domain = new TableTemplateDomain(metaData.DomainInfo);
                    }
                    else if (metaData.DomainInfo.DomainType == typeof(TypeDomain).Name)
                    {
                        domain = new TypeDomain(metaData.DomainInfo);
                    }

                    this.Add(domain);
                    domain.Category = this.Context.Categories.Prepare(metaData.DomainInfo.CategoryPath);
                    this.InvokeDomainCreatedEvent(authentication, new Domain[] { domain });
                    domain.Initialize(authentication, metaData);
                }
                else
                {
                    domain.Initialize(authentication, metaData);
                }

                return domain;
            });
        }

        //public async Task DeleteAsync(Authentication authentication, Domain domain, bool isCanceled, object result)
        //{
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        this.Remove(domain);
        //        domain.Dispose(authentication, isCanceled);
        //        this.InvokeDomainDeletedEvent(authentication, new Domain[] { domain }, new bool[] { isCanceled });
        //    });

        //    //return this.Dispatcher.InvokeAsync(() =>
        //    //{
        //    //    foreach (var item in this.ToArray<Domain>())
        //    //    {
        //    //        if (item.DomainInfo.DataBaseID == dataBaseID && item.DomainInfo.ItemPath == itemPath && item.DomainInfo.ItemType == itemType)
        //    //        {
        //    //            item.Dispose(authentication, isCanceled);
        //    //            return;
        //    //        }
        //    //    }
        //    //});
        //}

        private Domain CreateDomain(DomainInfo domainInfo)
        {
            var domainType = domainInfo.DomainType;
            if (domainType == typeof(TableContentDomain).Name)
            {
                return new TableContentDomain(domainInfo);
            }
            else if (domainType == typeof(TypeDomain).Name)
            {
                return new TypeDomain(domainInfo);
            }
            else if (domainType == typeof(TableTemplateDomain).Name)
            {
                return new TableTemplateDomain(domainInfo);
            }
            return null;
        }

        public Domain AddDomain(Authentication authentication, DomainInfo domainInfo)
        {
            var domain = this.CreateDomain(domainInfo);
            var category = this.Context.Categories.Prepare(domainInfo.CategoryPath);
            domain.Category = category;
            var dataBase = category.DataBase as DataBase;
            var isLoaded = dataBase.Service != null;
            //if (domain.DataBaseID == dataBase.ID && isLoaded == true && dataBase.IsResetting == false)
            //{
            //    var target = dataBase.Dispatcher.Invoke(() => dataBase.FindDomainHost(domain));
            //    if (target != null)
            //    {
            //        target.Attach(domain);
            //        domain.Host = target;
            //    }
            //}
            this.InvokeDomainCreatedEvent(authentication, new Domain[] { domain });
            return domain;
        }

        public Domain[] AddDomain(Authentication authentication, DomainMetaData[] metaDatas)
        {
            this.Dispatcher.VerifyAccess();
            var domainList = new List<Domain>(metaDatas.Length);
            foreach (var item in metaDatas)
            {
                var domainInfo = item.DomainInfo;
                var domain = this.CreateDomain(domainInfo);
                var category = this.Context.Categories.Prepare(domainInfo.CategoryPath);
                domain.Category = category;
                var dataBase = category.DataBase as DataBase;
                var isLoaded = dataBase.Service != null;
                domainList.Add(domain);
                domain.Initialize(authentication, item);
            }

            var query = from item in domainList
                        let dataBase = item.Category.DataBase as DataBase
                        where dataBase.Service != null
                        group item by dataBase as DataBase into g
                        select g;
            foreach (var item in query)
            {
                var dataBase = item.Key;
                dataBase.Dispatcher.InvokeAsync(() => dataBase.AttachDomainHost(item.ToArray()));
            }

            var domains = domainList.ToArray();
            this.InvokeDomainCreatedEvent(authentication, domains);

            foreach(var item in domains)
            {
                this.Context.creationEvent.Set(item.ID);
            }
            return domains;
        }

        public async Task<Domain> AddDomainAsync(Authentication authentication, DomainInfo domainInfo)
        {
            var domain = this.CreateDomain(domainInfo);
            var dataBase = await this.Dispatcher.InvokeAsync(() =>
            {
                var category = this.Context.Categories.Prepare(domainInfo.CategoryPath);
                domain.Category = category;
                return category.DataBase as DataBase;
            });
            var isLoaded = dataBase.Service != null;
            if (domain.DataBaseID == dataBase.ID && isLoaded == true && dataBase.IsResetting == false)
            {
                var target = await dataBase.FindDomainHostAsync(domain);
                if (target != null)
                {
                    target.Attach(domain);
                    domain.Host = target;
                }
            }
            await this.Dispatcher.InvokeAsync(() => this.InvokeDomainCreatedEvent(authentication, new Domain[] { domain }));
            return domain;
        }

        public async Task AddAsync(Authentication authentication, Domain domain, DataBase dataBase)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var categoryName = CategoryName.Create(dataBase.Name, domain.DomainInfo.ItemType);
                var category = this.Context.Categories.Prepare(categoryName);
                domain.Category = category;
                //domain.Logger = new DomainLogger(this.Context.Serializer, domain);
                //domain.Dispatcher = new CremaDispatcher(domain);
                this.InvokeDomainCreatedEvent(authentication, new Domain[] { domain });
            });
        }

        //public async Task RemoveAsync(Authentication authentication, Domain domain, bool isCanceled, object result)
        //{
        //    await this.Context.deletionEvent.WaitAsync(domain.ID);
        //    return;
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        this.Remove(domain);
        //        //var dispatcher = domain.Dispatcher;
        //        //domain.Dispatcher = null;
        //        //domain.Logger?.Dispose(true);
        //        //domain.Logger = null;
        //        domain.Dispose(authentication, isCanceled, result);
        //        //dispatcher.Dispose();
        //        this.InvokeDomainDeletedEvent(authentication, new Domain[] { domain }, new bool[] { isCanceled }, new object[] { result });
        //    });
        //}

        public bool Contains(Guid domainID)
        {
            return this.Contains(domainID.ToString());
        }

        public DomainMetaData[] GetMetaData(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));

            var domains = this.ToArray<Domain>();
            var metaDataList = new List<DomainMetaData>(domains.Length);
            foreach (var item in domains)
            {
                var metaData = item.GetMetaData(authentication);
                metaDataList.Add(metaData);
            }
            return metaDataList.ToArray();
        }

        public async Task<DomainMetaData[]> GetMetaDataAsync(Authentication authentication)
        {
            var domains = await this.Dispatcher.InvokeAsync(() => this.ToArray<Domain>());
            var metaDataList = new List<DomainMetaData>(domains.Length);
            foreach (var item in domains)
            {
                var metaData = await item.GetMetaDataAsync(authentication);
                metaDataList.Add(metaData);
            }
            return metaDataList.ToArray();
        }

        public Domain this[Guid domainID] => this[domainID.ToString()];

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.Context.Dispatcher;

        public new int Count => base.Count;

        public event EventHandler<DomainsCreatedEventArgs> DomainsCreated
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainsCreated += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainsCreated -= value;
            }
        }

        public event EventHandler<DomainsDeletedEventArgs> DomainsDeleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainsDeleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainsDeleted -= value;
            }
        }

        public event EventHandler<DomainEventArgs> DomainInfoChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainInfoChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainInfoChanged -= value;
            }
        }

        public event EventHandler<DomainEventArgs> DomainStateChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainStateChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainStateChanged -= value;
            }
        }

        public event EventHandler<DomainUserAddedEventArgs> DomainUserAdded
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainUserAdded += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainUserAdded -= value;
            }
        }

        public event EventHandler<DomainUserRemovedEventArgs> DomainUserRemoved
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainUserRemoved += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainUserRemoved -= value;
            }
        }

        public event EventHandler<DomainUserLocationEventArgs> DomainUserLocationChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserLocationChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserLocationChanged -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> DomainUserStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserStateChanged -= value;
            }
        }

        public event EventHandler<DomainUserLocationEventArgs> DomainUserEditBegun
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserEditBegun += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserEditBegun -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> DomainUserEditEnded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserEditEnded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.domainUserEditEnded -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> DomainOwnerChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.domainOwnerChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.domainOwnerChanged -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> DomainRowAdded
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainRowAdded += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainRowAdded -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> DomainRowChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainRowChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainRowChanged -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> DomainRowRemoved
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainRowRemoved += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainRowRemoved -= value;
            }
        }

        public event EventHandler<DomainPropertyEventArgs> DomainPropertyChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainPropertyChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainPropertyChanged -= value;
            }
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                base.CollectionChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                base.CollectionChanged -= value;
            }
        }

        protected virtual void OnDomainsCreated(DomainsCreatedEventArgs e)
        {
            this.domainsCreated?.Invoke(this, e);
        }

        protected virtual void OnDomainsDeleted(DomainsDeletedEventArgs e)
        {
            this.domainsDeleted?.Invoke(this, e);
        }

        protected virtual void OnDomainInfoChanged(DomainEventArgs e)
        {
            this.domainInfoChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainStateChanged(DomainEventArgs e)
        {
            this.domainStateChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainUserAdded(DomainUserAddedEventArgs e)
        {
            this.domainUserAdded?.Invoke(this, e);
        }

        protected virtual void OnDomainUserLocationChanged(DomainUserLocationEventArgs e)
        {
            this.domainUserLocationChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainUserStateChanged(DomainUserEventArgs e)
        {
            this.domainUserStateChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainUserEditBegun(DomainUserLocationEventArgs e)
        {
            this.domainUserEditBegun?.Invoke(this, e);
        }

        protected virtual void OnDomainUserEditEnded(DomainUserEventArgs e)
        {
            this.domainUserEditEnded?.Invoke(this, e);
        }

        protected virtual void OnDomainOwnerChanged(DomainUserEventArgs e)
        {
            this.domainOwnerChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainUserRemoved(DomainUserRemovedEventArgs e)
        {
            this.domainUserRemoved?.Invoke(this, e);
        }

        protected virtual void OnDomainRowAdded(DomainRowEventArgs e)
        {
            this.domainRowAdded?.Invoke(this, e);
        }

        protected virtual void OnDomainRowChanged(DomainRowEventArgs e)
        {
            this.domainRowChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainRowRemoved(DomainRowEventArgs e)
        {
            this.domainRowRemoved?.Invoke(this, e);
        }

        protected virtual void OnDomainPropertyChanged(DomainPropertyEventArgs e)
        {
            this.domainPropertyChanged?.Invoke(this, e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher?.VerifyAccess();
            base.OnCollectionChanged(e);
        }

        #region IDomainCollection

        IDomain IDomainCollection.this[Guid domainID]
        {
            get { return this[domainID]; }
        }

        IEnumerator<IDomain> IEnumerable<IDomain>.GetEnumerator()
        {
            this.Dispatcher.VerifyAccess();
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            this.Dispatcher.VerifyAccess();
            return this.GetEnumerator();
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.Context as IServiceProvider).GetService(serviceType);
        }

        #endregion
    }
}

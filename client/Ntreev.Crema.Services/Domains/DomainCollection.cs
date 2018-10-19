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
        private EventHandler<DomainEventArgs> domainCreated;
        private EventHandler<DomainDeletedEventArgs> domainDeleted;
        private EventHandler<DomainEventArgs> domainInfoChanged;
        private EventHandler<DomainEventArgs> domainStateChanged;
        private EventHandler<DomainUserEventArgs> domainUserAdded;
        private EventHandler<DomainUserEventArgs> domainUserChanged;
        private EventHandler<DomainUserRemovedEventArgs> domainUserRemoved;
        private EventHandler<DomainRowEventArgs> domainRowAdded;
        private EventHandler<DomainRowEventArgs> domainRowChanged;
        private EventHandler<DomainRowEventArgs> domainRowRemoved;
        private EventHandler<DomainPropertyEventArgs> domainPropertyChanged;

        public void InvokeDomainCreatedEvent(Authentication authentication, Domain domain)
        {
            var args = new DomainEventArgs(authentication, domain);
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainCreatedEvent), domain);
            var comment = EventMessageBuilder.BeginDomain(authentication, domain);
            var domainInfo = domain.DomainInfo;
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnDomainCreated(args);
            this.Context.InvokeItemsCreatedEvent(authentication, new IDomainItem[] { domain }, new object[] { domainInfo });
        }

        public void InvokeDomainDeletedEvent(Authentication authentication, Domain domain, bool isCanceled, object result)
        {
            var args = new DomainDeletedEventArgs(authentication, domain, isCanceled, result);
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainDeletedEvent), domain, isCanceled);
            var comment = isCanceled == false ? EventMessageBuilder.EndDomain(authentication, domain) : EventMessageBuilder.CancelDomain(authentication, domain);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnDomainDeleted(args);
            this.Context.InvokeItemsDeleteEvent(authentication, new IDomainItem[] { domain }, new string[] { domain.Path });
        }

        public void InvokeDomainRowAddedEvent(Authentication authentication, Domain domain, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows);
            this.OnDomainRowAdded(args);
        }

        public void InvokeDomainRowChangedEvent(Authentication authentication, Domain domain, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows);
            this.OnDomainRowChanged(args);
        }

        public void InvokeDomainRowRemovedEvent(Authentication authentication, Domain domain, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows);
            this.OnDomainRowChanged(args);
        }

        public void InvokeDomainPropertyChangedEvent(Authentication authentication, Domain domain, string propertyName, object value)
        {
            var args = new DomainPropertyEventArgs(authentication, domain, propertyName, value);
            this.OnDomainPropertyChanged(args);
        }

        public void InvokeDomainUserAddedEvent(Authentication authentication, Domain domain, DomainUser domainUser)
        {
            var args = new DomainUserEventArgs(authentication, domain, domainUser);
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainUserAddedEvent), domain, domainUser);
            var comment = EventMessageBuilder.EnterDomainUser(authentication, domain);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnDomainUserAdded(args);
        }

        public void InvokeDomainUserRemovedEvent(Authentication authentication, Domain domain, DomainUser domainUser, RemoveInfo removeInfo)
        {
            var args = new DomainUserRemovedEventArgs(authentication, domain, domainUser, removeInfo);
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainUserRemovedEvent), domain, domainUser, removeInfo.Reason, removeInfo.Message);
            var comment = removeInfo.Reason == RemoveReason.Kick
                ? EventMessageBuilder.KickDomainUser(authentication, domain, domainUser)
                : EventMessageBuilder.LeaveDomainUser(authentication, domain);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnDomainUserRemoved(args);
        }

        public void InvokeDomainUserChangedEvent(Authentication authentication, Domain domain, DomainUser domainUser)
        {
            var args = new DomainUserEventArgs(authentication, domain, domainUser);
            this.OnDomainUserChanged(args);
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
                    this.InvokeDomainCreatedEvent(authentication, domain);
                    domain.Initialize(authentication, metaData);
                }
                else
                {
                    domain.Initialize(authentication, metaData);
                }

                return domain;
            });
        }

        public async Task DeleteAsync(Authentication authentication, Domain domain, bool isCanceled, object result)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Remove(domain);
                domain.Dispose(authentication, isCanceled, result);
                this.InvokeDomainDeletedEvent(authentication, domain, isCanceled, result);
            });

            //return this.Dispatcher.InvokeAsync(() =>
            //{
            //    foreach (var item in this.ToArray<Domain>())
            //    {
            //        if (item.DomainInfo.DataBaseID == dataBaseID && item.DomainInfo.ItemPath == itemPath && item.DomainInfo.ItemType == itemType)
            //        {
            //            item.Dispose(authentication, isCanceled);
            //            return;
            //        }
            //    }
            //});
        }

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
            this.InvokeDomainCreatedEvent(authentication, domain);
            return domain;
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
            await this.Dispatcher.InvokeAsync(() => this.InvokeDomainCreatedEvent(authentication, domain));
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
                this.InvokeDomainCreatedEvent(authentication, domain);
            });
        }

        public async Task RemoveAsync(Authentication authentication, Domain domain, bool isCanceled, object result)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Remove(domain);
                //var dispatcher = domain.Dispatcher;
                //domain.Dispatcher = null;
                //domain.Logger?.Dispose(true);
                //domain.Logger = null;
                domain.Dispose(authentication, isCanceled, result);
                //dispatcher.Dispose();
                this.InvokeDomainDeletedEvent(authentication, domain, isCanceled, result);
            });
        }

        public Task<bool> ContainsAsync(Guid domainID)
        {
            return this.Dispatcher.InvokeAsync(() => this.Contains(domainID.ToString()));
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

        public event EventHandler<DomainEventArgs> DomainCreated
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainCreated += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainCreated -= value;
            }
        }

        public event EventHandler<DomainDeletedEventArgs> DomainDeleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainDeleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainDeleted -= value;
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

        public event EventHandler<DomainUserEventArgs> DomainUserAdded
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

        public event EventHandler<DomainUserEventArgs> DomainUserChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.domainUserChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.domainUserChanged -= value;
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

        protected virtual void OnDomainCreated(DomainEventArgs e)
        {
            this.domainCreated?.Invoke(this, e);
        }

        protected virtual void OnDomainDeleted(DomainDeletedEventArgs e)
        {
            this.domainDeleted?.Invoke(this, e);
        }

        protected virtual void OnDomainInfoChanged(DomainEventArgs e)
        {
            this.domainInfoChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainStateChanged(DomainEventArgs e)
        {
            this.domainStateChanged?.Invoke(this, e);
        }

        protected virtual void OnDomainUserAdded(DomainUserEventArgs e)
        {
            this.domainUserAdded?.Invoke(this, e);
        }

        protected virtual void OnDomainUserChanged(DomainUserEventArgs e)
        {
            this.domainUserChanged?.Invoke(this, e);
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

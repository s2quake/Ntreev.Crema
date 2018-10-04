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

        public void InvokeDomainDeletedEvent(Authentication authentication, Domain domain, bool isCanceled)
        {
            var args = new DomainDeletedEventArgs(authentication, domain, isCanceled);
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainDeletedEvent), domain, isCanceled);
            var comment = isCanceled == false ? EventMessageBuilder.EndDomain(authentication, domain) : EventMessageBuilder.CancelDomain(authentication, domain);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(comment);
            this.OnDomainDeleted(args);
            this.Context.InvokeItemsDeleteEvent(authentication, new IDomainItem[] { domain }, new string[] { domain.Path });
        }

        public async void InvokeDomainRowAddedEvent(Authentication authentication, Domain domain, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDomainRowAdded(args);
            });
        }

        public async void InvokeDomainRowChangedEvent(Authentication authentication, Domain domain, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDomainRowChanged(args);
            });
        }

        public async void InvokeDomainRowRemovedEvent(Authentication authentication, Domain domain, DomainRowInfo[] rows)
        {
            var args = new DomainRowEventArgs(authentication, domain, rows);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDomainRowChanged(args);
            });
        }

        public async void InvokeDomainPropertyChangedEvent(Authentication authentication, Domain domain, string propertyName, object value)
        {
            var args = new DomainPropertyEventArgs(authentication, domain, propertyName, value);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDomainPropertyChanged(args);
            });
        }

        public async void InvokeDomainUserAddedEvent(Authentication authentication, Domain domain, DomainUser domainUser)
        {
            var args = new DomainUserEventArgs(authentication, domain, domainUser);
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainUserAddedEvent), domain, domainUser);
            var comment = EventMessageBuilder.EnterDomainUser(authentication, domain);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.Debug(eventLog);
                this.CremaHost.Info(comment);
                this.OnDomainUserAdded(args);
            });
        }

        public async void InvokeDomainUserRemovedEvent(Authentication authentication, Domain domain, DomainUser domainUser, RemoveInfo removeInfo)
        {
            var args = new DomainUserRemovedEventArgs(authentication, domain, domainUser, removeInfo);
            var eventLog = EventLogBuilder.Build(authentication, this, nameof(InvokeDomainUserRemovedEvent), domain, domainUser, removeInfo.Reason, removeInfo.Message);
            var comment = removeInfo.Reason == RemoveReason.Kick
                ? EventMessageBuilder.KickDomainUser(authentication, domain, domainUser)
                : EventMessageBuilder.LeaveDomainUser(authentication, domain);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.Debug(eventLog);
                this.CremaHost.Info(comment);
                this.OnDomainUserRemoved(args);
            });
        }

        public async void InvokeDomainUserChangedEvent(Authentication authentication, Domain domain, DomainUser domainUser)
        {
            var args = new DomainUserEventArgs(authentication, domain, domainUser);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDomainUserChanged(args);
            });
        }

        public async void InvokeDomainInfoChangedEvent(Authentication authentication, Domain domain)
        {
            var args = new DomainEventArgs(authentication, domain);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDomainInfoChanged(args);
            });
        }

        public async void InvokeDomainStateChangedEvent(Authentication authentication, Domain domain)
        {
            var args = new DomainEventArgs(authentication, domain);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDomainStateChanged(args);
            });
        }

        public async Task<Domain> CreateAsync(Authentication authentication, DomainMetaData metaData)
        {
            return await await this.Dispatcher.InvokeAsync(async () =>
            {
                var domain = this[metaData.DomainID];

                if (domain == null)
                {
                    if (metaData.DomainInfo.DomainType == typeof(TableContentDomain).Name)
                    {
                        domain = new TableContentDomain(metaData.DomainInfo, this.Dispatcher);
                    }
                    else if (metaData.DomainInfo.DomainType == typeof(TableTemplateDomain).Name)
                    {
                        domain = new TableTemplateDomain(metaData.DomainInfo, this.Dispatcher);
                    }
                    else if (metaData.DomainInfo.DomainType == typeof(TypeDomain).Name)
                    {
                        domain = new TypeDomain(metaData.DomainInfo, this.Dispatcher);
                    }

                    this.Add(domain);
                    domain.Category = this.Context.Categories.Prepare(metaData.DomainInfo.CategoryPath);
                    domain.Dispatcher = new CremaDispatcher(domain);
                    this.InvokeDomainCreatedEvent(authentication, domain);
                    await domain.InitializeAsync(authentication, metaData);
                }
                else
                {
                    await domain.InitializeAsync(authentication, metaData);
                }

                return domain;
            });
        }

        public async Task DeleteAsync(Authentication authentication, Domain domain, bool isCanceled)
        {
            await await this.Dispatcher.InvokeAsync(async () =>
            {
                this.Remove(domain);
                var dispatcher = domain.Dispatcher;
                domain.Dispatcher = null;
                await dispatcher.InvokeAsync(() => domain.Dispose(authentication, isCanceled));
                if (dispatcher.Owner is DomainContext == false)
                {
                    dispatcher.Dispose();
                }
                this.InvokeDomainDeletedEvent(authentication, domain, isCanceled);
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
                return new TableContentDomain(domainInfo, this.Dispatcher);
            }
            else if (domainType == typeof(TypeDomain).Name)
            {
                return new TypeDomain(domainInfo, this.Dispatcher);
            }
            else if (domainType == typeof(TableTemplateDomain).Name)
            {
                return new TableTemplateDomain(domainInfo, this.Dispatcher);
            }
            return null;
        }

        public async Task<Domain> AddDomainAsync(DomainInfo domainInfo)
        {
            var userConext = this.CremaHost.UserContext;
            var authentication = await userConext.AuthenticateAsync(domainInfo.CreationInfo);
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
                    await target.RestoreAsync(authentication, domain);
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
                domain.Dispatcher = new CremaDispatcher(domain);
                this.InvokeDomainCreatedEvent(authentication, domain);
            });
        }

        public async Task RemoveAsync(Authentication authentication, Domain domain, bool isCanceled)
        {
            await await this.Dispatcher.InvokeAsync(async () =>
            {
                this.Remove(domain);
                var dispatcher = domain.Dispatcher;
                domain.Dispatcher = null;
                //domain.Logger?.Dispose(true);
                //domain.Logger = null;
                await dispatcher.InvokeAsync(() => domain.Dispose(authentication, isCanceled));
                dispatcher.Dispose();
                this.InvokeDomainDeletedEvent(authentication, domain, isCanceled);
            });
        }

        public Task<bool> ContainsAsync(Guid domainID)
        {
            return this.Dispatcher.InvokeAsync(() => this.Contains(domainID.ToString()));
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

        public Domain this[Guid domainID]
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return this[domainID.ToString()];
            }
        }

        public CremaHost CremaHost => this.Context.CremaHost;

        public CremaDispatcher Dispatcher => this.Context.Dispatcher;

        public new int Count
        {
            get
            {
                this.Dispatcher.VerifyAccess();
                return base.Count;
            }
        }

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

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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Ntreev.Crema.ServiceModel;
using Ntreev.Library.ObjectModel;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Crema.Services.Data;
using Ntreev.Library.IO;

namespace Ntreev.Crema.Services.Domains
{
    class DomainContext : ItemContext<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomainContext, IServiceProvider
    {
        private ItemsCreatedEventHandler<IDomainItem> itemsCreated;
        private ItemsRenamedEventHandler<IDomainItem> itemsRenamed;
        private ItemsMovedEventHandler<IDomainItem> itemsMoved;
        private ItemsDeletedEventHandler<IDomainItem> itemsDeleted;

        public DomainContext(CremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
            this.CremaHost.Debug(Resources.Message_DomainContextInitialize);
            this.Dispatcher = new CremaDispatcher(this);
            this.BasePath = cremaHost.GetPath(CremaPath.Domains);
            this.CremaHost.Opened += CremaHost_Opened;
            this.CremaHost.Debug(Resources.Message_DomainContextIsCreated);
        }

        public async Task InitializeAsync()
        {
            var dataBases = await this.CremaHost.DataBases.Dispatcher.InvokeAsync(() => this.CremaHost.DataBases.ToArray<DataBase>());

            foreach (var item in dataBases)
            {
                var categoryName = CategoryName.Create(item.Name);
                var category = this.Categories.AddNew(categoryName);
                category.DataBase = item;
            }
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

        public object GetService(System.Type serviceType)
        {
            return this.CremaHost.GetService(serviceType);
        }

        //public async Task RestoreAsync(Authentication authentication, Domain domain)
        //{
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        authentication.Sign();
        //        var dataBase = this.CremaHost.DataBases[domain.DataBaseID];
        //        var categoryName = CategoryName.Create(dataBase.Name, domain.DomainInfo.ItemType);
        //        //var category = this.Categories.Prepare(categoryName);
        //        //domain.Category = category;
        //        //domain.Dispatcher = new CremaDispatcher(domain);
        //        //this.Domains.InvokeDomainCreatedEvent(authentication, domain, domain.DomainInfo);
        //    });
        //}

        public async Task AddAsync(Authentication authentication, Domain domain, DataBase dataBase)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var categoryName = CategoryName.Create(dataBase.Name, domain.DomainInfo.ItemType);
                var category = this.Categories.Prepare(categoryName);
                domain.Category = category;
                domain.Context = this;
                domain.Logger = new DomainLogger(this.Serializer, domain);
                this.Domains.InvokeDomainCreatedEvent(authentication, new Domain[] { domain });
            });
        }

        public async Task RemoveAsync(Authentication authentication, Domain domain, bool isCanceled, object result)
        {
            await domain.Logger?.DisposeAsync(true);
            domain.Logger = null;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Domains.Remove(domain);
                //var dispatcher = domain.Dispatcher;
                //domain.Dispatcher = null;

                domain.Dispose(authentication, isCanceled, result);
                //dispatcher.Dispose();
                this.Domains.InvokeDomainDeletedEvent(authentication, new Domain[] { domain },new bool[] { isCanceled }, new object[] { result });
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

        public DomainMetaData[] GetDomainMetaDatas(Authentication authentication, Guid dataBaseID)
        {
            return this.Dispatcher.Invoke(() =>
            {
                var domains = this.GetDomains(dataBaseID);
                var metaDataList = new List<DomainMetaData>(domains.Length);
                foreach (var item in domains)
                {
                    metaDataList.Add(item.GetMetaData(authentication));
                }
                return metaDataList.ToArray();
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

        public Task DeleteDomainsAsync(Authentication authentication, Guid dataBaseID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var domains = this.GetDomains(dataBaseID);
                var isCanceleds = domains.Select(item => true).ToArray();
                var results = domains.Select(item => (object)null).ToArray();
                foreach (var item in domains)
                {
                    item.Dispose(authentication, true, null);
                }
                this.Domains.InvokeDomainDeletedEvent(authentication, domains, isCanceleds, results);
            });
        }

        public Task AttachUsersAsync(Authentication authentication, Guid dataBaseID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var domains = this.GetDomains(dataBaseID);
                foreach (var item in domains)
                {
                    item.Attach(authentication);
                }
            });
        }

        public Task DetachUsersAsync(Authentication authentication, Guid dataBaseID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var domains = this.GetDomains(dataBaseID);
                foreach (var item in domains)
                {
                    item.Detach(authentication);
                }
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

        public Task BeginTransactionAsync(Authentication authentication, string sourcePath, string destPath)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                DirectoryUtility.Delete(destPath);
                if (DirectoryUtility.Exists(sourcePath) == true)
                    DirectoryUtility.Copy(sourcePath, destPath);
            });
        }

        public Task EndTransactionAsync(Authentication authentication, string domainPath, string transactionPath)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                DirectoryUtility.Delete(transactionPath);
                if (DirectoryUtility.Exists(domainPath) == true)
                    DirectoryUtility.Copy(domainPath, transactionPath);
            });
        }

        public Task CancelTransactionAsync(string domainPath, string transactionPath)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (DirectoryUtility.Exists(transactionPath) == true)
                    DirectoryUtility.Copy(transactionPath, domainPath);
                DirectoryUtility.Delete(transactionPath);
            });
        }

        public DomainCollection Domains => base.Items;

        public CremaHost CremaHost { get; }

        public string BasePath { get; }

        public CremaDispatcher Dispatcher { get; }

        public IObjectSerializer Serializer => this.CremaHost.Serializer;

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

        public async Task RestoreAsync(CremaSettings settings)
        {
            if (settings.NoCache == false)
            {
                var dataBases = await this.Dispatcher.InvokeAsync(() => this.CremaHost.DataBases.ToArray<DataBase>());
                foreach (var item in dataBases)
                {
                    await this.RestoreAsync(item);
                }
            }
        }

        public async Task RestoreAsync(DataBase dataBase)
        {
            var restorers = this.GetDomainRestorers(dataBase.ID);
            if (restorers.Any() == false)
                return;

            var tasks = restorers.Select(item => item.RestoreAsync()).ToArray();
            var result = Task.WhenAll(tasks);

            try
            {
                await result;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    var domains = restorers.Where(item => item.Domain != null).Select(item => item.Domain).ToArray();
                    foreach(var item in domains)
                    {
                        var categoryName = CategoryName.Create(dataBase.Name, item.DomainInfo.ItemType);
                        var category = this.Categories.Prepare(categoryName);
                        item.Category = category;
                    }
                    this.Domains.InvokeDomainCreatedEvent(Authentication.System, domains);
                });
                this.CremaHost.Info(string.Format(Resources.Message_RestoreResult_Format, restorers.Length, 0));
            }
            catch
            {
                var exceptions = result.Exception.InnerExceptions;
                foreach (var item in exceptions)
                {
                    this.CremaHost.Error(item);
                }
                this.CremaHost.Info(string.Format(Resources.Message_RestoreResult_Format, restorers.Length - exceptions.Count, exceptions.Count));
            }
        }

        public async Task DisposeAsync()
        {
            var tasks = await this.Dispatcher.InvokeAsync(() =>
            {
                var taskList = new List<Task>(this.Domains.Count);
                foreach (var item in this.Domains.ToArray<Domain>())
                {
                    if (item.Logger != null)
                    {
                        taskList.Add(item.Logger.DisposeAsync(false));
                    }
                }
                return taskList.ToArray();
            });
            await Task.WhenAll(tasks);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Clear();
                this.Dispatcher.Dispose();
            });
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

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            await this.CremaHost.DataBases.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.DataBases.ItemsCreated += DataBases_ItemsCreated;
                this.CremaHost.DataBases.ItemsRenamed += DataBases_ItemsRenamed;
                this.CremaHost.DataBases.ItemsDeleted += DataBases_ItemDeleted;
            });
        }

        private async void DataBases_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
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
                this.CremaHost.Sign(Authentication.System);
                this.Categories.InvokeCategoriesCreatedEvent(Authentication.System, categoryList.ToArray());
            });
        }

        private async void DataBases_ItemsRenamed(object sender, ItemsRenamedEventArgs<IDataBase> e)
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
                this.CremaHost.Sign(Authentication.System);
                this.Categories.InvokeCategoriesRenamedEvent(Authentication.System, categoryList.ToArray(), categoryNameList.ToArray(), categoryPathList.ToArray());
            });
        }

        private async void DataBases_ItemDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
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
                    var localPath = Path.Combine(this.BasePath, $"{item.ID}");
                    DirectoryUtility.Delete(localPath);
                    category.Dispose();
                    categoryList.Add(category);
                    categoryPathList.Add(categoryPath);
                }
                this.CremaHost.Sign(Authentication.System);
                this.Categories.InvokeCategoriesDeletedEvent(Authentication.System, categoryList.ToArray(), categoryPathList.ToArray());
            });
        }

        private void DeleteDomains(IDataBase dataBase)
        {
            foreach (var item in this.Domains.ToArray<Domain>())
            {
                if (item.DataBaseID == dataBase.ID)
                {
                    item.Dispatcher.Invoke(() => item.Dispose(this));
                }
            }

            DirectoryUtility.Delete(this.BasePath, dataBase.ID.ToString());
        }

        private DomainRestorer[] GetDomainRestorers(Guid dataBaseID)
        {
            var path = Path.Combine(this.BasePath, $"{dataBaseID}");
            if (Directory.Exists(path) == false)
                return new DomainRestorer[] { };

            var directories = Directory.GetDirectories(path);
            var domainRestorerList = new List<DomainRestorer>(directories.Length);
            foreach (var item in directories)
            {
                var directoryInfo = new DirectoryInfo(item);
                if (Guid.TryParse(directoryInfo.Name, out Guid domainID) == true)
                {
                    domainRestorerList.Add(new DomainRestorer(this, item));
                }
            }

            return domainRestorerList.ToArray();
        }

        #region IDomainContext

        bool IDomainContext.Contains(string itemPath)
        {
            return this.Contains(itemPath);
        }

        IDomainCollection IDomainContext.Domains => this.Domains;

        IDomainCategoryCollection IDomainContext.Categories => this.Categories;

        IDomainItem IDomainContext.this[string itemPath] => this[itemPath] as IDomainItem;

        IDomainCategory IDomainContext.Root => this.Root;

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

        #endregion

        #region IServiceProvider 

        object IServiceProvider.GetService(System.Type serviceType)
        {
            return (this.CremaHost as ICremaHost).GetService(serviceType);
        }

        #endregion
    }
}

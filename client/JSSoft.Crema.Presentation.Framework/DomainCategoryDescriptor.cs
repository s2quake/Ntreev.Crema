﻿// Released under the MIT License.
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

using JSSoft.Crema.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace JSSoft.Crema.Presentation.Framework
{
    public class DomainCategoryDescriptor : DescriptorBase, IDomainCategoryDescriptor, IDomainItemDescriptor
    {
        private readonly IDomainCategory category;
        private readonly object owner;
        private readonly ObservableCollection<DomainCategoryDescriptor> categories = new();
        private readonly ObservableCollection<DomainDescriptor> domains = new();

        public DomainCategoryDescriptor(Authentication authentication, IDomainCategory category, DescriptorTypes descriptorTypes, object owner)
            : base(authentication, category, descriptorTypes)
        {
            this.category = category;
            this.owner = owner ?? this;
            this.category.Dispatcher.VerifyAccess();
            this.Name = category.Name;
            this.Path = category.Path;

            this.Domains = new ReadOnlyObservableCollection<DomainDescriptor>(this.domains);
            this.Categories = new ReadOnlyObservableCollection<DomainCategoryDescriptor>(this.categories);

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.category.Renamed += Category_Renamed;
            }

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsRecursive) == true)
            {
                if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                {
                    this.category.Domains.CollectionChanged += Domains_CollectionChanged;
                    this.category.Categories.CollectionChanged += Categories_CollectionChanged;
                }

                foreach (var item in this.category.Categories)
                {
                    var descriptor = new DomainCategoryDescriptor(this.authentication, item, this.descriptorTypes, this.owner);
                    item.ExtendedProperties[this.owner] = descriptor;
                    this.categories.Add(descriptor);
                }

                foreach (var item in this.category.Domains)
                {
                    var descriptor = new DomainDescriptor(this.authentication, item, this.descriptorTypes, this.owner);
                    item.ExtendedProperties[this.owner] = descriptor;
                    this.domains.Add(descriptor);
                }
            }

            if (this.category.GetService(typeof(IDataBase)) is IDataBase dataBase)
            {
                this.IsActivated = dataBase.IsLoaded;
                if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                {
                    dataBase.Dispatcher.InvokeAsync(() =>
                    {
                        dataBase.Loaded += DataBase_Loaded;
                        dataBase.Unloaded += DataBase_Unloaded;
                    });
                }
            }
        }

        [DescriptorProperty]
        public string Name { get; private set; }

        [DescriptorProperty]
        public string Path { get; private set; }

        [DescriptorProperty]
        public bool IsActivated { get; private set; }

        public ReadOnlyObservableCollection<DomainCategoryDescriptor> Categories { get; private set; }

        public ReadOnlyObservableCollection<DomainDescriptor> Domains { get; private set; }

        protected async override void OnDisposed(EventArgs e)
        {
            if (this.referenceTarget == null && this.category != null)
            {
                await this.category.Dispatcher.InvokeAsync(() =>
                {
                    if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                    {
                        this.category.Renamed -= Category_Renamed;

                    }
                    if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                    {
                        if (this.descriptorTypes.HasFlag(DescriptorTypes.IsRecursive) == true)
                        {
                            this.category.Domains.CollectionChanged -= Domains_CollectionChanged;
                            this.category.Categories.CollectionChanged -= Categories_CollectionChanged;
                        }
                    }
                    if (this.category.GetService(typeof(IDataBase)) is IDataBase dataBase)
                    {
                        if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                        {
                            dataBase.Loaded -= DataBase_Loaded;
                            dataBase.Unloaded -= DataBase_Unloaded;
                        }
                    }
                });
            }
            base.OnDisposed(e);
        }

        private async void Category_Renamed(object sender, EventArgs e)
        {
            this.Name = this.category.Name;
            this.Path = this.category.Path;
            await this.RefreshAsync();
        }

        private void Domains_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var descriptorList = new List<DomainDescriptor>(e.NewItems.Count);
                        foreach (IDomain item in e.NewItems)
                        {
                            if (item.ExtendedProperties.ContainsKey(this.owner) == true)
                            {
                                var descriptor = item.ExtendedProperties[this.owner] as DomainDescriptor;
                                descriptorList.Add(descriptor);
                            }
                            else
                            {
                                var descriptor = new DomainDescriptor(this.authentication, item, this.descriptorTypes, this.owner);
                                item.ExtendedProperties[this.owner] = descriptor;
                                descriptorList.Add(descriptor);
                            }

                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var item in descriptorList)
                            {
                                this.domains.Add(item);
                            }
                        });
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var descriptorList = new List<DomainDescriptor>(e.OldItems.Count);
                        foreach (IDomain item in e.OldItems)
                        {
                            var descriptor = item.ExtendedProperties[this.owner] as DomainDescriptor;
                            descriptorList.Add(descriptor);
                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var item in descriptorList)
                            {
                                this.domains.Remove(item);
                            }
                        });
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {

                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            this.categories.Clear();
                            this.domains.Clear();
                        });
                    }
                    break;
            }
        }

        private void Categories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var descriptorList = new List<DomainCategoryDescriptor>(e.NewItems.Count);
                        foreach (IDomainCategory item in e.NewItems)
                        {
                            if (item.ExtendedProperties.ContainsKey(this.owner) == true)
                            {
                                var descriptor = item.ExtendedProperties[this.owner] as DomainCategoryDescriptor;
                                descriptorList.Add(descriptor);
                            }
                            else
                            {
                                var descriptor = new DomainCategoryDescriptor(this.authentication, item, this.descriptorTypes, this.owner);
                                item.ExtendedProperties[this.owner] = descriptor;
                                descriptorList.Add(descriptor);
                            }
                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var item in descriptorList)
                            {
                                this.categories.Add(item);
                            }
                        });
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var descriptorList = new List<DomainCategoryDescriptor>(e.OldItems.Count);
                        foreach (IDomainCategory item in e.OldItems)
                        {
                            var descriptor = item.ExtendedProperties[this.owner] as DomainCategoryDescriptor;
                            descriptorList.Add(descriptor);
                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var item in descriptorList)
                            {
                                this.categories.Remove(item);
                            }
                        });
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {

                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            this.categories.Clear();
                        });
                    }
                    break;
            }
        }

        private async void DataBase_Loaded(object sender, EventArgs e)
        {
            this.IsActivated = true;
            await this.RefreshAsync();
        }

        private async void DataBase_Unloaded(object sender, EventArgs e)
        {
            this.IsActivated = false;
            await this.RefreshAsync();
        }

        #region IDomainCategoryDescriptor

        IDomainCategory IDomainCategoryDescriptor.Target => this.category;

        #endregion

        #region IDomainItemDescriptor

        IDomainItem IDomainItemDescriptor.Target => this.category as IDomainItem;

        #endregion
    }
}

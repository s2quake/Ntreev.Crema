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

using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Users.Properties;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Ntreev.Crema.Presentation.Users.PropertyItems.ViewModels
{
    abstract class EditorsViewModel : PropertyItemBase
    {
        private readonly ObservableCollection<DomainTreeItemBase> domains = new ObservableCollection<DomainTreeItemBase>();
        private readonly ICremaAppHost cremaAppHost;
        private readonly Authenticator authenticator;
        private IEnumerable users;
        private object selectedUser;
        private object descriptor;

        protected EditorsViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost)
            : base(cremaAppHost)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.DisplayName = Resources.Title_UsersEditingContent;
        }

        public abstract override bool CanSupport(object obj);

        public abstract string GetItemPath(object obj);

        public abstract string ItemType
        {
            get;
        }

        public override void SelectObject(object obj)
        {
            this.descriptor = obj;

            if (this.descriptor != null)
            {
                var itemPath = this.GetItemPath(this.descriptor);
                var itemType = this.ItemType;

                this.Users = null;
                foreach (var item in this.domains)
                {
                    if (item.DomainInfo.ItemPath == itemPath && item.DomainInfo.ItemType == itemType)
                    {
                        this.Users = item.Items;
                        break;
                    }
                }
                this.SelectedUser = null;
            }
            else
            {
                this.Users = null;
                this.SelectedUser = null;
            }
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        public override bool IsVisible => this.descriptor != null && this.users != null;

        public override object SelectedObject => this.descriptor;

        public IEnumerable Users
        {
            get => this.users;
            private set
            {
                this.users = value;
                this.NotifyOfPropertyChange(nameof(this.Users));
            }
        }

        public object SelectedUser
        {
            get => this.selectedUser;
            set
            {
                this.selectedUser = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedUser));
            }
        }

        private async void CremaAppHost_Opened(object sender, EventArgs e)
        {
            if (this.cremaAppHost.GetService(typeof(IDomainCollection)) is IDomainCollection domainCollection)
            {
                var items = await domainCollection.Dispatcher.InvokeAsync(() =>
                {
                    domainCollection.DomainsCreated += Domains_DomainsCreated;
                    domainCollection.DomainsDeleted += Domains_DomainsDeleted;
                    var domains = domainCollection.ToArray();
                    var itemList = new List<DomainTreeItemBase>(domains.Length);
                    foreach (var item in domains)
                    {
                        itemList.Add(item.Dispatcher.Invoke(() => new DomainTreeItemBase(this.authenticator, item, true, this)));
                    }
                    return itemList.ToArray();
                });

                foreach (var item in items)
                {
                    this.domains.Add(item);
                }
            }
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.domains.Clear();
        }

        private void Domains_DomainsDeleted(object sender, DomainsDeletedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in e.DomainInfos)
                {
                    for (var i = 0; i < this.domains.Count; i++)
                    {
                        if (this.domains[i].DomainID == item.DomainID)
                        {
                            this.domains.RemoveAt(i);
                            break;
                        }
                    }
                }
                this.SelectObject(this.descriptor);
            });
        }

        private void Domains_DomainsCreated(object sender, DomainsEventArgs e)
        {
            if (sender is IDomainCollection domainCollection)
            {
                foreach (var item in e.Domains)
                {
                    var domain = item;
                    var viewModel = domain.Dispatcher.Invoke(() => new DomainTreeItemBase(this.authenticator, domain, true, this));
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        this.domains.Add(viewModel);
                        this.SelectObject(this.descriptor);
                    });
                }
            }
        }
    }
}
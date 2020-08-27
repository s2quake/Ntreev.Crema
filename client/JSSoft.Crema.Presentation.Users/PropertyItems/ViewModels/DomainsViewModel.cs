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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Users.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Presentation.Users.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [ParentType("JSSoft.Crema.Presentation.Home.IPropertyService, JSSoft.Crema.Presentation.Home, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class DomainsViewModel : PropertyItemBase
    {
        private readonly ObservableCollection<DomainTreeItemBase> domains = new ObservableCollection<DomainTreeItemBase>();
        private readonly ICremaAppHost cremaAppHost;
        private readonly Authenticator authenticator;
        private DomainTreeItemBase selectedDomain;
        private IUserDescriptor descriptor;

        [ImportingConstructor]
        public DomainsViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost)
            : base(cremaAppHost)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.Domains = new ReadOnlyObservableCollection<DomainTreeItemBase>(this.domains);
            this.DisplayName = Resources.Title_UserDomainList;
        }

        public override bool CanSupport(object obj)
        {
            return obj is IUserDescriptor;
        }

        public override void SelectObject(object obj)
        {
            this.descriptor = obj as IUserDescriptor;
            if (this.descriptor != null)
            {
                foreach (var item in this.domains)
                {
                    item.IsVisible = item.ContainsUser(this.descriptor.UserID);
                }
                this.DisplayName = $"{Resources.Title_UserDomainList} [{this.descriptor.UserID}]";
            }
            else
            {
                foreach (var item in this.domains)
                {
                    item.IsVisible = false;
                }
                this.DisplayName = Resources.Title_UserDomainList;
            }
            this.SelectedDomain = null;
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        public override bool IsVisible
        {
            get
            {
                if (this.descriptor == null)
                    return false;
                foreach (var item in this.domains)
                {
                    if (item.IsVisible == true)
                        return true;
                }
                return false;
            }
        }

        public override object SelectedObject => this.descriptor;

        public ReadOnlyObservableCollection<DomainTreeItemBase> Domains { get; }

        public DomainTreeItemBase SelectedDomain
        {
            get => this.selectedDomain;
            set
            {
                this.selectedDomain = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedDomain));
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
            });
        }

        private void Domains_DomainsCreated(object sender, DomainsEventArgs e)
        {
            if (sender is IDomainCollection domainCollection)
            {
                foreach (var item in e.Domains)
                {
                    var domain = item;
                    if (domain.Dispatcher == null)
                        return;
                    var viewModel = domain.Dispatcher.Invoke(() => new DomainTreeItemBase(this.authenticator, domain, true, this));
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        this.domains.Add(viewModel);
                    });
                }
            }
        }
    }
}

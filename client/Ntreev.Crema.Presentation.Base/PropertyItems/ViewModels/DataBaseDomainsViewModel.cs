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

using Ntreev.Crema.Presentation.Base.Properties;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Base.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [ParentType(typeof(PropertyService))]
    class DataBaseDomainsViewModel : PropertyItemBase
    {
        private readonly ICremaAppHost cremaAppHost;
        private readonly ObservableCollection<DomainListItemBase> domains = new ObservableCollection<DomainListItemBase>();
        private readonly ReadOnlyObservableCollection<DomainListItemBase> domainsReadOnly;
        private DomainListItemBase selectedDomain;
        private IDataBaseDescriptor descriptor;

        [Import]
        private IBuildUp buildUp = null;
        [Import]
        private Authenticator authenticator = null;

        [ImportingConstructor]
        public DataBaseDomainsViewModel(ICremaAppHost cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += CremaAppHost_Opened;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.domainsReadOnly = new ReadOnlyObservableCollection<DomainListItemBase>(this.domains);
            this.DisplayName = Resources.Title_DataBaseDomainList;
        }

        public override bool CanSupport(object obj)
        {
            return obj is IDataBaseDescriptor;
        }

        public override void SelectObject(object obj)
        {
            this.descriptor = obj as IDataBaseDescriptor;
            if (this.descriptor != null)
            {
                foreach (var item in this.domains)
                {
                    item.IsVisible = this.descriptor.DataBaseID == item.DomainInfo.DataBaseID;
                }
            }
            else
            {
                foreach (var item in this.domains)
                {
                    item.IsVisible = false;
                }
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

        public ReadOnlyObservableCollection<DomainListItemBase> Domains => this.domainsReadOnly;

        public DomainListItemBase SelectedDomain
        {
            get => this.selectedDomain;
            set
            {
                if (this.selectedDomain != null)
                    this.selectedDomain.IsSelected = false;
                this.selectedDomain = value;
                if (this.selectedDomain != null)
                    this.selectedDomain.IsSelected = true;
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
                    var itemList = new List<DomainListItemBase>(domains.Length);
                    foreach (var item in domains)
                    {
                        itemList.Add(item.Dispatcher.Invoke(() => new DomainListItemBase(this.authenticator, item, true, this)));
                    }
                    return itemList.ToArray();
                });

                foreach (var item in items)
                {
                    this.buildUp?.BuildUp(item);
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
                    var dispatcher = domain.Dispatcher;
                    if (dispatcher == null)
                        return;
                    var viewModel = domain.Dispatcher.Invoke(() => new DomainListItemBase(this.authenticator, domain, true, this));
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        this.buildUp?.BuildUp(viewModel);
                        this.domains.Add(viewModel);
                    });
                }
            }
        }
    }
}

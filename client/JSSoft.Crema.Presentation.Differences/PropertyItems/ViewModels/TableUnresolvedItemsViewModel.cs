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

using JSSoft.Crema.Presentation.Differences.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Presentation.Differences.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [ParentType(typeof(PropertyService))]
    [Order(-1)]
    class TableUnresolvedItemsViewModel : PropertyItemBase
    {
        private TableTreeViewItemViewModel viewModel;

        [ImportingConstructor]
        public TableUnresolvedItemsViewModel(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            this.DisplayName = Resources.Title_UnresolvedItems;
        }

        public override bool IsVisible => this.Items.Any();

        public override object SelectedObject => this.viewModel;

        public override bool CanSupport(object obj)
        {
            return obj is TableTreeViewItemViewModel;
        }

        public ObservableCollection<TableUnresolvedItemListBoxItemViewModel> Items { get; } = new ObservableCollection<TableUnresolvedItemListBoxItemViewModel>();

        public override void SelectObject(object obj)
        {
            if (obj is TableTreeViewItemViewModel viewModel)
            {
                var query = from viewModelItem in this.GetViewModels(viewModel)
                            join unresolvedItem in viewModel.Source.UnresolvedItems on viewModelItem.Target equals unresolvedItem
                            select viewModelItem;

                this.Items.Clear();
                foreach (var item in query)
                {
                    var itemViewModel = new TableUnresolvedItemListBoxItemViewModel(this.ServiceProvider, item);
                    Items.Add(itemViewModel);
                    itemViewModel.PropertyChanged += ItemViewModel_PropertyChanged;
                }

                this.viewModel = viewModel;
            }
            else
            {
                this.Items.Clear();
                this.viewModel = null;
            }
            this.NotifyOfPropertyChange(nameof(this.DisplayName));
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.Items));
        }

        private void ItemViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TableUnresolvedItemListBoxItemViewModel viewModel && e.PropertyName == nameof(TemplateUnresolvedItemListBoxItemViewModel.IsResolved))
            {
                if (viewModel.IsResolved == true)
                {
                    this.Items.Remove(viewModel);
                    this.NotifyOfPropertyChange(nameof(this.IsVisible));
                }
            }
        }

        private IEnumerable<TreeViewItemViewModel> GetViewModels(TableTreeViewItemViewModel viewModel)
        {
            foreach (var item in EnumerableUtility.FamilyTree(viewModel.Browser.Items, i => i.Items))
            {
                yield return item;
            }
        }
    }
}

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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Presentation.Tables.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [Dependency(typeof(TableInfoViewModel))]
    [ParentType(typeof(PropertyService))]
    class BaseTableViewModel : PropertyItemBase, ISelector
    {
        private readonly Authenticator authenticator;
        private readonly Lazy<TableBrowserViewModel> browser;
        private ITableDescriptor descriptor;
        private TableListBoxItemViewModel[] tables;
        private TableListBoxItemViewModel selectedTable;

        [ImportingConstructor]
        public BaseTableViewModel(Authenticator authenticator, Lazy<TableBrowserViewModel> browser)
        {
            this.authenticator = authenticator;
            this.browser = browser;
        }

        public override bool CanSupport(object obj)
        {
            return obj is ITableDescriptor;
        }

        public override void SelectObject(object obj)
        {
            this.descriptor = obj as ITableDescriptor;

            if (this.descriptor != null)
            {
                var items = EnumerableUtility.Descendants<TreeViewItemViewModel, ITableDescriptor>(this.Browser.Items, item => item.Items)
                                             .Where(item => this.descriptor.TableInfo.TemplatedParent == item.TableInfo.Name)
                                             .ToArray();

                var viewModelList = new List<TableListBoxItemViewModel>();
                foreach (var item in items)
                {
                    var table = item.Target;
                    if (table.ExtendedProperties.ContainsKey(this) == true)
                    {
                        var descriptor = table.ExtendedProperties[this] as TableDescriptor;
                        viewModelList.Add(descriptor.Host as TableListBoxItemViewModel);
                    }
                    else
                    {
                        var viewModel = new TableListBoxItemViewModel(this.authenticator, item, this);
                        viewModelList.Add(viewModel);
                    }
                }
                this.Tables = viewModelList.ToArray();
            }
            else
            {
                this.Tables = new TableListBoxItemViewModel[] { };
            }

            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        public override bool IsVisible
        {
            get
            {
                if (this.descriptor == null)
                    return false;
                return this.tables?.Any() == true;
            }
        }

        public override object SelectedObject => this.descriptor;

        public TableListBoxItemViewModel[] Tables
        {
            get => this.tables;
            set
            {
                this.tables = value;
                this.NotifyOfPropertyChange(nameof(this.Tables));
            }
        }

        public TableListBoxItemViewModel SelectedTable
        {
            get => this.selectedTable;
            set
            {
                if (this.selectedTable != null)
                    this.selectedTable.IsSelected = false;
                this.selectedTable = value;
                if (this.selectedTable != null)
                    this.selectedTable.IsSelected = true;
                this.NotifyOfPropertyChange(nameof(this.SelectedTable));
            }
        }

        private TableBrowserViewModel Browser => this.browser.Value;

        #region ISelector

        object ISelector.SelectedItem
        {
            get => this.SelectedTable;
            set => this.SelectedTable = value as TableListBoxItemViewModel;
        }

        #endregion
    }
}

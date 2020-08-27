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
using Ntreev.Crema.Presentation.Types.BrowserItems.ViewModels;
using Ntreev.Crema.Presentation.Types.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using TypeDescriptor = Ntreev.Crema.Presentation.Framework.TypeDescriptor;

namespace Ntreev.Crema.Presentation.Types.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [Dependency("Ntreev.Crema.Presentation.Tables.PropertyItems.ViewModels.TableInfoViewModel, Ntreev.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("Ntreev.Crema.Presentation.Tables.IPropertyService, Ntreev.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class ReferencedTypesViewModel : PropertyItemBase, ISelector
    {
        private readonly Authenticator authenticator;
        private readonly Lazy<TypeBrowserViewModel> browser;
        private ITableDescriptor descriptor;
        private TypeListBoxItemViewModel[] types;
        private TypeListBoxItemViewModel selectedType;

        [ImportingConstructor]
        public ReferencedTypesViewModel(Authenticator authenticator, Lazy<TypeBrowserViewModel> browser)
        {
            this.authenticator = authenticator;
            this.browser = browser;
            this.DisplayName = Resources.Title_TypesBeingUsed;
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
                var types = EnumerableUtility.Descendants<TreeViewItemViewModel, ITypeDescriptor>(this.Browser.Items, item => item.Items);
                var query = from column in this.descriptor.TableInfo.Columns
                            join type in types on column.DataType equals (type.TypeInfo.CategoryPath + type.TypeInfo.Name)
                            select type;
                var descriptors = query.Distinct().ToArray();
                var viewModelList = new List<TypeListBoxItemViewModel>();

                foreach (var item in descriptors)
                {
                    var type = item.Target;
                    if (type.ExtendedProperties.ContainsKey(this) == true)
                    {
                        var descriptor = type.ExtendedProperties[this] as TypeDescriptor;
                        viewModelList.Add(descriptor.Host as TypeListBoxItemViewModel);
                    }
                    else
                    {
                        var viewModel = new TypeListBoxItemViewModel(this.authenticator, item, this);
                        viewModelList.Add(viewModel);
                    }
                }

                this.Types = viewModelList.ToArray();
            }
            else
            {
                this.Types = new TypeListBoxItemViewModel[] { };
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
                return this.types?.Any() == true;
            }
        }

        public override object SelectedObject => this.descriptor;

        public TypeListBoxItemViewModel[] Types
        {
            get => this.types;
            set
            {
                this.types = value;
                this.NotifyOfPropertyChange(nameof(this.Types));
            }
        }

        public TypeListBoxItemViewModel SelectedType
        {
            get => this.selectedType;
            set
            {
                if (this.selectedType != null)
                    this.selectedType.IsSelected = false;
                this.selectedType = value;
                if (this.selectedType != null)
                    this.selectedType.IsSelected = true;
                this.NotifyOfPropertyChange(nameof(this.SelectedType));
            }
        }

        private TypeBrowserViewModel Browser => this.browser.Value;

        #region ISelector

        object ISelector.SelectedItem
        {
            get => this.SelectedType;
            set => this.SelectedType = value as TypeListBoxItemViewModel;
        }

        #endregion
    }
}

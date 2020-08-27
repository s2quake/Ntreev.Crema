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

using JSSoft.Crema.Data.Diff;
using JSSoft.Crema.Presentation.Differences.BrowserItems.ViewModels;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.ComponentModel;

namespace JSSoft.Crema.Presentation.Differences.PropertyItems.ViewModels
{
    public class TemplateUnresolvedItemListBoxItemViewModel : ListBoxItemViewModel
    {
        private readonly object item;

        public TemplateUnresolvedItemListBoxItemViewModel(IServiceProvider serviceProvider, object item)
            : base(serviceProvider)
        {
            this.item = item;
            this.Target = item;
            if (this.item is TypeTreeViewItemViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        public void SelectInBrowserCommand()
        {
            if (this.item is TypeTreeViewItemViewModel viewModel)
            {
                //this.Dispatcher.InvokeAsync(() => this.shellViewModel.SelectedContent = this.typesViewModel);
                this.Dispatcher.InvokeAsync(() => viewModel.Browser.SelectedItem = viewModel, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        public override string DisplayName => $"{this.item}";

        public bool IsFlag
        {
            get
            {
                if (this.item is TypeTreeViewItemViewModel viewModel)
                {
                    return viewModel.IsFlag;
                }
                return false;
            }
        }

        public DiffState DiffState
        {
            get
            {
                if (this.item is TypeTreeViewItemViewModel viewModel)
                {
                    return viewModel.DiffState;
                }
                return DiffState.Unchanged;
            }
        }

        public bool IsResolved
        {
            get
            {
                if (this.item is TypeTreeViewItemViewModel viewModel)
                {
                    return viewModel.IsResolved;
                }
                return true;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TypeTreeViewItemViewModel && e.PropertyName == nameof(TypeTreeViewItemViewModel.IsResolved))
            {
                this.NotifyOfPropertyChange(nameof(this.IsResolved));
            }
        }
    }
}

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

using JSSoft.Crema.Data;
using JSSoft.Crema.Presentation.Differences.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Framework;
using System.ComponentModel;

namespace JSSoft.Crema.Presentation.Differences.PropertyItems.ViewModels
{
    abstract class TypeInfoViewModel : PropertyItemBase
    {
        private TypeTreeViewItemViewModel viewModel;
        private TypeInfo? typeInfo;

        public TypeInfoViewModel()
        {
            this.DisplayName = Resources.Title_TypeInfo;
        }

        public TypeInfo? TypeInfo => this.typeInfo;

        public override bool IsVisible => this.typeInfo != null;

        public override object SelectedObject => this.viewModel;

        public override bool CanSupport(object obj)
        {
            return obj is TypeTreeViewItemViewModel;
        }

        public override void SelectObject(object obj)
        {
            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            this.viewModel = obj as TypeTreeViewItemViewModel;
            if (this.viewModel != null)
            {
                this.DisplayName = $"{Resources.Title_TypeInfo}({this.GetHeader(this.viewModel)})";
                this.typeInfo = this.GetTypeInfo(this.viewModel);
                this.viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
            else
            {
                this.typeInfo = null;
            }

            this.NotifyOfPropertyChange(nameof(this.TypeInfo));
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        public bool IsLeft
        {
            get; set;
        }

        protected abstract TypeInfo? GetTypeInfo(TypeTreeViewItemViewModel item);

        protected abstract string GetHeader(TypeTreeViewItemViewModel item);

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TypeTreeViewItemViewModel.IsResolved) || e.PropertyName == string.Empty)
            {
                this.Dispatcher.InvokeAsync(() => this.SelectObject(this.viewModel));
            }
        }
    }
}

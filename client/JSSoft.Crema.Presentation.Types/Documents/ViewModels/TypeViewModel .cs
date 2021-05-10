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

using JSSoft.Crema.Data;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Threading;

namespace JSSoft.Crema.Presentation.Types.Documents.ViewModels
{
    [Export(typeof(ITypeDocument))]
    class TypeViewModel : DocumentBase, ITypeDocument
    {
        private readonly Authentication authentication;
        private CremaDataType dataType;
        private object selectedItem;
        private string selectedColumn;

        public TypeViewModel(Authentication authentication, IType type)
        {
            this.authentication = authentication;
            this.Target = type;
            this.Initialize();
        }

        public IType Target { get; private set; }

        public CremaDataType Source
        {
            get => this.dataType;
            private set
            {
                this.dataType = value;
                this.IsModified = false;
                this.NotifyOfPropertyChange(nameof(this.Source));
            }
        }

        public object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
            }
        }

        public int SelectedItemIndex
        {
            set
            {
                if (value >= 0 && value < this.dataType.View.Count)
                {
                    var item = this.dataType.View[value];
                    this.SelectedItem = item;
                }
                else
                {
                    this.SelectedItem = null;
                }
            }
        }

        public string SelectedColumn
        {
            get => this.selectedColumn;
            set
            {
                this.selectedColumn = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedColumn));
            }
        }

        public override string DisplayName
        {
            get
            {
                if (this.Source == null)
                    return string.Empty;
                return this.Source.TypeName;
            }
        }

        protected override async void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);
            await this.Target.Dispatcher.InvokeAsync(() =>
            {
                this.Target.TypeInfoChanged -= Type_TypeInfoChanged;
            });
        }

        private async void Initialize()
        {
            this.BeginProgress();
            var dataSet = await this.Target.GetDataSetAsync(this.authentication, null);
            var itemsSource = await this.Target.Dispatcher.InvokeAsync(() =>
             {
                 this.Target.TypeInfoChanged += Type_TypeInfoChanged;
                 return dataSet.Types.FirstOrDefault();
             });
            this.Source = itemsSource;
            this.EndProgress();
            this.Refresh();
        }

        private async void Type_TypeInfoChanged(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.BeginProgress();
            });

            var dataSet = await this.Target.GetDataSetAsync(this.authentication, null);

            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Source = dataSet.Types.FirstOrDefault();
                this.Refresh();
                this.EndProgress();
            }, DispatcherPriority.Background);
        }
    }
}

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
using JSSoft.Crema.Data.Diff;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Comparer.Templates.ViewModels
{
    class TemplateDocumentViewModel : DocumentBase
    {
        private readonly TemplateTreeViewItemViewModel viewModel;
        private readonly List<TemplateDocumentItemViewModel> itemList = new List<TemplateDocumentItemViewModel>();
        private TemplateDocumentItemViewModel selectedItem;
        private UndoService undoService = new UndoService();
        private ICommand resolveCommand;

        public TemplateDocumentViewModel(TemplateTreeViewItemViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.viewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.itemList.Add(new TemplateDocumentItemViewModel(viewModel));
            foreach (var item in viewModel.Items.OfType<TemplateTreeViewItemViewModel>())
            {
                this.itemList.Add(new TemplateDocumentItemViewModel(item));
            }
            foreach (var item in this.itemList)
            {
                item.PropertyChanged += DocumentItem_PropertyChanged;
            }
            this.undoService.Changed += UndoService_Changed;
            this.resolveCommand = new DelegateCommand(async (p) => await this.ResolveAsync(), (p) => this.CanResolve);
            this.SelectedItem = this.itemList.First();
            this.DisplayName = viewModel.DisplayName;
        }

        public async Task ResolveAsync()
        {
            try
            {
                this.Source.ResolveAll();
                this.undoService.Clear();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public override string ToString()
        {
            return this.viewModel.ToString();
        }

        public DiffTemplate Source
        {
            get { return this.viewModel.Source; }
        }

        public IEnumerable<TemplateDocumentItemViewModel> ItemsSource
        {
            get { return this.itemList; }
        }

        public TemplateDocumentItemViewModel SelectedItem
        {
            get { return this.selectedItem; }
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
            }
        }

        public string SelectedName
        {
            get { return $"{this.selectedItem}"; }
            set
            {
                var query = from item in this.itemList
                            where $"{item}" == value
                            select item;

                if (query.Any() == false)
                    return;

                this.SelectedItem = query.First();
            }
        }

        public ICommand ResolveCommand => this.resolveCommand;

        public IUndoService UndoService
        {
            get { return this.undoService; }
        }

        public bool CanResolve
        {
            get
            {
                return this.Source.IsResolved == false && this.Source.UnresolvedItems.Any() == false;
            }
        }

        protected override async Task<bool> CloseAsync()
        {
            return await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.IsModified == false)
                {
                    foreach (var item in this.itemList)
                    {
                        item.Source.RejectChanges();
                    }
                }
                else
                {
                    foreach (var item in this.itemList)
                    {
                        item.Source.AcceptChanges();
                    }
                }
                return true;
            });
        }

        private void DocumentItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.IsModified = this.itemList.Where(item => item.IsModified).Any();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TemplateTreeViewItemViewModel.DisplayName) || e.PropertyName == string.Empty)
            {
                this.DisplayName = this.viewModel.DisplayName;
            }
        }

        private void UndoService_Changed(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this.itemList)
                {
                    item.RefreshModifiedState();
                }
            });
        }
    }
}

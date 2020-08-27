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

using JSSoft.Crema.Presentation.SmartSet.Dialogs.ViewModels;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.SmartSet.BrowserItems.ViewModels
{
    class SmartSetCategoryTreeViewItemViewModel : TreeViewItemViewModel
    {
        private readonly ISmartSetCategory category;
        private readonly SmartSetBrowserViewModel browser;

        public SmartSetCategoryTreeViewItemViewModel(ISmartSetCategory category, SmartSetBrowserViewModel browser)
        {
            this.category = category;
            this.browser = browser;
            this.Target = category;

            foreach (var item in category.Categories)
            {
                this.AddViewModel(item);
            }

            foreach (var item in category.Items)
            {
                this.AddViewModel(item);
            }

            this.category.Renamed += Category_Renamed;
            this.category.Items.CollectionChanged += Category_ItemsChanged;
            this.category.Categories.CollectionChanged += Category_CategoriesChanged;
        }

        public async Task NewFolderAsync()
        {
            var dialog = new NewCategoryViewModel(this.category.Path, item => this.category.Categories.ContainsKey(item) == false);
            if (await dialog.ShowDialogAsync() != true)
                return;

            try
            {
                this.category.CreateCategory(dialog.CategoryName);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public async Task NewSmartSetAsync()
        {
            var dialog = new SmartSetEditViewModel(this.browser.Rules);
            if (await dialog.ShowDialogAsync() != true)
                return;

            try
            {
                var smartSet = this.category.CreateItem(dialog.SmartSetName);
                smartSet.RuleItems = dialog.RuleItems;
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public async Task RenameAsync()
        {
            var dialog = new RenameViewModel(this.category.Name, item => this.category.Parent.Categories.ContainsKey(item) == false);
            if (await dialog.ShowDialogAsync() != true)
                return;

            try
            {
                this.category.Name = dialog.NewName;
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

#pragma warning disable CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다. 'await' 연산자를 사용하여 비블로킹 API 호출을 대기하거나, 'await Task.Run(...)'을 사용하여 백그라운드 스레드에서 CPU 바인딩된 작업을 수행하세요.
        public async Task MoveAsync()
#pragma warning restore CS1998 // 이 비동기 메서드에는 'await' 연산자가 없으며 메서드가 동시에 실행됩니다. 'await' 연산자를 사용하여 비블로킹 API 호출을 대기하거나, 'await Task.Run(...)'을 사용하여 백그라운드 스레드에서 CPU 바인딩된 작업을 수행하세요.
        {

        }

        public async Task DeleteAsync()
        {
            if (await AppMessageBox.ConfirmDeleteAsync() == false)
                return;

            try
            {
                this.category.Dispose();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public override int CompareTo(object obj)
        {
            if (object.ReferenceEquals(this, obj) == true)
                return 0;
            else if (obj is SmartSetTreeViewItemViewModel == true)
                return -1;
            return base.CompareTo(obj);
        }

        public override string DisplayName => category.Name;

        private void AddViewModel(ISmartSet smartSet)
        {
            var viewModel = new SmartSetTreeViewItemViewModel(smartSet, this.browser)
            {
                Parent = this
            };
        }

        private void RemoveViewModel(ISmartSet smartSet)
        {
            var viewModel = this.Items.FirstOrDefault(item => item.Target == smartSet);
            if (viewModel == null)
                return;
            viewModel.Parent = null;
        }

        private void RepositionViewModel(ISmartSet smartSet)
        {
            var viewModel = this.Items.FirstOrDefault(item => item.Target == smartSet);
            if (viewModel == null)
                return;
            this.Items.Reposition(viewModel);
        }

        private void AddViewModel(ISmartSetCategory category)
        {
            var viewModel = new SmartSetCategoryTreeViewItemViewModel(category, this.browser)
            {
                Parent = this
            };
        }

        private void RemoveViewModel(ISmartSetCategory category)
        {
            var viewModel = this.Items.FirstOrDefault(item => item.Target == category);
            if (viewModel == null)
                return;
            viewModel.Parent = null;
        }

        private void RepositionViewModel(ISmartSetCategory category)
        {
            var viewModel = this.Items.FirstOrDefault(item => item.Target == category);
            if (viewModel == null)
                return;
            this.Items.Reposition(viewModel);
        }

        private void RefreshTableViewModels()
        {
            this.Items.Clear();
        }

        private void Category_Renamed(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(this.DisplayName));
        }

        private async void Category_ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (ISmartSet item in e.NewItems)
                            {
                                this.AddViewModel(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (ISmartSet item in e.OldItems)
                            {
                                this.RemoveViewModel(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (ISmartSet item in e.NewItems)
                            {
                                this.RepositionViewModel(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            var query = from vm in this.Items
                                        where vm is BookmarkTableTreeViewItemViewModel
                                        select vm;
                            this.Items.RemoveRange(query);
                        }
                        break;
                }
            });
        }

        private async void Category_CategoriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (ISmartSetCategory item in e.NewItems)
                            {
                                this.AddViewModel(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (ISmartSetCategory item in e.OldItems)
                            {
                                this.RemoveViewModel(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                        {
                            foreach (ISmartSetCategory item in e.NewItems)
                            {
                                this.RepositionViewModel(item);
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        {
                            var query = from vm in this.Items
                                        where vm is ModernUI.Framework.ViewModels.CategoryTreeViewItemViewModel
                                        select vm;
                            this.Items.RemoveRange(query);
                        }
                        break;
                }
            });
        }
    }
}

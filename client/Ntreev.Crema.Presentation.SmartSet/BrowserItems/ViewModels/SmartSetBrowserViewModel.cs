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
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ntreev.Crema.Presentation.SmartSet.BrowserItems.ViewModels
{
    abstract class SmartSetBrowserViewModel : TreeViewBase, ISmartSetBrowser, IPartImportsSatisfiedNotification
    {
        private readonly ICremaAppHost cremaAppHost;

        private readonly IRule[] rules;
        private readonly BookmarkRootTreeViewItemViewModel bookmarkCategory;

        [Import]
        private readonly IAppConfiguration configs = null;
        [Import]
        private readonly IBuildUp buildUp = null;

        private bool isVisible = true;

        private readonly DelegateCommand renameCommand;
        private readonly DelegateCommand deleteCommand;

        protected SmartSetBrowserViewModel(ICremaAppHost cremaAppHost, IEnumerable<IRule> rules)
        {
            this.rules = rules.OrderBy(item => item.DisplayName).ToArray();
            this.isVisible = true;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Loaded += CremaAppHost_Loaded;
            this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
            this.bookmarkCategory = this.CreateBookmarkRootViewModel();
            this.renameCommand = new DelegateCommand(this.Rename_Execute, this.Rename_CanExecute);
            this.deleteCommand = new DelegateCommand(this.Delete_Execute, this.Delete_CanExecute);
        }

        public string[] GetBookmarkCategoryPaths()
        {
            var query = from item in TreeViewItemViewModel.FamilyTree(this.bookmarkCategory)
                        where item is BookmarkRootTreeViewItemViewModel || item is BookmarkCategoryTreeViewItemViewModel
                        select TreeViewItemViewModel.BuildRelativePath(this.bookmarkCategory, item);
            return query.ToArray();
        }

        public string[] GetBookmarkItemPaths()
        {
            var query = from item in TreeViewItemViewModel.FamilyTree(this.bookmarkCategory)
                        select TreeViewItemViewModel.BuildRelativePath(this.bookmarkCategory, item, this.IsCategory);
            return query.ToArray();
        }

        private bool IsCategory(TreeViewItemViewModel viewModel)
        {
            return viewModel is BookmarkRootTreeViewItemViewModel || viewModel is BookmarkCategoryTreeViewItemViewModel;
        }

        public void AddBookmarkItem(string parentPath, object item)
        {
            var parent = this.GetBookmarkItem(parentPath);
            var viewModel = this.CreateTreeViewItemViewModel(parent, item);
            parent.Items.Add(viewModel);
            parent.IsExpanded = true;
            this.UpdateBookmarkItems();
            this.Dispatcher.InvokeAsync(() =>
            {
                this.SelectedItem = viewModel;
            });
        }

        public TreeViewItemViewModel GetBookmarkItem(string path)
        {
            var items = TreeViewItemViewModel.FamilyTree(this.bookmarkCategory);

            foreach (var item in items)
            {
                var itemmPath = TreeViewItemViewModel.BuildRelativePath(this.bookmarkCategory, item);
                if (path == itemmPath)
                    return item;
            }

            return null;
        }

        internal void UpdateBookmarkItems()
        {
            var query = from item in TreeViewItemViewModel.Descendants(this.bookmarkCategory)
                        select TreeViewItemViewModel.BuildRelativePath(this.bookmarkCategory, item, IsCategory);
            this.BookmarkItems = query.ToArray();

            static bool IsCategory(TreeViewItemViewModel viewModel)
            {
                return viewModel is BookmarkRootTreeViewItemViewModel || viewModel is BookmarkCategoryTreeViewItemViewModel;
            }
        }

        public async Task NewSmartSetTask()
        {
            var viewModel = this.SelectedItem as SmartSetCategoryTreeViewItemViewModel;
            await viewModel.NewSmartSetAsync();
        }

        public void SelectItem(TreeViewItemViewModel item)
        {
            this.SelectedItem = item;
            this.NotifyOfPropertyChange(nameof(this.CanNewSmartSet));
            this.NotifyOfPropertyChange(nameof(this.SelectedItem));
            this.OnSelectionChanged(EventArgs.Empty);
        }

        public bool IsVisible
        {
            get { return this.isVisible; }
            set
            {
                this.isVisible = value;
                this.NotifyOfPropertyChange(nameof(this.IsVisible));
            }
        }

        public bool CanNewSmartSet
        {
            get { return this.SelectedItem is SmartSetCategoryTreeViewItemViewModel; }
        }

        public BookmarkRootTreeViewItemViewModel BookmarkCategory
        {
            get { return this.bookmarkCategory; }
        }

        public IRule[] Rules
        {
            get { return this.rules; }
        }

        public abstract ISmartSetCategory Root
        {
            get;
        }

        public abstract TreeViewItemViewModel CreateTreeViewItemViewModel(TreeViewItemViewModel parent, object item);

        public abstract BookmarkRootTreeViewItemViewModel CreateBookmarkRootViewModel();

        public ICommand RenameCommand
        {
            get { return this.renameCommand; }
        }

        public ICommand DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        private void InitializeBookmarkItems()
        {
            foreach (var item in this.BookmarkItems)
            {
                if (NameValidator.VerifyCategoryPath(item) == true)
                {
                    var categoryName = new CategoryName(item);
                    var parent = this.GetBookmarkItem(categoryName.ParentPath);
                    var viewModel = this.bookmarkCategory.CreateInstance(categoryName, this);
                    viewModel.Parent = parent;
                }
                else
                {
                    var itemName = new ItemName(item);
                    var parent = this.GetBookmarkItem(itemName.CategoryPath);
                    var viewModel = this.CreateTreeViewItemViewModel(parent, item);
                    if (viewModel != null)
                    {
                        viewModel.Parent = parent;
                    }
                    else
                    {
                        new InvalidTreeViewItemViewModel(itemName, this.DeleteInvalidBookmark).Parent = parent;
                    }
                }
            }
        }

        protected virtual void OnLoaded(EventArgs e)
        {

        }

        protected virtual void OnUnloaded(EventArgs e)
        {

        }

        protected abstract string[] BookmarkItems
        {
            get; set;
        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            this.Items.Add(this.bookmarkCategory);
            this.Items.Add(new SmartSetContextTreeViewItemViewModel(this.Root, this));
            foreach (var item in this.Items)
            {
                this.buildUp.BuildUp(item);
            }
            this.OnLoaded(EventArgs.Empty);
            this.Dispatcher.InvokeAsync((System.Action)(() =>
            {
                this.InitializeBookmarkItems();
                this.configs.Update(this);
            }));
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            this.configs.Commit(this);
            this.bookmarkCategory.Items.Clear();
            this.Items.Clear();
            this.OnUnloaded(EventArgs.Empty);
        }

        private void Delete_Execute(object parameter)
        {
            //if (parameter is TypeTreeViewItemViewModel)
            //{
            //    (parameter as TypeTreeViewItemViewModel).DeleteCommand.Execute(parameter);
            //}
            //else if (parameter is CategoryTreeViewItemViewModel)
            //{
            //    (parameter as CategoryTreeViewItemViewModel).DeleteCommand.Execute(parameter);
            //}
        }

        private bool Delete_CanExecute(object parameter)
        {
            //if (parameter is DataBaseTreeViewItemViewModel)
            //    return false;

            //if (parameter is TypeTreeViewItemViewModel)
            //    return (parameter as TypeTreeViewItemViewModel).DeleteCommand.CanExecute(parameter);

            //if (parameter is CategoryTreeViewItemViewModel)
            //    return (parameter as CategoryTreeViewItemViewModel).DeleteCommand.CanExecute(parameter);

            return false;
        }

        private void Rename_Execute(object parameter)
        {
            //if (parameter is TypeTreeViewItemViewModel)
            //{
            //    (parameter as TypeTreeViewItemViewModel).RenameCommand.Execute(parameter);
            //}
            //else if (parameter is CategoryTreeViewItemViewModel)
            //{
            //    (parameter as CategoryTreeViewItemViewModel).RenameCommand.Execute(parameter);
            //}
        }

        private bool Rename_CanExecute(object parameter)
        {
            //if (parameter is DataBaseTreeViewItemViewModel)
            //    return false;

            //if (parameter is TypeTreeViewItemViewModel)
            //    return (parameter as TypeTreeViewItemViewModel).RenameCommand.CanExecute(parameter);

            //if (parameter is CategoryTreeViewItemViewModel)
            //    return (parameter as CategoryTreeViewItemViewModel).RenameCommand.CanExecute(parameter);

            return false;
        }

        private void DeleteInvalidBookmark(InvalidTreeViewItemViewModel viewModel)
        {
            viewModel.Parent = null;
            this.UpdateBookmarkItems();
        }

        [ConfigurationProperty(ScopeType = typeof(IUserConfiguration))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:사용되지 않는 private 멤버 제거", Justification = "<보류 중>")]
        private string[] Settings
        {
            get => this.GetSettings();
            set => this.SetSettings(value);
        }

        #region IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            this.buildUp.BuildUp(this.bookmarkCategory);
        }

        #endregion
    }
}

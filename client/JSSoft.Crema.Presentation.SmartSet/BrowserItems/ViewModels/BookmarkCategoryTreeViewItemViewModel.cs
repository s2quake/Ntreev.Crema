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

using Ntreev.Crema.Presentation.SmartSet.Dialogs.ViewModels;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
using Ntreev.ModernUI.Framework.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.SmartSet.BrowserItems.ViewModels
{
    abstract class BookmarkCategoryTreeViewItemViewModel : TreeViewItemViewModel
    {
        private CategoryName categoryName;

        protected BookmarkCategoryTreeViewItemViewModel(string path, SmartSetBrowserViewModel browser)
        {
            NameValidator.ValidateCategoryPath(path);
            this.categoryName = new CategoryName(path);
            this.Target = path;
            this.Owner = browser;
        }

        public async Task NewFolderAsync()
        {
            var query = from item in this.Items
                        where item is BookmarkCategoryTreeViewItemViewModel
                        let viewModel = item as BookmarkCategoryTreeViewItemViewModel
                        select viewModel.DisplayName;

            var dialog = new NewCategoryViewModel(PathUtility.Separator, query.ToArray());
            if (await dialog.ShowDialogAsync() != true)
                return;

            try
            {
                var viewModel = this.Browser.BookmarkCategory.CreateInstance(dialog.CategoryPath, this.Browser);
                this.Items.Add(viewModel);
                this.Items.Reposition(viewModel);
                this.IsExpanded = true;
                this.Browser.UpdateBookmarkItems();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        // 북마크 폴더 삭제
        public async Task DeleteAsync()
        {
            var dialog = new DeleteViewModel();
            if (await dialog.ShowDialogAsync() == true)
            {
                this.Parent.Items.Remove(this);
                this.Browser.UpdateBookmarkItems();
            }
        }

        // 북마크 폴더 이름 변경
        public async Task RenameAsync()
        {
            var categoryPaths = this.Browser.GetBookmarkCategoryPaths();
            var dialog = new RenameBookmarkCategoryViewModel(this.categoryName.Path, categoryPaths);
            if (await dialog.ShowDialogAsync() != true)
                return;

            try
            {
                this.categoryName.Name = dialog.NewName;
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
                this.Parent.Items.Reposition(this);
                this.Browser.UpdateBookmarkItems();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        // 북마크 폴더 이동
        public async Task MoveAsync()
        {
            var itemPaths = this.Browser.GetBookmarkItemPaths();
            var dialog = new MoveBookmarkItemViewModel(this.ServiceProvider, this.Path, itemPaths);
            if (await dialog.ShowDialogAsync() != true)
                return;

            try
            {
                var parentViewModel = this.Browser.GetBookmarkItem(dialog.TargetPath);
                this.Parent = parentViewModel;
                this.categoryName = new CategoryName(dialog.TargetPath, this.categoryName.Name);
                this.Browser.UpdateBookmarkItems();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public override string DisplayName => this.categoryName.Name;

        public string DisplayPath => this.categoryName.Path;

        public string Path => this.categoryName.Path;

        public string Name => categoryName.Name;

        public override int Order => 1;

        public SmartSetBrowserViewModel Browser => this.Owner as SmartSetBrowserViewModel;
    }
}

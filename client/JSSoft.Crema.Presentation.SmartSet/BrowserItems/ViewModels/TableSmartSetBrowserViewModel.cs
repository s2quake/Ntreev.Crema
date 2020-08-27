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
using JSSoft.Crema.Presentation.SmartSet.Properties;
using JSSoft.Crema.Presentation.Tables;
using JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Presentation.SmartSet.BrowserItems.ViewModels
{
    [Export(typeof(IBrowserItem))]
    [Export(typeof(TableSmartSetBrowserViewModel))]
    [RequiredAuthority(Authority.Guest)]
    [Dependency(typeof(ITableBrowser))]
    [ParentType(typeof(Tables.IBrowserService))]
    class TableSmartSetBrowserViewModel : SmartSetBrowserViewModel
    {
        private readonly Authenticator authenticator;
        private readonly TableSmartSetContext smartSetContext;
        private readonly Lazy<ITableBrowser> tableBrowser;
        private readonly Lazy<JSSoft.Crema.Presentation.Tables.IPropertyService> propertyService;

        [ImportingConstructor]
        public TableSmartSetBrowserViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost, [ImportMany] IEnumerable<IRule> rules,
            TableSmartSetContext smartSetContext, Lazy<ITableBrowser> tableBrowser, Lazy<Tables.IPropertyService> propertyService)
            : base(cremaAppHost, rules.Where(item => item.SupportType == typeof(ITableDescriptor)))
        {
            this.authenticator = authenticator;
            this.smartSetContext = smartSetContext;
            this.smartSetContext.BookmarkChanged += SmartSetContext_BookmarkChanged;
            this.tableBrowser = tableBrowser;
            this.propertyService = propertyService;
            this.DisplayName = Resources.Title_TableSmartCollection;
            this.Dispatcher.InvokeAsync(() => this.AttachPropertyService(this.propertyService.Value));
        }

        public override TreeViewItemViewModel CreateTreeViewItemViewModel(TreeViewItemViewModel parent, object item)
        {
            if (item is string path)
            {
                item = this.GetDescriptor(path);
            }

            if (item is ITableDescriptor descriptor)
            {
                if (parent is BookmarkRootTreeViewItemViewModel || parent is BookmarkCategoryTreeViewItemViewModel)
                    return new BookmarkTableTreeViewItemViewModel(this.authenticator, descriptor, this);
                return new TableTreeViewItemViewModel(this.authenticator, new TableDescriptor(this.authenticator, descriptor, true, this), this);
            }

            return null;
        }

        public override BookmarkRootTreeViewItemViewModel CreateBookmarkRootViewModel()
        {
            return new BookmarkTableRootTreeViewItemViewModel(this);
        }

        public override ISmartSetCategory Root => this.smartSetContext.Root;

        protected override void OnLoaded(EventArgs e)
        {
            base.OnLoaded(e);
            this.SyncBookmarkItems();
        }

        protected override void OnUnloaded(EventArgs e)
        {
            base.OnUnloaded(e);
            this.smartSetContext.Clear();
        }

        protected override string[] BookmarkItems
        {
            get => this.smartSetContext.BookmarkItems;
            set => this.smartSetContext.BookmarkItems = value;
        }

        private ITableDescriptor GetDescriptor(string path)
        {
            var itemName = new ItemName(path);
            var items = EnumerableUtility.Descendants<TreeViewItemViewModel, ITableDescriptor>(this.TableBrowser.Items.OfType<TreeViewItemViewModel>(), item => item.Items);
            foreach (var item in items)
            {
                if (item.Name == itemName.Name)
                    return item;
            }
            return null;
        }

        private void SmartSetContext_BookmarkChanged(object sender, EventArgs e)
        {
            this.SyncBookmarkItems();
        }

        private void SyncBookmarkItems()
        {
            //var tableBrowser = this.tableBrowser.Value;
            //var bookmarkItems = this.smartSetContext.BookmarkItems.ToDictionary(item => item);

            //var descriptors = EnumerableUtility.Descendants<TreeViewItemViewModel, ITableDescriptor>(tableBrowser.Items.OfType<TreeViewItemViewModel>(), item => item.ItemsSource)
            //                                   .ToDictionary(item => item.TableInfo.Name);

            //var rootCategory = this.BookmartCategory.Browser.Root;
            //var arrBookmartCategory = this.BookmartCategory.ItemsSource.ToArray();
            //foreach (var item in arrBookmartCategory)
            //{
            //    if (bookmarkItems.ContainsKey(item.DisplayName) == false)
            //        item.Parent = null;
            //}

            //var query = from item in this.smartSetContext.BookmarkItems
            //            where NameValidator.VerifyCategoryPath(item)
            //            orderby item
            //            select item;

            //var rootPath = BookmarkCategoryTreeViewItemViewModel.FromItems(query.ToArray());
            //this.buildUp.BuildUp(rootPath);

            //foreach (var item in rootPath.ItemsSource)
            //{
            //    this.BookmartCategory.ItemsSource.Add(item);
            //}

            //var dic = EnumerableUtility.Descendants<TreeViewItemViewModel>(this.BookmartCategory, item => item.ItemsSource).ToDictionary(item => item.Target as string);

            //foreach (var item in this.smartSetContext.BookmarkItems)
            //{
            //    if (NameValidator.VerifyItemPath(item) == true)
            //    {
            //        var itemName = new ItemName(item);
            //        if (itemName.CategoryPath == PathUtility.Separator)
            //        {
            //            var descriptor = descriptors[itemName.Name];
            //            var viewModel = this.CreateTreeViewItemViewModel(descriptor);
            //            this.BookmartCategory.ItemsSource.Add(viewModel);
            //        }
            //        else if (dic.ContainsKey(itemName.CategoryPath) == true)
            //        {
            //            var categoryViewModel = dic[itemName.CategoryPath];
            //            if (descriptors.ContainsKey(itemName.Name) == true)
            //            {
            //                var descriptor = descriptors[itemName.Name];
            //                var viewModel = this.CreateTreeViewItemViewModel(descriptor);
            //                categoryViewModel.ItemsSource.Add(viewModel);
            //            }
            //            else
            //            {
            //                var viewModel = new InvalidTreeViewItemViewModel(itemName.Path, this.DeleteInvalidBookmark);
            //                categoryViewModel.ItemsSource.Add(viewModel);
            //            }
            //        }
            //    }
            //}
        }

        private void DeleteInvalidBookmark(InvalidTreeViewItemViewModel viewModel)
        {
            viewModel.Parent.Items.Remove(viewModel);
            this.smartSetContext.UpdateBookmarkItems();
        }

        private ITableBrowser TableBrowser => this.tableBrowser.Value;
    }
}

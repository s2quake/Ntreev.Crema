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
using JSSoft.Crema.Presentation.Types;
using JSSoft.Crema.Presentation.Types.BrowserItems.ViewModels;
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
using System.Windows.Threading;

namespace JSSoft.Crema.Presentation.SmartSet.BrowserItems.ViewModels
{
    [Export(typeof(IBrowserItem))]
    [Export(typeof(TypeSmartSetBrowserViewModel))]
    [RequiredAuthority(Authority.Guest)]
    [Dependency(typeof(ITypeBrowser))]
    [ParentType(typeof(Types.IBrowserService))]
    class TypeSmartSetBrowserViewModel : SmartSetBrowserViewModel
    {
        private readonly Authenticator authenticator;
        private readonly TypeSmartSetContext smartSetContext;
        private readonly Lazy<ITypeBrowser> typeBrowser = null;
        private readonly Lazy<JSSoft.Crema.Presentation.Types.IPropertyService> propertyService;

        [ImportingConstructor]
        public TypeSmartSetBrowserViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost, [ImportMany] IEnumerable<IRule> rules, 
            TypeSmartSetContext smartSetContext, Lazy<ITypeBrowser> typeBrowser, Lazy<Types.IPropertyService> propertyService)
            : base(cremaAppHost, rules.Where(item => item.SupportType == typeof(ITypeDescriptor)))
        {
            this.smartSetContext = smartSetContext;
            this.smartSetContext.BookmarkChanged += SmartSetContext_BookmarkChanged;
            this.DisplayName = Resources.Title_TypeSmartCollection;
            this.Dispatcher.InvokeAsync(() => this.AttachPropertyService(this.propertyService.Value));
            this.authenticator = authenticator;
            this.typeBrowser = typeBrowser;
            this.propertyService = propertyService;
        }

        public override TreeViewItemViewModel CreateTreeViewItemViewModel(TreeViewItemViewModel parent, object item)
        {
            if (item is string path)
            {
                item = this.GetDescriptor(path);
            }

            if (item is ITypeDescriptor descriptor)
            {
                if (parent is BookmarkRootTreeViewItemViewModel || parent is BookmarkCategoryTreeViewItemViewModel)
                    return new BookmarkTypeTreeViewItemViewModel(this.authenticator, descriptor, this);
                return new TypeTreeViewItemViewModel(this.authenticator, new TypeDescriptor(this.authenticator, descriptor, true, this), this);
            }

            return null;
        }

        public override BookmarkRootTreeViewItemViewModel CreateBookmarkRootViewModel()
        {
            return new BookmarkTypeRootTreeViewItemViewModel(this);
        }

        public override ISmartSetCategory Root => this.smartSetContext.Root;

        protected async override void OnLoaded(EventArgs e)
        {
            base.OnLoaded(e);
            await this.Dispatcher.InvokeAsync(() => this.SyncBookmarkItems(), DispatcherPriority.ApplicationIdle);
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

        private ITypeDescriptor GetDescriptor(string path)
        {
            var itemName = new ItemName(path);
            var items = EnumerableUtility.Descendants<TreeViewItemViewModel, ITypeDescriptor>(this.TypeBrowser.Items.OfType<TreeViewItemViewModel>(), item => item.Items);
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
            //var typeBrowser = this.typeBrowser.Value;
            //var bookmarks = this.smartSetContext.BookmarkItems.ToDictionary(item => item);

            //var descriptors = EnumerableUtility.Descendants<TreeViewItemViewModel, ITypeDescriptor>(typeBrowser.Items.OfType<TreeViewItemViewModel>(), item => item.ItemsSource)
            //                                   .ToDictionary(item => item.TypeInfo.Name);

            //foreach (var item in this.BookmarkCategory.ItemsSource.ToArray())
            //{
            //    if (bookmarks.ContainsKey(item.DisplayName) == false)
            //        item.Parent = null;
            //}

            //foreach (var item in this.smartSetContext.BookmarkItems)
            //{
            //    if (descriptors.ContainsKey(item) == true)
            //    {
            //        var descriptor = descriptors[item];

            //        if (this.BookmarkCategory.ItemsSource.Any(i => i.Target == descriptor.Target) == true)
            //            continue;

            //        this.BookmarkCategory.ItemsSource.Add(this.CreateTreeViewItemViewModel(descriptor));
            //    }
            //    else
            //    {
            //        if (this.BookmarkCategory.ItemsSource.Any(i => i.DisplayName == item) == true)
            //            continue;

            //        this.BookmarkCategory.ItemsSource.Add(new InvalidTreeViewItemViewModel(item, this.DeleteBookmark));
            //    }
            //}
        }

        private void DeleteBookmark(InvalidTreeViewItemViewModel viewModel)
        {
            this.smartSetContext.SetIsBookmark(viewModel.DisplayName, false);
        }

        private ITypeBrowser TypeBrowser => this.typeBrowser.Value;
    }
}

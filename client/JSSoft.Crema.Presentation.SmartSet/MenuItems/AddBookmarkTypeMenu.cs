﻿// Released under the MIT License.
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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.SmartSet.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.SmartSet.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.SmartSet.Properties;
using JSSoft.Crema.Presentation.Types.BrowserItems.ViewModels;
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.SmartSet.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(TypeTreeViewItemViewModel))]
    class AddBookmarkTypeMenu : MenuItemBase
    {
        private readonly TypeSmartSetBrowserViewModel browser;
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public AddBookmarkTypeMenu(Authenticator authenticator, TypeSmartSetBrowserViewModel browser)
        {
            this.authenticator = authenticator;
            this.browser = browser;
            this.Icon = "Images/star.png";
            this.DisplayName = Resources.MenuItem_AddToBookmark;
            this.HideOnDisabled = true;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is ITypeDescriptor descriptor)
            {
                if (parameter is TypeTreeViewItemViewModel viewModel && viewModel.Owner is TypeSmartSetBrowserViewModel == true)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is ITypeDescriptor descriptor)
            {
                var itemPaths = this.browser.GetBookmarkItemPaths();
                var dialog = new AddBookmarkItemViewModel(this.authenticator, descriptor.Name, itemPaths);
                if (await dialog.ShowDialogAsync() == true)
                {
                    this.browser.AddBookmarkItem(dialog.TargetPath, descriptor);
                }
            }
        }
    }
}

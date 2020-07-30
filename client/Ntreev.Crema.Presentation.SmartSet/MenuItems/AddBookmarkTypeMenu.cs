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
using Ntreev.Crema.Presentation.SmartSet.BrowserItems.ViewModels;
using Ntreev.Crema.Presentation.SmartSet.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.SmartSet.Properties;
using Ntreev.Crema.Presentation.Types.BrowserItems.ViewModels;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ntreev.Crema.Presentation.SmartSet.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(TypeTreeViewItemViewModel))]
    class AddBookmarkTypeMenu : MenuItemBase
    {
        [Import]
        private TypeSmartSetBrowserViewModel browser = null;
        [Import]
        private Authenticator authenticator = null;

        public AddBookmarkTypeMenu()
        {
            this.Icon = "/Ntreev.Crema.Presentation.SmartSet;component/Images/star.png";
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

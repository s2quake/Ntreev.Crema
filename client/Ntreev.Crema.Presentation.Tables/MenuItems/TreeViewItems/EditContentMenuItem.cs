﻿//Released under the MIT License.
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
using Ntreev.Crema.Presentation.Tables.BrowserItems.ViewModels;
using Ntreev.Crema.Presentation.Tables.Properties;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Presentation.Tables.MenuItems.TreeViewItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(TableTreeViewItemViewModel))]
    [Category(nameof(CategoryAttribute.Default))]
    [Order(-1)]
    class EditContentMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public EditContentMenuItem(Authenticator authenticator)
        {
            this.DisplayName = Resources.MenuItem_EditContent;
            this.authenticator = authenticator;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is ITableDescriptor descriptor)
            {
                if (TableUtility.CanEditContent(this.authenticator, descriptor) == true)
                {
                    return true;
                }
            }
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is ITableDescriptor descriptor)
            {
                if (TableUtility.CanEditContent(this.authenticator, descriptor) == true)
                {
                    await TableUtility.EditContentAsync(this.authenticator, descriptor);
                }
            }
        }
    }
}

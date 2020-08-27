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

using JSSoft.Crema.Presentation.Converters.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Converters.Properties;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Converters.ToolBarItems.TableBrowser
{
    [Export(typeof(IToolBarItem))]
    [ParentType("JSSoft.Crema.Presentation.Tables.ITableBrowser, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class ExportRevisionDataBaseToolBarItem : ToolBarItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public ExportRevisionDataBaseToolBarItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.Icon = "Images/spreadsheet.png";
            this.DisplayName = Resources.MenuItem_Export;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is ISelector selector)
            {
                return selector.SelectedItem is ITableCategoryDescriptor || selector.SelectedItem is ITableDescriptor;
            }
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is ISelector selector)
            {
                if (selector.SelectedItem is ITableCategoryDescriptor categoryDescriptor)
                {
                    var dialog = await ExportViewModel.CreateInstanceAsync(this.authenticator, categoryDescriptor);
                    await dialog?.ShowDialogAsync();
                }
                else if (selector.SelectedItem is ITableDescriptor tableDescriptor)
                {
                    var dialog = await ExportViewModel.CreateInstanceAsync(this.authenticator, tableDescriptor);
                    await dialog?.ShowDialogAsync();
                }
            }
        }
    }
}

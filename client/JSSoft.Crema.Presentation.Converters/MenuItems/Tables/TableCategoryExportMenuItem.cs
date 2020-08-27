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
using JSSoft.Crema.ServiceModel;
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Converters.MenuItems.Tables
{
    [Export(typeof(IMenuItem))]
    [ParentType("JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels.TableCategoryTreeViewItemViewModel, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class TableCategoryExportMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;
        private readonly ICremaAppHost cremaAppHost;

        [ImportingConstructor]
        public TableCategoryExportMenuItem(Authenticator authenticator, ICremaAppHost cremaAppHost)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Loaded += this.InvokeCanExecuteChangedEvent;
            this.cremaAppHost.Unloaded += this.InvokeCanExecuteChangedEvent;
            this.DisplayName = Resources.MenuItem_Export;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is ITableCategoryDescriptor descriptor)
            {
                var dialog = await ExportViewModel.CreateInstanceAsync(this.authenticator, descriptor);
                await dialog?.ShowDialogAsync();
            }
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this.cremaAppHost.IsLoaded == false)
                return false;
            if (parameter is IPermissionDescriptor descriptor)
                return descriptor.AccessType >= AccessType.Guest;
            return false;
        }
    }
}

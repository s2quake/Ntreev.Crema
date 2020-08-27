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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Tables.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(TableListItemBase))]
    [Order(int.MinValue)]
    [Category(nameof(CategoryAttribute.Default))]
    [DefaultMenu]
    class SelectInBrowserMenuItem : MenuItemBase
    {
        private readonly IShell shell;
        private readonly TableBrowserViewModel browser;
        private readonly TableServiceViewModel service;

        [ImportingConstructor]
        public SelectInBrowserMenuItem(IShell shell, TableBrowserViewModel browser, TableServiceViewModel service)
        {
            this.shell = shell;
            this.browser = browser;
            this.service = service;
            this.DisplayName = Resources.MenuItem_SelectInTableBrowser;
        }

        protected override bool OnCanExecute(object parameter)
        {
            return base.OnCanExecute(parameter);
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is ITableDescriptor descriptor && this.browser != null && this.shell != null && this.service != null)
            {
                await this.Dispatcher.InvokeAsync(() => this.shell.SelectedService = this.service);
                await this.Dispatcher.InvokeAsync(() => this.browser.Select(descriptor));
            }
        }
    }
}

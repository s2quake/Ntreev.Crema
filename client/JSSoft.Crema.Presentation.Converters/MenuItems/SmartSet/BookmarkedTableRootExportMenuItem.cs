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
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Presentation.Converters.MenuItems.SmartSet
{
    [Export(typeof(IMenuItem))]
    [ParentType("JSSoft.Crema.Presentation.SmartSet.BrowserItems.ViewModels.BookmarkTableRootTreeViewItemViewModel, JSSoft.Crema.Presentation.SmartSet, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    [ParentType("JSSoft.Crema.Presentation.SmartSet.BrowserItems.ViewModels.BookmarkTableCategoryTreeViewItemViewModel, JSSoft.Crema.Presentation.SmartSet, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class BookmarkedTableRootExportMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;
        private readonly ICremaAppHost cremaAppHost;

        [ImportingConstructor]
        public BookmarkedTableRootExportMenuItem(Authenticator authenticator, ICremaAppHost cremaAppHost)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Loaded += this.InvokeCanExecuteChangedEvent;
            this.cremaAppHost.Unloaded += this.InvokeCanExecuteChangedEvent;
            this.DisplayName = Resources.MenuItem_Export;
        }

        protected async override void OnExecute(object parameter)
        {
            if (this.cremaAppHost.GetService(typeof(IDataBase)) is IDataBase dataBase)
            {
                var viewModel = parameter as TreeViewItemViewModel;

                var query = from item in viewModel.Items
                            where item.Target is ITable
                            let table = item.Target as ITable
                            select table;

                var paths = await dataBase.Dispatcher.InvokeAsync(() =>
                {
                    return query.Where(item => item.VerifyAccessType(this.authenticator, AccessType.Guest)).
                                 Select(item => item.Path).
                                 Distinct().
                                 ToArray();
                });

                if (paths.Any() == false)
                {
                    await AppMessageBox.ShowAsync(Resources.Message_NoneTablesToExport);
                }
                else
                {
                    var dialog = await ExportViewModel.CreateInstanceAsync(this.authenticator, this.cremaAppHost, paths);
                    if (dialog != null)
                        await dialog.ShowDialogAsync();
                }
            }
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (this.cremaAppHost.IsLoaded == true && parameter is TreeViewItemViewModel viewModel)
            {
                var query = from item in viewModel.Items
                            where item.Target is ITable
                            select item.Target as ITable;

                return query.Any();
            }
            return false;
        }
    }
}

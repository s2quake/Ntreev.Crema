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

using Microsoft.WindowsAPICodePack.Dialogs;
using JSSoft.Crema.Presentation.Converters.Properties;
using JSSoft.Crema.Services;
using JSSoft.Crema.Spreadsheet;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Converters.ToolBarItems
{
    [Export(typeof(IToolBarItem))]
    [ParentType("JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels.LogViewModel, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class ExportRevisionTableToolBarItem : ToolBarItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public ExportRevisionTableToolBarItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.Icon = "Images/spreadsheet.png";
            this.DisplayName = Resources.MenuItem_Export;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is ISelector selector && selector.SelectedItem is ListBoxItemViewModel viewModel)
            {
                return viewModel.Target is ITableItem;
            }
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            try
            {
                if (parameter is ISelector selector && selector.SelectedItem is ListBoxItemViewModel viewModel && viewModel.Target is ITableItem tableItem)
                {
                    if (viewModel is IInfoProvider provider)
                    {
                        var props = provider.Info;
                        var revision = (string)props["Revision"];
                        var tableName = await tableItem.Dispatcher.InvokeAsync(() => tableItem.Name);
                        var dialog = new CommonSaveFileDialog();
                        dialog.Filters.Add(new CommonFileDialogFilter("excel file", "*.xlsx"));
                        dialog.DefaultFileName = $"{tableName}_{revision}";
                        dialog.DefaultExtension = "xlsx";

                        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            var dataSet = await tableItem.GetDataSetAsync(this.authenticator, revision);
                            var writer = new SpreadsheetWriter(dataSet);
                            writer.Write(dialog.FileName);
                            await AppMessageBox.ShowAsync(Resources.Message_Exported);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }
    }
}

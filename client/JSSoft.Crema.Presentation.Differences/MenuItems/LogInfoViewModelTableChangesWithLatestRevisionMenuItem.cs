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

using JSSoft.Crema.Data.Diff;
using JSSoft.Crema.Presentation.Differences.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Differences.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(LogInfoViewModel))]
    class LogInfoViewModelTableChangesWithLatestRevisionMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public LogInfoViewModelTableChangesWithLatestRevisionMenuItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public LogInfoViewModelTableChangesWithLatestRevisionMenuItem()
        {
            this.DisplayName = Resources.MenuItem_CompareWithLatestRevision;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is LogInfoViewModel viewModel && viewModel.Target is ITable)
                return true;
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            var viewModel = parameter as LogInfoViewModel;
            var table = viewModel.Target as ITable;
            var dialog = new DiffDataTableViewModel(this.Initialize(viewModel, table))
            {
                DisplayName = Resources.Title_CompareWithLatestRevision,
            };
            await dialog.ShowDialogAsync();
        }

        private async Task<DiffDataTable> Initialize(LogInfoViewModel viewModel, ITable table)
        {
            var logs = await table.GetLogAsync(this.authenticator, null);
            var log = logs.First();

            var dataSet1 = await table.GetDataSetAsync(this.authenticator, viewModel.Revision);
            var dataSet2 = await table.GetDataSetAsync(this.authenticator, log.Revision);
            var header1 = $"[{viewModel.DateTime}] {viewModel.Revision}";
            var header2 = $"[{log.DateTime}] {log.Revision}";
            var dataSet = new DiffDataSet(dataSet1, dataSet2)
            {
                Header1 = header1,
                Header2 = header2,
            };
            return dataSet.Tables.First();
        }
    }
}

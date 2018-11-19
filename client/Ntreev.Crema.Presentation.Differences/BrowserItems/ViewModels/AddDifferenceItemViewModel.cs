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

using Ntreev.Crema.Presentation.Base.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Framework.Dialogs.ViewModels;
using Ntreev.Crema.Data.Diff;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Presentation.Differences.BrowserItems.ViewModels
{
    [Export(typeof(IBrowserItem))]
    [ParentType(typeof(BrowserService))]
    class AddDifferenceItemViewModel : ViewModelBase, IBrowserItem
    {
        [Import]
        private ICremaHost cremaHost = null;
        [Import]
        private ICremaAppHost cremaAppHost = null;

        [Import]
        private Authenticator authenticator = null;
        [Import]
        private Lazy<BrowserService> browserService = null;

        public void Add()
        {
            var destDataBaseName = this.SelectDataBase();
            if (destDataBaseName == null)
                return;

            var task = new BackgroundViewModel(this.authenticator, this.cremaHost, this.cremaAppHost.DataBaseName, destDataBaseName);
            task.ProgressChanged += Task_ProgressChanged;
            var dialog = new BackgroundTaskViewModel(task) { DisplayName = "데이터 베이스 비교하기", };
            dialog.ShowDialog();
        }

        public string DisplayName => "데이터 베이스 비교하기";

        public bool IsVisible => true;

        private void Task_ProgressChanged(object sender, Library.ProgressChangedEventArgs e)
        {
            if (sender is BackgroundViewModel task)
            {
                if (e.State == ProgressChangeState.Completed)
                {
                    this.browserService.Value.Add(task.Result);
                }
            }
        }

        private string SelectDataBase()
        {
            var dialog = new SelectDataBaseViewModel(this.authenticator, this.cremaAppHost, (item) => item.Name != this.cremaAppHost.DataBaseName);
            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedValue;
            }
            return null;
        }

        #region classes

        class BackgroundViewModel : BackgroundTaskBase
        {
            private readonly Authentication authentication;
            private readonly ICremaHost cremaHost;
            private readonly string dataBaseName1;
            private readonly string dataBaseName2;
            private DiffDataSet dataSet;

            public BackgroundViewModel(Authentication authentication, ICremaHost cremaHost, string dataBaseName1, string dataBaseName2)
            {
                this.authentication = authentication;
                this.cremaHost = cremaHost;
                this.dataBaseName1 = dataBaseName1;
                this.dataBaseName2 = dataBaseName2;
            }

            public DiffDataSet Result => this.dataSet;

            protected override async Task OnRunAsync(IProgress progress, CancellationToken cancellation)
            {
                progress.Report(0, "데이터 베이스 가져오는중");

                var dataBase1 = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext[this.dataBaseName1]);
                var dataBase2 = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext[this.dataBaseName2]);
                var tasks = new Task<CremaDataSet>[]
                {
                    dataBase1.GetDataSetAsync(this.authentication, DataSetType.All, null, null),
                    dataBase2.GetDataSetAsync(this.authentication, DataSetType.All, null, null),
                };
                await Task.WhenAll(tasks);
                progress.Report(0.5, "비교하는중");
                this.dataSet = new DiffDataSet(tasks[1].Result, tasks[0].Result, DiffMergeTypes.ReadOnly2)
                {
                    Header1 = this.dataBaseName2,
                    Header2 = this.dataBaseName1
                };
            }

            private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
        }

        #endregion
    }
}

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

using JSSoft.Crema.Data.Diff;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace JSSoft.Crema.Presentation.Differences.Dialogs.ViewModels
{
    public class DiffDataTableViewModel : ModalDialogBase
    {
        private DiffDataTable diffTable;
        private List<DiffDataTableItemViewModel> itemList;
        private object selectedItem;
        private string header1;
        private string header2;

        [Import]
        private readonly BrowserService browserService = null;
        [Import]
        private readonly DifferencesServiceViewModel service = null;
        [Import]
        private readonly IShell shell = null;

        internal DiffDataTableViewModel(Task<DiffDataTable> action)
        {
            this.Initialize(action);
        }

        public DiffDataTableViewModel(DiffDataTable diffTable)
        {
            this.Source = diffTable;
        }

        public async Task MergeAsync()
        {
            await this.TryCloseAsync();
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.browserService.Add(this.diffTable.DiffSet);
                this.shell.SelectedService = this.service;
            });
        }

        public IEnumerable ItemsSource => this.itemList;

        public object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
            }
        }

        public string SelectedName
        {
            get => $"{this.selectedItem}";
            set
            {
                var query = from item in this.itemList
                            where $"{item}" == value
                            select item;

                if (query.Any() == false)
                    return;

                this.SelectedItem = query.First();
            }
        }

        public DiffDataTable Source
        {
            get => this.diffTable;
            private set
            {
                this.diffTable = value;
                this.itemList = new List<DiffDataTableItemViewModel>
                {
                    new DiffDataTableItemViewModel(this, value)
                };
                foreach (var item in value.Childs)
                {
                    this.itemList.Add(new DiffDataTableItemViewModel(this, item));
                }
                this.selectedItem = this.itemList.First();
                this.Header1 = this.diffTable.Header1;
                this.Header2 = this.diffTable.Header2;
                this.NotifyOfPropertyChange(nameof(this.Source));
                this.NotifyOfPropertyChange(nameof(this.ItemsSource));
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.SelectedName));
            }
        }

        public string Header1
        {
            get => this.header1 ?? string.Empty;
            set
            {
                this.header1 = value;
                this.NotifyOfPropertyChange(nameof(this.Header1));
            }
        }

        public string Header2
        {
            get => this.header2 ?? string.Empty;
            set
            {
                this.header2 = value;
                this.NotifyOfPropertyChange(nameof(this.Header2));
            }
        }

        private async void Initialize(Task<DiffDataTable> action)
        {
            try
            {
                this.BeginProgress(Resources.Message_ReceivingInfo);
                this.Source = await action;
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                }, DispatcherPriority.ApplicationIdle);
                await AppMessageBox.ShowErrorAsync(e);
                await this.TryCloseAsync();
            }
            finally
            {
                this.EndProgress();
            }
        }
    }
}

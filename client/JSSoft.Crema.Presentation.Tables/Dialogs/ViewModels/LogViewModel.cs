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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class LogViewModel : ModalDialogAppBase, ISelector
    {
        private readonly Authentication authentication;
        private readonly ITableItem tableItem;
        private LogInfoViewModel[] itemsSource;
        private LogInfoViewModel selectedItem;
        private readonly ICommand previewCommand;

        private LogViewModel(Authentication authentication, ITableItem tableItem)
        {
            this.authentication = authentication;
            this.tableItem = tableItem;
            this.DisplayName = Resources.Title_ViewLog;
            this.previewCommand = new DelegateCommand((p) => this.PreviewAsync(), (p) => this.CanPreview);
        }

        public static async Task<LogViewModel> ShowDialogAsync(Authentication authentication, ITableItemDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITableItem tableItem)
            {
                try
                {
                    var dialog = await tableItem.Dispatcher.InvokeAsync(() => new LogViewModel(authentication, tableItem));
                    if (await dialog.ShowDialogAsync() == true)
                        return dialog;
                    return null;
                }
                catch (Exception e)
                {
                    CremaLog.Error(e);
                    return null;
                }
            }
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            return this.TryCloseAsync(true);
        }

        public Task PreviewAsync()
        {
            return this.selectedItem.PreviewAsync();
        }

        public LogInfoViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.CanPreview));
            }
        }

        public bool CanPreview
        {
            get
            {
                if (this.IsProgressing == true)
                    return false;
                return this.selectedItem != null;
            }
        }

        public IEnumerable<LogInfoViewModel> Items => this.itemsSource;

        public ICommand PreviewCommand => this.previewCommand;

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
            try
            {
                this.BeginProgress(Resources.Message_ReceivingInfo);
                var logs = await this.tableItem.GetLogAsync(this.authentication, null);
                this.itemsSource = await this.tableItem.Dispatcher.InvokeAsync(() =>
                {
                    var logList = new List<LogInfoViewModel>(logs.Length);
                    foreach (var item in logs)
                    {
                        logList.Add(new LogInfoViewModel(this.authentication, this.tableItem, item));
                    }
                    return logList.ToArray();
                });
                this.selectedItem = null;
                this.EndProgress();
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.Items));
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
                await this.TryCloseAsync();
            }
        }

        #region ISelector

        object ISelector.SelectedItem
        {
            get => this.SelectedItem;
            set
            {
                if (value is LogInfoViewModel viewModel)
                    this.SelectedItem = viewModel;
                else
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}

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
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Home.Dialogs.ViewModels
{
    public class LogViewModel : ModalDialogAppBase, ISelector
    {
        private readonly Authentication authentication;
        private readonly IDataBase dataBase;
        private LogInfoViewModel[] itemsSource;
        private LogInfoViewModel selectedItem;

        private LogViewModel(Authentication authentication, IDataBase dataBase)
            : base(dataBase)
        {
            this.authentication = authentication;
            this.dataBase = dataBase;
            this.DisplayName = Resources.Title_ViewLog;
        }

        public static async Task<LogViewModel> ShowDialogAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IDataBase dataBase)
            {
                try
                {
                    var dialog = await dataBase.Dispatcher.InvokeAsync(() => new LogViewModel(authentication, dataBase));
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

        public async Task CloseAsync()
        {
            await this.TryCloseAsync(true);
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

        public IEnumerable<LogInfoViewModel> ItemsSource => this.itemsSource;

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
            try
            {
                this.BeginProgress(Resources.Message_ReceivingInfo);
                var logs = await this.dataBase.GetLogAsync(this.authentication, null);
                this.itemsSource = await this.dataBase.Dispatcher.InvokeAsync(() =>
                {
                    var logList = new List<LogInfoViewModel>(logs.Length);
                    foreach (var item in logs)
                    {
                        logList.Add(new LogInfoViewModel(this.authentication, this.dataBase, item));
                    }
                    return logList.ToArray();
                });
                this.selectedItem = null;
                this.EndProgress();
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.ItemsSource));
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

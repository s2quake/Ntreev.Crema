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

using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework.Dialogs.ViewModels
{
    public class BackgroundTaskViewModel : ModalDialogBase
    {
        private readonly IStatusBarService statusBarService;
        private readonly IBackgroundTask task;

        public BackgroundTaskViewModel(IStatusBarService statusBarService, IBackgroundTask task)
        {
            this.statusBarService = statusBarService;
            this.task = task ?? throw new ArgumentNullException(nameof(task));
            this.task.ProgressChanged += Task_ProgressChanged;
            this.DisplayName = this.task.DisplayName;
        }

        public async Task HideAsync()
        {
            await this.TryCloseAsync();
        }

        public void Cancel()
        {
            this.task.Cancel();
            this.NotifyOfPropertyChange(nameof(this.CanCancel));
        }

        public bool CanCancel => this.task.IsBusy && this.task.IsCancellationRequested == false;

        public bool CanHide => true;

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            this.Dispatcher.InvokeAsync(() =>
            {
                this.statusBarService.AddTask(this.task);
                this.BeginProgress();
                this.Refresh();
            });
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (this.task != null && close == true)
            {
                this.task.ProgressChanged -= Task_ProgressChanged;
            }
        }

        private void Task_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.ProgressMessage = e.Message;

            if (e.State == ProgressChangeState.Completed || e.State == ProgressChangeState.Failed)
            {
                this.EndProgress(e.Message);
                this.Refresh();
            }
        }
    }
}

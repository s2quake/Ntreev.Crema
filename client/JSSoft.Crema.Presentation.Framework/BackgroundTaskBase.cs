﻿// Released under the MIT License.
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

using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JSSoft.Crema.Presentation.Framework
{
    public abstract class BackgroundTaskBase : PropertyChangedBase, IBackgroundTask
    {
        private readonly CancellationTokenSource cancellation = new();
        private string displayName;
        private bool isAlive = true;

        public BackgroundTaskBase()
        {
            Application.Current.Exit += Current_Exit;
        }

        public string DisplayName
        {
            get => this.displayName ?? this.ToString();
            set
            {
                this.displayName = value;
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }

        public bool IsBusy { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;

        public void Cancel()
        {
            this.cancellation.Cancel();
            this.NotifyOfPropertyChange(nameof(this.CanCancel));
        }

        void IBackgroundTask.RunAsync()
        {
            this.RunAsync();
        }

        public async void RunAsync()
        {
            var progress = new Progress();
            progress.Changed += Progress_Changed;
            try
            {
                this.IsBusy = true;
                await this.OnRunAsync(progress, this.cancellation.Token);
                await this.Dispatcher.InvokeAsync(() => progress.Complete());
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() => progress.Fail(e.Message));
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public bool CanCancel => this.cancellation.IsCancellationRequested == false;

        public bool IsCancellationRequested => this.cancellation.IsCancellationRequested;

        protected abstract Task OnRunAsync(IProgress progress, CancellationToken cancellation);

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            this.ProgressChanged?.Invoke(this, e);
        }

        private void Progress_Changed(object sender, ProgressChangedEventArgs e)
        {
            if (this.isAlive == true)
            {
                this.Dispatcher.InvokeAsync(() => this.OnProgressChanged(e));
            }
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            this.isAlive = false;
        }
    }
}

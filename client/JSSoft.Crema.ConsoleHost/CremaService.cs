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

using JSSoft.Crema.ServiceHosts;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost
{
    class CremaService : ICremaService, IServiceProvider, IPartImportsSatisfiedNotification
    {
        public const string Namespace = "http://www.ntreev.com";
        private const string cremaString = "Crema";
        private readonly IServiceProvider serviceProvider;
        private ICremaHost cremaHost;
        private Guid token;

        public CremaService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.Dispatcher = new CremaDispatcher(typeof(CremaService));
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        // public string GetPath(CremaPath pathType, params string[] paths)
        // {
        //     if (this.cremaHost == null)
        //         throw new InvalidOperationException();
        //     return this.cremaHost.GetPath(pathType, paths);
        // }

        public void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }

        public async Task OpenAsync()
        {
            this.ValidateOpen();
            this.ServiceState = JSSoft.Crema.Services.ServiceState.Opening;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnOpening(EventArgs.Empty);
                this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
            });
            this.token = await this.cremaHost.OpenAsync();
            await this.cremaHost.Dispatcher.InvokeAsync(() =>
            {
                this.cremaHost.CloseRequested += CremaHost_CloseRequested;
                this.cremaHost.Closed += CremaHost_Closed;
            });
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.ServiceState = JSSoft.Crema.Services.ServiceState.Open;
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync()
        {
            this.ValidateClose();
            this.ServiceState = JSSoft.Crema.Services.ServiceState.Closing;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnClosing(EventArgs.Empty);
            });
            await this.cremaHost.Dispatcher.InvokeAsync(() =>
            {
                this.cremaHost.CloseRequested -= CremaHost_CloseRequested;
                this.cremaHost.Closed -= CremaHost_Closed;
            });
            await this.cremaHost.CloseAsync(this.token);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.token = Guid.Empty;
                this.ServiceState = JSSoft.Crema.Services.ServiceState.Closed;
                this.OnClosed(new ClosedEventArgs(CloseReason.Shutdown, string.Empty));
            });
        }

        // public string Address { get; set; } = "localhost";

        // public int Timeout { get; set; } = 60000;

        public CremaDispatcher Dispatcher { get; private set; }

        public JSSoft.Crema.Services.ServiceState ServiceState { get; set; }

        public event EventHandler Opening;

        public event EventHandler Opened;

        public event EventHandler Closing;

        public event ClosedEventHandler Closed;

        protected virtual void OnOpening(EventArgs e)
        {
            this.Opening?.Invoke(this, e);
        }

        protected virtual void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        protected virtual void OnClosing(EventArgs e)
        {
            this.Closing?.Invoke(this, e);
        }

        protected virtual void OnClosed(ClosedEventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        protected ILogService LogService => this.cremaHost.GetService(typeof(ILogService)) as ILogService;

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            if (sender is ICremaHost cremaHost)
            {
                this.cremaHost.Opened -= CremaHost_Opened;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Opening;
                });
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Open;
                });
            }
        }

        private void CremaHost_CloseRequested(object sender, CloseRequestedEventArgs e)
        {
            e.AddTask(InvokeAsync());
            async Task InvokeAsync()
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Closing;
                });
            }
        }

        private async void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            if (sender is ICremaHost)
            {
                if (e.Reason == CloseReason.Restart)
                    this.cremaHost.Opened += CremaHost_Opened;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Closed;
                    this.OnClosed(e);
                });
            }
        }

        private void ValidateOpen()
        {
            if (this.ServiceState != JSSoft.Crema.Services.ServiceState.None)
                throw new InvalidOperationException();
        }

        private void ValidateClose()
        {
            if (this.ServiceState != JSSoft.Crema.Services.ServiceState.Open)
                throw new InvalidOperationException();
        }

        #region IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
        }

        #endregion
    }
}

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
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost
{
    class CremaService : ICremaService
    {
        private readonly IServiceProvider serviceProvider;
        private ICremaHost cremaHost;

        public CremaService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.Dispatcher = new CremaDispatcher(typeof(CremaService));
        }

        public async Task OpenAsync()
        {
            // this.cremaHost = cremaHost;
            // this.cremaHost.Opening += CremaHost_Opening;
            // this.cremaHost.Opened += CremaHost_Opened;
            // this.cremaHost.Closing += CremaHost_Closing;
            // this.cremaHost.Closed += CremaHost_Closed;
        }

        public async Task CloseAsync()
        {

        }

        public CremaDispatcher Dispatcher { get; private set; }

        public event EventHandler Opening;

        public event EventHandler Opened;

        public event EventHandler Closing;

        public event ClosedEventHandler Closed;

        // private async void CremaHost_Opening(object sender, EventArgs e)
        // {
        //     await this.dispatcher.InvokeAsync(() => this.Opening?.Invoke(this, e));
        // }

        // private async void CremaHost_Opened(object sender, EventArgs e)
        // {
        //     await this.dispatcher.InvokeAsync(() => this.Opened?.Invoke(this, e));
        // }

        // private async void CremaHost_Closing(object sender, EventArgs e)
        // {
        //     await this.dispatcher.InvokeAsync(() => this.Closing?.Invoke(this, e));
        // }

        // private async void CremaHost_Closed(object sender, ClosedEventArgs e)
        // {
        //     await this.dispatcher.InvokeAsync(() => this.Closed?.Invoke(this, e));
        // }
    }
}

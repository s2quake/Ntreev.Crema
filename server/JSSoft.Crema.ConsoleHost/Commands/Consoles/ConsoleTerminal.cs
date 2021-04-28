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

using JSSoft.Crema.Commands.Consoles;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost.Commands.Consoles
{
    [Export]
    class ConsoleTerminal : ConsoleTerminalBase
    {
        private readonly ICremaHost cremaHost;
        private readonly CremaApplication application;
        private readonly ConsoleCommandContext commandContext;
        private readonly ConsoleTerminalCancellation cancellation;

        [ImportingConstructor]
        public ConsoleTerminal(ICremaHost cremaHost, CremaApplication application,
            ConsoleCommandContext commandContext, ConsoleTerminalCancellation cancellation)
            : base(commandContext)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += (s, e) => this.IsEnabled = true;
            this.cremaHost.Closing += (s, e) => this.IsEnabled = false;
            this.application = application;
            this.application.Closed += Application_Closed;
            this.commandContext = commandContext;
            this.cancellation = cancellation;
        }

        public async Task StartAsync()
        {
            this.SetPrompt();
            await base.StartAsync(this.cancellation.Run());
        }

        private void Application_Closed(object sender, ClosedEventArgs e)
        {
            if (e.Reason == CloseReason.Shutdown && this.cancellation.IsRunning == true)
            {
                this.cancellation.Cancel();
            }
        }
    }
}

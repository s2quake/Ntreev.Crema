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

using JSSoft.Crema.ConsoleHost.Commands.Consoles;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost.Commands
{
    [Export(typeof(ICommand))]
    [Export]
    [ResourceUsageDescription]
    class ConnectCommand : CommandAsyncBase
    {
        private readonly CremaBootstrapper application;
        private readonly ICremaHost cremaHost;
        private readonly Lazy<ConsoleTerminal> terminal;

        [ImportingConstructor]
        public ConnectCommand(CremaBootstrapper application, ICremaHost cremaHost, Lazy<ConsoleTerminal> terminal)
            : base("connect")
        {
            this.application = application;
            this.cremaHost = cremaHost;
            this.terminal = terminal;
        }

        public void Cancel()
        {
            var terminal = this.application.GetService(typeof(ConsoleTerminal)) as ConsoleTerminal;
            terminal.Cancel();
        }

        [CommandPropertyRequired]
        public string Address
        {
            get;
            set;
        }

#if DEBUG
        [CommandProperty(InitValue = "en-US")]
#else
        [CommandProperty(InitValue = "")]
#endif
        public string Culture
        {
            get; set;
        }

#if DEBUG
        [CommandProperty('l', DefaultValue = "admin:admin")]
#else
        [CommandProperty('l')]
#endif
        public string LoginAuthentication
        {
            get;
            set;
        }

        public new ConsoleCommandContext CommandContext => base.CommandContext as ConsoleCommandContext;

        protected override Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            this.application.Culture = this.Culture;
            this.cremaHost.Closed += CremaHost_Closed;
            return this.WaitAsync();
        }

        private void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            if (e.Reason == CloseReason.Shutdown)
            {
                this.Cancel();
            }
        }

        private async Task WaitAsync()
        {
            this.CommandContext.SetAddress(this.Address);

            if (this.LoginAuthentication != null)
            {
                await this.Terminal.StartAsync(this.LoginAuthentication);
            }
            else
            {
                await Task.Delay(1);
                this.Terminal.Start();
            }
        }

        private ConsoleTerminal Terminal => this.terminal.Value;
    }
}
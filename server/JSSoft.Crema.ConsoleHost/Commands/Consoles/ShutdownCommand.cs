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

using JSSoft.Crema.Commands;
using JSSoft.Crema.Commands.Consoles;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class ShutdownCommand : ConsoleCommandAsyncBase
    {
        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        public ShutdownCommand(ICremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
        }

        [CommandPropertyRequired(DefaultValue = "")]
        [CommandPropertyTrigger(nameof(IsCancelled), false)]
        public DateTimeValue Time
        {
            get; set;
        }

        [CommandPropertySwitch('r')]
        [CommandPropertyTrigger(nameof(IsCancelled), false)]
        public bool IsRestart
        {
            get; set;
        }

        [CommandPropertySwitch('c')]
        [CommandPropertyTrigger(nameof(DateTimeValue), "")]
        public bool IsCancelled
        {
            get; set;
        }

        [CommandProperty('m')]
        [CommandPropertyTrigger(nameof(IsCancelled), false)]
        public string Message
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.IsOnline;

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            if (this.IsCancelled == true)
            {
                await this.cremaHost.CancelShutdownAsync(authentication);
            }
            else
            {
                var shutdownContext = new ShutdownContext()
                {
                    Milliseconds = this.Time.Milliseconds,
                    IsRestart = this.IsRestart,
                    Message = this.Message
                };
                await this.cremaHost.ShutdownAsync(authentication, shutdownContext);
            }
        }
    }
}

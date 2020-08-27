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
using JSSoft.Library.Commands;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot.Consoles
{
    [Export(typeof(IConsoleCommand))]
    class AutobotCommand : ConsoleCommandMethodBase
    {
        private readonly Lazy<AutobotService> autobotService;

        public AutobotCommand(Lazy<AutobotService> autobotService)
            : base("autobot")
        {
            this.autobotService = autobotService;
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(Count))]
        public Task StartAsync()
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            return this.AutobotService.StartAsync(authentication, this.Count);
        }

        [CommandMethod]
        public Task StopAsync()
        {
            return this.AutobotService.StopAsync();
        }

        [CommandProperty]
#if DEBUG
        [DefaultValue(10)]
#else
        [DefaultValue(0)]
#endif
        public int Count
        {
            get; set;
        }

        public override bool IsEnabled
        {
            get
            {
                if (this.CommandContext.IsOnline == false)
                    return false;
                return this.CommandContext.Authority == ServiceModel.Authority.Admin;
            }
        }

        protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
        {
            if (descriptor.DescriptorName == nameof(StartAsync))
            {
                return this.AutobotService.ServiceState == Services.ServiceState.None;
            }
            else if (descriptor.DescriptorName == nameof(StopAsync))
            {
                return this.AutobotService.ServiceState == Services.ServiceState.Open;
            }
            return base.IsMethodEnabled(descriptor);
        }

        private AutobotService AutobotService => this.autobotService.Value;
    }
}

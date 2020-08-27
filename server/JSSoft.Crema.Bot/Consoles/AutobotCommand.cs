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
using JSSoft.Crema.Services;
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
        private readonly ICremaHost cremaHost;
        [Import]
#pragma warning disable IDE0044 // 읽기 전용 한정자 추가
        private Lazy<AutobotService> autobotService = null;
#pragma warning restore IDE0044 // 읽기 전용 한정자 추가

        [ImportingConstructor]
        public AutobotCommand(ICremaHost cremaHost)
            : base("autobot")
        {
            this.cremaHost = cremaHost;
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(Count))]
        public async Task StartAsync()
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            await this.AutobotService.StartAsync(authentication, this.Count);
        }

        [CommandMethod]
        public async Task StopAsync()
        {
            await this.AutobotService.StopAsync();
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

        protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
        {
            if (descriptor.DescriptorName == nameof(StartAsync))
            {
                return this.cremaHost.ServiceState == ServiceState.Open && this.AutobotService.ServiceState == ServiceState.None;
            }
            else if (descriptor.DescriptorName == nameof(StopAsync))
            {
                return this.cremaHost.ServiceState == ServiceState.Open && this.AutobotService.ServiceState == ServiceState.Open;
            }
            return base.IsMethodEnabled(descriptor);
        }

        private AutobotService AutobotService => this.autobotService.Value;
    }
}

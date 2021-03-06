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

using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Commands;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class NotifyCommand : ConsoleCommandAsyncBase
    {
        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        public NotifyCommand(ICremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
            this.Message = string.Empty;
        }

        [CommandPropertyRequired]
        public string Message
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.IsOnline;

        public override string[] GetCompletions(CommandCompletionContext completionContext)
        {
            return this.UserCollection.Dispatcher.Invoke(() =>
            {
                var query = from item in this.UserCollection
                            select item.ID;
                return query.ToArray();
            });
        }

        protected override Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            return this.UserContext.NotifyMessageAsync(authentication, this.Message);
        }

        private IUserContext UserContext => this.cremaHost.GetService(typeof(IUserContext)) as IUserContext;

        private IUserCollection UserCollection => this.cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
    }
}

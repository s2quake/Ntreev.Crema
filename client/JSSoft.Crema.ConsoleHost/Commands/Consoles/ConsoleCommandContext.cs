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
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost.Commands.Consoles
{
    [Export(typeof(ConsoleCommandContext))]
    class ConsoleCommandContext : ConsoleCommandContextBase
    {
        private readonly ICremaHost cremaHost;
        private readonly CremaApplication application;
        private Authentication authentication;

        [ImportingConstructor]
        public ConsoleCommandContext(ICremaHost cremaHost,
            CremaApplication application,
            [ImportMany] IEnumerable<IConsoleDrive> driveItems,
            [ImportMany] IEnumerable<IConsoleCommand> commands)
            : base(driveItems, commands)
        {
            this.cremaHost = cremaHost;
            this.application = application;
            this.BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.Dispatcher = application.Dispatcher;
        }

        public async Task LoginAsync(string userID, SecureString password, bool force)
        {
            var token = await this.CremaHost.LoginAsync(userID, password, force);
            var authentication = await this.cremaHost.AuthenticateAsync(token);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.authentication = authentication;
                this.authentication.Expired += Authentication_Expired;
                this.Initialize(authentication);
            });
        }

        public async Task LogoutAsync()
        {
            if (this.authentication == null)
                throw new Exception("로그인되어 있지 않습니다.");
            await this.CremaHost.LogoutAsync(this.authentication);
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.authentication != null)
                    this.authentication.Expired -= Authentication_Expired;
                this.authentication = null;
                this.Release();
            });
        }

        public override ICremaHost CremaHost => this.cremaHost;

        public override Dispatcher Dispatcher { get; }

        public override string Address => this.application.Address;

        private void Authentication_Expired(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() => this.authentication = null);
        }
    }
}

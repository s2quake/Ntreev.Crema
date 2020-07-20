//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Crema.Commands.Consoles;
using Ntreev.Crema.ServiceHosts;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.ApplicationHost.Commands.Consoles
{
    [Export(typeof(ConsoleCommandContext))]
    public class ConsoleCommandContext : ConsoleCommandContextBase
    {
        private ICremaHost cremaHost;
        private Authentication authentication;
        [Import]
        private CremaService service = null;

        static ConsoleCommandContext()
        {
            CommandSettings.IsConsoleMode = false;
        }

        [ImportingConstructor]
        public ConsoleCommandContext(ICremaHost cremaHost,
            [ImportMany]IEnumerable<IConsoleDrive> rootItems,
            [ImportMany]IEnumerable<IConsoleCommand> commands)
            : base(rootItems, commands)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += (s, e) => this.BaseDirectory = this.cremaHost.GetPath(CremaPath.Documents);
        }

        public async Task LoginAsync(string userID, SecureString password)
        {
            if (this.authentication != null)
                throw new Exception("이미 로그인되어 있습니다.");
            var token = await this.CremaHost.LoginAsync(userID, password);
            this.authentication = await this.CremaHost.AuthenticateAsync(token);
            this.authentication.Expired += (s, e) => this.authentication = null;
            this.Initialize(authentication);
        }

#if DEBUG
        public Task LoginAsync(string userID, string password)
        {
            var secureString = new SecureString();
            foreach (var item in password)
            {
                secureString.AppendChar(item);
            }
            return this.LoginAsync(userID, secureString);
        }
#endif

        public async Task LogoutAsync()
        {
            if (this.authentication == null)
                throw new Exception("로그인되어 있지 않습니다.");
            await this.CremaHost.LogoutAsync(this.authentication);
            this.authentication = null;
            this.Release();
        }

        public override ICremaHost CremaHost => this.cremaHost;

        public override string Address
        {
            get
            {
                return AddressUtility.GetDisplayAddress($"localhost:{this.service.Port}");
            }
        }
    }
}

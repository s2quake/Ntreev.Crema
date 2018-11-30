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

using Ntreev.Crema.Commands;
using Ntreev.Crema.Commands.Consoles;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Commands;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.ConsoleHost.Commands.Consoles
{
    [Export(typeof(ConsoleCommandContext))]
    public class ConsoleCommandContext : ConsoleCommandContextBase
    {
        private readonly ICremaHost cremaHost;
        private Guid token;
        private string address;
        private Authentication authenticator;

        [ImportingConstructor]
        public ConsoleCommandContext(ICremaHost cremaHost,
            [ImportMany]IEnumerable<IConsoleDrive> driveItems,
            [ImportMany]IEnumerable<IConsoleCommand> commands,
            [ImportMany]IEnumerable<IConsoleCommandProvider> commandProviders)
            : base(driveItems, commands, commandProviders)
        {
            this.cremaHost = cremaHost;
            this.BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public override string Address
        {
            get { return this.address; }
        }

        public override ICremaHost CremaHost => this.cremaHost;

        public void SetAddress(string address)
        {
            this.address = address;
        }

        public async Task LoginAsync(string address, string userID, SecureString password)
        {
            this.token = await this.CremaHost.OpenAsync(address, userID, password);
            await this.CremaHost.Dispatcher.InvokeAsync(() => this.CremaHost.Closed += CremaHost_Closed);
            this.authenticator = this.CremaHost.GetService(typeof(Authenticator)) as Authenticator;
            this.address = address;
            this.Initialize(this.authenticator);
        }

        public async Task LogoutAsync()
        {
            if (this.authenticator == null)
                throw new Exception("로그인되어 있지 않습니다.");

            await this.CremaHost.CloseAsync(this.token);
            this.authenticator = null;
            this.token = Guid.Empty;
            this.Release();
        }

        private static string SecureStringToString(SecureString value)
        {
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        private void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            this.Release();
        }

        internal static void Validate(SecureString value1, SecureString value2)
        {
            if (SecureStringToString(value1) != SecureStringToString(value2))
                throw new Exception("암호가 일치하지 않습니다.");
        }
    }
}
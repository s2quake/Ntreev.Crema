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

using JSSoft.Crema.Services;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleDrive))]
    public sealed class DomainsConsoleDrive : ConsoleDriveBase
    {
        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        internal DomainsConsoleDrive(ICremaHost cremaHost)
            : base("domains")
        {
            this.cremaHost = cremaHost;
        }

        public override Task<object> GetObjectAsync(Authentication authentication, string path)
        {
            throw new NotImplementedException();
        }

        protected override Task OnCreateAsync(Authentication authentication, string path, string name)
        {
            throw new NotImplementedException();
        }

        protected override Task OnMoveAsync(Authentication authentication, string path, string newPath)
        {
            throw new NotImplementedException();
        }

        protected override Task OnDeleteAsync(Authentication authentication, string path)
        {
            throw new NotImplementedException();
        }

        protected override Task OnSetPathAsync(Authentication authentication, string path)
        {
            return Task.Delay(1);
        }

        public override string[] GetPaths()
        {
            return this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.Select(item => item.Path).ToArray());
        }

        private IDomainContext DomainContext => this.cremaHost.GetService(typeof(IDomainContext)) as IDomainContext;
    }
}

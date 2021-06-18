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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSSoft.Crema.ServiceHosts;
using JSSoft.Crema.ServiceModel;

namespace JSSoft.Crema.Services
{
    class AuthenticationTokenCollection
    {
        private readonly HashSet<Guid> tokens = new();
        private readonly CremaHost cremaHost;

        public AuthenticationTokenCollection(CremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
        }

        public Task AddAsync(Guid token)
        {
            return this.Dispatcher.InvokeAsync(() => this.tokens.Add(token));
        }

        public Task RemoveManyAsync(Guid[] tokens)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in tokens)
                {
                    this.tokens.Remove(item);
                }
            });
        }

        public async Task LogoutAsync()
        {
            var tokens = this.tokens.ToArray();
            foreach (var item in tokens)
            {
                await this.Service.LogoutAsync(item);
            }
            await this.Dispatcher.InvokeAsync(() => this.tokens.Clear());
        }

        public CremaDispatcher Dispatcher => this.cremaHost.Dispatcher;

        public ICremaHostService Service => this.cremaHost.Service;
    }
}

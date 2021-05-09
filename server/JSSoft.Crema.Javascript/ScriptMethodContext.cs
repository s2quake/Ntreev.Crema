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
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript
{
    sealed class ScriptMethodContext : Dictionary<object, object>, IScriptMethodContext
    {
        public readonly static Guid LoginKey = Guid.Parse("7DBFE96F-0AB0-48F9-A8A5-5E70426F4C0E");
        private readonly ScriptContext scriptContext;
        private readonly ICremaHost cremaHost;
        private Authentication authentication;
        private string token;

        public ScriptMethodContext(ScriptContext scriptContext, ICremaHost cremaHost, Authentication authentication)
        {
            this.scriptContext = scriptContext;
            this.cremaHost = cremaHost;
            this.authentication = authentication;
            if (this.authentication != null)
                this.token = $"{Guid.NewGuid()}";
        }

        public Authentication GetAuthentication(IScriptMethod scriptMethod)
        {
            return this.authentication;
        }

        public async Task<string> LoginAsync(string userID, string password)
        {
            if (this.authentication != null)
                throw new Exception("이미 로그인되어 있습니다.");
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            var authenticationToken = await this.cremaHost.LoginAsync(userID, StringUtility.ToSecureString(password), false);
            this.authentication = await this.cremaHost.AuthenticateAsync(authenticationToken);
            this.token = $"{Guid.NewGuid()}";
            return this.token;
        }

        public async Task LogoutAsync(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (this.token != token)
                throw new ArgumentException("token is not valid.", nameof(token));
            if (this.Properties.ContainsKey(LoginKey) == false)
                throw new InvalidOperationException("this method is invalid operation");
            await this.cremaHost.LogoutAsync(this.authentication);
            this.authentication = null;
            this.token = null;
        }

        public Task LogoutAsync()
        {
            return this.LogoutAsync(this.token);
        }

        public IDictionary<object, object> Properties => this;

        public TextWriter Out => this.scriptContext.Out;
    }
}
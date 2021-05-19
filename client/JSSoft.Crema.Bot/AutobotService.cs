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
using System.ComponentModel.Composition;
using System.Security;

namespace JSSoft.Crema.Bot
{
    [InheritedExport(typeof(IPlugin))]
    [Export]
    public class AutobotService : AutobotServiceBase, IPlugin
    {
        private readonly ICremaHost cremaHost;
        private Authentication authentication;

        [ImportingConstructor]
        public AutobotService(ICremaHost cremaHost, [ImportMany] IEnumerable<ITaskProvider> taskProviders)
            : base(cremaHost, taskProviders)
        {
            this.cremaHost = cremaHost;
        }

        public override string ToString()
        {
            return $"{nameof(AutobotService)}";
        }

        public void Initialize(Authentication authentication)
        {
            this.authentication = authentication;
        }

        public void Release()
        {

        }

        protected override AutobotBase CreateInstance(string autobotID, SecureString password)
        {
            return new Autobot(this, "localhost", autobotID, password);
        }

        public string Name => "bot";

        public Guid ID => GuidUtility.FromName(this.Name);
    }
}
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

using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using System;
using System.Linq;

namespace JSSoft.Crema.Services
{
    public class DomainsEventArgs : EventArgs
    {
        public DomainsEventArgs(Authentication authentication, IDomain[] domains)
        {
            this.Domains = domains;
            this.DomainInfos = domains.Select(item => item.DomainInfo).ToArray();
            this.DomainStates = domains.Select(item => item.DomainState).ToArray();
            this.SignatureDate = authentication.SignatureDate;
        }

        public IDomain[] Domains { get; }

        public DomainInfo[] DomainInfos { get; }

        public DomainState[] DomainStates { get; }

        public string UserID => this.SignatureDate.ID;

        public DateTime DateTime => this.SignatureDate.DateTime;

        public SignatureDate SignatureDate { get; }
    }
}

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

using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.Domain
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(Domain))]
    class GetDomainInfoMethod : ScriptFuncTaskBase<string, IDictionary<string, object>>
    {
        [ImportingConstructor]
        public GetDomainInfoMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task<IDictionary<string, object>> OnExecuteAsync(string domainID)
        {
            var domain = await this.CremaHost.GetDomainAsync(Guid.Parse(domainID));
            var domainInfo = domain.DomainInfo;
            var props = new Dictionary<string, object>
            {
                { nameof(domainInfo.DomainID), $"{domainInfo.DomainID}" },
                { nameof(domainInfo.DataBaseID), $"{domainInfo.DataBaseID}" },
                { nameof(domainInfo.ItemPath), domainInfo.ItemPath },
                { nameof(domainInfo.ItemType), $"{domainInfo.ItemType}" },
                { nameof(domainInfo.DomainType), $"{domainInfo.DomainType}" },
                { nameof(domainInfo.CategoryPath), $"{domainInfo.CategoryPath}" },
                { CremaSchema.Creator, domainInfo.CreationInfo.ID },
                { CremaSchema.CreatedDateTime, domainInfo.CreationInfo.DateTime },
                { CremaSchema.Modifier, domainInfo.ModificationInfo.ID },
                { CremaSchema.ModifiedDateTime, domainInfo.ModificationInfo.DateTime }
            };
            return props;
        }
    }
}

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

using JSSoft.Crema.Data;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.TableTemplate
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(TableTemplate))]
    class AddTableTemplateColumnMethod : ScriptActionTaskBase<string, string, string, string, bool?>
    {
        [ImportingConstructor]
        public AddTableTemplateColumnMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task OnExecuteAsync(string domainID, string columnName,
            [TypeNamesDescriptionAttribute]
            string typeName, string comment, bool? isKey)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            var domain = await this.CremaHost.GetDomainAsync(Guid.Parse(domainID));
            var template = domain.Host as ITableTemplate;
            var authentication = this.Context.GetAuthentication(this);
            var column = await template.AddNewAsync(authentication);
            await column.SetNameAsync(authentication, columnName);
            await column.SetDataTypeAsync(authentication, typeName);
            if (comment != null)
                await column.SetCommentAsync(authentication, comment);
            if (isKey != null)
                await column.SetIsKeyAsync(authentication, isKey.Value);
            await template.EndNewAsync(authentication, column);
        }

        class TypeNamesDescriptionAttribute : DescriptionAttribute
        {
            public TypeNamesDescriptionAttribute()
                : base(string.Join(", ", CremaDataTypeUtility.GetBaseTypeNames()) + " or typePath(e.g., /categoryPath/typeName)")
            {

            }
        }
    }
}

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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Extensions;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Javascript.Methods.TableTemplate
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(TableTemplate))]
    class SetTableTemplateColumnPropertyMethod : ScriptActionTaskBase<string, string, TableColumnProperties, object>
    {
        [ImportingConstructor]
        public SetTableTemplateColumnPropertyMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task OnExecuteAsync(string domainID, string columnName, TableColumnProperties propertyName, object value)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            var domain = await this.CremaHost.GetDomainAsync(Guid.Parse(domainID));
            var template = domain.Host as ITableTemplate;
            var authentication = this.Context.GetAuthentication(this);
            var column = template[columnName];
            if (column == null)
                throw new ItemNotFoundException(columnName);
            if (propertyName == TableColumnProperties.Index)
            {
                await column.SetIndexAsync(authentication, Convert.ToInt32(value));
            }
            else if (propertyName == TableColumnProperties.IsKey)
            {
                await column.SetIsKeyAsync(authentication, (bool)value);
            }
            else if (propertyName == TableColumnProperties.IsUnique)
            {
                await column.SetIsUniqueAsync(authentication, (bool)value);
            }
            else if (propertyName == TableColumnProperties.Name)
            {
                await column.SetNameAsync(authentication, (string)value);
            }
            else if (propertyName == TableColumnProperties.DataType)
            {
                await column.SetDataTypeAsync(authentication, (string)value);
            }
            else if (propertyName == TableColumnProperties.DefaultValue)
            {
                await column.SetDefaultValueAsync(authentication, $"{value}");
            }
            else if (propertyName == TableColumnProperties.Comment)
            {
                await column.SetCommentAsync(authentication, (string)value);
            }
            else if (propertyName == TableColumnProperties.AutoIncrement)
            {
                await column.SetAutoIncrementAsync(authentication, (bool)value);
            }
            else if (propertyName == TableColumnProperties.Tags)
            {
                await column.SetTagsAsync(authentication, (TagInfo)(string)value);
            }
            else if (propertyName == TableColumnProperties.IsReadOnly)
            {
                await column.SetIsReadOnlyAsync(authentication, (bool)value);
            }
            else if (propertyName == TableColumnProperties.AllowNull)
            {
                await column.SetAllowNullAsync(authentication, (bool)value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

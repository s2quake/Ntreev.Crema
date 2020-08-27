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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.TableContent
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(TableContent))]
    class AddTableContentRowMethod : ScriptFuncTaskBase<string, string, IDictionary<string, object>, object[]>
    {
        [ImportingConstructor]
        public AddTableContentRowMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        [ReturnParameterName("keys")]
        protected override async Task<object[]> OnExecuteAsync(string domainID, string tableName, IDictionary<string, object> fields)
        {
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));
            var domain = await this.CremaHost.GetDomainAsync(Guid.Parse(domainID));
            var contents = domain.Host as IEnumerable<ITableContent>;
            var content = contents.FirstOrDefault(item => item.Dispatcher.Invoke(() => item.Table.Name) == tableName);
            if (content == null)
                throw new TableNotFoundException(tableName);
            var authentication = this.Context.GetAuthentication(this);
            var tableInfo = content.Table.TableInfo;
            var row = await content.AddNewAsync(authentication, null);
            foreach (var item in fields)
            {
                var typeName = tableInfo.Columns.First(i => i.DataType == item.Key).DataType;
                var type = CremaDataTypeUtility.GetType(typeName);
                var value = CremaConvert.ChangeType(item.Value, type);
                await row.SetFieldAsync(authentication, item.Key, value);
            }
            await content.EndNewAsync(authentication, row);
            return tableInfo.Columns.Select(item => row[item.Name]).ToArray();
        }
    }
}

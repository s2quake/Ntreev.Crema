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

using Newtonsoft.Json.Schema;
using JSSoft.Crema.Commands.Consoles.Serializations;
using JSSoft.Crema.Data;
using JSSoft.Crema.Services;
using System.Linq;

namespace JSSoft.Crema.Commands.Consoles.TableContent
{
    abstract class ContentCommandBase : ConsoleCommandAsyncBase
    {
        protected ContentCommandBase(string name)
            : base(name)
        {

        }

        public ITableContent Content => this.CommandContext.Target as ITableContent;

        internal JSchema CreateSchema(ColumnInfo[] columns)
        {
            var schema = new JSchema();
            foreach (var item in columns)
            {
                var itemSchema = this.CreateSchema(item.DataType);
                itemSchema.Description = item.Comment;
                schema.Properties.Add(item.Name, itemSchema);
            }

            var required = columns.Where(item => item.AllowNull == false).Select(item => item.Name);
            foreach (var item in required)
            {
                schema.Required.Add(item);
            }
            return schema;
        }

        internal JSchema CreateSchema(string dataType)
        {
            if (CremaDataTypeUtility.IsBaseType(dataType) == true)
            {
                return JsonSchemaUtility.GetSchema(CremaDataTypeUtility.GetType(dataType));
            }
            else
            {
                var memberNames = this.Content.Dispatcher.Invoke(() =>
                {
                    var table = this.Content.Table;
                    var typeContext = table.GetService(typeof(ITypeContext)) as ITypeContext;
                    var type = typeContext[dataType] as IType;
                    return type.TypeInfo.Members.Select(i => i.Name).ToArray();
                });
                return JsonSchemaUtility.CreateSchema(memberNames);
            }
        }
    }
}

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

using JSSoft.Crema.Commands.Consoles.Serializations;
using JSSoft.Crema.Data;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles.TableContent
{
    [Export(typeof(IConsoleCommand))]
    [Category(nameof(ITableContent))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [CommandStaticProperty(typeof(TextEditorHost))]
    [ResourceDescription("../Resources")]
    class EditCommand : ContentCommandBase
    {
        public EditCommand()
            : base("edit")
        {

        }

        [CommandPropertyArray]
        public string[] Keys
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var tableInfo = this.Content.Dispatcher.Invoke(() => this.Content.Table.TableInfo);
            var keys = tableInfo.Columns.Where(item => item.IsKey).ToArray();
            var schema = this.CreateSchema(tableInfo.Columns);

            var fieldList = new List<object>();
            for (var i = 0; i < this.Keys.Length; i++)
            {
                var key = keys[i];
                var keyText = this.Keys[i];
                var type = CremaDataTypeUtility.IsBaseType(key.DataType) ? CremaDataTypeUtility.GetType(key.DataType) : typeof(string);
                var value = CremaConvert.ChangeType(keyText, type);
                fieldList.Add(value);
            }

            var fields = new JsonPropertiesInfo();
            var authentication = this.CommandContext.GetAuthentication(this);
            var tableRow = await this.Content.FindAsync(authentication, fieldList.ToArray());

            this.Content.Dispatcher.Invoke(() =>
            {
                foreach (var item in tableInfo.Columns)
                {
                    var field = tableRow[item.Name];
                    if (field != null || item.AllowNull == false)
                        fields.Add(item.Name, field);
                }
            });

            var result = fields.Execute(schema);
            if (result == null)
                return;

            foreach (var item in fields)
            {
                if (result.ContainsKey(item.Key) == false)
                {
                    await tableRow.SetFieldAsync(authentication, item.Key, DBNull.Value);
                }
            }

            foreach (var item in result)
            {
                if (tableInfo.Columns.Any(i => i.Name == item.Key) == false)
                    continue;
                var value1 = tableRow[item.Key];
                var value2 = item.Value;
                if (object.Equals(value1, value2) == true)
                    continue;

                await tableRow.SetFieldAsync(authentication, item.Key, item.Value);
            }
        }

        public override string[] GetCompletions(CommandCompletionContext completionContext)
        {
            var domain = this.Content.Dispatcher.Invoke(() => this.Content.Domain);
            var tableInfo = this.Content.Dispatcher.Invoke(() => this.Content.Table.TableInfo);

            return domain.Dispatcher.Invoke(() =>
            {
                var dataSet = domain.Source as CremaDataSet;
                var dataTable = dataSet.Tables[tableInfo.Name];

                if (completionContext.Arguments.Length >= dataTable.PrimaryKey.Length)
                    return null;

                var expItems = new List<string>();
                for (var i = 0; i < completionContext.Arguments.Length; i++)
                {
                    expItems.Add($"{dataTable.PrimaryKey[i].ColumnName}='{completionContext.Arguments[i]}'");
                }

                var expression = string.Join(" AND ", expItems);
                var query = from item in dataTable.Select(expression)
                            let value = CremaConvert.ChangeType(item[dataTable.PrimaryKey[completionContext.Arguments.Length]], typeof(string)) as string
                            select value;
                return query.ToArray();
            });
        }
    }
}

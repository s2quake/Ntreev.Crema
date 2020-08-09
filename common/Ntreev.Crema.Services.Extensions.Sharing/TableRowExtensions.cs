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

using Ntreev.Crema.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TableRowExtensions
    {
        public static Task<string> GenerateFilterExpressionAsync(this ITableRow tableRow, params string[] columnNames)
        {
            return tableRow.Dispatcher.InvokeAsync(() =>
            {
                var fieldList = new List<object>(columnNames.Length);
                foreach (var item in columnNames)
                {
                    var field = tableRow[item];
                    fieldList.Add(CremaDataExtensions.GenerateFieldExpression(item, field));
                }
                return string.Join(" and ", fieldList);
            });
        }

        public static Task<object[]> GetKeysAsync(this ITableRow tableRow)
        {
            return tableRow.Dispatcher.InvokeAsync(() =>
            {
                var content = tableRow.Content;
                var table = content.Table;
                var tableInfo = table.TableInfo;
                var keyList = new List<object>();
                foreach (var item in tableInfo.Columns)
                {
                    if (item.IsKey == true)
                    {
                        keyList.Add(tableRow[item.Name]);
                    }
                }
                return keyList.ToArray();
            });
        }
    }
}

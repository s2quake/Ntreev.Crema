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

using JSSoft.Library.Random;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class TableExtensions
    {
        public static async Task<ITable> AddRandomChildTableAsync(this ITable table, Authentication authentication, DataBaseSettings settings)
        {
            var copyData = RandomUtility.NextBoolean();
            var template = await table.NewTableAsync(authentication);
            await template.InitializeRandomAsync(authentication);
            await template.EndEditAsync(authentication);
            if (template.Target is ITable[] tables)
            {
                foreach (var item in tables)
                {
                    var minCount = settings.TableContext.MinRowCount;
                    var maxCount = settings.TableContext.MaxRowCount;
                    var count = RandomUtility.Next(minCount, maxCount);
                    await AddRandomRowsAsync(item, authentication, count);
                }
                return tables.First();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task AddRandomRowsAsync(this ITable table, Authentication authentication, int tryCount)
        {
            var target = table.Parent ?? table;
            var targetContent = target.Content;
            await targetContent.BeginEditAsync(authentication);
            await targetContent.EnterEditAsync(authentication);
            var failedCount = 0;
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    var parentRow = target != table ? targetContent.RandomOrDefault() : null;
                    await AddRandomRowAsync(table, parentRow, authentication);
                }
                catch
                {
                    failedCount++;
                }
                if (failedCount > 3)
                    break;
            }
            await targetContent.LeaveEditAsync(authentication);
            await targetContent.EndEditAsync(authentication);
        }

        public static async Task<ITableRow> AddRandomRowAsync(this ITable table, ITableRow parentRow, Authentication authentication)
        {
            if (table.Parent != null && parentRow == null)
                return null;

            var tableRow = await table.Content.AddNewAsync(authentication, parentRow?.ID);
            await tableRow.InitializeRandomAsync(authentication);
            await table.Content.EndNewAsync(authentication, tableRow);
            return tableRow;
        }
    }
}

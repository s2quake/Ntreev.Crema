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
using Ntreev.Crema.Services;
using Ntreev.Library.Random;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Ntreev.Library.ObjectModel;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Random
{
    public static class TableColumnExtensions
    {
        public static async Task InitializeRandomAsync(this ITableColumn tableColumn, Authentication authentication)
        {
            var template = tableColumn.Template;
            var table = tableColumn.Template.Target;

            if (RandomUtility.Within(75) == true)
            {
                await tableColumn.SetDataTypeAsync(authentication, CremaDataTypeUtility.GetBaseTypeNames().Random(item => item != typeof(bool).GetTypeName()));
            }
            else 
            {
                await tableColumn.SetDataTypeAsync(authentication, template.SelectableTypes.Random());
            }

            if (template.Count == 0)
            {
                await tableColumn.SetIsKeyAsync(authentication, true);
            }
            else if (RandomUtility.Within(10) && tableColumn.DataType != typeof(bool).GetTypeName())
            {
                await tableColumn.SetIsKeyAsync(authentication, true);
                await tableColumn.SetIsUniqueAsync(authentication, RandomUtility.Within(75));
            }

            if (RandomUtility.Within(25) && tableColumn.DataType != typeof(bool).GetTypeName())
            {
                var unique = RandomUtility.Within(75);
                if (unique != false || template.PrimaryKey.Count() != 1)
                {
                    await tableColumn.SetIsUniqueAsync(authentication, unique);
                }
            }

            if (RandomUtility.Within(25) == true)
            {
                await tableColumn.SetCommentAsync(authentication, RandomUtility.NextString());
            }

            if (RandomUtility.Within(25) == true)
            {
                await tableColumn.SetDefaultValueAsync(authentication, await tableColumn.GetRandomStringAsync());
            }

            if (CremaDataTypeUtility.CanUseAutoIncrement(tableColumn.DataType) == true && tableColumn.DefaultValue == null)
            {
                await tableColumn.SetAutoIncrementAsync(authentication, RandomUtility.NextBoolean());
            }

            if (RandomUtility.Within(5) == true)
            {
                await tableColumn.SetIsReadOnlyAsync(authentication, true);
            }
        }

        public static async Task ModifyRandomValueAsync(this ITableColumn tableColumn, Authentication authentication)
        {
            if (RandomUtility.Within(75) == true)
            {
                await SetRandomNameAsync(tableColumn, authentication);
            }
            else if (RandomUtility.Within(75) == true)
            {
                await SetRandomValueAsync(tableColumn, authentication);
            }
            else
            {
                await SetRandomCommentAsync(tableColumn, authentication);
            }
        }

        public static async Task ExecuteRandomTaskAsync(this ITableColumn tableColumn, Authentication authentication)
        {
            if (RandomUtility.Within(75) == true)
            {
                await SetRandomNameAsync(tableColumn, authentication);
            }
            else if (RandomUtility.Within(75) == true)
            {
                await SetRandomValueAsync(tableColumn, authentication);
            }
            else
            {
                await SetRandomCommentAsync(tableColumn, authentication);
            }
        }

        public static Task SetRandomNameAsync(this ITableColumn tableColumn, Authentication authentication)
        {
            var newName = RandomUtility.NextIdentifier();
            return tableColumn.SetNameAsync(authentication, newName);
        }

        public static async Task SetRandomValueAsync(this ITableColumn tableColumn, Authentication authentication)
        {
            //if (tableColumn.Template.IsFlag == true)
            //{
            //    tableColumn.SetValue(authentication, RandomUtility.NextBit());
            //}
            //else
            //{
            //    tableColumn.SetValue(authentication, RandomUtility.NextLong(long.MaxValue));
            //}
            await Task.Delay(0);
        }

        public static async Task SetRandomCommentAsync(this ITableColumn tableColumn, Authentication authentication)
        {
            if (RandomUtility.Within(50) == true)
            {
                await tableColumn.SetCommentAsync(authentication, RandomUtility.NextString());
            }
            else
            {
                await tableColumn.SetCommentAsync(authentication, string.Empty);
            }
        }

        public static async Task<string> GetRandomStringAsync(this ITableColumn tableColumn)
        {
            if (tableColumn.DefaultValue != null && RandomUtility.Next(3) == 0)
            {
                return null;
            }
            else if (tableColumn.AllowNull == true && RandomUtility.Next(4) == 0)
            {
                return null;
            }
            else
            {
                var template = tableColumn.Template;
                var dataType = tableColumn.DataType;
                if (CremaDataTypeUtility.IsBaseType(dataType) == false)
                {
                    var domain = template.Domain;
                    var typeContext = domain.GetService(typeof(ITypeContext)) as ITypeContext;
                    var type = typeContext.Types[dataType];
                    return await type.GetRandomStringAsync();
                }
                else
                {
                    var value = RandomUtility.Next(CremaDataTypeUtility.GetType(dataType));
                    return CremaConvert.ChangeType(value, typeof(string)) as string;
                }
            }
        }
    }
}

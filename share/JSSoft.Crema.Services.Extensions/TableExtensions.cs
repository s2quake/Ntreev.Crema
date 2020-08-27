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

using JSSoft.Library.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class TableExtensions
    {
        public static Task<IType[]> GetTypesAsync(this ITable table, Func<IType, bool> predicate)
        {
            return table.Dispatcher.InvokeAsync(() => GetTypes(table, predicate));
        }

        internal static IType[] GetTypes(ITable table)
        {
            return GetTypes(table, item => true);
        }

        internal static IType[] GetTypes(ITable table, Func<IType, bool> predicate)
        {
            table.Dispatcher.VerifyAccess();
            var typeContext = table.GetService(typeof(ITypeContext)) as ITypeContext;
            var typeList = new List<IType>(typeContext.Count());
            var tableInfo = table.TableInfo;
            foreach (var item in tableInfo.Columns)
            {
                if (typeContext[item.DataType] is IType type)
                {
                    typeList.Add(type);
                }
            }
            var query = from item in typeList.Distinct()
                        where predicate(item)
                        select item;
            return query.ToArray();
        }

        internal static ITable[] GetRelationTables(ITable table, Func<ITable, bool> _)
        {
            table.Dispatcher.VerifyAccess();
            while (table.Parent != null)
            {
                table = table.Parent;
            };

            return EnumerableUtility.FamilyTree(table, item => item.Childs).ToArray();
        }

        public static Task<ITable[]> GetRelationTablesAsync(this ITable table, Func<ITable, bool> predicate)
        {
            return table.Dispatcher.InvokeAsync(() => GetRelationTables(table, predicate));
        }

        internal static ITable[] GetAllRelationTables(ITable table)
        {
            return GetAllRelationTables(table, item => true);
        }

        internal static ITable[] GetAllRelationTables(ITable table, Func<ITable, bool> _)
        {
            while (table.Parent != null)
            {
                table = table.Parent;
            };

            if (table.TemplatedParent != null)
                table = table.TemplatedParent;

            return Collect(table).OrderBy(item => item.Name).ToArray();
        }

        public static Task<ITable[]> GetAllRelationTablesAsync(this ITable table, Func<ITable, bool> predicate)
        {
            return table.Dispatcher.InvokeAsync(() => GetAllRelationTables(table, predicate));
        }

        private static IEnumerable<ITable> Collect(ITable table)
        {
            yield return table;

            foreach (var item in table.Childs)
            {
                foreach (var i in Collect(item))
                {
                    yield return i;
                }
            }

            foreach (var item in table.DerivedTables)
            {
                yield return item;
            }
        }
    }
}

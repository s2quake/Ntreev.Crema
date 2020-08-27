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
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class TableCategoryExtensions
    {
        /// <summary>
        /// 대상 카테고리의 테이블 및 와 하위 카테고리의 테이블의 모든 목록을 가져옵니다.
        /// </summary>
        public static Task<ITable[]> GetAllTablesAsync(this ITableCategory category, Func<ITable, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in EnumerableUtility.FamilyTree<ITableItem, ITable>(category as ITableItem, item => item.Childs)
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }

        public static Task<ITable[]> GetAllRelationTablesAsync(this ITableCategory category, Func<ITable, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var tables = from item in EnumerableUtility.FamilyTree<ITableItem, ITable>(category as ITableItem, item => item.Childs)
                             select item;
                var allTables = tables.SelectMany(item => TableExtensions.GetAllRelationTables(item)).Distinct().OrderBy(item => item.Name);
                var query = from item in allTables
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }

        /// <summary>
        /// 대상 카테고리의 테이블 및 와 하위 카테고리의 테이블에서 사용되고 있는 타입의 목록을 가져옵니다.
        /// </summary>
        public static Task<IType[]> GetAllUsingTypesAsync(this ITableCategory category, Func<IType, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var tables = EnumerableUtility.FamilyTree<ITableItem, ITable>(category as ITableItem, item => item.Childs);
                var types = tables.SelectMany(item => TableExtensions.GetTypes(item)).Distinct();
                var query = from item in types
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }
    }
}

using Ntreev.Library.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Extensions
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

using Ntreev.Library.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TypeCategoryExtensions
    {
        public static Task<IType[]> GetAllTypesAsync(this ITypeCategory category, Func<IType, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in EnumerableUtility.FamilyTree<ITypeItem, IType>(category as ITypeItem, item => item.Childs)
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }

        //public static Task<ITable[]> GetAllRelationTablesAsync(this ITableCategory category, Func<ITable, bool> predicate)
        //{
        //    return category.Dispatcher.InvokeAsync(() =>
        //    {
        //        var tables = from item in EnumerableUtility.FamilyTree<ITableItem, ITable>(category as ITableItem, item => item.Childs)
        //                     select item;
        //        var allTables = tables.SelectMany(item => TableExtensions.GetAllRelationTables(item)).Distinct().OrderBy(item => item.Name);
        //        var query = from item in allTables
        //                    where predicate(item)
        //                    select item;
        //        return query.ToArray();
        //    });
        //}

        public static Task<ITable[]> GetAllUsingTablesAsync(this ITypeCategory category, Func<ITable, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var types = EnumerableUtility.FamilyTree<ITypeItem, IType>(category as ITypeItem, item => item.Childs);
                var tables = types.SelectMany(item => TypeExtensions.GetTables(item)).Distinct();
                var query = from item in tables
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }
    }
}

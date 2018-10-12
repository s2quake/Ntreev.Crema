using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Library.Linq;

namespace Ntreev.Crema.Services.Extensions
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

        internal static ITable[] GetRelationTables(ITable table, Func<ITable, bool> predicate)
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

            internal static ITable[] GetAllRelationTables(ITable table, Func<ITable, bool> predicate)
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

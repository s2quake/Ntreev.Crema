using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TypeExtensions
    {
        public static Task<ITable[]> GetTablesAsync(this IType type, Func<ITable, bool> predicate)
        {
            return type.Dispatcher.InvokeAsync(() => GetTables(type, predicate));
        }

        internal static ITable[] GetTables(IType type)
        {
            return GetTables(type, item => true);
        }

            internal static ITable[] GetTables(IType type, Func<ITable, bool> predicate)
        {
                var tables = type.GetService(typeof(ITableCollection)) as ITableCollection;
                var tableList = new List<ITable>(tables.Count);
                var path = type.Path;
                foreach (var item in tables)
                {
                    var tableInfo = item.TableInfo;
                    if (IsUsingType(tableInfo, path) == true)
                    {
                        tableList.Add(item);
                    }
                }
                var query = from item in tableList
                            where predicate(item)
                            select item;
                return query.ToArray();
        }

        private static bool IsUsingType(TableInfo tableInfo, string typePath)
        {
            foreach (var item in tableInfo.Columns)
            {
                if (item.DataType == typePath)
                    return true;
            }
            return false;
        }
    }
}

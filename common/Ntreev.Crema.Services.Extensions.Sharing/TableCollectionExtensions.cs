using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TableCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this ITableCollection tables, string tableName)
        {
            return tables.Dispatcher.InvokeAsync(() => tables.Contains(tableName));
        }
    }
}

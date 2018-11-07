using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class DataBaseContextExtensions
    {
        public static Task<bool> ContainsAsync(this IDataBaseContext dataBaseContext, string dataBaseName)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Contains(dataBaseName));
        }
    }
}

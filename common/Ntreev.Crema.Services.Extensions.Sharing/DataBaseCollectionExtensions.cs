using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class DataBaseCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this IDataBaseCollection dataBases, string dataBaseName)
        {
            return dataBases.Dispatcher.InvokeAsync(() => dataBases.Contains(dataBaseName));
        }
    }
}

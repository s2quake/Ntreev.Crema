using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Library.Linq;

namespace Ntreev.Crema.Services.Extensions
{
    public static class DataBaseExtensions
    {
        public static Task<bool> ContainsAsync(this IDataBase dataBase, Authentication authentication)
        {
            return dataBase.Dispatcher.InvokeAsync(() => dataBase.Contains(authentication));
        }
    }
}

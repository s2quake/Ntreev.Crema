using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TypeCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this ITypeCollection types, string typeName)
        {
            return types.Dispatcher.InvokeAsync(() => types.Contains(typeName));
        }
    }
}

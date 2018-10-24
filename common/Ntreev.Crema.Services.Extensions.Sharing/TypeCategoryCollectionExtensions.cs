using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TypeCategoryCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this ITypeCategoryCollection categories, string categoryPath)
        {
            return categories.Dispatcher.InvokeAsync(() => categories.Contains(categoryPath));
        }
    }
}

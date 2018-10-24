using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TableTemplateExtensions
    {
        public static Task<bool> ContainsAsync(this ITableTemplate template, string columnName)
        {
            return template.Dispatcher.InvokeAsync(() => template.Contains(columnName));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class DomainCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this IDomainCollection domains, Guid domainID)
        {
            return domains.Dispatcher.InvokeAsync(() => domains.Contains(domainID));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class DomainUserCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this IDomainUserCollection users, string userID)
        {
            return users.Dispatcher.InvokeAsync(() => users.Contains(userID));
        }
    }
}

using JSSoft.Communication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntreev.Crema.Services
{
    class ClientContext : ClientContextBase
    {
        public ClientContext(params IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {

        }
    }
}

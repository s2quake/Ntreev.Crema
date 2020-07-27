using JSSoft.Communication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntreev.Crema.ServiceHosts
{
    class ServerContext : ServerContextBase
    {
        public ServerContext(params IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {

        }
    }
}

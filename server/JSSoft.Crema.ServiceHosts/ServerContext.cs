using JSSoft.Communication;

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

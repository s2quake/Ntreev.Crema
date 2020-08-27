using JSSoft.Communication;

namespace JSSoft.Crema.ServiceHosts
{
    class ServerContext : ServerContextBase
    {
        public ServerContext(params IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {

        }
    }
}

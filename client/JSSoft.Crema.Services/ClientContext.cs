using JSSoft.Communication;

namespace JSSoft.Crema.Services
{
    class ClientContext : ClientContextBase
    {
        public ClientContext(params IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {

        }
    }
}

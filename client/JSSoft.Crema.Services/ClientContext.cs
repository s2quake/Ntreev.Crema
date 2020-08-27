using JSSoft.Communication;

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

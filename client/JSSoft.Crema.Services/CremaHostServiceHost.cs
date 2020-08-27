using JSSoft.Communication;
using JSSoft.Crema.ServiceHosts;

namespace JSSoft.Crema.Services
{
    class CremaHostServiceHost : ClientServiceHostBase<ICremaHostService, ICremaHostEventCallback>
    {
        private readonly CremaHost cremaHost;

        public CremaHostServiceHost(CremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
        }

        protected override ICremaHostEventCallback CreateCallback(ICremaHostService service)
        {
            this.cremaHost.Service = service;
            return this.cremaHost;
        }
    }
}

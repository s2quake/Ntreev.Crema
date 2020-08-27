using JSSoft.Communication;
using JSSoft.Crema.ServiceHosts.Domains;

namespace JSSoft.Crema.Services.Domains
{
    class DomainContextServiceHost : ClientServiceHostBase<IDomainContextService, IDomainContextEventCallback>
    {
        private readonly DomainContext domainContext;

        public DomainContextServiceHost(DomainContext domainContext)
        {
            this.domainContext = domainContext;
        }

        protected override IDomainContextEventCallback CreateCallback(IDomainContextService service)
        {
            this.domainContext.Service = service;
            return this.domainContext;
        }
    }
}

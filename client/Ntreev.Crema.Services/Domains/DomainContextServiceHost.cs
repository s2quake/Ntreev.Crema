using JSSoft.Communication;
using Ntreev.Crema.ServiceHosts.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntreev.Crema.Services.Domains
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

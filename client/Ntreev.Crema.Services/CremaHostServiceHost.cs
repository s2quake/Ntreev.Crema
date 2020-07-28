using JSSoft.Communication;
using Ntreev.Crema.ServiceHosts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntreev.Crema.Services
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

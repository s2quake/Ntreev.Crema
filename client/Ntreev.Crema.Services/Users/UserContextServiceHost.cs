using JSSoft.Communication;
using Ntreev.Crema.ServiceHosts.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntreev.Crema.Services.Users
{
    class UserContextServiceHost : ClientServiceHostBase<IUserContextService, IUserContextEventCallback>
    {
        private readonly UserContext userContext;

        public UserContextServiceHost(UserContext userContext)
        {
            this.userContext = userContext;
        }

        public IUserContextService Service { get; private set; }

        protected override IUserContextEventCallback CreateCallback(IUserContextService service)
        {
            this.userContext.Service = service;
            return this.userContext;
        }
    }
}

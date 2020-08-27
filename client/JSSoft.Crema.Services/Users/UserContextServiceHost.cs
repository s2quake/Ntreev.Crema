using JSSoft.Communication;
using JSSoft.Crema.ServiceHosts.Users;

namespace JSSoft.Crema.Services.Users
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

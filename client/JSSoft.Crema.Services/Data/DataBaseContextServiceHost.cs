using JSSoft.Communication;
using JSSoft.Crema.ServiceHosts.Data;

namespace JSSoft.Crema.Services.Data
{
    class DataBaseContextServiceHost : ClientServiceHostBase<IDataBaseContextService, IDataBaseContextEventCallback>
    {
        private readonly DataBaseContext dataBaseContext;

        public DataBaseContextServiceHost(DataBaseContext dataBaseContext)
        {
            this.dataBaseContext = dataBaseContext;
        }

        protected override IDataBaseContextEventCallback CreateCallback(IDataBaseContextService service)
        {
            this.dataBaseContext.Service = service;
            return this.dataBaseContext;
        }
    }
}

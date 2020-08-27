using JSSoft.Communication;
using Ntreev.Crema.ServiceHosts.Data;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseServiceHost : ClientServiceHostBase<IDataBaseService, IDataBaseEventCallback>
    {
        private readonly DataBase dataBase;

        public DataBaseServiceHost(DataBase dataBase)
        {
            this.dataBase = dataBase;
        }

        protected override IDataBaseEventCallback CreateCallback(IDataBaseService service)
        {
            this.dataBase.Service = service;
            return this.dataBase;
        }
    }
}

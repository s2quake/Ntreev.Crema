using JSSoft.Communication;
using Ntreev.Crema.ServiceHosts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ntreev.Crema.Services.Data
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

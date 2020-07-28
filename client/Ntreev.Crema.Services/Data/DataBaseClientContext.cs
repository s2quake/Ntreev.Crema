using System;
using System.Collections.Generic;
using System.Text;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseClientContext : ClientContext
    {
        public DataBaseClientContext(DataBaseServiceHost host)
            : base(host)
        {

        }
    }
}

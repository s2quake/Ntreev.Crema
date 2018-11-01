using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    class UserDataSet
    {
        public string[] ItemPaths { get; set; }

        public UserSerializationInfo[] Infos { get; set; }

        public SignatureDateProvider SignatureDateProvider { get; set; }
    }
}

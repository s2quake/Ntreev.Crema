using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Users.Serializations;
using Ntreev.Library;
using Ntreev.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    class UserSet
    {
        public string[] ItemPaths { get; set; }

        public UserSerializationInfo[] Infos { get; set; }

        public SignatureDateProvider SignatureDateProvider { get; set; }

        public static readonly UserSet Empty = new UserSet()
        {
            ItemPaths = new string[] { },
            Infos = new UserSerializationInfo[] { },
            SignatureDateProvider = SignatureDateProvider.Default,
        };
    }
}

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    public static class AuthorityExtensions
    {
        public static SecureString GetPassword(this Authority authority)
        {
            return $"{authority}".ToLower().ToSecureString();
        }
    }
}

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    public static class AuthorityUtility
    {
        public static UserFlags ToUserFlags(Authority authority)
        {
            switch (authority)
            {
                case Authority.Admin:
                    return UserFlags.Admin;
                case Authority.Member:
                    return UserFlags.Member;
                case Authority.Guest:
                    return UserFlags.Guest;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

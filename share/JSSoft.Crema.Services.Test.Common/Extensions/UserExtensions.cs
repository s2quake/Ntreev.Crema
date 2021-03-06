﻿using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Common.Extensions
{
    static class UserExtensions
    {
        public static SecureString GetPassword(this IUser user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            return user.Authority.GetPassword();
        }

        public static SecureString GetNextPassword(this IUser user)
        {
            return $"{RandomUtility.NextIdentifier()}".ToLower().ToSecureString();
        }
    }
}

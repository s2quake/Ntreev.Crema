using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class UserCollectionExtensions
    {
        public static async Task<IUser> BanRandomUserAsync(this IUserCollection userCollection, Authentication authentication)
        {
            var user = await userCollection.GetRandomUserAsync(Predicate);
            var message = RandomUtility.NextString();
            await user.BanAsync(authentication, message);
            return user;

            static bool Predicate(IUser user)
            {
                if (user.Authority == Authority.Admin)
                    return false;
                if (user.BanInfo.IsBanned == true)
                    return false;
                return true;
            }
        }

        public static async Task<IUser> BanRandomUserAsync(this IUserCollection userCollection, Authentication authentication, Authority authority)
        {
            var user = await userCollection.GetRandomUserAsync(authority, Predicate);
            var message = RandomUtility.NextString();
            await user.BanAsync(authentication, message);
            return user;

            static bool Predicate(IUser user)
            {
                if (user.BanInfo.IsBanned == true)
                    return false;
                return true;
            }
        }
    }
}

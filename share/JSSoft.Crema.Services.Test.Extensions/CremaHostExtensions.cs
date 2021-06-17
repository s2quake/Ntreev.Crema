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
    static partial class CremaHostExtensions
    {
        public static async Task<Authentication> LoginAgainAsync(this ICremaHost cremaHost, Authentication authentication)
        {
            if (cremaHost.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                var user = await userCollection.GetUserAsync(authentication.ID);
                var name = user.ID;
                var password = user.GetPassword();
                await cremaHost.LogoutAsync(authentication);
                var token = await cremaHost.LoginAsync(name, password);
                return await cremaHost.AuthenticateAsync(token);
            }
            throw new NotImplementedException();
        }

        public static Task<Authentication> LoginRandomAsync(this ICremaHost cremaHost)
        {
            var items = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            return LoginRandomAsync(cremaHost, items.Random());
        }

        public static Task<Authentication> LoginRandomAsync(this ICremaHost cremaHost, Authority authority)
        {
            return LoginRandomAsync(cremaHost, authority, DefaultPredicate);
        }

        public static async Task<Authentication> LoginRandomAsync(this ICremaHost cremaHost, Authority authority, Func<IUser, bool> predicate)
        {
            if (cremaHost.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                var user = await userCollection.GetRandomUserAsync(Test);
                var name = user.ID;
                var password = user.GetPassword();
                var token = await cremaHost.LoginAsync(name, password);
                return await cremaHost.AuthenticateAsync(token);
            }
            throw new NotImplementedException();

            bool Test(IUser user)
            {
                if (user.BanInfo.Path != string.Empty)
                    return false;
                if (user.UserState == UserState.Online)
                    return false;
                if (user.Authority != authority)
                    return false;
                return predicate(user);
            }
        }

        public static async Task<Authentication[]> LoginRandomManyAsync(this ICremaHost cremaHost, int count)
        {
            var authenticationList = new List<Authentication>(count);
            for (var i = 0; i < count; i++)
            {
                var authentication = await cremaHost.LoginRandomAsync();
                authenticationList.Add(authentication);
            }
            return authenticationList.ToArray();
        }

        private static bool DefaultPredicate(IUser _) => true;
    }
}

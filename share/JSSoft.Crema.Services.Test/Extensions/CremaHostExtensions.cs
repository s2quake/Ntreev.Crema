using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Library;
using JSSoft.Library.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class CremaHostExtensions
    {
        private static readonly Dictionary<Authentication, Guid> tokenByAuthentication = new();

        public static async Task<Authentication> StartAsync(this ICremaHost cremaHost)
        {
            var token = await cremaHost.OpenAsync();
            var authenticationToken = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication = await cremaHost.AuthenticateAsync(authenticationToken);
            tokenByAuthentication.Add(authentication, token);
            return authentication;
        }

        public static async Task<Authentication> StartRandomAsync(this ICremaHost cremaHost)
        {
            var token = await cremaHost.OpenAsync();
            var authentication = await cremaHost.LoginRandomAsync();
            tokenByAuthentication.Add(authentication, token);
            return authentication;
        }

        public static async Task<Authentication> StartRandomAsync(this ICremaHost cremaHost, Authority authority)
        {
            var token = await cremaHost.OpenAsync();
            var authentication = await cremaHost.LoginRandomAsync(authority);
            tokenByAuthentication.Add(authentication, token);
            return authentication;
        }

        public static async Task StopAsync(this ICremaHost cremaHost, Authentication authentication)
        {
            var token = tokenByAuthentication[authentication];
            await cremaHost.LogoutAsync(authentication);
            await cremaHost.CloseAsync(token);
            tokenByAuthentication.Remove(authentication);
        }

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

        public static async Task<Authentication> LoginRandomAsync(this ICremaHost cremaHost, Authority authority)
        {
            if (cremaHost.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                var user = await userCollection.GetRandomUserAsync(item => Predicate(item, authority));
                var name = user.ID;
                var password = user.GetPassword();
                var token = await cremaHost.LoginAsync(name, password);
                return await cremaHost.AuthenticateAsync(token);
            }
            throw new NotImplementedException();

            static bool Predicate(IUser user, Authority authority)
            {
                if (user.BanInfo.Path != string.Empty)
                    return false;
                if (user.UserState == UserState.Online)
                    return false;
                if (user.Authority != authority)
                    return false;
                return true;
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
    }
}

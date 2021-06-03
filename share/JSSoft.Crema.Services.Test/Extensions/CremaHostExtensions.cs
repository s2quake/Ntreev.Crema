using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
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

        public static Task<Authentication> LoginRandomAsync(this ICremaHost cremaHost)
        {
            var items = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            return LoginRandomAsync(cremaHost, items.Random());
        }

        public static async Task<Authentication> LoginRandomAsync(this ICremaHost cremaHost, Authority authority)
        {
            if (cremaHost.GetService(typeof(IUserCollection)) is IUserCollection userCollection)
            {
                var user = await userCollection.Dispatcher.InvokeAsync(() => userCollection.Random(item => item.UserState == UserState.None && item.Authority == authority));
                var name = user.ID;
                var password = user.GetPassword();
                var token = await cremaHost.LoginAsync(name, password);
                return await cremaHost.AuthenticateAsync(token);
            }
            throw new NotImplementedException();
        }
    }
}

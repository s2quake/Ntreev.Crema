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
        private static readonly Dictionary<Authentication, Guid> tokenByAuthentication = new();

        public static Task<Authentication> StartAsync(this ICremaHost cremaHost)
        {
            return StartAsync(cremaHost, Authentication.AdminID);
        }

        public static async Task<Authentication> StartAsync(this ICremaHost cremaHost, string userID)
        {
            var token = await cremaHost.OpenAsync();
            var userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var user = await userCollection.GetUserAsync(userID);
            var password = user.GetPassword();
            var authenticationToken = await cremaHost.LoginAsync(userID, password);
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
    }
}

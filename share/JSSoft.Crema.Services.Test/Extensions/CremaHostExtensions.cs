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
        private static readonly Dictionary<Authentication, Guid> authenticationToToken = new();

        public static async Task<Authentication> StartAsync(this ICremaHost cremaHost)
        {
            var token = await cremaHost.OpenAsync();
            var authenticationToken = await cremaHost.LoginAsync("admin", StringUtility.ToSecureString("admin"));
            var authentication = await cremaHost.AuthenticateAsync(authenticationToken);
            authenticationToToken.Add(authentication, token);
            return authentication;
        }

        public static async Task StopAsync(this ICremaHost cremaHost, Authentication authentication)
        {
            var token = authenticationToToken[authentication];
            await cremaHost.LogoutAsync(authentication);
            await cremaHost.CloseAsync(token);
            authenticationToToken.Remove(authentication);
        }
    }
}

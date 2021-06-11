using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class TestContextExtensions
    {
        private const string authenticationKey = "authentications";
        private const string cremaHostKey = "cremaHost";
        private const string userInfosKey = "userInfos";

        public static async Task InitializeAsync(this TestContext context, ICremaHost cremaHost)
        {
            context.Properties.Add(cremaHostKey, cremaHost);
            context.Properties.Add(authenticationKey, new HashSet<Authentication>());
            await Task.Delay(1);
        }

        public static Task<Authentication> LoginRandomAsync(this TestContext context)
        {
            var items = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            return LoginRandomAsync(context, items.Random());
        }

        public static async Task<Authentication> LoginRandomAsync(this TestContext context, Authority authority)
        {
            var authentications = context.Properties[authenticationKey] as HashSet<Authentication>;
            var cremaHost = context.Properties[cremaHostKey] as ICremaHost;
            var authentication = await cremaHost.LoginRandomAsync(authority);
            authentications.Add(authentication);
            return authentication;
        }

        public static async Task<Authentication> LoginAsync(this TestContext context, string userID)
        {
            var authentications = context.Properties[authenticationKey] as HashSet<Authentication>;
            var cremaHost = context.Properties[cremaHostKey] as ICremaHost;
            var userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var user = await userCollection.GetUserAsync(userID);
            var password = user.GetPassword();
            var authenticationToken = await cremaHost.LoginAsync(userID, password);
            var authentication = await cremaHost.AuthenticateAsync(authenticationToken);
            authentications.Add(authentication);
            return authentication;
        }

        public static async Task LogoutAsync(this TestContext context, Authentication authentication)
        {
            var authentications = context.Properties[authenticationKey] as HashSet<Authentication>;
            var cremaHost = context.Properties[cremaHostKey] as ICremaHost;
            await cremaHost.LogoutAsync(authentication);
            authentications.Remove(authentication);
        }

        public static async Task ReleaseAsync(this TestContext context)
        {
            var authentications = context.Properties[authenticationKey] as HashSet<Authentication>;
            var cremaHost = context.Properties[cremaHostKey] as ICremaHost;
            var query = from item in authentications
                        where item.IsExpired
                        select item;
            foreach (var item in query)
            {
                await cremaHost.LogoutAsync(item);
            }
            authentications.Clear();
        }

        public static void SetUserInfos(this TestContext context, UserContextSerializationInfo userInfos)
        {
            context.Properties.Add(userInfosKey, userInfos);
        }

        public static async Task LoginRandomManyAsync(this TestContext context, ICremaHost cremaHost)
        {
            if (context.Properties.Contains(userInfosKey) == true)
            {
                var userInfos = (UserContextSerializationInfo)context.Properties[userInfosKey];
                var count = (int)(userInfos.Users.Length * 0.1);
                await cremaHost.LoginRandomManyAsync(count);
            }
        }

        public static async Task LoadRandomDataBaseManyAsync(this TestContext context, IDataBaseContext dataBaseContext, Authentication authentication)
        {
            var count = await dataBaseContext.GetCountAsync();
            var total = count / 2;

            for (var i = 0; i < total; i++)
            {
                var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None);
                await dataBase.LoadAsync(authentication);
            }
        }
    }
}

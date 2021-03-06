﻿using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.Services.Test.Common.Extensions;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Common
{
    static class TestContextExtensions
    {
        private const string authenticationKey = "authentications";
        private const string appKey = "app";
        private const string cremaHostKey = "cremaHost";

        public static async Task InitializeAsync(this TestContext context, TestApplication app)
        {
            context.Properties.Add(appKey, app);
            context.Properties.Add(cremaHostKey, app.GetService(typeof(ICremaHost)));
            context.Properties.Add(authenticationKey, new HashSet<Authentication>());
            await Task.Delay(1);
        }

        public static Task<Authentication> LoginRandomAsync(this TestContext context)
        {
            return LoginRandomAsync(context, DefaultPredicate);
        }

        public static Task<Authentication> LoginRandomAsync(this TestContext context, Func<IUser, bool> predicate)
        {
            var items = new Authority[] { Authority.Admin, Authority.Member, Authority.Guest };
            return LoginRandomAsync(context, items.Random(), predicate);
        }

        public static Task<Authentication> LoginRandomAsync(this TestContext context, Authority authority)
        {
            return LoginRandomAsync(context, authority, DefaultPredicate);
        }

        public static async Task<Authentication> LoginRandomAsync(this TestContext context, Authority authority, Func<IUser, bool> predicate)
        {
            var authentications = context.Properties[authenticationKey] as HashSet<Authentication>;
            var app = context.Properties[appKey] as TestApplication;
            var cremaHost = context.Properties[cremaHostKey] as ICremaHost;
            var userFlags = AuthorityUtility.ToUserFlags(authority) | UserFlags.NotBanned | UserFlags.Offline;
            var userFilter = new UserFilter(userFlags, predicate);
            var user = await userFilter.GetUserAsync(app);
            var password = user.GetPassword();
            var authenticationToken = await cremaHost.LoginAsync(user.ID, password);
            var authentication = await cremaHost.AuthenticateAsync(authenticationToken);
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
            var authenticationToken = Guid.Empty;
            try
            {
                authenticationToken = await cremaHost.LoginAsync(userID, password);
            }
            catch (CremaException e)
            {
                if (e.Message == "b722d687-0a8d-4999-ad54-cf38c0c25d6f")
                {
                    Console.WriteLine("wqerwqerwwqrwer");
                    await cremaHost.LogoutAsync(userID, password);
                    authenticationToken = await cremaHost.LoginAsync(userID, password);
                }
                else
                {
                    throw e;
                }
            }
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
                        where item.IsExpired == false
                        select item;
            foreach (var item in query)
            {
                await cremaHost.LogoutAsync(item);
            }
            authentications.Clear();
        }

        private static bool DefaultPredicate(IUser _) => true;
    }
}

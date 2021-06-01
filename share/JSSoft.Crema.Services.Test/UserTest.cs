// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Library.Random;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;
using JSSoft.Library.ObjectModel;
using JSSoft.Crema.Services.Test.Extensions;
using JSSoft.Crema.ServiceModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JSSoft.Library;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class UserTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static Authentication expiredAuthentication;
        private static IUserCollection userCollection;
        private static IUser user;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(UserTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            user = await userCollection.GetRandomUserAsync();
            expiredAuthentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            await cremaHost.LogoutAsync(expiredAuthentication);
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.StopAsync(authentication);
            app.Release();
        }

        [TestMethod]
        public async Task MoveAsyncTestAsync()
        {
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            await user.MoveAsync(authentication, category.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsyncTestAsync_Null_Arg0_Fail()
        {
            await user.MoveAsync(null, "/");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsyncTestAsync_Null_Arg1_Fail()
        {
            await user.MoveAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task MoveAsyncTestAsync_Expired_Fail()
        {
            await user.MoveAsync(expiredAuthentication, null);
        }

        [TestMethod]
        public async Task DeleteAsyncTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync(item => Predicate(item, authentication));
            await user.DeleteAsync(authentication);

            static bool Predicate(IUser user, Authentication authentication)
            {
                if (user.ID == Authentication.AdminID)
                    return false;
                if (user.ID == authentication.ID)
                    return false;
                if (user.UserState == UserState.Online)
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsyncTestAsync_Null_Fail()
        {
            await user.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task DeleteAsyncTestAsync_Expired_Fail()
        {
            await user.DeleteAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_PermissionDenied_AdminID_Fail()
        {
            var admin = await userCollection.Dispatcher.InvokeAsync(() => userCollection[Authentication.AdminID]);
            await admin.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsyncTestAsync_PermissionDenied_Member_Fail()
        {
            var member = await cremaHost.LoginRandomAsync(Authority.Member);
            try
            {
                await user.DeleteAsync(member);
            }
            finally
            {
                await cremaHost.LogoutAsync(member);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsyncTestAsync_PermissionDenied_Guest_Fail()
        {
            var guest = await cremaHost.LoginRandomAsync(Authority.Guest);
            try
            {
                await user.DeleteAsync(guest);
            }
            finally
            {
                await cremaHost.LogoutAsync(guest);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_Online_Fail()
        {
            var user = await userCollection.GetRandomUserAsync(item => Predicate(item, authentication));
            await user.DeleteAsync(authentication);

            static bool Predicate(IUser user, Authentication authentication)
            {
                if (user.ID == Authentication.AdminID)
                    return false;
                if (user.ID == authentication.ID)
                    return false;
                if (user.UserState == UserState.None)
                    return false;
                return true;
            }
        }
    }
}

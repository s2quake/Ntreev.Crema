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

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class UserItemTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static Authentication expiredAuthentication;
        private static IUserContext userContext;
        private static IUserItem userItem;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(UserItemTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            userItem = await userContext.GetRandomUserItemAsync((item) => item != userContext.Root && item.Parent != userContext.Root);
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
        public async Task RenameAsyncTestAsync()
        {
            if (userItem.Path.EndsWith("/") == true)
            {
                await userItem.RenameAsync(authentication, RandomUtility.NextName());
            }
            else
            {
                try
                {
                    await userItem.RenameAsync(authentication, RandomUtility.NextName());
                }
                catch (NotImplementedException)
                {

                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsyncTestAsync_Null_Arg0_Fail()
        {
            await userItem.RenameAsync(null, RandomUtility.NextName());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsyncTestAsync_Null_Arg1_Fail()
        {
            await userItem.RenameAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task RenameAsyncTestAsync_Expired_Fail()
        {
            await userItem.RenameAsync(expiredAuthentication, RandomUtility.NextName());
        }

        [TestMethod]
        public async Task MoveAsyncTestAsync()
        {
            var rootItem = userContext.Root;
            await userItem.MoveAsync(authentication, rootItem.Path);
            Assert.AreEqual(rootItem, userItem.Parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsyncTestAsync_Null_Arg0_Fail()
        {
            var rootItem = userContext.Root;
            await userItem.MoveAsync(null, rootItem.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsyncTestAsync_Null_Arg1_Fail()
        {
            var rootItem = userContext.Root;
            await userItem.MoveAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsyncTestAsync_Empty_Arg1_Fail()
        {
            var rootItem = userContext.Root;
            await userItem.MoveAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CategoryNotFoundException))]
        public async Task MoveAsyncTestAsync_CategoryNotFound_Fail()
        {
            var rootItem = userContext.Root;
            await userItem.MoveAsync(authentication, "/qwerwqerwqerweq/wqerqwerwqerqwe/wqerqwerwqer/qwerqwer/");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsyncTestAsync_SameParent_Fail()
        {
            await userItem.MoveAsync(authentication, userItem.Parent.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task MoveAsyncTestAsync_Expired_Fail()
        {
            var rootItem = userContext.Root;
            await userItem.MoveAsync(expiredAuthentication, rootItem.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsyncTestAsync_PermissionDenied_Fail()
        {
            var member1 = await cremaHost.LoginRandomAsync(Authority.Member);
            var userItem = await userContext.GetRandomUserItemAsync((item) => item != userContext.Root);
            var rootItem = userContext.Root;
            try
            {
                await userItem.MoveAsync(member1, rootItem.Path);
            }
            finally
            {
                await cremaHost.LogoutAsync(member1);
            }
        }

        [TestMethod]
        public async Task DeleteAsyncTestAsync_Item()
        {
            var userItem = await userContext.GetRandomUserItemAsync((item) => item is IUser user && user.ID != authentication.ID);
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        public async Task DeleteAsyncTestAsync_Category()
        {
            var userItem = await userContext.GetRandomUserItemAsync((item) => item is IUserCategory && item.Childs.Any() == false);
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsyncTestAsync_Null_Arg0_Fail()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task DeleteAsyncTestAsync_Expired_Fail()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.DeleteAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsyncTestAsync_PermissionDenied_Fail()
        {
            var member1 = await cremaHost.LoginRandomAsync(Authority.Member);
            var userItem = await userContext.GetRandomUserItemAsync((item) => item is IUser);
            try
            {
                await userItem.DeleteAsync(member1);
            }
            finally
            {
                await cremaHost.LogoutAsync(member1);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_HasChild_Fail()
        {
            var userItem = await userContext.GetRandomUserItemAsync((item) => item is IUserCategory && item.Childs.Any() == true);
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_Self_Fail()
        {
            var member = await cremaHost.LoginRandomAsync(Authority.Admin);
            try
            {
                var userItem = await userContext.GetRandomUserItemAsync((item) => item is IUser user && item.Name == member.ID);
                await userItem.DeleteAsync(authentication);
            }
            finally
            {
                await cremaHost.LogoutAsync(member);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_Admin_Fail()
        {
            var userItem = await userContext.GetRandomUserItemAsync((item) => item is IUser user && user.ID == authentication.ID);
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_Online_Fail()
        {
            var member = await cremaHost.LoginRandomAsync();
            try
            {
                var userItem = await userContext.GetRandomUserItemAsync((item) => item is IUser user && user.ID == member.ID);
                await userItem.DeleteAsync(authentication);
            }
            finally
            {
                await cremaHost.LogoutAsync(member);
            }
        }
    }
}

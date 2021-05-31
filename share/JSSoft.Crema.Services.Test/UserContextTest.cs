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

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class UserContextTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static Authentication expiredAuthentication;
        private static IUserContext userContext;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(UserContextTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
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
        public void GetMetaDataTest()
        {
            userContext.Dispatcher.Invoke(() => userContext.GetMetaData(authentication));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMetaDataTest_Null_Fail()
        {
            userContext.GetMetaData(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public void GetMetaDataTest_Expired_Fail()
        {
            userContext.GetMetaData(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetMetaDataTest_Dispatcher_Fail()
        {
            userContext.GetMetaData(authentication);
        }

        [TestMethod]
        public async Task NotifyMessageAsyncTestAsync()
        {
            await userContext.NotifyMessageAsync(authentication, new string[] { }, RandomUtility.NextString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NotifyMessageAsyncTestAsync_Null_Arg0_Fail()
        {
            await userContext.NotifyMessageAsync(null, new string[] { }, RandomUtility.NextString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NotifyMessageAsyncTestAsync_Null_Arg1_Fail()
        {
            await userContext.NotifyMessageAsync(authentication, null, RandomUtility.NextString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NotifyMessageAsyncTestAsync_Null_Arg2_Fail()
        {
            await userContext.NotifyMessageAsync(authentication, new string[] { }, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task NotifyMessageAsyncTestAsync_Expired_Fail()
        {
            await userContext.NotifyMessageAsync(expiredAuthentication, new string[] { }, RandomUtility.NextString());
        }


        [TestMethod]
        public void ContainsTest()
        {
            var itemPath = userContext.Dispatcher.Invoke(() => userContext.Random().Path);
            var contains = userContext.Dispatcher.Invoke(() => userContext.Contains(itemPath));
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ContainsTest_Dispatcher_Fail()
        {
            var itemPath = userContext.Dispatcher.Invoke(() => userContext.Random().Path);
            userContext.Contains(itemPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsTest_Null_Fail()
        {
            userContext.Contains(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContainsTest_Empty_Arg_Fail()
        {
            try
            {
                userContext.Dispatcher.Invoke(() => userContext.Contains(string.Empty));
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [TestMethod]
        public void UsersTest()
        {
            Assert.IsNotNull(userContext.Users);
        }

        [TestMethod]
        public void CategoriesTest()
        {
            Assert.IsNotNull(userContext.Categories);
        }

        [TestMethod]
        public void RootTest()
        {
            Assert.IsNotNull(userContext.Root);
        }

        [TestMethod]
        public void IndexerTest()
        {
            var itemPath = userContext.Dispatcher.Invoke(() => userContext.Random().Path);
            var item = userContext.Dispatcher.Invoke(() => userContext[itemPath]);
            Assert.AreEqual(itemPath, item.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IndexerTest_Null_Fail()
        {
            var value = userContext[null];
            Assert.Fail($"{value}");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IndexerTest_Dispatcher_Fail()
        {
            var itemPath = userContext.Dispatcher.Invoke(() => userContext.Random().Path);
            var item = userContext[itemPath];
            Assert.Fail();
        }

        [TestMethod]
        public void ItemsCreatedTest()
        {
            userContext.Dispatcher.Invoke(() =>
            {
                userContext.ItemsCreated += UserContext_ItemsCreated;
                userContext.ItemsCreated -= UserContext_ItemsCreated;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsCreatedTest_Dispatcher_Fail()
        {
            userContext.ItemsCreated += UserContext_ItemsCreated;
        }

        [TestMethod]
        public void ItemsRenamedTest()
        {
            userContext.Dispatcher.Invoke(() => userContext.ItemsRenamed += UserContext_ItemsRenamed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsRenamedTest_Dispatcher_Fail()
        {
            userContext.ItemsRenamed += UserContext_ItemsRenamed;
        }

        [TestMethod]
        public void ItemsMovedTest()
        {
            userContext.Dispatcher.Invoke(() =>
            {
                userContext.ItemsMoved += UserContext_ItemsMoved;
                userContext.ItemsMoved -= UserContext_ItemsMoved;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsMovedTest_Dispatcher_Fail()
        {
            userContext.ItemsMoved += UserContext_ItemsMoved;
        }

        [TestMethod]
        public void ItemsDeletedTest()
        {
            userContext.Dispatcher.Invoke(() =>
            {
                userContext.ItemsDeleted += UserContext_ItemsDeleted;
                userContext.ItemsDeleted -= UserContext_ItemsDeleted;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsDeletedTest_Dispatcher_Fail()
        {
            userContext.ItemsDeleted += UserContext_ItemsDeleted;
        }

        [TestMethod]
        public void ItemsChangedTest()
        {
            userContext.Dispatcher.Invoke(() =>
            {
                userContext.ItemsChanged += UserContext_ItemsChanged;
                userContext.ItemsChanged -= UserContext_ItemsChanged;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsChangedTest_Dispatcher_Fail()
        {
            userContext.ItemsChanged += UserContext_ItemsChanged;
        }

        [TestMethod]
        public void TaskCompletedTest()
        {
            userContext.Dispatcher.Invoke(() =>
            {
                userContext.TaskCompleted += UserContext_TaskCompleted;
                userContext.TaskCompleted -= UserContext_TaskCompleted;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TaskCompletedTest_Dispatcher_Fail()
        {
            userContext.TaskCompleted += UserContext_TaskCompleted;
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            userContext.Dispatcher.Invoke(() =>
            {
                var enumerator = (userContext as IEnumerable).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Assert.IsNotNull(enumerator.Current);
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetEnumeratorTest_Dispatcher_Fail()
        {
            var enumerator = (userContext as IEnumerable).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetGenericEnumeratorTest()
        {
            userContext.Dispatcher.Invoke(() =>
            {
                var enumerator = (userContext as IEnumerable<IUserItem>).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Assert.IsNotNull(enumerator.Current);
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetGenericEnumeratorTest_Dispatcher_Fail()
        {
            var enumerator = (userContext as IEnumerable<IUserItem>).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        private void UserContext_ItemsCreated(object sender, ItemsCreatedEventArgs<IUserItem> e)
        {
            throw new NotImplementedException();
        }

        private void UserContext_ItemsRenamed(object sender, ItemsRenamedEventArgs<IUserItem> e)
        {
            throw new NotImplementedException();
        }

        private void UserContext_ItemsMoved(object sender, ItemsMovedEventArgs<IUserItem> e)
        {
            throw new NotImplementedException();
        }

        private void UserContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IUserItem> e)
        {
            throw new NotImplementedException();
        }

        private void UserContext_ItemsChanged(object sender, ItemsEventArgs<IUserItem> e)
        {
            throw new NotImplementedException();
        }

        private void UserContext_TaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

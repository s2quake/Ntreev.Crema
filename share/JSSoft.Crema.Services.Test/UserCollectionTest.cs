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
    public class UserCollectionTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IUserCollection userCollection;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(UserCollectionTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.StopAsync(authentication);
            app.Release();
        }

        [TestMethod]
        public void CountTest()
        {
            userCollection.Dispatcher.Invoke(() => userCollection.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CountTest_Dispatcher_Fail()
        {
            var count = userCollection.Count;
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsTest()
        {
            var userID = userCollection.Dispatcher.Invoke(() => userCollection.Random().ID);
            var contains = userCollection.Dispatcher.Invoke(() => userCollection.Contains(userID));
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ContainsTest_Dispatcher_Fail()
        {
            var userID = userCollection.Dispatcher.Invoke(() => userCollection.Random().ID);
            userCollection.Contains(userID);
            Assert.Fail();
        }

        [TestMethod]
        public void IndexerTest()
        {
            var userID = userCollection.Dispatcher.Invoke(() => userCollection.Random().ID);
            var user = userCollection.Dispatcher.Invoke(() => userCollection[userID]);
            Assert.AreEqual(userID, user.ID);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IndexerTest_Dispatcher_Fail()
        {
            var userID = userCollection.Dispatcher.Invoke(() => userCollection.Random().ID);
            var user = userCollection[userID];
            Assert.Fail();
        }

        [TestMethod]
        public void UsersCreatedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersCreated += UserCollection_UsersCreated;
                userCollection.UsersCreated -= UserCollection_UsersCreated;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersCreated_Dispatcher_Faile()
        {
            userCollection.UsersCreated += UserCollection_UsersCreated;
        }

        [TestMethod]
        public void UsersMovedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersMoved += UserCollection_UsersMoved;
                userCollection.UsersMoved -= UserCollection_UsersMoved;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersMoved_Dispatcher_Faile()
        {
            userCollection.UsersMoved += UserCollection_UsersMoved;
        }

        [TestMethod]
        public void UsersRenamedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersRenamed += UserCollection_UsersRenamed;
                userCollection.UsersRenamed -= UserCollection_UsersRenamed;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersRenamed_Dispatcher_Faile()
        {
            userCollection.UsersRenamed += UserCollection_UsersRenamed;
        }

        [TestMethod]
        public void UsersDeletedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersDeleted += UserCollection_UsersDeleted;
                userCollection.UsersDeleted -= UserCollection_UsersDeleted;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersDeleted_Dispatcher_Faile()
        {
            userCollection.UsersDeleted += UserCollection_UsersDeleted;
        }

        [TestMethod]
        public void UsersStateChangedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersStateChanged += UserCollection_UsersStateChanged;
                userCollection.UsersStateChanged -= UserCollection_UsersStateChanged;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersStateChanged_Dispatcher_Faile()
        {
            userCollection.UsersStateChanged += UserCollection_UsersStateChanged;
        }

        [TestMethod]
        public void UsersChangedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersChanged += UserCollection_UsersChanged;
                userCollection.UsersChanged -= UserCollection_UsersChanged;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersChanged_Dispatcher_Faile()
        {
            userCollection.UsersChanged += UserCollection_UsersChanged;
        }

        [TestMethod]
        public void UsersLoggedInTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersLoggedIn += UserCollection_UsersLoggedIn;
                userCollection.UsersLoggedIn -= UserCollection_UsersLoggedIn;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersLoggedIn_Dispatcher_Faile()
        {
            userCollection.UsersLoggedIn += UserCollection_UsersLoggedIn;
        }

        [TestMethod]
        public void UsersLoggedOutTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersLoggedOut += UserCollection_UsersLoggedOut;
                userCollection.UsersLoggedOut -= UserCollection_UsersLoggedOut;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersLoggedOut_Dispatcher_Faile()
        {
            userCollection.UsersLoggedOut += UserCollection_UsersLoggedOut;
        }

        [TestMethod]
        public void UsersKickedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersKicked += UserCollection_UsersKicked;
                userCollection.UsersKicked -= UserCollection_UsersKicked;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersKicked_Dispatcher_Faile()
        {
            userCollection.UsersKicked += UserCollection_UsersKicked;
        }

        [TestMethod]
        public void UsersBanChangedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.UsersBanChanged += UserCollection_UsersBanChanged;
                userCollection.UsersBanChanged -= UserCollection_UsersBanChanged;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersBanChanged_Dispatcher_Faile()
        {
            userCollection.UsersBanChanged += UserCollection_UsersBanChanged;
        }

        [TestMethod]
        public void MessageReceivedTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                userCollection.MessageReceived += UserCollection_MessageReceived;
                userCollection.MessageReceived -= UserCollection_MessageReceived;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MessageReceived_Dispatcher_Faile()
        {
            userCollection.MessageReceived += UserCollection_MessageReceived;
        }

        [TestMethod]
        public void GetEnumeratorTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                var enumerator = (userCollection as IEnumerable).GetEnumerator();
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
            var enumerator = (userCollection as IEnumerable).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetGenericEnumeratorTest()
        {
            userCollection.Dispatcher.Invoke(() =>
            {
                var enumerator = (userCollection as IEnumerable<IUser>).GetEnumerator();
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
            var enumerator = (userCollection as IEnumerable<IUser>).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        private void UserCollection_UsersCreated(object sender, ItemsCreatedEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersMoved(object sender, ItemsMovedEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersRenamed(object sender, ItemsRenamedEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersDeleted(object sender, ItemsDeletedEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersStateChanged(object sender, ItemsEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersChanged(object sender, ItemsEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersLoggedIn(object sender, ItemsEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersKicked(object sender, ItemsEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_UsersBanChanged(object sender, ItemsEventArgs<IUser> e)
        {
            throw new NotImplementedException();
        }

        private void UserCollection_MessageReceived(object sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

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
using JSSoft.Crema.Services.Extensions;

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsTest_Null_Test()
        {
            userCollection.Contains(null);
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void IndexerTest_Null_Fail()
        {
            var value = userCollection[null];
            Assert.Fail($"{value}");
        }

        [TestMethod]
        public async Task UsersCreatedTestAsync()
        {
            var userID = string.Empty;
            var userName = string.Empty;
            var userContext = userCollection.GetService(typeof(IUserContext)) as IUserContext;
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersCreated += UserCollection_UsersCreated;
            });
            var user1 = await userContext.GenerateUserAsync(authentication);
            Assert.AreEqual(userID, user1.ID);
            Assert.AreEqual(userName, user1.UserName);

            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersCreated -= UserCollection_UsersCreated;
            });
            var user2 = await userContext.GenerateUserAsync(authentication);
            Assert.AreNotEqual(userID, user2.ID);
            Assert.AreNotEqual(userName, user2.UserName);

            void UserCollection_UsersCreated(object sender, ItemsCreatedEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                userID = user.ID;
                userName = user.UserName;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersCreated_Dispatcher_Fail()
        {
            userCollection.UsersCreated += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersMovedTestAsync()
        {
            var userCategoryCollection = userCollection.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var user = await userCollection.GetRandomUserAsync();
            var category = await userCategoryCollection.GetRandomUserCategoryAsync(item => item != user.Category);
            var oldCategory = user.Category;
            var path = string.Empty;
            var categoryPath = string.Empty;
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersMoved += UserCollection_UsersMoved;
            });
            await user.MoveAsync(authentication, category.Path);
            Assert.AreEqual(path, user.Path);
            Assert.AreEqual(categoryPath, user.Category.Path);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersMoved -= UserCollection_UsersMoved;
            });
            await user.MoveAsync(authentication, oldCategory.Path);
            Assert.AreNotEqual(path, user.Path);
            Assert.AreNotEqual(categoryPath, user.Category.Path);

            void UserCollection_UsersMoved(object sender, ItemsMovedEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                path = user.Path;
                categoryPath = user.Category.Path;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersMoved_Dispatcher_Fail()
        {
            userCollection.UsersMoved += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersDeletedTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            var userPath = user.Path;
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersDeleted += UserCollection_UsersDeleted;
            });
            await user.DeleteAsync(authentication);
            Assert.AreEqual(string.Empty, userPath);
            Assert.IsNull(user.Category);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersDeleted -= UserCollection_UsersDeleted;
            });
            var user1 = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            var userPath1 = user1.Path;
            await user1.DeleteAsync(authentication);
            Assert.AreNotEqual(string.Empty, userPath1);

            void UserCollection_UsersDeleted(object sender, ItemsDeletedEventArgs<IUser> e)
            {
                userPath = string.Empty;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersDeleted_Dispatcher_Fail()
        {
            userCollection.UsersDeleted += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersStateChangedTestAsync()
        {
            var userState = UserState.None;
            var user = await userCollection.GetRandomUserAsync(item => item.UserState == UserState.None);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersStateChanged += UserCollection_UsersStateChanged;
            });
            var password = user.GetPassword();
            var token = await cremaHost.LoginAsync(user.ID, password);
            var authentication = await cremaHost.AuthenticateAsync(token);
            Assert.AreEqual(userState, user.UserState);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersStateChanged -= UserCollection_UsersStateChanged;
            });
            await cremaHost.LogoutAsync(authentication);
            Assert.AreNotEqual(userState, user.UserState);

            void UserCollection_UsersStateChanged(object sender, ItemsEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                userState = user.UserState;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersStateChanged_Dispatcher_Fail()
        {
            userCollection.UsersStateChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersChangedTestAsync()
        {
            var authentication = await cremaHost.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var userName = user.UserName;
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersChanged += UserCollection_UsersChanged;
            });
            var password = user.GetPassword();
            await user.SetUserNameAsync(authentication, password, RandomUtility.NextName());
            Assert.AreEqual(userName, user.UserName);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersChanged -= UserCollection_UsersChanged;
            });
            await user.SetUserNameAsync(authentication, password, $"{RandomUtility.NextName()}{RandomUtility.Next(100)}");
            Assert.AreNotEqual(userName, user.UserName);

            void UserCollection_UsersChanged(object sender, ItemsEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                userName = user.UserName;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersChanged_Dispatcher_Fail()
        {
            userCollection.UsersChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersLoggedInTestAsync()
        {
            var userID = string.Empty;
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersLoggedIn += UserCollection_UsersLoggedIn;
            });
            var authentication1 = await cremaHost.LoginRandomAsync();
            Assert.AreEqual(userID, authentication1.ID);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersLoggedIn -= UserCollection_UsersLoggedIn;
            });
            var authentication2 = await cremaHost.LoginRandomAsync();
            Assert.AreEqual(userID, authentication1.ID);
            Assert.AreNotEqual(userID, authentication2.ID);

            void UserCollection_UsersLoggedIn(object sender, ItemsEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                userID = user.ID;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersLoggedIn_Dispatcher_Fail()
        {
            userCollection.UsersLoggedIn += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersLoggedOutTestAsync()
        {
            var authentication1 = await cremaHost.LoginRandomAsync();
            var authentication2 = await cremaHost.LoginRandomAsync();
            var user1 = await userCollection.GetUserAsync(authentication1.ID);
            var user2 = await userCollection.GetUserAsync(authentication2.ID);
            var userID = string.Empty;
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersLoggedOut += UserCollection_UsersLoggedOut;
            });
            await cremaHost.LogoutAsync(authentication1);
            Assert.AreEqual(userID, user1.ID);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersLoggedOut -= UserCollection_UsersLoggedOut;
            });
            await cremaHost.LogoutAsync(authentication2);
            Assert.AreEqual(userID, user1.ID);
            Assert.AreNotEqual(userID, user2.ID);

            void UserCollection_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                userID = user.ID;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersLoggedOut_Dispatcher_Fail()
        {
            userCollection.UsersLoggedOut += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersKickedTestAsync()
        {
            var authentication1 = await cremaHost.LoginRandomAsync();
            var user1 = await userCollection.GetUserAsync(authentication1.ID);
            var authentication2 = await cremaHost.LoginRandomAsync();
            var user2 = await userCollection.GetUserAsync(authentication2.ID);
            var userID = string.Empty;
            var message = RandomUtility.NextString();
            var comment = string.Empty;
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersKicked += UserCollection_UsersKicked;
            });
            await user1.KickAsync(authentication, message);
            Assert.AreEqual(userID, user1.ID);
            Assert.AreEqual(message, comment);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersKicked -= UserCollection_UsersKicked;
            });
            await user2.KickAsync(authentication, RandomUtility.NextString());
            Assert.AreEqual(userID, user1.ID);
            Assert.AreEqual(message, comment);
            Assert.AreNotEqual(userID, user2.ID);

            void UserCollection_UsersKicked(object sender, ItemsEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                userID = user.ID;
                comment = (e.MetaData as string[]).Single();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersKicked_Dispatcher_Fail()
        {
            userCollection.UsersKicked += (s, e) => { };
        }

        [TestMethod]
        public async Task UsersBanChangedTestAsync()
        {
            var user1 = await userCollection.GetRandomUserAsync(item => item.Authority != Authority.Admin && item.BanInfo.IsBanned == false);
            var actualUserID = string.Empty;
            var actualMessage = string.Empty;
            var actualBanType = BanChangeType.Unban;
            var expectedMessage = RandomUtility.NextString();
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersBanChanged += UserCollection_UsersBanChanged;
            });
            await user1.BanAsync(authentication, expectedMessage);
            Assert.AreEqual(user1.ID, actualUserID);
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(BanChangeType.Ban, actualBanType);
            var user2 = await userCollection.GetRandomUserAsync(item => item.Authority != Authority.Admin && item.BanInfo.IsBanned == false);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.UsersBanChanged -= UserCollection_UsersBanChanged;
            });
            await user2.BanAsync(authentication, RandomUtility.NextString());
            Assert.AreEqual(user1.ID, actualUserID);
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(BanChangeType.Ban, actualBanType);
            Assert.AreNotEqual(actualUserID, user2.ID);

            void UserCollection_UsersBanChanged(object sender, ItemsEventArgs<IUser> e)
            {
                var user = e.Items.Single();
                var metaData = e.MetaData as object[];
                actualUserID = user.ID;
                actualBanType = (BanChangeType)metaData[0];
                actualMessage = (metaData[1] as string[]).Single();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UsersBanChanged_Dispatcher_Fail()
        {
            userCollection.UsersBanChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task MessageReceivedTest()
        {
            var authentication1 = await cremaHost.LoginRandomAsync();
            var authentication2 = await cremaHost.LoginRandomAsync();
            var user1 = await userCollection.GetUserAsync(authentication1.ID);
            var user2 = await userCollection.GetUserAsync(authentication2.ID);
            var actualMessage = string.Empty;
            var actualMessageType = MessageType.Notification;
            var actualUserID = string.Empty;
            var actualSenderID = string.Empty;
            var expectedMessage = RandomUtility.NextString();
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.MessageReceived += UserCollection_MessageReceived;
            });
            await user2.SendMessageAsync(authentication1, expectedMessage);
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(MessageType.None, actualMessageType);
            Assert.AreEqual(user2.ID, actualUserID);
            Assert.AreEqual(user1.ID, actualSenderID);
            await userCollection.Dispatcher.InvokeAsync(() =>
            {
                userCollection.MessageReceived -= UserCollection_MessageReceived;
            });
            await user1.SendMessageAsync(authentication2, RandomUtility.NextString());
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(MessageType.None, actualMessageType);
            Assert.AreEqual(user2.ID, actualUserID);
            Assert.AreEqual(user1.ID, actualSenderID);

            void UserCollection_MessageReceived(object sender, MessageEventArgs e)
            {
                var user = e.Items.Single();
                actualUserID = user.ID;
                actualSenderID = e.UserID;
                actualMessage = e.Message;
                actualMessageType = e.MessageType;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MessageReceived_Dispatcher_Fail()
        {
            userCollection.MessageReceived += (s, e) => { };
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
    }
}

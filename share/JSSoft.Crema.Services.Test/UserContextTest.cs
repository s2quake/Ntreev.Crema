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
            app.Initialize(context);
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
        public void RootTest()
        {
            Assert.IsNotNull(userContext.Root);
        }

        [TestMethod]
        public async Task IndexerTestAsync()
        {
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                var itemPath = userContext.Random().Path;
                var item = userContext[itemPath];
                Assert.AreEqual(itemPath, item.Path);
            });
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
        public async Task ItemsCreatedTestAsync()
        {
            var actualPath = string.Empty;
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsCreated += UserContext_ItemsCreated;
            });
            var userItem1 = await userContext.GenerateAsync(authentication);
            Assert.AreEqual(userItem1.Path, actualPath);
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsCreated -= UserContext_ItemsCreated;
            });
            var userItem2 = await userContext.GenerateAsync(authentication);
            Assert.AreEqual(userItem1.Path, actualPath);
            Assert.AreNotEqual(userItem2.Path, actualPath);

            void UserContext_ItemsCreated(object sender, ItemsCreatedEventArgs<IUserItem> e)
            {
                var userItem = e.Items.Single();
                actualPath = userItem.Path;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsCreatedTest_Dispatcher_Fail()
        {
            userContext.ItemsCreated += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsRenamedTestAsync()
        {
            var category = await userContext.GetRandomUserCategoryAsync(item => item.Parent != null);
            var actualName = string.Empty;
            var actualPath = string.Empty;
            var actualOldName = string.Empty;
            var actualOldPath = string.Empty;
            var expectedName = RandomUtility.NextName();
            var expectedOldName = category.Name;
            var expectedOldPath = category.Path;
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsRenamed += UserContext_ItemsRenamed;
            });
            await category.RenameAsync(authentication, expectedName);
            Assert.AreEqual(expectedName, actualName);
            Assert.AreEqual(category.Path, actualPath);
            Assert.AreEqual(expectedOldName, actualOldName);
            Assert.AreEqual(expectedOldPath, actualOldPath);
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsRenamed -= UserContext_ItemsRenamed;
            });
            await category.RenameAsync(authentication, RandomUtility.NextName());
            Assert.AreEqual(expectedName, actualName);
            Assert.AreNotEqual(category.Path, actualPath);
            Assert.AreEqual(expectedOldName, actualOldName);
            Assert.AreEqual(expectedOldPath, actualOldPath);

            void UserContext_ItemsRenamed(object sender, ItemsRenamedEventArgs<IUserItem> e)
            {
                var userItem = e.Items.Single();
                actualName = userItem.Name;
                actualPath = userItem.Path;
                actualOldName = e.OldNames.Single();
                actualOldPath = e.OldPaths.Single();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsRenamedTest_Dispatcher_Fail()
        {
            userContext.ItemsRenamed += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsMovedTestAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync(PredicateItem);
            var parentItem1 = await userContext.GetRandomUserItemAsync(item => PredicateParentItem(item, userItem));
            var actualPath = string.Empty;
            var actualOldPath = string.Empty;
            var actualOldParentPath = string.Empty;
            var expectedOldPath = userItem.Path;
            var expectedOldParentPath = userItem.Parent.Path;
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsMoved += UserContext_ItemsMoved;
            });
            await userItem.MoveAsync(authentication, parentItem1.Path);
            Assert.AreEqual(userItem.Path, actualPath);
            Assert.AreEqual(expectedOldPath, actualOldPath);
            Assert.AreEqual(expectedOldParentPath, actualOldParentPath);
            var parentItem2 = await userContext.GetRandomUserItemAsync(item => PredicateParentItem(item, userItem));
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsMoved -= UserContext_ItemsMoved;
            });
            await userItem.MoveAsync(authentication, parentItem2.Path);
            Assert.AreNotEqual(userItem.Path, actualPath);
            Assert.AreEqual(expectedOldPath, actualOldPath);
            Assert.AreEqual(expectedOldParentPath, actualOldParentPath);

            void UserContext_ItemsMoved(object sender, ItemsMovedEventArgs<IUserItem> e)
            {
                var userItem = e.Items.Single();
                actualPath = userItem.Path;
                actualOldPath = e.OldPaths.Single();
                actualOldParentPath = e.OldParentPaths.Single();
            }

            bool PredicateItem(IUserItem userItem)
            {
                if (userItem.Childs.Any() == true)
                    return false;
                if (userItem.Parent == null)
                    return false;
                return true;
            }

            bool PredicateParentItem(IUserItem userItem, IUserItem targetItem)
            {
                if (userItem is not IUserCategory category)
                    return false;
                if (targetItem.Parent == category)
                    return false;
                if (targetItem == userItem)
                    return false;
                if (category.Parent == null)
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsMovedTest_Dispatcher_Fail()
        {
            userContext.ItemsMoved += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsDeletedTestAsync()
        {
            var userItem1 = await userContext.GetRandomUserItemAsync(Predicate);
            var actualPath = string.Empty;
            var expectedPath = userItem1.Path;
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsDeleted += UserContext_ItemsDeleted;
            });
            await userItem1.DeleteAsync(authentication);
            Assert.AreEqual(expectedPath, actualPath);
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsDeleted -= UserContext_ItemsDeleted;
            });
            var userItem2 = await userContext.GetRandomUserItemAsync(Predicate);
            await userItem2.DeleteAsync(authentication);
            Assert.AreEqual(expectedPath, actualPath);

            void UserContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IUserItem> e)
            {
                var userItem = e.Items.Single();
                actualPath = e.ItemPaths.Single();
            }

            static bool Predicate(IUserItem userItem)
            {
                if (userItem is IUser user)
                {
                    if (user.ID == Authentication.AdminID)
                        return false;
                    if (user.UserState == UserState.Online)
                        return false;
                }
                if (userItem is IUserCategory)
                {
                    if (userItem.Parent == null)
                        return false;
                    if (userItem.Childs.Any() == true)
                        return false;
                }
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsDeletedTest_Dispatcher_Fail()
        {
            userContext.ItemsDeleted += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsChangedTestAsync()
        {
            var authentication = await cremaHost.LoginRandomAsync();
            var user = await userContext.GetUserAsync(authentication.ID);
            var password = user.GetPassword();
            var actualPath = string.Empty;
            var expectedName = RandomUtility.NextName();
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsChanged += UserContext_ItemsChanged;
            });
            await user.SetUserNameAsync(authentication, password, expectedName);
            Assert.AreEqual(user.Path, actualPath);
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.ItemsChanged -= UserContext_ItemsChanged;
            });
            await user.SetUserNameAsync(authentication, password, RandomUtility.NextName());
            Assert.AreEqual(user.Path, actualPath);

            void UserContext_ItemsChanged(object sender, ItemsEventArgs<IUserItem> e)
            {
                var userItem = e.Items.Single();
                actualPath = userItem.Path;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsChangedTest_Dispatcher_Fail()
        {
            userContext.ItemsChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task TaskCompletedTestAsync()
        {
            var authentication = await cremaHost.LoginRandomAsync();
            var user = await userContext.GetUserAsync(authentication.ID);
            var password = user.GetPassword();
            var actualTaskID = Guid.Empty;
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.TaskCompleted += UserContext_TaskCompleted;
            });
            var expectedTaskID = await (user.SetUserNameAsync(authentication, password, RandomUtility.NextName()) as Task<Guid>);
            Assert.AreEqual(expectedTaskID, actualTaskID);
            await userContext.Dispatcher.InvokeAsync(() =>
            {
                userContext.TaskCompleted -= UserContext_TaskCompleted;
            });
            expectedTaskID = await (user.SetUserNameAsync(authentication, password, RandomUtility.NextName()) as Task<Guid>);
            Assert.AreNotEqual(expectedTaskID, actualTaskID);

            void UserContext_TaskCompleted(object sender, TaskCompletedEventArgs e)
            {
                actualTaskID = e.TaskIDs.Single();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TaskCompletedTest_Dispatcher_Fail()
        {
            userContext.TaskCompleted += (s, e) => { };
        }

        [TestMethod]
        public async Task GetEnumeratorTestAsync()
        {
            await userContext.Dispatcher.InvokeAsync(() =>
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
        public async Task GetGenericEnumeratorTest()
        {
            await userContext.Dispatcher.InvokeAsync(() =>
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
    }
}

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
using JSSoft.Crema.Services.Test.Common;
using JSSoft.Crema.ServiceModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Test.Common.Extensions;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class UserContextTest
    {
        private static TestApplication app;
        private static Authentication expiredAuthentication;
        private static IUserContext userContext;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new();
            await app.InitializeAsync(context);
            await app.OpenAsync();
            userContext = app.GetService(typeof(IUserContext)) as IUserContext;
            expiredAuthentication = app.ExpiredAuthentication;
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await app.CloseAsync();
            await app.ReleaseAsync();
        }

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            await this.TestContext.InitializeAsync(app);
        }

        [TestCleanup]
        public async Task TestCleanupAsync()
        {
            await this.TestContext.ReleaseAsync();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task GetMetaData_TestAsync()
        {
            await userContext.Dispatcher.InvokeAsync(() => userContext.GetMetaData());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetMetaData_Dispatcher_FailTest()
        {
            userContext.GetMetaData();
        }

        [TestMethod]
        public async Task NotifyMessageAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userIDs = new string[] { };
            var message = RandomUtility.NextString();
            await userContext.NotifyMessageAsync(authentication, userIDs, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NotifyMessageAsync_Arg0_Null_FailTestAsync()
        {
            var userIDs = new string[] { };
            var message = RandomUtility.NextString();
            await userContext.NotifyMessageAsync(null, userIDs, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NotifyMessageAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var message = RandomUtility.NextString();
            await userContext.NotifyMessageAsync(authentication, null, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NotifyMessageAsync_Arg2_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userIDs = new string[] { };
            await userContext.NotifyMessageAsync(authentication, userIDs, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task NotifyMessageAsync_Expired_FailTestAsync()
        {
            var userIDs = new string[] { };
            var message = RandomUtility.NextString();
            await userContext.NotifyMessageAsync(expiredAuthentication, userIDs, message);
        }


        [TestMethod]
        public void Contains_Test()
        {
            var itemPath = userContext.Dispatcher.Invoke(() => userContext.Random().Path);
            var contains = userContext.Dispatcher.Invoke(() => userContext.Contains(itemPath));
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Contains_Dispatcher_FailTest()
        {
            var itemPath = userContext.Dispatcher.Invoke(() => userContext.Random().Path);
            userContext.Contains(itemPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Contains_Arg0_Null_FailTest()
        {
            userContext.Contains(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Contains_Arg0_Empty_FailTest()
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
        public void Root_Test()
        {
            Assert.IsNotNull(userContext.Root);
        }

        [TestMethod]
        public async Task Indexer_TestAsync()
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
        public void Indexer_Arg0_Null_FailTest()
        {
            var value = userContext[null];
            Assert.Fail($"{value}");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Indexer_Arg0_Empty_FailTestAsync()
        {
            var value = await userContext.Dispatcher.InvokeAsync(() => userContext[string.Empty]);
            Assert.Fail($"{value}");
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task Indexer_Arg0_Nonexistent_FailTestAsync()
        {
            var userCollection = userContext.GetService(typeof(IUserCollection)) as IUserCollection;
            var userID = await userCollection.GenerateNewUserIDAsync();
            await userContext.Dispatcher.InvokeAsync(() => userContext[userID]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Indexer_Dispatcher_FailTest()
        {
            var itemPath = userContext.Dispatcher.Invoke(() => userContext.Random().Path);
            var item = userContext[itemPath];
            Assert.Fail();
        }

        [TestMethod]
        public async Task ItemsCreated_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
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
        public async Task ItemsRenamed_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category = await categoryFilter.GetUserCategoryAsync(app);
            var actualName = string.Empty;
            var actualPath = string.Empty;
            var actualOldName = string.Empty;
            var actualOldPath = string.Empty;
            var expectedName = await category.Parent.GenerateNewCategoryNameAsync();
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
        public void ItemsRenamed_Dispatcher_FailTest()
        {
            userContext.ItemsRenamed += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsMoved_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItemFilter = new UserItemFilter() { HasParent = true };
            var userItem = await userItemFilter.GetUserItemAsync(app);
            var parentItemFilter = new UserItemFilter() { TargetToMove = userItem };
            var parentItem1 = await parentItemFilter.GetUserItemAsync(app);
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
            var parentItem2 = await parentItemFilter.GetUserItemAsync(app);
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
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsMoved_Dispatcher_FailTest()
        {
            userContext.ItemsMoved += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsDeleted_User_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userFilter1 = new UserFilter() { ExcludedUserIDs = new[] { Authentication.AdminID, authentication.ID } };
            var userItem1 = (await userFilter1.GetUserAsync(app)) as IUserItem;
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
            var userFilter2 = new UserFilter() { ExcludedUserIDs = new[] { Authentication.AdminID, authentication.ID } };
            var userItem2 = (await userFilter2.GetUserAsync(app)) as IUserItem;
            await userItem2.DeleteAsync(authentication);
            Assert.AreEqual(expectedPath, actualPath);

            void UserContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IUserItem> e)
            {
                var userItem = e.Items.Single();
                actualPath = e.ItemPaths.Single();
            }
        }

        [TestMethod]
        public async Task ItemsDeleted_UserCategory_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter1 = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userItem1 = (await userCategoryFilter1.GetUserCategoryAsync(app)) as IUserItem;
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
            var userCategoryFilter2 = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userItem2 = (await userCategoryFilter2.GetUserCategoryAsync(app)) as IUserItem;
            await userItem2.DeleteAsync(authentication);
            Assert.AreEqual(expectedPath, actualPath);

            void UserContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IUserItem> e)
            {
                var userItem = e.Items.Single();
                actualPath = e.ItemPaths.Single();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsDeleted_Dispatcher_FailTest()
        {
            userContext.ItemsDeleted += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
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
        public void ItemsChanged_Dispatcher_FailTest()
        {
            userContext.ItemsChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task TaskCompleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
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
        public void TaskCompleted_Dispatcher_FailTest()
        {
            userContext.TaskCompleted += (s, e) => { };
        }

        [TestMethod]
        public async Task GetEnumerator_TestAsync()
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
        public void GetEnumerator_Dispatcher_FailTest()
        {
            var enumerator = (userContext as IEnumerable).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task GetGenericEnumerator_Test()
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
        public void GetGenericEnumerator_Dispatcher_FailTest()
        {
            var enumerator = (userContext as IEnumerable<IUserItem>).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }
    }
}

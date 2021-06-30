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
using JSSoft.Crema.Services.Extensions;
using System.Linq;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class UserCategoryCollectionTest
    {
        private static TestApplication app;
        private static IUserCategoryCollection userCategoryCollection;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new TestApplication();
            await app.InitializeAsync(context);
            await app.OpenAsync();
            userCategoryCollection = app.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
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
        public void Count_Test()
        {
            userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Count_Dispatcher_FailTest()
        {
            var count = userCategoryCollection.Count;
            Assert.Fail();
        }

        [TestMethod]
        public async Task Contains_TestAsync()
        {
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category = await categoryFilter.GetUserCategoryAsync(app);
            var categoryPath = category.Path;
            var contains = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Contains(categoryPath));
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Contains_Arg0_Null_Test()
        {
            userCategoryCollection.Contains(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Contains_Dispatcher_FailTestAsync()
        {
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category = await categoryFilter.GetUserCategoryAsync(app);
            var categoryPath = category.Path;
            userCategoryCollection.Contains(categoryPath);
            Assert.Fail();
        }

        [TestMethod]
        public async Task Indexer_TestAsync()
        {
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category1 = await categoryFilter.GetUserCategoryAsync(app);
            var categoryPath = category1.Path;
            var category2 = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection[categoryPath]);
            Assert.AreEqual(category1, category2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Indexer_Arg0_Null_FailTest()
        {
            var value = userCategoryCollection[null];
            Assert.Fail($"{value}");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Indexer_Arg0_Empty_FailTestAsync()
        {
            await userCategoryCollection.Dispatcher.InvokeAsync(() => userCategoryCollection[string.Empty]);
        }

        [TestMethod]
        [ExpectedException(typeof(CategoryNotFoundException))]
        public async Task Indexer_Arg0_Nonexistent_FailTestAsync()
        {
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category = await categoryFilter.GetUserCategoryAsync(app);
            var name = await category.GenerateNewCategoryNameAsync();
            var categoryName = new CategoryName(category.Path, name);
            await userCategoryCollection.Dispatcher.InvokeAsync(() => userCategoryCollection[categoryName]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task IndexerTest_Dispatcher_FailTestAsync()
        {
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category1 = await categoryFilter.GetUserCategoryAsync(app);
            var categoryPath = category1.Path;
            var category2 = userCategoryCollection[categoryPath];
            Assert.Fail();
        }

        [TestMethod]
        public async Task CategoriesCreated_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var parent = await userCategoryCollection.GetRandomUserCategoryAsync();
            var expectedName = await parent.GenerateNewCategoryNameAsync();
            var expectedPath = new CategoryName(parent.Path, expectedName);
            var actualName = string.Empty;
            var actualPath = string.Empty;
            await userCategoryCollection.AddCategoriesCreatedEventHandlerAsync(UserCategoryCollection_CategoriesCreated);
            var category1 = await parent.AddNewCategoryAsync(authentication, expectedName);
            Assert.AreEqual(expectedName, actualName);
            Assert.AreEqual(expectedPath, actualPath);
            await userCategoryCollection.RemoveCategoriesCreatedEventHandlerAsync(UserCategoryCollection_CategoriesCreated);
            var category2 = parent.GenerateUserCategoryAsync(authentication);
            Assert.AreEqual(expectedName, actualName);
            Assert.AreEqual(expectedPath, actualPath);

            void UserCategoryCollection_CategoriesCreated(object sender, ItemsCreatedEventArgs<IUserCategory> e)
            {
                var category = e.Items.Single();
                actualName = category.Name;
                actualPath = category.Path;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesCreated_Dispatcher_FailTest()
        {
            userCategoryCollection.CategoriesCreated += (s, e) => { };
        }

        [TestMethod]
        public async Task CategoriesRenamed_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category = await categoryFilter.GetUserCategoryAsync(app);
            var parent = category.Parent;
            var oldName = category.Name;
            var expectedName = await parent.GenerateNewCategoryNameAsync();
            Console.WriteLine($"expectedName: {expectedName}");
            await parent.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in parent.Categories)
                {
                    Console.WriteLine($"childName: {item.Name}");
                }
            });
            var actualName = string.Empty;
            await userCategoryCollection.AddCategoriesRenamedEventHandlerAsync(UserCategoryCollection_CategoriesRenamed);
            await category.RenameAsync(authentication, expectedName);
            Assert.AreEqual(expectedName, actualName);
            await userCategoryCollection.RemoveCategoriesRenamedEventHandlerAsync(UserCategoryCollection_CategoriesRenamed);
            await category.RenameAsync(authentication, oldName);
            Assert.AreEqual(expectedName, actualName);

            void UserCategoryCollection_CategoriesRenamed(object sender, ItemsRenamedEventArgs<IUserCategory> e)
            {
                var category = e.Items.Single();
                actualName = category.Name;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesRenamed_Dispatcher_Fail()
        {
            userCategoryCollection.CategoriesRenamed += (s, e) => { };
        }

        [TestMethod]
        public async Task CategoriesMoved_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var categoryFilter = new UserCategoryFilter() { HasParent = true };
            var category = await categoryFilter.GetUserCategoryAsync(app);
            var parentFilter = new UserCategoryFilter() { CategoryToMove = category };
            var parent1 = await parentFilter.GetUserCategoryAsync(app);
            var expectedPath = new CategoryName(parent1.Path, category.Name);
            var actualPath = string.Empty;
            await userCategoryCollection.AddCategoriesMovedEventHandlerAsync(UserCategoryCollection_CategoriesMoved);
            await category.MoveAsync(authentication, parent1.Path);
            Assert.AreEqual(expectedPath, actualPath);
            await userCategoryCollection.RemoveCategoriesMovedEventHandlerAsync(UserCategoryCollection_CategoriesMoved);
            var parent2 = await parentFilter.GetUserCategoryAsync(app);
            await category.MoveAsync(authentication, parent2.Path);
            Assert.AreEqual(expectedPath, actualPath);

            void UserCategoryCollection_CategoriesMoved(object sender, ItemsMovedEventArgs<IUserCategory> e)
            {
                var category = e.Items.Single();
                actualPath = category.Path;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesMoved_Dispatcher_FailTest()
        {
            userCategoryCollection.CategoriesMoved += (s, e) => { };
        }

        [TestMethod]
        public async Task CategoriesDeleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var categoryFilter = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var category1 = await categoryFilter.GetUserCategoryAsync(app);
            var expectedPath = category1.Path;
            var actualPath = string.Empty;
            await userCategoryCollection.AddCategoriesDeletedEventHandlerAsync(UserCategoryCollection_CategoriesDeleted);
            await category1.DeleteAsync(authentication);
            Assert.AreEqual(expectedPath, actualPath);
            await userCategoryCollection.RemoveCategoriesDeletedEventHandlerAsync(UserCategoryCollection_CategoriesDeleted);
            var category2 = await categoryFilter.GetUserCategoryAsync(app);
            await category2.DeleteAsync(authentication);
            Assert.AreEqual(expectedPath, actualPath);

            void UserCategoryCollection_CategoriesDeleted(object sender, ItemsDeletedEventArgs<IUserCategory> e)
            {
                actualPath = e.ItemPaths.Single();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesDeleted_Dispatcher_FailTest()
        {
            userCategoryCollection.CategoriesDeleted += (s, e) => { };
        }

        [TestMethod]
        public void GetEnumerator_Test()
        {
            userCategoryCollection.Dispatcher.Invoke(() =>
            {
                var enumerator = (userCategoryCollection as IEnumerable).GetEnumerator();
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
            var enumerator = (userCategoryCollection as IEnumerable).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetGenericEnumerator_Test()
        {
            userCategoryCollection.Dispatcher.Invoke(() =>
            {
                var enumerator = (userCategoryCollection as IEnumerable<IUserCategory>).GetEnumerator();
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
            var enumerator = (userCategoryCollection as IEnumerable<IUserCategory>).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }
    }
}

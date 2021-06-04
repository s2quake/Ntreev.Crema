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
    public class UserCategoryCollectionTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IUserCategoryCollection userCategoryCollection;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(UserCategoryCollectionTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
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
            userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CountTest_Dispatcher_Fail()
        {
            var count = userCategoryCollection.Count;
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsTest()
        {
            var categoryPath = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Random().Path);
            var contains = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Contains(categoryPath));
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsTest_Null_Test()
        {
            userCategoryCollection.Contains(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ContainsTest_Dispatcher_Fail()
        {
            var categoryPath = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Random().Path);
            userCategoryCollection.Contains(categoryPath);
            Assert.Fail();
        }

        [TestMethod]
        public void IndexerTest()
        {
            var categoryPath = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Random().Path);
            var category = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection[categoryPath]);
            Assert.AreEqual(categoryPath, category.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IndexerTest_Null_Fail()
        {
            var value = userCategoryCollection[null];
            Assert.Fail($"{value}");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IndexerTest_Dispatcher_Fail()
        {
            var categoryPath = userCategoryCollection.Dispatcher.Invoke(() => userCategoryCollection.Random().Path);
            var category = userCategoryCollection[categoryPath];
            Assert.Fail();
        }

        [TestMethod]
        public void CategoriesCreatedTest()
        {
            userCategoryCollection.Dispatcher.Invoke(() =>
            {
                userCategoryCollection.CategoriesCreated += UserCategoryCollection_CategoriesCreated;
                userCategoryCollection.CategoriesCreated -= UserCategoryCollection_CategoriesCreated;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesCreated_Dispatcher_Fail()
        {
            userCategoryCollection.CategoriesCreated += UserCategoryCollection_CategoriesCreated;
        }

        [TestMethod]
        public void CategoriesRenamedTest()
        {
            userCategoryCollection.Dispatcher.Invoke(() =>
            {
                userCategoryCollection.CategoriesRenamed += UserCategoryCollection_CategoriesRenamed;
                userCategoryCollection.CategoriesRenamed -= UserCategoryCollection_CategoriesRenamed;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesRenamed_Dispatcher_Fail()
        {
            userCategoryCollection.CategoriesRenamed += UserCategoryCollection_CategoriesRenamed;
        }

        [TestMethod]
        public void CategoriesMovedTest()
        {
            userCategoryCollection.Dispatcher.Invoke(() =>
            {
                userCategoryCollection.CategoriesMoved += UserCategoryCollection_CategoriesMoved;
                userCategoryCollection.CategoriesMoved -= UserCategoryCollection_CategoriesMoved;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesMoved_Dispatcher_Fail()
        {
            userCategoryCollection.CategoriesMoved += UserCategoryCollection_CategoriesMoved;
        }

        [TestMethod]
        public void CategoriesDeletedTest()
        {
            userCategoryCollection.Dispatcher.Invoke(() =>
            {
                userCategoryCollection.CategoriesDeleted += UserCategoryCollection_CategoriesDeleted;
                userCategoryCollection.CategoriesDeleted -= UserCategoryCollection_CategoriesDeleted;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CategoriesDeleted_Dispatcher_Fail()
        {
            userCategoryCollection.CategoriesDeleted += UserCategoryCollection_CategoriesDeleted;
        }

        [TestMethod]
        public void GetEnumeratorTest()
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
        public void GetEnumeratorTest_Dispatcher_Fail()
        {
            var enumerator = (userCategoryCollection as IEnumerable).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GetGenericEnumeratorTest()
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
        public void GetGenericEnumeratorTest_Dispatcher_Fail()
        {
            var enumerator = (userCategoryCollection as IEnumerable<IUserCategory>).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        private void UserCategoryCollection_CategoriesCreated(object sender, ItemsCreatedEventArgs<IUserCategory> e)
        {
            throw new NotImplementedException();
        }

        private void UserCategoryCollection_CategoriesRenamed(object sender, ItemsRenamedEventArgs<IUserCategory> e)
        {
            throw new NotImplementedException();
        }

        private void UserCategoryCollection_CategoriesMoved(object sender, ItemsMovedEventArgs<IUserCategory> e)
        {
            throw new NotImplementedException();
        }

        private void UserCategoryCollection_CategoriesDeleted(object sender, ItemsDeletedEventArgs<IUserCategory> e)
        {
            throw new NotImplementedException();
        }
    }
}

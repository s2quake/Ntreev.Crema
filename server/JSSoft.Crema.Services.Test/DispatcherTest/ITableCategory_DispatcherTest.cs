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
using JSSoft.Library.IO;
using JSSoft.Library.Random;
using System;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.Services.Test.DispatcherTest
{
    [TestClass]
    public class ITableCategory_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITableCategory category;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITableCategory_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeAsync(authentication);
            category = dataBase.TableContext.Categories.Random();
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await dataBase.UnloadAsync(authentication);
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RenameAsync()
        {
            await category.RenameAsync(authentication, RandomUtility.NextIdentifier());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task MoveAsync()
        {
            await category.MoveAsync(authentication, PathUtility.Separator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync()
        {
            await category.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AddNewCategoryAsync()
        {
            await category.AddNewCategoryAsync(authentication, RandomUtility.NextIdentifier());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NewTableAsync()
        {
            await category.NewTableAsync(authentication);
        }

        [TestMethod]
        public async Task GetDataSetAsync()
        {
            await category.GetDataSetAsync(authentication, null);
        }

        [TestMethod]
        public async Task GetLogAsync()
        {
            await category.GetLogAsync(authentication, null);
        }

        [TestMethod]
        public async Task FindAsync()
        {
            await category.FindAsync(authentication, "1", ServiceModel.FindOptions.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Name()
        {
            Console.Write(category.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Path()
        {
            Console.Write(category.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IsLocked()
        {
            Console.Write(category.IsLocked);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IsPrivate()
        {
            Console.Write(category.IsPrivate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LockInfo()
        {
            Console.Write(category.LockInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccessInfo()
        {
            Console.Write(category.AccessInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Parent()
        {
            Console.Write(category.Parent);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Categories()
        {
            Console.Write(category.Categories);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Tables()
        {
            Console.Write(category.Tables);
        }

        [TestMethod]
        public void ExtendedProperties()
        {
            Console.Write(category.ExtendedProperties);
        }

        [TestMethod]
        public void Dispatcher()
        {
            Console.Write(category.Dispatcher);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Renamed()
        {
            category.Renamed += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Moved()
        {
            category.Moved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Deleted()
        {
            category.Deleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LockChanged()
        {
            category.LockChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccessChanged()
        {
            category.AccessChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPublicAsync()
        {
            await category.SetPublicAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPrivateAsync()
        {
            await category.SetPrivateAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AddAccessMemberAsync()
        {
            await category.AddAccessMemberAsync(authentication, "admin", ServiceModel.AccessType.Owner);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RemoveAccessMemberAsync()
        {
            await category.RemoveAccessMemberAsync(authentication, "admin");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LockAsync()
        {
            await category.LockAsync(authentication, RandomUtility.NextString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnlockAsync()
        {
            await category.UnlockAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetAccessType()
        {
            category.GetAccessType(authentication);
        }

        [TestMethod]
        public void GetService()
        {
            Console.Write(category.GetService(typeof(ICremaHost)));
        }
    }
}

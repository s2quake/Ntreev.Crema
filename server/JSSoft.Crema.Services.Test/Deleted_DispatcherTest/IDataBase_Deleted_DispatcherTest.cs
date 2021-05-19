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
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Deleted_DispatcherTest
{
    [TestClass]
    public class IDataBase_Deleted_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(IDataBase_Deleted_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.CreateRandomDataBaseAsync(authentication);
            await dataBase.DeleteAsync(authentication);
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LoadAsync()
        {
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnloadAsync()
        {
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EnterAsync()
        {
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LeaveAsync()
        {
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RenameAsync()
        {
            await dataBase.RenameAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync()
        {
            await dataBase.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CopyAsync()
        {
            await dataBase.CopyAsync(authentication, null, null, false);
        }

        [TestMethod]
        public void TypeContext()
        {
            Assert.IsNull(dataBase.TypeContext);
        }

        [TestMethod]
        public void TableContext()
        {
            Assert.IsNull(dataBase.TableContext);
        }

        [TestMethod]
        public void Name()
        {
            Console.WriteLine(dataBase.Name);
        }

        [TestMethod]
        public void IsLoaded()
        {
            Assert.IsFalse(dataBase.IsLoaded);
        }

        [TestMethod]
        public void ID()
        {
            Console.WriteLine(dataBase.ID);
        }

        [TestMethod]
        public void DataBaseInfo()
        {
            Console.WriteLine(dataBase.DataBaseInfo);
        }

        [TestMethod]
        public void DataBaseState()
        {
            Assert.AreEqual(ServiceModel.DataBaseState.None, dataBase.DataBaseState);
        }

        [TestMethod]
        public void Dispatcher()
        {
            Assert.IsNull(dataBase.Dispatcher);
        }

        [TestMethod]
        public void Renamed()
        {
            dataBase.Renamed += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void Deleted()
        {
            dataBase.Deleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void Loaded()
        {
            dataBase.Loaded += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void Unloaded()
        {
            dataBase.Unloaded += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void Changed()
        {
            dataBase.DataBaseInfoChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void StateChanged()
        {
            dataBase.DataBaseStateChanged += (s, e) => Assert.Inconclusive();
        }
    }
}

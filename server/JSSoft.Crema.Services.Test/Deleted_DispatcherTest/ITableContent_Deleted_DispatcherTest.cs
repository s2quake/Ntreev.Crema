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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.Services.Test.Deleted_DispatcherTest
{
    [TestClass]
    public class ITableContent_Deleted_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITableContent content;
        private static ITableRow row;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITableContent_Deleted_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeAsync(authentication);
            content = dataBase.TableContext.Tables.Random(item => item.Parent == null).Content;
            await content.BeginEditAsync(authentication);
            await content.EnterEditAsync(authentication);
            row = await content.AddNewAsync(authentication, null);
            await dataBase.LeaveAsync(authentication);
            await dataBase.UnloadAsync(authentication);
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EnterAsync()
        {
            await content.EnterEditAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LeaveAsync()
        {
            await content.LeaveEditAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CancelEditAsync()
        {
            await content.CancelEditAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ClearAsync()
        {
            await content.ClearAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AddNewAsync()
        {
            await content.AddNewAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EndNewAsync()
        {
            await content.EndNewAsync(authentication, row);
        }

        [TestMethod]
        public void Domain()
        {
            Console.Write(content.Domain);
        }

        [TestMethod]
        public void Table()
        {
            Console.Write(content.Table);
        }

        [TestMethod]
        public void Count()
        {
            Assert.AreEqual(0, content.Count);
        }

        [TestMethod]
        public void Dispatcher()
        {
            Assert.IsNull(content.Dispatcher);
        }

        [TestMethod]
        public void EditBegun()
        {
            content.EditBegun += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void EditEnded()
        {
            content.EditEnded += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void EditCanceled()
        {
            content.EditCanceled += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void GetEnumerator()
        {
            foreach (var item in content as IEnumerable)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        public void GetEnumeratorGeneric()
        {
            foreach (var item in content as IEnumerable<ITableRow>)
            {
                Console.Write(item);
            }
        }
    }
}

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
    public class ITableCollection_Deleted_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITableCollection tables;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITableCollection_Deleted_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeAsync(authentication);
            tables = dataBase.TableContext.Tables;
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
        public void Contains()
        {
            tables.Contains(string.Empty);
        }

        [TestMethod]
        public void Indexer()
        {
            Console.Write(tables[string.Empty]);
        }

        [TestMethod]
        public void TablesStateChanged()
        {
            tables.TablesStateChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void TablesChanged()
        {
            tables.TablesChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void TablesCreated()
        {
            tables.TablesCreated += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void TablesMoved()
        {
            tables.TablesMoved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void TablesRenamed()
        {
            tables.TablesRenamed += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void TablesDeleted()
        {
            tables.TablesDeleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void Count()
        {
            Console.Write(tables.Count);
        }

        [TestMethod]
        public void GetEnumerator()
        {
            foreach (var item in tables as IEnumerable)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        public void GetEnumeratorGeneric()
        {
            foreach (var item in tables as IEnumerable<ITable>)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetService()
        {
            Console.Write(tables.GetService(typeof(ICremaHost)));
        }
    }
}

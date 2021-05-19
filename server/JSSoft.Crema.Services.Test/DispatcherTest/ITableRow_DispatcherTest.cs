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
using JSSoft.Crema.Services.Random;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.DispatcherTest
{
    [TestClass]
    public class ITableRow_DispatcherTest
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
            app.Initialize(context, nameof(ITableRow_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeAsync(authentication);
            content = dataBase.TableContext.Tables.Random(item => item.Parent == null).Content;
            await content.BeginEditAsync(authentication);
            await content.EnterEditAsync(authentication);
            if (content.Count == 0)
                await content.AddRandomRowsAsync(authentication);
            row = content.Random();
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await content.LeaveEditAsync(authentication);
            await dataBase.UnloadAsync(authentication);
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync()
        {
            await row.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetIsEnabledAsync()
        {
            await row.SetIsEnabledAsync(authentication, false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetTagsAsync()
        {
            await row.SetTagsAsync(authentication, TagInfo.All);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetFieldAsync()
        {
            await row.SetFieldAsync(authentication, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Indexer()
        {
            Console.Write(row[string.Empty]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Tags()
        {
            Console.Write(row.Tags);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IsEnabled()
        {
            Console.Write(row.IsEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Content()
        {
            Console.Write(row.Content);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ParentID()
        {
            Console.Write(row.ParentID);
        }
    }
}

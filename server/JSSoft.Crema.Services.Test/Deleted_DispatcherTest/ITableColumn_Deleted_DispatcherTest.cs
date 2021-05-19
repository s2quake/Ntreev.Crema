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
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.Services.Test.Deleted_DispatcherTest
{
    [TestClass]
    public class ITableColumn_Deleted_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITableTemplate template;
        private static ITableColumn column;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITableColumn_Deleted_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeAsync(authentication);
            template = dataBase.TableContext.Tables.Random(item => item.TemplatedParent == null).Template;
            await template.BeginEditAsync(authentication);
            column = template.Random();
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
        public async Task DeleteAsync()
        {
            await column.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetIndexAsync()
        {
            await column.SetIndexAsync(authentication, RandomUtility.Next(int.MaxValue));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetIsKeyAsync()
        {
            await column.SetIsKeyAsync(authentication, RandomUtility.NextBoolean());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetIsUniqueAsync()
        {
            await column.SetIsUniqueAsync(authentication, RandomUtility.NextBoolean());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetNameAsync()
        {
            await column.SetNameAsync(authentication, RandomUtility.NextIdentifier());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetDataTypeAsync()
        {
            await column.SetDataTypeAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetDefaultValueAsync()
        {
            await column.SetDefaultValueAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetCommentAsync()
        {
            await column.SetCommentAsync(authentication, RandomUtility.NextString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetAutoIncrementAsync()
        {
            await column.SetAutoIncrementAsync(authentication, RandomUtility.NextBoolean());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetTagsAsync()
        {
            await column.SetTagsAsync(authentication, RandomUtility.NextTags());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetIsReadOnlyAsync()
        {
            await column.SetIsReadOnlyAsync(authentication, RandomUtility.NextBoolean());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetAllowNullAsync()
        {
            await column.SetAllowNullAsync(authentication, RandomUtility.NextBoolean());
        }

        [TestMethod]
        public void Index()
        {
            Console.Write(column.Index);
        }

        [TestMethod]
        public void IsKey()
        {
            Console.Write(column.IsKey);
        }

        [TestMethod]
        public void IsUnique()
        {
            Console.Write(column.IsUnique);
        }

        [TestMethod]
        public void Name()
        {
            Console.Write(column.Name);
        }

        [TestMethod]
        public void DataType()
        {
            Console.Write(column.DataType);
        }

        [TestMethod]
        public void DefaultValue()
        {
            Console.Write(column.DefaultValue);
        }

        [TestMethod]
        public void Comment()
        {
            Console.Write(column.Comment);
        }

        [TestMethod]
        public void AutoIncrement()
        {
            Console.Write(column.AutoIncrement);
        }

        [TestMethod]
        public void Tags()
        {
            Console.Write(column.Tags);
        }

        [TestMethod]
        public void IsReadOnly()
        {
            Console.Write(column.IsReadOnly);
        }

        [TestMethod]
        public void AllowNull()
        {
            Console.Write(column.AllowNull);
        }

        [TestMethod]
        public void Template()
        {
            Console.Write(column.Template);
        }
    }
}

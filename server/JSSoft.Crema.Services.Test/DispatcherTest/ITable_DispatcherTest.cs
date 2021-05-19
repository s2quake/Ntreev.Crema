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
    public class ITable_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITable table;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITable_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeAsync(authentication);
            table = dataBase.TableContext.Tables.Random();
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
            await table.RenameAsync(authentication, RandomUtility.NextIdentifier());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task MoveAsync()
        {
            await table.MoveAsync(authentication, PathUtility.Separator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync()
        {
            await table.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CopyAsync()
        {
            await table.CopyAsync(authentication, RandomUtility.NextIdentifier(), PathUtility.Separator, true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task InheritAsync()
        {
            await table.InheritAsync(authentication, RandomUtility.NextIdentifier(), PathUtility.Separator, true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NewTableAsync()
        {
            await table.NewTableAsync(authentication);
        }

        [TestMethod]
        public async Task GetDataSetAsync()
        {
            await table.GetDataSetAsync(authentication, null);
        }

        [TestMethod]
        public async Task GetLogAsync()
        {
            await table.GetLogAsync(authentication, null);
        }

        [TestMethod]
        public async Task FindAsync()
        {
            await table.FindAsync(authentication, "1", ServiceModel.FindOptions.None);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Parent()
        {
            Console.Write(table.Parent);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Name()
        {
            Console.Write(table.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableName()
        {
            Console.Write(table.TableName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Path()
        {
            Console.Write(table.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IsLocked()
        {
            Console.Write(table.IsLocked);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IsPrivate()
        {
            Console.Write(table.IsPrivate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LockInfo()
        {
            Console.Write(table.LockInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccessInfo()
        {
            Console.Write(table.AccessInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableInfo()
        {
            Console.Write(table.TableInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableState()
        {
            Console.Write(table.TableState);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Category()
        {
            Console.Write(table.Category);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Childs()
        {
            Console.Write(table.Childs);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DerivedTables()
        {
            Console.Write(table.DerivedTables);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TemplatedParent()
        {
            Console.Write(table.TemplatedParent);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Template()
        {
            Console.Write(table.Template);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Content()
        {
            Console.Write(table.Content);
        }

        [TestMethod]
        public void ExtendedProperties()
        {
            Console.Write(table.ExtendedProperties);
        }

        [TestMethod]
        public void Dispatcher()
        {
            Console.Write(table.Dispatcher);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Renamed()
        {
            table.Renamed += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Moved()
        {
            table.Moved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Deleted()
        {
            table.Deleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LockChanged()
        {
            table.LockChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccessChanged()
        {
            table.AccessChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableInfoChanged()
        {
            table.TableInfoChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableStateChanged()
        {
            table.TableStateChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPublicAsync()
        {
            await table.SetPublicAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPrivateAsync()
        {
            await table.SetPrivateAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AddAccessMemberAsync()
        {
            await table.AddAccessMemberAsync(authentication, "admin", ServiceModel.AccessType.Owner);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RemoveAccessMemberAsync()
        {
            await table.RemoveAccessMemberAsync(authentication, "admin");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LockAsync()
        {
            await table.LockAsync(authentication, RandomUtility.NextString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnlockAsync()
        {
            await table.UnlockAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetAccessType()
        {
            table.GetAccessType(authentication);
        }

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public void VerifyRead()
        //{
        //    table.VerifyRead(authentication);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public void VerifyOwner()
        //{
        //    table.VerifyOwner(authentication);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public void VerifyMember()
        //{
        //    table.VerifyMember(authentication);
        //}

        [TestMethod]
        public void GetService()
        {
            Console.Write(table.GetService(typeof(ICremaHost)));
        }
    }
}

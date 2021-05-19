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

namespace JSSoft.Crema.Services.Test.Deleted_DispatcherTest
{
    [TestClass]
    public class ITypeItem_Deleted_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITypeItem typeItem;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITypeItem_Deleted_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.TypeContext.AddRandomItemsAsync(authentication);
            typeItem = dataBase.TypeContext.Random();
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
        public async Task RenameAsync()
        {
            await typeItem.RenameAsync(authentication, RandomUtility.NextIdentifier());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task MoveAsync()
        {
            await typeItem.MoveAsync(authentication, PathUtility.Separator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync()
        {
            await typeItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetLogAsync()
        {
            await typeItem.GetLogAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task FindAsync()
        {
            await typeItem.FindAsync(authentication, "1", ServiceModel.FindOptions.None);
        }

        [TestMethod]
        public void Name()
        {
            Console.Write(typeItem.Name);
        }

        [TestMethod]
        public void Path()
        {
            Console.Write(typeItem.Path);
        }

        [TestMethod]
        public void IsLocked()
        {
            Console.Write(typeItem.IsLocked);
        }

        [TestMethod]
        public void IsPrivate()
        {
            Console.Write(typeItem.IsPrivate);
        }

        [TestMethod]
        public void Parent()
        {
            Console.Write(typeItem.Parent);
        }

        [TestMethod]
        public void Childs()
        {
            foreach (var item in typeItem.Childs)
            {
                Console.Write(item);
            }
        }

        [TestMethod]
        public void LockInfo()
        {
            Console.Write(typeItem.LockInfo);
        }

        [TestMethod]
        public void AccessInfo()
        {
            Console.Write(typeItem.AccessInfo);
        }

        [TestMethod]
        public void ExtendedProperties()
        {
            Console.Write(typeItem.ExtendedProperties);
        }

        [TestMethod]
        public void Dispatcher()
        {
            Assert.IsNull(typeItem.Dispatcher);
        }

        [TestMethod]
        public void Renamed()
        {
            typeItem.Renamed += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void Moved()
        {
            typeItem.Moved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void Deleted()
        {
            typeItem.Deleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void LockChanged()
        {
            typeItem.LockChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        public void AccessChanged()
        {
            typeItem.AccessChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPublicAsync()
        {
            await typeItem.SetPublicAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPrivateAsync()
        {
            await typeItem.SetPrivateAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AddAccessMemberAsync()
        {
            await typeItem.AddAccessMemberAsync(authentication, "admin", ServiceModel.AccessType.Owner);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RemoveAccessMemberAsync()
        {
            await typeItem.RemoveAccessMemberAsync(authentication, "admin");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LockAsync()
        {
            await typeItem.LockAsync(authentication, RandomUtility.NextString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnlockAsync()
        {
            await typeItem.UnlockAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetAccessType()
        {
            typeItem.GetAccessType(authentication);
        }
    }
}

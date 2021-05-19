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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Random;
using JSSoft.Library.Random;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.DispatcherTest
{
    [TestClass]
    public class IDomain_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITable table;
        private static IDomain domain;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(IDomain_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.TypeContext.AddRandomItemsAsync(authentication);
            await dataBase.TableContext.AddRandomItemsAsync(authentication);
            table = dataBase.TableContext.Tables.Random(item => item.TemplatedParent == null);
            await table.Template.BeginEditAsync(authentication);
            domain = table.Template.Domain;
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await table.Template.CancelEditAsync(authentication);
            await dataBase.UnloadAsync(authentication);
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync()
        {
            await domain.DeleteAsync(authentication, false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BeginUserEditAsync()
        {
            await domain.BeginUserEditAsync(authentication, DomainLocationInfo.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EndUserEditAsync()
        {
            await domain.EndUserEditAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task NewRowAsync()
        {
            await domain.NewRowAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetRowAsync()
        {
            await domain.SetRowAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RemoveRowAsync()
        {
            await domain.RemoveRowAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPropertyAsync()
        {
            await domain.SetPropertyAsync(authentication, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetUserLocationAsync()
        {
            await domain.SetUserLocationAsync(authentication, DomainLocationInfo.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task KickAsync()
        {
            await domain.KickAsync(authentication, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetOwnerAsync()
        {
            await domain.SetOwnerAsync(authentication, null);
        }

        [TestMethod]
        public void ID()
        {
            Console.Write(domain.ID);
        }

        [TestMethod]
        public void DataBaseID()
        {
            Console.Write(domain.DataBaseID);
        }

        [TestMethod]
        public void Source()
        {
            Console.Write(domain.Source);
        }

        [TestMethod]
        public void Host()
        {
            Console.Write(domain.Host);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DomainInfo()
        {
            Console.Write(domain.DomainInfo);
        }

        [TestMethod]
        public void DomainState()
        {
            Console.Write(domain.DomainState);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Users()
        {
            Console.Write(domain.Users);
        }

        [TestMethod]
        public void Dispatcher()
        {
            Console.Write(domain.Dispatcher);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UserAdded()
        {
            domain.UserAdded += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UserStateChanged()
        {
            domain.UserStateChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UserLocationChanged()
        {
            domain.UserLocationChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UserRemoved()
        {
            domain.UserRemoved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RowAdded()
        {
            domain.RowAdded += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RowRemoved()
        {
            domain.RowRemoved += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RowChanged()
        {
            domain.RowChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PropertyChanged()
        {
            domain.PropertyChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Deleted()
        {
            domain.Deleted += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DomainStateChanged()
        {
            domain.DomainStateChanged += (s, e) => Assert.Inconclusive();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetMetaDataAsync()
        {
            await domain.GetMetaDataAsync(authentication);
        }

        [TestMethod]
        public void GetService()
        {
            Console.Write(domain.GetService(typeof(ICremaHost)));
        }
    }
}

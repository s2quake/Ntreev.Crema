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
using System.Linq;
using JSSoft.Library;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.IO;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class DataBaseTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Guid token;
        private static IDataBaseContext dataBaseContext;
        private static Authentication expiredAuthentication;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context);
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            token = await cremaHost.OpenAsync();
            dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            expiredAuthentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            await dataBaseContext.GenerateDataBasesAsync(expiredAuthentication, 20);
            await context.LoginRandomManyAsync(cremaHost);
            await context.InitializeRandomSettingDataBasesAsync(dataBaseContext);
            await cremaHost.LogoutAsync(expiredAuthentication);
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.CloseAsync(token);
            app.Release();
        }

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            await this.TestContext.InitializeAsync(cremaHost);
        }

        [TestCleanup]
        public async Task TestCleanupAsync()
        {
            await this.TestContext.ReleaseAsync();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task GetMetaData_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            var metaData = await dataBase.Dispatcher.InvokeAsync(() => dataBase.GetMetaData(authentication));
            Assert.AreEqual(dataBase.DataBaseState, metaData.DataBaseState);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetMetaData_Arg0_Null_FailTestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.Dispatcher.InvokeAsync(() => dataBase.GetMetaData(null));
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task GetMetaData_Expired_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.Dispatcher.InvokeAsync(() => dataBase.GetMetaData(expiredAuthentication));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetMetaData_Dispatcher_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            dataBase.GetMetaData(authentication);
        }

        [TestMethod]
        public async Task LoadAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Unloaded);
            await dataBase.LoadAsync(authentication);
            Assert.AreEqual(DataBaseState.Loaded, dataBase.DataBaseState);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LoadAsync_Arg0_Null_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Unloaded);
            await dataBase.LoadAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task LoadAsync_Expired_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Unloaded);
            await dataBase.LoadAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Member_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Unloaded);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Guest_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Unloaded);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LoadAsync_Loaded_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        public async Task UnloadAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.UnloadAsync(authentication);
            Assert.AreEqual(DataBaseState.None, dataBase.DataBaseState);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UnloadAsync_Arg0_Null_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.UnloadAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task UnloadAsync_Expired_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.UnloadAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Member_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Guest_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnloadAsync_Unloaded_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Unloaded);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        public async Task EnterAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EnterAsync_Arg0_Null_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.EnterAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task EnterAsync_Expired_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.EnterAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EnterAsync_Enter_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            await dataBase.EnterAsync(authentication);
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EnterAsync_Unloaded_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Unloaded);
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        public async Task LeaveAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task RenameAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task DeleteAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task RevertAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task ImportAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task Contains_TestAsync()
        {

        }

        [TestMethod]
        public async Task GetLogAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task GetDataSetAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task BeginTransactionAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task CopyAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task Name_TestAsync()
        {

        }

        [TestMethod]
        public async Task IsLoaded_TestAsync()
        {

        }

        [TestMethod]
        public async Task IsLocked_TestAsync()
        {

        }

        [TestMethod]
        public async Task IsPrivate_TestAsync()
        {

        }

        [TestMethod]
        public async Task ID_TestAsync()
        {

        }

        [TestMethod]
        public async Task DataBaseInfo_TestAsync()
        {

        }

        [TestMethod]
        public async Task DataBaseState_TestAsync()
        {

        }

        [TestMethod]
        public async Task AuthenticationInfos_TestAsync()
        {

        }

        [TestMethod]
        public async Task Renamed_TestAsync()
        {

        }

        [TestMethod]
        public async Task Deleted_TestAsync()
        {

        }

        [TestMethod]
        public async Task Loaded_TestAsync()
        {

        }

        [TestMethod]
        public async Task Unloaded_TestAsync()
        {

        }

        [TestMethod]
        public async Task Resetting_TestAsync()
        {

        }

        [TestMethod]
        public async Task Reset_TestAsync()
        {

        }

        [TestMethod]
        public async Task AuthenticationEntered_TestAsync()
        {

        }

        [TestMethod]
        public async Task AuthenticationLeft_TestAsync()
        {

        }

        [TestMethod]
        public async Task DataBaseInfoChanged_TestAsync()
        {

        }

        [TestMethod]
        public async Task DataBaseStateChanged_TestAsync()
        {

        }

        [TestMethod]
        public async Task LockChanged_TestAsync()
        {

        }

        [TestMethod]
        public async Task AccessChanged_TestAsync()
        {

        }

        [TestMethod]
        public async Task TaskCompleted_TestAsync()
        {

        }
    }
}

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
using System.Threading.Tasks;
using JSSoft.Crema.Services.Test.Common;
using JSSoft.Crema.ServiceModel;
using System;
using System.Linq;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class DataBase_LeaveAsyncTest
    {
        private static TestApplication app;
        private static IDataBaseContext dataBaseContext;
        private static Authentication expiredAuthentication;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new();
            await app.InitializeAsync(context);
            await app.OpenAsync();
            dataBaseContext = app.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            expiredAuthentication = app.ExpiredAuthentication;
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await app.CloseAsync();
            await app.ReleaseAsync();
        }

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            await this.TestContext.InitializeAsync(app);
        }

        [TestCleanup]
        public async Task TestCleanupAsync()
        {
            await this.TestContext.ReleaseAsync();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task LeaveAsync_Admin_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        public async Task LeaveAsync_Member_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        public async Task LeaveAsync_Guest_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LeaveAsync_Arg0_Null_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LeaveAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task LeaveAsync_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LeaveAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LeaveAsync_Leved_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LeaveAsync_NotLoaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task LeaveAsync_Private_Admin_AccessTypeNone_FailTestAsync()
        {
            return this.LeaveAsync_Private_AccessTypeNone_FailTestAsync(Authority.Admin);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task LeaveAsync_Private_Member_AccessTypeNone_FailTestAsync()
        {
            return this.LeaveAsync_Private_AccessTypeNone_FailTestAsync(Authority.Member);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task LeaveAsync_Private_Guest_AccessTypeNone_FailTestAsync()
        {
            return this.LeaveAsync_Private_AccessTypeNone_FailTestAsync(Authority.Guest);
        }

        [TestMethod]
        public async Task LeaveAsync_Private_Admin_Owner_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginAsync(accessInfo.UserID);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Admin_Master_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Admin, AccessType.Master);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Admin_Developer_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Admin, AccessType.Developer);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Admin_Editor_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Admin, AccessType.Editor);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Admin_Guest_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Admin, AccessType.Guest);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Member_Developer_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Member, AccessType.Developer);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Member_Editor_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Member, AccessType.Editor);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Member_Guest_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Member, AccessType.Guest);
        }

        [TestMethod]
        public Task LeaveAsync_Private_Guest_Guest_TestAsync()
        {
            return this.LeaveAsync_Private_TestAsync(Authority.Guest, AccessType.Guest);
        }

        public async Task LeaveAsync_Locked_Admin_Locker_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.Locked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var lockInfo = dataBase.LockInfo;
            var authentication = await this.TestContext.LoginAsync(lockInfo.UserID);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task LeaveAsync_Locked_Admin_NotLocker_FailTestAsync()
        {
            return this.LeaveAsync_Locked_NotLocker_FailTestAsync(Authority.Admin);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task LeaveAsync_Locked_Member_NotLocker_FailTestAsync()
        {
            return this.LeaveAsync_Locked_NotLocker_FailTestAsync(Authority.Member);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task LeaveAsync_Locked_Guest_NotLocker_FailTestAsync()
        {
            return this.LeaveAsync_Locked_NotLocker_FailTestAsync(Authority.Guest);
        }

        private async Task LeaveAsync_Private_TestAsync(Authority authority, AccessType accessType)
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                AccessType = accessType
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var query = from item in accessInfo.Members
                        where item.AccessType == accessType
                        select item.UserID;
            var authentication = await this.TestContext.LoginRandomAsync(authority, item => query.Contains(item.ID));
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        private async Task LeaveAsync_Private_AccessTypeNone_FailTestAsync(Authority authority)
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginRandomAsync(authority, item => accessInfo.GetAccessType(item.ID) == AccessType.None);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }

        private async Task LeaveAsync_Locked_NotLocker_FailTestAsync(Authority authority)
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.Locked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var lockInfo = dataBase.LockInfo;
            var authentication = await this.TestContext.LoginRandomAsync(authority, item => item.ID != lockInfo.UserID);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
        }
    }
}

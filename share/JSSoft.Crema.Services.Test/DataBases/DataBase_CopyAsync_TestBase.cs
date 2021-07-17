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
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.Services.Test
{
    public abstract class DataBase_CopyAsync_TestBase
    {
        public abstract TestContext TestContext { get; set; }

        public abstract TestApplication App { get; }

        public abstract IDataBaseContext DataBaseContext { get; }

        public abstract Authentication ExpiredAuthentication { get; }

        public abstract DataBaseFlags LoadedFlags { get; }

        public abstract Task<string> GetRevisionAsync(IDataBase dataBase);

        //"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem"
        [TestMethod]
        public async Task CopyAsync_Admin_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            var newDataBase = await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
            Assert.AreEqual(newDataBaseName, newDataBase.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CopyAsync_Arg0_Null_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(null, newDataBaseName, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task CopyAsync_Arg0_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(this.ExpiredAuthentication, newDataBaseName, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CopyAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, null, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CopyAsync_Arg_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, string.Empty, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CopyAsync_Arg1_InvalidName_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = RandomUtility.NextInvalidName();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CopyAsync_Arg1_SameName_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = dataBase.Name;
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CopyAsync_Arg2_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, null, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CopyAsync_Arg2_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, string.Empty, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CopyAsync_Arg3_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task CopyAsync_Member_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task CopyAsync_Guest_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Admin_AccessTypeNone_FailTestAsync()
        {
            return this.CopyAsync_Private_AccessTypeNone_FailTestAsync(Authority.Admin);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Member_AccessTypeNone_FailTestAsync()
        {
            return this.CopyAsync_Private_AccessTypeNone_FailTestAsync(Authority.Member);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Guest_AccessTypeNone_FailTestAsync()
        {
            return this.CopyAsync_Private_AccessTypeNone_FailTestAsync(Authority.Guest);
        }

        [TestMethod]
        public async Task CopyAsync_Private_Admin_Owner_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Private | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginAsync(accessInfo.UserID);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        [TestMethod]
        public Task CopyAsync_Private_Admin_Master_TestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Admin, AccessType.Master);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Admin_Developer_FailTestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Admin, AccessType.Developer);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Admin_Editor_FailTestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Admin, AccessType.Editor);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Admin_Guest_FailTestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Admin, AccessType.Guest);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Member_Developer_FailTestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Member, AccessType.Developer);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Member_Editor_FailTestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Member, AccessType.Editor);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Member_Guest_FailTestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Member, AccessType.Guest);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Guest_Guest_FailTestAsync()
        {
            return this.CopyAsync_Private_TestAsync(Authority.Guest, AccessType.Guest);
        }

        public async Task CopyAsync_Locked_Admin_Locker_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.Locked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var lockInfo = dataBase.LockInfo;
            var authentication = await this.TestContext.LoginAsync(lockInfo.UserID);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Locked_Admin_NotLocker_FailTestAsync()
        {
            return this.CopyAsync_Locked_NotLocker_FailTestAsync(Authority.Admin);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Locked_Member_NotLocker_FailTestAsync()
        {
            return this.CopyAsync_Locked_NotLocker_FailTestAsync(Authority.Member);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Locked_Guest_NotLocker_FailTestAsync()
        {
            return this.CopyAsync_Locked_NotLocker_FailTestAsync(Authority.Guest);
        }

        private async Task CopyAsync_Private_TestAsync(Authority authority, AccessType accessType)
        {
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                AccessType = accessType,
                Settings = DataBaseSettings.Default
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var accessInfo = dataBase.AccessInfo;
            var query = from item in accessInfo.Members
                        where item.AccessType == accessType
                        select item.UserID;
            var authentication = await this.TestContext.LoginRandomAsync(authority, item => query.Contains(item.ID));
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        private async Task CopyAsync_Private_AccessTypeNone_FailTestAsync(Authority authority)
        {
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Private | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginRandomAsync(authority, item => accessInfo.GetAccessType(item.ID) == AccessType.None);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

        private async Task CopyAsync_Locked_NotLocker_FailTestAsync(Authority authority)
        {
            var dataBaseFilter = new DataBaseFilter(this.LoadedFlags | DataBaseFlags.Public | DataBaseFlags.Locked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(this.App);
            var lockInfo = dataBase.LockInfo;
            var authentication = await this.TestContext.LoginRandomAsync(authority, item => item.ID != lockInfo.UserID);
            var newDataBaseName = await this.DataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var revision = await this.GetRevisionAsync(dataBase);
            await dataBase.CopyAsync(authentication, newDataBaseName, comment, revision);
        }

    }
}

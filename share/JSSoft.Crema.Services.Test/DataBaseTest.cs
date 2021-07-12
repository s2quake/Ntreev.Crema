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
using JSSoft.Crema.Services.Test.Common;
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
        public async Task GetMetaData_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var metaData = await dataBase.Dispatcher.InvokeAsync(() => dataBase.GetMetaData());
            Assert.AreEqual(dataBase.DataBaseState, metaData.DataBaseState);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetMetaData_Dispatcher_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            dataBase.GetMetaData();
        }

        [TestMethod]
        public async Task LoadAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LoadAsync(authentication);
            Assert.AreEqual(DataBaseState.Loaded, dataBase.DataBaseState);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LoadAsync_Arg0_Null_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LoadAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task LoadAsync_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LoadAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Member_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Guest_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task LoadAsync_Loaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        public async Task LoadAsync_Private_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginAsync(accessInfo.UserID);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        public async Task LoadAsync_Private_Master_Admin_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Master };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userIDs = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID).ToArray();
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin, item => userIDs.Contains(item.ID));
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Private_Master_Member_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Master };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userIDs = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID).ToArray();
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member, item => userIDs.Contains(item.ID));
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Private_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin, item => item.ID != accessInfo.UserID);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Private_Developer_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Developer };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.First(item => item.AccessType == AccessType.Developer).UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Private_Editor_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Editor };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.First(item => item.AccessType == AccessType.Editor).UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task LoadAsync_Private_Guest_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Guest };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.First(item => item.AccessType == AccessType.Guest).UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            await dataBase.LoadAsync(authentication);
        }

        [TestMethod]
        public async Task UnloadAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.UnloadAsync(authentication);
            Assert.AreEqual(DataBaseState.None, dataBase.DataBaseState);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UnloadAsync_Arg0_Null_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.UnloadAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task UnloadAsync_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.UnloadAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Member_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Guest_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnloadAsync_Unloaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        public async Task UnloadAsync_Private_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginAsync(accessInfo.UserID);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        public async Task UnloadAsync_Private_Master_Admin_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Master };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userIDs = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID).ToArray();
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin, item => userIDs.Contains(item.ID));
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Private_Master_Member_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Master };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userIDs = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID).ToArray();
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member, item => userIDs.Contains(item.ID));
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Private_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin, item => item.ID != accessInfo.UserID);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Private_Developer_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Developer };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.First(item => item.AccessType == AccessType.Developer).UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Private_Editor_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Editor };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.First(item => item.AccessType == AccessType.Editor).UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnloadAsync_Private_Guest_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Guest };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.First(item => item.AccessType == AccessType.Guest).UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            await dataBase.UnloadAsync(authentication);
        }

        [TestMethod]
        public async Task EnterAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EnterAsync_Arg0_Null_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task EnterAsync_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task EnterAsync_Enter_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EnterAsync_Unloaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task EnterAsync_Private_CannotAccess_FailTestAsync()
        {
            var userCollection = dataBaseContext.GetService(typeof(IUserCollection)) as IUserCollection;
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var user = await userCollection.GetRandomUserAsync(item => accessInfo.IsMember(item.ID) == false && accessInfo.IsOwner(item.ID) == false);
            var authentication = await this.TestContext.LoginAsync(user.ID);
            await dataBase.EnterAsync(authentication);
        }

        [TestMethod]
        public async Task LeaveAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
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
        [ExpectedException(typeof(ArgumentException))]
        public async Task LeaveAsync_NotEntered_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.LeaveAsync(authentication);
        }

        [TestMethod]
        public async Task RenameAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(authentication, dataBaseName);
            Assert.AreEqual(dataBaseName, dataBase.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsync_Arg0_Null_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = RandomUtility.NextName();
            await dataBase.RenameAsync(null, expectedName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.RenameAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RenameAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.RenameAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RenameAsync_Arg1_InvalideName_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = RandomUtility.NextInvalidName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RenameAsync_Arg1_SameName_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.RenameAsync(authentication, dataBase.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task RenameAsync_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(expiredAuthentication, dataBaseName);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RenameAsync_Member_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RenameAsync_Guest_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RenameAsync_Loaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        public async Task RenameAsync_Private_Owner_TestAsync()
        {
            var userCollection = dataBaseContext.GetService(typeof(IUserCollection)) as IUserCollection;
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var user = await userCollection.GetRandomUserAsync(item => accessInfo.IsOwner(item.ID) == true);
            var authentication = await this.TestContext.LoginAsync(user.ID);
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        public async Task RenameAsync_Private_Master_Admin_TestAsync()
        {
            var userCollection = dataBaseContext.GetService(typeof(IUserCollection)) as IUserCollection;
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Master };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var masterMembers = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin, item => masterMembers.Contains(item.ID));
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RenameAsync_Private_Master_Member_FailTestAsync()
        {
            var userCollection = dataBaseContext.GetService(typeof(IUserCollection)) as IUserCollection;
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var masterMembers = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member, item => masterMembers.Contains(item.ID));
            var dataBaseName = RandomUtility.NextName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RenameAsync_Private_PermissionDenied_FailTestAsync()
        {
            var userCollection = dataBaseContext.GetService(typeof(IUserCollection)) as IUserCollection;
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userFilter = new UserFilter(UserFlags.NotBanned, item => accessInfo.IsMember(item.ID) == false && accessInfo.IsOwner(item.ID) == false);
            var user = await userFilter.GetUserAsync(app);
            var authentication = await this.TestContext.LoginAsync(user.ID);
            var dataBaseName = RandomUtility.NextInvalidName();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TestMethod]
        public async Task DeleteAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = dataBase.Name;
            await dataBase.DeleteAsync(authentication);
            var condition = await dataBaseContext.ContainsAsync(dataBaseName);
            Assert.IsFalse(condition);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_Arg0_Null_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task DeleteAsync_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.DeleteAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_Member_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            await dataBase.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_Guest_PermissionDenied_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            await dataBase.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_Loaded_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            await dataBase.DeleteAsync(authentication);
        }

        [TestMethod]
        public async Task DeleteAsync_Private_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = dataBase.Name;
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginAsync(accessInfo.UserID);
            await dataBase.DeleteAsync(authentication);
            var condition = await dataBaseContext.ContainsAsync(dataBaseName);
            Assert.IsFalse(condition);
        }

        [TestMethod]
        public async Task DeleteAsync_Private_MasterAccessType_Admin_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked) { AccessType = AccessType.Master };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = dataBase.Name;
            var accessInfo = dataBase.AccessInfo;
            var masterMembers = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin, item => masterMembers.Contains(item.ID));
            await dataBase.DeleteAsync(authentication);
            var condition = await dataBaseContext.ContainsAsync(dataBaseName);
            Assert.IsFalse(condition);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_Private_MasterAccessType_Member_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var dataBaseName = dataBase.Name;
            var accessInfo = dataBase.AccessInfo;
            var masterMembers = accessInfo.Members.Where(item => item.AccessType == AccessType.Master).Select(item => item.UserID);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member, item => masterMembers.Contains(item.ID));
            await dataBase.DeleteAsync(authentication);
        }

        [TestMethod]
        public async Task RevertAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RevertAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(null, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RevertAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RevertAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.RevertAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RevertAsync_Arg1_InvalidRevision_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var invalidRevision = RandomUtility.NextString();
            await dataBase.RevertAsync(authentication, invalidRevision);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task RevertAsync_Arg0_Expired_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(expiredAuthentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RevertAsync_Member_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RevertAsync_Guest_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RevertAsync_Loaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RevertAsync_Private_Developer_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                Settings = DataBaseSettings.Default,
                AccessType = AccessType.Developer
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.Where(item => item.AccessType == AccessType.Developer).Random().UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RevertAsync_Private_Editor_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                Settings = DataBaseSettings.Default,
                AccessType = AccessType.Editor
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.Where(item => item.AccessType == AccessType.Editor).Random().UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RevertAsync_Private_Guest_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                Settings = DataBaseSettings.Default,
                AccessType = AccessType.Guest
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var userID = accessInfo.Members.Where(item => item.AccessType == AccessType.Guest).Random().UserID;
            var authentication = await this.TestContext.LoginAsync(userID);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        public async Task ImportAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task Contains_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.EnterAsync(authentication);
            var condition1 = await dataBase.ContainsAsync(authentication);
            Assert.IsTrue(condition1);
            await dataBase.LeaveAsync(authentication);
            var condition2 = await dataBase.ContainsAsync(authentication);
            Assert.IsFalse(condition2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Contains_NotLoaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.ContainsAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Contains_Dispatcher_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            dataBase.Contains(authentication);
        }

        [TestMethod]
        public async Task GetLogAsync_Admin_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs1 = await dataBase.GetLogAsync(authentication, null);
            var logs2 = await dataBase.GetLogAsync(authentication, logs1.Random().Revision);
        }

        [TestMethod]
        public async Task GetLogAsync_Member_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs1 = await dataBase.GetLogAsync(authentication, null);
            var logs2 = await dataBase.GetLogAsync(authentication, logs1.Random().Revision);
        }

        [TestMethod]
        public async Task GetLogAsync_Guest_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var logs1 = await dataBase.GetLogAsync(authentication, null);
            var logs2 = await dataBase.GetLogAsync(authentication, logs1.Random().Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetLogAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetLogAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetLogAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetLogAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetLogAsync_Arg1_InvalidRevision_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var invalidRevision = RandomUtility.NextString();
            await dataBase.GetLogAsync(authentication, invalidRevision);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task GetLogAsync_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetLogAsync(expiredAuthentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task GetLogAsync_Private_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                Settings = DataBaseSettings.Default
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginRandomAsync(item => accessInfo.IsNotMember(item.ID) == true);
            await dataBase.GetLogAsync(authentication, null);
        }

        [TestMethod]
        public async Task GetDataSetAsync_Admin_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetDataSetAsync(authentication, DataSetType.All, null, null);
        }

        [TestMethod]
        public async Task GetDataSetAsync_Member_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetDataSetAsync(authentication, DataSetType.All, null, null);
        }

        [TestMethod]
        public async Task GetDataSetAsync_Guest_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetDataSetAsync(authentication, DataSetType.All, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetDataSetAsync_NotLoaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetDataSetAsync(authentication, DataSetType.All, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetDataSetAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetDataSetAsync(null, DataSetType.All, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetDataSetAsync_Arg3_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetDataSetAsync(authentication, DataSetType.All, null, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task GetDataSetAsync_Expired_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked)
            {
                Settings = DataBaseSettings.Default
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.GetDataSetAsync(expiredAuthentication, DataSetType.All, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task GetDataSetAsync_Private_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                Settings = DataBaseSettings.Default
            };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase.AccessInfo;
            var authentication = await this.TestContext.LoginRandomAsync(item => accessInfo.IsNotMember(item.ID) == true && accessInfo.UserID != item.ID);
            await dataBase.GetDataSetAsync(authentication, DataSetType.All, null, null);
        }

        [TestMethod]
        public async Task BeginTransactionAsync_TestAsync()
        {

        }

        [TestMethod]
        public async Task CopyAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, comment, false);
            Assert.AreEqual(expectedName, dataBase2.Name);
        }

        [TestMethod]
        public async Task CopyAsync_Force_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, comment, true);
            Assert.AreEqual(expectedName, dataBase2.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CopyAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var dataBase2 = await dataBase1.CopyAsync(null, expectedName, comment, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CopyAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var comment = RandomUtility.NextString();
            var dataBase2 = await dataBase1.CopyAsync(authentication, null, comment, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CopyAsync_Arg2_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CopyAsync_Arg2_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, string.Empty, false);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task CopyAsync_Expired_FailTestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var dataBase2 = await dataBase1.CopyAsync(expiredAuthentication, expectedName, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CopyAsync_Loaded_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task CopyAsync_Member_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, comment, false);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task CopyAsync_Guest_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, comment, false);
        }

        [TestMethod]
        public async Task CopyAsync_Private_Owner_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked);
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase1.AccessInfo;
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var authentication = await this.TestContext.LoginAsync(accessInfo.UserID);
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, comment, false);
        }

        public async Task CopyAsync_Private_Member_TestAsync(AccessType accessType)
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Private | DataBaseFlags.NotLocked)
            {
                AccessType = accessType
            };
            var dataBase1 = await dataBaseFilter.GetDataBaseAsync(app);
            var accessInfo = dataBase1.AccessInfo;
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            var authentication = await this.TestContext.LoginRandomAsync(item => accessInfo.GetAccessType(item.ID) == accessType);
            var dataBase2 = await dataBase1.CopyAsync(authentication, expectedName, comment, false);
        }

        [TestMethod]
        public Task CopyAsync_Private_Master_TestAsync()
        {
            return this.CopyAsync_Private_Member_TestAsync(AccessType.Master);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Developer_TestAsync()
        {
            return this.CopyAsync_Private_Member_TestAsync(AccessType.Developer);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Editor_TestAsync()
        {
            return this.CopyAsync_Private_Member_TestAsync(AccessType.Editor);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public Task CopyAsync_Private_Guest_TestAsync()
        {
            return this.CopyAsync_Private_Member_TestAsync(AccessType.Guest);
        }

        [TestMethod]
        public async Task Name_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase1 = await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            var dataBase2 = await dataBaseContext.GetDataBaseAsync(dataBase1.Name);
            Assert.AreEqual(dataBase1.Name, dataBase2.Name);
        }

        [TestMethod]
        public async Task IsLoaded_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            Assert.IsFalse(dataBase.IsLoaded);
            await dataBase.LoadAsync(authentication);
            Assert.IsTrue(dataBase.IsLoaded);
        }

        [TestMethod]
        public async Task IsLocked_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var comment = RandomUtility.NextString();
            Assert.IsFalse(dataBase.IsLocked);
            await dataBase.LockAsync(authentication, comment);
            Assert.IsTrue(dataBase.IsLocked);
        }

        [TestMethod]
        public async Task IsPrivate_TestAsync()
        {
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var comment = RandomUtility.NextString();
            Assert.IsFalse(dataBase.IsPrivate);
            await dataBase.SetPrivateAsync(authentication);
            Assert.IsTrue(dataBase.IsPrivate);
        }

        [TestMethod]
        public async Task ID_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase1 = await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            var dataBase2 = await dataBaseContext.GetDataBaseAsync(dataBase1.ID);
            Assert.AreEqual(dataBase1.ID, dataBase2.ID);
        }

        [TestMethod]
        public async Task DataBaseInfo_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            var dataBaseInfo = dataBase.DataBaseInfo;
            Assert.AreEqual(dataBase.Name, dataBaseInfo.Name);
            Assert.AreEqual(dataBase.ID, dataBaseInfo.ID);
        }

        [TestMethod]
        public async Task DataBaseState_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            var dataBaseState = dataBase.DataBaseState;
            Assert.AreEqual(DataBaseState.None, dataBase.DataBaseState);
        }


        [TestMethod]
        public async Task DataBaseFlags_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            var dataBaseFlags = dataBase.DataBaseFlags;
            Assert.AreEqual(DataBaseFlags.Public | DataBaseFlags.NotLoaded | DataBaseFlags.NotLocked, dataBase.DataBaseFlags);
        }

        [TestMethod]
        public async Task AuthenticationInfos_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var infos = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AuthenticationInfos);
            Assert.IsNotNull(infos);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AuthenticationInfos_Dispatcher_FailTestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var infos = dataBase.AuthenticationInfos;
        }

        [TestMethod]
        public async Task Renamed_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            var expectedName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var oldName = dataBase.Name;
            var actualName = string.Empty;
            await dataBase.AddRenamedEventHandlerAsync(DataBase_Renamed);
            await dataBase.RenameAsync(authentication, expectedName);
            Assert.AreEqual(expectedName, actualName);
            await dataBase.RemoveRenamedEventHandlerAsync(DataBase_Renamed);
            await dataBase.RenameAsync(authentication, oldName);
            Assert.AreEqual(expectedName, actualName);

            void DataBase_Renamed(object sender, EventArgs e)
            {
                if (sender is IDataBase dataBase)
                {
                    actualName = dataBase.Name;
                }
            }
        }

        [TestMethod]
        public async Task Deleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            var actualDataBase = dataBase;
            await dataBase.AddDeletedEventHandlerAsync(DataBase_Deleted);
            await dataBase.DeleteAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_Deleted(object sender, EventArgs e)
            {
                if (sender is IDataBase dataBase && dataBase == actualDataBase)
                {
                    actualDataBase = null;
                }
            }
        }

        [TestMethod]
        public async Task Loaded_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBase.AddLoadedEventHandlerAsync(DataBase_Loaded);
            await dataBase.LoadAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);
            await dataBase.UnloadAsync(authentication);

            await dataBase.RemoveLoadedEventHandlerAsync(DataBase_Loaded);
            actualDataBase = null;
            await dataBase.LoadAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_Loaded(object sender, EventArgs e)
            {
                if (sender is IDataBase dataBase)
                {
                    actualDataBase = dataBase;
                }
            }
        }

        [TestMethod]
        public async Task Unloaded_TestAsync()
        {
            var authenitcation = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBase.AddUnloadedEventHandlerAsync(DataBase_Unloaded);
            await dataBase.UnloadAsync(authenitcation);
            Assert.AreEqual(expectedDataBase, actualDataBase);
            await dataBase.LoadAsync(authenitcation);

            await dataBase.RemoveUnloadedEventHandlerAsync(DataBase_Unloaded);
            actualDataBase = null;
            await dataBase.UnloadAsync(authenitcation);
            Assert.IsNull(actualDataBase);

            void DataBase_Unloaded(object sender, EventArgs e)
            {
                if (sender is IDataBase dataBase)
                {
                    actualDataBase = dataBase;
                }
            }
        }

        [TestMethod]
        public async Task Resetting_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;

            var transaction1 = await dataBase.BeginTransactionAsync(authentication);
            await dataBase.AddResettingEventHandlerAsync(DataBase_Resetting);
            await transaction1.RollbackAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            var transaction2 = await dataBase.BeginTransactionAsync(authentication);
            await dataBase.RemoveResettingEventHandlerAsync(DataBase_Resetting);
            actualDataBase = null;
            await transaction2.RollbackAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_Resetting(object sender, EventArgs e)
            {
                if (sender is IDataBase dataBase)
                {
                    actualDataBase = dataBase;
                }
            }
        }

        [TestMethod]
        public async Task Reset_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;

            var transaction1 = await dataBase.BeginTransactionAsync(authentication);
            await dataBase.AddResetEventHandlerAsync(DataBase_Reset);
            await transaction1.RollbackAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            var transaction2 = await dataBase.BeginTransactionAsync(authentication);
            await dataBase.RemoveResetEventHandlerAsync(DataBase_Reset);
            actualDataBase = null;
            await transaction2.RollbackAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_Reset(object sender, EventArgs e)
            {
                if (sender is IDataBase dataBase)
                {
                    actualDataBase = dataBase;
                }
            }
        }

        [TestMethod]
        public async Task AuthenticationEntered_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedID = authentication.ID;
            var actualID = string.Empty;
            await dataBase.AddAuthenticationEnteredEventHandlerAsync(DataBase_AuthenticationEntered);
            await dataBase.EnterAsync(authentication);
            await dataBase.LeaveAsync(authentication);
            Assert.AreEqual(expectedID, actualID);

            await dataBase.RemoveAuthenticationEnteredEventHandlerAsync(DataBase_AuthenticationEntered);
            actualID = string.Empty;
            await dataBase.EnterAsync(authentication);
            Assert.AreEqual(string.Empty, actualID);

            void DataBase_AuthenticationEntered(object sender, AuthenticationEventArgs e)
            {
                if (object.Equals(sender, dataBase) == true)
                {
                    actualID = e.AuthenticationInfo.ID;
                }
            }
        }

        [TestMethod]
        public async Task AuthenticationLeft_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedID = authentication.ID;
            var actualID = string.Empty;
            await dataBase.EnterAsync(authentication);

            await dataBase.AddAuthenticationLeftEventHandlerAsync(DataBase_AuthenticationLeft);
            await dataBase.LeaveAsync(authentication);
            await dataBase.EnterAsync(authentication);
            Assert.AreEqual(expectedID, actualID);

            await dataBase.RemoveAuthenticationLeftEventHandlerAsync(DataBase_AuthenticationLeft);
            actualID = string.Empty;
            await dataBase.LeaveAsync(authentication);
            Assert.AreEqual(string.Empty, actualID);

            void DataBase_AuthenticationLeft(object sender, AuthenticationEventArgs e)
            {
                if (object.Equals(sender, dataBase) == true)
                {
                    actualID = e.AuthenticationInfo.ID;
                }
            }
        }

        [TestMethod]
        public async Task DataBaseInfoChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var typeCategoryCollection = dataBase.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBase.AddDataBaseInfoChangedEventHandlerAsync(DataBase_DataBaseInfoChanged);
            var typeCategory = await typeCategoryCollection.GetRandomTypeCategoryAsync();
            await typeCategory.AddRandomCategoryAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBase.RemoveDataBaseInfoChangedEventHandlerAsync(DataBase_DataBaseInfoChanged);
            actualDataBase = null;
            await typeCategory.AddRandomCategoryAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_DataBaseInfoChanged(object sender, EventArgs e)
            {
                if (object.Equals(sender, dataBase) == true)
                {
                    actualDataBase = sender as IDataBase;
                }
            }
        }

        [TestMethod]
        public async Task DataBaseStateChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBase.AddDataBaseStateChangedEventHandlerAsync(DataBase_DataBaseStateChanged);
            await dataBase.LoadAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBase.RemoveDataBaseStateChangedEventHandlerAsync(DataBase_DataBaseStateChanged);
            actualDataBase = null;
            await dataBase.UnloadAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_DataBaseStateChanged(object sender, EventArgs e)
            {
                if (object.Equals(sender, dataBase) == true)
                {
                    actualDataBase = sender as IDataBase;
                }
            }
        }

        [TestMethod]
        public async Task LockChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            var comment = RandomUtility.NextString();
            await dataBase.AddLockChangedEventHandlerAsync(DataBase_LockChanged);
            await dataBase.LockAsync(authentication, comment);
            await dataBase.UnlockAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBase.RemoveLockChangedEventHandlerAsync(DataBase_LockChanged);
            actualDataBase = null;
            await dataBase.LockAsync(authentication, comment);
            Assert.IsNull(actualDataBase);

            void DataBase_LockChanged(object sender, EventArgs e)
            {
                if (object.Equals(sender, dataBase) == true)
                {
                    actualDataBase = sender as IDataBase;
                }
            }
        }

        [TestMethod]
        public async Task AccessChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBase.AddAccessChangedEventHandlerAsync(DataBase_AccessChanged);
            await dataBase.SetPrivateAsync(authentication);
            await dataBase.SetPublicAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBase.RemoveAccessChangedEventHandlerAsync(DataBase_AccessChanged);
            actualDataBase = null;
            await dataBase.SetPrivateAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_AccessChanged(object sender, EventArgs e)
            {
                if (object.Equals(sender, dataBase) == true)
                {
                    actualDataBase = sender as IDataBase;
                }
            }
        }

        [TestMethod]
        public async Task TaskCompleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBase.AddTaskCompletedEventHandlerAsync(DataBase_TaskCompleted);
            await dataBase.SetPrivateAsync(authentication);
            await dataBase.SetPublicAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBase.AddTaskCompletedEventHandlerAsync(DataBase_TaskCompleted);
            actualDataBase = null;
            await dataBase.SetPrivateAsync(authentication);
            Assert.IsNull(actualDataBase);

            void DataBase_TaskCompleted(object sender, EventArgs e)
            {
                if (object.Equals(sender, dataBase) == true)
                {
                    actualDataBase = sender as IDataBase;
                }
            }
        }
    }
}

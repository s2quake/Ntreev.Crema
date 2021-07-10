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
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.InitializeRandomItemsAsync(authentication, DataBaseSettings.Default);
            await dataBase.UnloadAsync(authentication);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RevertAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.InitializeRandomItemsAsync(authentication, DataBaseSettings.Default);
            await dataBase.UnloadAsync(authentication);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(null, log.Revision);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RevertAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.InitializeRandomItemsAsync(authentication, DataBaseSettings.Default);
            await dataBase.UnloadAsync(authentication);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RevertAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.InitializeRandomItemsAsync(authentication, DataBaseSettings.Default);
            await dataBase.UnloadAsync(authentication);
            await dataBase.RevertAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RevertAsync_Arg1_InvalidRevision_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            var invalidRevision = RandomUtility.NextString();
            await dataBase.InitializeRandomItemsAsync(authentication, DataBaseSettings.Default);
            await dataBase.UnloadAsync(authentication);
            await dataBase.RevertAsync(authentication, invalidRevision);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task RevertAsync_Arg0_Expired_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
            await dataBase.InitializeRandomItemsAsync(authentication, DataBaseSettings.Default);
            await dataBase.UnloadAsync(authentication);
            var logs = await dataBase.GetLogAsync(authentication, null);
            var log = logs.Skip(1).Random();
            await dataBase.RevertAsync(expiredAuthentication, log.Revision);
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

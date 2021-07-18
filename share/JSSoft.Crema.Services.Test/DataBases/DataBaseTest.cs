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

        //[TestMethod]
        //public async Task GetLogAsync_Admin_TestAsync()
        //{
        //    var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
        //    var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
        //    var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
        //    var logs1 = await dataBase.GetLogAsync(authentication, null);
        //    var logs2 = await dataBase.GetLogAsync(authentication, logs1.Random().Revision);
        //}

        //[TestMethod]
        //public async Task GetLogAsync_Member_TestAsync()
        //{
        //    var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
        //    var dataBaseFilter = new DataBaseFilter(DataBaseFlags.Public | DataBaseFlags.NotLocked) { Settings = DataBaseSettings.Default };
        //    var dataBase = await dataBaseFilter.GetDataBaseAsync(app);
        //    var logs1 = await dataBase.GetLogAsync(authentication, null);
        //    var logs2 = await dataBase.GetLogAsync(authentication, logs1.Random().Revision);
        //}

        [TestMethod]
        public async Task BeginTransactionAsync_TestAsync()
        {

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

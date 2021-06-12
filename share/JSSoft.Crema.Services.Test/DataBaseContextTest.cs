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
    public class DataBaseContextTest
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
            await context.LoadRandomDataBaseManyAsync(dataBaseContext, expiredAuthentication);
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
            var metaData = await dataBaseContext.GetMetaDataAsync(authentication);
            Assert.AreNotEqual(0, metaData.DataBases.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetMetaData_Arg0_Null_TestAsync()
        {
            await dataBaseContext.GetMetaDataAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task GetMetaData_Expired_TestAsync()
        {
            await dataBaseContext.GetMetaDataAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetMetaData_Dispatcher_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            dataBaseContext.GetMetaData(authentication);
        }

        [TestMethod]
        public async Task AddNewDataBaseAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewDataBaseAsync_Arg0_Null_TestAsync()
        {
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(null, dataBaseName, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewDataBaseAsync_Arg1_Null_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, null, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewDataBaseAsync_Arg1_Exists_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var dataBaseName = dataBase.Name;
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewDataBaseAsync_Arg1_Empty_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, string.Empty, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewDataBaseAsync_Arg1_InvalidName_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseName = RandomUtility.NextInvalidName();
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, string.Empty, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewDataBaseAsync_Arg2_Null_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewDataBaseAsync_Arg2_Empty_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task AddNewDataBaseAsync_Expired_TestAsync()
        {
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(expiredAuthentication, dataBaseName, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task AddNewDataBaseAsync_Member_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task AddNewDataBaseAsync_Guest_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        [TestMethod]
        public async Task Contains_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var dataBaseName = dataBase.Name;
            var condition = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Contains(dataBaseName));
            Assert.IsTrue(condition);
        }

        [TestMethod]
        public async Task Contains_Arg0_Empty_TestAsync()
        {
            var condition = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Contains(string.Empty));
            Assert.IsFalse(condition);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Contains_Arg0_Null_FailTestAsync()
        {
            await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Contains(null));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Contains_Dispatcher_FailTestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var dataBaseName = dataBase.Name;
            var condition = dataBaseContext.Contains(dataBaseName);
        }

        [TestMethod]
        public async Task IndexerByDataBaseName_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var dataBaseName = dataBase.Name;
            var dataBase2 = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[dataBaseName]);
            Assert.AreEqual(dataBase, dataBase2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexerByDataBaseName_Arg0_Null_FailTestAsync()
        {
            await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[null]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task IndexerByDataBaseName_Arg0_Empty_FailTestAsync()
        {
            await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[string.Empty]);
        }

        [TestMethod]
        [ExpectedException(typeof(DataBaseNotFoundException))]
        public async Task IndexerByDataBaseName_Arg0_Nonexistent_FailTestAsync()
        {
            var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[dataBaseName]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task IndexerByDataBaseName_Dispatcher_FailTestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var dataBaseName = dataBase.Name;
            var dataBase2 = dataBaseContext[dataBaseName];
            Assert.Fail(dataBase2.Name);
        }

        [TestMethod]
        public async Task IndexerByDataBaseID_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var dataBaseID = dataBase.ID;
            var dataBase2 = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[dataBaseID]);
            Assert.AreEqual(dataBase, dataBase2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task IndexerByDataBaseID_Arg0_Empty_TestAsync()
        {
            await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[Guid.Empty]);
        }

        [TestMethod]
        [ExpectedException(typeof(DataBaseNotFoundException))]
        public async Task IndexerByDataBaseID_Arg0_Nonexistent_TestAsync()
        {
            await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[Guid.NewGuid()]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task IndexerByDataBaseID_Dispatcher_TestAsync()
        {
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync();
            var dataBaseID = dataBase.ID;
            var dataBase2 = dataBaseContext[dataBaseID];
            Assert.Fail(dataBase2.Name);
        }

        [TestMethod]
        public async Task ItemsCreated_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var expectedDataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync();
            var expectedComment = RandomUtility.NextString();
            var actualDataBaseName = string.Empty;
            var actualComment = string.Empty;
            await dataBaseContext.AddItemsCreatedEventHandlerAsync(DataBaseContext_ItemsCreated);
            await dataBaseContext.AddNewDataBaseAsync(authentication, expectedDataBaseName, expectedComment);
            Assert.AreEqual(expectedDataBaseName, actualDataBaseName);
            Assert.AreEqual(expectedComment, actualComment);

            await dataBaseContext.RemoveItemsCreatedEventHandlerAsync(DataBaseContext_ItemsCreated);
            await dataBaseContext.AddNewRandomDataBaseAsync(authentication);
            Assert.AreEqual(expectedDataBaseName, actualDataBaseName);
            Assert.AreEqual(expectedComment, actualComment);

            void DataBaseContext_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
            {
                var dataBaseInfo = (DataBaseInfo)e.Arguments.Single();
                actualDataBaseName = dataBaseInfo.Name;
                actualComment = dataBaseInfo.Comment;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsCreated_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsCreated += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsRenamed_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None);
            var oldDataBaseName = dataBase.Name;
            var expectedDataBaseName = RandomUtility.NextName();
            var actualDataBaseName = string.Empty;
            await dataBaseContext.AddItemsRenamedEventHandlerAsync(DataBaseContext_ItemsRenamed);
            await dataBase.RenameAsync(authentication, expectedDataBaseName);
            Assert.AreEqual(expectedDataBaseName, actualDataBaseName);

            await dataBaseContext.RemoveItemsRenamedEventHandlerAsync(DataBaseContext_ItemsRenamed);
            await dataBase.RenameAsync(authentication, oldDataBaseName);
            Assert.AreEqual(expectedDataBaseName, actualDataBaseName);

            void DataBaseContext_ItemsRenamed(object sender, ItemsRenamedEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single() as IDataBase;
                actualDataBaseName = dataBase.Name;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsRenamed_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsRenamed += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsDeleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var actualDataBaseName = string.Empty;
            var actualComment = string.Empty;
            var dataBase1 = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None);
            var dataBase2 = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None, item => item.Name != dataBase1.Name);
            var expectedDataBaseName = dataBase1.Name;
            var expectedComment = dataBase1.DataBaseInfo.Comment;

            await dataBaseContext.AddItemsDeletedEventHandlerAsync(DataBaseContext_ItemsDeleted);
            await dataBase1.DeleteAsync(authentication);
            Assert.AreEqual(expectedDataBaseName, actualDataBaseName);
            Assert.AreEqual(expectedComment, actualComment);

            await dataBaseContext.RemoveItemsDeletedEventHandlerAsync(DataBaseContext_ItemsDeleted);
            await dataBase2.DeleteAsync(authentication);
            Assert.AreEqual(expectedDataBaseName, actualDataBaseName);
            Assert.AreEqual(expectedComment, actualComment);

            void DataBaseContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single() as IDataBase;
                var dataBaseInfo = dataBase.DataBaseInfo;
                actualDataBaseName = dataBaseInfo.Name;
                actualComment = dataBaseInfo.Comment;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsDeleted_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsDeleted += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsLoaded_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase1 = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None);
            var dataBase2 = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None, item => item != dataBase1);
            var expectedDataBase = dataBase1;
            var actualDataBase = null as IDataBase;
            await dataBaseContext.AddItemsLoadedEventHandlerAsync(DataBaseContext_ItemsLoaded);
            await dataBase1.LoadAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBaseContext.RemoveItemsLoadedEventHandlerAsync(DataBaseContext_ItemsLoaded);
            await dataBase2.LoadAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            void DataBaseContext_ItemsLoaded(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single() as IDataBase;
                actualDataBase = dataBase;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsLoaded_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsLoaded += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsUnloaded_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase1 = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            var dataBase2 = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded, item => item != dataBase1);

            var expectedDataBase = dataBase1;
            var actualDataBase = null as IDataBase;
            await dataBaseContext.AddItemsUnloadedEventHandlerAsync(DataBaseContext_ItemsUnloaded);
            await dataBase1.UnloadAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBaseContext.RemoveItemsUnloadedEventHandlerAsync(DataBaseContext_ItemsUnloaded);
            await dataBase2.UnloadAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            void DataBaseContext_ItemsUnloaded(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single() as IDataBase;
                actualDataBase = dataBase;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsUnloaded_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsUnloaded += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsResetting_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBaseContext.AddItemsResettingEventHandlerAsync(DataBaseContext_ItemsResetting);
            var transaction1 = await dataBase.BeginTransactionAsync(authentication);
            await transaction1.RollbackAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBaseContext.RemoveItemsResettingEventHandlerAsync(DataBaseContext_ItemsResetting);
            var transaction2 = await dataBase.BeginTransactionAsync(authentication);
            await transaction2.RollbackAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            void DataBaseContext_ItemsResetting(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single() as IDataBase;
                actualDataBase = dataBase;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsResetting_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsResetting += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsReset_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            var expectedDataBase = dataBase;
            var actualDataBase = null as IDataBase;
            await dataBaseContext.AddItemsResetEventHandlerAsync(DataBaseContext_ItemsReset);
            var transaction1 = await dataBase.BeginTransactionAsync(authentication);
            await transaction1.RollbackAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            await dataBaseContext.RemoveItemsResetEventHandlerAsync(DataBaseContext_ItemsReset);
            var transaction2 = await dataBase.BeginTransactionAsync(authentication);
            await transaction2.RollbackAsync(authentication);
            Assert.AreEqual(expectedDataBase, actualDataBase);

            void DataBaseContext_ItemsReset(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single() as IDataBase;
                actualDataBase = dataBase;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsReset_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsReset += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsAuthenticationEntered_TestAsync()
        {
            var authentication1 = await this.TestContext.LoginRandomAsync();
            var authentication2 = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            var expectedDataBase = dataBase;
            var expectedUserID = authentication1.ID;
            var actualDataBase = null as IDataBase;
            var actualUserID = string.Empty;

            await dataBaseContext.AddItemsAuthenticationEnteredEventHandlerAsync(DataBaseContext_ItemsAuthenticationEntered);
            await dataBase.EnterAsync(authentication1);
            Assert.AreEqual(expectedDataBase, actualDataBase);
            Assert.AreEqual(expectedUserID, actualUserID);

            await dataBaseContext.RemoveItemsAuthenticationEnteredEventHandlerAsync(DataBaseContext_ItemsAuthenticationEntered);
            await dataBase.EnterAsync(authentication2);
            Assert.AreEqual(expectedDataBase, actualDataBase);
            Assert.AreEqual(expectedUserID, actualUserID);

            void DataBaseContext_ItemsAuthenticationEntered(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single();
                actualDataBase = dataBase;
                actualUserID = e.InvokeID;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsAuthenticationEntered_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsAuthenticationEntered += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsAuthenticationLeft_TestAsync()
        {
            var authentication1 = await this.TestContext.LoginRandomAsync();
            var authentication2 = await this.TestContext.LoginRandomAsync();
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            var expectedDataBase = dataBase;
            var expectedUserID = authentication1.ID;
            var actualDataBase = null as IDataBase;
            var actualUserID = string.Empty;

            await dataBaseContext.AddItemsAuthenticationLeftEventHandlerAsync(DataBaseContext_ItemsAuthenticationLeft);
            await dataBase.LeaveAsync(authentication1);
            Assert.AreEqual(expectedDataBase, actualDataBase);
            Assert.AreEqual(expectedUserID, actualUserID);

            await dataBaseContext.RemoveItemsAuthenticationLeftEventHandlerAsync(DataBaseContext_ItemsAuthenticationLeft);
            await dataBase.LeaveAsync(authentication2);
            Assert.AreEqual(expectedDataBase, actualDataBase);
            Assert.AreEqual(expectedUserID, actualUserID);

            void DataBaseContext_ItemsAuthenticationLeft(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single();
                actualDataBase = dataBase;
                actualUserID = e.InvokeID;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsAuthenticationLeft_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsAuthenticationLeft += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsInfoChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded);
            var dataBaseInfo = dataBase.DataBaseInfo;
            var typeContext = dataBase.GetService(typeof(ITypeContext)) as ITypeContext;
            var expectedModificationInfo = dataBaseInfo.ModificationInfo;
            var actualModificationInfo = dataBaseInfo.ModificationInfo;

            await dataBase.EnterAsync(authentication);
            await dataBaseContext.AddItemsInfoChangedEventHandlerAsync(DataBaseContext_ItemsInfoChanged);
            await typeContext.AddRandomCategoryAsync(authentication);
            Assert.AreNotEqual(expectedModificationInfo, actualModificationInfo);
            actualModificationInfo = expectedModificationInfo;

            await dataBaseContext.RemoveItemsInfoChangedEventHandlerAsync(DataBaseContext_ItemsInfoChanged);
            await typeContext.AddRandomCategoryAsync(authentication);
            await dataBase.LeaveAsync(authentication);
            Assert.AreEqual(expectedModificationInfo, actualModificationInfo);

            void DataBaseContext_ItemsInfoChanged(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single();
                actualModificationInfo = dataBase.DataBaseInfo.ModificationInfo;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsInfoChanged_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsInfoChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsStateChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None);
            var expectedDataBaseState = DataBaseState.Loaded;
            var actualDataBaseState = dataBase.DataBaseState;

            await dataBaseContext.AddItemsStateChangedEventHandlerAsync(DataBaseContext_ItemsStateChanged);
            await dataBase.LoadAsync(authentication);
            Assert.AreEqual(expectedDataBaseState, actualDataBaseState);

            await dataBaseContext.RemoveItemsStateChangedEventHandlerAsync(DataBaseContext_ItemsStateChanged);
            await dataBase.UnloadAsync(authentication);
            Assert.AreEqual(expectedDataBaseState, actualDataBaseState);

            void DataBaseContext_ItemsStateChanged(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single();
                actualDataBaseState = dataBase.DataBaseState;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsStateChanged_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsStateChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsAccessChanged_TestAsync()
        {
            var authenticaiton = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.Loaded, item => item.AccessInfo.IsPublic == true);
            var actualValue = dataBase.AccessInfo.IsPublic;

            await dataBaseContext.AddItemsAccessChangedEventHandlerAsync(DataBaseContext_ItemsAccessChanged);
            await dataBase.SetPrivateAsync(authenticaiton);
            Assert.IsFalse(actualValue);

            await dataBaseContext.RemoveItemsAccessChangedEventHandlerAsync(DataBaseContext_ItemsAccessChanged);
            await dataBase.SetPublicAsync(authenticaiton);
            Assert.IsFalse(actualValue);

            void DataBaseContext_ItemsAccessChanged(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single() as IDataBase;
                actualValue = dataBase.AccessInfo.IsPublic;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsAccessChanged_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsAccessChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task ItemsLockChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(item => item.LockInfo.IsNotLocked == true);
            var expectedComment = RandomUtility.NextString();
            var actualValue = dataBase.LockInfo.IsNotLocked;
            var actualComment = string.Empty;

            await dataBaseContext.AddItemsLockChangedEventHandlerAsync(DataBaseContext_ItemsLockChanged);
            await dataBase.LockAsync(authentication, expectedComment);
            Assert.IsFalse(actualValue);
            Assert.AreEqual(expectedComment, actualComment);

            await dataBaseContext.RemoveItemsLockChangedEventHandlerAsync(DataBaseContext_ItemsLockChanged);
            await dataBase.UnlockAsync(authentication);
            Assert.IsFalse(actualValue);
            Assert.AreEqual(expectedComment, actualComment);

            void DataBaseContext_ItemsLockChanged(object sender, ItemsEventArgs<IDataBase> e)
            {
                var dataBase = e.Items.Single();
                actualValue = dataBase.LockInfo.IsNotLocked;
                actualComment = dataBase.LockInfo.Comment;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ItemsLockChanged_Dispatcher_FailTest()
        {
            dataBaseContext.ItemsLockChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task TaskCompleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseState.None);
            var actualID = Guid.Empty;

            await dataBaseContext.AddTaskCompletedEventHandlerAsync(DataBaseContext_TaskCompleted);
            var taskID1 = await (dataBase.LoadAsync(authentication) as Task<Guid>);
            Assert.AreEqual(taskID1, actualID);

            await dataBaseContext.RemoveTaskCompletedEventHandlerAsync(DataBaseContext_TaskCompleted);
            var taskID2 = await (dataBase.UnloadAsync(authentication) as Task<Guid>);
            Assert.AreEqual(taskID1, actualID);
            Assert.AreNotEqual(Guid.Empty, taskID2);

            void DataBaseContext_TaskCompleted(object sender, TaskCompletedEventArgs e)
            {
                if (e.InvokeID == authentication.ID)
                    actualID = e.TaskIDs.Single();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TaskCompleted_Dispatcher_FailTest()
        {
            dataBaseContext.TaskCompleted += (s, e) => { };
        }


        [TestMethod]
        public async Task GetEnumerator_TestAsync()
        {
            await dataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                var enumerator = (dataBaseContext as IEnumerable).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Assert.IsNotNull(enumerator.Current);
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetEnumerator_Dispatcher_FailTest()
        {
            var enumerator = (dataBaseContext as IEnumerable).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task GetGenericEnumerator_Test()
        {
            await dataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                var enumerator = (dataBaseContext as IEnumerable<IDataBase>).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Assert.IsNotNull(enumerator.Current);
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetGenericEnumerator_Dispatcher_FailTest()
        {
            var enumerator = (dataBaseContext as IEnumerable<IDataBase>).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task Count_TestAsync()
        {
            var count = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Count);
            Assert.AreEqual(typeof(int), count.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Count_Dispatcher_FailTest()
        {
            Assert.Fail($"{dataBaseContext.Count}");
        }
    }
}

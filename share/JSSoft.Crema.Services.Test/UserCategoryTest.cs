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
using JSSoft.Crema.Services.Test.Extensions.Filters;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class UserCategoryTest
    {
        private static TestApplication app;
        private static IUserCategoryCollection userCategoryCollection;
        private static IUserCollection userCollection;
        private static IUserContext userContext;
        private static Authentication expiredAuthentication;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new();
            await app.InitializeAsync(context);
            await app.OpenAsync();
            userCategoryCollection = app.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            userCollection = app.GetService(typeof(IUserCollection)) as IUserCollection;
            userContext = app.GetService(typeof(IUserContext)) as IUserContext;
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
        public async Task RenameAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = RandomUtility.NextName();
            await userCategory.RenameAsync(authentication, name);
            Assert.AreEqual(name, userCategory.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsync_Arg0_Null_FailTestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = RandomUtility.NextName();
            await userCategory.RenameAsync(null, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.RenameAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RenameAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.RenameAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RenameAsync_Arg1_InvalidName_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = RandomUtility.NextInvalidName();
            await userCategory.RenameAsync(authentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RenameAsync_Root_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategory = userCategoryCollection.Root;
            var name = RandomUtility.NextName();
            await userCategory.RenameAsync(authentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RenameAsync_Member_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = RandomUtility.NextName();
            await userCategory.RenameAsync(authentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task RenameAsync_Guest_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = RandomUtility.NextName();
            await userCategory.RenameAsync(authentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task RenameAsync_Expired_FailTestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = RandomUtility.NextName();
            await userCategory.RenameAsync(expiredAuthentication, null);
        }

        [TestMethod]
        public async Task MoveAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategoryFilter = new UserCategoryFilter() { CategoryToMove = userCategory };
            var parentCategory = await parentCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.MoveAsync(authentication, parentCategory.Path);
            Assert.AreEqual(parentCategory, userCategory.Parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsync_Arg0_Null_FailTestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategoryFilter = new UserCategoryFilter() { CategoryToMove = userCategory };
            var parentCategory = await parentCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.MoveAsync(null, parentCategory.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.MoveAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task MoveAsync_Expired_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategoryFilter = new UserCategoryFilter() { CategoryToMove = userCategory };
            var parentCategory = await parentCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.MoveAsync(expiredAuthentication, parentCategory.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task MoveAsync_Root_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategory = userCategoryCollection.Root;
            var parentCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var parentCategory = await parentCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.MoveAsync(authentication, parentCategory.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsync_SameParent_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategory = userCategory.Parent;
            await userCategory.MoveAsync(authentication, parentCategory.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsync_Member_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategoryFilter = new UserCategoryFilter() { CategoryToMove = userCategory };
            var parentCategory = await parentCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.MoveAsync(authentication, parentCategory.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsync_Guest_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategoryFilter = new UserCategoryFilter() { CategoryToMove = userCategory };
            var parentCategory = await parentCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.MoveAsync(authentication, parentCategory.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsync_ToChild_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, HasCategories = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategory = await userCategory.Dispatcher.InvokeAsync(() => userCategory.Categories.Random());
            await userCategory.MoveAsync(authentication, parentCategory.Path);
        }

        [TestMethod]
        public async Task DeleteAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_Arg0_Null_FailTestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task DeleteAsync_Expired_FailTestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.DeleteAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_HasCategories_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, HasCategories = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_HasUsers_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, HasUsers = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_Member_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_Guest_PermissionDenied_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_Root_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategory = userCategoryCollection.Root;
            await userCategory.DeleteAsync(authentication);
        }

        [TestMethod]
        public async Task AddNewCategoryAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = await userCategory.GenerateNewCategoryNameAsync(RandomUtility.NextName());
            var newCategory = await userCategory.AddNewCategoryAsync(authentication, name);
            Assert.AreEqual(name, newCategory.Name);
            Assert.AreEqual(userCategory, newCategory.Parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewCategoryAsync_Arg0_Null_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = await userCategory.GenerateNewCategoryNameAsync(RandomUtility.NextName());
            await userCategory.AddNewCategoryAsync(null, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewCategoryAsync_Arg1_Null_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.AddNewCategoryAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewCategoryAsync_Arg1_Empty_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.AddNewCategoryAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task AddNewCategoryAsync_Expired_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = await userCategory.GenerateNewCategoryNameAsync(RandomUtility.NextName());
            await userCategory.AddNewCategoryAsync(expiredAuthentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewCategoryAsync_InvalidName_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = await userCategory.GenerateNewCategoryNameAsync(RandomUtility.NextInvalidName());
            await userCategory.AddNewCategoryAsync(authentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewCategoryAsync_ExitsCategoryName_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, HasCategories = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = await userCategory.Dispatcher.InvokeAsync(() => userCategory.Categories.Random().Name);
            await userCategory.AddNewCategoryAsync(authentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task AddNewCategoryAsync_Member_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, HasCategories = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = await userCategory.Dispatcher.InvokeAsync(() => userCategory.Categories.Random().Name);
            await userCategory.AddNewCategoryAsync(authentication, name);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task AddNewCategoryAsync_Guest_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, HasCategories = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var name = await userCategory.Dispatcher.InvokeAsync(() => userCategory.Categories.Random().Name);
            await userCategory.AddNewCategoryAsync(authentication, name);
        }

        [TestMethod]
        public async Task AddNewUserAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            var newUser = await userCategory.AddNewUserAsync(authentication, userID, password, userName, authority);
            Assert.AreEqual(userID, newUser.ID);
            Assert.AreEqual(userName, newUser.UserName);
            Assert.AreEqual(authority, newUser.Authority);
            Assert.AreEqual(UserState.None, newUser.UserState);
            Assert.IsFalse(newUser.BanInfo.IsBanned);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewUserAsync_Arg0_Null_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(null, userID, password, userName, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewUserAsync_Arg1_Null_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(authentication, null, password, userName, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewUserAsync_Arg0_Empty_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(authentication, string.Empty, password, userName, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewUserAsync_Arg2_Null_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(authentication, userID, null, userName, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddNewUserAsync_Arg3_Null_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            await userCategory.AddNewUserAsync(authentication, userID, password, null, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewUserAsync_Arg3_Empty_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            await userCategory.AddNewUserAsync(authentication, userID, password, string.Empty, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewUserAsync_Arg4_None_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(authentication, userID, password, userName, Authority.None);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task AddNewUserAsync_Expired_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(expiredAuthentication, userID, password, userName, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task AddNewUserAsync_Member_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(authentication, userID, password, userName, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task AddNewUserAsync_Guest_PermissionDenied_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var userID = await userCollection.GenerateNewUserIDAsync();
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(authentication, userID, password, userName, authority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddNewUserAsync_ExistsUesr_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var user = await userCollection.GetRandomUserAsync();
            var userID = user.ID;
            var authority = userContext.GetRandomAuthority();
            var password = userContext.GetPassword(authority);
            var userName = RandomUtility.NextName();
            await userCategory.AddNewUserAsync(authentication, userID, password, userName, authority);
        }

        [TestMethod]
        public async Task Name_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            Assert.AreNotEqual(string.Empty, userCategory.Name);
        }

        [TestMethod]
        public void Name_Root_Test()
        {
            var userCategory = userCategoryCollection.Root;
            Assert.AreEqual(string.Empty, userCategory.Name);
        }

        [TestMethod]
        public async Task Path_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            NameValidator.ValidateCategoryPath(userCategory.Path);
        }

        [TestMethod]
        public async Task Parent_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            Assert.IsNotNull(userCategory.Parent);
        }

        [TestMethod]
        public void Parent_Root_Test()
        {
            var userCategory = userCategoryCollection.Root;
            Assert.IsNull(userCategory.Parent);
        }

        [TestMethod]
        public async Task Categories_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasCategories = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in userCategory.Categories)
                {
                    Assert.IsNotNull(item);
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Categories_Dispatcher_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasCategories = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            foreach (var item in userCategory.Categories)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task Users_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasUsers = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            await userCategory.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in userCategory.Users)
                {
                    Assert.IsNotNull(item);
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Users_Dispatcher_TestAsync()
        {
            var userCategoryFilter = new UserCategoryFilter() { HasUsers = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            foreach (var item in userCategory.Users)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task Renamed_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var oldName = userCategory.Name;
            var expectedName = RandomUtility.NextName();
            var actualName = string.Empty;
            await userCategory.Dispatcher.InvokeAsync(() =>
            {
                userCategory.Renamed += UserCategory_Renamed;
            });
            await userCategory.RenameAsync(authentication, expectedName);
            Assert.AreEqual(expectedName, actualName);
            await userCategory.Dispatcher.InvokeAsync(() =>
            {
                userCategory.Renamed -= UserCategory_Renamed;
            });
            await userCategory.RenameAsync(authentication, oldName);
            Assert.AreEqual(expectedName, actualName);

            void UserCategory_Renamed(object sender, EventArgs e)
            {
                if (sender is IUserCategory userCategory)
                {
                    actualName = userCategory.Name;
                }
            }
        }

        [TestMethod]
        public async Task Moved_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var parentCategoryFilter = new UserCategoryFilter() { CategoryToMove = userCategory };
            var parentCategory = await parentCategoryFilter.GetUserCategoryAsync(app);
            var oldParentPath = userCategory.Parent.Path;
            var expectedParentPath = parentCategory.Path;
            var actualParentPath = string.Empty;
            await userCategory.Dispatcher.InvokeAsync(() =>
            {
                userCategory.Moved += UserCategory_Moved;
            });
            await userCategory.MoveAsync(authentication, expectedParentPath);
            Assert.AreEqual(expectedParentPath, actualParentPath);
            await userCategory.Dispatcher.InvokeAsync(() =>
            {
                userCategory.Moved -= UserCategory_Moved;
            });
            await userCategory.MoveAsync(authentication, oldParentPath);
            Assert.AreEqual(expectedParentPath, actualParentPath);

            void UserCategory_Moved(object sender, EventArgs e)
            {
                if (sender is IUserCategory userCategory)
                {
                    actualParentPath = userCategory.Parent.Path;
                }
            }
        }

        public async Task Deleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userCategoryFilter = new UserCategoryFilter() { HasParent = true, IsLeaf = true };
            var userCategory = await userCategoryFilter.GetUserCategoryAsync(app);
            var actualCategory = userCategory;
            await userCategory.Dispatcher.InvokeAsync(() =>
            {
                userCategory.Deleted += UserCategory_Deleted;
            });
            await userCategory.DeleteAsync(authentication);
            Assert.IsNull(actualCategory);

            void UserCategory_Deleted(object sender, EventArgs e)
            {
                if (sender is IUserCategory userCategory)
                {
                    actualCategory = null;
                }
            }
        }
    }
}

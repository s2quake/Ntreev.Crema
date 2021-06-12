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
    public class UserTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Guid token;
        private static IUserCategoryCollection userCategoryCollection;
        private static IUserCollection userCollection;
        private static Authentication expiredAuthentication;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context);
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            token = await cremaHost.OpenAsync();
            userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            expiredAuthentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            await cremaHost.LogoutAsync(expiredAuthentication);
            await context.LoginRandomManyAsync(cremaHost);
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
        public async Task MoveAsync_TestAsync()
        {
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync();
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            await user.MoveAsync(authentication, category.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsync_Arg0_Null_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            await user.MoveAsync(null, "/");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync();
            await user.MoveAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsync_Arg1_InvalidPath_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync();
            var categoryPath = RandomUtility.NextInvalidCategoryPath();
            await user.MoveAsync(authentication, categoryPath);
        }

        [TestMethod]
        [ExpectedException(typeof(CategoryNotFoundException))]
        public async Task MoveAsync_Arg1_NotExistsPath_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync();
            var userCategoryColleciton = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryColleciton.GetRandomUserCategoryAsync(item => item.Path != user.Category.Path);
            var categoryPath = new CategoryName(category.Path, RandomUtility.NextName());
            await user.MoveAsync(authentication, categoryPath);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task MoveAsync_Expired_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            await user.MoveAsync(expiredAuthentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsync_PermissionDenied_Member_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var user = await userCollection.GetRandomUserAsync();
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            await user.MoveAsync(authentication, category.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsync_PermissionDenied_Guest_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var user = await userCollection.GetRandomUserAsync();
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            await user.MoveAsync(authentication, category.Path);
        }

        [TestMethod]
        public async Task DeleteAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.None, item => item.ID != Authentication.AdminID && item.ID != authentication.ID);
            await user.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_Arg0_Null_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            await user.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task DeleteAsync_Expired_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            await user.DeleteAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_PermissionDenied_AdminID_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var admin = await userCollection.GetUserAsync(Authentication.AdminID);
            await admin.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_PermissionDenied_Member_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var user = await userCollection.GetRandomUserAsync();
            await user.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_PermissionDenied_Guest_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var user = await userCollection.GetRandomUserAsync();
            await user.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_Online_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != Authentication.AdminID && item.ID != authentication.ID);
            await user.DeleteAsync(authentication);
        }

        [TestMethod]
        public async Task SetUserNameAsync_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password = user.GetPassword();
            var userName = RandomUtility.NextName();
            var dateTime = DateTime.UtcNow;
            await user.SetUserNameAsync(authentication, password, userName);
            Assert.AreEqual(userName, user.UserName);
            Assert.IsTrue(user.UserInfo.ModificationInfo.DateTime > dateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetUserNameAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password = $"{user.Authority}".ToLower().ToSecureString();
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(null, password, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetUserNameAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(authentication, null, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetUserNameAsync_Arg1_WrongPassword_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password = user.GetNextPassword();
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(authentication, password, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetUserNameAsync_Arg2_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password = $"{user.Authority}".ToLower().ToSecureString();
            await user.SetUserNameAsync(authentication, password, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetUserNameAsync_Arg2_Empty_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password = user.GetPassword();
            await user.SetUserNameAsync(authentication, password, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task SetUserNameAsync_Expired_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password = user.GetPassword();
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(expiredAuthentication, password, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetUserNameAsync_OtherUser_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var otherUser = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != Authentication.AdminID);
            var password = otherUser.GetPassword();
            var userName = RandomUtility.NextName();
            await otherUser.SetUserNameAsync(authentication, password, userName);
            this.TestContext.WriteLine($"{nameof(otherUser)}: {otherUser.ID}");
            this.TestContext.WriteLine($"{nameof(authentication)}: {authentication.ID}");
        }

        [TestMethod]
        public async Task SetPasswordAsync_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password1 = user.GetPassword();
            var password2 = user.GetNextPassword();
            var dateTime = DateTime.UtcNow;
            await Task.Delay(1000);
            await user.SetPasswordAsync(authentication, password1, password2);
            await user.SetPasswordAsync(authentication, password2, password1);
            Assert.IsTrue(user.UserInfo.ModificationInfo.DateTime > dateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetPasswordAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password1 = user.GetPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(null, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetPasswordAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(authentication, null, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetPasswordAsync_Arg2_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password1 = user.GetPassword();
            await user.SetPasswordAsync(authentication, password1, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task SetPasswordAsync_Expired_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password1 = user.GetPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(expiredAuthentication, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetPasswordAsync_Arg1_WrongPassword_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password1 = user.GetNextPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(authentication, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetPasswordAsync_Arg1_SamePassword_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var password1 = user.GetPassword();
            var password2 = user.GetPassword();
            await user.SetPasswordAsync(authentication, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPasswordAsync_Other_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            var password1 = user.GetPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(authentication, password1, password2);
        }

        [TestMethod]
        public async Task ResetPasswordAsync_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            await user.ResetPasswordAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ResetPasswordAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            await user.ResetPasswordAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task ResetPasswordAsync_Expired_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            await user.ResetPasswordAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task ResetPasswordAsync_Admin_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            await user.ResetPasswordAsync(authentication);
        }

        [TestMethod]
        public async Task SendMessageAsync_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.SendMessageAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendMessageAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.SendMessageAsync(null, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendMessageAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            await user.SendMessageAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SendMessageAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            await user.SendMessageAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task SendMessageAsync_Expired_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            await user.SendMessageAsync(expiredAuthentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SendMessageAsync_Offline_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync();
            var user = await userCollection.GetRandomUserAsync(UserState.None, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.SendMessageAsync(authentication, message);
        }

        [TestMethod]
        public async Task KickAsync_Admin_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Admin, UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        public async Task KickAsync_Member_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Member, UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        public async Task KickAsync_Guest_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Guest, UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task KickAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(null, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task KickAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID);
            await user.KickAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task KickAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID);
            await user.KickAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task KickAsync_Expired_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(expiredAuthentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task KickAsync_PermissionDenied_Member_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Member);
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task KickAsync_PermissionDenied_Guest_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Guest);
            var user = await userCollection.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task KickAsync_Offline_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.None, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task KickAsync_Self_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetUserAsync(authentication.ID);
            var message = RandomUtility.NextString();
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        public async Task BanAsync_Online_Member_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var message = RandomUtility.NextString();
            var user = await userCollection.GetRandomUserAsync(Authority.Member, UserState.Online, item => item.ID != authentication.ID && item.BanInfo.IsNotBanned);
            await user.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, user.UserState);
            Assert.AreNotEqual(string.Empty, user.BanInfo.Path);
        }

        [TestMethod]
        public async Task BanAsync_Offline_Member_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var message = RandomUtility.NextString();
            var user = await userCollection.GetRandomUserAsync(Authority.Member, UserState.None, item => item.ID != authentication.ID && item.BanInfo.IsNotBanned);
            await user.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, user.UserState);
            Assert.AreNotEqual(string.Empty, user.BanInfo.Path);
        }

        [TestMethod]
        public async Task BanAsync_Online_Guest_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var message = RandomUtility.NextString();
            var user = await userCollection.GetRandomUserAsync(Authority.Guest, UserState.Online, item => item.ID != authentication.ID && item.BanInfo.IsNotBanned);
            await user.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, user.UserState);
            Assert.AreNotEqual(string.Empty, user.BanInfo.Path);
        }

        [TestMethod]
        public async Task BanAsync_Offline_Guest_TestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var message = RandomUtility.NextString();
            var user = await userCollection.GetRandomUserAsync(Authority.Guest, UserState.None, item => item.ID != authentication.ID && item.BanInfo.IsNotBanned);
            await user.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, user.UserState);
            Assert.AreNotEqual(string.Empty, user.BanInfo.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task BanAsync_Arg0_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(item => item.Authority == Authority.Member);
            var message = RandomUtility.NextString();
            await user.BanAsync(null, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task BanAsync_Arg1_Null_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Member);
            await user.BanAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task BanAsync_Arg1_Empty_FailTestAsync()
        {
            var authentication = await TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Member);
            await user.BanAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task BanAsync_Expired_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync(Authority.Member);
            var message = RandomUtility.NextString();
            await user.BanAsync(expiredAuthentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BanAsync_AlreadyBanned_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var message = RandomUtility.NextString();
            var user = await userCollection.GetRandomUserAsync(UserState.None, item => item.Authority != Authority.Admin && item.BanInfo.IsNotBanned);
            await user.BanAsync(authentication, message);
            await user.BanAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task BanAsync_PermissionDenied_Admin_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Admin, item => item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.BanAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task BanAsync_PermissionDenied_MemberAuthentication_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var user = await userCollection.GetRandomUserAsync(item => item.Authority != Authority.Admin && item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.BanAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task BanAsync_PermissionDenied_GuestAuthentication_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var user = await userCollection.GetRandomUserAsync(item => item.Authority != Authority.Admin && item.ID != authentication.ID);
            var message = RandomUtility.NextString();
            await user.BanAsync(authentication, message);
        }

        [TestMethod]
        public async Task UnbanAsync_Member_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Member, item => item.ID != authentication.ID && item.BanInfo.IsBanned);
            await user.UnbanAsync(authentication);
            Assert.AreEqual(string.Empty, user.BanInfo.Path);
        }

        [TestMethod]
        public async Task UnbanAsync_Guest_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Guest, item => item.ID != authentication.ID && item.BanInfo.IsBanned);
            await user.UnbanAsync(authentication);
            Assert.AreEqual(string.Empty, user.BanInfo.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UnbanAsync_Arg0_Null_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync(Authority.Member);
            await user.UnbanAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task UnbanAsync_Expired_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync(Authority.Member);
            await user.UnbanAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnbanAsync_Member_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var user = await userCollection.GetRandomUserAsync(Authority.Member, item => item.ID != authentication.ID);
            await user.UnbanAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnbanAsync_Guest_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Guest);
            var user = await userCollection.GetRandomUserAsync(Authority.Member);
            await user.UnbanAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnbanAsync_Unbanned_FailTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(Authority.Member, item => item.BanInfo.IsNotBanned);
            await user.UnbanAsync(authentication);
        }

        [TestMethod]
        public async Task ID_TestAsync()
        {
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            Assert.AreEqual(Authentication.AdminID, user.ID);
        }

        [TestMethod]
        public async Task UserName_TestAsync()
        {
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            Assert.AreEqual(Authentication.AdminName, user.UserName);
        }

        [TestMethod]
        public async Task Path_TestAsync()
        {
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            NameValidator.ValidateItemPath(user.Path);
        }

        [TestMethod]
        public async Task Authority_TestAsync()
        {
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            Assert.AreEqual(Authority.Admin, user.Authority);
        }

        [TestMethod]
        public async Task Category_TestAsync()
        {
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            Assert.IsNotNull(user.Category);
        }

        [TestMethod]
        public async Task UserInfo_TestAsync()
        {
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            Assert.AreEqual(Authentication.AdminID, user.UserInfo.ID);
            Assert.AreEqual(user.Path, user.UserInfo.Path);
            Assert.AreEqual(user.Category.Path.Trim(PathUtility.SeparatorChar), user.UserInfo.CategoryName);
            Assert.AreNotEqual(SignatureDate.Empty, user.UserInfo.CreationInfo);
            Assert.AreEqual(user.Authority, user.UserInfo.Authority);
        }

        [TestMethod]
        public async Task UserState_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            Assert.AreEqual(UserState.Online, user.UserState);
        }

        [TestMethod]
        public async Task BanInfo_TestAsync()
        {
            var user = await userCollection.GetUserAsync(Authentication.AdminID);
            Assert.AreEqual(string.Empty, user.BanInfo.Path);
            Assert.AreEqual(string.Empty, user.BanInfo.Comment);
            Assert.AreEqual(SignatureDate.Empty, user.BanInfo.SignatureDate);
        }

        [TestMethod]
        public async Task Renamed_TestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.Renamed += User_Renamed;
                user.Renamed -= User_Renamed;
            });

            void User_Renamed(object sender, EventArgs e)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Renamed_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            user.Renamed += (s, e) => { };
        }

        [TestMethod]
        public async Task Moved_TestAsync()
        {
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync();
            var oldCategory = user.Category;
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            var actualPath = string.Empty;
            var expectedPath = new ItemName(category.Path, user.ID).ToString();
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.Moved += User_Moved;
            });
            await user.MoveAsync(authentication, category.Path);
            Assert.AreEqual(expectedPath, actualPath);
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.Moved -= User_Moved;
            });
            await user.MoveAsync(authentication, oldCategory.Path);
            Assert.AreEqual(expectedPath, actualPath);

            void User_Moved(object sender, EventArgs e)
            {
                actualPath = user.Path;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Moved_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            user.Moved += (s, e) => { };
        }

        [TestMethod]
        public async Task Deleted_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.None, item => item.ID != Authentication.AdminID && item.ID != authentication.ID);
            var userID = user.ID;
            var actualID = string.Empty;
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.Deleted += User_Deleted;
            });
            await user.DeleteAsync(authentication);
            Assert.IsNull(user.Dispatcher);
            Assert.IsFalse(await userCollection.ContainsAsync(userID));
            Assert.AreEqual(userID, actualID);

            void User_Deleted(object sender, EventArgs e)
            {
                if (sender is IUser user)
                {
                    actualID = user.ID;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Deleted_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            user.Deleted += (s, e) => { };
        }

        [TestMethod]
        public async Task UserInfoChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync();
            var user = await userCollection.GetUserAsync(authentication.ID);
            var oldUserName = user.UserName;
            var actualUserName = string.Empty;
            var expectedUserName = RandomUtility.NextName();
            var password = user.GetPassword();
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.UserInfoChanged += User_UserInfoChanged;
            });
            await user.SetUserNameAsync(authentication, password, expectedUserName);
            Assert.AreEqual(expectedUserName, actualUserName);
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.UserInfoChanged -= User_UserInfoChanged;
            });
            await user.SetUserNameAsync(authentication, password, oldUserName);
            Assert.AreEqual(expectedUserName, actualUserName);

            void User_UserInfoChanged(object sender, EventArgs e)
            {
                if (sender is IUser user)
                {
                    actualUserName = user.UserName;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UserInfoChanged_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            user.UserInfoChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task UserStateChanged_TestAsync()
        {
            var user = await userCollection.GetRandomUserAsync(UserState.None, item => item.BanInfo.IsNotBanned == true);
            var actualUserState = UserState.None;
            var expectedUserState = UserState.Online;
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.UserStateChanged += User_UserStateChanged;
            });
            var authentication = await this.TestContext.LoginAsync(user.ID);
            Assert.AreEqual(expectedUserState, actualUserState);
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.UserStateChanged -= User_UserStateChanged;
            });
            await this.TestContext.LogoutAsync(authentication);
            Assert.AreEqual(expectedUserState, actualUserState);

            void User_UserStateChanged(object sender, EventArgs e)
            {
                if (sender is IUser user)
                {
                    actualUserState = user.UserState;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UserStateChanged_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            user.UserStateChanged += (s, e) => { };
        }

        [TestMethod]
        public async Task UserBanInfoChanged_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userCollection.GetRandomUserAsync(UserState.None, item => item.Authority != Authority.Admin && item.BanInfo.IsNotBanned);
            var actualPath = string.Empty;
            var actualComment = string.Empty;
            var expectedPath = user.Path;
            var expectedComment = RandomUtility.NextString();
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.UserBanInfoChanged += User_UserBanInfoChanged;
            });
            await user.BanAsync(authentication, expectedComment);
            Assert.AreEqual(expectedPath, actualPath);
            Assert.AreEqual(expectedComment, actualComment);
            await user.Dispatcher.InvokeAsync(() =>
            {
                user.UserBanInfoChanged -= User_UserBanInfoChanged;
            });
            await user.UnbanAsync(authentication);
            Assert.AreEqual(expectedPath, actualPath);
            Assert.AreEqual(expectedComment, actualComment);

            void User_UserBanInfoChanged(object sender, EventArgs e)
            {
                if (sender is IUser user)
                {
                    actualPath = user.BanInfo.Path;
                    actualComment = user.BanInfo.Comment;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UserBanInfoChanged_FailTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync();
            user.UserBanInfoChanged += (s, e) => { };
        }
    }
}

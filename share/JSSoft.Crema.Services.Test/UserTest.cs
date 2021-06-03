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
        private static IUserCollection userCollection;
        private static Authentication authentication;
        private static Authentication adminAuthentication;
        private static Authentication memberAuthentication;
        private static Authentication guestAuthentication;
        private static Authentication expiredAuthentication;
        private static IUser user;
        private static IUser adminUser;
        private static IUser memberUser;
        private static IUser guestUser;
        private static IUser otherUser;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(UserTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            adminAuthentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            memberAuthentication = await cremaHost.LoginRandomAsync(Authority.Member);
            guestAuthentication = await cremaHost.LoginRandomAsync(Authority.Guest);
            expiredAuthentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            user = await userCollection.Dispatcher.InvokeAsync(() => userCollection[authentication.ID]);
            adminUser = await userCollection.Dispatcher.InvokeAsync(() => userCollection[adminAuthentication.ID]);
            memberUser = await userCollection.Dispatcher.InvokeAsync(() => userCollection[memberAuthentication.ID]);
            guestUser = await userCollection.Dispatcher.InvokeAsync(() => userCollection[guestAuthentication.ID]);
            otherUser = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            await cremaHost.LogoutAsync(expiredAuthentication);
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.StopAsync(authentication);
            app.Release();
        }

        [TestMethod]
        public async Task MoveAsyncTestAsync()
        {
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            await user.MoveAsync(authentication, category.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsyncTestAsync_Null_Arg0_Fail()
        {
            await user.MoveAsync(null, "/");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsyncTestAsync_Null_Arg1_Fail()
        {
            await user.MoveAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsyncTestAsync_InvalidPath_Arg1_Fail()
        {
            var categoryPath = RandomUtility.NextInvalidCategoryPath();
            await user.MoveAsync(authentication, categoryPath);
        }

        [TestMethod]
        [ExpectedException(typeof(CategoryNotFoundException))]
        public async Task MoveAsyncTestAsync_NotExistsPath_Arg1_Fail()
        {
            var userCategoryColleciton = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryColleciton.GetRandomUserCategoryAsync(item => item.Path != user.Category.Path);
            var categoryPath = new CategoryName(category.Path, RandomUtility.NextName());
            await user.MoveAsync(authentication, categoryPath);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task MoveAsyncTestAsync_Expired_Fail()
        {
            await user.MoveAsync(expiredAuthentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsyncTestAsync_PermissionDenied_Member_Fail()
        {
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            await user.MoveAsync(memberAuthentication, category.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsyncTestAsync_PermissionDenied_Guest_Fail()
        {
            var userCategoryCollection = cremaHost.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
            var category = await userCategoryCollection.GetRandomUserCategoryAsync((item) => item != user.Category);
            await user.MoveAsync(guestAuthentication, category.Path);
        }

        [TestMethod]
        public async Task DeleteAsyncTestAsync()
        {
            var user = await userCollection.GetRandomUserAsync(item => Predicate(item, authentication));
            await user.DeleteAsync(authentication);

            static bool Predicate(IUser user, Authentication authentication)
            {
                if (user.ID == Authentication.AdminID)
                    return false;
                if (user.ID == authentication.ID)
                    return false;
                if (user.UserState == UserState.Online)
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsyncTestAsync_Null_Fail()
        {
            await user.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task DeleteAsyncTestAsync_Expired_Fail()
        {
            await user.DeleteAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_PermissionDenied_AdminID_Fail()
        {
            var admin = await userCollection.Dispatcher.InvokeAsync(() => userCollection[Authentication.AdminID]);
            await admin.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsyncTestAsync_PermissionDenied_Member_Fail()
        {
            await user.DeleteAsync(memberAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsyncTestAsync_PermissionDenied_Guest_Fail()
        {
            await user.DeleteAsync(guestAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsyncTestAsync_Online_Fail()
        {
            var user = await userCollection.GetRandomUserAsync(item => Predicate(item, authentication));
            await user.DeleteAsync(authentication);

            static bool Predicate(IUser user, Authentication authentication)
            {
                if (user.ID == Authentication.AdminID)
                    return false;
                if (user.ID == authentication.ID)
                    return false;
                if (user.UserState == UserState.None)
                    return false;
                return true;
            }
        }

        [TestMethod]
        public async Task SetUserNameAsyncTestAsync()
        {
            var authentication = await cremaHost.LoginRandomAsync();
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
        public async Task SetUserNameAsyncTestAsync_Null_Arg0_Fail()
        {
            var password = $"{user.Authority}".ToLower().ToSecureString();
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(null, password, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetUserNameAsyncTestAsync_Null_Arg1_Fail()
        {
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(authentication, null, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetUserNameAsyncTestAsync_WrongPassword_Arg1_Fail()
        {
            var password = user.GetNextPassword();
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(authentication, password, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetUserNameAsyncTestAsync_Null_Arg2_Fail()
        {
            var password = $"{user.Authority}".ToLower().ToSecureString();
            await user.SetUserNameAsync(authentication, password, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetUserNameAsyncTestAsync_Empty_Arg2_Fail()
        {
            var password = user.GetPassword();
            await user.SetUserNameAsync(authentication, password, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task SetUserNameAsyncTestAsync_Expired_Fail()
        {
            var password = user.GetPassword();
            var userName = RandomUtility.NextName();
            await user.SetUserNameAsync(expiredAuthentication, password, userName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetUserNameAsyncTestAsync_OtherUser_Fail()
        {
            var password = otherUser.GetPassword();
            var userName = RandomUtility.NextName();
            await otherUser.SetUserNameAsync(authentication, password, userName);
        }

        [TestMethod]
        public async Task SetPasswordAsyncTestAsync()
        {
            var user = await userCollection.Dispatcher.InvokeAsync(() => userCollection[authentication.ID]);
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
        public async Task SetPasswordAsyncTestAsync_Null_Arg0_Fail()
        {
            var password1 = user.GetPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(null, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetPasswordAsyncTestAsync_Null_Arg1_Fail()
        {
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(authentication, null, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SetPasswordAsyncTestAsync_Null_Arg2_Fail()
        {
            var password1 = user.GetPassword();
            await user.SetPasswordAsync(authentication, password1, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task SetPasswordAsyncTestAsync_Expired_Fail()
        {
            var user = await userCollection.Dispatcher.InvokeAsync(() => userCollection[expiredAuthentication.SignatureDate.ID]);
            var password1 = user.GetPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(expiredAuthentication, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetPasswordAsyncTestAsync_WrongPassword_Arg1_Fail()
        {
            var user = await userCollection.Dispatcher.InvokeAsync(() => userCollection[authentication.ID]);
            var password1 = user.GetNextPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(authentication, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SetPasswordAsyncTestAsync_SamePassword_Arg1_Fail()
        {
            var user = await userCollection.Dispatcher.InvokeAsync(() => userCollection[authentication.ID]);
            var password1 = user.GetPassword();
            var password2 = user.GetPassword();
            await user.SetPasswordAsync(authentication, password1, password2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetPasswordAsyncTestAsync_Other_Fail()
        {
            var user = await userCollection.GetRandomUserAsync(item => item.ID != authentication.ID);
            var password1 = user.GetPassword();
            var password2 = user.GetNextPassword();
            await user.SetPasswordAsync(authentication, password1, password2);
        }

        [TestMethod]
        public async Task ResetPasswordAsyncTestAsync()
        {
            await user.ResetPasswordAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ResetPasswordAsyncTestAsync_Null_Fail()
        {
            await user.ResetPasswordAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task ResetPasswordAsyncTestAsync_Expired_Fail()
        {
            await user.ResetPasswordAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task ResetPasswordAsyncTestAsync_Admin_Fail()
        {
            await user.ResetPasswordAsync(adminAuthentication);
        }

        [TestMethod]
        public async Task SendMessageAsyncTestAsync()
        {
            var message = RandomUtility.NextString();
            await user.SendMessageAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendMessageAsyncTestAsync_Null_Arg0_Fail()
        {
            var message = RandomUtility.NextString();
            await user.SendMessageAsync(null, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendMessageAsyncTestAsync_Null_Arg1_Fail()
        {
            await user.SendMessageAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SendMessageAsyncTestAsync_Empty_Arg1_Fail()
        {
            await user.SendMessageAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task SendMessageAsyncTestAsync_Expired_Fail()
        {
            await user.SendMessageAsync(expiredAuthentication, null);
        }

        [TestMethod]
        public async Task KickAsyncTestAsync_Admin()
        {
            var message = RandomUtility.NextString();
            var adminAuthentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            var adminUser = await userCollection.GetUserAsync(adminAuthentication.ID);
            await adminUser.KickAsync(authentication, message);
        }

        [TestMethod]
        public async Task KickAsyncTestAsync_Member()
        {
            var message = RandomUtility.NextString();
            var memberAuthentication = await cremaHost.LoginRandomAsync(Authority.Member);
            var memberUser = await userCollection.GetUserAsync(memberAuthentication.ID);
            await memberUser.KickAsync(authentication, message);
        }

        [TestMethod]
        public async Task KickAsyncTestAsync_Guest()
        {
            var message = RandomUtility.NextString();
            var guestAuthentication = await cremaHost.LoginRandomAsync(Authority.Guest);
            var guestUser = await userCollection.GetUserAsync(guestAuthentication.ID);
            await guestUser.KickAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task KickAsyncTestAsync_Null_Arg0_Fail()
        {
            var message = RandomUtility.NextString();
            await adminUser.KickAsync(null, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task KickAsyncTestAsync_Null_Arg1_Fail()
        {
            await adminUser.KickAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task KickAsyncTestAsync_Empty_Arg1_Fail()
        {
            await adminUser.KickAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task KickAsyncTestAsync_Expired_Fail()
        {
            var message = RandomUtility.NextString();
            await adminUser.KickAsync(expiredAuthentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task KickAsyncTestAsync_PermissionDenied_Member_Fail()
        {
            var message = RandomUtility.NextString();
            await user.KickAsync(memberAuthentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task KickAsyncTestAsync_PermissionDenied_Guest_Fail()
        {
            var message = RandomUtility.NextString();
            await user.KickAsync(guestAuthentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task KickAsyncTestAsync_Offline_Fail()
        {
            var message = RandomUtility.NextString();
            var user = await userCollection.GetRandomUserAsync(item => item.UserState == UserState.None);
            await user.KickAsync(authentication, message);
        }

        [TestMethod]
        public async Task BanAsyncTestAsync_Online_Member()
        {
            var message = RandomUtility.NextString();
            var memberAuthentication = await cremaHost.LoginRandomAsync(Authority.Member);
            var memberUser = await userCollection.GetUserAsync(memberAuthentication.ID);
            await memberUser.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, memberUser.UserState);
            Assert.AreNotEqual(string.Empty, memberUser.BanInfo.Path);
        }

        [TestMethod]
        public async Task BanAsyncTestAsync_Offline_Member()
        {
            var message = RandomUtility.NextString();
            var memberUser = await userCollection.GetRandomUserAsync(Predicate);
            await memberUser.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, memberUser.UserState);
            Assert.AreNotEqual(string.Empty, memberUser.BanInfo.Path);

            static bool Predicate(IUser user)
            {
                if (user.UserState == UserState.Online)
                    return false;
                if (user.Authority != Authority.Member)
                    return false;
                if (user.BanInfo.Path != string.Empty)
                    return false;
                return true;
            }
        }

        [TestMethod]
        public async Task BanAsyncTestAsync_Online_Guest()
        {
            var message = RandomUtility.NextString();
            var guestAuthentication = await cremaHost.LoginRandomAsync(Authority.Guest);
            var guestUser = await userCollection.GetUserAsync(guestAuthentication.ID);
            await guestUser.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, guestUser.UserState);
            Assert.AreNotEqual(string.Empty, guestUser.BanInfo.Path);
        }

        [TestMethod]
        public async Task BanAsyncTestAsync_Offline_Guest()
        {
            var message = RandomUtility.NextString();
            var guestUser = await userCollection.GetRandomUserAsync(Predicate);
            await guestUser.BanAsync(authentication, message);
            Assert.AreEqual(UserState.None, guestUser.UserState);
            Assert.AreNotEqual(string.Empty, guestUser.BanInfo.Path);

            static bool Predicate(IUser user)
            {
                if (user.UserState == UserState.Online)
                    return false;
                if (user.Authority != Authority.Guest)
                    return false;
                if (user.BanInfo.Path != string.Empty)
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task BanAsyncTestAsync_Null_Arg0_Fail()
        {
            var message = RandomUtility.NextString();
            await memberUser.BanAsync(null, message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task BanAsyncTestAsync_Null_Arg1_Fail()
        {
            await memberUser.BanAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task BanAsyncTestAsync_Empty_Arg1_Fail()
        {
            await memberUser.BanAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task BanAsyncTestAsync_Expired_Fail()
        {
            var message = RandomUtility.NextString();
            await memberUser.BanAsync(expiredAuthentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BanAsyncTestAsync_AlreadyBanned_Fail()
        {
            var message = RandomUtility.NextString();
            var memberUser = await userCollection.GetRandomUserAsync(Predicate);
            await memberUser.BanAsync(authentication, message);
            await memberUser.BanAsync(authentication, message);

            static bool Predicate(IUser user)
            {
                if (user.BanInfo.IsBanned == true)
                    return false;
                if (user.UserState == UserState.Online)
                    return false;
                if (user.Authority == Authority.Admin)
                    return false;
                return true;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task BanAsyncTestAsync_PermissionDenied_Admin_Fail()
        {
            var message = RandomUtility.NextString();
            await adminUser.BanAsync(authentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task BanAsyncTestAsync_PermissionDenied_Member_Fail()
        {
            var message = RandomUtility.NextString();
            await memberUser.BanAsync(memberAuthentication, message);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task BanAsyncTestAsync_PermissionDenied_Guest_Fail()
        {
            var message = RandomUtility.NextString();
            await memberUser.BanAsync(guestAuthentication, message);
        }

        [TestMethod]
        public async Task UnbanAsyncTestAsync_Member()
        {
            var message = RandomUtility.NextString();
            var memberAuthentication = await cremaHost.LoginRandomAsync(Authority.Member);
            var memberUser = await userCollection.GetUserAsync(memberAuthentication.ID);
            await memberUser.BanAsync(authentication, message);
            await memberUser.UnbanAsync(authentication);
            Assert.AreEqual(string.Empty, memberUser.BanInfo.Path);
        }

        [TestMethod]
        public async Task UnbanAsyncTestAsync_Guest()
        {
            var message = RandomUtility.NextString();
            var guestAuthentication = await cremaHost.LoginRandomAsync(Authority.Guest);
            var guestUser = await userCollection.GetUserAsync(guestAuthentication.ID);
            await guestUser.BanAsync(authentication, message);
            await guestUser.UnbanAsync(authentication);
            Assert.AreEqual(string.Empty, guestUser.BanInfo.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UnbanAsyncTestAsync_Null_Arg0_Fail()
        {
            await memberUser.UnbanAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task UnbanAsyncTestAsync_Expired_Fail()
        {
            await memberUser.UnbanAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnbanAsyncTestAsync_Member_Fail()
        {
            await memberUser.UnbanAsync(memberAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task UnbanAsyncTestAsync_Guest_Fail()
        {
            await memberUser.UnbanAsync(guestAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UnbanAsyncTestAsync_Unbanned_Fail()
        {
            await memberUser.UnbanAsync(authentication);
        }

        [TestMethod]
        public void IDTest()
        {
            Assert.AreEqual(Authentication.AdminID, user.ID);
        }

        [TestMethod]
        public void UserNameTest()
        {
            Assert.AreEqual(Authentication.AdminName, user.UserName);
        }

        [TestMethod]
        public void PathTest()
        {
            NameValidator.ValidateItemPath(user.Path);
        }

        [TestMethod]
        public void AuthorityTest()
        {
            Assert.AreEqual(Authority.Admin, user.Authority);
        }

        [TestMethod]
        public void CategoryTest()
        {
            Assert.IsNotNull(user.Category);
        }

        [TestMethod]
        public void UserInfoTest()
        {
            Assert.AreEqual(Authentication.AdminID, user.UserInfo.ID);
            Assert.AreEqual(user.Path, user.UserInfo.Path);
            Assert.AreEqual(user.Category.Path.Trim(PathUtility.SeparatorChar), user.UserInfo.CategoryName);
            Assert.AreNotEqual(SignatureDate.Empty, user.UserInfo.CreationInfo);
            Assert.AreEqual(user.Authority, user.UserInfo.Authority);
        }

        [TestMethod]
        public void UserStateTest()
        {
            Assert.AreEqual(UserState.Online, user.UserState);
        }

        [TestMethod]
        public void BanInfoTest()
        {
            Assert.AreEqual(string.Empty, user.BanInfo.Path);
            Assert.AreEqual(string.Empty, user.BanInfo.Comment);
            Assert.AreEqual(SignatureDate.Empty, user.BanInfo.SignatureDate);
        }

        [TestMethod]
        public void RenamedTest()
        {
            user.Dispatcher.Invoke(() =>
            {
                user.Renamed += User_Renamed;
                user.Renamed -= User_Renamed;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RenamedTest_Fail()
        {
            user.Renamed += User_Renamed;
        }

        [TestMethod]
        public void MovedTest()
        {
            user.Dispatcher.Invoke(() =>
            {
                user.Moved += User_Moved;
                user.Moved -= User_Moved;
            });
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MovedTest_Fail()
        {
            user.Moved += User_Moved;
        }

        private void User_Moved(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void DeletedTest()
        {
            user.Dispatcher.Invoke(() =>
            {
                user.Deleted += User_Deleted;
                user.Deleted -= User_Deleted;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeletedTest_Fail()
        {
            user.Deleted += User_Deleted;
        }

        [TestMethod]
        public void UserInfoChangedTest()
        {
            user.Dispatcher.Invoke(() =>
            {
                user.UserInfoChanged += User_UserInfoChanged;
                user.UserInfoChanged -= User_UserInfoChanged;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UserInfoChangedTest_Fail()
        {
            user.UserInfoChanged += User_UserInfoChanged;
        }

        [TestMethod]
        public void UserStateChangedTest()
        {
            user.Dispatcher.Invoke(() =>
            {
                user.UserStateChanged += User_UserStateChanged;
                user.UserStateChanged -= User_UserStateChanged;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UserStateChangedTest_Fail()
        {
            user.UserStateChanged += User_UserStateChanged;
        }

        [TestMethod]
        public void UserBanInfoChangedTest()
        {
            user.Dispatcher.Invoke(() =>
            {
                user.UserBanInfoChanged += User_UserBanInfoChanged;
                user.UserBanInfoChanged -= User_UserBanInfoChanged;
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UserBanInfoChangedTest_Fail()
        {
            user.UserBanInfoChanged += User_UserBanInfoChanged;
        }

        private void User_Renamed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void User_Deleted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void User_UserInfoChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void User_UserStateChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void User_UserBanInfoChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
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

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class UserItemTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Guid token;
        private static Authentication expiredAuthentication;
        private static IUserContext userContext;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context);
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            token = await cremaHost.OpenAsync();
            userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            expiredAuthentication = await cremaHost.LoginRandomAsync(Authority.Admin);
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
        [ExpectedException(typeof(NotImplementedException))]
        public async Task RenameAsync_User_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUser));
            await userItem.RenameAsync(authentication, RandomUtility.NextName());
        }

        [TestMethod]
        public async Task RenameAsync_Category_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUserCategory), item => item.Parent != null);
            var name = RandomUtility.NextName();
            await userItem.RenameAsync(authentication, name);
            Assert.AreEqual(name, userItem.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsync_Arg0_Null_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.RenameAsync(null, RandomUtility.NextName());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RenameAsync_Arg1_Null_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.RenameAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task RenameAsync_Expired_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            var name = RandomUtility.NextName();
            await userItem.RenameAsync(expiredAuthentication, name);
        }

        [TestMethod]
        public async Task MoveAsync_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var rootItem = userContext.Root;
            var userItem = await userContext.GetRandomUserItemAsync(item => item.Parent != null && item.Parent != rootItem);
            await userItem.MoveAsync(authentication, rootItem.Path);
            Assert.AreEqual(rootItem, userItem.Parent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsync_Arg0_Null_TestFailAsync()
        {
            var rootItem = userContext.Root;
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.MoveAsync(null, rootItem.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MoveAsync_Arg1_Null_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.MoveAsync(authentication, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsync_Arg1_Empty_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var rootItem = userContext.Root;
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.MoveAsync(authentication, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(CategoryNotFoundException))]
        public async Task MoveAsync_CategoryNotFound_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var rootItem = userContext.Root;
            var userItem = await userContext.GetRandomUserItemAsync();
            var category = await userContext.GetRandomUserCategoryAsync(item => item != userItem.Parent);
            var name = await category.GenerateNewCategoryNameAsync("folder");
            var categoryName = new CategoryName(category.Path, name);
            await userItem.MoveAsync(authentication, categoryName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task MoveAsync_SameParent_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync();
            var categoryPath = userItem.Parent.Path;
            await userItem.MoveAsync(authentication, categoryPath);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task MoveAsync_Expired_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            var rootItem = userContext.Root;
            await userItem.MoveAsync(expiredAuthentication, rootItem.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task MoveAsync_PermissionDenied_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userItem = await userContext.GetRandomUserItemAsync(item => item != userContext.Root);
            var rootItem = userContext.Root;
            await userItem.MoveAsync(authentication, rootItem.Path);
        }

        [TestMethod]
        public async Task DeleteAsync_Item_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userContext.GetRandomUserAsync(UserState.None, item => item.ID != authentication.ID && item.ID != Authentication.AdminID);
            var userItem = user as IUserItem;
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        public async Task DeleteAsync_Category_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUserCategory), item => item.Childs.Any() == false);
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_Arg0_Null_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationExpiredException))]
        public async Task DeleteAsync_Expired_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            await userItem.DeleteAsync(expiredAuthentication);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionDeniedException))]
        public async Task DeleteAsync_PermissionDenied_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUser));
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_HasChild_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUserCategory), item => item.Childs.Any() == true);
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_Self_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userContext.GetUserAsync(authentication.ID);
            var userItem = user as IUserItem;
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_Admin_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userContext.GetUserAsync(Authentication.AdminID);
            var userItem = user as IUserItem;
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync_Online_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userContext.GetRandomUserAsync(UserState.Online, item => item.ID != authentication.ID && item.ID != Authentication.AdminID);
            var userItem = user as IUserItem;
            await userItem.DeleteAsync(authentication);
        }

        [TestMethod]
        public async Task Name_User_TestAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUser));
            var condition = IdentifierValidator.Verify(userItem.Name);
            Assert.IsTrue(condition);
        }

        [TestMethod]
        public async Task Name_UserCategory_TestAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUserCategory));
            var condition = NameValidator.VerifyName(userItem.Name);
            Assert.IsTrue(condition);
        }

        [TestMethod]
        public async Task Path_TestAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            var condition = NameValidator.VerifyPath(userItem.Path);
            Assert.IsTrue(condition);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Child_Dispatcher_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            Assert.Fail($"{userItem.Childs.Any()}");
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public async Task Renamed_User_TestFailAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUser));
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Renamed += UserItem_Renamed;
                userItem.Renamed -= UserItem_Renamed;
            });
            await userItem.RenameAsync(authentication, RandomUtility.NextName());

            void UserItem_Renamed(object sender, EventArgs e)
            {
                throw new NotSupportedException();
            }
        }

        [TestMethod]
        public async Task Renamed_UserCategory_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUserCategory), item => item.Parent != null);
            var oldName = userItem.Name;
            var expectedName = RandomUtility.NextName();
            var actualName = string.Empty;
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Renamed += UserItem_Renamed;
            });
            await userItem.RenameAsync(authentication, expectedName);
            Assert.AreEqual(expectedName, actualName);
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Renamed -= UserItem_Renamed;
            });
            await userItem.RenameAsync(authentication, oldName);
            Assert.AreEqual(expectedName, actualName);

            void UserItem_Renamed(object sender, EventArgs e)
            {
                if (sender is IUserCategory category)
                {
                    actualName = category.Name;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Renamed_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            userItem.Renamed += (s, e) => { };
        }

        [TestMethod]
        public async Task Moved_User_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUser));
            var category = await userContext.GetRandomUserCategoryAsync(item => item != userItem.Parent);
            var oldParentPath = userItem.Parent.Path;
            var expectedParentPath = category.Path;
            var actualParentPath = string.Empty;
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Moved += UserItem_Moved;
            });
            await userItem.MoveAsync(authentication, expectedParentPath);
            Assert.AreEqual(expectedParentPath, actualParentPath);
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Moved -= UserItem_Moved;
            });
            await userItem.MoveAsync(authentication, oldParentPath);
            Assert.AreEqual(expectedParentPath, actualParentPath);

            void UserItem_Moved(object sender, EventArgs e)
            {
                if (sender is IUserItem userItem)
                {
                    actualParentPath = userItem.Parent.Path;
                }
            }
        }

        [TestMethod]
        public async Task Moved_UserCategory_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var userItem = await userContext.GetRandomUserItemAsync(typeof(IUserCategory), item => item.Parent != null);
            var parentItem = await userContext.GetRandomUserItemAsync(typeof(IUserCategory), item => item.Path.StartsWith(userItem.Path) == false);
            var oldParentPath = userItem.Parent.Path;
            var expectedParentPath = parentItem.Path;
            var actualParentPath = string.Empty;
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Moved += UserItem_Moved;
            });
            await userItem.MoveAsync(authentication, expectedParentPath);
            Assert.AreEqual(expectedParentPath, actualParentPath);
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Moved -= UserItem_Moved;
            });
            await userItem.MoveAsync(authentication, oldParentPath);
            Assert.AreEqual(expectedParentPath, actualParentPath);

            void UserItem_Moved(object sender, EventArgs e)
            {
                if (sender is IUserItem userItem)
                {
                    actualParentPath = userItem.Parent.Path;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Moved_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            userItem.Moved += (s, e) => { };
        }

        [TestMethod]
        public async Task Deleted_User_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userContext.GetRandomUserAsync(Authority.Member, UserState.None);
            var userItem = user as IUserItem;
            var expectedUserItem = userItem;
            var actualUserItem = null as IUserItem;
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Deleted += UserItem_Deleted;
            });
            await userItem.DeleteAsync(authentication);
            Assert.AreEqual(expectedUserItem, actualUserItem);

            void UserItem_Deleted(object sender, EventArgs e)
            {
                if (sender is IUserItem userItem)
                {
                    actualUserItem = userItem;
                }
            }
        }

        [TestMethod]
        public async Task Deleted_UserCategory_TestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Admin);
            var user = await userContext.GetRandomUserItemAsync(typeof(IUserCategory), item => item.Parent != null && item.Childs.Any() == false);
            var userItem = user as IUserItem;
            var expectedUserItem = userItem;
            var actualUserItem = null as IUserItem;
            await userItem.Dispatcher.InvokeAsync(() =>
            {
                userItem.Deleted += UserItem_Deleted;
            });
            await userItem.DeleteAsync(authentication);
            Assert.AreEqual(expectedUserItem, actualUserItem);

            void UserItem_Deleted(object sender, EventArgs e)
            {
                if (sender is IUserItem userItem)
                {
                    actualUserItem = userItem;
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Deleted_TestFailAsync()
        {
            var userItem = await userContext.GetRandomUserItemAsync();
            userItem.Deleted += (s, e) => { };
        }
    }
}

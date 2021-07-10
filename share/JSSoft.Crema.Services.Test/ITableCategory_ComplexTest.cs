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

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class ITableCategory_ComplexTest
    {
        private static TestApplication app;
        private static IDataBaseContext dataBaseContext;
        private static IDataBase dataBase;
        private static ITableCategory category;
        private static Authentication authentication;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new TestApplication();
            await app.InitializeAsync(context);
            await app.OpenAsync();
            authentication = await app.LoginRandomAsync(Authority.Admin);
            dataBaseContext = app.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseFlags.NotLoaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.InitializeRandomItemsAsync(authentication, DataBaseSettings.Default);
            category = await dataBase.GetRandomTableCategoryAsync(item => item.Parent != null);
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await dataBase.UnloadAsync(authentication);
            await app.LogoutAsync(authentication);
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
        public async Task CategoryLockRenameTestAsync()
        {
            var newName = RandomUtility.NextIdentifier();
            var parentPath = category.Parent.Path;
            var categoryPath = new CategoryName(parentPath, newName);
            await category.LockAsync(authentication, string.Empty);
            await category.RenameAsync(authentication, newName);

            Assert.AreEqual(categoryPath, category.Path);
            Assert.AreEqual(newName, category.Name);
        }
    }
}

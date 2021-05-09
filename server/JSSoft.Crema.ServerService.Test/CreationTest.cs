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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using JSSoft.Library.IO;
using JSSoft.Library;
using System.Linq;
using JSSoft.Library.Random;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.ServiceModel;
using System.Threading.Tasks;
using System.Text;

namespace JSSoft.Crema.ServerService.Test
{
    [TestClass]
    public class CreationTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static string tempDir;
        private static Guid token;
        private static IDataBase dataBase;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(context.TestDir));
            tempDir = Path.Combine(solutionDir, ".test", "unit_test");
            app = new();
            CremaBootstrapper.CreateRepository(app, tempDir, "git", "xml");
            app.BasePath = tempDir;
        }

        [TestInitialize()]
        public async Task InitializeAsync()
        {
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            token = await cremaHost.OpenAsync();
            var authentication = await cremaHost.LoginAdminAsync();
            var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            dataBase = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.First());
            await dataBase.LoadAsync(authentication);
            await cremaHost.LogoutAsync(authentication);
        }

        [TestCleanup()]
        public async Task CleanupAsync()
        {
            if (dataBase != null)
            {
                var authentication = await cremaHost.LoginAdminAsync();
                await dataBase.UnloadAsync(authentication);
                await cremaHost.LogoutAsync(authentication);
            }
            await cremaHost.CloseAsync(token);
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            app.Dispose();
            DirectoryUtility.Delete(tempDir);
        }

        [TestMethod]
        public async Task BaseTestAsync()
        {
            var authentication = await cremaHost.LoginRandomAsync(Authority.Member);

            await dataBase.EnterAsync(authentication);
            try
            {
                var tableContext = dataBase.TableContext;
                var category = await tableContext.Root.AddNewCategoryAsync(authentication, "Folder1");
                var category1 = await category.AddNewCategoryAsync(authentication, "Folder1");

                var template = await category1.NewTableAsync(authentication);
                await template.AddRandomColumnsAsync(authentication);
                await template.EndEditAsync(authentication);

                var table = (template.Target as ITable[]).First();

                var childTemplate1 = await table.NewTableAsync(authentication);
                await childTemplate1.EndEditAsync(authentication);

                var child = await category1.Dispatcher.InvokeAsync(() => table.Childs[childTemplate1.TableName]);
                await child.RenameAsync(authentication, "Child_abc");

                var childTemplate2 = await table.NewTableAsync(authentication);
                await childTemplate2.EndEditAsync(authentication);
            }
            finally
            {
                await dataBase.LeaveAsync(authentication);
            }
        }

        [TestMethod]
        public async Task GenerateStandardTestAsync()
        {
            var userContext = cremaHost.GetService(typeof(IUserContext)) as IUserContext;
            var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var user = userContext.Users.Random(item => item.Authority != Authority.Guest);
            var password = StringUtility.ToSecureString(user.Authority.ToString().ToLower());
            var authenticationToken = await cremaHost.LoginAsync(user.ID, password);
            var authentication = await cremaHost.AuthenticateAsync(authenticationToken);
            var dataBase = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.First());
            var tableContext = dataBase.TableContext;

            var time = DateTime.Now;

            var transaction = await dataBase.BeginTransactionAsync(authentication);
            await dataBase.GenerateStandardAsync(authentication);

            var diff = DateTime.Now - time;

            await transaction.CommitAsync(authentication);
        }

        public TestContext TestContext { get; set; }
    }
}
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
using JSSoft.Crema.Services.Test.Extensions;
using JSSoft.Crema.Services.Extensions;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class CreationTest
    {
        private static TestApplication app;
        private static TestServerConfigurator configurator;
        private static IDataBaseContext dataBaseContext;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new();
            configurator = new(app);
            await app.InitializeAsync(context);
            await app.OpenAsync();
            await configurator.GenerateDataBasesAsync(4);
            dataBaseContext = app.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
        }

        [TestInitialize]
        public async Task InitializeAsync()
        {
            await this.TestContext.InitializeAsync(app);
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await this.TestContext.ReleaseAsync();
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await app.CloseAsync();
            await app.ReleaseAsync();
        }

        [TestMethod]
        public async Task BaseTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);

            await dataBase.EnterAsync(authentication);
            try
            {
                var tableContext = dataBase.GetService(typeof(ITableContext)) as ITableContext;
                var category = await tableContext.Root.AddNewCategoryAsync(authentication);
                var category1 = await category.AddNewCategoryAsync(authentication);

                var template = await category1.NewTableAsync(authentication);
                await template.AddRandomColumnsAsync(authentication);
                await template.EndEditAsync(authentication);

                var table = (template.Target as ITable[]).First();

                var childTemplate1 = await table.NewTableAsync(authentication);
                await childTemplate1.AddRandomColumnsAsync(authentication);
                await childTemplate1.EndEditAsync(authentication);

                var child = (childTemplate1.Target as ITable[]).First();
                var childName = RandomUtility.NextName();
                await child.RenameAsync(authentication, childName);

                var childTemplate2 = await table.NewTableAsync(authentication);
                await childTemplate2.AddRandomColumnsAsync(authentication);
                await childTemplate2.EndEditAsync(authentication);
            }
            finally
            {
                await dataBase.LeaveAsync(authentication);
            }
        }

        //[TestMethod]
        public async Task GenerateStandardTestAsync()
        {
            var authentication = await this.TestContext.LoginRandomAsync(Authority.Member);
            var dataBase = await dataBaseContext.GetRandomDataBaseAsync(DataBaseFlags.Loaded | DataBaseFlags.Public | DataBaseFlags.NotLocked);
            await dataBase.EnterAsync(authentication);
            try
            {
                var transaction = await dataBase.BeginTransactionAsync(authentication);
                await dataBase.GenerateStandardAsync(authentication);
                await transaction.CommitAsync(authentication);
            }
            finally
            {
                await dataBase.LeaveAsync(authentication);
            }
        }

        public TestContext TestContext { get; set; }
    }
}
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
using System.Linq;
using JSSoft.Crema.Services;
using JSSoft.Library.Random;
using System.Collections.Generic;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using JSSoft.Crema.ServiceModel;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.ServerService.Test
{
    [TestClass]
    public class PermissionTest
    {
        private static ICremaHost cremaHost;
        private readonly static OnlineUserCollection users = new OnlineUserCollection();

        private TestContext testContext;

        private Authentication admin;
        private Authentication member;
        private Authentication someone;
        private Authentication guest;
        private IDataBase dataBase;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(context.TestDir));
            var tempDir = Path.Combine(solutionDir, "crema_repo", "permission_test", typeof(PermissionTest).Name);
            var empty = DirectoryUtility.Exists(tempDir) == false || DirectoryUtility.IsEmpty(tempDir);

            cremaHost = TestCrema.GetInstance(tempDir);
            cremaHost.Open();

            if (empty == true)
            {
                CremaSimpleGenerator.Generate(cremaHost, 10);
            }

            users.Initialize(cremaHost);
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            cremaHost.Dispatcher.Invoke(() => cremaHost.Close());
            cremaHost.Dispose();
        }

        [TestInitialize()]
        public void Initialize()
        {
            this.dataBase = cremaHost.PrimaryDataBase;
            this.admin = users.RandomAuthentication(Authority.Admin);
            this.member = users.RandomAuthentication(Authority.Member);
            this.someone = users.RandomAuthentication(Authority.Member);
            this.guest = users.RandomAuthentication(Authority.Guest);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            this.admin = null;
            this.guest = null;
            this.member = null;
            this.someone = null;
            this.dataBase = null;
        }

        [TestMethod]
        public async Task TableFailTestAsaync()
        {
            var tableContext = this.dataBase.TableContext;
            var table = tableContext.Tables.RandomSample();
            var child = table.Childs.Random();
            var category = EnumerableUtility.Ancestors(table as IItem).Random(item => item.Parent != null) as ITableCategory;

            await this.InvokeFailAsync<PermissionException>(table.SetPublicAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(table.SetPrivateAsync(this.guest));
            await this.InvokeFailAsync<PermissionException>(table.AddAccessMemberAsync(this.guest, this.someone.UserID, AccessType.ReadWrite));
            await this.InvokeFailAsync<PermissionException>(table.RemoveAccessMemberAsync(this.guest, this.someone.UserID));
            await this.InvokeFailAsync<PermissionException>(table.LockAsync(this.guest, RandomUtility.NextString()));
            await this.InvokeFailAsync<PermissionException>(table.UnlockAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(table.RenameTestAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(table.MoveTestAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(table.DeleteAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(table.NewTableAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(table.RevertAsync(this.guest, 0));

            await this.InvokeFailAsync<CremaException>(category.SetPublicAsync(this.guest));
            await this.InvokeFailAsync<CremaException>(category.SetPrivateAsync(this.guest));
            await this.InvokeFailAsync<CremaException>(category.AddAccessMemberAsync(this.guest, this.someone.UserID, AccessType.ReadWrite));
            await this.InvokeFailAsync<CremaException>(category.RemoveAccessMemberAsync(this.guest, this.someone.UserID));
            await this.InvokeFailAsync<CremaException>(category.LockAsync(this.guest, RandomUtility.NextString()));
            await this.InvokeFailAsync<CremaException>(category.UnlockAsync(this.guest));
            await this.InvokeFailAsync<CremaException>(category.RenameTestAsync(this.guest));
            await this.InvokeFailAsync<CremaException>(category.MoveTestAsync(this.guest));
            await this.InvokeFailAsync<CremaException>(category.DeleteAsync(this.guest));

            await this.InvokeFailAsync<PermissionException>(child.SetPublicAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(child.SetPrivateAsync(this.guest));
            await this.InvokeFailAsync<PermissionException>(child.AddAccessMemberAsync(this.guest, this.someone.UserID, AccessType.ReadWrite));
            await this.InvokeFailAsync<PermissionException>(child.RemoveAccessMemberAsync(this.guest, this.someone.UserID));
            await this.InvokeFailAsync<PermissionException>(child.LockAsync(this.guest, RandomUtility.NextString()));
            await this.InvokeFailAsync<PermissionException>(child.UnlockAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(child.RenameTestAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(child.MoveTestAsync(this.guest));
            await this.InvokeFailAsync<PermissionDeniedException>(child.DeleteAsync(this.guest));
        }

        [TestMethod]
        public async Task LockedTableFailTestAsync()
        {
            var tableContext = this.dataBase.TableContext;
            var table = tableContext.Tables.RandomSample();
            var child = table.Childs.Random();
            var category = EnumerableUtility.Ancestors(table as IItem).Random(item => item.Parent != null) as ITableCategory;

            await table.LockAsync(this.admin, string.Empty);

            try
            {
                await this.InvokeFailAsync<PermissionException>(table.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(table.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(table.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionException>(table.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionDeniedException>(table.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.NewTableAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.RevertAsync(this.member, 0));

                await this.InvokeFailAsync<CremaException>(category.SetPublicAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<CremaException>(category.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<CremaException>(category.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<CremaException>(category.UnlockAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.RenameTestAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.MoveTestAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.DeleteAsync(this.member));

                await this.InvokeFailAsync<PermissionException>(child.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(child.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(child.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionException>(child.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionException>(child.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.DeleteAsync(this.member));
            }
            finally
            {
                await table.UnlockAsync(this.admin);
            }
        }

        [TestMethod]
        public async Task PrivateTableFailTestAsync()
        {
            var tableContext = this.dataBase.TableContext;
            var table = tableContext.Tables.RandomSample();
            var child = table.Childs.Random();
            var category = EnumerableUtility.Ancestors(table as IItem).Random(item => item.Parent != null) as ITableCategory;

            await table.SetPrivateAsync(this.admin);

            try
            {
                await this.InvokeFailAsync<PermissionDeniedException>(table.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(table.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionDeniedException>(table.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(table.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionException>(table.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.NewTableAsync(this.member));

                //await this.InvokeFailAsync<CremaException>(category.SetPublicAsync(this.member));
                //await this.InvokeFailAsync<CremaException>(category.SetPrivateAsync(this.member));
                //await this.InvokeFailAsync<CremaException>(category.AddAccessMemberAsync(this.member, this.someone.UserID, ItemAccess.ReadWrite));
                //await this.InvokeFailAsync<CremaException>(category.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                //await this.InvokeFailAsync<CremaException>(category.Lock(this.member, RandomUtility.NextStringAsync()));
                //await this.InvokeFailAsync<CremaException>(category.UnlockAsync(this.member));
                //await this.InvokeFailAsync<CremaException>(category.RenameTestAsync(this.member));
                //await this.InvokeFailAsync<CremaException>(category.MoveTestAsync(this.member));
                //await this.InvokeFailAsync<CremaException>(category.DeleteAsync(this.member));

                //await this.InvokeFailAsync<PermissionException>(child.SetPublicAsync(this.member));
                //await this.InvokeFailAsync<PermissionDeniedException>(child.SetPrivateAsync(this.member));
                //await this.InvokeFailAsync<PermissionException>(child.AddAccessMemberAsync(this.member, this.someone.UserID, ItemAccess.ReadWrite));
                //await this.InvokeFailAsync<PermissionException>(child.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                //await this.InvokeFailAsync<PermissionException>(child.Lock(this.member, RandomUtility.NextStringAsync()));
                //await this.InvokeFailAsync<PermissionException>(child.UnlockAsync(this.member));
                //await this.InvokeFailAsync<PermissionDeniedException>(child.RenameTestAsync(this.member));
                //await this.InvokeFailAsync<PermissionDeniedException>(child.MoveTestAsync(this.member));
                //await this.InvokeFailAsync<PermissionDeniedException>(child.DeleteAsync(this.member));
            }
            finally
            {
                await table.SetPublicAsync(this.admin);
            }
        }

        [TestMethod]
        public async Task LockedTableCategoryFailTestAsync()
        {
            var tableContext = this.dataBase.TableContext;
            var category = tableContext.Categories.RandomSample();
            var table = category.Tables.Random(item => item.TemplatedParent == null && item.Childs.Any());
            var child = table.Childs.Random();

            await category.LockAsync(this.admin, string.Empty);

            try
            {
                await this.InvokeFailAsync<PermissionException>(category.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(category.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(category.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionException>(category.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionDeniedException>(category.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.AddNewCategoryAsync(this.member));

                await this.InvokeFailAsync<PermissionException>(table.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(table.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(table.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(table.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionException>(table.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.NewTableAsync(this.member));

                await this.InvokeFailAsync<PermissionException>(child.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(child.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(child.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(child.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionException>(child.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.DeleteAsync(this.member));
            }
            finally
            {
                category.Unlock(this.admin);
            }
        }

        [TestMethod]
        public async Task PrivateTableCategoryFailTestAsync()
        {
            var tableContext = this.dataBase.TableContext;
            var category = tableContext.Categories.RandomSample();
            var table = category.Tables.Random(item => item.TemplatedParent == null && item.Childs.Any());
            var child = table.Childs.Random();

            await category.SetPrivateAsync(this.admin);

            try
            {
                await this.InvokeFailAsync<PermissionDeniedException>(category.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(category.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(category.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(category.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(category.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionException>(category.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.AddNewCategoryAsync(this.member));

                await this.InvokeFailAsync<PermissionException>(table.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(table.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(table.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(table.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionException>(table.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(table.NewTableAsync(this.member));

                await this.InvokeFailAsync<PermissionException>(child.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(child.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(child.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(child.Lock(this.member, RandomUtility.NextStringAsync()));
                await this.InvokeFailAsync<PermissionException>(child.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(child.DeleteAsync(this.member));
            }
            finally
            {
                category.SetPublic(this.admin);
            }
        }

        [TestMethod]
        public async Task LockedTypeFailTestAsync()
        {
            var typeContext = this.dataBase.TypeContext;
            var type = typeContext.Types.RandomSample();
            var category = EnumerableUtility.Ancestors(type as IItem).Random(item => item.Parent != null) as ITypeCategory;

            await type.LockAsync(this.admin, string.Empty);

            try
            {
                await this.InvokeFailAsync<PermissionException>(type.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(type.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(type.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionException>(type.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<PermissionDeniedException>(type.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.DeleteAsync(this.member));

                await this.InvokeFailAsync<CremaException>(category.SetPublicAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<CremaException>(category.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<CremaException>(category.LockAsync(this.member, RandomUtility.NextString()));
                await this.InvokeFailAsync<CremaException>(category.UnlockAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.RenameTestAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.MoveTestAsync(this.member));
                await this.InvokeFailAsync<CremaException>(category.DeleteAsync(this.member));
            }
            finally
            {
                await type.UnlockAsync(this.admin);
            }
        }

        [TestMethod]
        public void PrivateTypeFailTest()
        {
            var typeContext = this.dataBase.TypeContext;
            var type = typeContext.Types.RandomSample();
            var category = EnumerableUtility.Ancestors(type as IItem).Random(item => item.Parent != null) as ITypeCategory;

            type.SetPrivate(this.admin);

            try
            {
                await this.InvokeFailAsync<PermissionDeniedException>(type.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(type.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionDeniedException>(type.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(type.Lock(this.member, RandomUtility.NextStringAsync()));
                await this.InvokeFailAsync<PermissionException>(type.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.DeleteAsync(this.member));
            }
            finally
            {
                type.SetPublic(this.admin);
            }
        }

        [TestMethod]
        public void LockedTypeCategoryFailTest()
        {
            var typeContext = this.dataBase.TypeContext;
            var category = typeContext.Categories.RandomSample();
            var type = category.Types.Random();

            category.Lock(this.admin, string.Empty);

            try
            {
                await this.InvokeFailAsync<PermissionException>(category.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(category.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(category.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionException>(category.Lock(this.member, RandomUtility.NextStringAsync()));
                await this.InvokeFailAsync<PermissionDeniedException>(category.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.AddNewCategoryAsync(this.member));

                await this.InvokeFailAsync<PermissionException>(type.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(type.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(type.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(type.Lock(this.member, RandomUtility.NextStringAsync()));
                await this.InvokeFailAsync<PermissionException>(type.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.DeleteAsync(this.member));
            }
            finally
            {
                category.Unlock(this.admin);
            }
        }

        [TestMethod]
        public async Task PrivateTypeCategoryFailTestAsync()
        {
            var typeContext = this.dataBase.TypeContext;
            var category = typeContext.Categories.RandomSample();
            var type = category.Types.Random();

            category.SetPrivate(this.admin);

            try
            {
                await this.InvokeFailAsync<PermissionDeniedException>(category.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(category.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(category.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(category.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(category.Lock(this.member, RandomUtility.NextStringAsync()));
                await this.InvokeFailAsync<PermissionException>(category.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.DeleteAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(category.AddNewCategoryAsync(this.member));

                await this.InvokeFailAsync<PermissionException>(type.SetPublicAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.SetPrivateAsync(this.member));
                await this.InvokeFailAsync<PermissionException>(type.AddAccessMemberAsync(this.member, this.someone.UserID, AccessType.ReadWrite));
                await this.InvokeFailAsync<PermissionException>(type.RemoveAccessMemberAsync(this.member, this.someone.UserID));
                await this.InvokeFailAsync<PermissionDeniedException>(type.Lock(this.member, RandomUtility.NextStringAsync()));
                await this.InvokeFailAsync<PermissionException>(type.UnlockAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.RenameTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.MoveTestAsync(this.member));
                await this.InvokeFailAsync<PermissionDeniedException>(type.DeleteAsync(this.member));
            }
            finally
            {
                category.SetPublic(this.admin);
            }
        }

        public TestContext TestContext
        {
            get { return this.testContext; }
            set { this.testContext = value; }
        }

        private void InvokeFail<T>(Action action) where T : Exception
        {
            try
            {
                action();
                Assert.Fail();
            }
            catch (T)
            {

            }
        }

        private async Task InvokeFailAsync<T>(Task task) where T : Exception
        {
            try
            {
                await task
                Assert.Fail();
            }
            catch (T)
            {

            }
        }
    }
}

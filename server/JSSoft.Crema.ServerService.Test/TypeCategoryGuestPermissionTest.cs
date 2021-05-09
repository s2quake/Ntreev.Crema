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
using JSSoft.Crema.ServiceModel;
using System.Threading.Tasks;

// namespace JSSoft.Crema.ServerService.Test
// {
//     [TestClass]
//     public class TypeCategoryGuestPermissionTest
//     {
//         private static ICremaHost cremaHost;

//         private readonly static Dictionary<IUser, Authentication> users = new Dictionary<IUser, Authentication>();

//         private TestContext testContext;

//         private Authentication guest;
//         private ITypeCategory category;

//         [ClassInitialize()]
//         public static void ClassInit(TestContext context)
//         {
//             var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(context.TestDir));
//             var tempDir = Path.Combine(solutionDir, "crema_repo", "permission_test", "type_category_guest");
//             var empty = DirectoryUtility.Exists(tempDir) == false || DirectoryUtility.IsEmpty(tempDir);

//             cremaHost = TestCrema.GetInstance(tempDir);
//             cremaHost.Open();

//             if (empty == true)
//             {
//                 CremaSimpleGenerator.Generate(cremaHost, 10);
//             }

//             var userContext = cremaHost.GetService<IUserContext>();

//             foreach (var item in userContext.Users)
//             {
//                 var authentication = cremaHost.Login(item.ID, item.Authority.ToString().ToLower());
//                 users.Add(item, authentication);
//             }
//         }

//         [TestInitialize()]
//         public void Initialize()
//         {
//             var dataBase = cremaHost.PrimaryDataBase;
//             var typeContext = dataBase.TypeContext;
//             this.guest = users.Where(item => item.Key.Authority == Authority.Guest).Random().Value;
//             this.category = typeContext.Categories.RandomOrDefault(item => item.IsLocked == false && item.Parent != null);
//         }

//         [TestCleanup()]
//         public void Cleanup()
//         {
//             this.guest = null;
//             this.category = null;
//         }

//         [ClassCleanup()]
//         public static void ClassCleanup()
//         {
//             cremaHost.Dispatcher.Invoke(() => cremaHost.Close());
//             cremaHost.Dispose();
//         }

//         [TestMethod]
//         public async Task TypeCategoryFailTest()
//         {
//             try
//             {
//                 var categories = this.category.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
//                 var category = categories.RandomOrDefault(item => item.VerifyWrite(this.guest));
//                 Assert.IsNull(category);
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryRenameFailTest()
//         {
//             try
//             {
//                 var types = this.category.GetService(typeof(ITypeCollection)) as ITypeCollection;
//                 var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), types.Select(item => item.Name));
//                 await this.category.RenameAsync(this.guest, newName);
//                 Assert.Fail("Rename");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryMoveFailTest()
//         {
//             try
//             {
//                 var categories = this.category.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
//                 var target = categories.Random(item => item != this.category.Parent);
//                 await this.category.MoveAsync(this.guest, target.Path);
//                 Assert.Fail("Move");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryDeleteFailTest()
//         {
//             try
//             {
//                 await this.category.DeleteAsync(this.guest);
//                 Assert.Fail("Delete");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryNewCategoryFailTest()
//         {
//             try
//             {
//                 await this.category.AddNewCategoryAsync(this.guest);
//                 Assert.Fail("NewCategory");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryNewTypeFailTest()
//         {
//             try
//             {
//                 await this.category.NewTypeAsync(this.guest);
//                 Assert.Fail("NewType");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryLockFailTest()
//         {
//             try
//             {
//                 await this.category.LockAsync(this.guest, string.Empty);
//                 Assert.Fail("Lock");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryUnlockFailTest()
//         {
//             try
//             {
//                 await this.category.UnlockAsync(this.guest);
//                 Assert.Fail("Unlock");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategorySetPrivateFailTest()
//         {
//             try
//             {
//                 await this.category.SetPrivateAsync(this.guest);
//                 Assert.Fail("SetPrivate");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategorySetPublicFailTest()
//         {
//             try
//             {
//                 await this.category.SetPublicAsync(this.guest);
//                 Assert.Fail("SetPublic");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryAddAccessMemberFailTest()
//         {
//             try
//             {
//                 await this.category.AddAccessMemberAsync(this.guest, users.Keys.Random().ID, AccessType.Editor);
//                 Assert.Fail("AddAccessMember");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeCategoryRemoveAccessMemberFailTest()
//         {
//             try
//             {
//                 await this.category.RemoveAccessMemberAsync(this.guest, users.Keys.Random().ID);
//                 Assert.Fail("RemoveAccessMember");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         public TestContext TestContext
//         {
//             get { return this.testContext; }
//             set { this.testContext = value; }
//         }
//     }
// }

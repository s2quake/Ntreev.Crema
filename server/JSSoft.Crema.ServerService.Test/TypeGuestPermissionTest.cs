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
using System.Threading.Tasks;
using JSSoft.Crema.ServiceModel;

// namespace JSSoft.Crema.ServerService.Test
// {
//     [TestClass]
//     public class TypeGuestPermissionTest
//     {
//         private static CremaBootstrapper app;
//         private static ICremaHost cremaHost;
//         private static Guid token;


//         private readonly static Dictionary<IUser, Authentication> authenticationByUser = new Dictionary<IUser, Authentication>();

//         private TestContext testContext;

//         private Authentication guest;
//         private IType type;

//         [ClassInitialize()]
//         public static async Task ClassInit(TestContext context)
//         {
//             var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(context.TestDir));
//             var tempDir = Path.Combine(solutionDir, "crema_repo", "permission_test", "type_guest");
//             var empty = DirectoryUtility.Exists(tempDir) == false || DirectoryUtility.IsEmpty(tempDir);

//             app = new();
//             cremaHost = TestCrema.GetInstance(tempDir);
//             token = await cremaHost.OpenAsync();

//             if (empty == true)
//             {
//                 CremaSimpleGenerator.Generate(cremaHost, 10);
//             }

//             var userCollection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
//             var users = await userCollection.Dispatcher.InvokeAsync(() => userCollection.ToArray());
//             foreach (var item in users)
//             {
//                 var password = StringUtility.ToSecureString($"{item.Authority}".ToLower());
//                 var authenticationToken = await cremaHost.LoginAsync(item.ID, password);
//                 var authentication = await cremaHost.AuthenticateAsync(authenticationToken);
//                 authenticationByUser.Add(item, authentication);
//             }
//         }

//         [TestInitialize()]
//         public async Task InitializeAsync()
//         {
//             var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
//             var dataBase = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.First());
//             var typeContext = dataBase.TypeContext;
//             this.guest = authenticationByUser.Where(item => item.Key.Authority == Authority.Guest).Random().Value;
//             this.type = typeContext.Types.RandomOrDefault(item => item.IsLocked == false);
//         }

//         [TestCleanup()]
//         public void Cleanup()
//         {
//             this.guest = null;
//             this.type = null;
//         }

//         [ClassCleanup()]
//         public static async Task ClassCleanupAsync()
//         {
//             await cremaHost.CloseAsync(token);
//             token = Guid.Empty;
//         }

//         [TestMethod]
//         public async Task TypeFailTestAsync()
//         {
//             try
//             {
//                 var types = this.type.GetService(typeof(ITypeCollection)) as ITypeCollection;
//                 var type = types.RandomOrDefault(item => item.VerifyAccessType(this.guest, AccessType.Editor));
//                 Assert.IsNull(type);
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeRenameFailTestAsync()
//         {
//             try
//             {
//                 var types = this.type.GetService(typeof(ITypeCollection)) as ITypeCollection;
//                 var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), types.Select(item => item.Name));
//                 await this.type.RenameAsync(this.guest, newName);
//                 Assert.Fail("Rename");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeMoveFailTestAsync()
//         {
//             try
//             {
//                 var categories = this.type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
//                 var category = categories.RandomOrDefault(item => item != this.type.Category);
//                 await this.type.MoveAsync(this.guest, category.Path);
//                 Assert.Fail("Move");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeDeleteFailTestAsync()
//         {
//             try
//             {
//                 await this.type.DeleteAsync(this.guest);
//                 Assert.Fail("Delete");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeTemplateEditFailTestAsync()
//         {
//             try
//             {
//                 await this.type.Template.BeginEditAsync(this.guest);
//                 Assert.Fail("Template.BeginEdit");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeLockFailTestAsync()
//         {
//             try
//             {
//                 await this.type.LockAsync(this.guest, string.Empty);
//                 Assert.Fail("Lock");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeUnlockFailTestAsync()
//         {
//             try
//             {
//                 await this.type.UnlockAsync(this.guest);
//                 Assert.Fail("Unlock");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeSetPrivateFailTestAsync()
//         {
//             try
//             {
//                 await this.type.SetPrivateAsync(this.guest);
//                 Assert.Fail("SetPrivate");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeSetPublicFailTestAsync()
//         {
//             try
//             {
//                 await this.type.SetPublicAsync(this.guest);
//                 Assert.Fail("SetPublic");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeAddAccessMemberFailTestAsync()
//         {
//             try
//             {
//                 await this.type.AddAccessMemberAsync(this.guest, authenticationByUser.Keys.Random().ID, AccessType.Editor);
//                 Assert.Fail("AddAccessMember");
//             }
//             catch (PermissionDeniedException)
//             {
//             }
//         }

//         [TestMethod]
//         public async Task TypeRemoveAccessMemberFailTestAsync()
//         {
//             try
//             {
//                 await this.type.RemoveAccessMemberAsync(this.guest, authenticationByUser.Keys.Random().ID);
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

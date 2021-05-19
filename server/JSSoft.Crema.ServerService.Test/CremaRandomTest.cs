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
using JSSoft.Library.Random;
using System.Reflection;
using System.Linq;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using JSSoft.Library;
using JSSoft.Crema.ServiceModel;

namespace JSSoft.Crema.ServerService.Test
{
    // [TestClass]
    // public class CremaRandomTest
    // {
    //     private static ICremaHost cremaHost;
    //     private static Guid token;

    //     [ClassInitialize()]
    //     public static async Task ClassInitAsync(TestContext context)
    //     {
    //         var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(context.TestDir));
    //         var path = Path.Combine(solutionDir, "crema_repo", "test_all");
    //         cremaHost = TestCrema.GetInstance(path);
    //     }

    //     [TestInitialize()]
    //     public async Task InitializeAsync()
    //     {
    //         token = await cremaHost.OpenAsync();
    //     }

    //     [TestCleanup()]
    //     public async Task CleanupAsync()
    //     {
    //         await cremaHost.CloseAsync(token);
    //         token = Guid.Empty;
    //     }

    //     [ClassCleanup()]
    //     public static async Task ClassCleanupAsync()
    //     {
    //         // cremaHost.Dispose();
    //     }

    //     [TestMethod]
    //     public async Task TestTestAsync()
    //     {
    //         var password = StringUtility.ToSecureString("admin");
    //         var token = await cremaHost.LoginAsync("Admin", password);
    //         var authentication = await cremaHost.AuthenticateAsync(token);
    //         var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    //         var dataBase = await dataBaseContext.Dispatcher.InvokeAsync(dataBaseContext.First);
    //         var tableContext = dataBase.GetService(typeof(ITableContext)) as ITableContext;
    //         var typeContext = dataBase.GetService(typeof(ITypeContext)) as ITypeContext;
    //         var tableRoot = await tableContext.Dispatcher.InvokeAsync(() => tableContext.Root);
    //         var typeRoot = await typeContext.Dispatcher.InvokeAsync(() => typeContext.Root);

    //         var category = await typeRoot.AddNewCategoryAsync(authentication, "sub");
    //         var category1 = await typeRoot.AddNewCategoryAsync(authentication, "other");

    //         var t_category_sub = await tableRoot.AddNewCategoryAsync(authentication, "sub");
    //         var t_category_sub_wow = await t_category_sub.AddNewCategoryAsync(authentication, "wow");
    //         var t_category_other = await tableRoot.AddNewCategoryAsync(authentication, "other");
    //         var t_category_other_hehe = await tableRoot.AddNewCategoryAsync(authentication, "hehe");

    //         var typeTemplate = await typeRoot.NewTypeAsync(authentication);
    //         await typeTemplate.EndEditAsync(authentication);

    //         var type = typeTemplate.Type;

    //         var tableTemplate = await t_category_sub_wow.NewTableAsync(authentication);
    //         await tableTemplate.AddKeyAsync(authentication, "key", "int");
    //         await tableTemplate.AddColumnAsync(authentication, "value", type.Path);
    //         await tableTemplate.EndEditAsync(authentication);

    //         var table = tableTemplate.Target as ITable;
    //         {
    //             var childTemplate = await table.NewTableAsync(authentication);
    //             await childTemplate.AddKeyAsync(authentication, "key", "int");
    //             await childTemplate.AddColumnAsync(authentication, "value", type.Path);
    //             await childTemplate.EndEditAsync(authentication);
    //         }

    //         {
    //             var childTemplate = await table.NewTableAsync(authentication);
    //             await childTemplate.AddKeyAsync(authentication, "key", "int");
    //             await childTemplate.AddColumnAsync(authentication, "value", type.Path);
    //             await childTemplate.EndEditAsync(authentication);
    //         }

    //         await table.InheritAsync(authentication, "table2", t_category_other.Path, false);

    //         for (int i = 0; i < 100; i++)
    //         {
    //             CremaHostUtility.TableMoveTest(cremaHost, authentication);
    //             CremaHostUtility.TableRenameTest(cremaHost, authentication);
    //             CremaHostUtility.TableCategoryMoveTest(cremaHost, authentication);
    //             CremaHostUtility.TableCategoryRenameTest(cremaHost, authentication);
    //         }


    //         //table.Childs.ToArray()[1].Delete(authentication);
    //         //table.Childs.ToArray()[0].Delete(authentication);

    //         //type.RenameAsync(authentication, "wow");
    //         //type.MoveAsync(authentication, category.Path);
    //         //category.RenameAsync(authentication, "sub1");
    //         //category.MoveAsync(authentication, category1.Path);
    //         //category.RenameAsync(authentication, "sub");
    //         //category1.Rename(authentication, "other1");
    //         ////type.MoveAsync(authentication, cremaHost.PrimaryDataBase.TypeContext.Root.Path);
    //         //type.Category.Delete(authentication);
    //     }

    //     [TestMethod]
    //     public void TestAll()
    //     {
    //         var methods = typeof(CremaHostUtility).GetMethods();
    //         var users = new Dictionary<string, Authentication>();

    //         CreateBase(cremaHost);

    //         //while(true)
    //         for (var i = 0; i < 100; i++)
    //         {
    //             var method = methods.Random(Predicate);
    //             if (method.Name.IndexOf("Delete") >= 0)
    //                 continue;

    //             if (users.Any() == false || RandomUtility.Within(20) == true)
    //             {
    //                 LogIn(cremaHost, users);
    //             }

    //             var authentication = users.Random().Value;

    //             try
    //             {
    //                 method.Invoke(null, new object[] { cremaHost, authentication, });
    //             }
    //             catch (PermissionDeniedException)
    //             {

    //             }

    //             CremaHostUtility.TableContentEditTest(cremaHost, authentication);

    //             if (users.Any() == true || RandomUtility.Within(10) == true)
    //             {
    //                 Logout(cremaHost, users);
    //             }
    //         }
    //     }

    //     [TestMethod]
    //     public async Task TestOneAsync()
    //     {
    //         var password = StringUtility.ToSecureString("admin");
    //         var authenticationToken = await cremaHost.LoginAsync("Admin", password);
    //         var authentication = await cremaHost.AuthenticateAsync(authentication);
    //         var dataBase = (await cremaHost.GetDataBasesAsync()).First();
    //         var transaction = await dataBase.BeginTransactionAsync(authentication);

    //         //Table_thorny.Child2
    //         var table = cremaHost.PrimaryDataBase.TableContext.Tables["Table_thorny"];
    //         var template = table.Childs["Child2"].Template;
    //         template.BeginEdit(authentication);

    //         var column = template.AddNew(authentication);
    //         column.SetName(authentication, "wow");
    //         column.SetIsKey(authentication, true);
    //         template.EndNew(authentication, column);

    //         template.EndEdit(authentication);

    //         await transaction.RollbackAsync(authentication);
    //     }

    //     private static void CreateBase(ICremaHost cremaHost)
    //     {
    //         var authentication = cremaHost.Dispatcher.Invoke(() =>
    //         {
    //             var userContext = cremaHost.GetService<IUserContext>();
    //             var user = userContext.Users.Random(item => item.Authority == Authority.Admin);
    //             return cremaHost.Login(user.ID, "admin");
    //         });

    //         if (cremaHost.PrimaryDataBase.TypeContext.Types.Any() == false)
    //         {
    //             for (int i = 0; i < 10; i++)
    //             {
    //                 CremaHostUtility.TypeCategoryCreateTest(cremaHost, authentication);
    //             }

    //             for (int i = 0; i < 10; i++)
    //             {
    //                 CremaHostUtility.TypeCreateTest(cremaHost, authentication);
    //             }
    //         }

    //         if (cremaHost.PrimaryDataBase.TableContext.Tables.Any() == false)
    //         {
    //             for (int i = 0; i < 10; i++)
    //             {
    //                 CremaHostUtility.TableCategoryCreateTest(cremaHost, authentication);
    //             }

    //             for (int i = 0; i < 10; i++)
    //             {
    //                 CremaHostUtility.TableCreateTest(cremaHost, authentication);
    //             }

    //             for (int i = 0; i < 10; i++)
    //             {
    //                 CremaHostUtility.ChildTableCreateTest(cremaHost, authentication);
    //             }

    //             for (int i = 0; i < 10; i++)
    //             {
    //                 CremaHostUtility.TableInheritTest(cremaHost, authentication);
    //             }

    //             for (int i = 0; i < 10; i++)
    //             {
    //                 CremaHostUtility.ChildTableCreateTest(cremaHost, authentication);
    //             }
    //         }

    //         cremaHost.Dispatcher.Invoke(() =>
    //         {
    //             var userContext = cremaHost.GetService<IUserContext>();
    //             userContext.Logout(authentication, authentication.UserID);
    //         });
    //     }

    //     private static void LogIn(ICremaHost cremaHost, Dictionary<string, Authentication> users)
    //     {
    //         cremaHost.Dispatcher.Invoke(() =>
    //         {
    //             var userContext = cremaHost.GetService<IUserContext>();
    //             var user = userContext.Users.RandomOrDefault(item => item.Authority != Authority.Guest && item.UserState == UserState.None);
    //             if (user != null)
    //             {
    //                 var authentication = cremaHost.Login(user.ID, user.Authority.ToString().ToLower());
    //                 users.Add(user.ID, authentication);
    //             }
    //         });
    //     }

    //     private static void Logout(ICremaHost cremaHost, Dictionary<string, Authentication> users)
    //     {
    //         if (users.Any() == false)
    //             return;

    //         cremaHost.Dispatcher.Invoke(() =>
    //         {
    //             var userContext = cremaHost.GetService<IUserContext>();
    //             var user = users.Random();
    //             userContext.Logout(user.Value, user.Key);
    //             users.Remove(user.Key);
    //         });
    //     }

    //     private static bool Predicate(MethodInfo methodInfo)
    //     {
    //         if (methodInfo.IsStatic == false)
    //             return false;

    //         var parameters = methodInfo.GetParameters();
    //         if (parameters.Count() != 2)
    //             return false;

    //         return parameters[0].ParameterType == typeof(ICremaHost) && parameters[1].ParameterType == typeof(Authentication);
    //     }
    // }
}

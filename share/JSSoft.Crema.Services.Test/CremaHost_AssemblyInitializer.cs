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
using JSSoft.Crema.Services.Random;
using JSSoft.Library;
using System.IO;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Test.Extensions;
using System.Reflection;

namespace JSSoft.Crema.Services.Test
{
    [TestClass]
    public class CremaHost_AssemblyInitializer
    {
        // private static CremaBootstrapper app;
        // private static ICremaHost cremaHost;
        // private static Authentication authentication;

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            AppUtility.ProductName = "CremaTest";
            AppUtility.ProductVersion = "1.0.0.0";
            AppUtility.UserAppDataPath = Path.Combine(context.TestRunResultsDirectory, "AppData");

            CremaRandomSettings.TypeContext.MinTypeCount = 1;
            CremaRandomSettings.TypeContext.MaxTypeCount = 3;
            CremaRandomSettings.TypeContext.MinTypeCategoryCount = 1;
            CremaRandomSettings.TypeContext.MaxTypeCategoryCount = 3;

            CremaRandomSettings.TableContext.MinRowCount = 1;
            CremaRandomSettings.TableContext.MaxRowCount = 10;
            CremaRandomSettings.TableContext.MinTableCount = 1;
            CremaRandomSettings.TableContext.MaxTableCount = 3;
            CremaRandomSettings.TableContext.MinTableCategoryCount = 1;
            CremaRandomSettings.TableContext.MaxTableCategoryCount = 3;

            TableTemplateExtensions.MinColumnCount = 2;
            TableTemplateExtensions.MaxColumnCount = 5;



            //var solutionDir = Path.GetDirectoryName(context.TestDir);
            //var exePath = Path.Combine(solutionDir, "server", "JSSoft.Crema.ConsoleHost", "bin", "debug", "netcoreapp3.1", "crema-server.dll");

            //var domain = System.AppDomain.CreateDomain("crema-server");
        }

        //[ClassInitialize]
        //public static async Task ClassInitAsync(TestContext context)
        //{
        //    app = new CremaBootstrapper();
        //    app.Initialize(context);
        //    cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
        //    authentication = await cremaHost.StartAsync();
        //}

        //[ClassCleanup]
        //public static async Task ClassCleanupAsync()
        //{
        //    await cremaHost.StopAsync(authentication);
        //    app.Dispose();
        //}

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {

        }
    }
}

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
using System;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.Services.Test.Deleted_DispatcherTest
{
    [TestClass]
    public class ITypeMember_Deleted_DispatcherTest
    {
        private static CremaBootstrapper app;
        private static ICremaHost cremaHost;
        private static Authentication authentication;
        private static IDataBase dataBase;
        private static ITypeTemplate template;
        private static ITypeMember member;

        [ClassInitialize]
        public static async Task ClassInitAsync(TestContext context)
        {
            app = new CremaBootstrapper();
            app.Initialize(context, nameof(ITypeMember_Deleted_DispatcherTest));
            cremaHost = app.GetService(typeof(ICremaHost)) as ICremaHost;
            authentication = await cremaHost.StartAsync();
            dataBase = await cremaHost.GetRandomDataBaseAsync();
            await dataBase.LoadAsync(authentication);
            await dataBase.EnterAsync(authentication);
            await dataBase.TypeContext.AddRandomItemsAsync(authentication);
            template = dataBase.TypeContext.Types.Random().Template;
            await template.BeginEditAsync(authentication);
            member = template.Random();
            await dataBase.LeaveAsync(authentication);
            await dataBase.UnloadAsync(authentication);
        }

        [ClassCleanup]
        public static async Task ClassCleanupAsync()
        {
            await cremaHost.StopAsync(authentication);
            app.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteAsync()
        {
            await member.DeleteAsync(authentication);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetIndexAsync()
        {
            await member.SetIndexAsync(authentication, RandomUtility.Next(int.MaxValue));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetNameAsync()
        {
            await member.SetNameAsync(authentication, RandomUtility.NextIdentifier());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetValueAsync()
        {
            await member.SetValueAsync(authentication, RandomUtility.NextLong(long.MaxValue));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SetCommentAsync()
        {
            await member.SetCommentAsync(authentication, RandomUtility.NextString());
        }

        [TestMethod]
        public void Index()
        {
            Console.Write(member.Index);
        }

        [TestMethod]
        public void Name()
        {
            Console.Write(member.Name);
        }

        [TestMethod]
        public void Value()
        {
            Console.Write(member.Value);
        }

        [TestMethod]
        public void Comment()
        {
            Console.Write(member.Comment);
        }

        [TestMethod]
        public void Template()
        {
            Console.Write(member.Template);
        }
    }
}

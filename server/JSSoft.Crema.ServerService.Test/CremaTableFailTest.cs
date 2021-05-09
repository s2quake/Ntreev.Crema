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
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSSoft.Crema.ServiceModel;

namespace JSSoft.Crema.ServerService.Test
{
    static class CremaTableFailTest
    {
        public static async Task RenameFailTestAsyncAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                var tables = table.GetService(typeof(ITableCollection)) as ITableCollection;
                var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), tables.Select(item => item.Name));
                await table.RenameAsync(authentication, newName);
                Assert.Fail("RenameFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task MoveFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                var category = categories.RandomOrDefault(item => item != table.Category);
                await table.MoveAsync(authentication, category.Path);
                Assert.Fail("MoveFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task DeleteFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.DeleteAsync(authentication);
                Assert.Fail("DeleteFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task NewChildFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.NewTableAsync(authentication);
                Assert.Fail("NewChildFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task ContentEditFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.Content.BeginEditAsync(authentication);
                Assert.Fail("ContentEditFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task TemplateEditFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.Template.BeginEditAsync(authentication);
                Assert.Fail("TemplateEditFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task LockFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.LockAsync(authentication, string.Empty);
                Assert.Fail("LockFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task UnlockFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.UnlockAsync(authentication);
                Assert.Fail("UnlockFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPrivateFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.SetPrivateAsync(authentication);
                Assert.Fail("SetPrivateFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPublicFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.SetPublicAsync(authentication);
                Assert.Fail("SetPublicFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task AddAccessMemberFailTestAsync<T>(ITable table, Authentication authentication, string memberID) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.AddAccessMemberAsync(authentication, memberID, AccessType.Editor);
                Assert.Fail("AddAccessMemberFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task RemoveAccessMemberFailTestAsync<T>(ITable table, Authentication authentication, string memberID) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            try
            {
                await table.RemoveAccessMemberAsync(authentication, memberID);
                Assert.Fail("RemoveAccessMemberFailTest");
            }
            catch (T)
            {
            }
        }
    }
}

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
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServerService.Test
{
    static class CremaChildTableTest
    {
        public static async Task RenameFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                var parent = table.Parent;
                var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), parent.Childs.Select(item => item.Name));
                await table.RenameAsync(authentication, newName);
                Assert.Fail("RenameFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task MoveFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                var parent = categories.RandomOrDefault(item => item.VerifyAccessType(authentication, AccessType.Developer));
                await table.MoveAsync(authentication, parent.Path);
                Assert.Fail("MoveFailTest");
            }
            catch (T)
            {
            }
        }

        public static async Task DeleteFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
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
            Assert.AreNotEqual(null, table.Parent);
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
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.Content.BeginEditAsync(authentication);
                Assert.Fail("Content.BeginEdit");
            }
            catch (T)
            {
            }
        }

        public static async Task TemplateEditFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.Template.BeginEditAsync(authentication);
                Assert.Fail("Template.BeginEdit");
            }
            catch (T)
            {
            }
        }

        public static async Task LockFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.LockAsync(authentication, string.Empty);
                Assert.Fail("Lock");
            }
            catch (T)
            {
            }
        }

        public static async Task UnlockFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.UnlockAsync(authentication);
                Assert.Fail("Unlock");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPrivateFailTestAsync<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.SetPrivateAsync(authentication);
                Assert.Fail("SetPrivate");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPublicFailTestTask<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.SetPublicAsync(authentication);
                Assert.Fail("SetPublic");
            }
            catch (T)
            {
            }
        }

        public static async Task AddAccessMemberFailTestAsync<T>(ITable table, Authentication authentication, string memberID) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.AddAccessMemberAsync(authentication, memberID, AccessType.Developer);
                Assert.Fail("AddAccessMember");
            }
            catch (T)
            {
            }
        }

        public static async Task RemoveAccessMemberFailTestAsync<T>(ITable table, Authentication authentication, string memberID) where T : Exception
        {
            Assert.AreNotEqual(null, table.Parent);
            try
            {
                await table.RemoveAccessMemberAsync(authentication, memberID);
                Assert.Fail("RemoveAccessMember");
            }
            catch (T)
            {
            }
        }
    }
}

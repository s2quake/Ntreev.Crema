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
    static class CremaTableFailTest
    {
        public static void RenameFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    var tables = table.GetService(typeof(ITableCollection)) as ITableCollection;
                    var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), tables.Select(item => item.Name));
                    table.Rename(authentication, newName);
                    Assert.Fail("RenameFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void MoveFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                    var category = categories.RandomOrDefault(item => item != table.Category);
                    table.Move(authentication, category.Path);
                    Assert.Fail("MoveFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void DeleteFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.Delete(authentication);
                    Assert.Fail("DeleteFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void NewChildFailTest<T>(ITable table, Authentication authentication)  where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.NewTable(authentication);
                    Assert.Fail("NewChildFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void ContentEditFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.Content.BeginEdit(authentication);
                    Assert.Fail("ContentEditFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void TemplateEditFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.Template.BeginEdit(authentication);
                    Assert.Fail("TemplateEditFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void LockFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.Lock(authentication, string.Empty);
                    Assert.Fail("LockFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void UnlockFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.Unlock(authentication);
                    Assert.Fail("UnlockFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPrivateFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.SetPrivate(authentication);
                    Assert.Fail("SetPrivateFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPublicFailTest<T>(ITable table, Authentication authentication) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.SetPublic(authentication);
                    Assert.Fail("SetPublicFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void AddAccessMemberFailTest<T>(ITable table, Authentication authentication, string memberID) where T :Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.AddAccessMember(authentication, memberID, AccessType.ReadWrite);
                    Assert.Fail("AddAccessMemberFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void RemoveAccessMemberFailTest<T>(ITable table, Authentication authentication, string memberID) where T : Exception
        {
            Assert.AreEqual(null, table.Parent);
            table.Dispatcher.Invoke(() =>
            {
                try
                {
                    table.RemoveAccessMember(authentication, memberID);
                    Assert.Fail("RemoveAccessMemberFailTest");
                }
                catch (T)
                {
                }
            });
        }
    }
}

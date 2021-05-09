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
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSSoft.Crema.ServiceModel;

namespace JSSoft.Crema.ServerService.Test
{
    static class CremaTableCategoryTest
    {
        public static void RenameFailTest<T>(ITableCategory category, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    var parent = category.Parent;
                    var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), parent.Categories.Select(item => item.Name));
                    category.RenameAsync(authentication, newName);
                    Assert.Fail("RenameFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void MoveFailTest<T>(ITableCategory category, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                    var descendants = EnumerableUtility.Descendants(category, item => item.Categories);
                    var target = categories.Random(item => descendants.Contains(item) == false && item != category.Parent);
                    category.MoveAsync(authentication, target.Path);
                    Assert.Fail("MoveFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void DeleteFailTest<T>(ITableCategory category, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.DeleteAsync(authentication);
                    Assert.Fail("DeleteFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void LockFailTest<T>(ITableCategory category, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.LockAsync(authentication, RandomUtility.NextString());
                    Assert.Fail("LockFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void UnlockFailTest<T>(ITableCategory category, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.UnlockAsync(authentication);
                    Assert.Fail("UnlockFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPrivateFailTest<T>(ITableCategory category, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.SetPrivateAsync(authentication);
                    Assert.Fail("SetPrivateFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPublicFailTest<T>(ITableCategory category, Authentication authentication) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.SetPublicAsync(authentication);
                    Assert.Fail("SetPublicFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void AddAccessMemberFailTest<T>(ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.AddAccessMemberAsync(authentication, memberID, AccessType.Editor);
                    Assert.Fail("AddAccessMemberFailTest");
                }
                catch (T)
                {
                }
            });
        }

        public static void RemoveAccessMemberFailTest<T>(ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            Assert.AreNotEqual(null, category.Parent);
            category.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.RemoveAccessMemberAsync(authentication, memberID);
                    Assert.Fail("RemoveAccessMemberFailTest");
                }
                catch (T)
                {
                }
            });
        }
    }
}

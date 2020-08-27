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
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServerService.Test
{
    static class CremaTableCategoryPermissionTest
    {
        public static void RenameFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Rename(authentication, RandomUtility.NextIdentifier());
                    Assert.Fail("Rename");
                }
                catch (T)
                {
                }
            });
        }

        public static void RenameParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.Rename(authentication, RandomUtility.NextIdentifier());
                    Assert.Fail("Rename");
                }
                catch (T)
                {
                }
            });
        }

        public static void MoveFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                    category.Move(authentication, categories.Root.Path);
                    Assert.Fail("Move");
                }
                catch (T)
                {
                }
            });
        }

        public static void MoveParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                    category.Parent.Move(authentication, categories.Root.Path);
                    Assert.Fail("Move");
                }
                catch (T)
                {
                }
            });
        }

        public static void DeleteFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Delete(authentication);
                    Assert.Fail("Delete");
                }
                catch (T)
                {
                }
            });
        }

        public static void DeleteParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.Delete(authentication);
                    Assert.Fail("Delete");
                }
                catch (T)
                {
                }
            });
        }

        public static void NewCategoryFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.AddNewCategory(authentication);
                    Assert.Fail("NewCategory");
                }
                catch (T)
                {
                }
            });
        }

        public static void NewTableFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.NewTable(authentication);
                    Assert.Fail("NewTable");
                }
                catch (T)
                {
                }
            });
        }

        public static void LockFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Lock(authentication, string.Empty);
                    Assert.Fail("Lock");
                }
                catch (T)
                {
                }
            });
        }

        public static void LockParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.Lock(authentication, string.Empty);
                    Assert.Fail("Lock");
                }
                catch (T)
                {
                }
            });
        }

        public static void UnlockFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Unlock(authentication);
                    Assert.Fail("Unlock");
                }
                catch (T)
                {
                }
            });
        }

        public static void UnlockParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.Unlock(authentication);
                    Assert.Fail("Unlock");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPrivateFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.SetPrivate(authentication);
                    Assert.Fail("SetPrivate");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPrivateParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.SetPrivate(authentication);
                    Assert.Fail("SetPrivate");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPublicFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.SetPublic(authentication);
                    Assert.Fail("SetPublic");
                }
                catch (T)
                {
                }
            });
        }

        public static void SetPublicParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.SetPublic(authentication);
                    Assert.Fail("SetPublic");
                }
                catch (T)
                {
                }
            });
        }

        public static void AddAccessMemberFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.AddAccessMember(authentication, memberID, AccessType.ReadWrite);
                    Assert.Fail("AddAccessMember");
                }
                catch (T)
                {
                }
            });
        }

        public static void AddAccessMemberParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.AddAccessMember(authentication, memberID, AccessType.ReadWrite);
                    Assert.Fail("AddAccessMember");
                }
                catch (T)
                {
                }
            });
        }

        public static void RemoveAccessMemberFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.RemoveAccessMember(authentication, memberID);
                    Assert.Fail("RemoveAccessMember");
                }
                catch (T)
                {
                }
            });
        }

        public static void RemoveAccessMemberParentFailTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            cremaHost.Dispatcher.Invoke(() =>
            {
                try
                {
                    category.Parent.RemoveAccessMember(authentication, memberID);
                    Assert.Fail("RemoveAccessMember");
                }
                catch (T)
                {
                }
            });
        }
    }
}

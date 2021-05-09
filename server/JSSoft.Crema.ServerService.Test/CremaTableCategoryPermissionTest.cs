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
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSSoft.Crema.ServiceModel;

namespace JSSoft.Crema.ServerService.Test
{
    static class CremaTableCategoryPermissionTest
    {
        public static async Task RenameFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.RenameAsync(authentication, RandomUtility.NextIdentifier());
                Assert.Fail("Rename");
            }
            catch (T)
            {
            }
        }

        public static async Task RenameParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.Parent.RenameAsync(authentication, RandomUtility.NextIdentifier());
                Assert.Fail("Rename");
            }
            catch (T)
            {
            }
        }

        public static async Task MoveFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                var categories = await category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                await category.MoveAsync(authentication, categories.Root.Path);
                Assert.Fail("Move");
            }
            catch (T)
            {
            }
        }

        public static async Task MoveParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                var categories = await category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
                await category.Parent.MoveAsync(authentication, categories.Root.Path);
                Assert.Fail("Move");
            }
            catch (T)
            {
            }
        }

        public static async Task DeleteFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.DeleteAsync(authentication);
                Assert.Fail("Delete");
            }
            catch (T)
            {
            }
        }

        public static async Task DeleteParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.Parent.DeleteAsync(authentication);
                Assert.Fail("Delete");
            }
            catch (T)
            {
            }
        }

        public static async Task NewCategoryFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.AddNewCategoryAsync(authentication);
                Assert.Fail("NewCategory");
            }
            catch (T)
            {
            }
        }

        public static async Task NewTableFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.NewTableAsync(authentication);
                Assert.Fail("NewTable");
            }
            catch (T)
            {
            }
        }

        public static async Task LockFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.LockAsync(authentication, string.Empty);
                Assert.Fail("Lock");
            }
            catch (T)
            {
            }
        }

        public static async Task LockParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.Parent.LockAsync(authentication, string.Empty);
                Assert.Fail("Lock");
            }
            catch (T)
            {
            }
        }

        public static async Task UnlockFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.UnlockAsync(authentication);
                Assert.Fail("Unlock");
            }
            catch (T)
            {
            }
        }

        public static async Task UnlockParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.Parent.UnlockAsync(authentication);
                Assert.Fail("Unlock");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPrivateFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.SetPrivateAsync(authentication);
                Assert.Fail("SetPrivate");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPrivateParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.Parent.SetPrivateAsync(authentication);
                Assert.Fail("SetPrivate");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPublicFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.SetPublicAsync(authentication);
                Assert.Fail("SetPublic");
            }
            catch (T)
            {
            }
        }

        public static async Task SetPublicParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication) where T : Exception
        {
            try
            {
                await category.Parent.SetPublicAsync(authentication);
                Assert.Fail("SetPublic");
            }
            catch (T)
            {
            }
        }

        public static async Task AddAccessMemberFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            try
            {
                await category.AddAccessMemberAsync(authentication, memberID, AccessType.Editor);
                Assert.Fail("AddAccessMember");
            }
            catch (T)
            {
            }
        }

        public static async Task AddAccessMemberParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            try
            {
                await category.Parent.AddAccessMemberAsync(authentication, memberID, AccessType.Editor);
                Assert.Fail("AddAccessMember");
            }
            catch (T)
            {
            }
        }

        public static async Task RemoveAccessMemberFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            try
            {
                await category.RemoveAccessMemberAsync(authentication, memberID);
                Assert.Fail("RemoveAccessMember");
            }
            catch (T)
            {
            }
        }

        public static async Task RemoveAccessMemberParentFailAsyncTest<T>(ICremaHost cremaHost, ITableCategory category, Authentication authentication, string memberID) where T : Exception
        {
            try
            {
                await category.Parent.RemoveAccessMemberAsync(authentication, memberID);
                Assert.Fail("RemoveAccessMember");
            }
            catch (T)
            {
            }
        }
    }
}

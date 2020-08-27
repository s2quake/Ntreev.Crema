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
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServerService.Test
{
    static class CremaTableTest
    {
        public static void RenameTest(this ITable table, Authentication authentication)
        {
            if (table.Parent == null)
            {
                var tables = table.GetService(typeof(ITableCollection)) as ITableCollection;
                var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), tables.Select(item => item.Name));
                table.Rename(authentication, newName);
            }
            else
            {
                var parent = table.Parent;
                var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), parent.Childs.Select(item => item.Name));
                table.Rename(authentication, newName);
            }
        }

        public static void MoveTest(this ITable table, Authentication authentication)
        {
            var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var category = categories.RandomOrDefault(item => item != table.Category);
            if (category == null)
            {
                Assert.Inconclusive();
            }
            table.Move(authentication, category.Path);
        }

        public static void RenameTest(this ITableCategory category, Authentication authentication)
        {
            Assert.AreNotEqual(null, category.Parent);
            var parent = category.Parent;
            var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), parent.Categories.Select(item => item.Name));
            category.Rename(authentication, newName);
        }

        public static void MoveTest(this ITableCategory category, Authentication authentication)
        {
            Assert.AreNotEqual(null, category.Parent);
            var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var descendants = EnumerableUtility.Descendants(category, item => item.Categories);
            var target = categories.Random(item => descendants.Contains(item) == false && item != category && item != category.Parent);
            if (target == null)
            {
                Assert.Inconclusive();
            }
            category.Move(authentication, target.Path);
        }

        public static void RenameTest(this IType type, Authentication authentication)
        {
            var types = type.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), types.Select(item => item.Name));
            type.Rename(authentication, newName);
        }

        public static void MoveTest(this IType type, Authentication authentication)
        {
            var categories = type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var category = categories.RandomOrDefault(item => item != type.Category);
            if (category == null)
            {
                Assert.Inconclusive();
            }
            type.Move(authentication, category.Path);
        }

        public static void RenameTest(this ITypeCategory category, Authentication authentication)
        {
            Assert.AreNotEqual(null, category.Parent);
            var parent = category.Parent;
            var newName = NameUtility.GenerateNewName(RandomUtility.NextIdentifier(), parent.Categories.Select(item => item.Name));
            category.Rename(authentication, newName);
        }

        public static void MoveTest(this ITypeCategory category, Authentication authentication)
        {
            Assert.AreNotEqual(null, category.Parent);
            var categories = category.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var descendants = EnumerableUtility.Descendants(category, item => item.Categories);
            var target = categories.Random(item => descendants.Contains(item) == false && item != category && item != category.Parent);
            if (target == null)
            {
                Assert.Inconclusive();
            }
            category.Move(authentication, target.Path);
        }
    }
}

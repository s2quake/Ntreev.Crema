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

using JSSoft.Crema.Data;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Extensions;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.ServerService.Test
{
    public static class CremaObjectGenerator
    {
        public static async Task GenerateStandardAsync(this IDataBase dataBase, Authentication authentication)
        {
            await dataBase.TypeContext.GenerateStandardAsync(authentication);
            await dataBase.TableContext.GenerateStandardAsync(authentication);
        }

        public static async Task GenerateStandardAsync(this ITypeContext context, Authentication authentication)
        {
            var root = context.Root;
            {
                await root.GenerateStandardTypeAsync(authentication);
                await root.GenerateStandardFlagsAsync(authentication);
            }

            var category = await root.AddNewCategoryAsync(authentication);
            {
                await category.GenerateStandardTypeAsync(authentication);
                await category.GenerateStandardFlagsAsync(authentication);
            }

            var subCategory = await category.AddNewCategoryAsync(authentication);
            {
                await subCategory.GenerateStandardTypeAsync(authentication);
                await subCategory.GenerateStandardFlagsAsync(authentication);
            }
        }

        public static async Task GenerateStandardAsync(this ITableContext context, Authentication authentication)
        {
            var typeContext = context.GetService(typeof(ITypeContext)) as ITypeContext;
            var root = context.Root;
            var types1 = typeContext.Types.Select(item => item.Name).ToArray();
            var types2 = CremaDataTypeUtility.GetBaseTypeNames();
            var allTypes = types1.Concat(types2);
            var allKeyTypes = types1.Concat(types2).Where(item => item != typeof(bool).GetTypeName());

            {
                var category = await root.AddNewCategoryAsync(authentication, "SingleKey");
                foreach (var item in allTypes)
                {
                    var table = await category.GenerateStandardTableAsync(authentication, "SingleKey", EnumerableUtility.AsEnumerable(item), allTypes.Where(i => i != item));
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "SingleKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }

            {
                var category = await root.AddNewCategoryAsync(authentication, "DoubleKey");
                var query = allKeyTypes.Permutations(2);
                for (int i = 0; i < allTypes.Count(); i++)
                {
                    var keys = query.Random();
                    var columns = allTypes.Except(keys);
                    var table = await category.GenerateStandardTableAsync(authentication, "DoubleKey", keys, columns);
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "DoubleKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }

            {
                var category = await root.AddNewCategoryAsync(authentication, "TripleKey");
                var query = allKeyTypes.Permutations(3);
                for (int i = 0; i < allTypes.Count(); i++)
                {
                    var keys = query.Random();
                    var columns = allTypes.Except(keys);
                    var table = await category.GenerateStandardTableAsync(authentication, "TripleKey", keys, columns);
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "TripleKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }

            {
                var category = await root.AddNewCategoryAsync(authentication, "QuadraKey");
                var query = allKeyTypes.Permutations(4);
                for (int i = 0; i < allTypes.Count(); i++)
                {
                    var keys = query.Random();
                    var columns = allTypes.Except(keys);
                    var table = await category.GenerateStandardTableAsync(authentication, "QuadraKey", keys, columns);
                    if (table == null)
                        continue;
                    await table.GenerateStandardChildAsync(authentication);
                    await table.GenerateStandardContentAsync(authentication);
                }

                var category1 = await root.AddNewCategoryAsync(authentication, "QuadraKeyRefs");
                {
                    foreach (var item in category.Tables)
                    {
                        var tables = await item.InheritAsync(authentication, "Ref_" + item.Name, category1.Path, false);
                        foreach (var i in tables)
                        {
                            await i.GenerateStandardContentAsync(authentication);
                        }
                    }
                }
            }
        }

        public static async Task<ITable> GenerateStandardTableAsync(this ITableCategory category, Authentication authentication, string prefix, IEnumerable<string> keyTypes, IEnumerable<string> columnTypes)
        {
            var tables = category.GetService(typeof(ITableCollection)) as ITableCollection;
            var tableName = string.Join("_", EnumerableUtility.Friends(prefix, keyTypes));

            if (tables.Contains(tableName) == true)
                return null;

            var template = await category.NewTableAsync(authentication);
            await template.SetTableNameAsync(authentication, tableName);

            foreach (var item in keyTypes)
            {
                await template.AddKeyAsync(authentication, item, item);
            }

            foreach (var item in columnTypes)
            {
                await template.AddColumnAsync(authentication, item, item);
            }

            try
            {
                await template.EndEditAsync(authentication);
                return template.Target as ITable;
            }
            catch
            {
                await template.CancelEditAsync(authentication);
                return null;
            }

            //var table = template.Table;
            //var content = table.Content;

            //content.BeginEdit(authentication);
            //content.EnterEdit(authentication);
            //try
            //{
            //    content.GenerateRowsAsync(authentication, RandomUtility.Next(10, 1000));
            //    content.LeaveEdit(authentication);
            //    content.EndEdit(authentication);
            //}
            //catch
            //{
            //    content.CancelEdit(authentication);
            //}

            //return table;
        }

        public static async Task<ITable> GenerateStandardChildAsync(this ITable table, Authentication authentication, string prefix, IEnumerable<string> keyTypes, IEnumerable<string> columnTypes)
        {
            var typeCollection = table.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var tableName = string.Join("_", EnumerableUtility.Friends(prefix, keyTypes));

            if (table.Childs.ContainsKey(tableName) == true)
                return null;

            var template = await table.NewTableAsync(authentication);
            await template.SetTableNameAsync(authentication, tableName);

            foreach (var item in keyTypes)
            {
                await template.AddKeyAsync(authentication, item, item);
            }

            foreach (var item in columnTypes)
            {
                await template.AddColumnAsync(authentication, item, item);
            }

            try
            {
                await template.EndEditAsync(authentication);
                return template.Target as ITable;
            }
            catch
            {
                await template.CancelEditAsync(authentication);
                return null;
            }
        }

        public static async Task GenerateStandardChildAsync(this ITable table, Authentication authentication)
        {
            var typeCollection = table.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var types1 = typeCollection.Select(item => item.Name).ToArray();
            var types2 = CremaDataTypeUtility.GetBaseTypeNames();
            var allTypes = types1.Concat(types2);
            var allKeyTypes = types1.Concat(types2).Where(item => item != typeof(bool).GetTypeName());

            var prefixes = new string[] { "SingleKey", "DoubleKey", "TripleKey", "QuadraKey", };

            for (int i = 0; i < prefixes.Length; i++)
            {
                var query = allKeyTypes.Permutations(i + 1);
                var keys = query.Random();
                var columns = allTypes.Except(keys);
                await table.GenerateStandardChildAsync(authentication, "Child_" + prefixes[i], keys, columns);
            }
        }

        //public static void GenerateStandardTable(this ITableCategory category, Authentication authentication)
        //{
        //    var types1 = category.DataBase.TypeContext.Types.Select(item => item.Name).ToArray();
        //    var types2 = CremaTypeUtility.GetBaseTypes();
        //    var allTypes = types1.Concat(types2);

        //    foreach (var item in allTypes)
        //    {
        //        var tableName = string.Join("_", "SingleKey", item);
        //        var template = category.NewTableAsync(authentication);
        //        template.SetTableName(authentication, tableName);
        //        template.SetComment(authentication, string.Format("Single Key Table : {0}", item));

        //        template.AddKey(authentication, item, item);

        //        var extraTypes = allTypes.Where(i => i != item);

        //        foreach (var i in extraTypes)
        //        {
        //            template.AddColumn(authentication, i, i);
        //        }

        //        template.EndEdit(authentication);
        //    }
        //}

        public static async Task GenerateStandardTypeAsync(this ITypeCategory category, Authentication authentication)
        {
            var template = await category.NewTypeAsync(authentication);
            await template.SetIsFlagAsync(authentication, false);
            await template.SetCommentAsync(authentication, "Standard Type");

            var az = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i).ToArray();

            await template.AddMemberAsync(authentication, "None", 0, "None Value");
            for (int i = 0; i < az.Length; i++)
            {
                await template.AddMemberAsync(authentication, az[i].ToString(), (long)i + 1, az[i] + " Value");
            }

            await template.EndEditAsync(authentication);
        }

        public static async Task GenerateStandardFlagsAsync(this ITypeCategory category, Authentication authentication)
        {
            var types = category.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var typeNames = await types.Dispatcher.InvokeAsync(() => types.Select(item => item.Name).ToArray());
            var newName = NameUtility.GenerateNewName("Flag", typeNames);
            var template = await category.NewTypeAsync(authentication);
            await template.SetTypeNameAsync(authentication, newName);
            await template.SetIsFlagAsync(authentication, true);
            await template.SetCommentAsync(authentication, "Standard Flag");

            await template.AddMemberAsync(authentication, "None", 0, "None Value");
            await template.AddMemberAsync(authentication, "A", 1, "A Value");
            await template.AddMemberAsync(authentication, "B", 2, "B Value");
            await template.AddMemberAsync(authentication, "C", 4, "C Value");
            await template.AddMemberAsync(authentication, "D", 8, "D Value");
            await template.AddMemberAsync(authentication, "AC", 1 | 4, "AC Value");
            await template.AddMemberAsync(authentication, "ABC", 1 | 2 | 4, "AC Value");
            await template.AddMemberAsync(authentication, "BD", 2 | 8, "AC Value");
            await template.AddMemberAsync(authentication, "All", 1 | 2 | 4 | 8, "All Value");

            await template.EndEditAsync(authentication);
        }

        private static async Task GenerateStandardContentAsync(this ITable table, Authentication authentication)
        {
            var content = table.Content;

            await content.BeginEditAsync(authentication);
            await content.EnterEditAsync(authentication);

            await content.GenerateRowsAsync(authentication, RandomUtility.Next(10, 1000));

            foreach (var item in content.Tables)
            {
                await item.Content.GenerateRowsAsync(authentication, RandomUtility.Next(10, 100));
            }

            try
            {
                await content.LeaveEditAsync(authentication);
                await content.EndEditAsync(authentication);
            }
            catch
            {
                await content.CancelEditAsync(authentication);
            }
        }
    }
}

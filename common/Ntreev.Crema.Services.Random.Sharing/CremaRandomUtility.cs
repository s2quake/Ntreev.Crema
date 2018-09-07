//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Data;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Random
{
    public static class CremaRandomUtility
    {
        private static TagInfo[] tags = new TagInfo[] { TagInfo.All, TagInfoUtility.Server, TagInfoUtility.Client, TagInfo.Unused };

        public static async Task GenerateAsync(this IDataBase dataBase, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                if (RandomUtility.Within(50) == true)
                    await dataBase.TypeContext.GenerateAsync(authentication);
                else
                    await dataBase.TableContext.GenerateAsync(authentication);
            }
        }

        public static async Task GenerateAsync(this IUserContext context, Authentication authentication)
        {
            if (RandomUtility.Within(25) == true)
                await context.GenerateCategoryAsync(authentication);
            else
                await context.GenerateUserAsync(authentication);
        }

        public static async Task GenerateAsync(this IUserContext context, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await context.GenerateAsync(authentication);
            }
        }

        public static Task GenerateAsync(this ITypeContext context, Authentication authentication)
        {
            if (RandomUtility.Within(25) == true)
                return context.AddRandomCategoryAsync(authentication);
            else
                return context.AddRandomTypeAsync(authentication);
        }

        public static async Task GenerateAsync(this ITypeContext context, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await context.GenerateAsync(authentication);
            }
        }

        public static async Task GenerateAsync(this ITableContext context, Authentication authentication)
        {
            if (RandomUtility.Within(25) == true)
                await context.GenerateCategoryAsync(authentication);
            else
                await context.GenerateTableAsync(authentication);
        }

        public static async Task GenerateAsync(this ITableContext context, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await context.GenerateAsync(authentication);
            }
        }

        public static async Task GenerateCategoriesAsync(this IUserContext context, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await context.GenerateCategoryAsync(authentication);
            }
        }

        public static async Task<bool> GenerateCategoryAsync(this IUserContext context, Authentication authentication)
        {
            if (RandomUtility.Within(50) == true)
            {
                await context.Root.AddNewCategoryAsync(authentication, RandomUtility.NextIdentifier());
            }
            else
            {
                var category = context.Categories.Random();
                if (GetLevel(category, (i) => i.Parent) > 4)
                    return false;
                await category.AddNewCategoryAsync(authentication, RandomUtility.NextIdentifier());
            }
            return true;
        }

        public static async Task GenerateUsersAsync(this IUserContext context, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await context.GenerateUserAsync(authentication);
            }
        }

        public static async Task<bool> GenerateUserAsync(this IUserContext context, Authentication authentication)
        {
            var category = context.Categories.Random();
            var newID = NameUtility.GenerateNewName("Test", context.Users.Select(item => item.ID).ToArray());
            var newName = newID.Replace("Test", "테스트");
            await category.AddNewUserAsync(authentication, newID, null, newName, Authority.Member);
            return true;
        }

        public static async Task GenerateCategoriesAsync(this ITableContext context, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await context.GenerateCategoryAsync(authentication);
            }
        }

        public static async Task<bool> GenerateCategoryAsync(this ITableContext context, Authentication authentication)
        {
            var categoryName = RandomUtility.NextIdentifier();
            var category = RandomUtility.Within(50) == true ? context.Root : context.Categories.Random();

            if (category.VerifyAccessType(authentication, AccessType.Master) == false)
                return false;

            if (GetLevel(category, (i) => i.Parent) > 4)
                return false;

            if (category.Categories.ContainsKey(categoryName) == true)
                return false;

            await category.AddNewCategoryAsync(authentication, categoryName);

            return true;
        }

        public static async Task GenerateTablesAsync(this ITableContext context, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await context.GenerateTableAsync(authentication);
            }
        }

        public static async Task GenerateTableAsync(this ITableContext context, Authentication authentication)
        {
            if (RandomUtility.Within(25) == true)
                await InheritTableAsync(context, authentication);
            else if (RandomUtility.Within(25) == true)
                await CopyTableAsync(context, authentication);
            else
                await CreateTableAsync(context, authentication);
        }

        public static async Task CreateTableAsync(this ITableContext context, Authentication authentication)
        {
            var category = context.Categories.Random();

            var template = await category.NewTableAsync(authentication);
            await GenerateColumnsAsync(template, authentication, RandomUtility.Next(3, 10));
            if (RandomUtility.Within(25) == true)
                await template.SetCommentAsync(authentication, RandomUtility.NextString());
            await template.EndEditAsync(authentication);

            var tables = template.Target as ITable[];
            var table = tables.First();

            while (RandomUtility.Within(10))
            {
                var childTemplate = await table.NewTableAsync(authentication);
                await GenerateColumnsAsync(childTemplate, authentication, RandomUtility.Next(3, 10));
                await childTemplate.EndEditAsync(authentication);
            }

            var content = table.Content;
            await content.EnterEditAsync(authentication);

            await GenerateRowsAsync(content, authentication, RandomUtility.Next(10, 100));

            foreach (var item in table.Childs)
            {
                await GenerateRowsAsync(item.Content, authentication, RandomUtility.Next(10, 100));
            }

            await content.LeaveEditAsync(authentication);
        }

        public static async Task InheritTableAsync(this ITableContext context, Authentication authentication)
        {
            var category = context.Categories.Random();
            var table = context.Tables.RandomOrDefault();

            if (table == null)
                return;

            await table.InheritAsync(authentication, "Table_" + RandomUtility.NextIdentifier(), category.Path, RandomUtility.NextBoolean());
        }

        public static async Task CopyTableAsync(this ITableContext context, Authentication authentication)
        {
            var category = context.Categories.Random();
            var table = context.Tables.RandomOrDefault();

            if (table == null)
                return;

            await table.CopyAsync(authentication, "Table_" + RandomUtility.NextIdentifier(), category.Path, RandomUtility.NextBoolean());
        }

        public static async Task GenerateColumnsAsync(this ITableTemplate template, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await CreateColumnAsync(template, authentication);
            }
        }

        public static async Task<bool> CreateColumnAsync(this ITableTemplate template, Authentication authentication)
        {
            var columnName = RandomUtility.NextIdentifier();

            if (await template.ContainsAsync(columnName) == true)
                return false;

            var column = await template.AddNewAsync(authentication);
            await column.SetNameAsync(authentication, columnName);

            if (template.PrimaryKey.Any() == false)
            {
                await column.SetIsKeyAsync(authentication, true);
            }
            else if (template.Count == 0 && RandomUtility.Within(10))
            {
                await column.SetIsKeyAsync(authentication, true);
                await column.SetIsUniqueAsync(authentication, RandomUtility.Within(75));
            }

            if (RandomUtility.Within(75) == true)
            {
                await column.SetTagsAsync(authentication, TagInfo.All);
            }
            else
            {
                await column.SetTagsAsync(authentication, tags.Random());
            }

            if (RandomUtility.Within(75) == true)
            {
                await column.SetDataTypeAsync(authentication, CremaDataTypeUtility.GetBaseTypeNames().Random());
            }
            else
            {
                await column.SetDataTypeAsync(authentication, template.SelectableTypes.Random());
            }

            if (RandomUtility.Within(25) == true)
            {
                await column.SetCommentAsync(authentication, RandomUtility.NextString());
            }

            if (CremaDataTypeUtility.CanUseAutoIncrement(column.DataType) == true)
            {
                await column.SetAutoIncrementAsync(authentication, RandomUtility.NextBoolean());
            }

            try
            {
                await template.EndNewAsync(authentication, column);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> ChangeColumnAsync(this ITableTemplate template, Authentication authentication)
        {
            var column = template.RandomOrDefault();

            if (column == null)
                return false;

            if (RandomUtility.Within(25) == true)
            {
                await column.SetNameAsync(authentication, RandomUtility.NextIdentifier());
            }

            if (RandomUtility.Within(75) == true)
            {
                await column.SetTagsAsync(authentication, TagInfo.All);
            }
            else
            {
                await column.SetTagsAsync(authentication, tags.Random());
            }

            if (RandomUtility.Within(25) == true)
            {
                await column.SetCommentAsync(authentication, RandomUtility.NextString());
            }

            return true;
        }

        public static async Task<bool> DeleteColumnAsync(this ITableTemplate template, Authentication authentication)
        {
            var column = template.RandomOrDefault();

            if (column == null || column.IsKey == true)
                return false;

            try
            {
                await column.DeleteAsync(authentication);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async Task EditRandomAsync(this ITableTemplate template, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                if (RandomUtility.Within(40) == true)
                    await CreateColumnAsync(template, authentication);
                else if (RandomUtility.Within(50) == true)
                    await ChangeColumnAsync(template, authentication);
                else if (RandomUtility.Within(25) == true)
                    await DeleteColumnAsync(template, authentication);
            }
        }

        public static Task EditRandomAsync(this ITableContent content, Authentication authentication)
        {
            return EditRandomAsync(content, authentication, 1);
        }

        public static async Task EditRandomAsync(this ITableContent content, Authentication authentication, int tryCount)
        {
            var failedCount = 0;
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    if (RandomUtility.Within(10) == true || content.Count < 5)
                        await CreateRowAsync(content, authentication);
                    else if (RandomUtility.Within(75) == true)
                        await ChangeRowAsync(content, authentication);
                    else if (RandomUtility.Within(10) == true)
                        await DeleteRowAsync(content, authentication);
                }
                catch
                {
                    failedCount++;
                }
                if (failedCount > 5)
                    break;
            }
        }

        public static async Task CreateRowAsync(this ITableContent content, Authentication authentication)
        {
            var table = content.Table;
            var parentContent = table.Parent.Content;
            string relationID = null;

            if (parentContent != null && parentContent.Any() == true)
            {
                relationID = parentContent.Random().RelationID;
            }

            var row = await content.AddNewAsync(authentication, relationID);

            var types = table.GetService(typeof(ITypeCollection)) as ITypeCollection;
            foreach (var item in table.TableInfo.Columns)
            {
                if (item.AutoIncrement == false)
                {
                    //row.SetField(authentication, item.Name, TypeContextExtensions.GetRandomValue(types, item));
                }
            }

            if (RandomUtility.Within(25) == true)
            {
                await row.SetTagsAsync(authentication, tags.Random());
            }

            if (RandomUtility.Within(25) == true)
            {
                await row.SetIsEnabledAsync(authentication, RandomUtility.NextBoolean());
            }

            await content.EndNewAsync(authentication, row);
        }

        public static async Task ChangeRowAsync(this ITableContent content, Authentication authentication)
        {
            var table = content.Table;
            var row = content.RandomOrDefault();

            if (row == null)
                return;

            var types = table.GetService(typeof(ITypeCollection)) as ITypeCollection;

            if (RandomUtility.Within(5) == true)
            {
                await row.SetTagsAsync(authentication, tags.Random());
            }
            else if (RandomUtility.Within(5) == true)
            {
                await row.SetIsEnabledAsync(authentication, RandomUtility.NextBoolean());
            }
            else
            {
                var columnInfo = table.TableInfo.Columns.Random();
                //row.SetField(authentication, columnInfo.Name, TypeContextExtensions.GetRandomValue(types, columnInfo));
            }
        }

        public static async Task DeleteRowAsync(this ITableContent content, Authentication authentication)
        {
            var row = content.RandomOrDefault();
            if (row == null)
                return;

            await row.DeleteAsync(authentication);
        }

        public static async Task GenerateRowsAsync(this ITableContent content, Authentication authentication, int tryCount)
        {
            int failedCount = 0;
            for (var i = 0; i < tryCount; i++)
            {
                if (await GenerateRowAsync(content, authentication) == true)
                    continue;

                failedCount++;
                if (failedCount > 5)
                    break;
            }
        }

        public static async Task<bool> GenerateRowAsync(this ITableContent content, Authentication authentication)
        {
            var row = await NewRandomRowAsync(content, authentication);

            if (FillFields(row, authentication) == true)
            {
                if (RandomUtility.Within(25) == true)
                {
                    await row.SetTagsAsync(authentication, tags.Random());
                }

                if (RandomUtility.Within(25) == true)
                {
                    await row.SetIsEnabledAsync(authentication, RandomUtility.NextBoolean());
                }

                try
                {
                    await content.EndNewAsync(authentication, row);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public static bool FillFields(this ITableRow row, Authentication authentication)
        {
            var table = row.Content.Table;
            var changed = 0;

            foreach (var item in table.TableInfo.Columns)
            {
                if (row[item.Name] != null)
                    continue;

                if (FillField(row, authentication, item) == false)
                    return false;
                changed++;
            }

            return changed > 0;
        }

        public static bool FillField(this ITableRow row, Authentication authentication, ColumnInfo columnInfo)
        {
            var content = row.Content;
            var types = content.Table.GetService(typeof(ITypeCollection)) as ITypeCollection;

            for (var i = 0; i < 20; i++)
            {
                //object value = TypeContextExtensions.GetRandomValue(types, columnInfo);

                //if (columnInfo.AllowNull == false && value == null)
                //    continue;

                ////if (Contains(content, columnInfo.Name, value) == false)
                //{
                //    row.SetField(authentication, columnInfo.Name, value);
                //    return true;
                //}
            }
            return false;
        }

        public static async Task<ITableRow> NewRandomRowAsync(this ITableContent content, Authentication authentication)
        {
            var table = content.Table;
            var parentContent = table.Parent.Content;
            if (parentContent != null)
            {
                var parentRow = parentContent.Random();
                if (parentRow == null)
                    return null;
                return await content.AddNewAsync(authentication, parentRow.RelationID);
            }
            return await content.AddNewAsync(authentication, null);
        }

        public static ITable RandomSample(this ITableCollection tables)
        {
            return tables.Random(item =>
            {
                if (item.IsPrivate == true)
                    return false;
                if (item.IsLocked == true)
                    return false;
                if (item.Childs.Any() == false)
                    return false;
                if (item.TemplatedParent != null)
                    return false;
                if (item.Category.Parent == null)
                    return false;
                return true;
            });
        }

        public static ITableCategory RandomSample(this ITableCategoryCollection categories)
        {
            return categories.Random(item =>
            {
                if (item.Parent == null)
                    return false;
                if (item.IsPrivate == true)
                    return false;
                if (item.IsLocked == true)
                    return false;
                if (item.Tables.Any(i => i.TemplatedParent == null && i.Childs.Any()) == false)
                    return false;
                return true;
            });
        }

        public static IType RandomSample(this ITypeCollection types)
        {
            return types.Random(item =>
            {
                if (item.IsPrivate == true)
                    return false;
                if (item.IsLocked == true)
                    return false;
                if (item.Category.Parent == null)
                    return false;
                return true;
            });
        }

        public static ITypeCategory RandomSample(this ITypeCategoryCollection categories)
        {
            return categories.Random(item =>
            {
                if (item.Parent == null)
                    return false;
                if (item.IsPrivate == true)
                    return false;
                if (item.IsLocked == true)
                    return false;
                if (item.Types.Any() == false)
                    return false;
                return true;
            });
        }

        public static IDataBase RandomDataBase(this ICremaHost cremaHost)
        {
            var dataBase = cremaHost.DataBases.Random();
            //if (dataBase.IsLoaded == false)
            //    dataBase.Load();
            return dataBase;
        }

        public static TagInfo RandomTags()
        {
            return tags.Random();
        }

        private static bool Contains(ITableContent content, string columnName, object value)
        {
            foreach (var item in content)
            {
                if (object.Equals(item[columnName], value) == true)
                    return true;
            }
            return false;
        }

        private static int GetLevel<T>(T category, Func<T, T> parentFunc)
        {
            int level = 0;

            var parent = parentFunc(category);
            while (parent != null)
            {
                level++;
                parent = parentFunc(parent);
            }
            return level;
        }
    }
}

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

using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class TableContextExtensions
    {
        public static async Task AddRandomItemsAsync(this ITableContext tableContext, Authentication authentication, DataBaseSettings settings)
        {
            await AddRandomCategoriesAsync(tableContext, authentication, settings);
            await AddRandomTablesAsync(tableContext, authentication, settings);
            await AddRandomChildTablesAsync(tableContext, authentication, settings);
            await AddRandomDerivedTablesAsync(tableContext, authentication, settings);
        }

        public static async Task AddRandomCategoriesAsync(this ITableContext tableContext, Authentication authentication, DataBaseSettings settings)
        {
            var minCount = settings.TableContext.MinTableCategoryCount;
            var maxCount = settings.TableContext.MaxTableCategoryCount;
            var count = RandomUtility.Next(minCount, maxCount);
            var tableCategoryCollection = tableContext.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            while (await tableCategoryCollection.GetCountAsync() < count)
            {
                await tableContext.AddRandomCategoryAsync(authentication);
            }
        }

        public static Task<ITableCategory> AddRandomCategoryAsync(this ITableCategory category, Authentication authentication)
        {
            var categoryName = RandomUtility.NextIdentifier();
            return category.AddNewCategoryAsync(authentication, categoryName);
        }

        public static Task<ITableCategory> AddRandomCategoryAsync(this ITableContext tableContext, Authentication authentication)
        {
            if (RandomUtility.Within(33) == true)
            {
                return tableContext.Root.AddRandomCategoryAsync(authentication);
            }
            else
            {
                var category = tableContext.Categories.Random();
                if (GetLevel(category, (i) => i.Parent) > 4)
                    return null;
                return category.AddRandomCategoryAsync(authentication);
            }
        }

        public static async Task AddRandomTablesAsync(this ITableContext tableContext, Authentication authentication, DataBaseSettings settings)
        {
            var minCount = settings.TableContext.MinTableCount;
            var maxCount = settings.TableContext.MaxTableCount;
            var count = RandomUtility.Next(minCount, maxCount);
            var tableCollection = tableContext.GetService(typeof(ITableCollection)) as ITableCollection;
            while (await tableCollection.GetCountAsync() < count)
            {
                await AddRandomTableAsync(tableContext, authentication, settings);
            }
        }

        public static Task<ITable> AddRandomTableAsync(this ITableContext tableContext, Authentication authentication, DataBaseSettings settings)
        {
            var category = tableContext.Categories.Random();
            return AddRandomTableAsync(category, authentication, settings);
        }

        public static async Task<ITable> AddRandomTableAsync(this ITableCategory category, Authentication authentication, DataBaseSettings settings)
        {
            var template = await category.NewTableAsync(authentication);
            await template.InitializeRandomAsync(authentication);
            await template.EndEditAsync(authentication);

            if (template.Target is ITable[] tables)
            {
                foreach (var item in tables)
                {
                    var minCount = settings.TableContext.MinRowCount;
                    var maxCount = settings.TableContext.MaxRowCount;
                    var count = RandomUtility.Next(minCount, maxCount);
                    await item.AddRandomRowsAsync(authentication, count);
                }
                return tables.First();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task AddRandomDerivedTablesAsync(this ITableContext tableContext, Authentication authentication, DataBaseSettings settings)
        {
            var minCount = settings.TableContext.MinDerivedTableCount;
            var maxCount = settings.TableContext.MaxDerivedTableCount;
            var count = RandomUtility.Next(minCount, maxCount);
            while (await GetDerivedTableCountAsync() < count)
            {
                await AddRandomDerivedTableAsync(tableContext, authentication);
            }

            Task<int> GetDerivedTableCountAsync()
            {
                var tableCollection = tableContext.GetService(typeof(ITableCollection)) as ITableCollection;
                return tableCollection.Dispatcher.InvokeAsync(() =>
                {
                    return tableCollection.Where(item => item.TemplatedParent != null).Count();
                });
            }
        }

        public static Task<ITable[]> AddRandomDerivedTableAsync(this ITableContext tableContext, Authentication authentication)
        {
            var category = tableContext.Categories.Random();
            return AddRandomDerivedTableAsync(category, authentication);
        }

        public static async Task<ITable[]> AddRandomDerivedTableAsync(this ITableCategory category, Authentication authentication)
        {
            var tableName = RandomUtility.NextIdentifier();
            var copyData = RandomUtility.NextBoolean();
            var tableContext = category.GetService(typeof(ITableContext)) as ITableContext;
            var table = tableContext.Tables.Random(item => item.TemplatedParent == null && item.Parent == null);
            return await table.InheritAsync(authentication, tableName, category.Path, copyData);
        }

        public static async Task AddRandomChildTablesAsync(this ITableContext tableContext, Authentication authentication, DataBaseSettings settings)
        {
            var minCount = settings.TableContext.MinChildTableCount;
            var maxCount = settings.TableContext.MaxChildTableCount;
            var count = RandomUtility.Next(minCount, maxCount);
            while (await GetChildTableCountAsync() < count)
            {
                await AddRandomChildTableAsync(tableContext, authentication, settings);
            }

            Task<int> GetChildTableCountAsync()
            {
                var tableCollection = tableContext.GetService(typeof(ITableCollection)) as ITableCollection;
                return tableCollection.Dispatcher.InvokeAsync(() =>
                {
                    return tableCollection.Where(item => item.Parent != null && item.TemplatedParent == null).Count();
                });
            }
        }

        public static Task<ITable> AddRandomChildTableAsync(this ITableContext tableContext, Authentication authentication, DataBaseSettings settings)
        {
            var table = tableContext.Tables.Random(item => item.TemplatedParent == null && item.Parent == null);
            return table.AddRandomChildTableAsync(authentication, settings);
        }

        public static Task<ITableItem> GetRandomTableItemAsync(this ITableContext tableContext)
        {
            return GetRandomTableItemAsync(tableContext, DefaultPredicate);
        }

        public static Task<ITableItem> GetRandomTableItemAsync(this ITableContext tableContext, Func<ITableItem, bool> predicate)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.Random(predicate));
        }

        private static bool DefaultPredicate(ITableItem _) => true;

        private static int GetLevel<T>(T category, Func<T, T> parentFunc)
        {
            var level = 0;

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

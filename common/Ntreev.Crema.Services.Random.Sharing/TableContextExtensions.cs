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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Data;
using Ntreev.Crema.Services;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Random;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Ntreev.Library;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Random
{
    public static class TableContextExtensions
    {
        static TableContextExtensions()
        {
            MinTableCount = 10;
            MaxTableCount = 200;
            MinTableCategoryCount = 1;
            MaxTableCategoryCount = 20;
            MinRowCount = 100;
            MaxRowCount = 10000;
        }

        public static async Task AddRandomItemsAsync(this ITableContext tableContext, Authentication authentication)
        {
            await AddRandomCategoriesAsync(tableContext, authentication);
            await AddRandomTablesAsync(tableContext, authentication);
            await AddRandomChildTablesAsync(tableContext, authentication, 10);
            await AddRandomDerivedTablesAsync(tableContext, authentication, 10);
        }

        public static Task AddRandomCategoriesAsync(this ITableContext tableContext, Authentication authentication)
        {
            return AddRandomCategoriesAsync(tableContext, authentication, RandomUtility.Next(MinTableCategoryCount, MaxTableCategoryCount));
        }

        public static async Task AddRandomCategoriesAsync(this ITableContext tableContext, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
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

        public static Task AddRandomTablesAsync(this ITableContext tableContext, Authentication authentication)
        {
            return AddRandomTablesAsync(tableContext, authentication, RandomUtility.Next(MinTableCount, MaxTableCount));
        }

        public static async Task AddRandomTablesAsync(this ITableContext tableContext, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await AddRandomTableAsync(tableContext, authentication);
            }
        }

        public static Task<ITable> AddRandomTableAsync(this ITableContext tableContext, Authentication authentication)
        {
            var category = tableContext.Categories.Random();
            return AddRandomTableAsync(category, authentication);
        }

        public static async Task<ITable> AddRandomTableAsync(this ITableCategory category, Authentication authentication)
        {
            var template = await category.NewTableAsync(authentication);
            await template.InitializeRandomAsync(authentication);
            await template.EndEditAsync(authentication);

            if (template.Target is ITable[] tables)
            {
                foreach (var item in tables)
                {
                    await AddRandomRowsAsync(item, authentication, RandomUtility.Next(MinRowCount, MaxRowCount));
                }
                return tables.First();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task AddRandomDerivedTablesAsync(this ITableContext tableContext, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await AddRandomDerivedTableAsync(tableContext, authentication);
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

        public static async Task AddRandomChildTablesAsync(this ITableContext tableContext, Authentication authentication, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                await AddRandomChildTableAsync(tableContext, authentication);
            }
        }

        public static Task<ITable> AddRandomChildTableAsync(this ITableContext tableContext, Authentication authentication)
        {
            var table = tableContext.Tables.Random(item => item.TemplatedParent == null && item.Parent == null);
            return AddRandomChildTableAsync(table, authentication);
        }

        public static async Task<ITable> AddRandomChildTableAsync(this ITable table, Authentication authentication)
        {
            var copyData = RandomUtility.NextBoolean();
            var template = await table.NewTableAsync(authentication);
            await template.InitializeRandomAsync(authentication);
            await template.EndEditAsync(authentication);
            if (template.Target is ITable[] tables)
            {
                foreach (var item in tables)
                {
                    await AddRandomRowsAsync(item, authentication, RandomUtility.Next(MinRowCount, MaxRowCount));
                }
                return tables.First();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task AddRandomRowsAsync(this ITable table, Authentication authentication, int tryCount)
        {
            var target = table.Parent ?? table;
            var targetContent = target.Content;
            await targetContent.BeginEditAsync(authentication);
            await targetContent.EnterEditAsync(authentication);
            var failedCount = 0;
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    var parentRow = target != table ? targetContent.RandomOrDefault() : null;
                    await AddRandomRowAsync(table, parentRow, authentication);
                }
                catch
                {
                    failedCount++;
                }
                if (failedCount > 3)
                    break;
            }
            await targetContent.LeaveEditAsync(authentication);
            await targetContent.EndEditAsync(authentication);
        }

        public static async Task<ITableRow> AddRandomRowAsync(this ITable table, ITableRow parentRow, Authentication authentication)
        {
            if (table.Parent != null && parentRow == null)
                return null;

            var tableRow = await table.Content.AddNewAsync(authentication, parentRow?.RelationID);
            await tableRow.InitializeRandomAsync(authentication);
            await table.Content.EndNewAsync(authentication, tableRow);
            return tableRow;
        }

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

        public static int MinTableCount { get; set; }

        public static int MaxTableCount { get; set; }

        public static int MinTableCategoryCount { get; set; }

        public static int MaxTableCategoryCount { get; set; }

        public static int MinRowCount { get; set; }

        public static int MaxRowCount { get; set; }
    }
}

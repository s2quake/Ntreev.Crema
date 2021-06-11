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

using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class TableContextExtensions
    {
        public static Task<bool> ContainsAsync(this ITableContext tableContext, string itemPath)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.Contains(itemPath));
        }

        public static Task<bool> ContainsTableAsync(this ITableContext tableContext, string tableName)
        {
            if (tableContext.GetService(typeof(ITableCollection)) is ITableCollection tableCollection)
            {
                return tableCollection.ContainsAsync(tableName);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsTableCategoryAsync(this ITableContext tableContext, string categoryPath)
        {
            if (tableContext.GetService(typeof(ITableCategoryCollection)) is ITableCategoryCollection tableCategoryCollection)
            {
                return tableCategoryCollection.ContainsAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITableItem> GetTableItemAsync(this ITableContext tableContext, string itemPath)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext[itemPath]);
        }

        public static Task<ITableItem[]> GetTableItemsAsync(this ITableContext tableContext)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ToArray());
        }

        public static Task<ITable> GetTableAsync(this ITableContext tableContext, string tableName)
        {
            if (tableContext.GetService(typeof(ITableCollection)) is ITableCollection tableCollection)
            {
                return tableCollection.GetTableAsync(tableName);
            }
            throw new NotImplementedException();
        }

        public static Task<ITable[]> GetTablesAsync(this ITableContext tableContext)
        {
            if (tableContext.GetService(typeof(ITableCollection)) is ITableCollection tableCollection)
            {
                return tableCollection.GetTablesAsync();
            }
            throw new NotImplementedException();
        }

        public static Task<ITableCategory> GetTableCategoryAsync(this ITableContext tableContext, string categoryPath)
        {
            if (tableContext.GetService(typeof(ITableCategoryCollection)) is ITableCategoryCollection tableCategoryCollection)
            {
                return tableCategoryCollection.GetCategoryAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITableCategory[]> GetTableCategoriesAsync(this ITableContext tableContext)
        {
            if (tableContext.GetService(typeof(ITableCategoryCollection)) is ITableCategoryCollection tableCategoryCollection)
            {
                return tableCategoryCollection.GetCategoriesAsync();
            }
            throw new NotImplementedException();
        }

        public static Task AddItemsCreatedAsync(this ITableContext tableContext, ItemsCreatedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsCreated += handler);
        }

        public static Task AddItemsRenamedAsync(this ITableContext tableContext, ItemsRenamedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsRenamed += handler);
        }

        public static Task AddItemsMovedAsync(this ITableContext tableContext, ItemsMovedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsMoved += handler);
        }

        public static Task AddItemsDeletedAsync(this ITableContext tableContext, ItemsDeletedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsDeleted += handler);
        }

        public static Task AddItemsChangedAsync(this ITableContext tableContext, ItemsEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsChanged += handler);
        }

        public static Task AddItemsAccessChangedAsync(this ITableContext tableContext, ItemsEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsAccessChanged += handler);
        }

        public static Task AddItemsLockChangedAsync(this ITableContext tableContext, ItemsEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsLockChanged += handler);
        }

        public static Task RemoveItemsCreatedAsync(this ITableContext tableContext, ItemsCreatedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsCreated -= handler);
        }

        public static Task RemoveItemsRenamedAsync(this ITableContext tableContext, ItemsRenamedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsRenamed -= handler);
        }

        public static Task RemoveItemsMovedAsync(this ITableContext tableContext, ItemsMovedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsMoved -= handler);
        }

        public static Task RemoveItemsDeletedAsync(this ITableContext tableContext, ItemsDeletedEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsDeleted -= handler);
        }

        public static Task RemoveItemsChangedAsync(this ITableContext tableContext, ItemsEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsChanged -= handler);
        }

        public static Task RemoveItemsAccessChangedAsync(this ITableContext tableContext, ItemsEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsAccessChanged -= handler);
        }

        public static Task RemoveItemsLockChangedAsync(this ITableContext tableContext, ItemsEventHandler<ITableItem> handler)
        {
            return tableContext.Dispatcher.InvokeAsync(() => tableContext.ItemsLockChanged -= handler);
        }
    }
}

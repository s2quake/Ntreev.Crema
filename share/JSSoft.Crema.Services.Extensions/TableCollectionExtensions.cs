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

using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class TableCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this ITableCollection tableCollection, string tableName)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.Contains(tableName));
        }

        public static Task<ITable> GetTableAsync(this ITableCollection tableCollection, string tableName)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection[tableName]);
        }

        public static Task<ITable[]> GetTablesAsync(this ITableCollection tableCollection)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.ToArray());
        }

        public static Task AddTablesStateChangedAsync(this ITableCollection tableCollection, ItemsEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesStateChanged += handler);
        }

        public static Task AddTablesChangedAsync(this ITableCollection tableCollection, ItemsChangedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesChanged += handler);
        }

        public static Task AddTablesCreatedAsync(this ITableCollection tableCollection, ItemsCreatedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesCreated += handler);
        }

        public static Task AddTablesMovedAsync(this ITableCollection tableCollection, ItemsMovedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesMoved += handler);
        }

        public static Task AddTablesRenamedAsync(this ITableCollection tableCollection, ItemsRenamedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesRenamed += handler);
        }

        public static Task AddTablesDeletedAsync(this ITableCollection tableCollection, ItemsDeletedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesDeleted += handler);
        }

        public static Task RemoveTablesStateChangedAsync(this ITableCollection tableCollection, ItemsEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesStateChanged -= handler);
        }

        public static Task RemoveTablesChangedAsync(this ITableCollection tableCollection, ItemsChangedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesChanged -= handler);
        }

        public static Task RemoveTablesCreatedAsync(this ITableCollection tableCollection, ItemsCreatedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesCreated -= handler);
        }

        public static Task RemoveTablesMovedAsync(this ITableCollection tableCollection, ItemsMovedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesMoved -= handler);
        }

        public static Task RemoveTablesRenamedAsync(this ITableCollection tableCollection, ItemsRenamedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesRenamed -= handler);
        }

        public static Task RemoveTablesDeletedAsync(this ITableCollection tableCollection, ItemsDeletedEventHandler<ITable> handler)
        {
            return tableCollection.Dispatcher.InvokeAsync(() => tableCollection.TablesDeleted -= handler);
        }
    }
}

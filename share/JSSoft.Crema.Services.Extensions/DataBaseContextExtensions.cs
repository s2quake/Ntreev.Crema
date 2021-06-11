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
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;

namespace JSSoft.Crema.Services.Extensions
{
    public static class DataBaseContextExtensions
    {
        public static Task<DataBaseContextMetaData> GetMetaDataAsync(this IDataBaseContext dataBaseContext, Authentication authentication)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.GetMetaData(authentication));
        }

        public static Task<bool> ContainsAsync(this IDataBaseContext dataBaseContext, string dataBaseName)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Contains(dataBaseName));
        }

        public static Task<int> GetCountAsync(this IDataBaseContext dataBaseContext)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Count);
        }

        public static Task<IDataBase> GetDataBaseAsync(this IDataBaseContext dataBaseContext, string dataBaseName)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[dataBaseName]);
        }

        public static Task<IDataBase> GetDataBaseAsync(this IDataBaseContext dataBaseContext, Guid dataBaseID)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext[dataBaseID]);
        }

        public static Task<IDataBase[]> GetDataBasesAsync(this IDataBaseContext dataBaseContext)
        {
            return GetDataBasesAsync(dataBaseContext, DefaultPredicate);
        }

        public static Task<IDataBase[]> GetDataBasesAsync(this IDataBaseContext dataBaseContext, Func<IDataBase, bool> predicate)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in dataBaseContext
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }

        public static Task<IDataBase[]> GetDataBasesAsync(this IDataBaseContext dataBaseContext, DataBaseState dataBaseState)
        {
            return GetDataBasesAsync(dataBaseContext, dataBaseState, DefaultPredicate);
        }

        public static Task<IDataBase[]> GetDataBasesAsync(this IDataBaseContext dataBaseContext, DataBaseState dataBaseState, Func<IDataBase, bool> predicate)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in dataBaseContext
                            where item.DataBaseState == dataBaseState && predicate(item)
                            select item;
                return query.ToArray();
            });
        }

        public static Task<string> GenerateNewDataBaseNameAsync(this IDataBaseContext dataBaseContext)
        {
            return GenerateNewDataBaseNameAsync(dataBaseContext, "database");
        }

        public static Task<string> GenerateNewDataBaseNameAsync(this IDataBaseContext dataBaseContext, string dataBaseName)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => NameUtility.GenerateNewName(dataBaseName, dataBaseContext.Select(item => item.Name)));
        }

        public static Task AddItemsCreatedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsCreatedEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsCreated += handler);
        }

        public static Task AddItemsRenamedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsRenamedEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsRenamed += handler);
        }

        public static Task AddItemsDeletedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsDeletedEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsDeleted += handler);
        }

        public static Task AddItemsLoadedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsLoaded += handler);
        }

        public static Task AddItemsUnloadedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsUnloaded += handler);
        }

        public static Task AddItemsResettingEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsResetting += handler);
        }

        public static Task AddItemsResetEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsReset += handler);
        }

        public static Task AddItemsAuthenticationEnteredEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsAuthenticationEntered += handler);
        }

        public static Task AddItemsAuthenticationLeftEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsAuthenticationLeft += handler);
        }

        public static Task AddItemsInfoChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsInfoChanged += handler);
        }

        public static Task AddItemsStateChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsStateChanged += handler);
        }

        public static Task AddItemsAccessChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsAccessChanged += handler);
        }

        public static Task AddItemsLockChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsLockChanged += handler);
        }

        public static Task AddTaskCompletedEventHandlerAsync(this IDataBaseContext dataBaseContext, TaskCompletedEventHandler handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.TaskCompleted += handler);
        }
        public static Task RemoveItemsCreatedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsCreatedEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsCreated -= handler);
        }

        public static Task RemoveItemsRenamedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsRenamedEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsRenamed -= handler);
        }

        public static Task RemoveItemsDeletedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsDeletedEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsDeleted -= handler);
        }

        public static Task RemoveItemsLoadedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsLoaded -= handler);
        }

        public static Task RemoveItemsUnloadedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsUnloaded -= handler);
        }

        public static Task RemoveItemsResettingEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsResetting -= handler);
        }

        public static Task RemoveItemsResetEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsReset -= handler);
        }

        public static Task RemoveItemsAuthenticationEnteredEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsAuthenticationEntered -= handler);
        }

        public static Task RemoveItemsAuthenticationLeftEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsAuthenticationLeft -= handler);
        }

        public static Task RemoveItemsInfoChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsInfoChanged -= handler);
        }

        public static Task RemoveItemsStateChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsStateChanged -= handler);
        }

        public static Task RemoveItemsAccessChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsAccessChanged -= handler);
        }

        public static Task RemoveItemsLockChangedEventHandlerAsync(this IDataBaseContext dataBaseContext, ItemsEventHandler<IDataBase> handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.ItemsLockChanged -= handler);
        }

        public static Task RemoveTaskCompletedEventHandlerAsync(this IDataBaseContext dataBaseContext, TaskCompletedEventHandler handler)
        {
            return dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.TaskCompleted -= handler);
        }

        private static bool DefaultPredicate(IDataBase _) => true;
    }
}

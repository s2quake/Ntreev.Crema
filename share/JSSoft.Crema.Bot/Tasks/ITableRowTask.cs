﻿// Released under the MIT License.
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
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITableRowTask))]
    [TaskClass]
    class ITableRowTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var row = context.Target as ITableRow;
            var content = row.Content;
            if (context.IsCompleted(row) == true)
            {
                try
                {
                    if (context.AllowException == false)
                    {
                        if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                        {
                            var tableInfo = await content.Dispatcher.InvokeAsync(() => content.Table.TableInfo);
                            var keys = tableInfo.Columns.Where(item => item.IsKey).Select(item => item.Name).ToArray();
                            var keyFilterExpression = await row.GenerateFilterExpressionAsync(keys);
                            if ((await content.SelectAsync(authentication, keyFilterExpression)).Any() == true)
                                return;

                            var uniques = tableInfo.Columns.Where(item => item.IsUnique && item.IsKey == false).Select(item => item.Name).ToArray();
                            foreach (var item in uniques)
                            {
                                var itemExpression = await row.GenerateFilterExpressionAsync(item);
                                if ((await content.SelectAsync(authentication, itemExpression)).Any() == true)
                                    return;
                            }

                            var allowNulls = tableInfo.Columns.Where(item => item.AllowNull).Select(item => item.Name).ToArray();
                            foreach (var item in allowNulls)
                            {
                                if (row[item] == null)
                                    return;
                            }
                        }
                    }
                    if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                    {
                        await content.EndNewAsync(authentication, row);
                    }
                }
                catch
                {

                }
                finally
                {
                    context.State = null;
                    context.Pop(row);
                }
            }
        }

        public Type TargetType => typeof(ITableRow);

        public bool IsEnabled => false;

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITableRow row, TaskContext context)
        {
            var authentication = context.Authentication;
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == false)
            {
                await row.DeleteAsync(authentication);
                context.State = System.Data.DataRowState.Deleted;
                context.Pop(row);
            }
        }

        [TaskMethod(Weight = 5)]
        public async Task SetIsEnabledAsync(ITableRow row, TaskContext context)
        {
            var authentication = context.Authentication;
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            var isEnabled = RandomUtility.NextBoolean();
            await row.SetIsEnabledAsync(authentication, isEnabled);
        }

        [TaskMethod(Weight = 5)]
        public async Task SetTagsAsync(ITableRow row, TaskContext context)
        {
            var authentication = context.Authentication;
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            var tags = TagInfoUtility.Names.Random();
            await row.SetTagsAsync(authentication, (TagInfo)tags);
        }

        [TaskMethod]
        public async Task SetFieldAsync(ITableRow row, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            await Task.Delay(0);
        }

        [TaskMethod]
        public async Task MoveLeftAsync(ITableRow row, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                return;
            await Task.Delay(0);
        }
    }
}

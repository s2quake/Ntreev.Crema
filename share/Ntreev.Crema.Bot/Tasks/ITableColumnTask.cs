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

using Ntreev.Crema.Data;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Random;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITableColumnTask))]
    [TaskClass]
    public class ITableColumnTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var column = context.Target as ITableColumn;
            if (context.IsCompleted(column) == true)
            {
                var template = column.Template;
                if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                {
                    if (await template.Dispatcher.InvokeAsync(() => template.Any()) == false)
                    {
                        await column.SetIsKeyAsync(authentication, true);
                    }
                    try
                    {
                        await template.EndNewAsync(authentication, column);
                    }
                    catch
                    {

                    }
                }
                context.State = null;
                context.Pop(column);
            }
        }

        public Type TargetType => typeof(ITableColumn);

        public bool IsEnabled => false;

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == false)
            {
                await column.DeleteAsync(authentication);
                context.State = System.Data.DataRowState.Deleted;
                context.Complete(column);
            }
        }

        [TaskMethod]
        public async Task SetIndexAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (object.Equals(context.State, System.Data.DataRowState.Detached) == true)
                    return;
            }
            var index = RandomUtility.Next(column.Template.Count);
            await column.SetIndexAsync(authentication, index);
        }

        [TaskMethod(Weight = 20)]
        public async Task SetIsKeyAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var isKey = RandomUtility.NextBoolean();
            await column.SetIsKeyAsync(authentication, isKey);
        }

        [TaskMethod]
        public async Task SetIsUniqueAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var isUnique = RandomUtility.NextBoolean();
            if (context.AllowException == false)
            {
                if (isUnique == true && column.DataType == typeof(bool).GetTypeName())
                    return;
                var template = column.Template;
                if (isUnique == false && column.IsKey == true && template.Count(item => item.IsKey) == 1)
                    return;
            }
            await column.SetIsUniqueAsync(authentication, isUnique);
        }

        [TaskMethod]
        public async Task SetNameAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var columnName = RandomUtility.NextIdentifier();
            await column.SetNameAsync(authentication, columnName);
        }

        [TaskMethod]
        public async Task SetDataTypeAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var template = column.Template;
            if (RandomUtility.Within(75) == true)
            {
                var dataType = CremaDataTypeUtility.GetBaseTypeNames().Random();
                if (Verify(dataType) == false)
                    return;
                await column.SetDataTypeAsync(authentication, dataType);
            }
            else
            {
                var dataType = template.SelectableTypes.Random();
                if (Verify(dataType) == false)
                    return;
                await column.SetDataTypeAsync(authentication, dataType);
            }

            bool Verify(string dataType)
            {
                if (context.AllowException == true)
                    return true;
                if (column.AutoIncrement == true && CremaDataTypeUtility.CanUseAutoIncrement(column.DataType) == false)
                    return false;
                return true;
            }
        }

        //[TaskMethod]
        public async Task SetDefaultValueAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var defaultValue = await column.GetRandomStringAsync();
            await column.SetDefaultValueAsync(authentication, defaultValue);
        }

        [TaskMethod]
        public async Task SetCommentAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            await column.SetCommentAsync(authentication, comment);
        }

        [TaskMethod]
        public async Task SetAutoIncrementAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var autoIncrement = RandomUtility.NextBoolean();
            if (context.AllowException == false)
            {
                if (autoIncrement == true && CremaDataTypeUtility.CanUseAutoIncrement(column.DataType) == false)
                    return;
            }
            await column.SetAutoIncrementAsync(authentication, autoIncrement);
        }

        [TaskMethod]
        public async Task SetTagsAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var tags = (TagInfo)TagInfoUtility.Names.Random();
            await column.SetTagsAsync(authentication, tags);
        }

        [TaskMethod]
        public async Task SetIsReadOnlyAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            if (column.IsKey == false || RandomUtility.Within(55) == true)
            {
                var isReadOnly = RandomUtility.NextBoolean();
                await column.SetIsReadOnlyAsync(authentication, isReadOnly);
            }
        }

        [TaskMethod]
        public async Task SetAllowNullAsync(ITableColumn column, TaskContext context)
        {
            var authentication = context.Authentication;
            var allowNull = RandomUtility.NextBoolean();
            if (context.AllowException == false)
            {
                if (await column.Dispatcher.InvokeAsync(() => column.IsKey == true && allowNull == true) == true)
                    return;
            }
            await column.SetAllowNullAsync(authentication, allowNull);
        }
    }
}

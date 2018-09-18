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
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Ntreev.Crema.Services.Random;
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
                        await template.EndNewAsync(context.Authentication, column);
                    }
                    catch
                    {

                    }
                }
                context.State = null;
                context.Pop(column);
            }
        }

        public Type TargetType
        {
            get { return typeof(ITableColumn); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITableColumn column, TaskContext context)
        {
            if (object.Equals(context.State, System.Data.DataRowState.Detached) == false)
            {
                await column.DeleteAsync(context.Authentication);
                context.State = System.Data.DataRowState.Deleted;
                context.Complete(column);
            }
        }

        [TaskMethod]
        public async Task SetIndexAsync(ITableColumn column, TaskContext context)
        {
            var index = RandomUtility.Next(column.Template.Count);
            await column.SetIndexAsync(context.Authentication, index);
        }

        [TaskMethod(Weight = 20)]
        public async Task SetIsKeyAsync(ITableColumn column, TaskContext context)
        {
            var isKey = RandomUtility.NextBoolean();
            await column.SetIsKeyAsync(context.Authentication, isKey);
        }

        [TaskMethod]
        public async Task SetIsUniqueAsync(ITableColumn column, TaskContext context)
        {
            var isUnique = RandomUtility.NextBoolean();
            if (context.AllowException == false)
            {
                if (isUnique == true && column.DataType == typeof(bool).GetTypeName())
                    return;
                var template = column.Template;
                if (isUnique == false && column.IsKey == true && template.Count(item => item.IsKey) == 1)
                    return;
            }
            await column.SetIsUniqueAsync(context.Authentication, isUnique);
        }

        [TaskMethod]
        public async Task SetNameAsync(ITableColumn column, TaskContext context)
        {
            var columnName = RandomUtility.NextIdentifier();
            await column.SetNameAsync(context.Authentication, columnName);
        }

        [TaskMethod]
        public async Task SetDataTypeAsync(ITableColumn column, TaskContext context)
        {
            var template = column.Template;
            if (RandomUtility.Within(75) == true)
            {
                var dataType = CremaDataTypeUtility.GetBaseTypeNames().Random();
                if (Verify(dataType) == false)
                    return;
                await column.SetDataTypeAsync(context.Authentication, dataType);
            }
            else
            {
                var dataType = template.SelectableTypes.Random();
                if (Verify(dataType) == false)
                    return;
                await column.SetDataTypeAsync(context.Authentication, dataType);
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
            var defaultValue = await column.GetRandomStringAsync();
            await column.SetDefaultValueAsync(context.Authentication, defaultValue);
        }

        [TaskMethod]
        public async Task SetCommentAsync(ITableColumn column, TaskContext context)
        {
            var comment = RandomUtility.NextString();
            await column.SetCommentAsync(context.Authentication, comment);
        }

        [TaskMethod]
        public async Task SetAutoIncrementAsync(ITableColumn column, TaskContext context)
        {
            var autoIncrement = RandomUtility.NextBoolean();
            if (context.AllowException == false)
            {
                if (autoIncrement == true && CremaDataTypeUtility.CanUseAutoIncrement(column.DataType) == false)
                    return;
            }
            await column.SetAutoIncrementAsync(context.Authentication, autoIncrement);
        }

        [TaskMethod]
        public async Task SetTagsAsync(ITableColumn column, TaskContext context)
        {
            var tags = (TagInfo)TagInfoUtility.Names.Random();
            await column.SetTagsAsync(context.Authentication, tags);
        }

        [TaskMethod]
        public async Task SetIsReadOnlyAsync(ITableColumn column, TaskContext context)
        {
            if (column.IsKey == false || RandomUtility.Within(55) == true)
            {
                var isReadOnly = RandomUtility.NextBoolean();
                await column.SetIsReadOnlyAsync(context.Authentication, isReadOnly);
            }
        }

        [TaskMethod]
        public async Task SetAllowNullAsync(ITableColumn column, TaskContext context)
        {
            var allowNull = RandomUtility.NextBoolean();
            await column.SetAllowNullAsync(context.Authentication, allowNull);
        }
    }
}

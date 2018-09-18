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
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITableTask))]
    [TaskClass]
    public class ITableTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var table = context.Target as ITable;
            if (context.IsCompleted(table) == true)
            {
                context.Pop(table);
            }
            //else if (RandomUtility.Within(75) == true)
            //{
            //    var content = await table.Dispatcher.InvokeAsync(() => table.Content);
            //    context.Push(content, RandomUtility.Next(100));
            //}
            else if (RandomUtility.Within(75) == true)
            {
                var template = await table.Dispatcher.InvokeAsync(() => table.Template);
                context.Push(template);
            }
        }

        public Type TargetType
        {
            get { return typeof(ITable); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod]
        public void GetAccessType(ITable table, TaskContext context)
        {
            table.Dispatcher.Invoke(() =>
            {
                table.GetAccessType(context.Authentication);
            });
        }

        [TaskMethod(Weight = 10)]
        public async Task LockAsync(ITable table, TaskContext context)
        {
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (string.IsNullOrEmpty(comment) == true)
                    return;
                var lockInfo = await table.Dispatcher.InvokeAsync(() => table.LockInfo);
                if (lockInfo.IsLocked == true || lockInfo.IsInherited == true)
                    return;
            }

            await table.LockAsync(context.Authentication, comment);
        }

        [TaskMethod]
        public async Task UnlockAsync(ITable table, TaskContext context)
        {
            if (context.AllowException == false)
            {
                var lockInfo = await table.Dispatcher.InvokeAsync(() => table.LockInfo);
                if (lockInfo.IsLocked == false || lockInfo.IsInherited == true)
                    return;
            }
            await table.UnlockAsync(context.Authentication);
        }

        [TaskMethod]
        public async Task SetPublicAsync(ITable table, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == false)
                    return;
            }

            await table.SetPublicAsync(context.Authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(ITable table, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == true)
                    return;
            }
            await table.SetPrivateAsync(context.Authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(ITable table, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == false)
                    return;
            }
            var userContext = table.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            var accessType = RandomUtility.NextEnum<AccessType>();
            await table.AddAccessMemberAsync(context.Authentication, memberID, accessType);
        }

        [TaskMethod]
        public async Task RemoveAccessMemberAsync(ITable table, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == false)
                    return;
            }
            var userContext = table.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            await table.RemoveAccessMemberAsync(context.Authentication, memberID);
        }

        [TaskMethod]
        public async Task RenameAsync(ITable table, TaskContext context)
        {
            if (context.AllowException == false)
            {
                var tableState = await table.Dispatcher.InvokeAsync(() => table.TableState);
                if (tableState != TableState.None)
                    return;
            }
            var tableName = RandomUtility.NextIdentifier();
            await table.RenameAsync(context.Authentication, tableName);
        }

        [TaskMethod]
        public async Task MoveAsync(ITable table, TaskContext context)
        {
            var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.Category.Path) == categoryPath)
                    return;
            }
            await table.MoveAsync(context.Authentication, categoryPath);
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITable table, TaskContext context)
        {
            await Task.Delay(0);
        }

        [TaskMethod]
        public async Task SetTagsAsync(ITable table, TaskContext context)
        {
            var tags = (TagInfo)TagInfoUtility.Names.Random();
            var template = table.Template;

            if (context.AllowException == false)
            {
                var editableState = await template.Dispatcher.InvokeAsync(() => template.EditableState);
                if (editableState != EditableState.None)
                    return;
            }

            await template.BeginEditAsync(context.Authentication);
            try
            {
                await template.SetTagsAsync(context.Authentication, tags);
                await template.EndEditAsync(context.Authentication);
            }
            catch
            {
                await template.CancelEditAsync(context.Authentication);
                throw;
            }
        }

        [TaskMethod]
        public async Task CopyAsync(ITable table, TaskContext context)
        {
            var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            var tableName = RandomUtility.NextIdentifier();
            var copyData = RandomUtility.NextBoolean();
            await table.CopyAsync(context.Authentication, tableName, categoryPath, copyData);
        }

        [TaskMethod]
        public async Task InheritAaync(ITable table, TaskContext context)
        {
            var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            var tableName = RandomUtility.NextIdentifier();
            var copyData = RandomUtility.NextBoolean();
            await table.InheritAsync(context.Authentication, tableName, categoryPath, copyData);
        }

        [TaskMethod]
        public async Task NewTableAsync(ITable table, TaskContext context)
        {
            var template = await table.NewTableAsync(context.Authentication);
            context.Push(template);
        }

        [TaskMethod]
        public async Task GetDataSetAsync(ITable table, TaskContext context)
        {
            await table.GetDataSetAsync(context.Authentication, null);
        }

        [TaskMethod]
        public async Task GetLogAsync(ITable table, TaskContext context)
        {
            await table.GetLogAsync(context.Authentication, null);
        }

        [TaskMethod]
        public async Task FindAsync(ITable table, TaskContext context)
        {
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await table.FindAsync(context.Authentication, text, option);
        }
    }
}

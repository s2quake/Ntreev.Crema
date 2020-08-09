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
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Extensions;
using Ntreev.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Linq;
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
            else if (RandomUtility.Within(75) == true)
            {
                var content = await table.Dispatcher.InvokeAsync(() => table.Content);
                context.Push(content, RandomUtility.Next(100));
            }
            else if (RandomUtility.Within(75) == true)
            {
                var template = await table.Dispatcher.InvokeAsync(() => table.Template);
                context.Push(template);
            }
        }

        public Type TargetType => typeof(ITable);

        public bool IsEnabled => false;

        //[TaskMethod]
        public void GetAccessType(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            table.Dispatcher.Invoke(() =>
            {
                table.GetAccessType(authentication);
            });
        }

        //[TaskMethod(Weight = 10)]
        public async Task LockAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (string.IsNullOrEmpty(comment) == true)
                    return;
                var lockInfo = await table.Dispatcher.InvokeAsync(() => table.LockInfo);
                if (lockInfo.IsLocked == true || lockInfo.IsInherited == true)
                    return;
            }

            await table.LockAsync(authentication, comment);
        }

        //[TaskMethod]
        public async Task UnlockAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                var lockInfo = await table.Dispatcher.InvokeAsync(() => table.LockInfo);
                if (lockInfo.IsLocked == false || lockInfo.IsInherited == true)
                    return;
            }
            await table.UnlockAsync(authentication);
        }

        //[TaskMethod]
        public async Task SetPublicAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == false)
                    return;
            }

            await table.SetPublicAsync(authentication);
        }

        //[TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == true)
                    return;
            }
            await table.SetPrivateAsync(authentication);
        }

        //[TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == false)
                    return;
            }
            var userContext = table.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            var accessType = RandomUtility.NextEnum<AccessType>();
            await table.AddAccessMemberAsync(authentication, memberID, accessType);
        }

        //[TaskMethod]
        public async Task RemoveAccessMemberAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.IsPrivate) == false)
                    return;
            }
            var userContext = table.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            await table.RemoveAccessMemberAsync(authentication, memberID);
        }

        [TaskMethod]
        public async Task RenameAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            var tableName = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            await table.RenameAsync(authentication, tableName);

            Task<bool> VerifyAsync()
            {
                return table.Dispatcher.InvokeAsync(() =>
                {
                    if (table.Name == tableName)
                        return false;
                    if (table.TableState != TableState.None)
                        return false;
                    if (table.TemplatedParent != null && table.TemplatedParent.TableState != TableState.None)
                        return false;
                    if (table.Parent != null && table.TemplatedParent != null)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod]
        public async Task MoveAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            await table.MoveAsync(authentication, categoryPath);

            Task<bool> VerifyAsync()
            {
                return table.Dispatcher.InvokeAsync(() =>
                {
                    if (table.Parent != null)
                        return false;
                    if (table.Category.Path == categoryPath)
                        return false;
                    if (table.TableState != TableState.None)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
                if ((await table.GetTypesAsync(item => true)).Any() == true)
                    return;
            }
            await table.DeleteAsync(authentication);
            context.Pop(table);

            Task<bool> VerifyAsync()
            {
                return table.Dispatcher.InvokeAsync(() =>
                {
                    if (table.TableState != TableState.None)
                        return false;
                    if (table.Childs.Any() == true)
                        return false;
                    if (table.DerivedTables.Any() == true)
                        return false;
                    if (table.TemplatedParent != null)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod]
        public async Task CopyAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            var tableName = RandomUtility.NextIdentifier();
            var copyData = RandomUtility.NextBoolean();
            if (context.AllowException == false)
            {
                if (await table.Dispatcher.InvokeAsync(() => table.Parent) != null)
                    return;
            }
            await table.CopyAsync(authentication, tableName, categoryPath, copyData);
        }

        [TaskMethod]
        public async Task InheritAaync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            var categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            var tableName = RandomUtility.NextIdentifier();
            var copyData = RandomUtility.NextBoolean();
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            await table.InheritAsync(authentication, tableName, categoryPath, copyData);

            Task<bool> VerifyAsync()
            {
                return table.Dispatcher.InvokeAsync(() =>
                {
                    if (table.Parent != null)
                        return false;
                    if (table.TemplatedParent != null)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod]
        public async Task NewTableAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            var template = await table.NewTableAsync(authentication);
            context.Push(template);

            async Task<bool> VerifyAsync()
            {
                if ((await table.GetAllRelationTablesAsync(item => item.TableState != TableState.None)).Any() == true)
                    return false;
                return await table.Dispatcher.InvokeAsync(() =>
                {
                    if (table.TemplatedParent != null)
                        return false;
                    if (table.TableState != TableState.None)
                        return false;
                    return true;
                });
            }
        }

        //[TaskMethod]
        public async Task GetDataSetAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            await table.GetDataSetAsync(authentication, null);
        }

        //[TaskMethod]
        public async Task GetLogAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            await table.GetLogAsync(authentication, null);
        }

        //[TaskMethod]
        public async Task FindAsync(ITable table, TaskContext context)
        {
            var authentication = context.Authentication;
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await table.FindAsync(authentication, text, option);
        }
    }
}

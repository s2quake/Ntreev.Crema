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
using Ntreev.Library.ObjectModel;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITableCategoryTask))]
    [TaskClass]
    public class ITableCategoryTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var category = context.Target as ITableCategory;
            var tables = category.GetService(typeof(ITableCollection)) as ITableCollection;
            var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            if (context.IsCompleted(category) == true)
            {
                context.Pop(category);
            }
            else if (categories.Count < RandomUtility.Next(Math.Max(10, categories.Count + 1)))
            {
                await this.AddNewCategoryAsync(category, context);
                context.Complete(category);
            }
            else if (tables.Count < RandomUtility.Next(Math.Max(10, tables.Count + 1)))
            {
                var template = await category.NewTableAsync(context.Authentication);
                context.Push(template);
            }
            else
            {
                context.Complete(category);
            }
        }

        public Type TargetType
        {
            get { return typeof(ITableCategory); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod]
        public async Task GetAccessTypeAsync(ITableCategory category, TaskContext context)
        {
            await category.Dispatcher.InvokeAsync(() => category.GetAccessType(context.Authentication));
        }

        //[TaskMethod(Weight = 10)]
        public async Task LockAsync(ITableCategory category, TaskContext context)
        {
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (comment == string.Empty)
                    return;
                if (category.IsLocked == true)
                    return;
            }
            await category.LockAsync(context.Authentication, comment);
        }

        //[TaskMethod]
        public async Task UnlockAsync(ITableCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (await category.Dispatcher.InvokeAsync(()=> category.IsLocked) == false)
                    return;
            }
            await category.UnlockAsync(context.Authentication);
        }

        //[TaskMethod]
        public async Task SetPublicAsync(ITableCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
            await category.SetPublicAsync(context.Authentication);
        }

        //[TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(ITableCategory category, TaskContext context)
        {
            if (category.Parent == null)
                return;
            await category.SetPrivateAsync(context.Authentication);
        }

        //[TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(ITableCategory category, TaskContext context)
        {
            if (category.Parent == null)
                return;
            var userContext = category.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            var accessType = RandomUtility.NextEnum<AccessType>();
            if (NameValidator.VerifyItemPath(memberID) == true)
            {
                await category.AddAccessMemberAsync(context.Authentication, new ItemName(memberID).Name, accessType);
            }
            else
            {
                await category.AddAccessMemberAsync(context.Authentication, memberID, accessType);
            }
        }

        //[TaskMethod(Weight = 10)]
        public async Task RemoveAccessMemberAsync(ITableCategory category, TaskContext context)
        {
            if (category.Parent == null)
                return;
            var userContext = category.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Select(item => item.Path).Random());
            if (NameValidator.VerifyItemPath(memberID) == true)
            {
                await category.RemoveAccessMemberAsync(context.Authentication, new ItemName(memberID).Name);
            }
            else
            {
                await category.RemoveAccessMemberAsync(context.Authentication, memberID);
            }
        }

        [TaskMethod]
        public async Task RenameAsync(ITableCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
            }
            var categoryName = RandomUtility.NextIdentifier();
            await category.RenameAsync(context.Authentication, categoryName);
        }

        [TaskMethod]
        public async Task MoveAsync(ITableCategory category, TaskContext context)
        {
            var categories = category.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (categoryPath.StartsWith(category.Path) == true)
                    return;
                if (category.Parent.Path == categoryPath)
                    return;
            }
            await category.MoveAsync(context.Authentication, categoryPath);
        }

        //[TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITableCategory category, TaskContext context)
        {
            await category.DeleteAsync(context.Authentication);
            context.Pop(category);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddNewCategoryAsync(ITableCategory category, TaskContext context)
        {
            var categoryNanme = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (category.GetAccessType(context.Authentication) < AccessType.Master)
                    return;
            }
            await category.AddNewCategoryAsync(context.Authentication, categoryNanme);
        }

        [TaskMethod(Weight = 10)]
        public async Task NewTableAsync(ITableCategory category, TaskContext context)
        {
            var template = await category.NewTableAsync(context.Authentication);
            context.Push(template);
        }

        //[TaskMethod]
        public async Task GetDataSetAsync(ITableCategory category, TaskContext context)
        {
            await category.GetDataSetAsync(context.Authentication, null);
        }

        //[TaskMethod]
        public async Task GetLogAsync(ITableCategory category, TaskContext context)
        {
            await category.GetLogAsync(context.Authentication, null);
        }

        //[TaskMethod]
        public async Task FindAsync(ITableCategory category, TaskContext context)
        {
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await category.FindAsync(context.Authentication, text, option);
        }
    }
}

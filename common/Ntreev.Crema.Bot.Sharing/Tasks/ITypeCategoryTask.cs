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
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITypeCategoryTask))]
    [TaskClass]
    public class ITypeCategoryTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var category = context.Target as ITypeCategory;
            var types = category.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var categories = category.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            if (context.IsCompleted(category) == true)
            {
                context.Pop(category);
            }
            else if (categories.Count < RandomUtility.Next(Math.Max(10, categories.Count + 1)))
            {
                await this.AddNewCategoryAsync(category, context);
                context.Complete(category);
            }
            else if (types.Count < RandomUtility.Next(Math.Max(10, types.Count + 1)))
            {
                var template = await category.NewTypeAsync(context.Authentication);
                context.Push(template);
            }
            else
            {
                context.Complete(category);
            }
        }

        public Type TargetType
        {
            get { return typeof(ITypeCategory); }
        }

        public bool IsEnabled
        {
            get { return false; }
        }

        [TaskMethod]
        public async Task GetAccessTypeAsync(ITypeCategory category, TaskContext context)
        {
            category.Dispatcher.Invoke(() =>
            {
                category.GetAccessType(context.Authentication);
            });
        }

        //[TaskMethod]
        //public async Task VerifyReadAsync(ITypeCategory category, TaskContext context)
        //{
        //    category.Dispatcher.Invoke(() =>
        //    {
        //        category.VerifyRead(context.Authentication);
        //    });
        //}

        //[TaskMethod]
        //public async Task VerifyOwnerAsync(ITypeCategory category, TaskContext context)
        //{
        //    category.Dispatcher.Invoke(() =>
        //    {
        //        category.VerifyOwner(context.Authentication);
        //    });
        //}

        //[TaskMethod]
        //public async Task VerifyMemberAsync(ITypeCategory category, TaskContext context)
        //{
        //    category.Dispatcher.Invoke(() =>
        //    {
        //        category.VerifyMember(context.Authentication);
        //    });
        //}

        [TaskMethod(Weight = 10)]
        public async Task LockAsync(ITypeCategory category, TaskContext context)
        {
            if (category.Parent == null)
                return;
            var comment = RandomUtility.NextString();
            await category.LockAsync(context.Authentication, comment);
        }

        [TaskMethod]
        public async Task UnlockAsync(ITypeCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsLocked == false)
                    return;
            }
            await category.UnlockAsync(context.Authentication);
        }

        [TaskMethod]
        public async Task SetPublicAsync(ITypeCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
            category.SetPublicAsync(context.Authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(ITypeCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
            await category.SetPrivateAsync(context.Authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(ITypeCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
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

        [TaskMethod]
        public async Task RemoveAccessMemberAsync(ITypeCategory category, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
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

        [TaskMethod(Weight = 25)]
        public async Task RenameAsync(ITypeCategory category, TaskContext context)
        {
            var categoryName = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
            }
            await category.RenameAsync(context.Authentication, categoryName);
        }

        [TaskMethod(Weight = 25)]
        public async Task MoveAsync(ITypeCategory category, TaskContext context)
        {
            var categories = category.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (category.Parent == null || category.Path == categoryPath)
                    return;
            }
            await category.MoveAsync(context.Authentication, categoryPath);
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITypeCategory category, TaskContext context)
        {
            await Task.Delay(0);
        }

        [TaskMethod]
        public async Task AddNewCategoryAsync(ITypeCategory category, TaskContext context)
        {
            var categoryNanme = RandomUtility.NextIdentifier();
            await category.AddNewCategoryAsync(context.Authentication, categoryNanme);
        }

        [TaskMethod(Weight = 10)]
        public async Task NewTypeAsync(ITypeCategory category, TaskContext context)
        {
            var template = await category.NewTypeAsync(context.Authentication);
            context.Push(template);
        }

        [TaskMethod]
        public async Task GetDataSetAsync(ITypeCategory category, TaskContext context)
        {
            await category.GetDataSetAsync(context.Authentication, null);
        }

        [TaskMethod]
        public async Task GetLogAsync(ITypeCategory category, TaskContext context)
        {
            await category.GetLogAsync(context.Authentication, null);
        }

        [TaskMethod]
        public async Task FindAsync(ITypeCategory category, TaskContext context)
        {
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await category.FindAsync(context.Authentication, text, option);
        }
    }
}

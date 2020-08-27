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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(ITypeCategoryTask))]
    [TaskClass]
    public class ITypeCategoryTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var authentication = context.Authentication;
            var category = context.Target as ITypeCategory;
            var types = category.GetService(typeof(ITypeCollection)) as ITypeCollection;
            var categories = category.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            if (context.IsCompleted(category) == true)
            {
                context.Pop(category);
            }
            else if (categories.Count < RandomUtility.Next(Math.Max(10, categories.Count + 1)))
            {
                if (context.AllowException == false)
                {
                    if (await category.Dispatcher.InvokeAsync(() => category.GetAccessType(authentication)) < AccessType.Master)
                        return;
                }
                await this.AddNewCategoryAsync(category, context);
                context.Complete(category);
            }
            else if (types.Count < RandomUtility.Next(Math.Max(10, types.Count + 1)))
            {
                if (context.AllowException == false)
                {
                    if (await category.Dispatcher.InvokeAsync(() => category.GetAccessType(authentication)) < AccessType.Master)
                        return;
                }
                var template = await category.NewTypeAsync(authentication);
                context.Push(template);
            }
            else
            {
                context.Complete(category);
            }
        }

        public Type TargetType => typeof(ITypeCategory);

        public bool IsEnabled => false;

        //[TaskMethod]
        public Task GetAccessTypeAsync(ITypeCategory category, TaskContext context)
        {
            throw new NotImplementedException();
            //category.Dispatcher.Invoke(() =>
            //{
            //    category.GetAccessType(authentication);
            //});
        }

        //[TaskMethod]
        //public async Task VerifyReadAsync(ITypeCategory category, TaskContext context)
        //{
        //    category.Dispatcher.Invoke(() =>
        //    {
        //        category.VerifyRead(authentication);
        //    });
        //}

        //[TaskMethod]
        //public async Task VerifyOwnerAsync(ITypeCategory category, TaskContext context)
        //{
        //    category.Dispatcher.Invoke(() =>
        //    {
        //        category.VerifyOwner(authentication);
        //    });
        //}

        //[TaskMethod]
        //public async Task VerifyMemberAsync(ITypeCategory category, TaskContext context)
        //{
        //    category.Dispatcher.Invoke(() =>
        //    {
        //        category.VerifyMember(authentication);
        //    });
        //}

        //[TaskMethod(Weight = 10)]
        public async Task LockAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (string.IsNullOrEmpty(comment) == true)
                    return;
                var lockInfo = await category.Dispatcher.InvokeAsync(() => category.LockInfo);
                if (lockInfo.IsLocked == true || lockInfo.IsInherited == true)
                    return;
            }

            await category.LockAsync(authentication, comment);
        }

        [TaskMethod]
        public async Task UnlockAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                var lockInfo = await category.Dispatcher.InvokeAsync(() => category.LockInfo);
                if (lockInfo.IsLocked == false || lockInfo.IsInherited == true)
                    return;

            }
            await category.UnlockAsync(authentication);
        }

        [TaskMethod]
        public async Task SetPublicAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
            await category.SetPublicAsync(authentication);
        }

        //[TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (category.IsPrivate == false)
                    return;
            }
            await category.SetPrivateAsync(authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
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
                await category.AddAccessMemberAsync(authentication, new ItemName(memberID).Name, accessType);
            }
            else
            {
                await category.AddAccessMemberAsync(authentication, memberID, accessType);
            }
        }

        [TaskMethod]
        public async Task RemoveAccessMemberAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
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
                await category.RemoveAccessMemberAsync(authentication, new ItemName(memberID).Name);
            }
            else
            {
                await category.RemoveAccessMemberAsync(authentication, memberID);
            }
        }

        [TaskMethod(Weight = 25)]
        public async Task RenameAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var categoryName = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (category.Parent == null)
                    return;
                if (await category.Dispatcher.InvokeAsync(() => category.VerifyAccessType(authentication, AccessType.Master)) == false)
                    return;
            }
            await category.RenameAsync(authentication, categoryName);
        }

        [TaskMethod(Weight = 25)]
        public async Task MoveAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var categories = category.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            await category.MoveAsync(authentication, categoryPath);

            async Task<bool> VerifyAsync()
            {
                if ((await category.GetAllTypesAsync(item => item.TypeState != TypeState.None)).Any() == true)
                    return false;

                if ((await category.GetAllUsingTablesAsync(item => item.TableState != TableState.None)).Any() == true)
                    return false;

                return await category.Dispatcher.InvokeAsync(() =>
                {
                    if (category.Parent == null)
                        return false;
                    if (category.Parent.Path == categoryPath)
                        return false;
                    if (category.Path == categoryPath)
                        return false;
                    if (categoryPath.StartsWith(category.Path) == true)
                        return false;
                    if (category.VerifyAccessType(authentication, AccessType.Master) == false)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == true)
                    return;
            }
            await category.DeleteAsync(authentication);
            context.Pop(category);

            async Task<bool> VerifyAsync()
            {
                if ((await category.GetAllTypesAsync(item => true)).Any() == true)
                    return false;
                return await category.Dispatcher.InvokeAsync(() =>
                {
                    if (category.Parent == null)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod]
        public async Task AddNewCategoryAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var categoryNanme = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (category.GetAccessType(authentication) < AccessType.Master)
                    return;
            }
            await category.AddNewCategoryAsync(authentication, categoryNanme);
        }

        [TaskMethod(Weight = 10)]
        public async Task NewTypeAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await category.Dispatcher.InvokeAsync(() => category.GetAccessType(authentication)) < AccessType.Master)
                    return;
            }
            var template = await category.NewTypeAsync(authentication);
            context.Push(template);
        }

        [TaskMethod]
        public async Task GetDataSetAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            await category.GetDataSetAsync(authentication, null);
        }

        [TaskMethod]
        public async Task GetLogAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            await category.GetLogAsync(authentication, null);
        }

        [TaskMethod]
        public async Task FindAsync(ITypeCategory category, TaskContext context)
        {
            var authentication = context.Authentication;
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await category.FindAsync(authentication, text, option);
        }
    }
}

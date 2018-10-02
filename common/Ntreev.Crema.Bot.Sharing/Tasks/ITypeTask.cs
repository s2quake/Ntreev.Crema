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
    [Export(typeof(ITypeTask))]
    [TaskClass]
    public class ITypeTask : ITaskProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var type = context.Target as IType;
            if (context.IsCompleted(type) == true)
            {
                context.Pop(type);
            }
            else if (RandomUtility.Within(75) == true)
            {
                context.Push(type.Template);
            }
            await Task.Delay(0);
        }

        public Type TargetType
        {
            get { return typeof(IType); }
        }

        [TaskMethod]
        public async Task GetAccessTypeAsync(IType type, TaskContext context)
        {
            await type.Dispatcher.InvokeAsync(() => type.GetAccessType(context.Authentication));
        }

        [TaskMethod(Weight = 10)]
        public async Task LockAsync(IType type, TaskContext context)
        {
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (string.IsNullOrEmpty(comment) == true)
                    return;
                var lockInfo = await type.Dispatcher.InvokeAsync(() => type.LockInfo);
                if (lockInfo.IsLocked == true || lockInfo.IsInherited == true)
                    return;
            }
            await type.LockAsync(context.Authentication, comment);
        }

        [TaskMethod]
        public async Task UnlockAsync(IType type, TaskContext context)
        {
            if (context.AllowException == false)
            {
                var lockInfo = await type.Dispatcher.InvokeAsync(() => type.LockInfo);
                if (lockInfo.IsLocked == false || lockInfo.IsInherited == true)
                    return;
            }
            await type.UnlockAsync(context.Authentication);
        }

        [TaskMethod]
        public async Task SetPublicAsync(IType type, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == false)
                    return;
            }
            await type.SetPublicAsync(context.Authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(IType type, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == true)
                    return;
            }
            await type.SetPrivateAsync(context.Authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(IType type, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == false)
                    return;
            }
            var userContext = type.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Users.Random().ID);
            var accessType = RandomUtility.NextEnum<AccessType>();
            await type.AddAccessMemberAsync(context.Authentication, memberID, accessType);
        }

        [TaskMethod]
        public async Task RemoveAccessMemberAsync(IType type, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == false)
                    return;
            }
            var userContext = type.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Users.Random().ID);
            await type.RemoveAccessMemberAsync(context.Authentication, memberID);
        }

        [TaskMethod(Weight = 25)]
        public async Task RenameAsync(IType type, TaskContext context)
        {
            if (context.AllowException == false)
            {
                var typeState = await type.Dispatcher.InvokeAsync(() => type.TypeState);
                if (typeState != TypeState.None)
                    return;
            }
            var typeName = RandomUtility.NextIdentifier();
            await type.RenameAsync(context.Authentication, typeName);
        }

        [TaskMethod(Weight = 25)]
        public async Task MoveAsync(IType type, TaskContext context)
        {
            var categories = type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.Category.Path) == categoryPath)
                    return;
            }
            await type.MoveAsync(context.Authentication, categoryPath);
        }

        [TaskMethod(Weight = 5)]
        public async Task DeleteAsync(IType type, TaskContext context)
        {
            if (context.AllowException == false)
            {
                var typeState = await type.Dispatcher.InvokeAsync(() => type.TypeState);
                if (typeState.HasFlag(TypeState.IsBeingEdited) == true)
                    return;
            }
            await type.DeleteAsync(context.Authentication);
        }

        //[TaskMethod]
        public async Task SetTagsAsync(IType type, TaskContext context)
        {
            var tags = (TagInfo)TagInfoUtility.Names.Random();
            var template = type.Template;
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
        public async Task CopyAsync(IType type, TaskContext context)
        {
            var categories = type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            var typeName = RandomUtility.NextIdentifier();
            await type.CopyAsync(context.Authentication, typeName, categoryPath);
        }

        [TaskMethod]
        public async Task GetDataSetAsync(IType type, TaskContext context)
        {
            await type.GetDataSetAsync(context.Authentication, null);
        }

        [TaskMethod]
        public async Task GetLogAsync(IType type, TaskContext context)
        {
            await type.GetLogAsync(context.Authentication, null);
        }

        [TaskMethod]
        public async Task FindAsync(IType type, TaskContext context)
        {
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await type.FindAsync(context.Authentication, text, option);
        }
    }
}

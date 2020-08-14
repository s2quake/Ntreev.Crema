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
using Ntreev.Crema.Services.Extensions;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Linq;
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

        public Type TargetType => typeof(IType);

        [TaskMethod]
        public async Task GetAccessTypeAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            await type.Dispatcher.InvokeAsync(() => type.GetAccessType(authentication));
        }

        [TaskMethod(Weight = 10)]
        public async Task LockAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (string.IsNullOrEmpty(comment) == true)
                    return;
                var lockInfo = await type.Dispatcher.InvokeAsync(() => type.LockInfo);
                if (lockInfo.IsLocked == true || lockInfo.IsInherited == true)
                    return;
            }
            await type.LockAsync(authentication, comment);
        }

        [TaskMethod]
        public async Task UnlockAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                var lockInfo = await type.Dispatcher.InvokeAsync(() => type.LockInfo);
                if (lockInfo.IsLocked == false || lockInfo.IsInherited == true)
                    return;
            }
            await type.UnlockAsync(authentication);
        }

        [TaskMethod]
        public async Task SetPublicAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == false)
                    return;
            }
            await type.SetPublicAsync(authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task SetPrivateAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == true)
                    return;
            }
            await type.SetPrivateAsync(authentication);
        }

        [TaskMethod(Weight = 10)]
        public async Task AddAccessMemberAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == false)
                    return;
            }
            var userContext = type.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Users.Random().ID);
            var accessType = RandomUtility.NextEnum<AccessType>();
            await type.AddAccessMemberAsync(authentication, memberID, accessType);
        }

        [TaskMethod]
        public async Task RemoveAccessMemberAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await type.Dispatcher.InvokeAsync(() => type.IsPrivate) == false)
                    return;
            }
            var userContext = type.GetService(typeof(IUserContext)) as IUserContext;
            var memberID = await userContext.Dispatcher.InvokeAsync(() => userContext.Users.Random().ID);
            await type.RemoveAccessMemberAsync(authentication, memberID);
        }

        [TaskMethod(Weight = 25)]
        public async Task RenameAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            var typeName = RandomUtility.NextIdentifier();
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            await type.RenameAsync(authentication, typeName);

            async Task<bool> VerifyAsync()
            {
                if ((await type.GetTablesAsync(item => item.TableState != TableState.None)).Any() == true)
                    return false;
                return await type.Dispatcher.InvokeAsync(() =>
                {
                    if (type.TypeState != TypeState.None)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod(Weight = 25)]
        public async Task MoveAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            var categories = type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            await type.MoveAsync(authentication, categoryPath);

            async Task<bool> VerifyAsync()
            {
                if ((await type.GetTablesAsync(item => item.TableState != TableState.None)).Any() == true)
                    return false;
                return await type.Dispatcher.InvokeAsync(() =>
                {
                    if (type.TypeState != TypeState.None)
                        return false;
                    return true;
                });
            }
        }

        [TaskMethod(Weight = 5)]
        public async Task DeleteAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            if (context.AllowException == false)
            {
                if (await VerifyAsync() == false)
                    return;
            }
            await type.DeleteAsync(authentication);
            context.Pop(type);

            async Task<bool> VerifyAsync()
            {
                if ((await type.GetTablesAsync(item => true)).Any() == true)
                    return false;
                return await type.Dispatcher.InvokeAsync(() =>
                {
                    if (type.TypeState != TypeState.None)
                        return false;
                    return true;
                });
            }
        }

        //[TaskMethod]
        public async Task SetTagsAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            var tags = (TagInfo)TagInfoUtility.Names.Random();
            var template = type.Template;
            await template.BeginEditAsync(authentication);
            try
            {
                await template.SetTagsAsync(authentication, tags);
                await template.EndEditAsync(authentication);
            }
            catch
            {
                await template.CancelEditAsync(authentication);
                throw;
            }
        }

        [TaskMethod]
        public async Task CopyAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            var categories = type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            var categoryPath = await categories.Dispatcher.InvokeAsync(() => categories.Random().Path);
            var typeName = RandomUtility.NextIdentifier();
            await type.CopyAsync(authentication, typeName, categoryPath);
        }

        [TaskMethod]
        public async Task GetDataSetAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            await type.GetDataSetAsync(authentication, null);
        }

        [TaskMethod]
        public async Task GetLogAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            await type.GetLogAsync(authentication, null);
        }

        [TaskMethod]
        public async Task FindAsync(IType type, TaskContext context)
        {
            var authentication = context.Authentication;
            var text = RandomUtility.NextWord();
            var option = RandomUtility.NextEnum<FindOptions>();
            await type.FindAsync(authentication, text, option);
        }
    }
}

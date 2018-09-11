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
    [Export(typeof(IUserTask))]
    [TaskClass(Weight = 10)]
    public class IUserTask : ITaskProvider
    {
        public IUserTask()
        {

        }

        public Task InvokeAsync(TaskContext context)
        {
            var user = context.Target as IUser;
            if (context.IsCompleted(user) == true)
            {
                context.Pop(user);
            }
            else if (RandomUtility.Within(50) == true)
            {
                context.Complete(user);
            }
            return Task.Delay(0);
        }

        public Type TargetType
        {
            get { return typeof(IUser); }
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        [TaskMethod]
        public Task MoveAsync(IUser user, TaskContext context)
        {
            return user.Dispatcher.InvokeAsync(async () =>
            {
                var categories = user.GetService(typeof(IUserCategoryCollection)) as IUserCategoryCollection;
                var categoryPath = categories.Random().Path;
                if (Verify(categoryPath) == false)
                    return;
                await user.MoveAsync(context.Authentication, categoryPath);
            });

            bool Verify(string categoryPath)
            {
                if (context.AllowException == true)
                    return true;
                if (user.Category.Path == categoryPath)
                    return false;
                return true;
            }
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(IUser user, TaskContext context)
        {
            await user.DeleteAsync(context.Authentication);
        }

        [TaskMethod]
        public async Task ChangeUserInfoAsync(IUser user, TaskContext context)
        {
            await Task.Delay(0);
        }

        [TaskMethod]
        public async Task SendMessageAsync(IUser user, TaskContext context)
        {
            var message = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (string.IsNullOrEmpty(message) == true)
                    return;
                if (await user.Dispatcher.InvokeAsync(() => user.UserState) != UserState.Online)
                    return;
            }
            await user.SendMessageAsync(context.Authentication, message);
        }

        [TaskMethod]
        public async Task KickAsync(IUser user, TaskContext context)
        {
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (string.IsNullOrEmpty(comment) == true)
                    return;
                if (await user.Dispatcher.InvokeAsync(() => user.Authority) == Authority.Admin)
                    return;
                if (await user.Dispatcher.InvokeAsync(() => user.UserState) != UserState.Online)
                    return;
            }
            await user.KickAsync(context.Authentication, comment);
        }

        [TaskMethod]
        public async Task BanAsync(IUser user, TaskContext context)
        {
            var comment = RandomUtility.NextString();
            if (context.AllowException == false)
            {
                if (string.IsNullOrEmpty(comment) == true)
                    return;
                if (await user.Dispatcher.InvokeAsync(() => user.BanInfo.Path) != string.Empty)
                    return;
                if (await user.Dispatcher.InvokeAsync(() => user.Authority) == Authority.Admin)
                    return;
            }
            await user.BanAsync(context.Authentication, comment);
        }

        [TaskMethod]
        public async Task UnbanAsync(IUser user, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (await user.Dispatcher.InvokeAsync(() => user.BanInfo.Path) != user.Path)
                    return;
            }
            await user.UnbanAsync(context.Authentication);
        }
    }
}

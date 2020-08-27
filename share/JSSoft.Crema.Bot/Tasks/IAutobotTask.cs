// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(IAutobotTask))]
    [TaskClass]
    class IAutobotTask : ITaskProvider
    {
        public IAutobotTask()
        {

        }

        public async Task InvokeAsync(TaskContext context)
        {
            if (context.Target is AutobotBase autobot)
            {
                if (autobot.IsOnline == true)
                {
                    if (RandomUtility.Within(75) == true)
                    {
                        if (autobot.GetService(typeof(ICremaHost)) is ICremaHost cremaHost && cremaHost.GetService(typeof(IDataBaseContext)) is IDataBaseContext dataBaseContext)
                        {
                            var dataBase = await dataBaseContext.Dispatcher.InvokeAsync(() => dataBaseContext.Random());
                            context.Push(dataBase);
                        }
                    }
                    else if (RandomUtility.Within(75) == true)
                    {
                        if (autobot.GetService(typeof(IUserContext)) is IUserContext userContext)
                        {
                            if (RandomUtility.Within(50) == true)
                            {
                                var user = await userContext.Dispatcher.InvokeAsync(() => userContext.Users.Random());
                                context.Push(user);
                            }
                            else if (RandomUtility.Within(50) == true)
                            {
                                var category = await userContext.Dispatcher.InvokeAsync(() => userContext.Categories.Random());
                                context.Push(category);
                            }
                            else
                            {
                                var userItem = await userContext.Dispatcher.InvokeAsync(() => userContext.Random());
                                context.Push(userItem);
                            }
                        }
                    }
                    //else if (RandomUtility.Within(10) == true)
                    //{
                    //    var dataBase = autobot.CremaHost.Dispatcher.Invoke(() => autobot.CremaHost.DataBases);
                    //    context.Push(dataBase);
                    //}
                }
            }
        }

        public Type TargetType => typeof(AutobotBase);

        public bool IsEnabled => true;

        [TaskMethod]
        public async Task LoginAsync(AutobotBase autobot, TaskContext context)
        {
            if (context.AllowException == false)
            {
                if (autobot.IsOnline == true)
                    return;
                if (autobot.GetService(typeof(IUserContext)) is IUserContext userContext)
                {
                    var banInfo = await userContext.Dispatcher.InvokeAsync(() => userContext.Users[autobot.AutobotID].BanInfo);
                    if (banInfo.IsBanned == true)
                        return;
                }
            }
            try
            {
                await autobot.LoginAsync();
            }
            catch
            {
                autobot.Cancel();
                context.Pop(autobot);
            }
        }

        [TaskMethod]
        public async Task LogoutAsync(AutobotBase autobot, TaskContext context)
        {
            if (autobot.IsOnline == true)
            {
                await autobot.LogoutAsync();
            }
        }

        [TaskMethod(Weight = 1)]
        public async Task CreateAutobotAsync(Autobot autobot, TaskContext context)
        {
            var authentication = context.Authentication;
            var autobotService = autobot.Service;
            if (context.AllowException == false)
            {
                if (autobot.IsOnline == false)
                    return;
                if (authentication.Authority != Authority.Admin)
                    return;
            }
            var info = autobotService.GetRandomUserInfo();
            if (autobot.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                if (await userContext.Users.ContainsAsync(info.ID) == false)
                {
                    var category = userContext.Categories.Random();
                    await category.AddNewUserAsync(authentication, info.ID, info.Password, info.Name, info.Authority);
                }
                await autobotService.CreateAutobotAsync(authentication, info.ID, info.Password);
            }
        }
    }
}

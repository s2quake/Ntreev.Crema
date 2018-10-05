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
                        if (autobot.GetService(typeof(ICremaHost)) is ICremaHost cremaHost && cremaHost.GetService(typeof(IDataBaseCollection)) is IDataBaseCollection dataBases)
                        {
                            var dataBase = await dataBases.Dispatcher.InvokeAsync(() => dataBases.Random());
                            context.Push(dataBase);
                        }
                    }
                    else if (RandomUtility.Within(75) == true)
                    {
                        //if (autobot.GetService(typeof(IUserContext)) is IUserContext userContext)
                        //{
                        //    var userItem = await userContext.Dispatcher.InvokeAsync(() => userContext.Random());
                        //    context.Push(userItem);
                        //}
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
            await autobot.LoginAsync();
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
            if (context.AllowException == false)
            {
                if (autobot.IsOnline == false)
                    return;
                if (context.Authentication.Authority != Authority.Admin)
                    return;
            }
            var autobotID = $"Autobot{RandomUtility.Next(1000)}";
            var authority = RandomUtility.NextEnum<Authority>();
            var authentication = context.Authentication;
            if (autobot.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                if (await userContext.Users.ContainsAsync(autobotID) == false)
                {
                    var category = userContext.Categories["/autobots/"];
                    await category.AddNewUserAsync(authentication, autobotID, StringUtility.ToSecureString("1111"), autobotID, authority);
                }
                await autobot.Service.CreateAutobotAsync(context.Authentication, autobotID);
            }
        }
    }
}

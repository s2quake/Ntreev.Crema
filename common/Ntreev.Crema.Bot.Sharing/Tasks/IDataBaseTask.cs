﻿//Released under the MIT License.
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
    [Export(typeof(IDataBaseTask))]
    [Export(typeof(IConfigurationPropertyProvider))]
    [TaskClass]
    public class IDataBaseTask : ITaskProvider, IConfigurationPropertyProvider
    {
        public async Task InvokeAsync(TaskContext context)
        {
            var dataBase = context.Target as IDataBase;
            if (context.IsCompleted(dataBase) == true)
            {
                context.Pop(dataBase);
            }
            else
            {
                if (await dataBase.ContainsAsync(context.Authentication) == true)
                {
                    if (RandomUtility.Within(35) == true)
                    {
                        var typeItem = await dataBase.Dispatcher.InvokeAsync(() => dataBase.TypeContext.Random());
                        context.Push(typeItem);
                    }
                    else
                    {
                        var tableItem = await dataBase.Dispatcher.InvokeAsync(() => dataBase.TableContext.Random());
                        context.Push(tableItem);
                    }
                }
            }
        }

        public Type TargetType
        {
            get { return typeof(IDataBase); }
        }

        [TaskMethod]
        public async Task EnterAsync(IDataBase dataBase, TaskContext context)
        {
            if (await dataBase.ContainsAsync(context.Authentication) == false)
            {
                await dataBase.EnterAsync(context.Authentication);
            }
        }

        [TaskMethod]
        public async Task LeaveAsync(IDataBase dataBase, TaskContext context)
        {
            if (context.IsCompleted(dataBase) == true)
            {
                await dataBase.LeaveAsync(context.Authentication);
                context.Pop(dataBase);
            }
        }

        [TaskMethod(Weight = 10)]
        public async Task RenameAsync(IDataBase dataBase, Authentication authentication)
        {
            var dataBaseName = RandomUtility.NextIdentifier();
            await dataBase.RenameAsync(authentication, dataBaseName);
        }

        [TaskMethod(Weight = 1)]
        public async Task DeleteAsync(IDataBase dataBase, Authentication authentication)
        {
            await dataBase.DeleteAsync(authentication);
        }

        [TaskMethod(Weight = 5)]
        public async Task CopyAsync(IDataBase dataBase, Authentication authentication)
        {
            var dataBaseName = RandomUtility.NextIdentifier();
            var comment = RandomUtility.NextString();
            var force = RandomUtility.NextBoolean();
            await dataBase.CopyAsync(authentication, dataBaseName, comment, force);
        }

        [ConfigurationProperty(ScopeType = typeof(ICremaConfiguration))]
        public bool IsEnabled
        {
            get; set;
        }

        #region IConfigurationPropertyProvider

        string IConfigurationPropertyProvider.Name => "bot.database";

        #endregion
    }
}
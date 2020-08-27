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

using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Random;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Bot.Tasks
{
    [Export(typeof(ITaskProvider))]
    [Export(typeof(IDataBaseContextTask))]
    [Export(typeof(IConfigurationPropertyProvider))]
    [TaskClass(Weight = 10)]
    public class IDataBaseContextTask : ITaskProvider, IConfigurationPropertyProvider
    {
        [ImportingConstructor]
        public IDataBaseContextTask()
        {

        }

        public Task InvokeAsync(TaskContext context)
        {
            var dataBaseContext = context.Target as IDataBaseContext;
            if (context.IsCompleted(dataBaseContext) == true)
            {
                context.Pop(dataBaseContext);
            }
            else
            {
                context.Complete(dataBaseContext);
            }
            return Task.Delay(0);
        }

        public Type TargetType => typeof(IDataBaseContext);

        [TaskMethod(Weight = 10)]
        public async Task AddNewDataBaseAsync(IDataBaseContext dataBaseContext, TaskContext context)
        {
            var authentication = context.Authentication;
            var dataBaseName = RandomUtility.NextIdentifier();
            var comment = RandomUtility.NextString();
            await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        #region IConfigurationPropertyProvider

        string IConfigurationPropertyProvider.Name => "bot.databaseCollection";

        #endregion
    }
}

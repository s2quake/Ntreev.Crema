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
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    abstract class AccessCommandBase : ConsoleCommandAsyncBase
    {
        protected AccessCommandBase()
        {

        }

        protected AccessCommandBase(string name)
            : base(name)
        {

        }

        protected string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path) == true)
                return this.CommandContext.Path;
            return this.CommandContext.GetAbsolutePath(path);
        }

        public override bool IsEnabled
        {
            get
            {
                if (this.CommandContext.IsOnline == false)
                    return false;
                return this.CommandContext.Drive is DataBasesConsoleDrive drive;
            }
        }

        protected async Task<IAccessible> GetObjectAsync(Authentication authentication, string path)
        {
            var absolutePath = this.GetAbsolutePath(path);
            var drive = this.CommandContext.Drive as DataBasesConsoleDrive;
            if (await drive.GetObjectAsync(authentication, absolutePath) is IAccessible accessible)
            {
                return accessible;
            }
            throw new ArgumentException($"'{path}' dose not exists.");
        }

        protected void Invoke(Authentication authentication, IAccessible accessible, Action action)
        {
            var task = UsingDataBase.SetAsync(accessible as IServiceProvider, authentication);
            task.Wait();
            using (task.Result)
            {
                if (accessible is IDispatcherObject dispatcherObject)
                {
                    dispatcherObject.Dispatcher.Invoke(action);
                }
                else
                {
                    action();
                }
            }
        }

        protected async Task<T> InvokeAsync<T>(Authentication authentication, IAccessible accessible, Func<T> func)
        {
            using var task = await UsingDataBase.SetAsync(accessible as IServiceProvider, authentication);
            if (accessible is IDispatcherObject dispatcherObject)
            {
                return await dispatcherObject.Dispatcher.InvokeAsync(func);
            }
            else
            {
                return await Task.Run(func);
            }
        }
    }
}

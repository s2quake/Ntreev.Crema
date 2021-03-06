﻿// Released under the MIT License.
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
    abstract class LockCommandBase : ConsoleCommandAsyncBase
    {
        protected LockCommandBase()
        {

        }

        protected LockCommandBase(string name)
            : base(name)
        {

        }

        public override bool IsEnabled
        {
            get
            {
                if (this.CommandContext.IsOnline == false)
                    return false;
                return this.CommandContext.Drive is DataBasesConsoleDrive;
            }
        }

        protected string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path) == true)
                return this.CommandContext.Path;
            return this.CommandContext.GetAbsolutePath(path);
        }

        protected async Task<ILockable> GetObjectAsync(Authentication authentication, string path)
        {
            var drive = this.CommandContext.Drive as DataBasesConsoleDrive;
            if (await drive.GetObjectAsync(authentication, path) is ILockable lockable)
            {
                return lockable;
            }
            throw new NotImplementedException();
        }

        protected void Invoke(Authentication authentication, ILockable lockable, Action action)
        {
            //using (UsingDataBase.Set(lockable as IServiceProvider, authentication, true))
            {
                if (lockable is IDispatcherObject dispatcherObject)
                {
                    dispatcherObject.Dispatcher.Invoke(action);
                }
                else
                {
                    action();
                }
            }
        }

        protected Task<T> InvokeAsync<T>(Authentication authentication, ILockable lockable, Func<T> func)
        {
            //using (UsingDataBase.Set(lockable as IServiceProvider, authentication, true))
            {
                if (lockable is IDispatcherObject dispatcherObject)
                {
                    return dispatcherObject.Dispatcher.InvokeAsync(func);
                }
                else
                {
                    return Task.Run(func);
                }
            }
        }
    }
}

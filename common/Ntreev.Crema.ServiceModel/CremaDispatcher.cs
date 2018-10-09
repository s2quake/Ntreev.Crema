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

#pragma warning disable 0612
using Ntreev.Crema.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Ntreev.Crema.ServiceModel
{
    public sealed class CremaDispatcher
    {
        private Dispatcher dispatcher;
        private DispatcherFrame dispatcherFrame;

        public CremaDispatcher(object owner)
        {
            var eventSet = new ManualResetEvent(false);
            this.Owner = owner;
            var thread = new Thread(() =>
            {
                this.dispatcher = Dispatcher.CurrentDispatcher;
                this.dispatcher.UnhandledException += Dispatcher_UnhandledException;
                this.dispatcherFrame = new DispatcherFrame(true);
                eventSet.Set();
                try
                {
                    Dispatcher.PushFrame(this.dispatcherFrame);
                }
                catch
                {
                    this.dispatcher = null;
                    this.dispatcherFrame = null;
                }
            })
            {
                Name = owner.ToString()
            };
            thread.Start();
            eventSet.WaitOne();
        }

        public CremaDispatcher(object owner, Dispatcher dispatcher)
        {
            this.Owner = owner;
            this.dispatcher = dispatcher;
        }

        public void VerifyAccess()
        {
            this.dispatcher.VerifyAccess();
        }

        public bool CheckAccess()
        {
            return this.dispatcher.CheckAccess();
        }

        public void Invoke(Action action)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                this.InvokeUnix(action);
            }
            else
            {
                this.InvokeDefault(action);
            }
        }

        public Task InvokeAsync(Action action)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return this.InvokeAsyncUnix(action);
            }
            else
            {
                return this.InvokeAsyncDefault(action);
            }
        }

        public void InvokeTask(Func<Task> callback)
        {
            this.dispatcher.Invoke(callback, DispatcherPriority.Send).Wait();
        }

        public Task InvokeTaskAsync(Func<Task> callback)
        {
            return this.dispatcher.Invoke(callback, DispatcherPriority.Send);
        }

        public Task<TResult> InvokeTaskAsync<TResult>(Func<Task<TResult>> callback)
        {
            return this.dispatcher.Invoke(callback, DispatcherPriority.Send);
        }

        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return this.InvokeUnix(callback);
            }
            else
            {
                return this.InvokeDefault(callback);
            }
        }

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return this.InvokeAsyncUnix(callback);
            }
            else
            {
                return this.InvokeAsyncDefault(callback);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool sync)
        {
            if (this.dispatcher != null)
            {
                if (this.dispatcherFrame != null)
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        this.dispatcherFrame.Continue = false;
                        this.dispatcher.InvokeShutdown();
                        this.dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => { }));
                    }
                    else
                    {
                        if (sync == true)
                        {
                            this.dispatcher.InvokeShutdown();
                        }
                        else
                        {
                            this.dispatcherFrame.Continue = false;
                        }
                    }
                    this.dispatcherFrame = null;
                }
                this.dispatcher = null;
            }
        }

        public string Name
        {
            get { return this.Owner.ToString(); }
        }

        public object Owner { get; }

        public static implicit operator Dispatcher(CremaDispatcher dispatcher)
        {
            return dispatcher.dispatcher;
        }

        private void InvokeDefault(Action action)
        {
            this.dispatcher.Invoke(action, DispatcherPriority.Send);
        }

        private Task InvokeAsyncDefault(Action action)
        {
            return this.dispatcher.InvokeAsync(action, DispatcherPriority.Send).Task;
        }

        private TResult InvokeDefault<TResult>(Func<TResult> callback)
        {
            return this.dispatcher.Invoke(callback, DispatcherPriority.Send);
        }

        private Task<TResult> InvokeAsyncDefault<TResult>(Func<TResult> callback)
        {
            return this.dispatcher.InvokeAsync(callback, DispatcherPriority.Send).Task;
        }

        private void InvokeUnix(Action action)
        {
            if (this.dispatcher.CheckAccess() == true)
            {
                action();
                return;
            }
            else
            {
                var func = new Func<object>(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        return e;
                    }
                    return null;
                });
                var eventSet = new ManualResetEvent(false);
                var result = this.dispatcher.BeginInvoke(DispatcherPriority.Send, func);
                result.Completed += (s, e) => eventSet.Set();
                if (result.Status != DispatcherOperationStatus.Completed)
                    eventSet.WaitOne();
                if (result.Result is Exception ex)
                    throw ex;
            }
        }

        private Task InvokeAsyncUnix(Action action)
        {
            return Task.Run(() => this.Invoke(action));
        }

        private TResult InvokeUnix<TResult>(Func<TResult> callback)
        {
            if (this.dispatcher.CheckAccess() == true)
            {
                return callback();
            }
            else
            {
                var func = new Func<object>(() =>
                {
                    try
                    {
                        return callback();
                    }
                    catch (Exception e)
                    {
                        return new ExceptionHost(e);
                    }
                });
                var eventSet = new ManualResetEvent(false);
                var result = this.dispatcher.BeginInvoke(DispatcherPriority.Send, func);
                result.Completed += (s, e) => eventSet.Set();
                if (result.Status != DispatcherOperationStatus.Completed)
                    eventSet.WaitOne();
                if (result.Result is ExceptionHost host)
                    throw host.Exception;
                return (TResult)result.Result;
            }
        }

        private Task<TResult> InvokeAsyncUnix<TResult>(Func<TResult> callback)
        {
            return Task.Run(() => this.Invoke(callback));
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        #region classes

        class ExceptionHost
        {
            public ExceptionHost(Exception exception)
            {
                this.Exception = exception;
            }

            public Exception Exception { get; }
        }

        #endregion
    }
}

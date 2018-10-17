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

namespace Ntreev.Crema.ServiceModel
{
    public sealed class CremaDispatcher
    {
        private readonly CremaDispatcherScheduler scheduler;
        private readonly TaskFactory factory;
        private readonly CancellationTokenSource cancellationToken;

        public CremaDispatcher(object owner)
        {
            var eventSet = new ManualResetEvent(false);
            this.cancellationToken = new CancellationTokenSource();
            this.scheduler = new CremaDispatcherScheduler(this.cancellationToken.Token);
            this.factory = new TaskFactory(this.cancellationToken.Token, TaskCreationOptions.None, TaskContinuationOptions.None, this.scheduler);
            this.Owner = owner;
            this.Thread = new Thread(() =>
            {
                eventSet.Set();
                this.scheduler.Run();
                this.Disposed?.Invoke(this, EventArgs.Empty);
            })
            {
                Name = owner.ToString()
            };
            this.Thread.Start();
            eventSet.WaitOne();
        }

        public override string ToString()
        {
            return $"{this.Owner}";
        }

        public void VerifyAccess()
        {
            if (!this.CheckAccess())
            {
                throw new InvalidOperationException("The calling thread cannot access this object because a different thread owns it.");
            }
        }

        public bool CheckAccess()
        {
            return this.Thread == Thread.CurrentThread;
        }

        public void Invoke(Action action)
        {
            if (this.CheckAccess() == true)
            {
                action();
            }
            else
            {
                var task = this.factory.StartNew(action);
                task.Wait();
            }
        }

        public Task InvokeAsync(Action action)
        {
            return this.factory.StartNew(action);
        }

        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            if (this.CheckAccess() == true)
            {
                return callback();
            }
            else
            {
                var task = this.factory.StartNew(callback);
                task.Wait();
                return task.Result;
            }
        }

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            return this.factory.StartNew(callback);
        }

        public void Dispose()
        {
            this.cancellationToken.Cancel();
        }

        public async Task DisposeAsync()
        {
            var task = this.factory.StartNew(() => { });
            this.cancellationToken.Cancel();
            await task;
        }

        public string Name
        {
            get { return this.Owner.ToString(); }
        }

        public object Owner { get; }

        public Thread Thread { get; }

        public event EventHandler Disposed;
    }
}

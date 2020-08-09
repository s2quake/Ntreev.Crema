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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Bot
{
    public abstract class AutobotBase : IServiceProvider
    {
        private readonly TaskContext taskContext = new TaskContext();
        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private IEnumerable<ITaskProvider> taskProviders;
        private CremaDispatcher dispatcher;

        public AutobotBase(string autobotID)
        {
            this.AutobotID = autobotID;
            this.dispatcher = new CremaDispatcher(this);
            this.MinSleepTime = 1;
            this.MaxSleepTime = 10;
        }

        public override string ToString()
        {
            return $"Autobot: {this.AutobotID}";
        }

        public void Cancel()
        {
            this.cancelTokenSource.Cancel();
        }

        public async Task CancelAsync()
        {
            this.cancelTokenSource.Cancel();
            while (this.dispatcher != null)
            {
                await Task.Delay(1);
            }
        }

        public async Task LoginAsync()
        {
            var authentication = await this.OnLoginAsync();
            authentication.Expired += Authentication_Expired;
            this.taskContext.Authentication = authentication;
        }

        public async Task LogoutAsync()
        {
            await this.OnLogoutAsync(this.taskContext.Authentication);
            this.taskContext.Authentication = null;
        }

        public int MinSleepTime { get; set; }

        public int MaxSleepTime { get; set; }

        public bool IsOnline => this.taskContext.Authentication != null;

        public string AutobotID { get; }

        public abstract AutobotServiceBase Service { get; }

        public bool AllowException
        {
            get => this.taskContext.AllowException;
            set => this.taskContext.AllowException = value;
        }

        public abstract object GetService(Type serviceType);

        public Task ExecuteAsync(IEnumerable<ITaskProvider> taskProviders)
        {
            this.taskProviders = taskProviders;
            this.taskContext.Push(this);
            return this.dispatcher.InvokeAsync(async () => await this.ProcessAsync());
        }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        protected abstract Task<Authentication> OnLoginAsync();

        protected abstract Task OnLogoutAsync(Authentication authentication);

        private async Task InvokeTaskAsync(MethodInfo method, ITaskProvider taskProvider, object target)
        {
            try
            {
                if (method != null)
                {
                    var result = method.Invoke(taskProvider, new object[] { target, this.taskContext });
                    if (result is Task task)
                    {
                        await task;
                    }
                }
            }
            catch
            {

            }
            finally
            {
                this.taskContext.DoTask();
            }
        }

        private async Task ProcessAsync()
        {
            try
            {
                while (this.cancelTokenSource.IsCancellationRequested == false)
                {
                    var sleep = RandomUtility.Next(this.MinSleepTime, this.MaxSleepTime);
                    Thread.Sleep(sleep);

                    if (this.taskContext.Target == null)
                    {
                        this.taskContext.Push(this);
                    }

                    var taskProvider = RandomTaskProvider(this.taskContext.Target);

                    try
                    {
                        if (this.cancelTokenSource.IsCancellationRequested == true)
                            break;
                        await taskProvider.InvokeAsync(this.taskContext);
                    }
                    catch
                    {
                        this.taskContext.Complete(this.taskContext.Target);
                        continue;
                    }

                    if (this.taskContext.Target != null && taskProvider.TargetType.IsAssignableFrom(this.taskContext.Target.GetType()) == true)
                    {
                        var method = RandomMethod(taskProvider);
                        if (this.cancelTokenSource.IsCancellationRequested == true)
                            break;
                        await this.InvokeTaskAsync(method, taskProvider, this.taskContext.Target);
                    }
                }
                if (this.taskContext.Authentication != null)
                {
                    await this.OnLogoutAsync(this.taskContext.Authentication);
                }
                this.dispatcher.Dispose();
                this.dispatcher = null;
                this.OnDisposed(EventArgs.Empty);
            }
            catch (Exception e)
            {
                CremaLog.Fatal(e);
            }
        }

        private static MethodInfo RandomMethod(ITaskProvider taskProvider)
        {
            var weight = RandomUtility.Next(100) + 1;
            var methods = taskProvider.GetType().GetMethods();
            return methods.RandomOrDefault((item) => Predicate(item));

            bool Predicate(MethodInfo methodInfo)
            {
                if (methodInfo.IsStatic == true)
                    return false;

                if (methodInfo.ReturnType != typeof(void) && methodInfo.ReturnType != typeof(Task))
                    return false;

                var attr = methodInfo.GetCustomAttribute<TaskMethodAttribute>();
                if (attr == null)
                    return false;

                if (attr.Weight < weight)
                    return false;

                var parameters = methodInfo.GetParameters();
                if (parameters.Count() != 2)
                    return false;

                return parameters[1].ParameterType == typeof(TaskContext);
            }
        }

        private ITaskProvider RandomTaskProvider(object target)
        {
            return this.taskProviders.WeightedRandom(SelectWeight, Predicate);

            bool Predicate(ITaskProvider taskProvider)
            {
                if (taskProvider.TargetType.IsAssignableFrom(target.GetType()) == false)
                    return false;
                return true;
            }

            static int SelectWeight(ITaskProvider predicate)
            {
                if (!(Attribute.GetCustomAttribute(predicate.GetType(), typeof(TaskClassAttribute)) is TaskClassAttribute attr))
                    return 100;
                return attr.Weight;
            }
        }

        private void Authentication_Expired(object sender, EventArgs e)
        {
            this.taskContext.Authentication = null;
        }
    }
}
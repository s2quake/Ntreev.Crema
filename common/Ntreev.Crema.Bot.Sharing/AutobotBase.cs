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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ntreev.Library.Random;
using Ntreev.Library;
using Ntreev.Crema.ServiceModel;
using System.Threading;
using System.Reflection;
using Ntreev.Crema.Services;
using System.Security;

namespace Ntreev.Crema.Bot
{
    public abstract class AutobotBase : IServiceProvider
    {
        private readonly static object error = new object();
        private readonly string autobotID;
        private readonly TaskContext taskContext = new TaskContext();
        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private IEnumerable<ITaskProvider> taskProviders;

        public AutobotBase(string autobotID)
        {
            this.autobotID = autobotID;
            this.MinSleepTime = 1;
            this.MaxSleepTime = 10;
        }

        public void Cancel()
        {
            this.cancelTokenSource.Cancel();
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

        public int MinSleepTime
        {
            get; set;
        }

        public int MaxSleepTime
        {
            get; set;
        }

        public bool IsOnline
        {
            get { return this.taskContext.Authentication != null; }
        }

        public string AutobotID
        {
            get { return this.autobotID; }
        }

        public abstract AutobotServiceBase Service
        {
            get;
        }

        public bool AllowException
        {
            get { return this.taskContext.AllowException; }
            set { this.taskContext.AllowException = value; }
        }

        public abstract object GetService(Type serviceType);

        public async Task ExecuteAsync(IEnumerable<ITaskProvider> taskProviders)
        {
            this.taskProviders = taskProviders;
            this.taskContext.Push(this);
            await Task.Run(async () => await this.Execute(taskProviders));
        }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        protected abstract Task<Authentication> OnLoginAsync();

        protected abstract Task OnLogoutAsync(Authentication authentication);

        private void InvokeTask(MethodInfo method, ITaskProvider taskProvider, object target)
        {
            try
            {
                if (method != null)
                {
                    var result = method.Invoke(taskProvider, new object[] { target, this.taskContext });
                    if (result is Task task)
                    {
                        task.Wait();
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

        private async Task Execute(IEnumerable<ITaskProvider> taskProviders)
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
                        var task = taskProvider.InvokeAsync(this.taskContext);
                        task.Wait();
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
                        this.InvokeTask(method, taskProvider, this.taskContext.Target);
                    }
                }
                if (this.taskContext.Authentication != null)
                {
                    await this.OnLogoutAsync(this.taskContext.Authentication);
                }
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

            int SelectWeight(ITaskProvider predicate)
            {
                var attr = Attribute.GetCustomAttribute(predicate.GetType(), typeof(TaskClassAttribute)) as TaskClassAttribute;
                if (attr == null)
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
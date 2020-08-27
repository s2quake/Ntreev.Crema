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

using JSSoft.Crema.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript
{
    public abstract class ScriptMethodBase : IScriptMethod
    {
        private Delegate delegateVariable;

        protected ScriptMethodBase()
        {
            this.Name = ToCamelCase(this.GetType().Name);
        }

        protected ScriptMethodBase(string name)
        {
            this.Name = name;
        }

        protected ScriptMethodBase(ICremaHost cremaHost)
        {
            this.Name = ToCamelCase(this.GetType().Name);
            this.CremaHost = cremaHost;
        }

        public string Name { get; }

        public Delegate Delegate
        {
            get
            {
                if (this.delegateVariable == null)
                {
                    this.delegateVariable = this.CreateDelegate();
                }
                return this.delegateVariable;
            }
        }

        public IScriptMethodContext Context
        {
            get;
            set;
        }

        protected abstract Delegate CreateDelegate();

        protected virtual void OnDisposed()
        {

        }

        protected virtual void OnInitialized()
        {

        }

        protected ICremaHost CremaHost { get; }

        private static string ToCamelCase(string text)
        {
            var name = Regex.Replace(text, @"^([A-Z])", MatchEvaluator);

            return Regex.Replace(name, @"(Method)$", string.Empty);
        }

        private static string MatchEvaluator(Match match)
        {
            return match.Value.ToLower();
        }

        internal void Dispose()
        {
            this.OnDisposed();
        }

        internal void Initialize()
        {
            this.OnInitialized();
        }
    }

    public abstract class ScriptActionBase<T> : ScriptMethodBase
    {
        public ScriptActionBase()
        {

        }

        public ScriptActionBase(string name)
            : base(name)
        {

        }

        public ScriptActionBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Action<T>(this.OnExecute);
        }

        [BaseDelegate]
        protected abstract void OnExecute(T arg);
    }

    public abstract class ScriptFuncBase<TResult> : ScriptMethodBase
    {
        public ScriptFuncBase()
        {

        }

        public ScriptFuncBase(string name)
            : base(name)
        {

        }

        public ScriptFuncBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Func<TResult>(this.OnExecute);
        }

        [BaseDelegate]
        protected abstract TResult OnExecute();
    }

    public abstract class ScriptActionTaskBase<T> : ScriptMethodBase
    {
        public ScriptActionTaskBase()
        {

        }

        public ScriptActionTaskBase(string name)
            : base(name)
        {

        }

        public ScriptActionTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Action<T>((arg) =>
            {
                var task = this.OnExecuteAsync(arg);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [BaseDelegate]
        protected abstract Task OnExecuteAsync(T arg);
    }

    public abstract class ScriptActionTaskBase<T1, T2> : ScriptMethodBase
    {
        public ScriptActionTaskBase()
        {

        }

        public ScriptActionTaskBase(string name)
            : base(name)
        {

        }

        public ScriptActionTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Action<T1, T2>((arg1, arg2) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [BaseDelegate]
        protected abstract Task OnExecuteAsync(T1 arg1, T2 arg2);
    }

    public abstract class ScriptActionTaskBase<T1, T2, T3> : ScriptMethodBase
    {
        public ScriptActionTaskBase()
        {

        }

        public ScriptActionTaskBase(string name)
            : base(name)
        {

        }

        public ScriptActionTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Action<T1, T2, T3>((arg1, arg2, arg3) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2, arg3);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [BaseDelegate]
        protected abstract Task OnExecuteAsync(T1 arg1, T2 arg2, T3 arg3);
    }

    public abstract class ScriptActionTaskBase<T1, T2, T3, T4> : ScriptMethodBase
    {
        public ScriptActionTaskBase()
        {

        }

        public ScriptActionTaskBase(string name)
            : base(name)
        {

        }

        public ScriptActionTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Action<T1, T2, T3, T4>((arg1, arg2, arg3, arg4) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2, arg3, arg4);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [BaseDelegate]
        protected abstract Task OnExecuteAsync(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public abstract class ScriptActionTaskBase<T1, T2, T3, T4, T5> : ScriptMethodBase
    {
        public ScriptActionTaskBase()
        {

        }

        public ScriptActionTaskBase(string name)
            : base(name)
        {

        }

        public ScriptActionTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Action<T1, T2, T3, T4, T5>((arg1, arg2, arg3, arg4, arg5) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2, arg3, arg4, arg5);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [BaseDelegate]
        protected abstract Task OnExecuteAsync(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }

    public abstract class ScriptFuncTaskBase<TResult> : ScriptMethodBase
    {
        public ScriptFuncTaskBase()
        {

        }

        public ScriptFuncTaskBase(string name)
            : base(name)
        {

        }

        public ScriptFuncTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Func<TResult>(() =>
            {
                var task = this.OnExecuteAsync();
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
                return task.Result;
            });
        }

        [BaseDelegate]
        protected abstract Task<TResult> OnExecuteAsync();
    }

    public abstract class ScriptFuncTaskBase<T, TResult> : ScriptMethodBase
    {
        public ScriptFuncTaskBase()
        {

        }

        public ScriptFuncTaskBase(string name)
            : base(name)
        {

        }

        public ScriptFuncTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Func<T, TResult>((arg) =>
            {
                var task = this.OnExecuteAsync(arg);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
                return task.Result;
            });
        }

        [BaseDelegate]
        protected abstract Task<TResult> OnExecuteAsync(T arg);
    }

    public abstract class ScriptFuncTaskBase<T1, T2, TResult> : ScriptMethodBase
    {
        public ScriptFuncTaskBase()
        {

        }

        public ScriptFuncTaskBase(string name)
            : base(name)
        {

        }

        public ScriptFuncTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Func<T1, T2, TResult>((arg1, arg2) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
                return task.Result;
            });
        }

        [BaseDelegate]
        protected abstract Task<TResult> OnExecuteAsync(T1 arg1, T2 arg2);
    }

    public abstract class ScriptFuncTaskBase<T1, T2, T3, TResult> : ScriptMethodBase
    {
        public ScriptFuncTaskBase()
        {

        }

        public ScriptFuncTaskBase(string name)
            : base(name)
        {

        }

        public ScriptFuncTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Func<T1, T2, T3, TResult>((arg1, arg2, arg3) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2, arg3);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
                return task.Result;
            });
        }

        [BaseDelegate]
        protected abstract Task<TResult> OnExecuteAsync(T1 arg1, T2 arg2, T3 arg3);
    }

    public abstract class ScriptFuncTaskBase<T1, T2, T3, T4, TResult> : ScriptMethodBase
    {
        public ScriptFuncTaskBase()
        {

        }

        public ScriptFuncTaskBase(string name)
            : base(name)
        {

        }

        public ScriptFuncTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Func<T1, T2, T3, T4, TResult>((arg1, arg2, arg3, arg4) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2, arg3, arg4);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
                return task.Result;
            });
        }

        [BaseDelegate]
        protected abstract Task<TResult> OnExecuteAsync(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public abstract class ScriptFuncTaskBase<T1, T2, T3, T4, T5, TResult> : ScriptMethodBase
    {
        public ScriptFuncTaskBase()
        {

        }

        public ScriptFuncTaskBase(string name)
            : base(name)
        {

        }

        public ScriptFuncTaskBase(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override sealed Delegate CreateDelegate()
        {
            return new Func<T1, T2, T3, T4, T5, TResult>((arg1, arg2, arg3, arg4, arg5) =>
            {
                var task = this.OnExecuteAsync(arg1, arg2, arg3, arg4, arg5);
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
                return task.Result;
            });
        }

        [BaseDelegate]
        protected abstract Task<TResult> OnExecuteAsync(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}

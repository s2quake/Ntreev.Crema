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
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JSSoft.Crema.Bot
{
    public sealed class TaskContext
    {
        private readonly Stack<TaskItem> stacks = new Stack<TaskItem>();

        internal TaskContext()
        {

        }

        public Authentication Authentication { get; internal set; }

        public void Push(object target)
        {
            this.Push(target, RandomUtility.Next(10));
        }

        public void Push(object target, int count)
        {
            this.stacks.Push(new TaskItem()
            {
                Target = target,
                Count = count,
            });
        }

        public void DoTask()
        {
            this.stacks.Peek().Count--;
        }

        /// <summary>
        /// 작업 스택에 대상 인스턴스가 push 될때 일정 수준의 작업 횟수가 지정되는데 
        /// 작업 횟수를 다 완료했다고 강제로 설정한다.
        /// </summary>
        public void Complete(object target)
        {
            if (this.stacks.Peek().Target != target)
                throw new Exception();
            this.stacks.Peek().Count = 0;
        }

        /// <summary>
        /// 현재 작업 스택에서 에서 즉시 대상 인스턴스를 제거한다.
        /// </summary>
        public void Pop(object target)
        {
            if (this.stacks.Peek().Target != target)
                throw new Exception();
            this.stacks.Pop();
            this.DoTask();
        }

        public bool IsCompleted(object target)
        {
            if (this.stacks.Peek().Target != target)
                throw new ArgumentException();
            return this.stacks.Peek().Count <= 0;
        }

        public object Target
        {
            get
            {
                if (this.stacks.Any())
                    return this.stacks.Peek().Target;
                return null;
            }
        }

        public object State
        {
            get; set;
        }

        public bool AllowException
        {
            get; set;
        }

        #region classes

        class TaskItem
        {
            public object Target { get; set; }

            public int Count { get; set; }

            public override string ToString()
            {
                return this.Target.ToString();
            }
        }

        #endregion
    }
}

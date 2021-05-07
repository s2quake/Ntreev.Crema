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

using JSSoft.Crema.ServiceModel;
using System;
using System.Timers;

namespace JSSoft.Crema.Services
{
    class ShutdownTimer
    {
        private readonly Timer timer = new Timer() { Interval = 1000 };

        public ShutdownTimer(ShutdownContext context)
        {
            this.Context = context;
            this.DateTime = DateTime.Now.AddMilliseconds(context.Milliseconds);
            this.timer.Elapsed += Timer_Elapsed;
        }

        public void Start() => this.timer.Start();

        public void Stop()
        {
            this.timer.Stop();
            this.timer.Dispose();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.Context.Cancellation.IsCancellationRequested == true)
            {
                this.OnCancelled(EventArgs.Empty);
            }
            else if (DateTime.Now >= this.DateTime)
            {
                this.OnDone(EventArgs.Empty);
            }
            else
            {
                this.OnElapsed(EventArgs.Empty);
            }
        }

        public ShutdownContext Context { get; }

        public DateTime DateTime { get; }

        public bool IsRestart => this.Context.IsRestart;

        public event EventHandler Done;

        public event EventHandler Elapsed;

        public event EventHandler Cancelled;

        protected virtual void OnDone(EventArgs e)
        {
            this.Done?.Invoke(this, e);
        }

        protected virtual void OnElapsed(EventArgs e)
        {
            this.Elapsed?.Invoke(this, e);
        }

        protected virtual void OnCancelled(EventArgs e)
        {
            this.Cancelled?.Invoke(this, e);
        }
    }
}

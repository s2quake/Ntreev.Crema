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
    class ShutdownTimer : IDisposable
    {
        private readonly Timer timer = new() { Interval = 1000 };
        private ShutdownContext shutdownContext;

        public ShutdownTimer()
        {
            this.timer.Elapsed += Timer_Elapsed;
        }

        public void Start(ShutdownContext shutdownContext)
        {
            this.shutdownContext = shutdownContext;
            this.DateTime = DateTime.Now.AddMilliseconds(shutdownContext.Milliseconds);
            if (this.timer.Enabled == true)
                this.timer.Stop();
            this.timer.Start();
        }

        public void Stop()
        {
            if (this.timer.Enabled == true)
                this.timer.Stop();
        }

        public void InvokeExceptionHandler(Exception e)
        {
            this.shutdownContext.InvokeShutdownException(e);
        }

        public CloseReason CloseReason => this.shutdownContext.IsRestart ? CloseReason.Restart : CloseReason.None;

        public string Message => this.shutdownContext.Message;

        public DateTime DateTime { get; private set; }

        public bool Enabled => this.timer.Enabled;

        public event EventHandler Done;

        public event EventHandler Elapsed;

        protected virtual void OnDone(EventArgs e)
        {
            this.Done?.Invoke(this, e);
        }

        protected virtual void OnElapsed(EventArgs e)
        {
            this.Elapsed?.Invoke(this, e);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now >= this.DateTime)
            {
                this.OnDone(EventArgs.Empty);
            }
            else
            {
                this.OnElapsed(EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }
    }
}

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
using System.Threading.Tasks;
using System.Timers;

namespace Ntreev.Crema.Services
{
    class PingTimer : IDisposable
    {
        private readonly static double pingInterval = 15000;
        private readonly Func<bool> action;
        private Timer timer;
        private bool isProgressing;

        public PingTimer(Func<bool> action, int timeout)
        {
            this.action = action;
            if (timeout > 0)
            {
                this.timer = new Timer(Math.Max(pingInterval / 2, 1000));
                this.timer.Elapsed += Timer_Elapsed;
                this.timer.Start();
            }
        }

        public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Dispose();
            }
            this.timer = null;
        }

        public event EventHandler Faulted;

        protected virtual void OnFaulted(EventArgs e)
        {
            this.Faulted?.Invoke(this, e);
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.isProgressing == true)
                return;
            try
            {
                this.isProgressing = true;
                await Task.Run(this.action);
            }
            catch
            {
                this.Dispose();
                this.OnFaulted(EventArgs.Empty);
            }
            finally
            {
                this.isProgressing = false;
            }
        }
    }
}

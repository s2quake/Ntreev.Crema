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
using System.Threading.Tasks;
using System.Timers;

namespace JSSoft.Crema.Services
{
    class ShutdownTimer : IDisposable
    {
        private readonly Timer timer = new() { Interval = 100 };
        private readonly CremaHost cremaHost;
        private string address;
        private ShutdownContext shutdownContext;

        public ShutdownTimer(CremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Closed += CremaHost_Closed;
            this.address = cremaHost.Address;
        }

        public void Start(ShutdownContext shutdownContext, string address)
        {
            this.shutdownContext = shutdownContext;
            this.address = address;
            if (this.timer.Enabled == true)
                this.timer.Stop();
        }

        public void Stop()
        {
            if (this.timer.Enabled == true)
                this.timer.Stop();
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }

        public event EventHandler Done;

        protected virtual void OnDone(EventArgs e)
        {
            this.Done?.Invoke(this, e);
        }

        private void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            if (e.Reason == CloseReason.Restart)
            {
                this.TryOpenAsync();
            }
        }

        private async void TryOpenAsync()
        {
            var count = 0;
            var address = this.address;
            while (true)
            {
                await Task.Delay(1);
                try
                {
                    var hostname = AddressUtility.GetIPAddress(address);
                    var port = AddressUtility.GetPort(address);
                    using var client = new System.Net.Sockets.TcpClient(hostname, 4002);
                    this.OnDone(EventArgs.Empty);
                    break;
                }
                catch (Exception e)
                {
                    count++;
                    if (count > 5)
                    {
                        this.shutdownContext.InvokeShutdownException(e);
                        break;
                    }
                }
            }
        }
    }
}

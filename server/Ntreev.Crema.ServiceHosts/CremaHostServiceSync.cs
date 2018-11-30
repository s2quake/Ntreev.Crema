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
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts
{
    partial class CremaHostService
    {
        public ResultBase<Guid> Subscribe(string userID, byte[] password, string version, string platformID, string culture)
        {
            return this.InvokeTask(Task.Run(() => this.SubscribeAsync(userID, password, version, platformID, culture)));
        }

        public ResultBase Unsubscribe()
        {
            return this.InvokeTask(Task.Run(() => this.UnsubscribeAsync()));
        }

        public ResultBase<string> GetVersion()
        {
            return this.InvokeTask(Task.Run(() => this.GetVersionAsync()));
        }

        public ResultBase<bool> IsOnline(string userID, byte[] password)
        {
            return this.InvokeTask(Task.Run(() => this.IsOnlineAsync(userID, password)));
        }

        public ResultBase<DataBaseInfo[]> GetDataBaseInfos()
        {
            return this.InvokeTask(Task.Run(() => this.GetDataBaseInfosAsync()));
        }

        public ResultBase<ServiceInfo> GetServiceInfo()
        {
            return this.InvokeTask(Task.Run(() => this.GetServiceInfoAsync()));
        }

        public ResultBase Shutdown(int milliseconds, ShutdownType shutdownType, string message)
        {
            return this.InvokeTask(Task.Run(() => this.ShutdownAsync(milliseconds, shutdownType, message)));
        }

        public ResultBase CancelShutdown()
        {
            return this.InvokeTask(Task.Run(() => this.CancelShutdownAsync()));
        }

        public bool IsAlive()
        {
            return this.InvokeTask(Task.Run(() => this.IsAliveAsync()));
        }

        private T InvokeTask<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}

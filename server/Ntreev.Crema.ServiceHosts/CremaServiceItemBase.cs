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
using Ntreev.Library.IO;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Ntreev.Crema.ServiceHosts
{
    public abstract class CremaServiceItemBase<T> : ICremaServiceItem, IDisposable
    {
        private readonly string sessionID;
        private ServiceHostBase host;

        protected CremaServiceItemBase(CremaService service)
        {
            this.Service = service;
            this.CremaHost = this.Service.GetService(typeof(ICremaHost)) as ICremaHost;
            this.LogService = this.CremaHost.GetService(typeof(ILogService)) as ILogService;
            OperationContext.Current.Host.Closing += Host_Closing;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;
            OperationContext.Current.Channel.Closed += Channel_Closed;
            this.host = OperationContext.Current.Host;
            this.Channel = OperationContext.Current.Channel;
            this.sessionID = OperationContext.Current.Channel.SessionId;
            this.Callback = OperationContext.Current.GetCallbackChannel<T>();
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            this.host.Closing -= Host_Closing;
            this.host = null;
            this.Channel = null;
            this.Callback = default(T);
            this.LogService.Debug($"[{this.OwnerID}] {this.GetType().Name} {nameof(ICremaServiceItem.CloseAsync)}");
        }

        protected void InvokeEvent(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                this.LogService.Error(e);
            }
        }

        protected T Callback { get; private set; }

        protected IContextChannel Channel { get; private set; }

        protected abstract void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo);

        protected string OwnerID { get; set; }

        protected CremaService Service { get; }

        protected ICremaHost CremaHost { get; }

        protected ILogService LogService { get; }

        private void Host_Closing(object sender, EventArgs e)
        {
            try
            {
                this.OnServiceClosed(SignatureDate.Empty, CloseInfo.Empty);
                this.Callback = default(T);
            }
            catch
            {

            }
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            this.Channel.Abort();
            this.Channel = null;
            this.Callback = default(T);
        }

        protected abstract Task OnCloseAsync(bool disconnect);

        private async Task CloseAsync(bool disconnect)
        {
            await this.OnCloseAsync(disconnect);
            if (disconnect == true)
            {
                if (this.Channel != null)
                {
                    this.Channel.Closed -= Channel_Closed;
                    this.Channel.Faulted -= Channel_Faulted;
                    this.Channel.Abort();
                }
                this.Channel = null;
                this.Callback = default(T);
            }
            this.LogService.Debug($"{this.GetType().Name}.{nameof(OnCloseAsync)}");
        }

        #region ICremaServiceItem

        Task ICremaServiceItem.CloseAsync(bool disconnect)
        {
            return this.CloseAsync(disconnect);
        }

        #endregion

        async void IDisposable.Dispose()
        {
            await this.CloseAsync(false);
        }

        #region IDisposable

        #endregion
    }
}

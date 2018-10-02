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
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Ntreev.Crema.ServiceHosts
{
    public abstract class CremaServiceItemBase<T> : ICremaServiceItem
    {
        private readonly ILogService logService;
        private readonly string sessionID;
        private ServiceHostBase host;

        protected CremaServiceItemBase(ILogService logService)
        {
            this.logService = logService;
            OperationContext.Current.Host.Closing += Host_Closing;
            this.host = OperationContext.Current.Host;
            this.Channel = OperationContext.Current.Channel;
            this.sessionID = OperationContext.Current.Channel.SessionId;
            this.Callback = OperationContext.Current.GetCallbackChannel<T>();
        }

        //public event EventHandler Disposed;

        protected void InvokeEvent(string userID, string exceptionUserID, Action action)
        {
            CremaService.Dispatcher?.InvokeAsync(() =>
            {
                if (this.sessionID == null || (userID != null && userID == exceptionUserID))
                    return;
                if (this.Channel != null)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        this.logService.Error(e);
                    }
                }
            });
        }

        //protected virtual void OnDisposed(EventArgs e)
        //{
        //    this.Disposed?.Invoke(this, e);
        //}

        protected T Callback { get; private set; }

        protected IContextChannel Channel { get; private set; }

        protected abstract void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo);

        protected string OwnerID { get; set; }

        private void Host_Closing(object sender, EventArgs e)
        {
            if (this.Callback != null)
            {
                if (this.Channel.State == CommunicationState.Opened)
                {
                    this.OnServiceClosed(SignatureDate.Empty, CloseInfo.Empty);
                }
                this.Callback = default(T);
            }
        }

        protected abstract Task OnAbortAsync(bool disconnect);

        async Task ICremaServiceItem.AbortAsync(bool disconnect)
        {
            if (this.host != null)
            {
                await this.OnAbortAsync(disconnect);
                //this.host.Closing -= Host_Closing;
                this.host = null;
                this.Channel = null;
                this.Callback = default(T);
                //this.OnDisposed(EventArgs.Empty);
                this.logService.Debug($"[{this.OwnerID}] {this.GetType().Name} {nameof(IDisposable.Dispose)}");
            }
        }

        #region IDisposable

        //void IDisposable.Dispose()
        //{
        //    this.host.Closing -= Host_Closing;
        //    this.host = null;
        //    this.Channel = null;
        //    this.Callback = default(T);
        //    this.OnDisposed(EventArgs.Empty);
        //    this.logService.Debug($"[{this.OwnerID}] {this.GetType().Name} {nameof(IDisposable.Dispose)}");
        //}

        #endregion
    }
}

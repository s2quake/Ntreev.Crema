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
    public abstract class CremaServiceItemBase<T> : ICremaServiceItem
    {
        private readonly ICremaHost cremaHost;
        private readonly ILogService logService;
        private readonly string sessionID;
        private ServiceHostBase host;

        protected CremaServiceItemBase(ICremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
            this.logService = cremaHost.GetService(typeof(ILogService)) as ILogService;
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
            this.logService.Debug($"[{this.OwnerID}] {this.GetType().Name} {nameof(ICremaServiceItem.CloseAsync)}");
        }

        protected void InvokeEvent(string userID, string exceptionUserID, Action action)
        {
            if (userID != null && userID == exceptionUserID)
                return;

            //CremaService.Dispatcher.InvokeAsync(() =>
            //{
            try
            {
                action();
            }
            catch (Exception e)
            {
                this.logService.Error(e);
            }
            //});
        }

        protected void InvokeEvent(string userID, string exceptionUserID, Action action, long id, string name)
        {
            if (userID != null && userID == exceptionUserID)
                return;

            //CremaService.Dispatcher.InvokeAsync(() =>
            //{
            try
            {
                action();
                if (name != null)
                {
                    if (userID != null)
                    {
                        var path = System.IO.Path.Combine(@"E:\Crema\repo\debug", userID, "ServerServiceLog.txt");
                        FileUtility.Prepare(path);
                        File.AppendAllText(path, $"{id}\t{name}{Environment.NewLine}");
                    }
                    else
                    {
                        //System.Diagnostics.Trace.WriteLine(name);
                    }

                }
            }
            catch (Exception e)
            {
                this.logService.Error(e);
            }
            //});
        }

        protected T Callback { get; private set; }

        protected IContextChannel Channel { get; private set; }

        protected abstract void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo);

        protected string OwnerID { get; set; }

        private void Host_Closing(object sender, EventArgs e)
        {
            this.OnServiceClosed(SignatureDate.Empty, CloseInfo.Empty);
            this.Callback = default(T);
        }

        private void Channel_Faulted(object sender, EventArgs e)
        {
            this.Channel.Abort();
            this.Channel = null;
            this.Callback = default(T);
        }

        protected abstract Task OnCloseAsync(bool disconnect);

        async Task ICremaServiceItem.CloseAsync(bool disconnect)
        {
            await this.OnCloseAsync(disconnect);
            this.logService.Debug($"{this.GetType().Name}.{nameof(OnCloseAsync)}");
        }
    }
}

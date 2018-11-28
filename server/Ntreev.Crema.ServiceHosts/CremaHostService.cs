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
using System.Security;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Ntreev.Crema.ServiceHosts.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    partial class CremaHostService : CremaServiceItemBase<ICremaHostEventCallback>, ICremaHostService
    {
        private readonly CremaService service;
        private Guid authenticationToken;
        private Authentication authentication;
        private long index = 0;

        public CremaHostService(CremaService service, ICremaHost cremaHost)
            : base(cremaHost)
        {
            this.service = service;
            this.CremaHost = cremaHost;
            this.LogService = cremaHost.GetService(typeof(ILogService)) as ILogService;
            this.LogService.Debug($"{nameof(CremaHostService)} Constructor");
            this.OwnerID = nameof(CremaHostService);
        }

        public async Task<ResultBase<Guid>> SubscribeAsync(string userID, byte[] password, string version, string platformID, string culture)
        {
            var result = new ResultBase<Guid>();
            try
            {
                var serverVersion = typeof(ICremaHost).Assembly.GetName().Version;
                var clientVersion = new Version(version);

                if (clientVersion < serverVersion)
                    throw new ArgumentException(Resources.Exception_LowerVersion, nameof(version));

                this.authenticationToken = await this.CremaHost.LoginAsync(userID, ToSecureString(userID, password));
                this.authentication = await this.CremaHost.AuthenticateAsync(this.authenticationToken);
                await this.authentication.AddRefAsync(this, (a) => this.CremaHost.LogoutAsync(a));
                this.OwnerID = this.authentication.ID;
                result.Value = this.authenticationToken;
                result.SignatureDate = this.authentication.SignatureDate;
                this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(SubscribeAsync)}");
            }
            catch (Exception e)
            {
                this.OwnerID = $"{userID} - failed";
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        private async void AuthenticationUtility_Disconnected(object sender, EventArgs e)
        {
            var authentication = sender as Authentication;
            if (this.authentication != null && this.authentication == authentication)
            {
                await this.CremaHost.LogoutAsync(this.authentication);
            }
        }

        public async Task<ResultBase> UnsubscribeAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.CremaHost.LogoutAsync(this.authentication);
                this.authentication = null;
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
                this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(UnsubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<ServiceInfo[]>> GetServiceInfosAsync()
        {
            var result = new ResultBase<ServiceInfo[]>();
            try
            {
                result.Value = await Task.Run(() => this.service.ServiceInfos);
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo[]>> GetDataBaseInfosAsync()
        {
            var result = new ResultBase<DataBaseInfo[]>();
            try
            {
                result.Value = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext.Select(item => item.DataBaseInfo).ToArray());
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<string>> GetVersionAsync()
        {
            var result = new ResultBase<string>();
            try
            {
                result.Value = await Task.Run(() => AppUtility.ProductVersion.ToString());
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase<bool>> IsOnlineAsync(string userID, byte[] password)
        {
            var result = new ResultBase<bool>();
            try
            {
                var userContext = this.CremaHost.GetService(typeof(IUserContext)) as IUserContext;
                var text = Encoding.UTF8.GetString(password);
                result.Value = await userContext.IsOnlineUserAsync(userID, StringUtility.ToSecureString(StringUtility.Decrypt(text, userID)));
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> ShutdownAsync(int milliseconds, ShutdownType shutdownType, string message)
        {
            var result = new ResultBase();
            try
            {
                await this.CremaHost.ShutdownAsync(this.authentication, milliseconds, shutdownType, message);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> CancelShutdownAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.CremaHost.CancelShutdownAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public ICremaHost CremaHost { get; }

        public ILogService LogService { get; }

        protected override void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = signatureDate };
            this.Callback?.OnServiceClosed(callbackInfo, closeInfo);
        }

        private static SecureString ToSecureString(string userID, byte[] password)
        {
            var text = Encoding.UTF8.GetString(password);
            return StringToSecureString(StringUtility.Decrypt(text, userID));
        }

        private static SecureString StringToSecureString(string value)
        {
            var secureString = new SecureString();
            foreach (var item in value)
            {
                secureString.AppendChar(item);
            }
            return secureString;
        }

        private IDataBaseContext DataBaseContext => this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;

        #region ICremaServiceItem

        protected override async Task OnCloseAsync(bool disconnect)
        {
            await Task.Delay(1);
            if (this.authentication != null)
            {
                this.authentication = null;
            }
        }

        #endregion
    }
}

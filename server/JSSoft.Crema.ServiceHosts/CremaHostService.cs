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

using JSSoft.Crema.ServiceHosts.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts
{
    class CremaHostService : CremaServiceItemBase<ICremaHostEventCallback>, ICremaHostService
    {
        private Peer peer;

        public CremaHostService(CremaService service, ICremaHostEventCallback callback)
            : base(service, callback)
        {
            this.LogService.Debug($"{nameof(CremaHostService)} Constructor");
            this.OwnerID = nameof(CremaHostService);
            this.CremaHost.CloseRequested += CremaHost_CloseRequested;
        }

        private void CremaHost_CloseRequested(object sender, CloseRequestedEventArgs e)
        {
            // this.authenticationByToken.Clear();
        }

        public async Task DisposeAsync()
        {
            if (this.peer != null)
            {
                foreach (var item in this.peer.Authentications)
                {
                    await this.CremaHost.LogoutAsync(item);
                }
                this.peer.Dispose();
                this.peer = null;
            }
            // this.authenticationByToken.Clear();
            this.CremaHost.CloseRequested -= CremaHost_CloseRequested;
        }

        public async Task<ResultBase<Guid>> SubscribeAsync(string version, string platformID, string culture)
        {
            if (this.peer != null)
                throw new InvalidOperationException();
            var serverVersion = typeof(ICremaHost).Assembly.GetName().Version;
            var clientVersion = new Version(version);
            var token = Guid.NewGuid();
            if (clientVersion < serverVersion)
                throw new ArgumentException(Resources.Exception_LowerVersion, nameof(version));

            await Task.Delay(1);
            this.peer = new Peer(token);
            this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(SubscribeAsync)}");
            return new ResultBase<Guid>()
            {
                Value = token,
                SignatureDate = new SignatureDateProvider(this.OwnerID)
            };
        }

        public async Task<ResultBase<Guid>> LoginAsync(string userID, byte[] password)
        {
            var authenticationToken = await this.CremaHost.LoginAsync(userID, ToSecureString(userID, password));
            var authentication = await this.CremaHost.AuthenticateAsync(authenticationToken);
            this.peer.Add(authenticationToken, authentication);
            this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(LoginAsync)}");
            return new ResultBase<Guid>()
            {
                Value = authenticationToken,
                SignatureDate = authentication.SignatureDate
            };
        }

        public async Task<ResultBase> LogoutAsync(Guid authenticationToken)
        {
            var authentication = this.peer[authenticationToken];
            var authenticationID = authentication.ID;
            await this.CremaHost.LogoutAsync(authentication);
            this.peer.Remove(authenticationToken);
            this.OwnerID = nameof(CremaHostService);
            this.LogService.Debug($"[{authenticationID}] {nameof(CremaHostService)} {nameof(LogoutAsync)}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider(authenticationID)
            };
        }

        public async Task<ResultBase> LogoutAsync(string userID, byte[] password)
        {
            await this.CremaHost.LogoutAsync(userID, ToSecureString(userID, password));
            this.LogService.Debug($"[{userID}] {nameof(CremaHostService)} {nameof(LogoutAsync)}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider(userID)
            };
        }

        public async Task<ResultBase> UnsubscribeAsync(Guid token)
        {
            if (this.peer is null)
                throw new InvalidOperationException();
            await Task.Delay(1);
            this.peer.Dispose();
            this.peer = null;
            this.LogService.Debug($"[{this.OwnerID}] {nameof(CremaHostService)} {nameof(UnsubscribeAsync)}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider(this.OwnerID)
            };
        }

        public async Task<ResultBase<ServiceInfo>> GetServiceInfoAsync()
        {
            var value = await Task.Run(() => this.Service.ServiceInfo);
            return new ResultBase<ServiceInfo>()
            {
                SignatureDate = new SignatureDateProvider(this.OwnerID),
                Value = value
            };
        }

        public async Task<ResultBase<DataBaseInfo[]>> GetDataBaseInfosAsync()
        {
            var value = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext.Select(item => item.DataBaseInfo).ToArray());
            return new ResultBase<DataBaseInfo[]>()
            {
                SignatureDate = new SignatureDateProvider(this.OwnerID),
                Value = value
            };
        }

        public async Task<ResultBase<string>> GetVersionAsync()
        {
            var value = await Task.Run(() => AppUtility.ProductVersion.ToString());
            return new ResultBase<string>()
            {
                SignatureDate = new SignatureDateProvider(this.OwnerID),
                Value = value
            };
        }

        public async Task<ResultBase> ShutdownAsync(Guid authenticationToken, int milliseconds, bool isRestart, string message)
        {
            var authentication = this.peer[authenticationToken];
            var shutdownContext = new ShutdownContext()
            {
                Milliseconds = milliseconds,
                IsRestart = isRestart,
                Message = message
            };
            await this.CremaHost.ShutdownAsync(authentication, shutdownContext);
            return new ResultBase()
            {
                SignatureDate = authentication.SignatureDate
            };
        }

        public async Task<ResultBase> CancelShutdownAsync(Guid authenticationToken)
        {
            var authentication = this.peer[authenticationToken];
            await this.CremaHost.CancelShutdownAsync(authentication);
            return new ResultBase()
            {
                SignatureDate = authentication.SignatureDate
            };
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
    }
}

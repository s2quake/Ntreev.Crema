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

using JSSoft.Communication;
using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceHosts;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data;
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users;
using JSSoft.Library;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    [Export(typeof(ILogService))]
    [InheritedExport(typeof(ICremaHost))]
    class CremaHost : ICremaHost, IServiceProvider, ILogService, ICremaHostEventCallback, IDisposable
    {
        private readonly IServiceProvider container;
        private readonly IConfigurationPropertyProvider[] propertiesProviders;
        private readonly CremaSettings settings;
        private readonly AuthenticationCollection authentications = new();
        private readonly List<ServiceHostBase> hosts;
        private readonly ClientContext clientContext;
        private readonly ShutdownTimer shutdownTimer;

        private UserConfiguration configs;
        private PluginCollection plugins;
        private LogService log;
        private Guid token;
        private Guid serviceToken;

        [ImportingConstructor]
        public CremaHost(IServiceProvider container, [ImportMany] IEnumerable<IConfigurationPropertyProvider> propertiesProviders, CremaSettings settings)
        {
            CremaLog.Attach(this);
            this.container = container;
            this.propertiesProviders = propertiesProviders.ToArray();
            this.settings = settings;
            this.Dispatcher = new CremaDispatcher(this);
            CremaLog.Debug($"available tags : {string.Join(",", TagInfoUtility.Names)}");
            CremaLog.Debug("Crema created.");

            this.DataBaseContext = new DataBaseContext(this);
            this.UserContext = new UserContext(this);
            this.DomainContext = new DomainContext(this);

            this.hosts = new List<ServiceHostBase>()
            {
                new CremaHostServiceHost(this),
                new DataBaseContextServiceHost(this.DataBaseContext),
                new DomainContextServiceHost(this.DomainContext),
                new UserContextServiceHost(this.UserContext)
            };
            this.clientContext = new ClientContext(this.hosts.ToArray());
            this.shutdownTimer = new(this);
            this.shutdownTimer.Done += ShutdownTimer_Done;
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(ICremaHost))
                return this;
            if (serviceType == typeof(IDataBaseContext))
                return this.DataBaseContext;
            if (serviceType == typeof(IUserContext))
                return this.UserContext;
            if (serviceType == typeof(IUserCollection))
                return this.UserContext.Users;
            if (serviceType == typeof(IUserCategoryCollection))
                return this.UserContext.Categories;
            if (serviceType == typeof(IDomainContext))
                return this.DomainContext;
            if (serviceType == typeof(IDomainCollection))
                return this.DomainContext.Domains;
            if (serviceType == typeof(IDomainCategoryCollection))
                return this.DomainContext.Categories;
            if (serviceType == typeof(ILogService))
                return this;
            if (serviceType == typeof(IUserConfiguration))
                return this.configs;

            if (this.container != null)
            {
                try
                {
                    return this.container.GetService(serviceType);
                }
                catch (Exception e)
                {
                    CremaLog.Error(e);
                    return null;
                }
            }

            return null;
        }

        public async Task OpenAsync()
        {
            try
            {
                var address = this.settings.Address;
                var version = typeof(CremaHost).Assembly.GetName().Version;
                var platform = Environment.OSVersion.Platform;
                var culture = CultureInfo.CurrentCulture;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Closed)
                        throw new InvalidOperationException(Resources.Exception_AlreadyConnected);
                    this.ServiceState = ServiceState.Opening;
                    this.OnOpening(EventArgs.Empty);
                });
                this.Address = address;
                this.clientContext.Closed += ClientContext_Closed;
                this.clientContext.Host = AddressUtility.GetIPAddress(address);
                this.clientContext.Port = AddressUtility.GetPort(address);
                this.serviceToken = await this.clientContext.OpenAsync();
                this.ServiceInfo = await this.Service.GetServiceInfoAsync();
                await this.Service.SubscribeAsync($"{version}", $"{platform}", $"{culture}");
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.plugins = new PluginCollection(this.container.GetService(typeof(IEnumerable<IPlugin>)) as IEnumerable<IPlugin>);
                    this.authentications.Initialize(this, this.plugins);
                    this.ServiceState = ServiceState.Open;
                    this.OnOpened(EventArgs.Empty);
                    CremaLog.Debug($"Crema opened : {address}");
                });
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.log?.Dispose();
                    this.log = null;
                    this.Address = string.Empty;
                    this.ServiceState = ServiceState.None;
                });
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task<Guid> LoginAsync(string userID, SecureString password, bool force)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Open)
                        throw new InvalidOperationException(Resources.Exception_NotOpened);
                });
                this.AuthenticationToken = await this.Service.LoginAsync(userID, UserContext.Encrypt(userID, password), force);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.log = new LogService(this.Address.Replace(':', '_'), userID, AppUtility.UserAppDataPath)
                    {
                        Verbose = this.settings.Verbose
                    };
                });
                await this.UserContext.InitializeAsync(userID, this.AuthenticationToken);
                await this.DataBaseContext.InitializeAsync(this.AuthenticationToken);
                await this.DomainContext.InitializeAsync(this.AuthenticationToken);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.configs = new UserConfiguration(this.GetConfigPath(userID), this.propertiesProviders);
                });
                return this.AuthenticationToken;
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.AuthenticationToken = Guid.Empty;
                    this.log?.Dispose();
                    this.log = null;
                    this.configs = null;
                });
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task<Authentication> AuthenticateAsync(Guid authenticationToken)
        {
            if (this.AuthenticationToken != authenticationToken)
                throw new InvalidOperationException(Resources.Exception_InvalidToken);
            return await this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.CurrentUser.Authentication);
        }

        public async Task LogoutAsync(Authentication authentication)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException(Resources.Exception_NotOpened);
            });
            await this.Service.LogoutAsync();
            await this.DomainContext.ReleaseAsync();
            await this.DataBaseContext.ReleaseAsync();
            await this.UserContext.ReleaseAsync();
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.AuthenticationToken = Guid.Empty;
                this.log?.Dispose();
                this.log = null;
                this.configs.Commit();
                this.configs = null;
            });
        }

        public void SaveConfigs()
        {
            try
            {
                this.configs.Commit();
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task CloseAsync()
        {
            try
            {
                var waiter = await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Open)
                        throw new InvalidOperationException(Resources.Exception_NotConnected);
                    this.ServiceState = ServiceState.Closing;
                    var closer = new InternalCloseRequestedEventArgs(CloseReason.None);
                    this.OnCloseRequested(closer);
                    return closer.WhenAll();
                });
                await waiter;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnClosing(EventArgs.Empty);
                    this.plugins.Release();
                });
                await this.Service.UnsubscribeAsync();
                this.clientContext.Closed -= ClientContext_Closed;
                await this.clientContext.CloseAsync(this.serviceToken, 0);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.authentications.Release(Authentication.SystemID);
                    this.serviceToken = Guid.Empty;
                    this.Address = null;
                    this.ServiceState = ServiceState.Closed;
                    this.OnClosed(new ClosedEventArgs(CloseReason.None));
                    CremaLog.Debug("Crema closed.");
                });
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task ShutdownAsync(Authentication authentication, ShutdownContext shutdownContext)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (shutdownContext is null)
                    throw new ArgumentNullException(nameof(shutdownContext));

                var milliseconds = shutdownContext.Milliseconds;
                var isRestart = shutdownContext.IsRestart;
                var message = shutdownContext.Message;
                var address = this.Address;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DebugMethod(authentication, this, nameof(ShutdownAsync), this, milliseconds, isRestart, message);
                });
                var result = await this.Service.ShutdownAsync(milliseconds, isRestart, message);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.shutdownTimer.Start(shutdownContext, address);
                    this.Sign(authentication, result);
                });
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task CancelShutdownAsync(Authentication authentication)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DebugMethod(authentication, this, nameof(CancelShutdownAsync));
                });
                var result = await this.Service.CancelShutdownAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.shutdownTimer.Stop();
                    this.Sign(authentication, result);
                });
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        private async void ShutdownTimer_Done(object sender, EventArgs e)
        {
            this.shutdownTimer.Stop();
            await this.OpenAsync();
        }

        public void Dispose()
        {
            this.ValidateDispose();

            if (Environment.ExitCode != 0 && this.ServiceState == ServiceState.Open)
            {
                throw new InvalidOperationException("server is not closed.");
            }
            this.log?.Dispose();
            this.DataBaseContext?.Dispose();
            this.DataBaseContext = null;
            this.DomainContext?.Dispose();
            this.DomainContext = null;
            this.UserContext?.Dispose();
            this.UserContext = null;
            this.Dispatcher?.Dispose();
            this.Dispatcher = null;
            this.shutdownTimer.Dispose();
            this.OnDisposed(EventArgs.Empty);
            CremaLog.Debug("Crema disposed.");
            CremaLog.Detach(this);
        }

        public void Debug(object message)
        {
            this.Log.Debug(message);
        }

        public void Info(object message)
        {
            this.Log.Info(message);
        }

        public void Error(object message)
        {
            this.Log.Error(message);
        }

        public void Warn(object message)
        {
            this.Log.Warn(message);
        }

        public void Fatal(object message)
        {
            this.Log.Fatal(message);
        }

        public void Sign(Authentication authentication, ResultBase result)
        {
            result.Validate(authentication);
        }

        public void Sign<T>(Authentication authentication, ResultBase<T> result)
        {
            result.Validate(authentication);
        }

        public void Sign(Authentication authentication, SignatureDate signatureDate)
        {
            authentication.Sign(signatureDate.DateTime);
        }

        public async Task<ResultBase<TResult>> InvokeServiceAsync<TResult>(Func<ResultBase<TResult>> func)
        {
            var result = await Task.Run(func);
            result.Validate();
            return result;
        }

        public async Task<ResultBase> InvokeServiceAsync(Func<ResultBase> func)
        {
            var result = await Task.Run(func);
            result.Validate();
            return result;
        }

        public ResultBase<TResult> InvokeService<TResult>(Func<ResultBase<TResult>> func)
        {
            var result = func();
            result.Validate();
            return result;
        }

        public ResultBase InvokeService(Func<ResultBase> func)
        {
            var result = func();
            result.Validate();
            return result;
        }

        public ConfigurationBase Configs => this.configs;

        public string Address { get; private set; } = string.Empty;

        public string UserID => this.User != null ? this.User.ID : string.Empty;

        public string UserName => this.User != null ? this.User.UserName : string.Empty;

        public Authority Authority => this.User != null ? this.User.Authority : Authority.Guest;

        public User User => this.UserContext.CurrentUser;

        public DataBaseContext DataBaseContext { get; private set; }

        public DomainContext DomainContext { get; private set; }

        public UserContext UserContext { get; private set; }

        public CremaDispatcher Dispatcher { get; private set; }

        public ServiceInfo ServiceInfo { get; private set; }

        public ServiceState ServiceState { get; set; }

        public ICremaHostService Service { get; set; }

        public event EventHandler Opening;

        public event EventHandler Opened;

        public event CloseRequestedEventHandler CloseRequested;

        public event EventHandler Closing;

        public event ClosedEventHandler Closed;

        public event EventHandler Disposed;

        protected virtual void OnOpening(EventArgs e)
        {
            this.Opening?.Invoke(this, e);
        }

        protected virtual void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        protected virtual void OnCloseRequested(CloseRequestedEventArgs e)
        {
            this.CloseRequested?.Invoke(this, e);
        }

        protected virtual void OnClosing(EventArgs e)
        {
            this.Closing?.Invoke(this, e);
        }

        protected virtual void OnClosed(ClosedEventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        private void ValidateOpen(string address, string userID, SecureString password)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (userID is null)
                throw new ArgumentNullException(nameof(userID));
            if (password is null)
                throw new ArgumentNullException(nameof(password));
        }

        private void ValidateDispose()
        {
            if (Environment.ExitCode == 0 && this.ServiceState == ServiceState.Open)
                throw new InvalidOperationException(Resources.Exception_NotClosed);
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_AlreadyDisposed);
        }

        public Task ReleaseAsync()
        {
            return Task.Delay(1);
            // await this.Dispatcher.InvokeAsync(() =>
            // {
            //     this.OnClosing(EventArgs.Empty);
            //     foreach (var item in this.plugins.Reverse())
            //     {
            //         item.Release();
            //     }
            // });
            // await this.WaitReleaseAsync();
            // await this.Dispatcher.InvokeAsync(() =>
            // {
            //     foreach (var item in this.authentications)
            //     {
            //         item.InvokeExpiredEvent(Authentication.SystemID);
            //     }
            //     this.serviceToken = Guid.Empty;
            //     this.Address = null;
            //     this.ServiceState = ServiceState.Closed;
            //     this.token = Guid.Empty;
            //     this.OnClosed(new ClosedEventArgs(CloseReason.None, string.Empty));
            //     CremaLog.Debug("Crema closed.");
            // });
        }

        private string GetConfigPath(string userID)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var productName = AppUtility.ProductName;
            var address = this.Address.Replace(':', '-');
            return Path.Combine(path, productName, $"{userID}@{address}.config");
        }

        private async Task WaitReleaseAsync()
        {
            var handles = new ManualResetEvent[]
            {
                this.DataBaseContext.ReleaseHandle,
                this.UserContext.ReleaseHandle,
                this.DomainContext.ReleaseHandle,
            };
            await Task.Run(() => ManualResetEvent.WaitAll(handles));
        }

        private void ClientContext_Closed(object sender, CloseEventArgs e)
        {
            this.clientContext.Closed -= ClientContext_Closed;
            Task.Run(async () =>
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnClosing(EventArgs.Empty);
                    this.plugins.Release();
                });
                await this.WaitReleaseAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.authentications.Release(Authentication.SystemID);
                    this.serviceToken = Guid.Empty;
                    this.Address = null;
                    this.ServiceState = ServiceState.Closed;
                    this.OnClosed(new ClosedEventArgs((CloseReason)e.CloseCode, string.Empty));
                    CremaLog.Debug("Crema closed.");
                });
            });
        }

        private LogService Log
        {
            get
            {
                if (this.log == null)
                    throw new InvalidOperationException(Resources.Exception_NotOpened);
                return this.log;
            }
        }

        internal Guid AuthenticationToken { get; set; }

        #region ICremaHostEventCallback

        void ICremaHostEventCallback.OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs)
        {

        }

        #endregion

        #region ILogService

        LogVerbose ILogService.Verbose
        {
            get => this.Log.Verbose;
            set => this.Log.Verbose = value;
        }

        void ILogService.AddRedirection(TextWriter writer, LogVerbose verbose)
        {
            this.log.AddRedirection(writer, verbose);
        }

        void ILogService.RemoveRedirection(TextWriter writer)
        {
            this.log.RemoveRedirection(writer);
        }

        string ILogService.Name => this.log.Name;

        string ILogService.FileName => this.log.FileName;

        bool ILogService.IsEnabled => this.ServiceState == ServiceState.Open;

        #endregion

        #region ICremaHost

        async Task<Guid> ICremaHost.OpenAsync()
        {
            await this.OpenAsync();
            this.token = Guid.NewGuid();
            return this.token;
        }

        async Task ICremaHost.CloseAsync(Guid token)
        {
            if (this.token != token)
                throw new InvalidOperationException(Resources.Exception_InvalidToken);
            await this.CloseAsync();
            this.token = Guid.Empty;
        }

        #endregion
    }
}

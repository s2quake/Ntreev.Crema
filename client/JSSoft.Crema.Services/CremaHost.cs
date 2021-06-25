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
        private readonly IComponentProvider componentProvider;
        private readonly CremaSettings settings;
        private readonly AuthenticationCollection authentications = new();
        private readonly List<ServiceHostBase> hosts;
        private readonly ClientContext clientContext;
        private readonly ShutdownTimer shutdownTimer;
        private readonly UserConfigurationCollection configs;
        private readonly AuthenticationTokenCollection authenticationTokens;

        private PluginCollection plugins;
        private LogService log;
        private Guid token;
        private Guid serviceToken;
        private Guid subscribeToken;

        [ImportingConstructor]
        public CremaHost(IServiceProvider container,
            IComponentProvider componentProvider,
            [ImportMany] IEnumerable<IConfigurationPropertyProvider> propertiesProviders,
            CremaSettings settings)
        {
            this.container = container;
            this.componentProvider = componentProvider;
            this.settings = settings;
            this.Dispatcher = new CremaDispatcher(this);
            this.configs = new UserConfigurationCollection(this, propertiesProviders.ToArray());
            this.authenticationTokens = new AuthenticationTokenCollection(this);
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
            this.clientContext = new ClientContext(this.componentProvider, this.hosts.ToArray());
            this.shutdownTimer = new(this);
            this.shutdownTimer.Done += ShutdownTimer_Done;
        }

        private async void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            var users = e.Items.Select(item => item as User);
            var items = users.Select(item => item.ID).ToArray();
            await this.authenticationTokens.RemoveManyAsync(items);
            await this.configs.RemoveManyAsync(items);
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
                this.log = new LogService(address.Replace(':', '_'), AppUtility.UserAppDataPath)
                {
                    LogLevel = this.settings.Verbose
                };
                this.Address = address;
                this.clientContext.Host = AddressUtility.GetIPAddress(address);
                this.clientContext.Port = AddressUtility.GetPort(address);
                this.serviceToken = await this.clientContext.OpenAsync();
                this.clientContext.Closed += ClientContext_Closed;
                this.ServiceInfo = await this.Service.GetServiceInfoAsync();
                this.subscribeToken = await this.Service.SubscribeAsync($"{version}", $"{platform}", $"{culture}");
                await this.UserContext.InitializeAsync(this.subscribeToken);
                await this.DataBaseContext.InitializeAsync(this.subscribeToken);
                await this.DomainContext.InitializeAsync(this.subscribeToken);
                await this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut);
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
                if (this.ServiceState != ServiceState.Open)
                {
                    this.log?.Dispose();
                    this.log = null;
                    this.Address = string.Empty;
                }
                this.ErrorInternal(e);
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
                    var closer = new InternalCloseRequestedEventArgs(CloseReason.None);
                    this.ServiceState = ServiceState.Closing;
                    this.OnCloseRequested(closer);
                    return closer.WhenAll();
                });
                await waiter;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnClosing(EventArgs.Empty);
                    this.plugins.Release();
                });

                await this.UserContext.Dispatcher.InvokeAsync(() => this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut);
                await this.authenticationTokens.LogoutAsync();
                await this.DomainContext.ReleaseAsync();
                await this.DataBaseContext.ReleaseAsync();
                await this.UserContext.ReleaseAsync();
                await this.Service.UnsubscribeAsync(this.subscribeToken);
                this.clientContext.Closed -= ClientContext_Closed;
                await this.clientContext.CloseAsync(this.serviceToken, 0);
                await this.WaitReleaseAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.authentications.Release(Authentication.SystemID);
                    this.configs.Clear();
                    this.serviceToken = Guid.Empty;
                    this.Address = null;
                    this.ServiceState = ServiceState.Closed;
                    this.OnClosed(new ClosedEventArgs(CloseReason.None));
                    CremaLog.Debug("Crema closed.");
                });
                this.log.Dispose();
                this.log = null;
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task<Guid> LoginAsync(string userID, SecureString password)
        {
            try
            {
                if (userID is null)
                    throw new ArgumentNullException(nameof(userID));
                if (password is null)
                    throw new ArgumentNullException(nameof(password));

                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Open)
                        throw new InvalidOperationException(Resources.Exception_NotOpened);
                    this.Debug($"{this.GetType().Name}.{nameof(LoginAsync)} : {userID}");
                });
                var authenticationToken = await this.Service.LoginAsync(userID, UserContext.Encrypt(userID, password));
                var address = this.Address;
                await this.UserContext.LoginAsync(userID, authenticationToken);
                await this.authenticationTokens.AddAsync(userID, authenticationToken);
                await this.configs.AddAsync(userID, address);
                return authenticationToken;
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task LogoutAsync(Authentication authentication)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                var authenticationID = authentication.ID;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Open)
                        throw new InvalidOperationException(Resources.Exception_NotOpened);
                    // if (authentication.ID != this.UserID)
                    //     throw new InvalidOperationException();
                    this.DebugMethod(authentication, this, nameof(LogoutAsync));
                });
                await this.Service.LogoutAsync(authentication.Token);
                await this.UserContext.LogoutAsync(authentication.ID);
                await this.configs.RemoveAsync(authenticationID);
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task LogoutAsync(string userID, SecureString password)
        {
            if (userID is null)
                throw new ArgumentNullException(nameof(userID));
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException(Resources.Exception_NotOpened);
                this.Debug($"{this.GetType().Name}.{nameof(LogoutAsync)} : {userID}");
            });
            await this.Service.LogoutAsync(userID, UserContext.Encrypt(userID, password));
            // if (this.AuthenticationToken != Guid.Empty)
            // {
            //     await this.LogoutAsync();
            // }
        }

        public async Task<Authentication> AuthenticateAsync(Guid authenticationToken)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException(Resources.Exception_NotOpened);
            });
            return await this.UserContext.AuthenticateAsync(authenticationToken);
        }

        public async Task ShutdownAsync(Authentication authentication, ShutdownContext shutdownContext)
        {
            try
            {
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));
                if (shutdownContext is null)
                    throw new ArgumentNullException(nameof(shutdownContext));

                var milliseconds = shutdownContext.Milliseconds;
                var isRestart = shutdownContext.IsRestart;
                var message = shutdownContext.Message;
                var address = this.Address;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Open)
                        throw new InvalidOperationException();
                    this.DebugMethod(authentication, this, nameof(ShutdownAsync), this, milliseconds, isRestart, message);
                });
                var result = await this.Service.ShutdownAsync(authentication.Token, milliseconds, isRestart, message);
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
                if (authentication is null)
                    throw new ArgumentNullException(nameof(authentication));
                if (authentication.IsExpired == true)
                    throw new AuthenticationExpiredException(nameof(authentication));

                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Open)
                        throw new InvalidOperationException();
                    this.DebugMethod(authentication, this, nameof(CancelShutdownAsync));
                });
                var result = await this.Service.CancelShutdownAsync(authentication.Token);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.shutdownTimer.Stop();
                    this.Sign(authentication, result);
                    this.Info("Shutdown cancelled.");
                });
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
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
        }

        public void Sign(Authentication authentication, ResultBase result)
        {
            authentication.SignatureDate = result.SignatureDate;
        }

        public void Sign<T>(Authentication authentication, ResultBase<T> result)
        {
            authentication.SignatureDate = result.SignatureDate;
        }

        public void Sign(Authentication authentication, SignatureDate signatureDate)
        {
            authentication.Sign(signatureDate.DateTime);
        }

        public string Address { get; private set; } = string.Empty;

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

        private async void ShutdownTimer_Done(object sender, EventArgs e)
        {
            this.shutdownTimer.Stop();
            await this.OpenAsync();
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

        // private string GetConfigPath(string userID)
        // {
        //     var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //     var productName = AppUtility.ProductName;
        //     var address = this.Address.Replace(':', '-');
        //     return Path.Combine(path, productName, $"{userID}@{address}.config");
        // }

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
                    // this.AuthenticationToken = Guid.Empty;
                    this.log?.Dispose();
                    this.log = null;

                    this.authentications.Release(Authentication.SystemID);
                    this.serviceToken = Guid.Empty;
                    this.Address = string.Empty;
                    this.ServiceState = ServiceState.Closed;
                    this.OnClosed(new ClosedEventArgs((CloseReason)e.CloseCode, string.Empty));
                    CremaLog.Debug("Crema closed.");
                });
            });
        }

        private void ErrorInternal(Exception e)
        {
            if (this.log != null)
            {
                this.log.Error(e);
            }
            else
            {
                CremaLog.Error(e);
            }
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

        // internal Guid AuthenticationToken { get; set; }

        #region ICremaHostEventCallback

        void ICremaHostEventCallback.OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs)
        {

        }

        #endregion

        #region ILogService

        LogLevel ILogService.Verbose
        {
            get => this.Log.LogLevel;
            set => this.Log.LogLevel = value;
        }

        void ILogService.AddRedirection(TextWriter writer, LogLevel verbose)
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
            if (this.ServiceState != ServiceState.Open)
                throw new InvalidOperationException();
            if (this.token != token)
                throw new ArgumentException(Resources.Exception_InvalidToken, nameof(token));
            await this.CloseAsync();
            this.token = Guid.Empty;
        }

        #endregion
    }
}

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

using JSSoft.Communication;
using Microsoft.Win32;
using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceHosts;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Data;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    [Export(typeof(ILogService))]
    [InheritedExport(typeof(ICremaHost))]
    class CremaHost : ICremaHost, IServiceProvider, ILogService, ICremaHostEventCallback
    {
        private readonly IServiceProvider container;
        private readonly IConfigurationPropertyProvider[] propertiesProviders;
        private readonly CremaSettings settings;
        private readonly List<Authentication> authentications = new List<Authentication>();
        private readonly List<ServiceHostBase> hosts;

        private UserConfiguration configs;
        private IEnumerable<IPlugin> plugins;

        private LogService log;
        private Guid token;
#pragma warning disable CS0169 // CremaHost.host' 필드가 사용되지 않았습니다.
        private readonly CremaHostServiceHost host;
#pragma warning restore CS0169 // CremaHost.host' 필드가 사용되지 않았습니다.
        //private PingTimer pingTimer;
        private readonly ClientContext clientContext;
        private Guid serviceToken;

        [ImportingConstructor]
        public CremaHost(IServiceProvider container, [ImportMany] IEnumerable<IConfigurationPropertyProvider> propertiesProviders, CremaSettings settings)
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            CremaLog.Attach(this);
            this.container = container;
            this.propertiesProviders = propertiesProviders.ToArray();
            this.settings = settings;
            this.Dispatcher = new CremaDispatcher(this);
            CremaLog.Debug($"available tags : {string.Join(",", TagInfoUtility.Names)}");
            CremaLog.Debug("Crema created.");

            this.DataBaseContext = new DataBaseContext(this);
            this.DomainContext = new DomainContext(this);
            this.UserContext = new UserContext(this);

            this.hosts = new List<ServiceHostBase>()
            {
                new CremaHostServiceHost(this),
                new DataBaseContextServiceHost(this.DataBaseContext),
                new DomainContextServiceHost(this.DomainContext),
                new UserContextServiceHost(this.UserContext)
            };
            this.clientContext = new ClientContext(this.hosts.ToArray());
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

        public Task InvokeCloseAsync(CloseInfo closeInfo)
        {
            return this.CloseAsync(closeInfo);
        }

        public async Task<Guid> OpenAsync(string address, string userID, SecureString password)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Closed)
                        throw new InvalidOperationException(Resources.Exception_AlreadyConnected);
                    this.ServiceState = ServiceState.Opening;
                    this.OnOpening(EventArgs.Empty);
                });
                this.serviceToken = await this.clientContext.OpenAsync();
                this.ServiceInfo = (await this.Service.GetServiceInfoAsync()).Value;
                var version = typeof(CremaHost).Assembly.GetName().Version;
                var result = await this.Service.SubscribeAsync(userID, UserContext.Encrypt(userID, password), $"{version}", $"{Environment.OSVersion.Platform}", $"{CultureInfo.CurrentCulture}");
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.AuthenticationToken = result.Value;
                    this.IPAddress = AddressUtility.GetIPAddress(address);
                    this.Address = AddressUtility.GetDisplayAddress(address);
                    this.UserID = userID;
                    this.log = new LogService(this.Address.Replace(':', '_'), userID, AppUtility.UserAppDataPath)
                    {
                        Verbose = this.settings.Verbose
                    };
                    //this.UserContext = new UserContext(this);
                    //this.DataBaseContext = new DataBaseContext(this);
                    //this.DomainContext = new DomainContext(this);
                });
                await this.UserContext.InitializeAsync(userID, this.AuthenticationToken);
                await this.DataBaseContext.InitializeAsync(this.AuthenticationToken);
                await this.DomainContext.InitializeAsync(this.AuthenticationToken);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Authority = this.UserContext.CurrentUser.Authority;
                    this.configs = new UserConfiguration(this.ConfigPath, this.propertiesProviders);
                    this.plugins = (this.container.GetService(typeof(IEnumerable<IPlugin>)) as IEnumerable<IPlugin>).ToArray();
                    foreach (var item in this.plugins)
                    {
                        var authentication = new Authentication(new AuthenticationProvider(this.UserContext.CurrentUser), item.ID);
                        this.authentications.Add(authentication);
                        item.Initialize(authentication);
                    }
                    this.ServiceState = ServiceState.Open;
                    this.OnOpened(EventArgs.Empty);
                    this.token = Guid.NewGuid();
                    CremaLog.Debug($"Crema opened : {address} {userID}");
                });
                return this.token;
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = ServiceState.None;
                    this.UserContext = null;
                    this.log?.Dispose();
                    this.log = null;
                    this.Address = null;
                });
                CremaLog.Error(e);
                throw;
            }
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

        public async Task CloseAsync(Guid token)
        {
            try
            {
                if (this.token != token)
                    throw new ArgumentException(Resources.Exception_InvalidToken, nameof(token));
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException(Resources.Exception_NotConnected);
                await this.CloseAsync(CloseInfo.Empty);

            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task ShutdownAsync(Authentication authentication, int milliseconds, ShutdownType shutdownType, string message)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DebugMethod(authentication, this, nameof(ShutdownAsync), this, milliseconds, shutdownType, message);
                });
                var result = await this.Service.ShutdownAsync(milliseconds, shutdownType, message);
                await this.Dispatcher.InvokeAsync(() =>
                {
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
                    this.Sign(authentication, result);
                });
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw;
            }
        }

        public void Dispose()
        {
            this.ValidateDispose();

            if (Environment.ExitCode != 0 && this.ServiceState == ServiceState.Open)
            {
                throw new InvalidOperationException("server is not closed.");
            }
            this.log?.Dispose();
            this.DataBaseContext.Dispose();
            this.DataBaseContext = null;
            this.DomainContext.Dispose();
            this.DomainContext = null;
            this.UserContext.Dispose();
            this.UserContext = null;
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
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

        public string Address { get; private set; }

        public string UserID { get; private set; }

        public Authority Authority { get; private set; }

        public DataBaseContext DataBaseContext { get; private set; }

        public DomainContext DomainContext { get; private set; }

        public UserContext UserContext { get; private set; }

        public CremaDispatcher Dispatcher { get; private set; }

        public ServiceInfo ServiceInfo { get; private set; }

        public string IPAddress { get; private set; }

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

        private void ValidateDispose()
        {
            if (Environment.ExitCode == 0 && this.ServiceState == ServiceState.Open)
                throw new InvalidOperationException(Resources.Exception_NotClosed);
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_AlreadyDisposed);
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {

        }

        private async void PingTimer_Faulted(object sender, EventArgs e)
        {
            try
            {
                await this.CloseAsync(new CloseInfo(CloseReason.NoResponding, string.Empty));
            }
            catch
            {

            }
        }

        private void Service_Closed(object sender, EventArgs e)
        {

        }

        private async void Service_Faulted(object sender, EventArgs e)
        {
            try
            {
                await this.CloseAsync(new CloseInfo(CloseReason.Faulted, string.Empty));
            }
            catch
            {

            }
        }

        private async Task CloseAsync(CloseInfo closeInfo)
        {
            var waiter = await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException();
                this.ServiceState = ServiceState.Closing;
                var closer = new InternalCloseRequestedEventArgs();
                this.OnCloseRequested(closer);
                return closer.WhenAll();
            });
            await waiter;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnClosing(EventArgs.Empty);
                foreach (var item in this.plugins.Reverse())
                {
                    item.Release();
                }
            });
            await this.DomainContext.CloseAsync(closeInfo);
            await this.DataBaseContext.CloseAsync(closeInfo);
            await this.UserContext.CloseAsync(closeInfo);
            await this.Service.UnsubscribeAsync();
            await Task.Delay(100);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.DomainContext = null;
                this.DataBaseContext = null;
                this.UserContext = null;
                foreach (var item in this.authentications)
                {
                    item.InvokeExpiredEvent(Authentication.SystemID);
                }
                this.log?.Dispose();
                this.log = null;
                this.Address = null;
                this.UserID = null;
                this.ServiceState = ServiceState.Closed;
                this.token = Guid.Empty;
                this.OnClosed(new ClosedEventArgs(closeInfo.Reason, closeInfo.Message));
                this.configs.Commit();
                this.configs = null;
                CremaLog.Debug("Crema closed.");
            });
        }

        private string ConfigPath
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var productName = AppUtility.ProductName;
                var address = this.Address.Replace(':', '-');
                return Path.Combine(path, productName, $"{this.UserID}@{address}.config");
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

        internal Guid AuthenticationToken { get; set; }

        #region ICremaHostEventCallback

        async void ICremaHostEventCallback.OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo)
        {
            await this.CloseAsync(closeInfo);
        }

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

        string ICremaHost.Address => this.Address;

        #endregion
    }
}

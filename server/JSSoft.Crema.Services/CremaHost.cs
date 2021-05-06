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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data;
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users;
using JSSoft.Library;
using JSSoft.Library.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Timers;

namespace JSSoft.Crema.Services
{
    [Export(typeof(ILogService))]
    [InheritedExport(typeof(ICremaHost))]
    class CremaHost : ICremaHost, ILogService, IDisposable
    {
        private RepositoryConfiguration configs;
        private IPlugin[] plugins;

        private readonly IServiceProvider container;
        private readonly CremaSettings settings;
        private readonly IRepositoryProvider[] repositoryProviders;
        private readonly IObjectSerializer[] serializers;
        private readonly IEnumerable<Lazy<IConfigurationPropertyProvider>> propertiesProviders;
        private LogService log;
        private Guid token;
        private ShutdownTimer shutdownTimer;


        [ImportingConstructor]
        public CremaHost(IServiceProvider container, CremaSettings settings,
            [ImportMany] IEnumerable<IRepositoryProvider> repositoryProviders,
            [ImportMany] IEnumerable<IObjectSerializer> serializers,
            [ImportMany] IEnumerable<Lazy<IConfigurationPropertyProvider>> propertiesProviders)
        {
            CremaLog.Attach(this);
            CremaLog.Debug("crema instance created.");
            this.container = container;
            this.settings = settings;
            this.repositoryProviders = repositoryProviders.ToArray();
            this.serializers = serializers.ToArray();
            this.propertiesProviders = propertiesProviders;
            CremaLog.Debug("crema log service initialized.");
            CremaLog.Debug($"available tags : {string.Join(", ", TagInfoUtility.Names)}");
            this.Dispatcher = new CremaDispatcher(this);
            CremaLog.Debug("crema dispatcher initialized.");
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
                return this.log != null ? this : null;
            if (serviceType == typeof(IObjectSerializer))
                return this.Serializer;
            if (this.ServiceState == ServiceState.Open && serviceType == typeof(IRepositoryConfiguration))
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Closed)
                        throw new InvalidOperationException();
                    this.ServiceState = ServiceState.Opening;
                });
                this.BasePath = this.settings.BasePath;
                this.RepositoryLocker = new RepositoryLocker(this);
                this.RepositoryProvider = repositoryProviders.First(item => item.Name == this.settings.RepositoryModule);
                this.Serializer = serializers.First(item => item.Name == this.settings.FileType);
                this.log = new LogService("log", this.GetPath(CremaPath.Logs), false)
                {
                    Name = "repository",
                    Verbose = settings.Verbose
                };
                this.RepositoryDispatcher = new CremaDispatcher(this.RepositoryProvider);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnOpening(EventArgs.Empty);
                    this.Info(Resources.Message_ProgramInfo, AppUtility.ProductName, AppUtility.ProductVersion);
                    this.Info("Repository module : {0}", this.settings.RepositoryModule);
                    this.Info(Resources.Message_ServiceStart);
                    this.configs = new RepositoryConfiguration(Path.Combine(this.BasePath, "configs"), this.propertiesProviders.Select(item => item.Value));
                    this.UserContext = new UserContext(this);
                    this.DataBaseContext = new DataBaseContext(this);
                    this.DomainContext = new DomainContext(this);
                });
                await this.UserContext.InitializeAsync();
                await this.DataBaseContext.InitializeAsync();
                await this.DomainContext.InitializeAsync();
                await this.DomainContext.RestoreAsync(settings);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.plugins = (this.container.GetService(typeof(IEnumerable<IPlugin>)) as IEnumerable<IPlugin>).TopologicalSort().ToArray();
                    foreach (var item in this.plugins)
                    {
                        var authentication = new Authentication(new AuthenticationProvider(item), item.ID);
                        item.Initialize(authentication);
                        this.Info("Plugin : {0}", item.Name);
                    }
                    this.Info("Crema module has been started.");
                    this.ServiceState = ServiceState.Open;
                    this.OnOpened(EventArgs.Empty);
                });
                await this.DataBaseContext.RestoreStateAsync(this.settings);
            }
            catch (Exception e)
            {
                this.UserContext = null;
                this.DomainContext = null;
                this.DataBaseContext = null;
                CremaLog.Error(e);
                throw;
            }
        }

        public async Task CloseAsync(CloseReason reason, string message)
        {
            try
            {
                var waiter = await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ServiceState != ServiceState.Open)
                        throw new InvalidOperationException();
                    this.ServiceState = ServiceState.Closing;
                    var closer = new InternalCloseRequestedEventArgs(reason);
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
                await this.DomainContext.DisposeAsync();
                await this.DataBaseContext.DisposeAsync();
                await this.UserContext.DisposeAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DataBaseContext = null;
                    this.DomainContext = null;
                    this.UserContext = null;
                    this.Info("Crema module has been stopped.");
                    this.ServiceState = ServiceState.Closed;
                    this.OnClosed(new ClosedEventArgs(reason, message));
                });
                this.configs = null;
                this.RepositoryLocker.Dispose();
                this.RepositoryLocker = null;
                await this.RepositoryDispatcher.DisposeAsync();
                this.RepositoryDispatcher = null;
                this.log.Dispose();
                this.log = null;
            }
            catch (Exception e)
            {
                this.log.Error(e);
                throw;
            }
        }

        public async Task ShutdownAsync(Authentication authentication, int milliseconds, ShutdownType shutdownType, string message)
        {
            try
            {
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DebugMethod(authentication, this, nameof(ShutdownAsync), this, milliseconds, shutdownType, message);
                    this.ValidateShutdown(authentication, milliseconds);
                });
                if (string.IsNullOrEmpty(message) == false)
                    await this.UserContext.NotifyMessageAsync(Authentication.System, message);
                var dateTime = DateTime.Now.AddMilliseconds(milliseconds);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.shutdownTimer == null)
                    {
                        this.shutdownTimer = new ShutdownTimer()
                        {
                            Interval = 1000,
                        };
                        this.shutdownTimer.Elapsed += ShutdownTimer_Elapsed;
                    }
                    this.shutdownTimer.DateTime = dateTime;
                    this.shutdownTimer.ShutdownType = shutdownType;
                    this.shutdownTimer.Start();
                });
                if (milliseconds >= 1000)
                    await this.SendShutdownMessageAsync((dateTime - DateTime.Now) + new TimeSpan(0, 0, 0, 0, 500), true);
            }
            catch (Exception e)
            {
                this.log.Error(e);
                throw;
            }
        }

        public async Task CancelShutdownAsync(Authentication authentication)
        {
            try
            {
                if (this.ServiceState != ServiceState.Open)
                    throw new InvalidOperationException();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.DebugMethod(authentication, this, nameof(CancelShutdownAsync), this);
                    this.ValidateCancelShutdown(authentication);
                    if (this.shutdownTimer != null)
                    {
                        this.shutdownTimer.Elapsed -= ShutdownTimer_Elapsed;
                        this.shutdownTimer.Stop();
                        this.shutdownTimer.Dispose();
                        this.shutdownTimer = null;
                        this.Info($"[{authentication}] Shutdown cancelled.");
                    }
                });
            }
            catch (Exception e)
            {
                this.log.Error(e);
                throw;
            }
        }

        public Task<Guid> LoginAsync(string userID, SecureString password)
        {
            return this.UserContext.LoginAsync(userID, password);
        }

        public Task<Authentication> AuthenticateAsync(Guid authenticationToken)
        {
            return this.UserContext.AuthenticateAsync(authenticationToken);
        }

        public Task LogoutAsync(Authentication authentication)
        {
            return this.UserContext.LogoutAsync(authentication);
        }

        public void Debug(object message)
        {
            this.log.Debug(message);
        }

        public void Info(object message)
        {
            this.log.Info(message);
        }

        public void Error(object message)
        {
            this.log.Error(message);
        }

        public void Warn(object message)
        {
            this.log.Warn(message);
        }

        public void Fatal(object message)
        {
            this.log.Fatal(message);
        }

        public void Dispose()
        {
            if (this.DataBaseContext != null)
            {
                throw new InvalidOperationException(Resources.Exception_NotClosed);
            }
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            this.OnDisposed(EventArgs.Empty);
            CremaLog.Detach(this);
        }

        public void Sign(Authentication authentication)
        {
            authentication.Sign();
        }

        public void Sign(Authentication authentication, SignatureDate signatureDate)
        {
            authentication.Sign(signatureDate.DateTime);
        }

        public string GetPath(CremaPath pathType, params string[] paths)
        {
            return GetPath(this.BasePath, pathType, paths);
        }

        public static string GetPath(string basePath, CremaPath pathType, params string[] paths)
        {
            return pathType switch
            {
                CremaPath.Repository => Path.Combine(Path.Combine(basePath, CremaString.Repository), Path.Combine(paths)),
                CremaPath.RepositoryUsers => Path.Combine(Path.Combine(basePath, CremaString.Repository, CremaString.Users), Path.Combine(paths)),
                CremaPath.RepositoryDataBases => Path.Combine(Path.Combine(basePath, CremaString.Repository, CremaString.DataBases), Path.Combine(paths)),
                _ => Path.Combine(Path.Combine(basePath, $"{pathType}".ToLower()), Path.Combine(paths)),
            };
            throw new NotImplementedException();
        }

        public RepositoryLocker RepositoryLocker { get; private set; }

        public IRepositoryProvider RepositoryProvider { get; private set; }

        public string BasePath { get; private set; }

        public ServiceState ServiceState { get; set; }

        public bool NoCache => this.settings.NoCache;

        public LogVerbose Verbose
        {
            get => this.log.Verbose;
            set => this.log.Verbose = value;
        }

        public DataBaseContext DataBaseContext { get; private set; }

        public DomainContext DomainContext { get; private set; }

        public UserContext UserContext { get; private set; }

        public CremaDispatcher Dispatcher { get; private set; }

        public CremaDispatcher RepositoryDispatcher { get; private set; }

        public IObjectSerializer Serializer { get; private set; }

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

        private async void ShutdownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now >= this.shutdownTimer.DateTime)
            {
                await this.Shutdown(this.shutdownTimer.ShutdownType);
            }
            else
            {
                var timeSpan = this.shutdownTimer.DateTime - DateTime.Now;
                await this.SendShutdownMessageAsync(timeSpan, false);
            }
        }

        private async Task SendShutdownMessageAsync(TimeSpan timeSpan, bool about)
        {
            if (about == true)
            {
                if (timeSpan.TotalSeconds >= 3600)
                {
                    await this.UserContext.NotifyMessageAsync(Authentication.System, $"crema shuts down after about {timeSpan.Hours} hours.");
                }
                else if (timeSpan.TotalSeconds >= 60)
                {
                    await this.UserContext.NotifyMessageAsync(Authentication.System, $"crema shuts down after about {timeSpan.Minutes} minutes.");
                }
                else
                {
                    await this.UserContext.NotifyMessageAsync(Authentication.System, $"crema shuts down after about {timeSpan.Seconds} seconds.");
                }
            }
            else
            {
                if (timeSpan.TotalSeconds % 3600 == 0)
                {
                    await this.UserContext.NotifyMessageAsync(Authentication.System, $"crema shuts down after {timeSpan.Hours} hours.");
                }
                else if (timeSpan.TotalSeconds % 60 == 0)
                {
                    await this.UserContext.NotifyMessageAsync(Authentication.System, $"crema shuts down after {timeSpan.Minutes} minutes.");
                }
                else if (timeSpan.TotalSeconds == 30 || timeSpan.TotalSeconds == 15 || timeSpan.TotalSeconds == 10 || timeSpan.TotalSeconds <= 5)
                {
                    await this.UserContext.NotifyMessageAsync(Authentication.System, $"crema shuts down after {timeSpan.Seconds} seconds.");
                }
            }
        }

        private async Task Shutdown(ShutdownType shutdownType)
        {
            if (this.shutdownTimer != null)
            {
                this.shutdownTimer.Elapsed -= ShutdownTimer_Elapsed;
                this.shutdownTimer.Stop();
                this.shutdownTimer.Dispose();
                this.shutdownTimer = null;
            }

            var isRestart = shutdownType.HasFlag(ShutdownType.Restart);
            await this.CloseAsync(isRestart ? CloseReason.Restart : CloseReason.None, string.Empty);
            if (isRestart == true)
            {
                this.settings.NoCache = shutdownType.HasFlag(ShutdownType.NoCache);
                await this.OpenAsync();
            }
        }

        private void ValidateShutdown(Authentication authentication, int milliseconds)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
            if (milliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(milliseconds), "invalid milliseconds value");
        }

        private void ValidateCancelShutdown(Authentication authentication)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();
        }

        #region classes

        class ShutdownTimer : Timer
        {
            public DateTime DateTime { get; set; }

            public ShutdownType ShutdownType { get; set; }
        }

        #endregion

        #region ILogService

        LogVerbose ILogService.Verbose
        {
            get => this.log.Verbose;
            set => this.log.Verbose = value;
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
            await this.CloseAsync(CloseReason.None, string.Empty);
            this.token = Guid.Empty;
        }

        #endregion
    }
}

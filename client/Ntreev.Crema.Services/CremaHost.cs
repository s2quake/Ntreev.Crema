﻿//Released under the MIT License.
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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Data;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace Ntreev.Crema.Services
{
    [Export(typeof(ILogService))]
    [InheritedExport(typeof(ICremaHost))]
    class CremaHost : ICremaHost, IServiceProvider, ILogService
    {
        private readonly List<Authentication> authentications = new List<Authentication>();
        private readonly List<ICremaService> services = new List<ICremaService>();
        private CloseInfo closeInfo;

        private CremaConfiguration configs;
        private IEnumerable<IPlugin> plugins;

        [Import]
        private IServiceProvider container = null;
        private CremaSettings settings;
        private LogService log;
        private Guid token;

        [ImportMany]
        private IEnumerable<IConfigurationPropertyProvider> propertiesProviders = null;

        [ImportingConstructor]
        public CremaHost(CremaSettings settings)
        {
            this.settings = settings;
            this.Dispatcher = new CremaDispatcher(this);
            CremaLog.Debug($"available tags : {string.Join(",", TagInfoUtility.Names)}");
            CremaLog.Debug("Crema created.");
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(ICremaHost))
                return this;
            if (serviceType == typeof(IDataBaseCollection))
                return this.DataBases;
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
            if (this.ServiceState == ServiceState.Opened && serviceType == typeof(ICremaConfiguration))
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

        public void AddService(ICremaService service)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.services.Add(service);
                CremaLog.Debug($"{service.GetType().Name} Initialized.");
            });
        }

        //public Task AddServiceAsync(ICremaService service)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        this.services.Add(service);
        //        CremaLog.Debug($"{service.GetType().Name} Initialized.");
        //    });
        //}

        public async Task RemoveServiceAsync(ICremaService service)
        {
            var isAny = await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.services.Contains(service) == false)
                    return false;
                this.services.Remove(service);
                CremaLog.Debug($"{service.GetType().Name} Released.");
                return this.services.Any();
            });
            if (isAny == false)
            {
                await this.CloseAsync(closeInfo);
            }
        }

        public Task RemoveServiceAsync(ICremaService service, CloseInfo closeInfo)
        {
            this.closeInfo = closeInfo;
            return this.RemoveServiceAsync(service);
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
                this.ServiceInfos = await GetServiceInfoAsync(address);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.IPAddress = AddressUtility.GetIPAddress(address);
                    this.Address = AddressUtility.GetDisplayAddress(address);
                    this.UserID = userID;
                    this.log = new LogService(this.Address.Replace(':', '_'), userID, AppUtility.UserAppDataPath)
                    {
                        Verbose = this.settings.Verbose
                    };
                    this.UserContext = new UserContext(this);
                    this.DataBases = new DataBaseCollection(this);
                    this.DomainContext = new DomainContext(this);
                });
                this.AuthenticationToken = await this.UserContext.InitializeAsync(this.IPAddress, ServiceInfos[nameof(UserService)], userID, password);
                await this.DataBases.InitializeAsync(this.IPAddress, this.AuthenticationToken, ServiceInfos[nameof(DataBaseCollectionService)]);
                await this.DomainContext.InitializeAsync(this.IPAddress, this.AuthenticationToken, ServiceInfos[nameof(DomainService)]);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Authority = this.UserContext.CurrentUser.Authority;
                    this.configs = new CremaConfiguration(this.ConfigPath, this.propertiesProviders);
                    this.plugins = (this.container.GetService(typeof(IEnumerable<IPlugin>)) as IEnumerable<IPlugin>).ToArray();
                    foreach (var item in this.plugins)
                    {
                        var authentication = new Authentication(new AuthenticationProvider(this.UserContext.CurrentUser), item.ID);
                        this.authentications.Add(authentication);
                        item.Initialize(authentication);
                    }
                    this.ServiceState = ServiceState.Opened;
                    this.OnOpened(EventArgs.Empty);
                    this.token = Guid.NewGuid();
                    CremaLog.Debug($"Crema opened : {address} {userID}");
                });
                return this.token;
            }
            catch (Exception e)
            {
                await this.UserContext?.CloseAsync(CloseInfo.Empty);
                this.UserContext = null;
                this.log?.Dispose();
                this.log = null;
                this.Address = null;
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
                if (this.ServiceState != ServiceState.Opened)
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
                var result = await Task.Run(() => this.UserContext.Service.Shutdown(milliseconds, shutdownType, message));
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
                var result = await Task.Run(() => this.UserContext.Service.CancelShutdown());
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

            if (Environment.ExitCode != 0 && this.ServiceState == ServiceState.Opened)
            {
                throw new InvalidOperationException("server is not closed.");
            }

            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            this.OnDisposed(EventArgs.Empty);
            CremaLog.Debug("Crema disposed.");
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

        // mac의 mono 환경에서는 바인딩 값이 서버와 다를 경우 접속이 거부되는 현상이 있음(버그로 추정)
        // binding.SentTimeout 값이 달라도 접속이 안됨.
        public static Binding CreateBinding(ServiceInfo serviceInfo)
        {
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.MaxBufferPoolSize = long.MaxValue;
            binding.MaxBufferSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;

            var isDebug = false;
            if (serviceInfo.PlatformID != string.Empty && Enum.TryParse<PlatformID>(serviceInfo.PlatformID, out var platformID) == false)
            {
                isDebug = true;
                platformID = (PlatformID)Enum.Parse(typeof(PlatformID), serviceInfo.PlatformID.Replace("DEBUG_", string.Empty));
            }

#if DEBUG
            if (isDebug == true && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                binding.SendTimeout = TimeSpan.MaxValue;
            }
#endif

            return binding;
        }

        public ConfigurationBase Configs => this.configs;

        public string Address { get; private set; }

        public string UserID { get; private set; }

        public Authority Authority { get; private set; }

        //public User User
        //{
        //    get
        //    {
        //        if (this.ServiceState != ServiceState.Opened)
        //            return null;
        //        return this.UserContext.Users[this.UserID];
        //    }
        //}

        public DataBaseCollection DataBases { get; private set; }

        public DomainContext DomainContext { get; private set; }

        public UserContext UserContext { get; private set; }

        public CremaDispatcher Dispatcher { get; private set; }

        public IReadOnlyDictionary<string, ServiceInfo> ServiceInfos { get; private set; }

        public string IPAddress { get; private set; }

        public ServiceState ServiceState { get; set; }

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
            if (Environment.ExitCode == 0 && this.ServiceState == ServiceState.Opened)
                throw new InvalidOperationException(Resources.Exception_NotClosed);
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_AlreadyDisposed);
        }

        private async Task CloseAsync(CloseInfo closeInfo)
        {
            var waiter = await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ServiceState != ServiceState.Opened)
                    throw new InvalidOperationException();
                this.ServiceState = ServiceState.Closing;
                var closer = new InternalCloseRequestedEventArgs();
                this.OnCloseRequested(closer);
                return closer.WhenAll();
            });
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnClosing(EventArgs.Empty);
            });
            foreach (var item in this.services.Reverse<ICremaService>())
            {
                await item.CloseAsync(closeInfo);
            }
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.services.Clear();
                this.DomainContext = null;
                this.DataBases = null;
                this.UserContext = null;
                foreach (var item in this.plugins.Reverse())
                {
                    item.Release();
                }
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
                CremaLog.Debug("Crema closed.");
            });
        }

        private static async Task<IReadOnlyDictionary<string, ServiceInfo>> GetServiceInfoAsync(string address)
        {
            var serviceClient = DescriptorServiceFactory.CreateServiceClient(address);
            serviceClient.Open();
            try
            {
                var version = serviceClient.GetVersion();
                Console.WriteLine(version);
                var serviceInfos = serviceClient.GetServiceInfos();
                return serviceInfos.ToDictionary(item => item.Name);
            }
            finally
            {
                serviceClient.Close();
            }
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

        #region ILogService

        LogVerbose ILogService.Verbose
        {
            get => this.Log.Verbose;
            set => this.Log.Verbose = value;
        }

        TextWriter ILogService.RedirectionWriter
        {
            get => this.Log.RedirectionWriter;
            set => this.Log.RedirectionWriter = value;
        }

        string ILogService.Name => this.log.Name;

        string ILogService.FileName => this.log.FileName;

        bool ILogService.IsEnabled => this.ServiceState == ServiceState.Opened;

        #endregion

        #region ICremaHost

        string ICremaHost.Address => this.Address;

        ICremaConfiguration ICremaHost.Configs => this.configs;

        #endregion
    }
}

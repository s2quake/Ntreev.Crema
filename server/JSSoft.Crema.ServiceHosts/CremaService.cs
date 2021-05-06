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
using JSSoft.Crema.ServiceHosts.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts
{
    public class CremaService : ICremaService, IServiceProvider
    {
        public const string Namespace = "http://www.ntreev.com";
        private const string cremaString = "Crema";
        private readonly List<ServiceHostBase> hosts = new List<ServiceHostBase>();
        private readonly IServiceProvider serviceProvider;
        private IServiceHostProvider[] hostProviders;
        private ServerContext serverContext;
        private ServiceInfo serviceInfo;
        private ICremaHost cremaHost;
        private IConfigurationCommitter configCommitter;
        private Guid token;
        private Guid serviceToken;

        public CremaService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.Dispatcher = new CremaDispatcher(typeof(CremaService));
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public string GetPath(CremaPath pathType, params string[] paths)
        {
            if (this.cremaHost == null)
                throw new InvalidOperationException();
            return this.cremaHost.GetPath(pathType, paths);
        }

        public void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }

        public async Task OpenAsync()
        {
            this.ValidateOpen();
            this.ServiceState = JSSoft.Crema.Services.ServiceState.Opening;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnOpening(EventArgs.Empty);
                this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
                this.hostProviders = (this.GetService(typeof(IEnumerable<IServiceHostProvider>)) as IEnumerable<IServiceHostProvider>).TopologicalSort().ToArray();
                this.serviceInfo.Port = this.Port;
                this.serviceInfo.Timeout = this.Timeout;
                this.serviceInfo.Version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;
                this.serviceInfo.PlatformID = $"{Environment.OSVersion.Platform}";
                this.serviceInfo.Culture = $"{CultureInfo.CurrentCulture}";
                this.serviceInfo.ServiceItems = this.hostProviders.Select(item => new ServiceItemInfo() { Name = item.Name }).ToArray();
            });
            this.token = await this.cremaHost.OpenAsync();
            this.configCommitter = this.cremaHost.GetService(typeof(IRepositoryConfiguration)) as IConfigurationCommitter;
            await this.cremaHost.Dispatcher.InvokeAsync(() =>
            {
                this.cremaHost.CloseRequested += CremaHost_CloseRequested;
                this.cremaHost.Closed += CremaHost_Closed;
            });
            await this.StartServicesAsync();
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.ServiceState = JSSoft.Crema.Services.ServiceState.Open;
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync()
        {
            this.ValidateClose();
            this.ServiceState = JSSoft.Crema.Services.ServiceState.Closing;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnClosing(EventArgs.Empty);
            });
            await this.cremaHost.Dispatcher.InvokeAsync(() =>
            {
                this.cremaHost.CloseRequested -= CremaHost_CloseRequested;
                this.cremaHost.Closed -= CremaHost_Closed;
            });
            await this.StopServicesAsync(CloseReason.None);
            await this.cremaHost.CloseAsync(this.token);
            this.configCommitter.Commit();
            this.configCommitter = null;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.token = Guid.Empty;
                this.ServiceState = JSSoft.Crema.Services.ServiceState.Closed;
                this.OnClosed(new ClosedEventArgs(CloseReason.None, string.Empty));
            });
        }

        public int Port { get; set; } = AddressUtility.DefaultPort;

        public int Timeout { get; set; } = 60000;

        public ServiceInfo ServiceInfo => this.ServiceState == JSSoft.Crema.Services.ServiceState.Open ? this.serviceInfo : ServiceInfo.Empty;

        public CremaDispatcher Dispatcher { get; private set; }

        public JSSoft.Crema.Services.ServiceState ServiceState { get; set; }

        public event EventHandler Opening;

        public event EventHandler Opened;

        public event EventHandler Closing;

        public event ClosedEventHandler Closed;

        protected virtual void OnOpening(EventArgs e)
        {
            this.Opening?.Invoke(this, e);
        }

        protected virtual void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        protected virtual void OnClosing(EventArgs e)
        {
            this.Closing?.Invoke(this, e);
        }

        protected virtual void OnClosed(ClosedEventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        protected ILogService LogService => this.cremaHost.GetService(typeof(ILogService)) as ILogService;

        private static bool IsPortUsed(int port)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return false;
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var items = properties.GetActiveTcpListeners();
            return items.Any(item => item.Port == port);
        }

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            if (sender is ICremaHost cremaHost)
            {
                this.cremaHost.Opened -= CremaHost_Opened;
                this.configCommitter = this.cremaHost.GetService(typeof(IRepositoryConfiguration)) as IConfigurationCommitter;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Opening;
                });
                await this.StartServicesAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Open;
                });
            }
        }

        private void CremaHost_CloseRequested(object sender, CloseRequestedEventArgs e)
        {
            e.AddTask(InvokeAsync());
            async Task InvokeAsync()
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Closing;
                });
                await this.StopServicesAsync(e.CloseReason);
            }
        }

        private async void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            if (sender is ICremaHost)
            {
                if (e.Reason == CloseReason.Restart)
                    this.cremaHost.Opened += CremaHost_Opened;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.configCommitter.Commit();
                    this.configCommitter = null;
                    this.ServiceState = JSSoft.Crema.Services.ServiceState.Closed;
                    this.OnClosed(e);
                });
            }
        }

        private async Task StartServicesAsync()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this.hostProviders)
                {
                    var host = item.CreateInstance();
                    this.hosts.Add(host);
                    this.LogService.Info(Resources.ServiceStart, item.Name);
                }
            });
            this.serverContext = await Task.Run(() => new ServerContext(this.hosts.ToArray()));
            this.serviceToken = await this.serverContext.OpenAsync();
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.LogService.Info(Resources.ServiceStart, cremaString);
            });
        }

        private async Task StopServicesAsync(CloseReason closeReason)
        {
            await this.serverContext.CloseAsync(this.serviceToken, (int)closeReason);
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this.hosts.Reverse<ServiceHostBase>())
                {
                    this.LogService.Info(Resources.ServiceStop, item.GetType().Name);
                }
                this.serviceToken = Guid.Empty;
                this.hosts.Clear();
                this.serviceInfo = ServiceInfo.Empty;
                this.LogService.Info(Resources.ServiceStop, cremaString);
            });
        }

        private void ValidateOpen()
        {
            if (this.ServiceState != JSSoft.Crema.Services.ServiceState.None)
                throw new InvalidOperationException();
            this.ValidatePort(this.Port);
        }

        private void ValidateClose()
        {
            if (this.ServiceState != JSSoft.Crema.Services.ServiceState.Open)
                throw new InvalidOperationException();
        }

        private void ValidatePort(int port)
        {
            if (IsPortUsed(port) == true)
            {
                throw new InvalidOperationException($"port {port} can not use.");
            }
        }
    }
}

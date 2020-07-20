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
using Ntreev.Crema.Data.Xml;
using Ntreev.Library.Linq;
using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceHosts.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Diagnostics;
using System.Globalization;
using Ntreev.Library;
using System.Net.NetworkInformation;
using System.Net;
using JSSoft.Communication;

namespace Ntreev.Crema.ServiceHosts
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
        // private CremaHostServiceHost cremaHostServiceHost;
        private Guid token;

        class ServerContext : ServerContextBase
        {
            public ServerContext(params IServiceHost[] serviceHosts)
                : base(serviceHosts)
            {

            }
        }

        public CremaService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.Dispatcher = new CremaDispatcher(typeof(CremaService));
            AuthenticationUtility.Initialize(this);
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public void Dispose()
        {
            AuthenticationUtility.Release(this);
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }

        public async Task OpenAsync()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateOpen();
                this.ServiceState = Ntreev.Crema.Services.ServiceState.Opening;
                this.OnOpening(EventArgs.Empty);
                this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
                this.hostProviders = (this.GetService(typeof(IEnumerable<IServiceHostProvider>)) as IEnumerable<IServiceHostProvider>).TopologicalSort().ToArray();
                var serviceItemList = new List<ServiceItemInfo>(this.hostProviders.Length);
                var port = this.Port;
                foreach (var item in this.hostProviders)
                {
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                        port++;
                    serviceItemList.Add(new ServiceItemInfo()
                    {
                        Name = item.Name,
                        Port = port,
                    });
                }
                this.serviceInfo.Port = this.Port;
                this.serviceInfo.Timeout = this.Timeout;
                this.serviceInfo.Version = $"{new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion)}";
                this.serviceInfo.PlatformID = $"{Environment.OSVersion.Platform}";
                this.serviceInfo.Culture = $"{CultureInfo.CurrentCulture}";
                this.serviceInfo.ServiceItems = serviceItemList.ToArray();
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
                this.ServiceState = Ntreev.Crema.Services.ServiceState.Open;
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateClose();
                this.ServiceState = Ntreev.Crema.Services.ServiceState.Closing;
                this.OnClosing(EventArgs.Empty);
            });
            await this.cremaHost.Dispatcher.InvokeAsync(() =>
            {
                this.cremaHost.CloseRequested -= CremaHost_CloseRequested;
                this.cremaHost.Closed -= CremaHost_Closed;
            });
            await this.StopServicesAsync();
            await this.cremaHost.CloseAsync(this.token);
            this.configCommitter.Commit();
            this.configCommitter = null;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.token = Guid.Empty;
                this.ServiceState = Ntreev.Crema.Services.ServiceState.Closed;
                this.OnClosed(new ClosedEventArgs(CloseReason.Shutdown, string.Empty));
            });
        }

        public int Port { get; set; } = AddressUtility.DefaultPort;

        public int Timeout { get; set; } = 60000;

        public ServiceInfo ServiceInfo => this.ServiceState == Ntreev.Crema.Services.ServiceState.Open ?  this.serviceInfo : ServiceInfo.Empty;

        public CremaDispatcher Dispatcher { get; private set; }

        public Ntreev.Crema.Services.ServiceState ServiceState { get; set; }

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
                    this.ServiceState = Ntreev.Crema.Services.ServiceState.Opening;
                });
                await this.StartServicesAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = Ntreev.Crema.Services.ServiceState.Open;
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
                    this.ServiceState = Ntreev.Crema.Services.ServiceState.Closing;
                });
                await this.StopServicesAsync();
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
                    this.ServiceState = Ntreev.Crema.Services.ServiceState.Closed;
                    this.OnClosed(e);
                });
            }
        }

        private Task StartServicesAsync()
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                // this.cremaHostServiceHost = new CremaHostServiceHost(this, this.Port);
                // this.cremaHostServiceHost.OpenAsync();
                this.LogService.Info(Resources.ServiceStart, nameof(CremaHostServiceHost));
                foreach (var item in this.hostProviders)
                {
                    var serviceItemInfo = this.serviceInfo.GetServiceItem(item.Name);
                    var host = item.CreateInstance();
                    // host.OpenAsync();
                    this.hosts.Add(host);
                    this.LogService.Info(Resources.ServiceStart_Port, host.GetType().Name, serviceItemInfo.Port);
                }
                this.LogService.Info(Resources.ServiceStart, cremaString);
            });
        }

        private Task StopServicesAsync()
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.hosts.Any() == false)
                    return;

                foreach (var item in this.hosts.Reverse<ServiceHostBase>())
                {
                    // item.CloseAsync();
                    this.LogService.Info(Resources.ServiceStop, item.GetType().Name);
                }

                this.hosts.Clear();
                this.serviceInfo = ServiceInfo.Empty;
                this.LogService.Info(Resources.ServiceStop, cremaString);

                // this.cremaHostServiceHost.CloseAsync();
                // this.cremaHostServiceHost = null;
                this.LogService.Info(Resources.ServiceStop, nameof(CremaHostServiceHost));
            });
        }

        private void ValidateOpen()
        {
            if (this.ServiceState != Ntreev.Crema.Services.ServiceState.None)
                throw new InvalidOperationException();

            var providers = (this.GetService(typeof(IEnumerable<IServiceHostProvider>)) as IEnumerable<IServiceHostProvider>).TopologicalSort().ToArray();
            var serviceItemList = new List<ServiceItemInfo>(providers.Length);
            var port = this.Port;
            this.ValidatePort(port);
            foreach (var item in providers)
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    port++;
                this.ValidatePort(port);
            }
        }

        private void ValidateClose()
        {
            if (this.ServiceState != Ntreev.Crema.Services.ServiceState.Open)
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

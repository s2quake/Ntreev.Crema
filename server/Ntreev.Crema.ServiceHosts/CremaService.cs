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

namespace Ntreev.Crema.ServiceHosts
{
    public class CremaService : ICremaService, IServiceProvider
    {
        public const string Namespace = "http://www.ntreev.com";
        private const string cremaString = "Crema";
        private readonly List<ServiceHost> hosts = new List<ServiceHost>();
        private readonly IServiceProvider serviceProvider;
        private ServiceInfo serviceInfo;
        private ICremaHost cremaHost;
        private ILogService logService;
        private IConfigurationCommitter configCommitter;
        private CremaHostServiceHost cremaHostServiceHost;
        private Guid token;

        public CremaService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.Dispatcher = new CremaDispatcher(typeof(CremaServiceItemHost));
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
                this.ServiceState = ServiceState.Opening;
                this.OnOpening(EventArgs.Empty);
                this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
            });
            this.token = await this.cremaHost.OpenAsync();
            this.configCommitter = this.cremaHost.GetService(typeof(IRepositoryConfiguration)) as IConfigurationCommitter;
            this.cremaHost.Closed += CremaHost_Closed;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.logService = this.cremaHost.GetService(typeof(ILogService)) as ILogService;
                this.cremaHostServiceHost = new CremaHostServiceHost(this, this.Port);
                this.cremaHostServiceHost.Open();
                this.logService.Info(Resources.ServiceStart, nameof(CremaHostServiceHost));
            });
            await this.StartServicesAsync();
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.ServiceState = ServiceState.Opened;
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnClosing(EventArgs.Empty);
            });
            await this.StopServicesAsync();
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.cremaHostServiceHost.Close();
                this.cremaHostServiceHost = null;
                this.logService.Info(Resources.ServiceStop, nameof(CremaHostServiceHost));
            });
            await this.cremaHost.CloseAsync(this.token);
            this.configCommitter.Commit();
            this.configCommitter = null;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.token = Guid.Empty;
                this.OnClosed(new ClosedEventArgs(CloseReason.Shutdown, string.Empty));
            });
        }

        public async Task RestartAsync()
        {
            await this.StopServicesAsync();
            await this.cremaHost.CloseAsync(this.token);
            this.configCommitter.Commit();
            this.configCommitter = null;
            this.token = await this.cremaHost.OpenAsync();
            this.configCommitter = this.cremaHost.GetService(typeof(IRepositoryConfiguration)) as IConfigurationCommitter;
            await this.StartServicesAsync();
        }

        public int Port { get; set; } = AddressUtility.DefaultPort;

        public int Timeout { get; set; } = 60000;

        public ServiceInfo ServiceInfo => this.serviceInfo;

        public CremaDispatcher Dispatcher { get; private set; }

        public ServiceState ServiceState { get; set; }

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

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            if (sender is ICremaHost cremaHost)
            {
                cremaHost.Opened -= CremaHost_Opened;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = ServiceState.Opening;
                });
                await this.StartServicesAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = ServiceState.Opened;
                });
            }
        }

        private async void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            if (sender is ICremaHost cremaHost)
            {
                if (e.Reason == CloseReason.Restart)
                    cremaHost.Opened += CremaHost_Opened;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ServiceState = ServiceState.Closing;
                });
                await this.StopServicesAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (e.Reason == CloseReason.Shutdown)
                    {
                        this.cremaHostServiceHost.Close();
                        this.cremaHostServiceHost = null;
                        this.logService.Info(Resources.ServiceStop, nameof(CremaHostServiceHost));
                        this.configCommitter.Commit();
                        this.configCommitter = null;
                        this.token = Guid.Empty;
                        this.ServiceState = ServiceState.Closed;
                        this.OnClosed(e);
                    }
                    else
                    {
                        this.ServiceState = ServiceState.Closed;
                    }
                });
            }
        }

        private Task StartServicesAsync()
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var providers = this.GetService(typeof(IEnumerable<IServiceHostProvider>)) as IEnumerable<IServiceHostProvider>;
                var items = providers.TopologicalSort().ToArray();
                this.serviceInfo.Port = this.Port;
                this.serviceInfo.Timeout = this.Timeout;
                this.serviceInfo.Version = $"{new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion)}";
                this.serviceInfo.PlatformID = $"{Environment.OSVersion.Platform}";
                this.serviceInfo.Culture = $"{CultureInfo.CurrentCulture}";

                var serviceItemList = new List<ServiceItemInfo>(items.Length);
                var port = this.Port + 1;
                foreach (var item in items)
                {
                    var host = item.CreateInstance(port);
                    host.Open();
                    this.hosts.Add(host);
                    this.logService.Info(Resources.ServiceStart_Port, host.GetType().Name, port);
                    serviceItemList.Add(new ServiceItemInfo()
                    {
                        Name = item.Name,
                        Port = port,
                    });
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                        port++;
                }
                this.serviceInfo.ServiceItems = serviceItemList.ToArray();
                this.logService.Info(Resources.ServiceStart, cremaString);
            });
        }

        private Task StopServicesAsync()
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.hosts.Any() == false)
                    return;

                foreach (var item in this.hosts.Reverse<ServiceHost>())
                {
                    item.Close();
                    this.logService.Info(Resources.ServiceStop, item.GetType().Name);
                }

                this.hosts.Clear();
                this.serviceInfo = ServiceInfo.Empty;
                this.logService.Info(Resources.ServiceStop, cremaString);
            });
        }
    }
}

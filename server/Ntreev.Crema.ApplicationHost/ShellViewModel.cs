using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using Ntreev.Crema.ServiceHosts;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ntreev.Crema.ApplicationHost
{
    [Export(typeof(IShell))]
    class ShellViewModel : ScreenBase
    {
        private readonly CremaSettings settings;
        private readonly CremaService service;
        private readonly IAppConfiguration configs;
        private ServiceState serviceState;

        [ImportingConstructor]
        public ShellViewModel(CremaService service, CremaSettings settings, IAppConfiguration configs)
        {
            this.service = service;
            this.settings = settings;
            this.configs = configs;
            this.DisplayName = "Crema Server";
            this.Dispatcher.InvokeAsync(() =>
            {
                this.configs.Update(this);
            });
        }

        public async Task OpenServiceAsync()
        {
            this.ServiceState = ServiceState.Opening;
            try
            {
                this.BeginProgress();
                await this.service.OpenAsync();
                this.ServiceState = ServiceState.Opened;
                this.EndProgress();
            }
            catch (Exception e)
            {
                AppMessageBox.ShowError(e);
                this.ServiceState = ServiceState.None;
                this.EndProgress();
            }
        }

        public async Task CloseServiceAsync()
        {
            this.ServiceState = ServiceState.Closing;
            try
            {
                this.BeginProgress();
                await this.service.CloseAsync();
                this.ServiceState = ServiceState.None;
                this.EndProgress();
            }
            catch (Exception e)
            {
                AppMessageBox.ShowError(e);
                this.ServiceState = ServiceState.Opened;
                this.EndProgress();
            }
        }

        public void SelectBasePath()
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.BasePath = dialog.FileName;
            }
        }

        public bool CanOpenService
        {
            get
            {
                if (Directory.Exists(this.BasePath) == false)
                    return false;
                return this.serviceState == ServiceState.None;
            }
        }

        public bool CanCloseService => this.serviceState == ServiceState.Opened;

        [ConfigurationProperty]
        [DefaultValue("")]
        public string BasePath
        {
            get => this.settings.BasePath ?? string.Empty;
            set
            {
                this.settings.BasePath = value;
                this.NotifyOfPropertyChange(nameof(this.BasePath));
                this.NotifyOfPropertyChange(nameof(this.CanOpenService));
                this.NotifyOfPropertyChange(nameof(this.CanCloseService));
            }
        }

        public ServiceState ServiceState
        {
            get => this.serviceState;
            set
            {
                this.serviceState = value;
                this.NotifyOfPropertyChange(nameof(this.ServiceState));
                this.NotifyOfPropertyChange(nameof(this.CanOpenService));
                this.NotifyOfPropertyChange(nameof(this.CanCloseService));
            }
        }

        protected override async Task CloseAsync()
        {
            if (this.ServiceState == ServiceState.Opened)
            {
                this.ServiceState = ServiceState.Closing;
                await this.service.CloseAsync();
                this.ServiceState = ServiceState.Closed;
            }
        }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(this.ServiceState != ServiceState.Opening && this.ServiceState != ServiceState.Closing);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            this.configs.Commit(this);
        }
    }
}

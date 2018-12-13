using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using Ntreev.Crema.ServiceHosts;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
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
        private int port = 4004;

        [ImportingConstructor]
        public ShellViewModel(CremaService service, CremaSettings settings, IAppConfiguration configs, AppSettings appSettings)
        {
            this.service = service;
            this.settings = settings;
            this.configs = configs;
            this.DisplayName = "Crema Server";
            this.Dispatcher.InvokeAsync(() =>
            {
                this.configs.Update(this);
                this.Initialize(appSettings);
            });
        }

        public async Task OpenServiceAsync()
        {
            this.ServiceState = ServiceState.Opening;
            try
            {
                this.BeginProgress();
                this.service.Port = this.port;
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

        public void CreateRepository()
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var basePath = dialog.FileName;
                var isEmpty = DirectoryUtility.IsEmpty(basePath);
                if (isEmpty == false)
                {
                    AppMessageBox.Show("대상 경로는 비어있지 않습니다.");
                    return;
                }

                CremaBootstrapper.CreateRepository(this.service, basePath, "git", "xml");
                AppMessageBox.Show("저장소를 생성했습니다.");
                this.BasePath = basePath;
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

        [ConfigurationProperty]
        [DefaultValue(4004)]
        public int Port
        {
            get => this.port;
            set
            {
                this.port = value;
                this.NotifyOfPropertyChange(nameof(this.Port));
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

        protected override async Task<bool> CloseAsync()
        {
            if (this.ServiceState == ServiceState.Opened)
            {
                if (AppMessageBox.ShowProceed("서비스가 실행중입니다. 서비스 중지후 종료하시겠습니까?") == true)
                {
                    this.ServiceState = ServiceState.Closing;
                    try
                    {
                        await this.service.CloseAsync();
                    }
                    catch (Exception e)
                    {
                        AppMessageBox.ShowError(e);
                    }
                    finally
                    {
                        this.ServiceState = ServiceState.Closed;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
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

        private async void Initialize(AppSettings appSettings)
        {
            if (appSettings.BasePath != string.Empty)
            {
                this.BasePath = PathUtility.GetFullPath(appSettings.BasePath);
            }

            if (appSettings.Port != 0)
            {
                this.Port = appSettings.Port;
            }

            this.settings.DataBases = appSettings.DataBases;

            if (appSettings.Run == true)
            {
                await this.OpenServiceAsync();
            }
        }
    }
}

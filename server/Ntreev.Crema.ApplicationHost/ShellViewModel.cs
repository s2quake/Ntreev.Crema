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

using Caliburn.Micro;
using Microsoft.Win32;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ntreev.Crema.ApplicationHost
{
    [Export(typeof(IShell))]
    class ShellViewModel : ScreenBase, IShell
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
            await new ProgressAction(this)
            {
                Try = async () =>
                {
                    this.ServiceState = ServiceState.Opening;
                    this.service.Port = this.port;
                    await this.service.OpenAsync();
                    this.ServiceState = ServiceState.Open;
                },
                Catch = async (e) =>
                {
                    this.ServiceState = ServiceState.None;
                    await Task.FromException(e);
                }
            }.RunAsync();
        }

        public async Task CloseServiceAsync()
        {
            await new ProgressAction(this)
            {
                Try = async () =>
                {
                    this.ServiceState = ServiceState.Closing;
                    await this.service.CloseAsync();
                    this.ServiceState = ServiceState.None;
                },
                Catch = async (e) =>
                {
                    this.ServiceState = ServiceState.Open;
                    await Task.FromException(e);
                }
            }.RunAsync();
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

        public async Task CreateRepositoryAsync()
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
                   await AppMessageBox.ShowAsync("대상 경로는 비어있지 않습니다.");
                   return;
               }

               CremaBootstrapper.CreateRepository(this.service, basePath, "git", "xml");
               await AppMessageBox.ShowAsync("저장소를 생성했습니다.");
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

        public bool CanCloseService => this.serviceState == ServiceState.Open;

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
            if (this.ServiceState == ServiceState.Open)
            {
                if (await AppMessageBox.ShowProceedAsync("서비스가 실행중입니다. 서비스 중지후 종료하시겠습니까?") == true)
                {
                    this.ServiceState = ServiceState.Closing;
                    try
                    {
                        await this.service.CloseAsync();
                    }
                    catch (Exception e)
                    {
                        await AppMessageBox.ShowErrorAsync(e);
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

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            return base.TryCloseAsync(this.ServiceState != ServiceState.Opening && this.ServiceState != ServiceState.Closing);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
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

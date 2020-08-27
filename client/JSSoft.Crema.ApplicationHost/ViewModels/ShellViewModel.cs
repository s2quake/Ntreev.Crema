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

using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Linq;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Ntreev.Crema.ApplicationHost.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : ScreenBase, IShell
    {
        private readonly Lazy<ICremaHost> cremaHost;
        private readonly Lazy<ICremaAppHost> cremaAppHost;
        private readonly Lazy<IStatusBarService> statusBarService;
        private readonly Lazy<IMenuService> menuService;
        private readonly IEnumerable<Lazy<IContentService>> contentServices;
        private readonly IEnumerable<Lazy<IBrowserItem>> browsers;
        private object selectedService;

        [ImportingConstructor]
        public ShellViewModel(IServiceProvider serviceProvider,
            Lazy<ICremaHost> cremaHost,
            Lazy<ICremaAppHost> cremaAppHost,
            Lazy<IStatusBarService> statusBarService,
            Lazy<IMenuService> menuService,
            [ImportMany] IEnumerable<Lazy<IContentService>> contentServices,
            [ImportMany] IEnumerable<Lazy<IBrowserItem>> browsers)
            : base(serviceProvider)
        {
            this.cremaHost = cremaHost;
            this.cremaAppHost = cremaAppHost;
            this.statusBarService = statusBarService;
            this.menuService = menuService;
            this.contentServices = contentServices;
            this.browsers = browsers;
        }

        public double Left
        {
            get => Properties.Settings.Default.X;
            set
            {
                if (this.WindowState == WindowState.Maximized)
                    return;
                Properties.Settings.Default.X = value;
            }
        }

        public double Top
        {
            get => Properties.Settings.Default.Y;
            set
            {
                if (this.WindowState == WindowState.Maximized)
                    return;
                Properties.Settings.Default.Y = value;
            }
        }

        public double Width
        {
            get
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) == true)
                    return 700;
                return Properties.Settings.Default.Width;
            }
            set
            {
                if (this.WindowState == WindowState.Maximized)
                    return;
                Properties.Settings.Default.Width = value;
            }
        }

        public double Height
        {
            get
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) == true)
                    return 525;
                return Properties.Settings.Default.Height;
            }
            set
            {
                if (this.WindowState == WindowState.Maximized)
                    return;
                Properties.Settings.Default.Height = value;
            }
        }

        public WindowState WindowState
        {
            get => Properties.Settings.Default.WindowState;
            set => Properties.Settings.Default.WindowState = value;
        }

        public string Title
        {
            get
            {
                if (this.CremaHost.ServiceState == ServiceState.Open)
                    return $"{AppUtility.ProductName} - {this.CremaAppHost.DataBaseName} ({this.CremaHost.Address} - {this.CremaHost.UserID})";
                else
                    return AppUtility.ProductName;
            }
        }

        public IEnumerable Browsers => this.browsers.Select(item => item.Value).ToArray();

        public IStatusBarService StatusBarService => this.statusBarService.Value;

        public IMenuService MenuService => this.menuService.Value;

        public IEnumerable Services => EnumerableUtility.OrderByAttribute(this.contentServices.Select(item => item.Value)).ToArray();

        public ICremaHost CremaHost => this.cremaHost.Value;

        public ICremaAppHost CremaAppHost => this.cremaAppHost.Value;

        public object SelectedService
        {
            get => this.selectedService;
            set
            {
                if (this.selectedService == value)
                    return;

                this.selectedService = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedService));

                if (this.selectedService is IContentService && this.CremaAppHost.IsLoaded == true)
                {
                    var contentService = this.selectedService as IContentService;
                    //this.ActivateItem(contentService);
                    this.SelectedServiceType = contentService.GetType().FullName;
                    this.UserConfigs.Commit(this);
                }

                this.OnServiceChanged(EventArgs.Empty);
            }
        }

        public object Content => this.cremaAppHost;

        public event EventHandler Loaded;

        public event EventHandler Closed;

        public event EventHandler ServiceChanged;

        protected async override Task<bool> CloseAsync()
        {
            var closed = false;
            if (this.CremaAppHost.IsOpened == true)
            {
                this.CremaAppHost.Closed += (s, e) => closed = true;
                await this.CremaAppHost.LogoutAsync();
                while (closed == false)
                {
                    await Task.Delay(1);
                }
            }
            return true;
        }

        protected virtual void OnLoaded(EventArgs e)
        {
            this.Loaded?.Invoke(this, e);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        protected virtual void OnServiceChanged(EventArgs e)
        {
            this.ServiceChanged?.Invoke(this, e);
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            this.SelectedService = this.CremaAppHost;
            this.Refresh();
        }

        protected async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            this.CremaHost.Opened += (s, e) => this.UserConfigs.Update(this);
            this.CremaAppHost.Opened += CremaAppHost_Opened;
            this.CremaAppHost.Loaded += CremaAppHost_Loaded;
            this.CremaAppHost.Unloaded += CremaAppHost_Unloaded;
            this.CremaAppHost.Closed += CremaAppHost_Closed;

            this.OnLoaded(EventArgs.Empty);
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            Properties.Settings.Default.ThemeColor = this.CremaAppHost.ThemeColor;
            Properties.Settings.Default.Save();
            this.OnClosed(EventArgs.Empty);
        }

        private void CremaAppHost_Opened(object sender, EventArgs e)
        {

        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(this.Title));
            var selectedService = this.contentServices.Select(item => item.Value).FirstOrDefault(item => item.GetType().FullName == this.SelectedServiceType);
            if (selectedService == null)
            {
                selectedService = EnumerableUtility.OrderByAttribute(this.contentServices.Select(item => item.Value)).First();
            }
            this.SelectedService = selectedService;
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(this.Title));
            this.SelectedService = null;
            this.SelectedService = this.cremaAppHost;
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.SelectedService = this.cremaAppHost;
        }

        [ConfigurationProperty]
        private string SelectedServiceType { get; set; }

        private IUserConfiguration UserConfigs => this.CremaHost.GetService(typeof(IUserConfiguration)) as IUserConfiguration;

        #region

        async Task IShell.CloseAsync()
        {
            await this.TryCloseAsync();
        }

        #endregion
    }
}

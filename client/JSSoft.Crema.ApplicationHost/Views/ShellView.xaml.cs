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

using FirstFloor.ModernUI.Windows.Controls;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace JSSoft.Crema.ApplicationHost.Views
{
    [Export(typeof(ShellView))]
    public partial class ShellView : ModernWindow, INotifyPropertyChanged
    {
        public static DependencyProperty ToolsProperty =
            DependencyProperty.Register(nameof(Tools), typeof(ToolBar), typeof(ShellView),
                new FrameworkPropertyMetadata(null));

        public static DependencyProperty IsLogVisibleProperty =
            DependencyProperty.Register(nameof(IsLogVisible), typeof(bool), typeof(ShellView),
                new FrameworkPropertyMetadata(false, IsLogVisiblePropertyChangedCallback));

        public static DependencyProperty LogViewHeightProperty =
            DependencyProperty.Register(nameof(LogViewHeight), typeof(double), typeof(ShellView),
                new FrameworkPropertyMetadata(50.0));

        private IMenuService menuService;
        private ICremaHost cremaHost;
        private ICremaAppHost cremaAppHost;
        private IAppConfiguration configs;
        private IShell shell;
        private TextWriter redirectionWriter;

        public event PropertyChangedEventHandler PropertyChanged;

        public ShellView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public ShellView(Lazy<IMenuService> menuService, Lazy<ICremaHost> cremaHost, Lazy<ICremaAppHost> cremaAppHost, Lazy<IAppConfiguration> configs, Lazy<IShell> shell)
        {
            InitializeComponent();

            this.Dispatcher.InvokeAsync(() =>
            {
                this.menuService = menuService.Value;
                this.cremaHost = cremaHost.Value;
                this.cremaAppHost = cremaAppHost.Value;
                foreach (var item in this.menuService.MenuItems)
                {
                    this.SetInputBindings(item);
                }
                this.cremaHost.Opened += CremaHost_Opened;
                this.cremaHost.Closing += CremaHost_Closing;
                this.cremaAppHost.Loaded += CremaAppHost_Loaded;
                this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
                this.configs = configs.Value;
                this.shell = shell.Value;

                App.Writer.TextBox = this.logView;
                this.InitializeFromSettings();

                Application.Current.MainWindow = this;
                this.configs.Update(this);
                if (this.IsLogVisible == true)
                    this.logRow.Height = new GridLength(this.LogViewHeight);
                this.shell.Closed += Shell_Closed;
                this.Dispatcher.InvokeAsync(this.ConnectWithSettings);
            }, DispatcherPriority.Render);
        }

        public ToolBar Tools
        {
            get => (ToolBar)this.GetValue(ToolsProperty);
            set => this.SetValue(ToolsProperty, value);
        }

        [ConfigurationProperty("isLogVisible")]
        public bool IsLogVisible
        {
            get => (bool)this.GetValue(IsLogVisibleProperty);
            set => this.SetValue(IsLogVisibleProperty, value);
        }

        [ConfigurationProperty("logViewHeight")]
        public double LogViewHeight
        {
            get => (double)this.GetValue(LogViewHeightProperty);
            set => this.SetValue(LogViewHeightProperty, value);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name));
        }

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            if (this.cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                await userContext.Dispatcher.InvokeAsync(() =>
                {
                    userCollection.UsersKicked += UserCollection_UsersKicked;
                    userCollection.UsersBanChanged += UserCollection_UsersBanChanged;
                });
            }

            if (this.cremaHost.GetService(typeof(ILogService)) is ILogService logService)
            {
                this.redirectionWriter = new LogWriter() { TextBox = this.logView, };
                logService.AddRedirection(this.redirectionWriter, LogLevel.Debug);
            }
        }

        private void CremaHost_Closing(object sender, EventArgs e)
        {
            var logService = this.cremaHost.GetService(typeof(ILogService)) as ILogService;
            if (this.cremaAppHost.Address != null && this.redirectionWriter is LogWriter writer)
            {
                writer.TextBox = null;
            }
            logService.RemoveRedirection(this.redirectionWriter);
            this.redirectionWriter = null;
        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            var foreground = this.FindResource("WindowText");
            var background = this.FindResource("WindowBackground");

            if (foreground is SolidColorBrush)
                JSSoft.Crema.ApplicationHost.Properties.Settings.Default.Foreground = foreground as SolidColorBrush;
            if (background is SolidColorBrush)
                JSSoft.Crema.ApplicationHost.Properties.Settings.Default.Background = background as SolidColorBrush;
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            var i = Application.Current.Windows.Count;
        }

        private void UserCollection_UsersKicked(object sender, ItemsEventArgs<IUser> e)
        {
            var userID = this.cremaAppHost.UserID;
            var userIDs = e.Items.Select(item => item.ID).ToArray();
            var comments = e.MetaData as string[];

            this.Dispatcher.InvokeAsync(() =>
            {
                for (var i = 0; i < userIDs.Length; i++)
                {
                    if (userIDs[i] == userID)
                    {
                        if (this.IsActive == false)
                        {
                            FlashWindowUtility.FlashWindow(this);
                        }
                        AppMessageBox.ShowAsync(comments[i], Properties.Resources.Message_KickedByAdministrator);
                        break;
                    }
                }
            });
        }

        private async void UserCollection_UsersBanChanged(object sender, ItemsEventArgs<IUser> e)
        {
            foreach (var item in e.Items)
            {
                if (item.ID == this.cremaAppHost.UserID && item.Path != string.Empty)
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        if (this.IsActive == false)
                        {
                            FlashWindowUtility.FlashWindow(this);
                        }
                        AppMessageBox.ShowAsync(item.BanInfo.Comment, Properties.Resources.Message_BannedByAdministrator);
                    });
                    break;
                }
            }
        }

        private void SetInputBindings(IMenuItem menuItem)
        {
            if (menuItem.InputGesture != null)
            {
                if (menuItem.IsVisible == true)
                    this.InputBindings.Add(new InputBinding(menuItem.Command, menuItem.InputGesture));

                if (menuItem is INotifyPropertyChanged notifyObject)
                {
                    notifyObject.PropertyChanged += (s, e) =>
                    {
                        if (menuItem.IsVisible == true)
                        {
                            this.InputBindings.Add(new InputBinding(menuItem.Command, menuItem.InputGesture));
                        }
                        else
                        {
                            for (var i = 0; i < this.InputBindings.Count; i++)
                            {
                                if (this.InputBindings[i].Command == menuItem)
                                {
                                    this.InputBindings.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    };
                }
            }

            foreach (var item in menuItem.MenuItems)
            {
                this.SetInputBindings(item);
            }
        }

        private void ModernWindow_Activated(object sender, EventArgs e)
        {
            FlashWindowUtility.StopFlashingWindow(this);
        }

        private void ModernWindow_Initialized(object sender, EventArgs e)
        {

        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Application.Current.MainWindow = this;
            //this.configs.Update(this);
            //if (this.IsLogVisible == true)
            //    this.logRow.Height = new GridLength(this.LogViewHeight);
            //this.shell.Closed += Shell_Closed;
            //this.Dispatcher.InvokeAsync(this.ConnectWithSettings);
        }

        private void Shell_Closed(object sender, EventArgs e)
        {
            if (this.IsLogVisible == true)
                this.LogViewHeight = this.logRow.ActualHeight;
            this.configs.Commit(this);
        }

        private void ModernWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void IsLogVisiblePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ShellView;
            var isVisible = (bool)e.NewValue;
            if (isVisible == true)
            {
                self.splitter.Visibility = Visibility.Visible;
                self.logRow.Height = new GridLength(self.LogViewHeight);
            }
            else
            {
                self.splitter.Visibility = Visibility.Collapsed;
                self.LogViewHeight = self.logRow.ActualHeight;
                self.logRow.Height = new GridLength(0, GridUnitType.Auto);
            }
        }

        private async void ConnectWithSettings()
        {
            if (this.FindResource("bootstrapper") is AppBootstrapper bootstrapper && Uri.TryCreate(bootstrapper.Settings.Address, UriKind.Absolute, out var uri))
            {
                try
                {
                    var ss = uri.UserInfo.Split(':');
                    var dataBaseName = uri.LocalPath.TrimStart(PathUtility.SeparatorChar);
                    await this.cremaAppHost.LoginAsync(uri.Authority, ss[0], ss[1], dataBaseName);
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                }
            }
        }

        private void InitializeFromSettings()
        {
            if (this.FindResource("bootstrapper") is AppBootstrapper bootstrapper)
            {
                if (bootstrapper.Settings.Theme != string.Empty)
                {
                    this.cremaAppHost.Theme = bootstrapper.Settings.Theme;

                }

                if (bootstrapper.Settings.ThemeColor != string.Empty)
                {
                    try
                    {
                        var themeColor = (Color)ColorConverter.ConvertFromString(bootstrapper.Settings.ThemeColor);
                        this.cremaAppHost.ThemeColor = themeColor;
                    }
                    catch (Exception e)
                    {
                        CremaLog.Error(e);
                    }
                }
            }
        }
    }
}

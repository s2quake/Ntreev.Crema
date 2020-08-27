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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Home.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace JSSoft.Crema.Presentation.Home.Services.ViewModels
{
    [Export(typeof(ICremaAppHost)), PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    [Export(typeof(IContentService))]
    class CremaAppHostViewModel : ViewModelBase, ICremaAppHost, IContentService
    {
        private readonly ICremaHost cremaHost;
        private readonly IAppConfiguration configs;
        private readonly IBuildUp buildUp;
        private readonly Lazy<DataBaseServiceViewModel> dataBaseService;
        private readonly Lazy<DataBaseListViewModel> dataBaseSelections;
        private readonly Lazy<IShell> shell;

        private ConnectionItemViewModel connectionItem;
        private readonly ICommand loginCommand;
        private SecureString securePassword;

        private bool hasError;
        private bool isOpened;
        private bool isLoaded;
        private bool isEncrypted;

        private readonly Authenticator authenticator;
        private string dataBaseName;
        private Guid token;
        private Color themeColor;
        private string theme;
        private string address;
        private IConfigurationCommitter userConfigCommitter;

        private IDataBase dataBase;


        private string filterExpression;
        private bool caseSensitive;
        private bool globPattern;

        static CremaAppHostViewModel()
        {
            Themes.Add("Dark", new Uri("/JSSoft.Crema.Presentation.Framework;component/Assets/CremaUI.Dark.xaml", UriKind.Relative));
            Themes.Add("Light", new Uri("/JSSoft.Crema.Presentation.Framework;component/Assets/CremaUI.Light.xaml", UriKind.Relative));
        }

        public static Dictionary<string, Uri> Themes { get; } = new Dictionary<string, Uri>(StringComparer.CurrentCultureIgnoreCase);

        [ImportingConstructor]
        public CremaAppHostViewModel(ICremaHost cremaHost, IAppConfiguration configs, IBuildUp buildUp,
            Lazy<DataBaseServiceViewModel> dataBaseService, Lazy<DataBaseListViewModel> dataBaseSelections, Lazy<IShell> shell)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += CremaHost_Opened;
            this.configs = configs;
            this.buildUp = buildUp;
            this.dataBaseService = dataBaseService;
            this.dataBaseSelections = dataBaseSelections;
            this.shell = shell;
            this.theme = Themes.Keys.FirstOrDefault();
            this.themeColor = FirstFloor.ModernUI.Presentation.AppearanceManager.Current.AccentColor;
            this.loginCommand = new DelegateCommand((p) => this.LoginAsync(), (p) => this.CanLogin);
            this.ConnectionItems = ConnectionItemCollection.Read(this, AppUtility.GetDocumentFilename("ConnectionList.xml"));
            this.buildUp.BuildUp(this.ConnectionItems);
            this.ConnectionItem = this.ConnectionItems.FirstOrDefault(item => item.IsDefault);
            this.authenticator = this.cremaHost.GetService(typeof(Authenticator)) as Authenticator;
            this.configs.Update(this);
            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.IsProgressing))
                {
                    this.Shell.IsProgressing = this.IsProgressing;
                }
                else if (e.PropertyName == nameof(this.ProgressMessage))
                {
                    this.Shell.ProgressMessage = this.ProgressMessage;
                }
            };
        }

        public override string ToString()
        {
            return Resources.Title_Start;
        }

        public async Task MoveToWikiAsync()
        {
            try
            {
                Process.Start("https://github.com/s2quake/Crema/wiki");
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    await AppMessageBox.ShowErrorAsync(noBrowser.Message);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e.Message);
            }
        }

        public async Task ShowHelpAsync()
        {
            try
            {
                Process.Start("https://github.com/s2quake/Crema/wiki");
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    await AppMessageBox.ShowErrorAsync(noBrowser.Message);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e.Message);
            }
        }

        public Task LoginAsync()
        {
            return this.LoginAsync(this.ConnectionItem.Address, this.ConnectionItem.ID, this.securePassword, this.ConnectionItem.DataBaseName);
        }

        public async Task LoginAsync(string address, string userID, SecureString password, string dataBaseName)
        {
            this.HasError = false;
            this.BeginProgress();

            try
            {
                this.ProgressMessage = Resources.Message_ConnectingToServer;
                if (await CremaBootstrapper.IsOnlineAsync(address, userID, password) == true)
                {
                    if (await AppMessageBox.ShowQuestion(Resources.Message_SameIDConnected) == false)
                    {
                        this.ProgressMessage = string.Empty;
                        this.EndProgress();
                        this.HasError = false;
                        return;
                    }
                }
                await this.OpenAsync(address, userID, password);

                if (dataBaseName != string.Empty)
                {
                    await this.LoadAsync(dataBaseName);
                }
                else
                {
                    this.EndProgress();
                    this.dataBase = null;
                    this.dataBaseName = null;
                    this.address = null;
                    this.Refresh();
                    this.ProgressMessage = string.Empty;
                }
            }
            catch (TimeoutException)
            {
                this.ErrorMessage = Resources.Message_ConnectionFailed;
            }
            //catch (EndpointNotFoundException)
            //{
            //    this.ErrorMessage = Resources.Message_ConnectionFailed;
            //}
            catch (Exception e)
            {
                this.ErrorMessage = e.Message;
                if (this.IsOpened == true)
                {
                    await AppMessageBox.ShowErrorAsync(e.Message);
                }
            }
            finally
            {
                this.EndProgress(this.ProgressMessage);
            }
        }

        public async Task LogoutAsync()
        {
            var closer = new InternalCloseRequestedEventArgs();
            this.OnCloseRequested(closer);
            await closer.WhenAll();

            this.ErrorMessage = string.Empty;
            if (this.DataBaseName != string.Empty)
                await this.UnloadAsync();

            await this.CloseAsync();
            this.NotifyOfPropertyChange(nameof(this.CanLogin));
        }

        public async void AddConnectionItem()
        {
            var dialog = new ConnectionItemEditViewModel(this);
            if (await dialog.ShowDialogAsync() == true)
            {
                this.ConnectionItems.Add(dialog.ConnectionInfo);
                this.ConnectionItems.Write();
            }
        }

        public void RemoveConnectionItem(ConnectionItemViewModel connectionItem)
        {
            if (this.ConnectionItem == connectionItem)
            {
                this.ConnectionItem = null;
            }
            this.ConnectionItems.Remove(connectionItem);
        }

        public async void RemoveConnectionItem()
        {
            if (await AppMessageBox.ConfirmDeleteAsync() == false)
                return;
            this.RemoveConnectionItem(this.ConnectionItem);
        }

        public void EditConnectionItem()
        {
            this.EditConnectionItem(this.ConnectionItem);
        }

        public async void EditConnectionItem(ConnectionItemViewModel connectionItem)
        {
            var dialog = new ConnectionItemEditViewModel(this, connectionItem.Clone());
            if (await dialog.ShowDialogAsync() == true)
            {
                connectionItem.Assign(dialog.ConnectionInfo);
                this.ConnectionItems.Write();
                if (this.ConnectionItem == connectionItem)
                {
                    FirstFloor.ModernUI.Presentation.AppearanceManager.Current.AccentColor = connectionItem.ThemeColor;
                    FirstFloor.ModernUI.Presentation.AppearanceManager.Current.ThemeSource = Themes[connectionItem.Theme];
                    this.SetPassword(this.ConnectionItem.Password, true);
                }
            }
        }

        public void SetPassword(string password, bool isEncrypted)
        {
            if (string.IsNullOrEmpty(password) == true)
            {
                this.securePassword = null;
            }
            else
            {
                this.securePassword = new SecureString();
                try
                {
                    if (isEncrypted == true)
                    {
                        foreach (var item in StringUtility.Decrypt(password, this.ConnectionItem.ID))
                        {
                            this.securePassword.AppendChar(item);
                        }
                    }
                    else
                    {
                        foreach (var item in password)
                        {
                            this.securePassword.AppendChar(item);
                        }
                    }
                }
                catch
                {
                    return;
                }
            }
            this.isEncrypted = isEncrypted;
            this.NotifyOfPropertyChange(nameof(this.CanLogin));
        }

        public async void SelectDataBase(string dataBaseName)
        {
            try
            {
                if (this.isOpened == false)
                    throw new InvalidOperationException(Resources.Exception_CannotSelectWithoutLoggingIn);

                this.BeginProgress();
                if (this.DataBaseName != string.Empty)
                    await this.UnloadAsync();
                await this.LoadAsync(dataBaseName);
                this.EndProgress();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                this.EndProgress();
            }
        }

        public async Task LoadAsync(string dataBaseName)
        {
            var waiter = new TaskWaiter();
            var isProgressing = this.IsProgressing;
            this.OnLoading(EventArgs.Empty);

            if (isProgressing == false)
                this.BeginProgress();
            this.ProgressMessage = Resources.Message_LoadingDataBase;
            try
            {
                var autoLoad = this.authenticator.Authority == Authority.Admin && Keyboard.Modifiers == ModifierKeys.Shift;
                await this.EnterDataBaseAsync(dataBaseName);
                await waiter.WaitAsync(2000);
                this.dataBaseName = dataBaseName;
            }
            catch
            {
                if (isProgressing == false)
                    this.EndProgress();
                this.dataBase = null;
                this.dataBaseName = null;
                this.Refresh();
                throw;
            }

            this.IsLoaded = true;
            if (isProgressing == false)
                this.EndProgress();
            this.Refresh();
            this.OnLoaded(EventArgs.Empty);
        }

        public async Task UnloadAsync()
        {
            if (this.DataBaseName == string.Empty)
                throw new InvalidOperationException();

            await this.CloseDocumentsAsync(false);
            var args = new InternalCloseRequestedEventArgs();
            this.OnUnloadRequested(args);
            await args.WhenAll();
            this.OnUnloading(EventArgs.Empty);
            this.BeginProgress();
            await this.LeaveDataBaseAsync();
            this.dataBaseName = null;
            this.IsLoaded = false;
            this.EndProgress();
            this.Refresh();
            this.OnUnloaded(EventArgs.Empty);
        }

        public bool IsLoaded
        {
            get => this.isLoaded;
            private set
            {
                this.isLoaded = value;
                this.NotifyOfPropertyChange(nameof(this.IsLoaded));
                this.NotifyOfPropertyChange(nameof(this.IsVisible));
            }
        }

        public bool IsOpened
        {
            get => this.isOpened;
            private set
            {
                this.isOpened = value;
                this.NotifyOfPropertyChange(nameof(this.IsOpened));
                this.NotifyOfPropertyChange(nameof(this.IsVisible));
            }
        }

        public bool HasError
        {
            get => this.hasError;
            set
            {
                this.hasError = value;
                this.NotifyOfPropertyChange(nameof(this.HasError));
            }
        }

        public string ErrorMessage
        {
            get => this.ProgressMessage;
            set
            {
                this.ProgressMessage = value;
                this.NotifyOfPropertyChange(nameof(this.ErrorMessage));
                this.HasError = (value ?? string.Empty) != string.Empty;
            }
        }

        public string DataBaseName => this.dataBaseName ?? string.Empty;

        public string Address => this.address ?? string.Empty;

        public Color ThemeColor
        {
            get => this.themeColor;
            set
            {
                this.themeColor = value;
                FirstFloor.ModernUI.Presentation.AppearanceManager.Current.AccentColor = value;
                this.NotifyOfPropertyChange(nameof(ThemeColor));
            }
        }

        public string Theme
        {
            get => this.theme;
            set
            {
                var themeValue = Themes[value];
                FirstFloor.ModernUI.Presentation.AppearanceManager.Current.ThemeSource = themeValue;
                this.theme = Themes.First(item => item.Value == themeValue).Key;
                this.NotifyOfPropertyChange(nameof(Theme));
            }
        }

        public Authority Authority { get; private set; }

        public bool IsVisible => this.IsOpened;

        public DataBaseServiceViewModel DataBaseService => this.dataBaseService.Value;

        public ConnectionItemCollection ConnectionItems { get; }

        public ConnectionItemViewModel ConnectionItem
        {
            get => this.connectionItem;
            set
            {
                if (this.connectionItem == value)
                    return;

                this.ValidateSetConnectionItem(value);

                if (this.connectionItem != null)
                {
                    this.connectionItem.IsDefault = false;
                }
                this.connectionItem = value;
                if (this.connectionItem != null)
                {
                    this.connectionItem.IsDefault = true;
                    this.ThemeColor = this.connectionItem.ThemeColor;
                    this.Theme = this.connectionItem.Theme;
                    this.SetPassword(this.connectionItem.Password, true);
                }

                this.NotifyOfPropertyChange(nameof(this.CanLogin));
                this.NotifyOfPropertyChange(nameof(this.ConnectionItem));
                this.NotifyOfPropertyChange(nameof(this.CanRemoveConnectionItem));
                this.NotifyOfPropertyChange(nameof(this.CanEditConnectionItem));
            }
        }

        public bool CanLogin
        {
            get
            {
                if (this.ConnectionItem == null)
                    return false;
                if (this.securePassword == null)
                    return false;
                if (this.IsProgressing == true)
                    return false;
                return true;
            }
        }

        public bool CanRemoveConnectionItem => this.ConnectionItem != null;

        public bool CanEditConnectionItem => this.ConnectionItem != null;

        public string FilterExpression
        {
            get => this.filterExpression ?? string.Empty;
            set
            {
                if (this.filterExpression == value)
                    return;

                if (this.FilterExpression == string.Empty)
                {
                    foreach (var item in this.ConnectionItems)
                    {
                        item.IsVisible = true;
                        item.Pattern = string.Empty;
                    }
                }

                this.filterExpression = value;

                if (this.FilterExpression == string.Empty)
                {
                    foreach (var item in this.ConnectionItems)
                    {
                        item.IsVisible = true;
                        item.Pattern = string.Empty;
                    }
                }
                else
                {
                    foreach (var item in this.ConnectionItems)
                    {
                        item.IsVisible = this.Filter(item);
                        item.Pattern = this.FilterExpression;
                        item.CaseSensitive = this.CaseSensitive;
                    }

                    if (this.connectionItem != null && this.connectionItem.IsVisible == false)
                        this.ConnectionItem = null;
                }

                this.NotifyOfPropertyChange(nameof(this.FilterExpression));
            }
        }

        public bool CaseSensitive
        {
            get => this.caseSensitive;
            set
            {
                this.caseSensitive = value;
                this.NotifyOfPropertyChange(nameof(this.CaseSensitive));
            }
        }

        public bool GlobPattern
        {
            get => this.globPattern;
            set
            {
                this.globPattern = value;
                this.NotifyOfPropertyChange(nameof(this.GlobPattern));
            }
        }

        public IUserConfiguration UserConfigs { get; private set; }

        public string DisplayName => Resources.Title_Start;

        public event EventHandler Loading;

        public event EventHandler Loaded;

        public event CloseRequestedEventHandler UnloadRequested;

        public event EventHandler Unloading;

        public event EventHandler Unloaded;

        public event EventHandler Resetting;

        public event EventHandler Reset;

        public event EventHandler Opened;

        public event CloseRequestedEventHandler CloseRequested;

        public event EventHandler Closed;

        protected virtual void OnLoading(EventArgs e)
        {
            this.Loading?.Invoke(this, e);
        }

        protected virtual void OnLoaded(EventArgs e)
        {
            this.Loaded?.Invoke(this, e);
        }

        protected virtual void OnUnloadRequested(CloseRequestedEventArgs e)
        {
            this.UnloadRequested?.Invoke(this, e);
        }

        protected virtual void OnUnloading(EventArgs e)
        {
            this.Unloading?.Invoke(this, e);
        }

        protected virtual void OnUnloaded(EventArgs e)
        {
            this.Unloaded?.Invoke(this, e);
        }

        protected virtual void OnResetting(EventArgs e)
        {
            this.Resetting?.Invoke(this, e);
        }

        protected virtual void OnReset(EventArgs e)
        {
            this.Reset?.Invoke(this, e);
        }

        protected virtual void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        protected virtual void OnCloseRequested(CloseRequestedEventArgs e)
        {
            this.CloseRequested?.Invoke(this, e);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        private async void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            this.cremaHost.Closed -= CremaHost_Closed;
            this.isOpened = false;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.IsOpened = false;
                this.EndProgress();
                if (e.Reason != CloseReason.None)
                    this.ErrorMessage = e.Message;
                else
                    this.ProgressMessage = e.Message;

                if (this.IsLoaded == true)
                {
                    this.dataBaseName = null;
                    this.IsLoaded = false;
                    this.OnUnloaded(EventArgs.Empty);
                }
                this.address = null;
                this.Refresh();
                this.OnClosed(EventArgs.Empty);
                this.userConfigCommitter.Commit();
                this.userConfigCommitter = null;
                this.UserConfigs = null;
            });
        }

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            this.isOpened = true;
            this.Authority = this.cremaHost.Authority;
            this.cremaHost.Closed += CremaHost_Closed;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.configs.Commit(this);
                this.Refresh();
            });
        }

        private async void DataBase_Unloaded(object sender, EventArgs e)
        {
            var dataBase = sender as IDataBase;
            dataBase.Unloaded -= DataBase_Unloaded;
            var dataBaseName = dataBase.Name;
            if (this.dataBaseName == dataBaseName)
            {
                this.isLoaded = false;
            }
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.dataBaseName == dataBaseName)
                {
                    this.dataBase = null;
                    this.dataBaseName = null;
                    this.IsLoaded = false;
                    this.Refresh();
                    this.OnUnloaded(EventArgs.Empty);
                }
            });
        }

        private void DataBase_Resetting(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                this.BeginProgress();
                this.OnResetting(EventArgs.Empty);
            });
        }

        private void DataBase_Reset(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                this.OnReset(EventArgs.Empty);
                this.EndProgress();
            });
        }

        private async Task OpenAsync(string address, string userID, SecureString password)
        {
            this.token = await this.cremaHost.OpenAsync(address, userID, password);
            this.address = address;
            this.IsOpened = true;
            this.connectionItem.LastConnectedDateTime = DateTime.Now;
            this.ConnectionItems.Write();
            this.OnOpened(EventArgs.Empty);
            this.UserConfigs = this.cremaHost.GetService(typeof(IUserConfiguration)) as IUserConfiguration;
            this.userConfigCommitter = this.cremaHost.GetService(typeof(IUserConfiguration)) as IConfigurationCommitter;
        }

        private async Task CloseAsync()
        {
            this.cremaHost.Closed -= CremaHost_Closed;
            await this.cremaHost.CloseAsync(this.token);
            this.token = Guid.Empty;
            this.address = null;
            this.IsOpened = false;
            this.Refresh();
            this.OnClosed(EventArgs.Empty);
            this.userConfigCommitter.Commit();
            this.userConfigCommitter = null;
            this.UserConfigs = null;
        }

        private async Task EnterDataBaseAsync(string dataBaseName)
        {
            var dataBases = this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var autoLoad = this.authenticator.Authority == Authority.Admin && Keyboard.Modifiers == ModifierKeys.Shift;
            var dataBase = await dataBases.Dispatcher.InvokeAsync(() => dataBases[dataBaseName]);
            if (dataBase == null)
                throw new ArgumentException(string.Format(Resources.Exception_NonExistentDataBase, dataBaseName), nameof(dataBaseName));
            if (dataBase.IsLoaded == false && autoLoad == true)
            {
                await dataBase.LoadAsync(this.authenticator);
            }
            await dataBase.EnterAsync(this.authenticator);
            await dataBase.Dispatcher.InvokeAsync(() =>
            {
                dataBase.Unloaded += DataBase_Unloaded;
                dataBase.Resetting += DataBase_Resetting;
                dataBase.Reset += DataBase_Reset;
            });
            this.dataBase = dataBase;
        }

        private async Task LeaveDataBaseAsync()
        {
            await this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                this.dataBase.Unloaded -= DataBase_Unloaded;
                this.dataBase.Resetting -= DataBase_Resetting;
                this.dataBase.Reset -= DataBase_Reset;
            });
            await this.dataBase.LeaveAsync(this.authenticator);
            this.dataBase = null;
        }

        private async Task CloseDocumentsAsync(bool save)
        {
            var documentServices = this.cremaHost.GetService(typeof(IEnumerable<IDocumentService>)) as IEnumerable<IDocumentService>;
            var query = from documentService in documentServices
                        from document in documentService.Documents
                        select document;

            var documentList = query.ToList();
            foreach (var item in documentList.ToArray())
            {
                item.Disposed += (s, e) => documentList.Remove(item);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (item.IsModified == true && save == false)
                        item.IsModified = false;
                    item.Dispose();
                });
                await Task.Delay(1);
            }

            while (documentList.Any())
            {
                await Task.Delay(1);
            }
        }

        private void ValidateSetConnectionItem(ConnectionItemViewModel value)
        {
            if (this.IsOpened == true)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (this.connectionItem.Address != value.Address || this.connectionItem.ID != value.ID || this.connectionItem.PasswordID != value.PasswordID)
                    throw new ArgumentException(Resources.Exception_InvalidConnectionItem, nameof(value));
            }
        }

        private bool Filter(ConnectionItemViewModel connectionItem)
        {
            if (this.Filter(connectionItem.Name) == true)
                return true;
            if (this.Filter(connectionItem.Address) == true)
                return true;
            if (this.Filter(connectionItem.DataBaseName) == true)
                return true;
            if (this.Filter(connectionItem.ID) == true)
                return true;
            return false;
        }

        private bool Filter(string text)
        {
            if (this.GlobPattern == true)
                return StringUtility.Glob(text, this.FilterExpression, this.CaseSensitive);
            else if (this.CaseSensitive == false)
                return text.IndexOf(filterExpression, StringComparison.OrdinalIgnoreCase) >= 0;
            return text.IndexOf(filterExpression) >= 0;
        }

        private IShell Shell => this.shell.Value;

        #region IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataBase))
                return this.dataBase;
            return this.cremaHost.GetService(serviceType);
        }

        #endregion

        #region ICremaAppHost

        async Task ICremaAppHost.LoginAsync(string address, string userID, string password, string dataBaseName)
        {
            var connectionItem = new ConnectionItemViewModel(this)
            {
                Name = "Temporary",
                Address = address,
                ID = userID,
                Password = StringUtility.Encrypt(password, userID),
                DataBaseName = dataBaseName,
                IsTemporary = true,
                Theme = this.Theme,
                ThemeColor = this.ThemeColor,
            };

            this.ConnectionItems.Add(connectionItem);
            this.ConnectionItem = connectionItem;
            await this.LoginAsync();
        }

        IEnumerable<IConnectionItem> ICremaAppHost.ConnectionItems => this.ConnectionItems;

        IConnectionItem ICremaAppHost.ConnectionItem
        {
            get => this.ConnectionItem;
            set
            {
                if (value is ConnectionItemViewModel connectionItem)
                {
                    this.ConnectionItem = connectionItem;
                }
            }
        }

        IEnumerable<IDataBaseDescriptor> ICremaAppHost.DataBases => this.dataBaseSelections.Value.Items;

        #endregion
    }
}

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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Home.Services.ViewModels
{
    [Export]
    class DataBaseListViewModel : ListBoxBase<DataBaseItemViewModel>
    {
        private readonly ICremaHost cremaHost;
        private readonly Lazy<CremaAppHostViewModel> cremaAppHost;
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public DataBaseListViewModel(Authenticator authenticator, ICremaHost cremaHost, Lazy<CremaAppHostViewModel> cremaAppHost, IPropertyService propertyService)
        {
            this.authenticator = authenticator;
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaAppHost = cremaAppHost;
            this.AttachPropertyService(propertyService);
            this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaAppHost.Loaded += CremaAppHost_Loaded;
                this.CremaAppHost.Unloaded += CremaAppHost_Unloaded;
            });
        }

        public async void SelectDataBase()
        {
            var dialog = new SelectDataBaseViewModel(this.authenticator, this.CremaAppHost);
            if (await dialog.ShowDialogAsync() != true)
                return;

            try
            {
                await this.CremaAppHost.LoadAsync(dialog.SelectedItem.Name);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public async Task EnterAsync()
        {
            try
            {
                if (this.CremaAppHost.IsLoaded == true)
                    await this.CremaAppHost.UnloadAsync();
                await this.CremaAppHost.LoadAsync(this.SelectedItem.Name);
                this.NotifyOfPropertyChange(nameof(this.CanEnter));
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public async Task CreateAsync()
        {
            await DataBaseUtility.CreateAsync(this.authenticator, this.cremaHost);
        }

        public bool CanEnter
        {
            get
            {
                if (this.SelectedItem == null)
                    return false;
                if (this.CremaAppHost.IsProgressing == true)
                    return false;
                if (this.SelectedItem.Name == this.CremaAppHost.DataBaseName)
                    return false;
                if (this.SelectedItem.ConnectionItem == null)
                    return this.authenticator.Authority == Authority.Admin && Keyboard.Modifiers == ModifierKeys.Shift;
                return true;
            }
        }

        public bool CanCreate
        {
            get
            {
                if (this.authenticator.IsOpened == false)
                    return false;
                return DataBaseUtility.CanCreate(this.authenticator);
            }
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);
            this.NotifyOfPropertyChange(nameof(this.CanEnter));
        }

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            var dataBases = await this.DataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                this.DataBaseContext.ItemsCreated += DataBaseContext_ItemsCreated;
                this.DataBaseContext.ItemsDeleted += DataBaseContext_ItemsDeleted;
                return this.DataBaseContext.Select(item => new DataBaseItemViewModel(this.authenticator, item, this)).ToArray();
            });

            await this.Dispatcher.InvokeAsync(() =>
            {
                var query = from connectionItem in this.CremaAppHost.ConnectionItems
                            where connectionItem.Address == this.CremaAppHost.ConnectionItem.Address &&
                                  connectionItem.ID == this.CremaAppHost.ConnectionItem.ID &&
                                  connectionItem.PasswordID == this.CremaAppHost.ConnectionItem.PasswordID
                            join dataBase in dataBases on connectionItem.DataBaseName equals dataBase.Name
                            select connectionItem;

                this.Items.Clear();
                foreach (var item in dataBases)
                {
                    var connectionItem = query.FirstOrDefault(i => i.DataBaseName == item.Name);
                    if (connectionItem != null)
                    {
                        item.ConnectionItem = connectionItem;
                    }
                    this.Items.Add(item);
                }
                this.Refresh();
            });
        }

        private async void DataBaseContext_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
            var dataBases = e.Items.Select(item => new DataBaseItemViewModel(this.authenticator, item, this)).ToArray();
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in dataBases)
                {
                    this.Items.Add(item);
                }
            });
        }

        private async void DataBaseContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
        {
            var dataBaseNames = e.ItemPaths;
            await this.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in dataBaseNames
                            join viewModel in this.Items on item equals viewModel.Name
                            select viewModel;
                foreach (var item in query)
                {
                    this.Items.Remove(item);
                }
            });
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            foreach (var item in this.Items)
            {
                item.IsCurrent = false;
            }
        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            foreach (var item in this.Items)
            {
                if (item.Name == this.CremaAppHost.DataBaseName)
                    item.IsCurrent = true;
            }
        }

        private CremaAppHostViewModel CremaAppHost => this.cremaAppHost.Value;

        private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    }
}

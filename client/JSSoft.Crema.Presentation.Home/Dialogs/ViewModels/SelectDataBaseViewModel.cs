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
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Home.Dialogs.ViewModels
{
    public class SelectDataBaseViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly ICremaAppHost cremaAppHost;
        private readonly Func<DataBaseItemViewModel, bool> predicate;
        private DataBaseItemViewModel selectedItem;
        private string selectedValue;
        private readonly Func<Task> actionAsync;

        public SelectDataBaseViewModel(ICremaAppHost cremaAppHost, string address)
            : this(cremaAppHost, address, (s) => true)
        {

        }

        public SelectDataBaseViewModel(ICremaAppHost cremaAppHost, string address, Func<DataBaseItemViewModel, bool> predicate)
            : base(cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.predicate = predicate;
            this.actionAsync = () => this.InitializeAsync(address);
            this.DisplayName = Resources.Title_SelectDataBase;
        }

        public SelectDataBaseViewModel(Authentication authentication, ICremaAppHost cremaAppHost)
            : this(authentication, cremaAppHost, (s) => true)
        {

        }

        public SelectDataBaseViewModel(Authentication authentication, ICremaAppHost cremaAppHost, Func<DataBaseItemViewModel, bool> predicate)
            : base(cremaAppHost)
        {
            this.authentication = authentication;
            this.predicate = predicate;
            this.actionAsync = () => this.InitializeAsync(authentication, cremaAppHost);
            this.SupportsDescriptor = true;
            this.DisplayName = Resources.Title_SelectDataBase;
        }

        public async Task OKAsync()
        {
            await this.TryCloseAsync(true);
        }

        public ObservableCollection<DataBaseItemViewModel> ItemsSource { get; } = new ObservableCollection<DataBaseItemViewModel>();

        public DataBaseItemViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.selectedItem != null)
                    this.selectedItem.IsSelected = false;
                this.selectedItem = value;
                if (this.selectedItem != null)
                    this.selectedItem.IsSelected = true;
                this.selectedValue = this.selectedItem?.Name;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.SelectedValue));
                this.NotifyOfPropertyChange(nameof(this.CanOK));
            }
        }

        public string SelectedValue
        {
            get => this.selectedValue ?? string.Empty;
            set => this.selectedValue = value;
        }

        public bool CanOK
        {
            get
            {
                if (this.selectedItem == null)
                    return false;
                return this.predicate(this.selectedItem);
            }
        }

        public bool ConnectableOnly { get; set; }

        public bool SupportsDescriptor { get; }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
            await this.actionAsync();
        }

        private async Task InitializeAsync(string address)
        {
            try
            {
                this.BeginProgress(Resources.Message_ReceivingInformation);
                var dataBaseInfos = await CremaBootstrapper.GetDataBasesAsync(address);
                var query = from connectionItem in this.cremaAppHost.ConnectionItems
                            where connectionItem.Address == this.cremaAppHost.ConnectionItem.Address &&
                                  connectionItem.ID == this.cremaAppHost.ConnectionItem.ID &&
                                  connectionItem.PasswordID == this.cremaAppHost.ConnectionItem.PasswordID
                            join dataBase in dataBaseInfos on connectionItem.DataBaseName equals dataBase.Name
                            select connectionItem;

                foreach (var item in dataBaseInfos)
                {
                    var viewModel = new DataBaseItemViewModel(this.authentication, item, this);
                    if (this.ConnectableOnly == true)
                    {
                        viewModel.ConnectionItem = query.FirstOrDefault(i => i.DataBaseName == item.Name);
                    }
                    this.ItemsSource.Add(viewModel);
                }
                this.selectedItem = this.ItemsSource.FirstOrDefault(item => item.Name == this.selectedValue);
                this.EndProgress();
                this.Refresh();
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(Resources.Message_ConnectionFailed, e);
                await this.TryCloseAsync();
            }
        }

        private async Task InitializeAsync(Authentication authentication, ICremaAppHost cremaAppHost)
        {
            try
            {
                this.BeginProgress(Resources.Message_ReceivingInformation);
                var dataBases = cremaAppHost.DataBases.ToArray();

                var query = from connectionItem in this.cremaAppHost.ConnectionItems
                            where connectionItem.Address == this.cremaAppHost.ConnectionItem.Address &&
                                  connectionItem.ID == this.cremaAppHost.ConnectionItem.ID &&
                                  connectionItem.PasswordID == this.cremaAppHost.ConnectionItem.PasswordID
                            join dataBase in dataBases on connectionItem.DataBaseName equals dataBase.DataBaseInfo.Name
                            select connectionItem;

                foreach (var item in dataBases)
                {
                    var viewModel = new DataBaseItemViewModel(authentication, item, this);
                    if (this.ConnectableOnly == true)
                    {
                        viewModel.ConnectionItem = query.FirstOrDefault(i => i.DataBaseName == item.DataBaseInfo.Name);
                    }
                    this.ItemsSource.Add(viewModel);
                }
                this.selectedItem = this.ItemsSource.FirstOrDefault(item => item.Name == this.selectedValue);
                this.EndProgress();
                this.Refresh();
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(Resources.Message_ConnectionFailed, e);
                await this.TryCloseAsync();
            }
        }
    }
}

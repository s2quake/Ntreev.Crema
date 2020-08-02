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

using Ntreev.Crema.Presentation.Base.Properties;
using Ntreev.Crema.Presentation.Base.Services.ViewModels;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Base.Dialogs.ViewModels
{
    class ConnectionItemEditViewModel : ModalDialogBase
    {
        private readonly ICremaAppHost cremaAppHost;
        private ConnectionItemViewModel connectionItemInfo;
        private bool isModified;
        private bool isPasswordChanged;

        public ConnectionItemEditViewModel(ICremaAppHost cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.connectionItemInfo = new ConnectionItemViewModel();
            this.connectionItemInfo.PropertyChanged += ConnectionItemInfo_PropertyChanged;
            this.IsNew = true;
            this.DisplayName = Resources.Title_AddConnectionItem;
        }

        public ConnectionItemEditViewModel(ICremaAppHost cremaAppHost, ConnectionItemViewModel connectionItemInfo)
        {
            this.cremaAppHost = cremaAppHost;
            this.connectionItemInfo = connectionItemInfo ?? throw new ArgumentNullException(nameof(connectionItemInfo));
            if (this.connectionItemInfo.IsTemporary == true)
                throw new ArgumentException();
            this.connectionItemInfo.PropertyChanged += ConnectionItemInfo_PropertyChanged;
            this.DisplayName = Resources.Title_EditConnectionItem;
        }

        public async Task ChangeAsync()
        {
            if (this.isPasswordChanged == true)
            {
                this.connectionItemInfo.Password = StringUtility.Encrypt(this.connectionItemInfo.Password, this.connectionItemInfo.ID);
            }
            await this.TryCloseAsync(true);
        }

        public async Task SelectDataBaseAsync()
        {
            var dialog = new SelectDataBaseViewModel(this.cremaAppHost, this.ConnectionInfo.Address)
            {
                SelectedValue = this.connectionItemInfo.DataBaseName,
            };

            if (await dialog.ShowDialogAsync() == true)
            {
                this.connectionItemInfo.DataBaseName = dialog.SelectedItem.Name;
            }
        }

        public async Task SelectThemeColorAsync()
        {
            var dialog = new SelectColorViewModel()
            {
                CurrentColor = this.connectionItemInfo.ThemeColor,
            };

            if (await dialog.ShowDialogAsync() == true)
            {
                this.connectionItemInfo.ThemeColor = dialog.CurrentColor;
            }
        }

        public ConnectionItemViewModel ConnectionInfo
        {
            get => this.connectionItemInfo;
            set
            {
                if (this.connectionItemInfo != null)
                    this.connectionItemInfo.PropertyChanged -= ConnectionItemInfo_PropertyChanged;
                this.connectionItemInfo = value;
                if (this.connectionItemInfo != null)
                    this.connectionItemInfo.PropertyChanged += ConnectionItemInfo_PropertyChanged;
                this.isModified = false;
                this.NotifyOfPropertyChange(nameof(this.ConnectionInfo));
                this.NotifyOfPropertyChange(nameof(this.isModified));
            }
        }

        public IEnumerable<string> Themes => CremaAppHostViewModel.Themes.Keys;

        public bool CanSelectDataBase
        {
            get
            {
                if (string.IsNullOrEmpty(this.connectionItemInfo.Address) == true)
                    return false;
                return true;
            }
        }

        public bool CanChange
        {
            get
            {
                if (this.isModified == false)
                    return false;
                if (this.connectionItemInfo.Address == string.Empty)
                    return false;
                if (this.connectionItemInfo.ID == string.Empty)
                    return false;
                return true;
            }
        }

        public bool IsNew { get; }

        private void ConnectionItemInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectionItemViewModel.Address))
            {
                this.NotifyOfPropertyChange(nameof(this.CanSelectDataBase));
            }
            else if (e.PropertyName == nameof(ConnectionItemViewModel.Password))
            {
                this.isPasswordChanged = true;
            }
            this.isModified = true;
            this.NotifyOfPropertyChange(nameof(this.CanChange));
        }
    }
}

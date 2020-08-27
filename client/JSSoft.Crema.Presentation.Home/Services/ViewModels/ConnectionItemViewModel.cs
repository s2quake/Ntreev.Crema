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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace JSSoft.Crema.Presentation.Home.Services.ViewModels
{
    class ConnectionItemViewModel : ListBoxItemViewModel, IConnectionItem
    {
        private readonly CremaAppHostViewModel cremaAppHost;
        private string name;
        private string address;
        private string dataBaseName;
        private string userID;
        private string password;
        private string theme;
        private Color themeColor = FirstFloor.ModernUI.Presentation.AppearanceManager.Current.AccentColor;
        private bool isCurrentTheme;
        private bool isTemporary;

        public ConnectionItemViewModel(CremaAppHostViewModel cremaAppHost)
            : base(cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost ?? throw new ArgumentNullException(nameof(cremaAppHost));
        }

        public ConnectionItemViewModel Clone()
        {
            return new ConnectionItemViewModel(this.cremaAppHost)
            {
                name = this.name,
                address = this.address,
                dataBaseName = this.dataBaseName,
                userID = this.userID,
                password = this.password,
                themeColor = this.themeColor,
                theme = this.theme,
                IsDefault = this.IsDefault,
                LastConnectedDateTime = this.LastConnectedDateTime,
            };
        }

        public void Assign(ConnectionItemViewModel connectionInfo)
        {
            this.name = connectionInfo.name;
            this.address = connectionInfo.address;
            this.dataBaseName = connectionInfo.dataBaseName;
            this.userID = connectionInfo.userID;
            this.password = connectionInfo.password;
            this.themeColor = connectionInfo.themeColor;
            this.theme = connectionInfo.theme;
            this.IsDefault = connectionInfo.IsDefault;
            this.LastConnectedDateTime = connectionInfo.LastConnectedDateTime;
            this.Refresh();
            this.RefreshIsCurrentThemeProperty();
        }

        public string Name
        {
            get => this.name ?? string.Empty;
            set
            {
                this.name = value;
                this.NotifyOfPropertyChange(nameof(this.Name));
            }
        }

        public string Address
        {
            get => this.address ?? string.Empty;
            set
            {
                this.address = value;
                this.NotifyOfPropertyChange(nameof(this.Address));
            }
        }

        public string DataBaseName
        {
            get => this.dataBaseName ?? string.Empty;
            set
            {
                this.dataBaseName = value;
                this.NotifyOfPropertyChange(nameof(this.DataBaseName));
            }
        }

        public string ID
        {
            get => this.userID ?? string.Empty;
            set
            {
                this.userID = value;
                this.NotifyOfPropertyChange(nameof(this.ID));
            }
        }

        public string Password
        {
            get => this.password ?? string.Empty;
            set
            {
                this.password = value;
                this.NotifyOfPropertyChange(nameof(this.Password));
            }
        }

        public Color ThemeColor
        {
            get => this.themeColor;
            set
            {
                this.themeColor = value;
                this.NotifyOfPropertyChange(nameof(this.ThemeColor));
            }
        }

        public string Theme
        {
            get => this.theme ?? "Dark";
            set
            {
                this.theme = value;
                this.NotifyOfPropertyChange(nameof(this.Theme));
            }
        }

        public bool Equals(ConnectionItemViewModel dest)
        {
            if (this.Name != dest.Name)
                return false;
            if (this.Address != dest.Address)
                return false;
            if (this.DataBaseName != dest.DataBaseName)
                return false;
            if (this.ID != dest.ID)
                return false;
            if (this.themeColor != dest.themeColor)
                return false;
            if (this.theme != dest.theme)
                return false;
            return true;
        }

        public Guid PasswordID
        {
            get
            {
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream);
                using var md5 = MD5.Create();
                writer.Write(this.Password);
                writer.Close();

                var bytes = md5.ComputeHash(stream.GetBuffer());
                var sBuilder = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    sBuilder.Append(bytes[i].ToString("x2"));
                }
                return Guid.Parse(sBuilder.ToString());
            }
        }

        public bool IsDefault { get; set; }

        public DateTime LastConnectedDateTime { get; set; }

        public bool IsCurrentTheme
        {
            get => this.isCurrentTheme;
            private set
            {
                this.isCurrentTheme = value;
                this.NotifyOfPropertyChange(nameof(this.IsCurrentTheme));
            }
        }

        public bool IsTemporary
        {
            get => this.isTemporary;
            set
            {
                this.isTemporary = value;
                this.NotifyOfPropertyChange(nameof(this.IsTemporary));
            }
        }

        private void Current_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FirstFloor.ModernUI.Presentation.AppearanceManager.ThemeSource))
            {
                this.RefreshIsCurrentThemeProperty();
            }
        }

        private void RefreshIsCurrentThemeProperty()
        {
            foreach (var item in CremaAppHostViewModel.Themes)
            {
                if (item.Key == this.Theme && item.Key == this.cremaAppHost.Theme)
                {
                    this.IsCurrentTheme = true;
                    return;
                }
            }
            this.IsCurrentTheme = false;
        }
    }
}

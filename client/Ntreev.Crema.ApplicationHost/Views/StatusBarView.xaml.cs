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
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Ntreev.Library;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Ntreev.Crema.ApplicationHost.Views
{
    [Export]
    partial class StatusBarView : UserControl, INotifyPropertyChanged
    {
        private readonly Lazy<ICremaHost> cremaHost;
        private readonly Lazy<ICremaAppHost> cremaAppHost;

        public StatusBarView()
        {
            this.InitializeComponent();
        }

        [ImportingConstructor]
        public StatusBarView(Lazy<ICremaHost> cremaHost, Lazy<ICremaAppHost> cremaAppHost)
        {
            this.cremaHost = cremaHost;
            this.cremaAppHost = cremaAppHost;
            this.InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.CremaAppHost != null)
            {
                this.CremaAppHost.Loaded += CremaAppHost_Loaded;
                this.CremaAppHost.Unloaded += CremaAppHost_Unloaded;
            }

            if (this.CremaHost != null)
            {
                this.CremaHost.Opened += CremaHost_Opened;
                this.CremaHost.Closed += CremaHost_Closed;
            }

            this.dataBaseButton.Visibility = System.Windows.Visibility.Hidden;
        }

        public string DataBaseName
        {
            get
            {
                if (this.CremaAppHost == null)
                    return string.Empty;
                if (this.CremaAppHost.DataBaseName == string.Empty)
                    return Properties.Resources.Label_SelectDataBase;
                return this.CremaAppHost.DataBaseName;
            }
        }

        public ICremaHost CremaHost => this.cremaHost.Value;

        public ICremaAppHost CremaAppHost => this.cremaAppHost.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.dataBaseButton.Visibility = Visibility.Visible;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DataBaseName)));
            });
        }

        private async void CremaHost_Closed(object sender, ClosedEventArgs e)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.dataBaseButton.Visibility = Visibility.Hidden;
            });
        }

        private void CremaAppHost_Loaded(object sender, EventArgs e)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DataBaseName)));
        }

        private void CremaAppHost_Unloaded(object sender, EventArgs e)
        {

        }

        private void DataBaseButton_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();
            var items = this.CremaAppHost.DataBases.Select(item => item.Name).ToArray();
            var query = from connectionItem in this.CremaAppHost.ConnectionItems
                        where connectionItem.Address == this.CremaAppHost.ConnectionItem.Address &&
                              connectionItem.ID == this.CremaAppHost.ConnectionItem.ID &&
                              connectionItem.PasswordID == this.CremaAppHost.ConnectionItem.PasswordID
                        join item in items on connectionItem.DataBaseName equals item
                        select connectionItem;

            foreach (var item in items)
            {
                var connectionItem = query.FirstOrDefault(i => i.DataBaseName == item) ?? new DisabledConnectionItem() { DataBaseName = item, };
                var menuItem = new MenuItem()
                {
                    Header = connectionItem,
                    IsChecked = this.CremaAppHost.DataBaseName == connectionItem.DataBaseName,
                    IsEnabled = this.CremaAppHost.DataBaseName != connectionItem.DataBaseName && connectionItem is DisabledConnectionItem == false,
                };
                menuItem.HeaderTemplate = this.FindResource("DataBase_MenuItem_HeaderTemplate") as DataTemplate;
                menuItem.Click += DataBaseMenuItem_Click;
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.Placement = PlacementMode.Top;
            contextMenu.PlacementTarget = this.dataBaseButton;
            contextMenu.IsOpen = true;
        }

        private async void DataBaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem.Header is IConnectionItem connectionItem)
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaAppHost.SelectDataBase(connectionItem.DataBaseName);
                });
            }
        }

        #region DisabledConnectionItem

        class DisabledConnectionItem : IConnectionItem
        {
            public string Name => throw new NotImplementedException();

            public string Address => throw new NotImplementedException();

            public string DataBaseName { get; set; }

            public string ID => throw new NotImplementedException();

            public Color ThemeColor => Colors.Transparent;

            public string Theme => throw new NotImplementedException();

            public Guid PasswordID => Guid.Empty;
        }

        #endregion
    }
}

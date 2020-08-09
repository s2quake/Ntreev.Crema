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
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.DataGrid.Controls;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ntreev.Crema.Presentation.Home.Services.Views
{
    /// <summary>
    /// DataBaseSelectionView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DataBaseListView : UserControl, IPartImportsSatisfiedNotification
    {
        [Import]
        private readonly ICremaAppHost cremaAppHost = null;
        [Import]
        private readonly IAppConfiguration configs = null;
        [Import]
        private readonly IPropertyService propertyService = null;

        public DataBaseListView()
        {
            InitializeComponent();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if (this.IsKeyboardFocusWithin == true && this.gridControl.SelectedItem != null && this.propertyService.SelectedObject != this.gridControl.SelectedItem)
            {
                this.Dispatcher.InvokeAsync(() => this.propertyService.SelectedObject = this.gridControl.SelectedItem);
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ModernDataRow dataRow && dataRow.DataContext is ICommand command)
            {
                if (command.CanExecute(dataRow.DataContext) == true)
                {
                    command.Execute(dataRow.DataContext);
                }
            }
        }

        private void CremaAppHost_Opened(object sender, EventArgs e)
        {
            this.configs.Update(this);
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.configs.Commit(this);
        }

        #region IPartImportsSatisfiedNotification

        async void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.cremaAppHost.Opened += CremaAppHost_Opened;
                this.cremaAppHost.Closed += CremaAppHost_Closed;
            });
        }

        #endregion
    }
}

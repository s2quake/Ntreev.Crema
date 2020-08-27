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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace JSSoft.Crema.Presentation.Home.Services.Views
{
    /// <summary>
    /// ConnectionListView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConnectionListView : UserControl
    {
        //public static readonly DependencyProperty ItemsSourceProperty =
        //    DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ConnectionListView));

        //public static readonly DependencyProperty SelectedItemProperty =
        //    DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(ConnectionListView));

        public ConnectionListView()
        {
            InitializeComponent();

            //BindingOperations.SetBinding(this.ListBox, ItemsControl.ItemsSourceProperty, new Binding(nameof(ItemsSource)) { Source = this });
            //BindingOperations.SetBinding(this.ListBox, Selector.SelectedItemProperty, new Binding(nameof(SelectedItem)) { Source = this, Mode = BindingMode.TwoWay });
        }

        //public IEnumerable ItemsSource
        //{
        //    get => (IEnumerable)this.GetValue(ItemsSourceProperty);
        //    set => this.SetValue(ItemsSourceProperty, value);
        //}

        //public object SelectedItem
        //{
        //    get => (object)this.GetValue(SelectedItemProperty);
        //    set => this.SetValue(SelectedItemProperty, value);
        //}

        private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is ICommand command)
            {
                if (command.CanExecute(fe.DataContext) == true)
                {
                    command.Execute(fe.DataContext);
                    e.Handled = true;
                }
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                if (this.ListBox.SelectedItem != null)
                {
                    if (this.ListBox.ItemContainerGenerator.ContainerFromItem(this.ListBox.SelectedItem) is ListBoxItem container)
                    {
                        container.BringIntoView();
                    }
                }
            }, DispatcherPriority.Background);
        }
    }
}

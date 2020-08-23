using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ntreev.Crema.Presentation.Home.Services.Views
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

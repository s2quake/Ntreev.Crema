using Ntreev.ModernUI.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ntreev.Crema.ApplicationHost.Dialogs.Views
{
    /// <summary>
    /// LoginView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginView : UserControl
    {
        private static DependencyProperty PasswordProperty = DependencyProperty.Register(nameof(Password), typeof(SecureString), typeof(LoginView));

        private BindingExpressionBase passwordBinding;

        public LoginView()
        {
            InitializeComponent();

            this.passwordBinding = BindingOperations.SetBinding(this, PasswordProperty, new Binding(nameof(Password))
            {
                Mode = BindingMode.OneWayToSource,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
            });
        }

        private SecureString Password
        {
            get { return (SecureString)this.GetValue(PasswordProperty); }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.UpdatePassword();
        }

        private async void UpdatePassword()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.SetValue(PasswordProperty, this.passwordBox.SecurePassword);
                this.passwordBinding.UpdateSource();
            });
        }
    }
}

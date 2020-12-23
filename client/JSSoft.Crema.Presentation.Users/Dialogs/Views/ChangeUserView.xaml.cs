﻿// Released under the MIT License.
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

using JSSoft.ModernUI.Framework.Controls;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JSSoft.Crema.Presentation.Users.Dialogs.Views
{
    partial class ChangeUserView : UserControl
    {
        private static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(nameof(Password), typeof(SecureString), typeof(ChangeUserView));
        private readonly BindingExpressionBase passwordBinding;

        public ChangeUserView()
        {
            this.InitializeComponent();
            this.passwordBinding = BindingOperations.SetBinding(this, PasswordProperty, new Binding(nameof(Password))
            {
                Mode = BindingMode.OneWayToSource,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
            });
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.passwordBox1.Focus();
        }

        private SecureString Password => (SecureString)this.GetValue(PasswordProperty);

        private void PasswordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.UpdatePassword();
        }

        private void PasswordBox2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.UpdatePassword();
        }

        private async void UpdatePassword()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var isValid = PasswordBoxUtility.GetIsValid(this.passwordBox1);
                if (isValid == true)
                    this.SetValue(PasswordProperty, this.passwordBox1.SecurePassword);
                else
                    this.SetValue(PasswordProperty, null);
                this.passwordBinding.UpdateSource();
            });
        }
    }
}
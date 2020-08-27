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
using JSSoft.Crema.Presentation.Home.Services.ViewModels;
using JSSoft.ModernUI.Framework.Controls;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Home.Services.Views
{
    [Export]
    partial class CremaAppHostView : UserControl
    {
        private readonly ICremaAppHost cremaAppHost;

        public CremaAppHostView()
        {
            this.InitializeComponent();
        }

        [ImportingConstructor]
        public CremaAppHostView(ICremaAppHost cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.InitializeComponent();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (this.filterBox.IsKeyboardFocusWithin == true)
            {
                if (Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (e.Key == Key.Escape)
                    {
                        this.filterBox.Text = string.Empty;
                        this.serverList.Focus();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Enter)
                    {
                        if (this.filterBox.GetBindingExpression(FilterBox.TextProperty) is BindingExpression be)
                        {
                            be.UpdateSource();
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (this.cremaAppHost is CremaAppHostViewModel viewModel)
            {
                viewModel.SetPassword(passwordBox.Password, false);
            }
        }
    }
}

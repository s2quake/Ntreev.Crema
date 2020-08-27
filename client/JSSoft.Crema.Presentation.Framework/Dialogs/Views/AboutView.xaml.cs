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
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Framework.Dialogs.Views
{
    /// <summary>
    /// AboutView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AboutView : Window
    {
        public AboutView()
        {
            InitializeComponent();

            this.Loaded += AboutView_Loaded;
            this.Unloaded += AboutView_Unloaded;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
                this.Close();
        }

        private void AboutView_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.Capture(this, CaptureMode.SubTree);
            Mouse.AddMouseDownHandler(this, this.MouseButtonEventHandler);
        }

        private void AboutView_Unloaded(object sender, RoutedEventArgs e)
        {
            Mouse.RemoveMouseDownHandler(this, this.MouseButtonEventHandler);
        }

        private void MouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            this.Close();
        }
    }
}

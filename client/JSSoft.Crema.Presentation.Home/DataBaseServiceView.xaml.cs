﻿//Released under the MIT License.
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

using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace JSSoft.Crema.Presentation.Home
{
    /// <summary>
    /// DataBaseServiceView.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export(typeof(DataBaseServiceView))]
    public partial class DataBaseServiceView : UserControl
    {
        private readonly IAppConfiguration configs;

        public DataBaseServiceView()
        {
            this.InitializeComponent();
        }

        [ImportingConstructor]
        public DataBaseServiceView(IAppConfiguration configs)
        {
            this.configs = configs;
            this.InitializeComponent();
        }

        private void Expander_Loaded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander.DataContext == null)
                return;

            if (this.configs.TryGetValue<bool>(this.GetType(), expander.DataContext.GetType(), nameof(expander.IsExpanded), out var isExpanded) == true)
            {
                expander.IsExpanded = isExpanded;
            }
        }

        private void Expander_Unloaded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if (expander.DataContext == null)
                return;

            this.configs.SetValue(this.GetType(), expander.DataContext.GetType(), nameof(expander.IsExpanded), expander.IsExpanded);
        }
    }
}

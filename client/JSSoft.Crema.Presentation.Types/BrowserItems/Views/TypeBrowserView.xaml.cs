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

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Types.BrowserItems.Views
{
    /// <summary>
    /// Interaction logic for TypeBrowserView.xaml
    /// </summary>
    [Export]
    public partial class TypeBrowserView : UserControl
    {
        private readonly IPropertyService propertyService;

        public TypeBrowserView()
        {
            InitializeComponent();
        }

        [ImportingConstructor]
        public TypeBrowserView(IPropertyService propertyService)
        {
            this.propertyService = propertyService;
            this.InitializeComponent();
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if (this.IsKeyboardFocusWithin == true && this.treeView.SelectedItem != null && this.propertyService.SelectedObject != this.treeView.SelectedItem)
            {
                this.Dispatcher.InvokeAsync(() => this.propertyService.SelectedObject = this.treeView.SelectedItem);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.Key)
            {
                case Key.Down:
                    {
                        if (this.FilterBox.IsKeyboardFocusWithin == false || this.treeView.IsFocused == true)
                            break;

                        if (this.treeView.SelectedItem == null)
                        {
                            if (this.treeView.ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem item)
                            {
                                item.IsSelected = true;
                            }
                        }

                        this.treeView.Focus();
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    {
                        if (this.treeView.ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem item &&
                            item.IsSelected == true && item.IsFocused == true)
                        {
                            this.FilterBox.Focus();
                        }
                    }
                    break;
                case Key.Escape:
                    {
                        if (Keyboard.Modifiers == ModifierKeys.None && this.FilterBox.Text != string.Empty)
                        {
                            this.FilterBox.Text = string.Empty;
                            e.Handled = true;
                        }
                    }
                    break;
            }
        }
    }
}

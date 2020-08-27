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
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Tables.Documents.Views
{
    /// <summary>
    /// Interaction logic for TableViewerView.xaml
    /// </summary>
    public partial class TableDataFinderView : UserControl
    {
        [Import]
        private readonly IAppConfiguration configs = null;
        [Import]
        private readonly IStatusBarService statusBarService = null;
        private IInputElement focuedElement;
        private ILineInfo lineInfo;
        private TextBox editableTextBox;

        public TableDataFinderView()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.gridControl != null)
            {
                this.lineInfo = new GridControlLineInfo(this.gridControl);
                this.gridControl.CurrentChanged += (s, e) => this.RefreshLineInfo();
            }

            this.FindingText.ApplyTemplate();
            this.editableTextBox = this.FindingText.Template.FindName("PART_EditableTextBox", this.FindingText) as TextBox;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Down)
                {
                    if (this.editableTextBox != null &&
                        this.editableTextBox.IsKeyboardFocused == true &&
                        this.gridControl.Focusable == true)
                    {
                        this.gridControl.Focus();
                        e.Handled = true;
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Enter)
                {
                    if (this.Find.IsEnabled == true)
                    {
                        this.Find.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                }
            }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            this.focuedElement = e.NewFocus;
            this.RefreshLineInfo();
        }

        private void PART_EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var comboBox = textBox.TemplatedParent as ComboBox;
            comboBox.Text = textBox.Text;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dispatcher.InvokeAsync((this.focuedElement ?? this.FindingText).Focus);

            this.configs.Update(this);
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null)
            {
                this.configs.Commit(this);
            }
        }

        private void RefreshLineInfo()
        {
            if (this.gridControl.CurrentItem != null)
            {
                if (this.statusBarService.LineInfo != this.lineInfo)
                    this.statusBarService.LineInfo = this.lineInfo;
            }
            else
            {
                this.statusBarService.LineInfo = null;
            }
        }

        private void GridControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up && Keyboard.Modifiers == ModifierKeys.None &&
                this.gridControl.Items.IndexOf(this.gridControl.CurrentItem) == 0)
            {
                this.FindingText.Focus();
                e.Handled = true;
            }
        }
    }
}

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

using JSSoft.Crema.Data.Diff;
using JSSoft.ModernUI.Framework.DataGrid.Controls;
using System.Windows;
using Xceed.Wpf.DataGrid;

namespace JSSoft.Crema.Presentation.Controls
{
    class DiffColumnManagerCell : ModernColumnManagerCell
    {
        private static readonly DependencyPropertyKey DiffStatePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DiffState), typeof(DiffState), typeof(DiffColumnManagerCell),
                new FrameworkPropertyMetadata(DiffState.Unchanged));
        public static readonly DependencyProperty DiffStateProperty = DiffStatePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsDummyPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsDummy), typeof(bool), typeof(DiffColumnManagerCell),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDummyProperty = IsDummyPropertyKey.DependencyProperty;

        public DiffColumnManagerCell()
        {

        }

        public DiffState DiffState => (DiffState)this.GetValue(DiffStateProperty);

        public bool IsDummy => (bool)this.GetValue(IsDummyProperty);

        protected override void PrepareContainer(DataGridContext dataGridContext, object item)
        {
            base.PrepareContainer(dataGridContext, item);
            var columnItem = CremaDataTableItemControl.GetReference(this.ParentColumn);
            var diffState = columnItem != null ? DiffUtility.GetDiffState(columnItem) : DiffState.Unchanged;
            this.SetValue(DiffStatePropertyKey, diffState);
            this.SetValue(IsDummyPropertyKey, this.FieldName.StartsWith(DiffUtility.DiffDummyKey));
        }
    }
}

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

using JSSoft.Crema.Data;
using JSSoft.Crema.Presentation.Differences.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Framework;
using System.ComponentModel;
using System.Linq;

namespace JSSoft.Crema.Presentation.Differences.PropertyItems.ViewModels
{
    abstract class TemplateColumnInfoViewModel : PropertyItemBase
    {
        private TemplateTreeViewItemViewModel viewModel;
        private TableInfo? tableInfo;
        private TemplateColumnInfoItemViewModel[] columns = new TemplateColumnInfoItemViewModel[] { };

        public TemplateColumnInfoViewModel()
        {
            this.DisplayName = Resources.Title_ColumnInfo;
        }

        public TemplateColumnInfoItemViewModel[] Columns => this.columns;

        public override bool IsVisible => this.columns.Any();

        public override object SelectedObject => this.viewModel;

        public override bool CanSupport(object obj)
        {
            return obj is TemplateTreeViewItemViewModel;
        }

        public override void SelectObject(object obj)
        {
            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            this.viewModel = obj as TemplateTreeViewItemViewModel;
            if (this.viewModel != null)
            {
                this.DisplayName = $"{Resources.Title_ColumnInfo}({this.GetHeader(this.viewModel)})";
                this.tableInfo = this.GetTableInfo(this.viewModel);
                this.viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
            else
            {
                this.tableInfo = null;
            }

            if (this.tableInfo != null)
                this.columns = this.tableInfo.Value.Columns.Select(item => new TemplateColumnInfoItemViewModel(item)).ToArray();
            else
                this.columns = new TemplateColumnInfoItemViewModel[] { };

            this.NotifyOfPropertyChange(nameof(this.Columns));
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        public bool IsLeft
        {
            get; set;
        }

        protected abstract TableInfo? GetTableInfo(TemplateTreeViewItemViewModel item);

        protected abstract string GetHeader(TemplateTreeViewItemViewModel item);

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TemplateTreeViewItemViewModel.IsResolved) || e.PropertyName == string.Empty)
            {
                this.SelectObject(this.viewModel);
            }
        }
    }
}

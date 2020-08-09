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

using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Diff;
using Ntreev.Crema.Presentation.Differences.Documents.ViewModels;
using Ntreev.ModernUI.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Ntreev.Crema.Presentation.Differences.BrowserItems.ViewModels
{
    class TableTreeViewItemViewModel : DifferenceTreeViewItemViewModel
    {
        private readonly DocumentServiceViewModel documentService = null;
        private string header1;
        private string header2;

        public TableTreeViewItemViewModel(BrowserViewModel browser, DocumentServiceViewModel documentService, DiffDataTable diffTable)
            : base(browser)
        {
            this.documentService = documentService;
            this.Source = diffTable;
            this.Source.PropertyChanged += DiffType_PropertyChanged;
            this.Source.SourceItem1.PropertyChanged += SourceItem1_PropertyChanged;
            this.Source.SourceItem2.PropertyChanged += SourceItem2_PropertyChanged;
            this.header1 = diffTable.Header1;
            this.header2 = diffTable.Header2;
            this.ViewCommand = new DelegateCommand(this.View);
            this.IsActivated = diffTable.DiffState != DiffState.Unchanged;
            this.Target = diffTable;

            foreach (var item in diffTable.Childs)
            {
                var viewModel = new TableTreeViewItemViewModel(browser, documentService, diffTable: item);
                viewModel.PropertyChanged += ChildViewModel_PropertyChanged;
                this.Items.Add(viewModel);
                if (this.IsActivated == false && viewModel.DiffState != DiffState.Unchanged)
                {
                    this.IsActivated = true;
                }
            }
            this.Dispatcher.InvokeAsync(() =>
            {
                if (this.DiffState != DiffState.Unchanged && this.Parent != null)
                {
                    this.Parent.IsExpanded = true;
                }
            });
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        public void View()
        {
            this.documentService.View(this);
        }

        public override string DisplayName
        {
            get
            {
                if (this.Source.ItemName1 == this.Source.ItemName2)
                    return this.Source.ItemName1;
                return $"{this.Source.ItemName1} => {this.Source.ItemName2}";
            }
        }

        public DiffState DiffState => this.Source.DiffState;

        public bool IsResolved => this.Source.IsResolved;

        public bool IsActivated { get; }

        public DiffDataTable Source { get; }

        public CremaDataTable Source1 => this.Source.SourceItem1;

        public CremaDataTable Source2 => this.Source.SourceItem2;

        public IEnumerable<object> UnresolvedItems => this.Source.UnresolvedItems;

        public string Header1
        {
            get => this.header1 ?? string.Empty;
            set
            {
                this.header1 = value;
                this.NotifyOfPropertyChange(() => this.Header1);
            }
        }

        public string Header2
        {
            get => this.header2;
            set
            {
                this.header2 = value;
                this.NotifyOfPropertyChange(() => this.Header2);
            }
        }

        public ICommand ViewCommand { get; }

        public bool IsInherited => false;

        public bool IsBaseTemplate => false;

        private void DiffType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Source.DiffState) || e.PropertyName == string.Empty)
            {
                this.NotifyOfPropertyChange(nameof(this.DiffState));
            }

            if (e.PropertyName == nameof(this.Source.IsResolved))
            {
                this.NotifyOfPropertyChange(nameof(this.IsResolved));
            }
        }

        private void SourceItem1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaDataTable.TableName))
            {
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }

        private void SourceItem2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaDataTable.TableName))
            {
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }

        private void ChildViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Source.DiffState) || e.PropertyName == string.Empty)
            {
                this.NotifyOfPropertyChange(nameof(this.DiffState));
            }
        }
    }
}

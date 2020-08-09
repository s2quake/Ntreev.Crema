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
    class TypeTreeViewItemViewModel : DifferenceTreeViewItemViewModel
    {
        private readonly DocumentServiceViewModel documentService;
        private string header1;
        private string header2;

        public TypeTreeViewItemViewModel(BrowserViewModel browser, DocumentServiceViewModel documentService, DiffDataType diffType)
            : base(browser)
        {
            this.documentService = documentService;
            this.Source = diffType;
            this.Source.PropertyChanged += DiffType_PropertyChanged;
            this.Source.SourceItem1.PropertyChanged += DataType1_PropertyChanged;
            this.Source.SourceItem2.PropertyChanged += DataType2_PropertyChanged;
            this.header1 = diffType.Header1;
            this.header2 = diffType.Header2;
            this.ViewCommand = new DelegateCommand(this.View);
            this.IsActivated = diffType.DiffState != DiffState.Unchanged;
            this.Target = diffType;
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
                if (this.Source.SourceItem1.TypeName == Source.SourceItem2.TypeName)
                    return this.Source.SourceItem1.TypeName;
                return $"{this.Source.SourceItem1.TypeName} => {this.Source.SourceItem2.TypeName}";
            }
        }

        public DiffState DiffState => this.Source.DiffState;

        public bool IsResolved => this.Source.IsResolved;

        public bool IsActivated { get; }

        public DiffDataType Source { get; }

        public CremaDataType Source1 => this.Source.SourceItem1;

        public CremaDataType Source2 => this.Source.SourceItem2;

        public IEnumerable<object> UnresolvedItems => new object[] { };

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

        public bool IsFlag => this.Source2.IsFlag;

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

        private void DataType1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaDataType.TypeName))
            {
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }

        private void DataType2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaDataType.TypeName))
            {
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }
    }
}

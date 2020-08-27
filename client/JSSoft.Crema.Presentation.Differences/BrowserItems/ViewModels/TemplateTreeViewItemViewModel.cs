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

using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Diff;
using JSSoft.Crema.Presentation.Differences.Documents.ViewModels;
using JSSoft.ModernUI.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Differences.BrowserItems.ViewModels
{
    class TemplateTreeViewItemViewModel : DifferenceTreeViewItemViewModel
    {
        private readonly DocumentServiceViewModel documentService = null;
        private string header1;
        private string header2;

        public TemplateTreeViewItemViewModel(BrowserViewModel browser, DocumentServiceViewModel documentService, DiffTemplate diffTemplate)
            : base(browser)
        {
            this.documentService = documentService;
            this.Source = diffTemplate;
            this.Source.PropertyChanged += DiffTemplate_PropertyChanged;
            this.Source.SourceItem1.PropertyChanged += Template1_PropertyChanged;
            this.Source.SourceItem2.PropertyChanged += Template2_PropertyChanged;
            this.header1 = diffTemplate.Header1;
            this.header2 = diffTemplate.Header2;
            this.ViewCommand = new DelegateCommand(this.View);
            this.IsActivated = diffTemplate.DiffState != DiffState.Unchanged;
            this.Target = diffTemplate;

            foreach (var item in this.Source.DiffTable.Childs)
            {
                this.Items.Add(new TemplateTreeViewItemViewModel(browser, documentService, diffTemplate: item.Template));
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
                if (this.Source.ItemName1 == Source.ItemName2)
                    return this.Source.ItemName1;
                return $"{this.Source.ItemName1} => {this.Source.ItemName2}";
            }
        }

        public DiffState DiffState => this.Source.DiffState;

        public bool IsResolved => this.Source.IsResolved;

        public bool IsActivated { get; }

        public DiffTemplate Source { get; }

        public CremaTemplate Source1 => this.Source.SourceItem1;

        public CremaTemplate Source2 => this.Source.SourceItem2;

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

        private void DiffTemplate_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Source.DiffState) || e.PropertyName == string.Empty)
            {
                this.NotifyOfPropertyChange(nameof(this.DiffState));
            }

            if (e.PropertyName == nameof(this.Source.IsResolved))
            {
                this.NotifyOfPropertyChange(nameof(this.IsResolved));
            }

            //if (e.PropertyName == nameof(this.diffTemplate.Name) || e.PropertyName == string.Empty)
            //{
            //    this.NotifyOfPropertyChange(nameof(this.DisplayName));
            //}
        }

        private void Template1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaTemplate.TableName))
            {
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }

        private void Template2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaTemplate.TableName))
            {
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }
    }
}

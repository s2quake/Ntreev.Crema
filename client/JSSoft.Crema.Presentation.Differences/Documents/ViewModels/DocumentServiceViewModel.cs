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

using JSSoft.Crema.Presentation.Differences.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;

namespace JSSoft.Crema.Presentation.Differences.Documents.ViewModels
{
    [Export(typeof(IDifferenceDocumentService))]
    [InheritedExport(typeof(DocumentServiceViewModel))]
    class DocumentServiceViewModel : DocumentServiceBase<IDocument>, IDifferenceDocumentService
    {
        private readonly ICremaAppHost cremaAppHost;

        [ImportingConstructor]
        public DocumentServiceViewModel(ICremaAppHost cremaAppHost)
        {
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Closed += CremaAppHost_Closed;
            this.DisplayName = Resources.Title_Differences;
        }

        public void View(TableTreeViewItemViewModel viewModel)
        {
            var targetModel = viewModel.Parent is TableTreeViewItemViewModel parent ? parent : viewModel;
            var document = this.Items.OfType<TableDocumentViewModel>().FirstOrDefault(item => item.Source == targetModel.Source);
            if (document == null)
            {
                document = new TableDocumentViewModel(targetModel);
                this.Items.Add(document);
            }
            document.SelectedName = $"{viewModel}";
            var cancellation = new CancellationTokenSource();
            this.ActivateItemAsync(document, cancellation.Token);
        }

        public void View(TemplateTreeViewItemViewModel viewModel)
        {
            var targetModel = viewModel.Parent is TemplateTreeViewItemViewModel parent ? parent : viewModel;
            var document = this.Items.OfType<TemplateDocumentViewModel>().FirstOrDefault(item => item.Source == targetModel.Source);
            if (document == null)
            {
                document = new TemplateDocumentViewModel(targetModel);
                this.Items.Add(document);
            }
            document.SelectedName = $"{viewModel}";
            var cancellation = new CancellationTokenSource();
            this.ActivateItemAsync(document, cancellation.Token);
        }

        public void View(TypeTreeViewItemViewModel viewModel)
        {
            var document = this.Items.OfType<TypeDocumentViewModel>().FirstOrDefault(item => item.Source == viewModel.Source);
            if (document == null)
            {
                document = new TypeDocumentViewModel(viewModel);
                this.Items.Add(document);
            }
            var cancellation = new CancellationTokenSource();
            this.ActivateItemAsync(document, cancellation.Token);
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);
        }

        private void CremaAppHost_Closed(object sender, EventArgs e)
        {
            this.Items.Clear();
        }
    }
}

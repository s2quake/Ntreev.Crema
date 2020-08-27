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

using JSSoft.Crema.Data.Diff;
using JSSoft.Crema.Presentation.Differences.BrowserItems.ViewModels;
using JSSoft.ModernUI.Framework;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Differences.Documents.ViewModels
{
    class TypeDocumentViewModel : DifferenceDocumentBase
    {
        private readonly TypeTreeViewItemViewModel viewModel;
        private readonly UndoService undoService = new UndoService();

        public TypeDocumentViewModel(TypeTreeViewItemViewModel viewModel)
            : base(viewModel)
        {
            this.viewModel = viewModel;
            this.undoService.Changed += UndoService_Changed;
            this.ResolveCommand = new DelegateCommand(async (p) => await this.ResolveAsync(), (p) => this.CanResolve);
            this.DisplayName = viewModel.DisplayName;
        }

        public override string ToString()
        {
            return this.viewModel.ToString();
        }

        public async Task ResolveAsync()
        {
            try
            {
                this.Source.Resolve();
                this.undoService.Clear();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public bool IsResolved => this.viewModel.IsResolved;

        public DiffDataType Source => this.viewModel.Source;

        public string Header1 => this.viewModel.Header1;

        public string Header2 => this.viewModel.Header2;

        public ICommand ResolveCommand { get; }

        public IUndoService UndoService => this.undoService;

        public bool CanResolve => this.Source.IsResolved == false;

        protected override async Task<bool> CloseAsync()
        {
            return await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.IsModified == false)
                    this.Source.RejectChanges();
                else
                    this.Source.AcceptChanges();
                return true;
            });
        }

        private void UndoService_Changed(object sender, EventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                if (this.IsResolved == true)
                    this.IsModified = false;
                else
                    this.IsModified = this.Source.HasChanges();
            });
        }
    }
}

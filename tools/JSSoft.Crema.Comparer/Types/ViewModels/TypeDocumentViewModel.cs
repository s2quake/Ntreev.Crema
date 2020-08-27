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
using JSSoft.Crema.Data.Diff;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Comparer.Types.ViewModels
{
    class TypeDocumentViewModel : DocumentBase
    {
        private readonly TypeTreeViewItemViewModel viewModel;
        private UndoService undoService = new UndoService();
        private ICommand resolveCommand;

        public TypeDocumentViewModel(TypeTreeViewItemViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.undoService.Changed += UndoService_Changed;
            this.resolveCommand = new DelegateCommand(async (p) => await this.ResolveAsync(), (p) => this.CanResolve);
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

        public bool IsResolved
        {
            get { return this.viewModel.IsResolved; }
        }

        public DiffDataType Source
        {
            get { return this.viewModel.Source; }
        }

        public string Header1
        {
            get { return this.viewModel.Header1; }
        }

        public string Header2
        {
            get { return this.viewModel.Header2; }
        }

        public ICommand ResolveCommand => this.resolveCommand;

        public IUndoService UndoService
        {
            get { return this.undoService; }
        }

        public bool CanResolve
        {
            get { return this.Source.IsResolved == false; }
        }

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

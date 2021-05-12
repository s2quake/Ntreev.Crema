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

using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.RuntimeService;
using JSSoft.Crema.Tools.Framework;
using JSSoft.Crema.Tools.View.Dialogs.ViewModels;
using JSSoft.ModernUI.Framework;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Tools.View.ViewModels
{
    [Export(typeof(IContent))]
    class DataViewModel : ContentBase
    {
        [Import]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:읽기 전용 한정자 추가", Justification = "<보류 중>")]
        private IRuntimeService service = null;
        [Import]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:읽기 전용 한정자 추가", Justification = "<보류 중>")]
        private IAppConfiguration configs = null;
        [ImportMany]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:읽기 전용 한정자 추가", Justification = "<보류 중>")]
        private IEnumerable<IDataSerializer> serializers = null;
        [Import]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:읽기 전용 한정자 추가", Justification = "<보류 중>")]
        private Lazy<IContentService> contentService = null;

        [ImportingConstructor]
        public DataViewModel()
        {
            this.DisplayName = "New View...";
            this.GroupName = "View";
            this.LoadCommand = new DelegateCommand(async (p) => await this.LoadAsync(p as string));
        }

        public async Task LoadAsync()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("crema data", "*.dat"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await this.LoadAsync(dialog.FileName);
            }
        }

        public async Task LoadAsync(string filename)
        {
            var contentService = this.contentService.Value;
            var viewModel = new FileViewModel(this);
            contentService.Contents.Add(viewModel);
            contentService.SelectedContent = viewModel;
            viewModel.Disposed += ViewModel_Disposed;
            await viewModel.OpenAsync(filename);
        }

        public async Task ConnectAsync()
        {
            var dialog = new ConnectionViewModel();
            this.configs.Update(dialog);

            if (await dialog.ShowDialogAsync() == true)
            {
                var contentService = this.contentService.Value;
                var viewModel = new RemoteViewModel(this, this.service, this.serializers.First(item => item.Name == "bin"));
                
                contentService.Contents.Add(viewModel);
                contentService.SelectedContent = viewModel;
                viewModel.Disposed += ViewModel_Disposed;
                await viewModel.ConnectAsync(dialog.Address, dialog.DataBase, dialog.Tags, dialog.FilterExpression);
                this.configs.Commit(dialog);
            }
        }

        public ICommand LoadCommand { get; private set; }

        private void ViewModel_Disposed(object sender, EventArgs e)
        {
            var contentService = this.contentService.Value;
            contentService.Contents.Remove(sender);
            contentService.SelectedContent = this;
        }
    }
}

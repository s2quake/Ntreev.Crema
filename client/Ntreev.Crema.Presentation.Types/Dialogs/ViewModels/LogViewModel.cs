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

using Ntreev.Crema.Presentation.Types.Properties;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ntreev.Crema.Presentation.Types.Dialogs.ViewModels
{
    public class LogViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly ITypeItem typeItem;
        private LogInfoViewModel[] itemsSource;
        private LogInfoViewModel selectedItem;
        private readonly ICommand previewCommand;

        public LogViewModel(Authentication authentication, ITypeItem typeItem)
        {
            this.authentication = authentication;
            this.typeItem = typeItem;
            this.DisplayName = Resources.Title_ViewLog;
            this.previewCommand = new DelegateCommand(async (p) => await this.PreviewAsync(), (p) => this.CanPreview);
            this.Initialize();
        }

        public async Task CloseAsync()
        {
            await this.TryCloseAsync(true);
        }

        public async Task PreviewAsync()
        {
            await this.selectedItem.PreviewAsync();
        }

        public LogInfoViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.CanPreview));
            }
        }

        public bool CanPreview
        {
            get
            {
                if (this.IsProgressing == true)
                    return false;
                return this.selectedItem != null;
            }
        }

        public IEnumerable<LogInfoViewModel> Items => this.itemsSource;

        public ICommand PreviewCommand => this.previewCommand;

        private async void Initialize()
        {
            try
            {
                this.BeginProgress(Resources.Message_ReceivingInfo);
                var query = from item in await this.typeItem.GetLogAsync(this.authentication, null)
                            select new LogInfoViewModel(this.authentication, this.typeItem, item);
                this.itemsSource = query.ToArray();
                this.selectedItem = null;
                this.EndProgress();
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.Items));
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
                await this.TryCloseAsync();
            }
        }
    }
}
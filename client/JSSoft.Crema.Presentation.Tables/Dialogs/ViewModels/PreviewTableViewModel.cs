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
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Linq;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    class PreviewTableViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly ITable table;
        private readonly string revision;
        private CremaDataTable source;

        public PreviewTableViewModel(Authentication authentication, ITable table, string revision)
        {
            this.authentication = authentication;
            this.table = table;
            this.revision = revision;
            this.Initialize();
        }

        public CremaDataTable Source
        {
            get => this.source;
            private set
            {
                this.source = value;
                this.NotifyOfPropertyChange(nameof(this.Source));
            }
        }

        private async void Initialize()
        {
            try
            {
                this.DisplayName = await this.table.Dispatcher.InvokeAsync(() => $"{this.table.Name} - {revision}");
                this.BeginProgress(Resources.Message_ReceivingInfo);
                var dataSet = await this.table.GetDataSetAsync(this.authentication, this.revision);
                this.Source = dataSet.Tables.FirstOrDefault();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                await this.TryCloseAsync();
            }
            finally
            {
                this.EndProgress();
            }
        }
    }
}

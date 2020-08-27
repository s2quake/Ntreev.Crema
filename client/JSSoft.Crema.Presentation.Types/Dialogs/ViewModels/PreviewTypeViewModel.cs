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
using JSSoft.Crema.Presentation.Types.Properties;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Linq;

namespace JSSoft.Crema.Presentation.Types.Dialogs.ViewModels
{
    class PreviewTypeViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly IType type;
        private readonly string revision;
        private CremaDataType source;

        public PreviewTypeViewModel(Authentication authentication, IType type, string revision)
        {
            this.authentication = authentication;
            this.type = type;
            this.revision = revision;
            this.Initialize();
        }

        public CremaDataType Source
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
                this.DisplayName = await this.type.Dispatcher.InvokeAsync(() => $"{this.type.Name} - {revision}");
                this.BeginProgress(Resources.Message_ReceivingInfo);
                var dataSet = await this.type.GetDataSetAsync(this.authentication, this.revision);
                this.Source = dataSet.Types.FirstOrDefault();
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

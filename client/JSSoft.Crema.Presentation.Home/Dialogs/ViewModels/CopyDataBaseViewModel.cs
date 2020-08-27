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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.Services;
using JSSoft.Library.ObjectModel;
using JSSoft.ModernUI.Framework;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Home.Dialogs.ViewModels
{
    public class CopyDataBaseViewModel : ModalDialogAppBase
    {
        private readonly IDataBase dataBase;
        private readonly Authentication authentication;
        private string dataBaseName;
        private string comment;

        private CopyDataBaseViewModel(Authentication authentication, IDataBase dataBase)
            : base(dataBase)
        {
            this.authentication = authentication;
            this.dataBase = dataBase;
            this.dataBase.Dispatcher.VerifyAccess();
            this.SourceDataBaseName = this.dataBase.Name;
            this.DisplayName = Resources.Title_CopyDataBase;
        }

        public static Task<CopyDataBaseViewModel> CreateInstanceAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IDataBase dataBase)
            {
                return dataBase.Dispatcher.InvokeAsync(() =>
                {
                    return new CopyDataBaseViewModel(authentication, dataBase);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        public async Task CopyAsync()
        {
            await new ProgressAction(this)
            {
                BeginMessage = Resources.Message_CopingDataBase,
                Try = async () =>
                {
                    await this.dataBase.CopyAsync(this.authentication, this.DataBaseName, this.Comment, false);
                    await this.TryCloseAsync(true);
                    await AppMessageBox.ShowAsync(Resources.Message_CopiedDataBase);
                }
            }.RunAsync();
        }

        public string DataBaseName
        {
            get => this.dataBaseName ?? string.Empty;
            set
            {
                if (this.dataBaseName == value)
                    return;
                this.dataBaseName = value;
                this.NotifyOfPropertyChange(nameof(this.DataBaseName));
                this.NotifyOfPropertyChange(nameof(this.CanCopy));
            }
        }

        public string SourceDataBaseName { get; }

        public string Comment
        {
            get => this.comment ?? string.Empty;
            set
            {
                if (this.comment == value)
                    return;
                this.comment = value;
                this.NotifyOfPropertyChange(nameof(this.Comment));
                this.NotifyOfPropertyChange(nameof(this.CanCopy));
            }
        }

        public bool CanCopy
        {
            get
            {
                if (this.IsProgressing == true)
                    return false;
                if (this.Comment == string.Empty)
                    return false;
                if (this.DataBaseName == string.Empty)
                    return false;
                return NameValidator.VerifyName(this.DataBaseName);
            }
        }
    }
}

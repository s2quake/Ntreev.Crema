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
using JSSoft.Crema.Presentation.Home.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Home.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(DataBaseMenuItem))]
    class DeleteDataBaseMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;
        private readonly ICremaAppHost cremaAppHost;

        [ImportingConstructor]
        public DeleteDataBaseMenuItem(Authenticator authenticator, ICremaAppHost cremaAppHost)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.cremaAppHost.Opened += this.InvokeCanExecuteChangedEvent;
            this.cremaAppHost.Closed += this.InvokeCanExecuteChangedEvent;
            this.DisplayName = Resources.MenuItem_DeleteDataBase;
        }

        protected override bool OnCanExecute(object parameter)
        {
            return this.cremaAppHost.IsOpened && this.authenticator.Authority == Authority.Admin;
        }

        protected async override void OnExecute(object parameter)
        {
            var dialog = new SelectDataBaseViewModel(this.authenticator, this.cremaAppHost, (s) => DataBaseDescriptorUtility.IsLoaded(this.authenticator, s) == false);
            if (await dialog.ShowDialogAsync() != true)
                return;
            if (DataBaseDescriptorUtility.IsLoaded(this.authenticator, dialog.SelectedItem) == true)
            {
                await AppMessageBox.ShowAsync("현재 사용중인 데이터베이스는 삭제할 수 없습니다.");
                return;
            }
            if (await new DeleteViewModel().ShowDialogAsync() != true)
                return;
            await DataBaseUtility.DeleteAsync(this.authenticator, dialog.SelectedItem);
        }
    }
}

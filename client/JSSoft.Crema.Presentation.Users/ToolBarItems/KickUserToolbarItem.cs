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
using JSSoft.Crema.Presentation.Users.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.Users.Properties;
using JSSoft.ModernUI.Framework;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Users.ToolBarItems
{
    [Export(typeof(IToolBarItem))]
    [ParentType(typeof(UserBrowserViewModel))]
    class KickUserToolbarItem : ToolBarItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public KickUserToolbarItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.Icon = "Images/kick.png";
            this.DisplayName = Resources.MenuItem_Kick;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is UserBrowserViewModel browser && browser.SelectedItem is IUserDescriptor descriptor)
            {
                return UserUtility.CanKick(this.authenticator, descriptor);
            }
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is UserBrowserViewModel browser && browser.SelectedItem is IUserDescriptor descriptor)
            {
                await UserUtility.KickAsync(this.authenticator, descriptor);
            }
        }
    }
}

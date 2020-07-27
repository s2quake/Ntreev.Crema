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

using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.ApplicationHost.Dialogs.ViewModels
{
    class LoginViewModel : ModalDialogBase
    {
        private string userID;
        private SecureString password;

        public LoginViewModel()
        {
            this.DisplayName = "Login";
        }

        public Task LoginAsync()
        {
            return this.TryCloseAsync(true);
        }

        public string UserID
        {
            get => this.userID ?? string.Empty;
            set
            {
                this.userID = value;
                this.NotifyOfPropertyChange(nameof(this.UserID));
            }
        }

        public SecureString Password
        {
            get => this.password;
            set
            {
                this.password = value;
                this.NotifyOfPropertyChange(nameof(this.Password));
                this.NotifyOfPropertyChange(nameof(this.CanLogin));
            }
        }

        public bool CanLogin
        {
            get
            {
                if (this.UserID == string.Empty)
                    return false;
                if (this.Password == null)
                    return false;
                return this.IsProgressing == false;
            }
        }
    }
}

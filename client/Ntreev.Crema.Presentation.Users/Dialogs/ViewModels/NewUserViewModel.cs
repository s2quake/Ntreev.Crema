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

using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Users.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Users.Dialogs.ViewModels
{
    public class NewUserViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly IUserCategory category;
        private string userID;
        private SecureString password;
        private string userName;
        private Authority authority = Authority.Member;

        private NewUserViewModel(Authentication authentication, IUserCategory category)
        {
            this.authentication = authentication;
            this.authentication.Expired += Authentication_Expired;
            this.category = category;
            this.category.Dispatcher.VerifyAccess();
            this.DisplayName = Resources.Title_NewUser;
        }

        public static Task<NewUserViewModel> CreateInstanceAsync(Authentication authentication, IUserCategoryDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IUserCategory category)
            {
                return category.Dispatcher.InvokeAsync(() =>
                {
                    return new NewUserViewModel(authentication, category);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        public async Task CreateAsync()
        {
            try
            {
                this.BeginProgress(Resources.Message_NewUser);
                await this.category.AddNewUserAsync(this.authentication, this.ID, this.Password, this.UserName, this.Authority);
                this.EndProgress();
                await this.TryCloseAsync(true);
                await AppMessageBox.ShowAsync(Resources.Message_NewUserComplete);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public bool CanCreate
        {
            get
            {
                if (this.ID == string.Empty)
                    return false;
                if (IdentifierValidator.Verify(this.ID) == false)
                    return false;
                if (this.Password == null)
                    return false;
                if (this.UserName == string.Empty)
                    return false;
                return this.IsProgressing == false;
            }
        }

        public string ID
        {
            get { return this.userID ?? string.Empty; }
            set
            {
                this.userID = value;
                this.NotifyOfPropertyChange(nameof(this.ID));
                this.NotifyOfPropertyChange(nameof(this.CanCreate));
            }
        }

        public SecureString Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                this.NotifyOfPropertyChange(nameof(this.Password));
                this.NotifyOfPropertyChange(nameof(this.CanCreate));
            }
        }

        public string UserName
        {
            get { return this.userName ?? string.Empty; }
            set
            {
                this.userName = value;
                this.NotifyOfPropertyChange(nameof(this.UserName));
                this.NotifyOfPropertyChange(nameof(this.CanCreate));
            }
        }

        public Authority Authority
        {
            get { return this.authority; }
            set
            {
                this.authority = value;
                this.NotifyOfPropertyChange(nameof(this.Authority));
                this.NotifyOfPropertyChange(nameof(this.CanCreate));
            }
        }

        public IEnumerable<Authority> Authorities
        {
            get
            {
                return Enum.GetValues(typeof(Authority)).Cast<Authority>();
            }
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (close == true)
            {
                this.authentication.Expired -= Authentication_Expired;
            }
        }

        private async void Authentication_Expired(object sender, EventArgs e)
        {
            await this.TryCloseAsync();
        }
    }
}

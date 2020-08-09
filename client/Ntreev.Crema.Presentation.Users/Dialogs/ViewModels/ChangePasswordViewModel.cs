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
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Security;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Users.Dialogs.ViewModels
{
    public class ChangePasswordViewModel : ModalDialogAppBase
    {
        private readonly Authentication authentication;
        private readonly IUser user;
        private SecureString password;
        private SecureString newPassword;

        private ChangePasswordViewModel(Authentication authentication, IUser user)
        {
            this.authentication = authentication;
            this.user = user;
            this.user.Dispatcher.VerifyAccess();
            this.DisplayName = Resources.Title_ChangePassword;
        }

        public static Task<ChangePasswordViewModel> CreateInstanceAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IUser user)
            {
                return user.Dispatcher.InvokeAsync(() =>
                {
                    return new ChangePasswordViewModel(authentication, user);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        public static async Task<ChangePasswordViewModel> CreateInstanceAsync(Authentication authentication, IServiceProvider serviceProvider, string userID)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));

            if (serviceProvider.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                try
                {
                    var user = await userContext.Dispatcher.InvokeAsync(() => userContext.Users[userID]);
                    return await user.Dispatcher.InvokeAsync(() =>
                    {
                        return new ChangePasswordViewModel(authentication, user);
                    });
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                    return null;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public async Task ChangeAsync()
        {
            try
            {
                this.BeginProgress(Resources.Message_Change);
                await this.user.ChangeUserInfoAsync(this.authentication, this.Password, this.NewPassword, null, null);
                this.EndProgress();
                await this.TryCloseAsync(true);
                await AppMessageBox.ShowInfoAsync(Resources.Message_ChangeComplete);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public SecureString Password
        {
            get => this.password;
            set
            {
                this.password = value;

                this.NotifyOfPropertyChange(nameof(this.Password));
                this.NotifyOfPropertyChange(nameof(this.CanChange));
            }
        }

        public SecureString NewPassword
        {
            get => this.newPassword;
            set
            {
                this.newPassword = value;

                this.NotifyOfPropertyChange(nameof(this.NewPassword));
                this.NotifyOfPropertyChange(nameof(this.CanChange));
            }
        }

        public bool CanChange
        {
            get
            {
                if (this.Password == null || this.NewPassword == null)
                    return false;

                return this.IsProgressing == false;
            }
        }
    }
}

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
using JSSoft.Crema.Presentation.Users.Properties;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Users.Dialogs.ViewModels
{
    public class DeleteUserViewModel : DeleteAsyncViewModel
    {
        private readonly Authentication authentication;
        private readonly IUser user;

        private DeleteUserViewModel(Authentication authentication, IUser user)
        {
            this.authentication = authentication;
            this.authentication.Expired += Authentication_Expired;
            this.user = user;
            this.user.Dispatcher.VerifyAccess();
            this.DisplayName = Resources.Title_DeleteUser;
        }

        public static Task<DeleteUserViewModel> CreateInstanceAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IUser user)
            {
                return user.Dispatcher.InvokeAsync(() =>
                {
                    return new DeleteUserViewModel(authentication, user);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected override Task OnDeleteAsync()
        {
            return this.user.DeleteAsync(this.authentication);
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

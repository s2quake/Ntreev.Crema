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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Users.Properties;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Users.Dialogs.ViewModels
{
    public class KickEditorViewModel : CommentAsyncViewModel
    {
        private readonly Authentication authentication;
        private readonly IDomainUser domainUser;

        private KickEditorViewModel(Authentication authentication, IDomainUser domainUser)
        {
            this.authentication = authentication;
            this.authentication.Expired += Authentication_Expired;
            this.domainUser = domainUser;
            this.CommentHeader = Resources.Label_KickReason;
            this.DisplayName = Resources.Title_KickEditor;
        }

        public static Task<KickEditorViewModel> CreateInstanceAsync(Authentication authentication, IDomainUserDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IDomainUser domainUser)
            {
                return domainUser.Dispatcher.InvokeAsync(() =>
                {
                    return new KickEditorViewModel(authentication, domainUser);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected override Task ConfirmAsync(string comment)
        {
            return this.domainUser.KickAsync(this.authentication, comment);
        }

        protected override void Verify(string comment, Action<bool> isValid)
        {
            isValid(true);
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

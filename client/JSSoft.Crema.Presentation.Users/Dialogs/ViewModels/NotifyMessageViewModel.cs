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
using JSSoft.ModernUI.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Users.Dialogs.ViewModels
{
    public class NotifyMessageViewModel : ModalDialogAppBase
    {
        private readonly Authentication authentication;
        private readonly IUserContext userContext;
        private string message;

        private NotifyMessageViewModel(Authentication authentication, IUserContext userContext, string[] userIDs)
        {
            this.authentication = authentication;
            this.userContext = userContext;
            this.userContext.Dispatcher.VerifyAccess();
            this.TargetUserIDs = userIDs;
            this.DisplayName = Resources.Title_NotifyMessage;
        }

        public static Task<NotifyMessageViewModel> CreateInstanceAsync(Authentication authentication, IServiceProvider serviceProvider)
        {
            return CreateInstanceAsync(authentication, serviceProvider, null);
        }

        public static Task<NotifyMessageViewModel> CreateInstanceAsync(Authentication authentication, IServiceProvider serviceProvider, string[] userIDs)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (serviceProvider.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                return userContext.Dispatcher.InvokeAsync(() =>
                {
                    return new NotifyMessageViewModel(authentication, userContext, userIDs);
                });
            }
            else
            {
                throw new ArgumentException("ServiceProvider does not provide IUserContext", nameof(serviceProvider));
            }
        }

        public string Message
        {
            get => this.message ?? string.Empty;
            set
            {
                this.message = value;
                this.NotifyOfPropertyChange(nameof(this.Message));
                this.NotifyOfPropertyChange(nameof(this.CanNotify));
            }
        }

        public string[] TargetUserIDs { get; private set; }

        public string TargetUserID => this.TargetUserIDs.Any() == false ? "All Users" : string.Join(",", this.TargetUserIDs);

        public async Task NotifyAsync()
        {
            try
            {
                this.BeginProgress(Resources.Message_SendMessage);
                await this.userContext.NotifyMessageAsync(this.authentication, this.TargetUserIDs, this.Message);
                this.EndProgress();
                await this.TryCloseAsync(true);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public bool CanNotify
        {
            get
            {
                if (this.IsProgressing == true)
                    return false;
                if (this.Message == string.Empty)
                    return false;
                return true;
            }
        }
    }
}

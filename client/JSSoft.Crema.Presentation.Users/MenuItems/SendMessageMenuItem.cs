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
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Users.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(UserTreeItemBase))]
    [ParentType(typeof(DomainUserTreeItemBase))]
    [ParentType(typeof(DomainUserListItemBase))]
    [Order(int.MinValue)]
    [Category(nameof(CategoryAttribute.Default))]
    [DefaultMenu]
    class SendMessageMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public SendMessageMenuItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.Icon = "Images/message.png";
            this.DisplayName = Resources.MenuItem_SendMessage;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is IUserDescriptor userDescriptor)
            {
                return UserUtility.CanSendMessage(this.authenticator, userDescriptor);
            }
            else if (parameter is IDomainUserDescriptor domainUserDescriptor)
            {
                return DomainUserUtility.CanSendMessage(this.authenticator, domainUserDescriptor);
            }
            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            if (parameter is IUserDescriptor userDescriptor)
            {
                await UserUtility.SendMessageAsync(this.authenticator, userDescriptor);
            }
            else if (parameter is IDomainUserDescriptor domainUserDescriptor)
            {
                await DomainUserUtility.SendMessageAsync(this.authenticator, domainUserDescriptor);
            }
        }
    }
}

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
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.ModernUI.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Home.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [ParentType(typeof(PropertyService))]
    class DomainUsersViewModel : PropertyItemBase
    {
        private readonly Authenticator authenticator;
        private ReadOnlyObservableCollection<DomainUserListItemBase> users;
        private DomainUserListItemBase selectedUser;
        private IDomainDescriptor descriptor;

        [ImportingConstructor]
        public DomainUsersViewModel(Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.DisplayName = Resources.Title_DomainUserList;
        }

        public override bool CanSupport(object obj)
        {
            return obj is IDomainDescriptor;
        }

        public async override void SelectObject(object obj)
        {
            this.descriptor = obj as IDomainDescriptor;

            if (this.descriptor != null)
            {
                var domain = this.descriptor.Target;

                if (domain.ExtendedProperties.ContainsKey(this) == true)
                {
                    var listBase = domain.ExtendedProperties[this] as DomainUserListBase;
                    this.Users = listBase.Users;
                }
                else
                {
                    var listBase = await domain.Dispatcher.InvokeAsync(() => new DomainUserListBase(this.authenticator, domain, true, this));
                    this.Users = listBase.Users;
                }

                this.SelectedUser = null;
            }
            else
            {
                this.Users = null;
                this.SelectedUser = null;
            }
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        public override bool IsVisible => this.descriptor != null;

        public override object SelectedObject => this.descriptor;

        public ReadOnlyObservableCollection<DomainUserListItemBase> Users
        {
            get => this.users;
            private set
            {
                this.users = value;
                this.NotifyOfPropertyChange(nameof(this.Users));
            }
        }

        public DomainUserListItemBase SelectedUser
        {
            get => this.selectedUser;
            set
            {
                this.selectedUser = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedUser));
            }
        }
    }
}

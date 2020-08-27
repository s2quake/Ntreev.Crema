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
using JSSoft.Crema.Presentation.Users.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Users.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.ModernUI.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Users.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [ParentType("JSSoft.Crema.Presentation.Home.IPropertyService, JSSoft.Crema.Presentation.Home, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class DataBaseUsersViewModel : PropertyItemBase
    {
        private readonly Authenticator authenticator;
        private readonly ICremaAppHost cremaAppHost;
        private DataBaseUserItemViewModel selectedUser;
        private IDataBaseDescriptor descriptor;
        private INotifyPropertyChanged notifyObject;

        [ImportingConstructor]
        public DataBaseUsersViewModel(Authenticator authenticator, ICremaAppHost cremaAppHost)
            : base(cremaAppHost)
        {
            this.authenticator = authenticator;
            this.cremaAppHost = cremaAppHost;
            this.DisplayName = Resources.Title_DataBaseUsers;
        }

        public override bool CanSupport(object obj)
        {
            return obj is IDataBaseDescriptor;
        }

        public override void SelectObject(object obj)
        {
            if (this.notifyObject != null)
            {
                this.notifyObject.PropertyChanged -= NotifyObject_PropertyChanged;
            }

            this.descriptor = obj as IDataBaseDescriptor;
            this.notifyObject = obj as INotifyPropertyChanged;
            this.SyncUsers(this.descriptor == null ? new AuthenticationInfo[] { } : this.descriptor.AuthenticationInfos);

            if (this.notifyObject != null)
            {
                this.notifyObject.PropertyChanged += NotifyObject_PropertyChanged;
            }
        }

        public async Task NotifyMessageAsync()
        {
            var userIDs = this.Users.Select(item => item.ID).ToArray();
            var dialog = await NotifyMessageViewModel.CreateInstanceAsync(this.authenticator, this.cremaAppHost, userIDs);
            await dialog.ShowDialogAsync();
        }

        private void NotifyObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.descriptor.AuthenticationInfos) || e.PropertyName == string.Empty)
            {
                this.SyncUsers(this.descriptor.AuthenticationInfos);
            }
        }

        public override bool IsVisible => this.descriptor != null && this.Users.Any();

        public override object SelectedObject => this.descriptor;

        public bool CanNotifyMessage => this.authenticator.Authority == Authority.Admin;

        public ObservableCollection<DataBaseUserItemViewModel> Users { get; } = new ObservableCollection<DataBaseUserItemViewModel>();

        public DataBaseUserItemViewModel SelectedUser
        {
            get => this.selectedUser;
            set
            {
                this.selectedUser = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedUser));
            }
        }

        private void SyncUsers(AuthenticationInfo[] authenticationInfos)
        {
            foreach (var item in this.Users.ToArray())
            {
                if (authenticationInfos.Any(i => i.ID == item.ID) == false)
                {
                    this.Users.Remove(item);
                }
            }

            foreach (var item in authenticationInfos)
            {
                if (this.Users.Any(i => i.ID == item.ID) == false)
                {
                    var viewModel = new DataBaseUserItemViewModel(this.ServiceProvider, item);
                    this.Users.Add(viewModel);
                }
            }

            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }
    }
}

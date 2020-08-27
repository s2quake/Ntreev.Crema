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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace JSSoft.Crema.Presentation.Framework.Dialogs.ViewModels
{
    public class AccessItemViewModel : PropertyChangedBase
    {
        private readonly AccessViewModel viewMdoel;
        private readonly IAccessible accessible;
        private readonly Dispatcher dispatcher;
        private readonly Authentication authentication;

        private readonly string memberID;
        private AccessType accessType;
        private bool canChange = true;

        public AccessItemViewModel(AccessViewModel viewMdoel, string memberID, AccessType accessType)
        {
            this.viewMdoel = viewMdoel;
            this.accessible = viewMdoel.Accessible;
            this.dispatcher = viewMdoel.Dispatcher;
            this.authentication = viewMdoel.Authentication;
            this.memberID = memberID;
            this.accessType = accessType;
        }

        public async Task DeleteAsync()
        {
            if (await AppMessageBox.ConfirmDeleteAsync() == false)
                return;
            try
            {
                await this.accessible.RemoveAccessMemberAsync(this.authentication, this.MemberID);
                this.viewMdoel.ItemsSource.Remove(this);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public IEnumerable UserItems => this.viewMdoel.UserItems;

        public string MemberID => this.memberID;

        public IEnumerable AccessTypes
        {
            get
            {
                yield return AccessType.Owner;
                yield return AccessType.Master;
                yield return AccessType.Editor;
                yield return AccessType.Guest;
            }
        }

        public AccessType AccessType
        {
            get => this.accessType;
            set => this.SetAccessTypeAsync(value);
        }

        public bool CanChange
        {
            get => this.canChange;
            private set
            {
                this.canChange = value;
                this.NotifyOfPropertyChange(nameof(this.CanChange));
            }
        }

        private async void SetAccessTypeAsync(AccessType accessType)
        {
            var oldValue = this.accessType;
            try
            {
                this.accessType = accessType;
                this.NotifyOfPropertyChange(nameof(this.AccessType));
                this.CanChange = false;
                await this.accessible.SetAccessMemberAsync(this.authentication, this.MemberID, accessType);
                var accessInfo = this.accessible.AccessInfo;
                this.accessType = accessType;
                await this.Dispatcher.InvokeAsync(() => this.viewMdoel.SetAccessInfo(accessInfo));
            }
            catch (Exception e)
            {
                this.accessType = oldValue;
                await AppMessageBox.ShowErrorAsync(e);
            }
            finally
            {
                this.CanChange = true;
                this.NotifyOfPropertyChange(nameof(this.AccessType));
            }
        }

        internal void SetAccessType(AccessType accessType)
        {
            this.accessType = accessType;
            this.NotifyOfPropertyChange(nameof(this.AccessType));
        }
    }
}

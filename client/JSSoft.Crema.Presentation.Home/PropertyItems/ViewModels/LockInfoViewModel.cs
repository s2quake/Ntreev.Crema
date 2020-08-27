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
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.ModernUI.Framework;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Presentation.Home.PropertyItems.ViewModels
{
    [Export(typeof(IPropertyItem))]
    [RequiredAuthority(Authority.Guest)]
    [ParentType(typeof(PropertyService))]
    public class LockInfoViewModel : PropertyItemBase
    {
        private ILockableDescriptor descriptor;
        private LockInfo lockInfo;

        public LockInfoViewModel()
        {
            this.DisplayName = Resources.Title_LockInfo;
        }

        public override bool CanSupport(object obj)
        {
            return obj is ILockableDescriptor;
        }

        public override void SelectObject(object obj)
        {
            this.Detach();
            this.descriptor = obj as ILockableDescriptor;
            this.Attach();
        }

        public override object SelectedObject => this.descriptor;

        public override bool IsVisible
        {
            get
            {
                if (this.descriptor == null)
                    return false;
                return this.LockInfo.UserID != string.Empty;
            }
        }

        public LockInfo LockInfo
        {
            get => this.lockInfo;
            private set
            {
                this.lockInfo = value;
                this.NotifyOfPropertyChange(nameof(this.LockInfo));
            }
        }

        private void Descriptor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ILockableDescriptor.LockInfo) || e.PropertyName == string.Empty)
            {
                this.lockInfo = this.descriptor.LockInfo;
                this.NotifyOfPropertyChange(nameof(this.IsVisible));
            }
        }

        private void Attach()
        {
            if (this.descriptor != null)
            {
                this.descriptor.PropertyChanged += Descriptor_PropertyChanged;
                this.LockInfo = this.descriptor.LockInfo;
            }
            this.NotifyOfPropertyChange(nameof(this.IsVisible));
            this.NotifyOfPropertyChange(nameof(this.SelectedObject));
        }

        private void Detach()
        {
            if (this.descriptor != null)
            {
                this.descriptor.PropertyChanged -= Descriptor_PropertyChanged;
            }
        }
    }
}
﻿// Released under the MIT License.
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
using JSSoft.Crema.Services;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JSSoft.Crema.Presentation.Home.Services.ViewModels
{
    public class DataBaseItemViewModel : DataBaseListItemBase
    {
        private bool isCurrent;

        public DataBaseItemViewModel(Authentication authentication, IDataBase dataBase, object owner)
            : base(authentication, dataBase, true, owner)
        {

        }

        public async Task CopyAsync()
        {
            await DataBaseUtility.CopyAsync(this.authentication, this);
        }

        public async Task RenameAsync()
        {
            await DataBaseUtility.RenameAsync(this.authentication, this);
        }

        public async Task DeleteAsync()
        {
            await DataBaseUtility.DeleteAsync(this.authentication, this);
        }

        public async Task LoadAsync()
        {
            await DataBaseUtility.LoadAsync(this.authentication, this);
        }

        public async Task UnloadAsync()
        {
            await DataBaseUtility.UnloadAsync(this.authentication, this);
        }

        public async Task LockAsync()
        {
            await LockableDescriptorUtility.LockAsync(this.authentication, this);
        }

        public async Task UnlockAsync()
        {
            await LockableDescriptorUtility.UnlockAsync(this.authentication, this);
        }

        public async Task SetPrivateAsync()
        {
            await AccessibleDescriptorUtility.SetPrivateAsync(this.authentication, this);
        }

        public async Task SetPublicAsync()
        {
            await AccessibleDescriptorUtility.SetPublicAsync(this.authentication, this);
        }

        public async Task SetAuthorityAsync()
        {
            await AccessibleDescriptorUtility.SetAuthorityAsync(this.authentication, this);
        }

        public async Task ViewLogAsync()
        {
            await DataBaseUtility.ViewLogAsync(this.authentication, this);
        }

        [DescriptorProperty]
        public bool CanCopy => DataBaseUtility.CanCopy(this.authentication, this);

        [DescriptorProperty]
        public bool CanRename => DataBaseUtility.CanRename(this.authentication, this);

        [DescriptorProperty]
        public bool CanDelete => DataBaseUtility.CanDelete(this.authentication, this);

        [DescriptorProperty]
        public bool CanLoad => DataBaseUtility.CanLoad(this.authentication, this);

        [DescriptorProperty]
        public bool CanUnload => DataBaseUtility.CanUnload(this.authentication, this);

        [DescriptorProperty]
        public bool CanLock => PermissionDescriptorUtility.CanLock(this.authentication, this);

        [DescriptorProperty]
        public bool CanUnlock => PermissionDescriptorUtility.CanUnlock(this.authentication, this);

        [DescriptorProperty]
        public bool CanSetPrivate => PermissionDescriptorUtility.CanSetPrivate(this.authentication, this);

        [DescriptorProperty]
        public bool CanSetPublic => PermissionDescriptorUtility.CanSetPublic(this.authentication, this);

        [DescriptorProperty]
        public bool CanSetAuthority => PermissionDescriptorUtility.CanSetAuthority(this.authentication, this);

        [DescriptorProperty]
        public bool CanViewLog => DataBaseUtility.CanViewLog(this.authentication, this);

        public Color Color
        {
            get
            {
                if (this.ConnectionItem == null)
                    return Colors.Transparent;
                return this.ConnectionItem.ThemeColor;
            }
        }

        public IConnectionItem ConnectionItem { get; set; }

        public bool IsCurrent
        {
            get => this.isCurrent;
            set
            {
                this.isCurrent = value;
                this.NotifyOfPropertyChange(nameof(this.IsCurrent));
            }
        }
    }
}

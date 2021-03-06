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
using JSSoft.ModernUI.Framework;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels
{
    public class TableTreeViewItemViewModel : TableTreeItemBase
    {
        public TableTreeViewItemViewModel(Authentication authentication, ITable table)
            : this(authentication, table, null)
        {

        }

        public TableTreeViewItemViewModel(Authentication authentication, ITable table, object owner)
            : this(authentication, new TableDescriptor(authentication, table, DescriptorTypes.All, owner), owner)
        {

        }

        public TableTreeViewItemViewModel(Authentication authentication, TableDescriptor descriptor)
            : this(authentication, descriptor, null)
        {

        }

        public TableTreeViewItemViewModel(Authentication authentication, TableDescriptor descriptor, object owner)
            : base(authentication, descriptor, owner)
        {
            this.RenameCommand = new DelegateCommand(async item => await this.RenameAsync(), item => this.CanRename);
            this.DeleteCommand = new DelegateCommand(async item => await this.DeleteAsync(), item => this.CanDelete);
            this.EditCommand = new DelegateCommand(async item =>
            {
                if (this.CanEditContent == true)
                    await this.EditContentAsync();
                else if (CanViewContent == true)
                    await this.ViewContentAsync();
            });
        }

        public async Task EditContentAsync()
        {
            await TableUtility.EditContentAsync(this.authentication, this.descriptor);
        }

        public async Task ViewContentAsync()
        {
            await TableUtility.ViewContentAsync(this.authentication, this.descriptor);
        }

        public async Task CancelEditAsync()
        {
            await TableUtility.CancelEditAsync(this.authentication, this.descriptor);
        }

        public async Task EndEditAsync()
        {
            await TableUtility.EndEditAsync(this.authentication, this.descriptor);
        }

        public async Task EditTemplateAsync()
        {
            await TableUtility.EditTemplateAsync(this.authentication, this.descriptor);
        }

        public async Task ViewTemplateAsync()
        {
            await TableUtility.ViewTemplateAsync(this.authentication, this.descriptor);
        }

        public async Task CopyAsync()
        {
            await TableUtility.CopyAsync(this.authentication, this.descriptor);
        }

        public async Task InheritAsync()
        {
            await TableUtility.InheritAsync(this.authentication, this.descriptor);
        }

        public async Task RenameAsync()
        {
            if (await TableUtility.RenameAsync(this.authentication, this.descriptor) == false)
                return;

            if (this.Owner is ISelector selector)
                selector.SelectedItem = this;
        }

        public async Task MoveAsync()
        {
            if (await TableUtility.MoveAsync(this.authentication, this.descriptor) == false)
                return;

            if (this.Owner is ISelector)
                this.ExpandAncestors();
        }

        public async Task DeleteAsync()
        {
            await TableUtility.DeleteAsync(this.authentication, this.descriptor);
        }

        public async Task ViewLogAsync()
        {
            await TableItemUtility.ViewLogAsync(this.authentication, this.descriptor);
        }

        public async Task NewTableAsync()
        {
            await TableUtility.NewTableAsync(this.authentication, this.descriptor);
        }

        [DescriptorProperty]
        public bool CanEditTemplate => TableUtility.CanEditTemplate(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanViewTemplate => TableUtility.CanViewTemplate(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanEditContent => TableUtility.CanEditContent(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanViewContent => TableUtility.CanEditContent(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanCancelEdit => TableUtility.CanCancelEdit(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanEndEdit => TableUtility.CanEditEdit(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanCopy => TableUtility.CanCopy(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanInherit => TableUtility.CanInherit(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanViewLog => TableItemUtility.CanViewLog(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanNewTable => TableUtility.CanNewTable(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanRename => TableUtility.CanRename(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanMove => TableUtility.CanMove(this.authentication, this.descriptor);

        [DescriptorProperty]
        public bool CanDelete => TableUtility.CanDelete(this.authentication, this.descriptor);

        public ICommand RenameCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand EditCommand { get; }

        public override string DisplayName
        {
            get
            {
                if (this.Parent is TableTreeViewItemViewModel == true)
                    return this.descriptor.TableName;
                else
                    return this.descriptor.Name;
            }
        }

        protected override TableTreeItemBase CreateInstance(Authentication authentication, TableDescriptor descriptor, object owner)
        {
            return new TableTreeViewItemViewModel(authentication, descriptor, owner);
        }
    }
}

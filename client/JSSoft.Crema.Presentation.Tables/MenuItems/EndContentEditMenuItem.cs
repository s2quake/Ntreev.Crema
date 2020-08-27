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

using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Tables.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Presentation.Tables.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType(typeof(TableTreeItemBase))]
    [ParentType(typeof(TableListItemBase))]
    [Category("Close")]
    class EndContentEditMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public EndContentEditMenuItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
            this.DisplayName = Resources.MenuItem_EndContentEditing;
            this.HideOnDisabled = true;
        }

        protected override bool OnCanExecute(object parameter)
        {
            if (parameter is ITableDescriptor descriptor)
            {
                if (TableDescriptorUtility.IsBeingEdited(this.authenticator, descriptor) == false)
                    return false;
                return descriptor.IsContentOwner == true || this.authenticator.Authority == Authority.Admin;
            }

            return false;
        }

        protected async override void OnExecute(object parameter)
        {
            try
            {
                if (parameter is ITableDescriptor descriptor && descriptor.Target is ITable table)
                {
                    if (await AppMessageBox.ShowProceedAsync(Resources.Message_ConfirmToEndContentEditing) == false)
                        return;
                    await table.Content.EndEditAsync(this.authenticator);
                }
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }
    }
}

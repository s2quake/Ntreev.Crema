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
using Ntreev.Crema.Presentation.Types.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Types.Documents.ViewModels;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Types
{
    public static class TypeCategoryUtility
    {
        public static bool CanRename(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Master;
            return false;
        }

        public static bool CanMove(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Master;
            return false;
        }

        public static bool CanDelete(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Master;
            return false;
        }

        public static bool CanNewType(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Master;
            return false;
        }

        public static bool CanNewFolder(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Master;
            return false;
        }

        public static bool CanFind(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Guest;
            return false;
        }

        public static bool CanViewLog(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Guest;
            return false;
        }

        public static async Task<string> NewTypeAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            var dialog = await NewTypeViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return dialog.TypeName;
            return null;
        }

        public static async Task<string> NewFolderAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            var dialog = await NewTypeCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return dialog.CategoryName;
            return null;
        }

        public static async Task<bool> RenameAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            var comment = await LockAsync(authentication, descriptor, nameof(ITypeCategory.RenameAsync));
            if (comment == null)
                return false;
            var dialog = await RenameCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            var dialogResult = await ShowDialogAsync(dialog);
            await UnlockAsync(authentication, descriptor, comment);
            return dialogResult;
        }

        public static async Task<bool> MoveAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            var comment = await LockAsync(authentication, descriptor, nameof(ITypeCategory.MoveAsync));
            if (comment == null)
                return false;
            var dialog = await MoveTypeCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            var dialogResult = await ShowDialogAsync(dialog);
            await UnlockAsync(authentication, descriptor, comment);
            return dialogResult;
        }

        public static async Task<bool> DeleteAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            var dialog = await DeleteTypeCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> FindAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITypeCategory category && category.GetService(typeof(TypeDocumentServiceViewModel)) is TypeDocumentServiceViewModel documentService)
            {
                documentService.AddFinder(authentication, descriptor.Path);
                return await Task.Run(() => true);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<bool> ViewLogAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (descriptor.Target is ITypeCategory category)
            {
                var dialog = new LogViewModel(authentication, category as ITypeItem);
                return await dialog.ShowDialogAsync() == true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static async Task<string> LockAsync(Authentication authentication, ITypeCategoryDescriptor descriptor, string comment)
        {
            if (descriptor.Target is ITypeCategory category)
            {
                try
                {
                    var lockInfo = await category.Dispatcher.InvokeAsync(() => category.LockInfo);
                    if (lockInfo.IsLocked == false || lockInfo.IsInherited == true)
                    {
                        var lockComment = comment + ":" + Guid.NewGuid();
                        await category.LockAsync(authentication, lockComment);
                        return lockComment;
                    }
                    return string.Empty;
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                    return null;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static async Task UnlockAsync(Authentication authentication, ITypeCategoryDescriptor descriptor, string comment)
        {
            if (descriptor.Target is ITypeCategory category)
            {
                if (category.Dispatcher == null)
                    return;

                try
                {
                    var lockInfo = await category.Dispatcher.InvokeAsync(() => category.LockInfo);
                    if (lockInfo.IsLocked == true && lockInfo.IsInherited == false && lockInfo.Comment == comment)
                    {
                        await category.UnlockAsync(authentication);
                    }
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static async Task<bool> ShowDialogAsync(ModalDialogBase dialog)
        {
            if (dialog != null)
                return await dialog.ShowDialogAsync() == true;
            return false;
        }
    }
}

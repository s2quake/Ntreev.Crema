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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Users
{
    public static class UserCategoryUtility
    {
        public static bool CanRename(Authentication authentication, IUserCategoryDescriptor _)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanMove(Authentication authentication, IUserCategoryDescriptor _)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanDelete(Authentication authentication, IUserCategoryDescriptor _)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanNewUser(Authentication authentication, IUserCategoryDescriptor _)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanNewFolder(Authentication authentication, IUserCategoryDescriptor _)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static async Task<string> NewUserAsync(Authentication authentication, IUserCategoryDescriptor descriptor)
        {
            var dialog = await NewUserViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return dialog.UserName;
            return null;
        }

        public static async Task<string> NewFolderAsync(Authentication authentication, IUserCategoryDescriptor descriptor)
        {
            var dialog = await NewUserCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return dialog.CategoryName;
            return null;
        }

        public static async Task<bool> RenameAsync(Authentication authentication, IUserCategoryDescriptor descriptor)
        {
            var dialog = await RenameCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> MoveAsync(Authentication authentication, IUserCategoryDescriptor descriptor)
        {
            var dialog = await MoveUserCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> DeleteAsync(Authentication authentication, IUserCategoryDescriptor descriptor)
        {
            var dialog = await DeleteUserCategoryViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }
    }
}

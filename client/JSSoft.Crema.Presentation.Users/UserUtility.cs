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
using JSSoft.ModernUI.Framework;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Users
{
    public static class UserUtility
    {
        public static bool CanRename(Authentication _1, IUserDescriptor _2)
        {
            return false;
        }

        public static bool CanMove(Authentication authentication, IUserDescriptor _)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanDelete(Authentication authentication, IUserDescriptor descriptor)
        {
            if (authentication.ID == descriptor.UserInfo.ID)
                return false;
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanSendMessage(Authentication authentication, IUserDescriptor descriptor)
        {
            if (authentication.ID == descriptor.UserInfo.ID)
                return false;
            return UserDescriptorUtility.IsOnline(authentication, descriptor);
        }

        public static bool CanChange(Authentication authentication, IUserDescriptor _)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanKick(Authentication authentication, IUserDescriptor descriptor)
        {
            if (authentication == null)
                return false;
            if (authentication.ID == descriptor.UserInfo.ID)
                return false;
            if (authentication.Authority != Authority.Admin)
                return false;
            return UserDescriptorUtility.IsOnline(authentication, descriptor) == true;
        }

        public static bool CanBan(Authentication authentication, IUserDescriptor descriptor)
        {
            if (authentication == null)
                return false;
            if (authentication.ID == descriptor.UserInfo.ID)
                return false;
            if (authentication.Authority != Authority.Admin)
                return false;
            return UserDescriptorUtility.IsBanned(authentication, descriptor) == false;
        }

        public static bool CanUnban(Authentication authentication, IUserDescriptor descriptor)
        {
            if (authentication == null)
                return false;
            if (authentication.ID == descriptor.UserInfo.ID)
                return false;
            if (authentication.Authority != Authority.Admin)
                return false;
            return UserDescriptorUtility.IsBanned(authentication, descriptor) == true;
        }

        public static async Task<bool> RenameAsync(Authentication _1, IUserDescriptor _2)
        {
            return await Task.Run(() => false);
        }

        public static async Task<bool> MoveAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            var dialog = await MoveUserViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> DeleteAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            var dialog = await DeleteUserViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> SendMessageAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            if (descriptor.Target is IUser)
            {
                var dialog = await SendMessageViewModel.CreateInstanceAsync(authentication, descriptor);
                if (dialog != null && await dialog.ShowDialogAsync() == true)
                    return true;
                return false;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<bool> ChangeAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            var dialog = await ChangeUserViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> KickAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            var dialog = await KickViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> BanAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            var dialog = await BanViewModel.CreateInstanceAsync(authentication, descriptor);
            if (dialog != null && await dialog.ShowDialogAsync() == true)
                return true;
            return false;
        }

        public static async Task<bool> UnbanAsync(Authentication authentication, IUserDescriptor descriptor)
        {
            if (descriptor.Target is IUser user)
            {
                try
                {
                    await user.UnbanAsync(authentication);
                    return true;
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                    return false;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

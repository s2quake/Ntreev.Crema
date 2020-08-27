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
using JSSoft.Crema.Presentation.Home.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Home.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Home
{
    public static class DataBaseUtility
    {
        public static bool CanCopy(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanCreate(Authentication authentication)
        {
            return authentication != null && authentication.Authority == Authority.Admin;
        }

        public static bool CanRename(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (DataBaseDescriptorUtility.IsLoaded(authentication, descriptor) == true)
                return false;
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanDelete(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (DataBaseDescriptorUtility.IsLoaded(authentication, descriptor) == true)
                return false;
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanLoad(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (DataBaseDescriptorUtility.IsLoaded(authentication, descriptor) == true)
                return false;
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Owner;
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanUnload(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (DataBaseDescriptorUtility.IsLoaded(authentication, descriptor) == false)
                return false;
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Owner;
            return authentication.Authority == Authority.Admin;
        }

        public static bool CanViewLog(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (descriptor is IPermissionDescriptor permissionDescriptor)
                return permissionDescriptor.AccessType >= AccessType.Guest;
            return false;
        }

        public async static Task<bool> CreateAsync(Authentication authentication, IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(ICremaHost)) is ICremaHost cremaHost)
            {
                var dialog = await CreateDataBaseViewModel.CreateInstanceAsync(authentication, cremaHost);
                return await dialog?.ShowDialogAsync() == true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public async static Task<bool> CopyAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            var dialog = await CopyDataBaseViewModel.CreateInstanceAsync(authentication, descriptor);
            return await dialog?.ShowDialogAsync() == true;
        }

        public async static Task<string> RenameAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            var dialog = await RenameDataBaseViewModel.CreateInstanceAsync(authentication, descriptor);
            if (await dialog?.ShowDialogAsync() == true)
                return dialog.NewName;
            return null;
        }

        public async static Task<bool> DeleteAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (descriptor.Target is IDataBase dataBase)
            {
                if (await new DeleteViewModel().ShowDialogAsync() != true)
                    return false;

                try
                {
                    await dataBase.DeleteAsync(authentication);
                    await AppMessageBox.ShowAsync(Resources.Message_Deleted);
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

        public async static Task<bool> LoadAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (descriptor.Target is IDataBase dataBase)
            {
                try
                {
                    await dataBase.LoadAsync(authentication);
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

        public async static Task<bool> UnloadAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            if (descriptor.Target is IDataBase dataBase)
            {
                try
                {
                    if (descriptor.AuthenticationInfos.Any() == true && await AppMessageBox.ShowProceedAsync(Resources.Message_VerifyToCloseDataBase) == false)
                        return false;
                    await dataBase.UnloadAsync(authentication);
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

        public static async Task<bool> ViewLogAsync(Authentication authentication, IDataBaseDescriptor descriptor)
        {
            return await LogViewModel.ShowDialogAsync(authentication, descriptor) != null;
        }
    }
}

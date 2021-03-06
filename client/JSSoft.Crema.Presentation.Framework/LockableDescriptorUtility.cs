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

using JSSoft.Crema.Presentation.Framework.Dialogs.ViewModels;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework
{
    public static class LockableDescriptorUtility
    {
        public static bool IsLocked(Authentication authentication, ILockableDescriptor descriptor)
        {
            return descriptor.LockInfo.IsLocked;
        }

        public static bool IsLockOwner(Authentication authentication, ILockableDescriptor descriptor)
        {
            return descriptor.LockInfo.IsOwner(authentication.ID);
        }

        public static bool IsLockInherited(Authentication authentication, ILockableDescriptor descriptor)
        {
            return descriptor.LockInfo.IsInherited;
        }

        public static bool CanLock(Authentication authentication, ILockableDescriptor descriptor)
        {
            if (descriptor.LockInfo.IsLocked == true && descriptor.LockInfo.IsInherited == false)
                return false;

            return authentication.Authority == Authority.Admin || authentication.Authority == Authority.Member;
        }

        public static bool CanUnlock(Authentication authentication, ILockableDescriptor descriptor)
        {
            if (descriptor.LockInfo.IsLocked == false || descriptor.LockInfo.IsInherited == true)
                return false;

            return descriptor.LockInfo.IsOwner(authentication.ID) == true;
        }

        public static async Task<bool> LockAsync(Authentication authentication, ILockableDescriptor descriptor)
        {
            var dialog = await LockLockableViewModel.CreateInstanceAsync(authentication, descriptor);
            return await dialog?.ShowDialogAsync() == true;
        }

        public static async Task<bool> UnlockAsync(Authentication authentication, ILockableDescriptor descriptor)
        {
            if (descriptor.Target is ILockable lockable)
            {
                try
                {
                    await lockable.UnlockAsync(authentication);
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

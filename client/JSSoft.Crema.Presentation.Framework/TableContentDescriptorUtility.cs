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

using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework
{
    public static class TableContentDescriptorUtility
    {
        public static async Task BeginEditAsync(Authentication authentication, ITableContentDescriptor descriptor)
        {
            if (descriptor.Target is ITableContent content)
            {
                if (content.Domain == null)
                {
                    await content.BeginEditAsync(authentication);
                }
                var domain = content.Domain;
                var isEntered = await domain.Users.ContainsAsync(authentication.ID);
                if (isEntered == false)
                {
                    await content.EnterEditAsync(authentication);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<bool> EndEditAsync(Authentication authentication, ITableContentDescriptor descriptor)
        {
            if (descriptor.Target is ITableContent content)
            {
                await content.LeaveEditAsync(authentication);
                var domain = content.Domain;
                var isEmpty = await domain.Dispatcher.InvokeAsync(() => domain.Users.Any() == false);
                if (isEmpty == true)
                {
                    await content.EndEditAsync(authentication);
                }
                return true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

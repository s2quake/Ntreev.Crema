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
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.ObjectModel;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class NewTableViewModel : TemplateViewModel
    {
        private readonly ITableCategory category;
        private readonly ITableContext tableContext;

        public NewTableViewModel(Authentication authentication, ITableCategory category, ITableTemplate template)
            : base(authentication, template, true)
        {
            this.category = category;
            this.category.Dispatcher.VerifyAccess();
            this.tableContext = category.GetService(typeof(ITableContext)) as ITableContext;
            this.DisplayName = Resources.Title_NewTable;
        }

        public static async Task<NewTableViewModel> CreateInstanceAsync(Authentication authentication, ITableCategoryDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITableCategory category)
            {
                var template = await category.NewTableAsync(authentication);
                return await category.Dispatcher.InvokeAsync(() =>
                {
                    return new NewTableViewModel(authentication, category, template);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected async override void Verify(Action<bool> isVerify)
        {
            if (this.TableName == string.Empty)
                return;
            if (NameValidator.VerifyName(this.TableName) == false)
                return;
            var result = await this.tableContext.Tables.ContainsAsync(this.TableName) == false;
            isVerify(result);
        }
    }
}
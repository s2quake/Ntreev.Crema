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
using JSSoft.Crema.Presentation.Types.Properties;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.ObjectModel;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Types.Dialogs.ViewModels
{
    public class NewTypeViewModel : TemplateViewModel
    {
        private readonly ITypeContext typeContext;

        public NewTypeViewModel(Authentication authentication, ITypeCategory category, ITypeTemplate template)
            : base(authentication, template, true)
        {
            this.Category = category;
            this.typeContext = category.GetService(typeof(ITypeContext)) as ITypeContext;
            this.DisplayName = Resources.Title_NewType;
        }

        public static async Task<NewTypeViewModel> CreateInstanceAsync(Authentication authentication, ITypeCategoryDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITypeCategory category)
            {
                var template = await category.NewTypeAsync(authentication);
                return await category.Dispatcher.InvokeAsync(() =>
                {
                    return new NewTypeViewModel(authentication, category, template);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected ITypeCategory Category { get; }

        protected async override void Verify(Action<bool> isVerify)
        {
            if (this.TypeName == string.Empty)
                return;
            if (NameValidator.VerifyName(this.TypeName) == false)
                return;
            var result = await this.typeContext.Types.ContainsAsync(this.TypeName) == false;
            isVerify(result);
        }
    }
}

﻿//Released under the MIT License.
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

using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Extensions;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Ntreev.Crema.Javascript.Methods.User
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(User))]
    class RenameUserCategoryMethod : ScriptFuncTaskBase<string, string, string>
    {
        [ImportingConstructor]
        public RenameUserCategoryMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        [ReturnParameterName("categoryPath")]
        protected override async Task<string> OnExecuteAsync(string categoryPath, string newName)
        {
            var category = await this.CremaHost.GetUserCategoryAsync(categoryPath);
            var authentication = this.Context.GetAuthentication(this);
            await category.RenameAsync(authentication, newName);
            return category.Path;
        }
    }
}

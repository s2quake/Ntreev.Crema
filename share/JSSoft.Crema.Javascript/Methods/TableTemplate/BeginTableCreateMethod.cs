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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.TableTemplate
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(TableTemplate))]
    class BeginTableCreateMethod : ScriptFuncTaskBase<string, string, string>
    {
        [ImportingConstructor]
        public BeginTableCreateMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        [ReturnParameterName("domainID")]
        protected override async Task<string> OnExecuteAsync(string dataBaseName, string parentPath)
        {
            var dataBase = await this.CremaHost.GetDataBaseAsync(dataBaseName);

            if (NameValidator.VerifyCategoryPath(parentPath) == true)
            {
                var category = dataBase.TableContext.Categories[parentPath];
                if (category == null)
                    throw new CategoryNotFoundException(parentPath);
                var authentication = this.Context.GetAuthentication(this);
                var template = await category.NewTableAsync(authentication);
                return $"{template.Domain.ID}";
            }
            else if (NameValidator.VerifyItemPath(parentPath) == true)
            {
                if (!(dataBase.TableContext[parentPath] is ITable table))
                    throw new CategoryNotFoundException(parentPath);
                var authentication = this.Context.GetAuthentication(this);
                var template = await table.NewTableAsync(authentication);
                return $"{template.Domain.ID}";
            }
            else
            {
                if (!(dataBase.TableContext.Tables[parentPath] is ITable table))
                    throw new CategoryNotFoundException(parentPath);
                var authentication = this.Context.GetAuthentication(this);
                var template = await table.NewTableAsync(authentication);
                return $"{template.Domain.ID}";
            }
        }
    }
}

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
using JSSoft.Crema.Presentation.Framework.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class RenameTableViewModel : RenameAsyncAppViewModel
    {
        private readonly Authentication authentication;
        private readonly ITable table;

        private RenameTableViewModel(Authentication authentication, ITable table)
            : base(table.TableName)
        {
            this.authentication = authentication;
            this.table = table;
            this.table.Dispatcher.VerifyAccess();
            this.DisplayName = Resources.Title_RenameTable;
        }

        public static Task<RenameTableViewModel> CreateInstanceAsync(Authentication authentication, ITableDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITable table)
            {
                return table.Dispatcher.InvokeAsync(() =>
                {
                    return new RenameTableViewModel(authentication, table);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        protected async override void VerifyRename(string newName, Action<bool> isVerify)
        {
            var result = await await this.table.Dispatcher.InvokeAsync(async () =>
            {
                var tableContext = this.table.GetService(typeof(ITableContext)) as ITableContext;
                var categoryNames = tableContext.Categories.Select(item => item.Path).ToArray();
                if (categoryNames.Contains(newName, StringComparer.OrdinalIgnoreCase) == true)
                    return false;

                if (this.table.Childs.ContainsKey(newName) == true)
                    return false;

                if (this.table.Parent != null)
                {
                    if (this.table.Parent.Name == newName)
                        return false;
                    return true;
                }

                return await tableContext.Tables.ContainsAsync(newName) == false;
            });

            isVerify(result);
        }

        protected override Task OnRenameAsync(string newName)
        {
            return this.table.RenameAsync(this.authentication, newName);
        }
    }
}

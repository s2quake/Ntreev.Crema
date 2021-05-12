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

using JSSoft.Crema.Data;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class NewRowViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly ITableRow row;
        private readonly TableInfo tableInfo;
        private readonly Dictionary<string, TypeInfo> typeInfoByName;
        private readonly List<object> items = new();
        private readonly ITableContent parentContent;

        private NewRowViewModel(Authentication authentication, ITableRow row, TableInfo tableInfo, TypeInfo[] typeInfos)
        {
            this.authentication = authentication;
            this.row = row;
            this.row.Dispatcher.VerifyAccess();
            this.tableInfo = tableInfo;
            this.typeInfoByName = typeInfos.ToDictionary(item => item.Path);
            this.parentContent = row.Content.Tables.FirstOrDefault(item => item.Name == tableInfo.ParentName)?.Content;

            if (this.parentContent != null)
            {
                this.items.Add(new NewRowParentItemViewModel(authentication, this.row, this.parentContent));
            }

            foreach (var item in this.tableInfo.Columns)
            {
                if (CremaDataTypeUtility.IsBaseType(item.DataType) == false)
                {
                    var typeInfo = this.typeInfoByName[item.DataType];
                    this.items.Add(new NewRowItemViewModel(authentication, row, item, typeInfo));
                }
                else
                {
                    this.items.Add(new NewRowItemViewModel(authentication, row, item));
                }
            }
            this.DisplayName = $"New Row - {tableInfo.Name}";
        }

        public static async Task<NewRowViewModel> CreateAsync(Authentication authentication, ITableContent content)
        {
            var tuple = await content.Dispatcher.InvokeAsync(() =>
            {
                var table = content.Table;
                var domain = content.Domain;
                var dataSet = domain.Source as CremaDataSet;
                var tableInfo = dataSet.Tables[table.Name, table.Category.Path].TableInfo;
                var typeInfos = dataSet.Types.Select(item => item.TypeInfo).ToArray();
                return (tableInfo, typeInfos);
            });
            var row = await content.AddNewAsync(authentication, null);
            return await row.Dispatcher.InvokeAsync(() => new NewRowViewModel(authentication, row, tuple.tableInfo, tuple.typeInfos));
        }

        public IReadOnlyList<object> Items => this.items;

        public async Task InsertAsync()
        {
            try
            {
                await this.row.Content.EndNewAsync(this.authentication, this.row);
                this.Keys = await this.row.GetKeysAsync();
                await this.TryCloseAsync(true);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public object[] Keys { get; private set; }
    }
}

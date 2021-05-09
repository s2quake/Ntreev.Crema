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
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.Converters;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    class NewRowParentItemViewModel : PropertyChangedBase
    {
        private readonly Authentication authentication;
        private readonly ITableRow row;
        private readonly ITableContent parentContent;
        private readonly TableInfo tableInfo;
        private readonly ContentToStringConverter converter = new();
        private readonly List<RowItemByKey> itemList;
        private RowItemByKey value;

        public NewRowParentItemViewModel(Authentication authentication, ITableRow row, ITableContent parentContent)
        {
            this.authentication = authentication;
            this.row = row;
            this.parentContent = parentContent;
            this.tableInfo = parentContent.Table.TableInfo;
            this.itemList = new List<RowItemByKey>(parentContent.Count);
            foreach (var item in parentContent)
            {
                var rowItem = new RowItemByKey()
                {
                    IsEnabled = item.IsEnabled,
                    Tags = item.Tags,
                    ID = item.ID,
                    Value = this.GetKey(item, this.tableInfo),
                };
                this.itemList.Add(rowItem);
            }
        }

        public IEnumerable Items => this.itemList;

        public object Value
        {
            get => this.value;
            set
            {
                Invoke();
                async void Invoke()
                {
                    if (value is RowItemByKey rowItem)
                    {
                        await this.row.SetParentAsync(this.authentication, rowItem.ID);
                        this.value = rowItem;
                        this.NotifyOfPropertyChange(nameof(this.Value));
                    }
                }
            }
        }

        public bool IsKey => false;

        public bool IsUnique => false;

        public string Comment => string.Empty;

        public string Name => nameof(CremaSchema.ParentID);

        private string GetKey(ITableRow row, TableInfo tableInfo)
        {
            var textList = new List<string>();
            foreach (var item in tableInfo.Columns)
            {
                if (item.IsKey == true)
                {
                    var text = (string)this.converter.Convert(row[item.Name], typeof(string), null, CultureInfo.CurrentUICulture);
                    textList.Add(text);
                }
            }
            return string.Join(", ", textList);
        }
    }
}

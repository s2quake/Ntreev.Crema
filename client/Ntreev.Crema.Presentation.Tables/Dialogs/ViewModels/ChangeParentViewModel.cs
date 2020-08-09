//Released under the MIT License.
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

using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.Presentation.Tables.Properties;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class ChangeParentViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly ITableRow row;
        private readonly ITableContent parentContent;
        private readonly TableInfo tableInfo;
        private readonly ContentToStringConverter converter = new ContentToStringConverter();
        private readonly List<RowItemByKey> itemList;
        private RowItemByKey value;
        private RowItemByKey currentValue;

        private ChangeParentViewModel(Authentication authentication, ITableRow row, ITableContent parentContent)
        {
            this.authentication = authentication;
            this.row = row;
            this.parentContent = parentContent;
            this.tableInfo = parentContent.Table.TableInfo;
            this.itemList = new List<RowItemByKey>(parentContent.Count)
            {
                new RowItemByKey() { IsEnabled = true, Value = "(null)" }
            };
            this.value = this.itemList.First();
            foreach (var item in parentContent)
            {
                var rowItem = new RowItemByKey()
                {
                    IsEnabled = item.IsEnabled,
                    Tags = item.Tags,
                    ID = item.ID,
                    Value = this.GetKey(item, this.tableInfo),
                };
                if (row.ParentID == rowItem.ID)
                {
                    this.value = rowItem;
                }
                this.itemList.Add(rowItem);
            }
            this.currentValue = this.value;
            this.DisplayName = "Change Parent";
        }

        public static Task<ChangeParentViewModel> CreateAsync(Authentication authentication, ITableRow row)
        {
            return row.Dispatcher.InvokeAsync(() =>
            {
                var content = row.Content;
                var table = content.Table;
                var tables = content.Tables;
                var tableInfo = table.TableInfo;
                var parentContent = tables.FirstOrDefault(item => item.Name == tableInfo.ParentName)?.Content;
                if (parentContent == null)
                    throw new InvalidOperationException("content has not parent.");

                return new ChangeParentViewModel(authentication, row, parentContent);
            });
        }

        public async Task ChangeAsync()
        {
            try
            {
                this.BeginProgress(Resources.Message_Changing);
                await this.row.SetParentAsync(this.authentication, this.value.ID);
                this.EndProgress();
                await this.TryCloseAsync(true);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public IReadOnlyList<RowItemByKey> Items => this.itemList;

        public RowItemByKey Value
        {
            get { return this.value; }
            set
            {
                Invoke();
                async void Invoke()
                {
                    await this.row.SetParentAsync(this.authentication, value.ID);
                    this.value = value;
                    this.NotifyOfPropertyChange(nameof(this.Value));
                    this.NotifyOfPropertyChange(nameof(this.CanChange));
                }
            }
        }

        public bool IsKey => false;

        public bool IsUnique => false;

        public string Comment => string.Empty;

        public string Name => nameof(CremaSchema.ParentID);

        public bool CanChange => this.value.ID != this.currentValue.ID;

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

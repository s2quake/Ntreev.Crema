using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels
{
    class NewRowParentItemViewModel : PropertyChangedBase
    {
        private readonly Authentication authentication;
        private readonly ITableRow row;
        private readonly ITableContent parentContent;
        private readonly TableInfo tableInfo;
        private readonly ContentToStringConverter converter = new ContentToStringConverter();
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
            get { return this.value; }
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

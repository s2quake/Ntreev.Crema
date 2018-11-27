using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.Presentation.Tables.Properties;
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
                this.TryClose(true);
            }
            catch (Exception e)
            {
                this.EndProgress();
                AppMessageBox.ShowError(e);
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

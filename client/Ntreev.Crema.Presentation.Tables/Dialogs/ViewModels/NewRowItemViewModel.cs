using Ntreev.Crema.Data;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class NewRowItemViewModel : PropertyChangedBase
    {
        private readonly Authentication authentication;
        private readonly ITableRow row;
        private readonly ColumnInfo columnInfo;
        private readonly TypeInfo typeInfo;
        private readonly object[] items;
        private object value;
        

        public NewRowItemViewModel(Authentication authentication, ITableRow row, ColumnInfo columnInfo)
        {
            this.authentication = authentication;
            this.row = row;
            this.columnInfo = columnInfo;
            this.value = row[columnInfo.Name];
        }

        public NewRowItemViewModel(Authentication authentication, ITableRow row, ColumnInfo columnInfo, TypeInfo typeInfo)
        {
            this.authentication = authentication;
            this.row = row;
            this.columnInfo = columnInfo;
            this.typeInfo = typeInfo;
            this.items = typeInfo.Members.Select(item => (object)item).ToArray();
            this.value = row[columnInfo.Name];
            
        }

        public string DataType => this.columnInfo.DataType;

        public string Name => this.columnInfo.Name;

        public object Value
        {
            get => this.value;
            set
            {
                Invoke();
                async void Invoke()
                {
                    await this.row.SetFieldAsync(this.authentication, this.columnInfo.Name, value);
                    this.value = value;
                    this.NotifyOfPropertyChange(nameof(this.Value));
                }
            }
        }

        public object[] Items => this.items;

        public bool IsFlag => this.typeInfo.IsFlag;
    }
}

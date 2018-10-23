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
        private object value;

        public NewRowItemViewModel(Authentication authentication, ITableRow row, ColumnInfo columnInfo)
        {
            this.authentication = authentication;
            this.row = row;
            this.columnInfo = columnInfo;
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

    }
}

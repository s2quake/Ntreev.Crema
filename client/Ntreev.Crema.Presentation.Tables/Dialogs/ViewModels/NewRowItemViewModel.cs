using Ntreev.Crema.Data;
using Ntreev.Crema.Services;
using Ntreev.Library;
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
            this.Items = typeInfo.Members.Select(item => (object)item).ToArray();
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

        public object[] Items { get; }

        public bool IsFlag => this.typeInfo.IsFlag;

        public string Comment => this.columnInfo.Comment;

        public bool IsKey => this.columnInfo.IsKey;

        public bool IsUnique => this.columnInfo.IsUnique;

        public TagInfo Tags => this.columnInfo.DerivedTags;
    }
}

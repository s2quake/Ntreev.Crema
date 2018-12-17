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

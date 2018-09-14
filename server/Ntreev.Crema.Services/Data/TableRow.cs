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
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TableRow : DomainBasedRow, ITableRow
    {
        public TableRow(TableContentBase content, DataRow row)
            : base(content.Domain, row)
        {
            this.Content = content;
        }

        public TableRow(TableContentBase content, DataTable table)
            : base(content.Domain, table)
        {
            this.Content = content;
        }

        public TableRow(TableContentBase content, DataTable table, string parentID)
            : base(content.Domain, table, parentID)
        {
            this.Content = content;
        }

        public Task SetIsEnabledAsync(Authentication authentication, bool value)
        {
            return this.SetFieldAsync(authentication, CremaSchema.Enable, value);
        }

        public Task SetTagsAsync(Authentication authentication, TagInfo value)
        {
            return this.SetFieldAsync(authentication, CremaSchema.Tags, value.ToString());
        }

        public Task SetFieldAsync(Authentication authentication, string columnName, object value)
        {
            return base.SetFieldAsync(authentication, columnName, value);
        }

        public object this[string columnName] => base.GetField<object>(columnName);

        public TagInfo Tags => (TagInfo)(this.GetField<string>(CremaSchema.Tags));

        public bool IsEnabled => this.GetField<bool>(CremaSchema.Enable);

        public TableContentBase Content { get; }

        public override DataBase DataBase => this.Content.DataBase;

        public override CremaDispatcher Dispatcher => this.Content.Dispatcher;

        public override CremaHost CremaHost => this.Content.CremaHost;

        public string RelationID
        {
            get
            {
                var dataRow = this.Row;
                var table = dataRow.Table;
                if (table.Columns.Contains(CremaSchema.__RelationID__) == false)
                    return string.Empty;
                return dataRow.Field<string>(CremaSchema.__RelationID__);
            }
        }

        public string ParentID
        {
            get
            {
                var dataRow = this.Row;
                var table = dataRow.Table;
                if (table.Columns.Contains(CremaSchema.__ParentID__) == false)
                    return string.Empty;
                return dataRow.Field<string>(CremaSchema.__ParentID__);
            }
        }

        private void ValidateDispatcher(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
        }

        #region ITableRow

        ITableContent ITableRow.Content => this.Content as ITableContent;

        #endregion
    }
}

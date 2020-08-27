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

using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods.DataBase
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(DataBase))]
    class GetTableItemDataMethod : ScriptFuncTaskBase<string, string, string, IDictionary<string, IDictionary<int, object>>>
    {
        [ImportingConstructor]
        public GetTableItemDataMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override async Task<IDictionary<string, IDictionary<int, object>>> OnExecuteAsync(string dataBaseName, string tableItemPath, string revision)
        {
            var tableItem = await this.CremaHost.GetTableItemAsync(dataBaseName, tableItemPath);
            var authentication = this.Context.GetAuthentication(this);
            var dataSet = await tableItem.GetDataSetAsync(authentication, revision);
            var tables = new Dictionary<string, IDictionary<int, object>>(dataSet.Tables.Count);
            foreach (var item in dataSet.Tables)
            {
                var rows = this.GetDataRows(item);
                tables.Add(item.Name, rows);
            }
            return tables;
        }

        private IDictionary<int, object> GetDataRows(CremaDataTable dataTable)
        {
            var props = new Dictionary<int, object>();
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var dataRow = dataTable.Rows[i];
                props.Add(i, this.GetDataRow(dataRow));
            }
            return props;
        }

        private IDictionary<string, object> GetDataRow(CremaDataRow dataRow)
        {
            var dataTable = dataRow.Table;
            var props = new Dictionary<string, object>();
            foreach (var item in dataTable.Columns)
            {
                var value = dataRow[item];
                if (value == DBNull.Value)
                    props.Add(item.ColumnName, null);
                else
                    props.Add(item.ColumnName, value);
            }
            if (dataRow.ParentID != null)
                props.Add(CremaSchema.__ParentID__, dataRow.ParentID);
            if (dataRow.RelationID != null)
                props.Add(CremaSchema.__RelationID__, dataRow.RelationID);
            return props;
        }
    }
}

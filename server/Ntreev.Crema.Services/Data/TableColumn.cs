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
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TableColumn : DomainBasedRow, ITableColumn
    {
        private readonly TableTemplateBase template;

        public TableColumn(TableTemplateBase template, DataRow row)
            : base(template.Domain, row)
        {
            this.template = template;
        }

        public TableColumn(TableTemplateBase template, DataTable table)
            : base(template.Domain, table)
        {
            this.template = template;
            var query = from DataRow item in table.Rows
                        where (item.RowState == DataRowState.Deleted || item.RowState == DataRowState.Detached) == false
                        select item.Field<string>(CremaSchema.ColumnName);

            var newName = NameUtility.GenerateNewName("Column", query);
            Initialize();

            async void Initialize()
            {
                await this.SetFieldAsync(null, CremaSchema.ColumnName, newName);
            }
        }

        public Task SetIndexAsync(Authentication authentication, int value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.Index, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetIsKeyAsync(Authentication authentication, bool value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.IsKey, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetIsUniqueAsync(Authentication authentication, bool value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.IsUnique, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetNameAsync(Authentication authentication, string value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.ColumnName, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetDataTypeAsync(Authentication authentication, string value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.DataType, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetDefaultValueAsync(Authentication authentication, string value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.DefaultValue, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetCommentAsync(Authentication authentication, string value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.Comment, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetAutoIncrementAsync(Authentication authentication, bool value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.AutoIncrement, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetTagsAsync(Authentication authentication, TagInfo value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.Tags, value.ToString());
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetIsReadOnlyAsync(Authentication authentication, bool value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.ReadOnly, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public Task SetAllowNullAsync(Authentication authentication, bool value)
        {
            try
            {
                return this.SetFieldAsync(authentication, CremaSchema.AllowNull, value);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public int Index => this.GetField<int>(CremaSchema.Index);

        public bool IsKey => this.GetField<bool>(CremaSchema.IsKey);

        public bool IsUnique => this.GetField<bool>(CremaSchema.IsUnique);

        public string Name => this.GetField<string>(CremaSchema.ColumnName);

        public string DataType => this.GetField<string>(CremaSchema.DataType);

        public string DefaultValue => this.GetField<string>(CremaSchema.DefaultValue);

        public string Comment => this.GetField<string>(CremaSchema.Comment);

        public bool AutoIncrement => this.GetField<bool>(CremaSchema.AutoIncrement);

        public TagInfo Tags => (TagInfo)(this.GetField<string>(CremaSchema.Tags));

        public bool IsReadOnly => this.GetField<bool>(CremaSchema.ReadOnly);

        public bool AllowNull => this.GetField<bool>(CremaSchema.AllowNull);

        public override CremaDispatcher Dispatcher => this.template.Dispatcher;

        public override DataBase DataBase => this.template.DataBase;

        public override CremaHost CremaHost => this.template.CremaHost;

        private void ValidateDispatcher(Authentication authentication)
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
        }

        #region ITableTemplate

        ITableTemplate ITableColumn.Template => this.template;

        #endregion
    }
}

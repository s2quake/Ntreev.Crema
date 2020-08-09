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

using Ntreev.Crema.Data.Properties;
using Ntreev.Crema.Data.Xml;
using Ntreev.Library;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Ntreev.Crema.Data
{
    public sealed class CremaTemplate : IListSource, IXmlSerializable, INotifyPropertyChanged, IDisposable
    {
        private readonly CremaTemplateColumnBuilder builder;
        private bool isEventsAttached;

        public CremaTemplate()
            : this(new CremaDataTable())
        {

        }

        public CremaTemplate(CremaDataTable targetTable)
        {
            if (targetTable == null)
                throw new ArgumentNullException(nameof(targetTable));
            if (targetTable.TemplateNamespace != string.Empty)
                throw new ArgumentException(Resources.Exception_CannotEditInheritedTable, nameof(targetTable));

            this.builder = new CremaTemplateColumnBuilder(this);
            this.InternalObject = new InternalTemplate(this, this.builder) { TargetTable = (InternalDataTable)targetTable };
            this.Attributes = new CremaAttributeCollection(this.InternalObject);
            this.Columns = new CremaTemplateColumnCollection(this.InternalObject);

            this.AttachEventHandlers();
        }

        public static CremaTemplate Create(CremaDataSet dataSet, string tableName, string categoryPath)
        {
            var dataTable = dataSet.Tables.Add(tableName, categoryPath);
            return new CremaTemplate(dataTable);
        }

        public static CremaTemplate Create(CremaDataTable parentTable)
        {
            var dataTable = parentTable.Childs.Add();
            return new CremaTemplate(dataTable);
        }

        public CremaTemplateColumn NewColumn()
        {
            var dataRow = this.InternalObject.NewRow() as InternalTemplateColumn;
            return dataRow.Target;
        }

        public CremaTemplateColumn AddColumn(string columnName)
        {
            return this.AddColumn(columnName, typeof(string).GetTypeName(), null);
        }

        public CremaTemplateColumn AddColumn(string columnName, Type type)
        {
            return this.AddColumn(columnName, type.GetTypeName(), null);
        }

        public CremaTemplateColumn AddColumn(string columnName, Type type, string comment)
        {
            return this.AddColumn(columnName, type.GetTypeName(), comment);
        }

        public CremaTemplateColumn AddColumn(string columnName, string dataTypeName)
        {
            return this.AddColumn(columnName, dataTypeName, null);
        }

        public CremaTemplateColumn AddColumn(string columnName, string dataTypeName, string comment)
        {
            var column = (this.InternalObject.NewRow() as InternalTemplateColumn).Target;
            column.Name = columnName;
            column.DataTypeName = dataTypeName;
            column.Comment = comment;

            this.Columns.Add(column);
            return column;
        }

        public void ImportColumn(CremaTemplateColumn column)
        {
            this.InternalObject.ImportRow((InternalTemplateColumn)column);
        }

        public void Dispose()
        {
            if (this.DataTable != null)
            {
                this.DetachEventHandlers();
            }
        }

        public void AcceptChanges()
        {
            this.InternalObject.AcceptChanges();
        }

        public void RejectChanges()
        {
            this.InternalObject.RejectChanges();
        }

        public bool HasChanges()
        {
            return this.HasChanges(false);
        }

        public bool HasChanges(bool isComparable)
        {
            for (var i = 0; i < this.InternalObject.Rows.Count; i++)
            {
                var dataRow = this.InternalObject.Rows[i];
                if (dataRow.RowState == DataRowState.Unchanged)
                    continue;
                if (isComparable == false)
                    return true;
                if (CompareDataRow(dataRow) == true)
                    return true;
            }

            return false;

            static bool CompareDataRow(DataRow dataRow)
            {
                if (dataRow.HasVersion(DataRowVersion.Original) == false)
                    return false;

                for (var i = 0; i < dataRow.Table.Columns.Count; i++)
                {
                    if (object.Equals(dataRow[i], dataRow[i, DataRowVersion.Original]) == false)
                        return true;
                }
                return false;
            }
        }

        internal void BeginLoadData()
        {
            this.InternalObject.BeginLoadData();
        }

        public void EndLoadData()
        {
            this.InternalObject.EndLoadData();
        }

        public void Validate()
        {
            if (this.Columns.Any() == false)
                throw new CremaDataException(Resources.Exception_AtLeastOneColumn);
            if (this.Columns.Any(item => item.IsKey) == false)
                throw new CremaDataException(Resources.Exception_AtLeastOneKey);
        }

        public string Comment
        {
            get => this.DataTable.Comment;
            set => this.DataTable.Comment = value;
        }

        public string ParentName
        {
            get
            {
                if (this.DataTable.Parent == null)
                    return string.Empty;
                return this.DataTable.Parent.TableName;
            }
        }

        public string TableName
        {
            get => this.DataTable.TableName;
            set
            {
                this.DataTable.TableName = value;
                this.InvokePropertyChangedEvent(nameof(this.TableName), nameof(this.Name), nameof(this.Namespace));
            }
        }

        public string Name => this.DataTable.Name;

        public string Namespace => this.DataTable.Namespace;

        public string CategoryPath
        {
            get => this.DataTable.CategoryPath;
            set => this.DataTable.CategoryPath = value;
        }

        public TagInfo Tags
        {
            get => this.DataTable.Tags;
            set => this.DataTable.Tags = value;
        }

        public TagInfo DerivedTags => this.DataTable.DerivedTags;

        public Guid TableID => this.DataTable.TableID;

        public CremaDataTable DataTable
        {
            get => (CremaDataTable)this.InternalObject.TargetTable;
            internal set => this.InternalObject.TargetTable = (InternalDataTable)value;
        }

        public CremaDataSet DataSet => this.DataTable?.DataSet;

        public bool IsModified { get; private set; }

        public CremaAttributeCollection Attributes { get; }

        public CremaTemplateColumnCollection Columns { get; }

        public string[] Types => this.InternalObject.Types;

        public DataView View => this.InternalObject.DefaultView;

        public SignatureDate ModificationInfo => this.DataTable.ModificationInfo;

        public SignatureDateProvider SignatureDateProvider
        {
            get => this.InternalObject.SignatureDateProvider;
            set => this.InternalObject.SignatureDateProvider = value;
        }

        public TagInfo ParentTags
        {
            get
            {
                if (this.DataTable.Parent == null)
                    return TagInfo.All;
                return this.DataTable.Parent.Tags;
            }
        }

        public TableInfo TableInfo => this.DataTable.TableInfo;

        public PropertyCollection ExtendedProperties => this.InternalObject.ExtendedProperties;

        public bool ReadOnly
        {
            get => this.InternalObject.ReadOnly;
            set
            {
                this.InternalObject.ReadOnly = value;
                this.InvokePropertyChangedEvent(nameof(this.ReadOnly));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event CremaTemplateColumnChangeEventHandler ColumnChanging;

        public event CremaTemplateColumnChangeEventHandler ColumnChanged;

        public event CremaTemplateColumnChangeEventHandler ColumnDeleted;

        public event CremaTemplateColumnChangeEventHandler ColumnDeleting;

        public event CremaTemplateClearEventHandler Cleared;

        public event CremaTemplateClearEventHandler Clearing;

        public event CremaTemplateNewColumnEventHandler TemplateNewColumn;

        private void OnColumnChanged(CremaTemplateColumnChangeEventArgs e)
        {
            this.ColumnChanged?.Invoke(this, e);
        }

        private void OnColumnChanging(CremaTemplateColumnChangeEventArgs e)
        {
            this.ColumnChanging?.Invoke(this, e);
        }

        private void OnColumnDeleted(CremaTemplateColumnChangeEventArgs e)
        {
            this.ColumnDeleted?.Invoke(this, e);
        }

        private void OnColumnDeleting(CremaTemplateColumnChangeEventArgs e)
        {
            this.ColumnDeleting?.Invoke(this, e);
        }

        private void OnCleared(CremaTemplateClearEventArgs e)
        {
            this.Cleared?.Invoke(this, e);
        }

        private void OnClearing(CremaTemplateClearEventArgs e)
        {
            this.Clearing?.Invoke(this, e);
        }

        private void OnTemplateNewColumn(CremaTemplateNewColumnEventArgs e)
        {
            this.TemplateNewColumn?.Invoke(this, e);
        }

        private void Table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            this.OnColumnChanged(new CremaTemplateColumnChangeEventArgs(e));
        }

        private void Table_ColumnChanging(object sender, DataColumnChangeEventArgs e)
        {
            this.OnColumnChanging(new CremaTemplateColumnChangeEventArgs(e));
        }

        private void Table_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            this.IsModified = true;
            this.OnColumnChanged(new CremaTemplateColumnChangeEventArgs(e));
        }

        private void Table_RowChanging(object sender, DataRowChangeEventArgs e)
        {
            this.OnColumnChanging(new CremaTemplateColumnChangeEventArgs(e));
        }

        private void Table_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            this.IsModified = true;
            this.OnColumnDeleted(new CremaTemplateColumnChangeEventArgs(e));
        }

        private void Table_RowDeleting(object sender, DataRowChangeEventArgs e)
        {
            this.OnColumnDeleting(new CremaTemplateColumnChangeEventArgs(e));
        }

        private void Table_TableCleared(object sender, DataTableClearEventArgs e)
        {
            this.IsModified = true;
            this.OnCleared(new CremaTemplateClearEventArgs(e));
        }

        private void Table_TableClearing(object sender, DataTableClearEventArgs e)
        {
            this.OnClearing(new CremaTemplateClearEventArgs(e));
        }

        private void Table_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            this.OnTemplateNewColumn(new CremaTemplateNewColumnEventArgs(e));
        }

        private void InvokePropertyChangedEvent(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        private void InvokePropertyChangedEvent(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                this.InvokePropertyChangedEvent(new PropertyChangedEventArgs(propertyName));
            }
        }

        internal CremaTemplateColumn InvokeNewTemplateColumnFromBuilder(CremaTemplateColumnBuilder builder)
        {
            return new CremaTemplateColumn(builder);
        }

        internal void AttachEventHandlers()
        {
            if (this.isEventsAttached == true)
                throw new Exception();
            this.InternalObject.ColumnChanging += Table_ColumnChanging;
            this.InternalObject.ColumnChanged += Table_ColumnChanged;
            this.InternalObject.RowChanging += Table_RowChanging;
            this.InternalObject.RowChanged += Table_RowChanged;
            this.InternalObject.RowDeleted += Table_RowDeleted;
            this.InternalObject.RowDeleting += Table_RowDeleting;
            this.InternalObject.TableCleared += Table_TableCleared;
            this.InternalObject.TableClearing += Table_TableClearing;
            this.InternalObject.TableNewRow += Table_TableNewRow;
            this.isEventsAttached = true;
        }

        internal void DetachEventHandlers()
        {
            if (this.isEventsAttached == false)
                throw new Exception();
            this.InternalObject.ColumnChanging -= Table_ColumnChanging;
            this.InternalObject.ColumnChanged -= Table_ColumnChanged;
            this.InternalObject.RowChanging -= Table_RowChanging;
            this.InternalObject.RowChanged -= Table_RowChanged;
            this.InternalObject.RowDeleted -= Table_RowDeleted;
            this.InternalObject.RowDeleting -= Table_RowDeleting;
            this.InternalObject.TableCleared -= Table_TableCleared;
            this.InternalObject.TableClearing -= Table_TableClearing;
            this.InternalObject.TableNewRow -= Table_TableNewRow;
            this.isEventsAttached = false;
        }

        internal InternalTemplate InternalObject { get; }

        internal bool IsDiffMode
        {
            get => this.InternalObject.IsDiffMode;
            set => this.InternalObject.IsDiffMode = value;
        }

        internal CremaTemplateColumnCollection Items => this.Columns;

        #region IListSource

        bool IListSource.ContainsListCollection => (this.InternalObject as IListSource).ContainsListCollection;

        System.Collections.IList IListSource.GetList()
        {
            return (this.InternalObject as IListSource).GetList();
        }

        #endregion

        #region IXmlSerializable

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            var dataSet = new CremaDataSet();

            var name = reader.GetAttribute(nameof(this.DataTable.Name));
            var categoryPath = reader.GetAttribute(nameof(this.DataTable.CategoryPath));

            reader.ReadStartElement();
            reader.MoveToContent();

            dataSet.ReadXmlSchema(reader);
            reader.MoveToContent();
            dataSet.ReadXml(reader);
            reader.MoveToContent();

            reader.ReadStartElement(nameof(this.Columns));
            reader.MoveToContent();

            this.InternalObject.InternalTargetTable = (InternalDataTable)dataSet.Tables[name, categoryPath];
            this.InternalObject.OmitSignatureDate = true;
            while (reader.NodeType == XmlNodeType.Element)
            {
                var column = this.NewColumn();
                this.ReadAttribute(reader, column);
                this.Read(reader, column);
                column.InternalObject.TargetColumn = (InternalDataColumn)this.InternalObject.InternalTargetTable.Columns[column.Name];
                this.Columns.Add(column);
            }
            reader.ReadEndElement();
            reader.MoveToContent();
            this.InternalObject.OmitSignatureDate = false;
        }

        private void ReadAttribute(XmlReader reader, CremaTemplateColumn column)
        {
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                var attribute = this.Attributes[reader.Name].InternalAttribute;
                var value = CremaXmlConvert.ToValue(reader.Value, attribute.DataType);
                column.InternalObject[attribute] = value;
            }
            reader.MoveToContent();
        }

        private void Read(XmlReader reader, CremaTemplateColumn column)
        {
            reader.ReadStartElement("Item");
            reader.MoveToContent();
            while (reader.NodeType == XmlNodeType.Element)
            {

                var dataColumn = this.InternalObject.Columns[reader.Name];
                reader.ReadStartElement();
                reader.MoveToContent();
                var value = CremaXmlConvert.ToValue(reader.ReadContentAsString(), dataColumn.DataType);
                reader.MoveToContent();
                reader.ReadEndElement();
                column.InternalObject[dataColumn] = value;
                reader.MoveToContent();
            }
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(this.DataTable.Name), this.DataTable.Name);
            writer.WriteAttributeString(nameof(this.DataTable.CategoryPath), this.DataTable.CategoryPath);

            this.DataSet.WriteXmlSchema(writer);
            this.DataSet.WriteXml(writer);

            var columns = this.Columns.OrderBy(item => item.Index);
            writer.WriteStartElement(nameof(this.Columns));
            foreach (var item in columns)
            {
                writer.WriteStartElement("Item");
                foreach (var attr in this.Attributes)
                {
                    if (attr.IsVisible == false)
                        continue;
                    this.WriteAttribute(writer, item.InternalObject, attr.InternalAttribute);
                }
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnIsKey);
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnColumnName);
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnDataType);
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnComment);
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnDefaultValue);
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnAllowNull);
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnReadOnly);
                this.WriteField(writer, item.InternalObject, this.InternalObject.columnIsUnique);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteAttribute(XmlWriter writer, InternalRowBase dataRow, InternalAttribute dataColumn)
        {
            var field = dataRow[dataColumn];
            if (field == DBNull.Value || object.Equals(field, dataColumn.DefaultValue) == true)
                return;

            var textValue = CremaXmlConvert.ToString(field, dataColumn.DataType);
            if (string.IsNullOrEmpty(textValue) == false)
            {
                writer.WriteStartAttribute(dataColumn.AttributeName);
                writer.WriteValue(textValue);
                writer.WriteEndAttribute();
            }
        }

        private void WriteField(XmlWriter writer, InternalTemplateColumn dataRow, DataColumn dataColumn)
        {
            var dataType = dataColumn.DataType;
            var value = dataRow[dataColumn];
            if (value == DBNull.Value)
                return;

            var textValue = CremaXmlConvert.ToString(value, dataType);
            if (object.Equals(value, dataColumn.DefaultValue) == false)
            {
                writer.WriteStartElement(dataColumn.ColumnName);
                writer.WriteValue(textValue);
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}

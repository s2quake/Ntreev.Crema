// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Crema.Data;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace JSSoft.Crema.Runtime.Serialization.Binary
{
    [Export(typeof(IDataSerializer))]
    class BinaryDataSerializer : IDataSerializer
    {
        public const int formatterVersion = 0;

        private BinaryTableHeader tableHeader = new BinaryTableHeader();
        private BinaryTableInfo tableInfo = new BinaryTableInfo();
        private readonly HashSet<string> strings = new HashSet<string>();
        private List<BinaryColumnInfo> columns;

        [ImportingConstructor]
        public BinaryDataSerializer()
        {

        }

        public string Name => "bin";

        public void Serialize(Stream stream, SerializationSet dataSet)
        {
            var fileHeader = new BinaryFileHeader();
            var tables = dataSet.Tables;
            var tableIndexes = new List<BinaryTableIndex>(tables.Length);

            var writer = new BinaryWriter(stream);
            writer.WriteValue(fileHeader);

            fileHeader.MagicValue = BinaryFileHeader.DefaultMagicValue;
            fileHeader.IndexOffset = stream.Position;

            writer.Seek(Marshal.SizeOf(typeof(BinaryTableIndex)) * tables.Length, SeekOrigin.Current);
            fileHeader.TypesHashValue = this.GetStringID(dataSet.TypesHashValue);
            fileHeader.TablesHashValue = this.GetStringID(dataSet.TablesHashValue);
            fileHeader.Tags = this.GetStringID((string)dataSet.Tags);
            fileHeader.TablesOffset = stream.Position;
            fileHeader.TableCount = tables.Length;
            fileHeader.Revision = this.GetStringID(dataSet.Revision);
            fileHeader.Name = this.GetStringID(dataSet.Name);

            var t = new Dictionary<string, Stream>();

            Parallel.ForEach(tables, item =>
            {
                var memory = new MemoryStream();
                var formatter = new BinaryDataSerializer();
                formatter.SerializeTable(memory, item, dataSet.Types);
                memory.Position = 0;
                lock (t)
                {
                    t.Add(item.Name, memory);
                }
            });

            foreach (var item in tables)
            {
                var tableIndex = new BinaryTableIndex()
                {
                    TableName = this.GetStringID(item.Name),
                    Offset = stream.Position
                };
                tableIndexes.Add(tableIndex);
                t[item.Name].CopyTo(stream);
            }

            fileHeader.StringResourcesOffset = stream.Position;
            writer.WriteResourceStrings(this.strings.ToArray());

            writer.Seek(0, SeekOrigin.Begin);
            writer.WriteValue(fileHeader);

            writer.Seek((int)fileHeader.IndexOffset, SeekOrigin.Begin);
            writer.WriteArray(tableIndexes.ToArray());
        }

        private void SerializeTable(Stream stream, SerializationTable dataTable, SerializationType[] types)
        {
            var columns = dataTable.Columns;
            var rows = dataTable.Rows;
            this.tableHeader.MagicValue = BinaryTableHeader.DefaultMagicValue;
            this.tableHeader.HashValue = this.GetStringID(dataTable.HashValue);

            this.tableInfo.TableName = this.GetStringID(dataTable.Name);
            this.tableInfo.CategoryName = this.GetStringID(dataTable.CategoryPath);
            this.tableInfo.ColumnsCount = columns.Length;
            this.tableInfo.RowsCount = rows.Length;

            this.CollectColumns(columns);

            var writer = new BinaryWriter(stream);

            writer.WriteValue(this.tableHeader);

            this.tableHeader.TableInfoOffset = writer.GetPosition();
            writer.WriteValue(this.tableInfo);
            this.tableHeader.ColumnsOffset = writer.GetPosition();

            writer.WriteArray(this.columns.ToArray());

            this.tableHeader.RowsOffset = writer.GetPosition();
            this.WriteRows(writer, rows, columns, types);

            this.tableHeader.StringResourcesOffset = writer.GetPosition();
            writer.WriteResourceStrings(this.strings.ToArray());

            this.tableHeader.UserOffset = writer.GetPosition();
            writer.Write((byte)0);

            var lastPosition = writer.GetPosition();
            writer.Seek(0, SeekOrigin.Begin);
            writer.WriteValue(this.tableHeader);
            writer.Seek((int)this.tableHeader.TableInfoOffset, SeekOrigin.Begin);
            writer.WriteValue(this.tableInfo);
            writer.SetPosition(lastPosition);
        }

        private void CollectColumns(SerializationColumn[] columns)
        {
            this.columns = new List<BinaryColumnInfo>(columns.Length);

            foreach (var item in columns)
            {
                var columnInfo = new BinaryColumnInfo()
                {
                    ColumnName = this.GetStringID(item.Name),
                    DataType = this.GetStringID(item.DataType),
                    IsKey = item.IsKey ? 1 : 0
                };
                this.columns.Add(columnInfo);
            }
        }

        private void WriteRows(BinaryWriter writer, SerializationRow[] rows, SerializationColumn[] columns, SerializationType[] types)
        {
            foreach (var item in rows)
            {
                this.WriteRow(writer, item, columns, types);
            }
        }

        private void WriteRow(BinaryWriter writer, SerializationRow dataRow, SerializationColumn[] columns, SerializationType[] types)
        {
            var headerPosition = writer.GetPosition();
            var valueOffsets = new int[columns.Length];
            var byteLength = 0;
            writer.Write(byteLength);
            var fieldPosition = writer.GetPosition();
            writer.WriteArray(valueOffsets);

            for (var i = 0; i < columns.Length; i++)
            {
                var dataColumn = columns[i];
                var value = dataRow.Fields[i];

                if (value == null || value == DBNull.Value)
                    continue;

                valueOffsets[i] = (int)(writer.GetPosition() - fieldPosition);
                this.WriteField(writer, dataColumn, value, types);
            }

            var lastPosition = writer.GetPosition();
            byteLength = (int)(lastPosition - fieldPosition);

            writer.SetPosition(headerPosition);
            writer.WriteValue(byteLength);
            writer.WriteArray(valueOffsets);
            writer.SetPosition(lastPosition);
        }

        private void WriteField(BinaryWriter writer, SerializationColumn dataColumn, object value, SerializationType[] types)
        {
            if (dataColumn.DataType == typeof(bool).GetTypeName())
            {
                writer.Write((bool)value);
            }
            else if (dataColumn.DataType == typeof(string).GetTypeName())
            {
                writer.Write(this.GetStringID(value as string));
            }
            else if (dataColumn.DataType == typeof(float).GetTypeName())
            {
                writer.Write((float)value);
            }
            else if (dataColumn.DataType == typeof(double).GetTypeName())
            {
                writer.Write((double)value);
            }
            else if (dataColumn.DataType == typeof(sbyte).GetTypeName())
            {
                writer.Write((sbyte)value);
            }
            else if (dataColumn.DataType == typeof(byte).GetTypeName())
            {
                writer.Write((byte)value);
            }
            else if (dataColumn.DataType == typeof(int).GetTypeName())
            {
                writer.Write((int)value);
            }
            else if (dataColumn.DataType == typeof(uint).GetTypeName())
            {
                writer.Write((uint)value);
            }
            else if (dataColumn.DataType == typeof(short).GetTypeName())
            {
                writer.WriteValue((short)value);
            }
            else if (dataColumn.DataType == typeof(ushort).GetTypeName())
            {
                writer.WriteValue((ushort)value);
            }
            else if (dataColumn.DataType == typeof(long).GetTypeName())
            {
                writer.Write((long)value);
            }
            else if (dataColumn.DataType == typeof(ulong).GetTypeName())
            {
                writer.Write((ulong)value);
            }
            else if (dataColumn.DataType == typeof(DateTime).GetTypeName())
            {
                var dateTime = (DateTime)value;
                var delta = dateTime - new DateTime(1970, 1, 1);
                var seconds = Convert.ToInt64(delta.TotalSeconds);
                writer.Write(seconds);
            }
            else if (dataColumn.DataType == typeof(TimeSpan).GetTypeName())
            {
                if (value is TimeSpan timeSpanValue)
                    writer.Write(timeSpanValue.Ticks);
                else if (value is long longValue)
                    writer.Write(longValue);
                else
                    throw new InvalidOperationException($"{value.GetType()} is invalid timespan type");
            }
            else if (dataColumn.DataType == typeof(Guid).GetTypeName())
            {
                var bytes = ((Guid)value).ToByteArray();
                writer.Write(bytes);
            }
            else if (dataColumn.DataType == typeof(string).GetTypeName())
            {
                writer.Write(this.GetStringID(value.ToString()));
            }
            else
            {
                var type = types.First(item => dataColumn.DataType == item.CategoryPath + item.Name);
                writer.Write((long)type.ConvertFromString(value.ToString()));
            }
        }

        private int GetStringID(string text)
        {
            lock (this.strings)
            {
                text = text.Replace(Environment.NewLine, "\n");
                if (this.strings.Contains(text) == false)
                    this.strings.Add(text);
                return text.GetHashCode();
            }
        }
    }
}

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

using Ntreev.Crema.Reader.Binary;
using Ntreev.Crema.Reader.Internal;
using Ntreev.Crema.Reader.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Ntreev.Crema.Reader
{
    [Flags]
    public enum ReadOptions
    {
        None = 0,
        [Obsolete]
        LazyLoading = 1,
        CaseSensitive = 2,

        Mask = 0xff,
    }

    public interface IColumn
    {
        ITable Table { get; }

        string Name { get; }

        Type DataType { get; }

        bool IsKey { get; }

        int Index { get; }
    }

    public interface IColumnCollection : IEnumerable<IColumn>, ICollection, IEnumerable
    {
        IColumn this[int index] { get; }

        IColumn this[string columnName] { get; }

        ITable Table { get; }
    }

    public interface IDataSet
    {
        ITableCollection Tables { get; }

        string Revision { get; }

        string TypesHashValue { get; }

        string TablesHashValue { get; }

        string Tags { get; }

        string Name { get; }
    }

    public interface IRow
    {
        ITable Table { get; }

        object this[string columnName] { get; }

        object this[int columnIndex] { get; }

        object this[IColumn column] { get; }

        bool HasValue(string columnName);

        bool HasValue(int columnIndex);

        bool HasValue(IColumn column);
    }

    public interface IRowCollection : IEnumerable<IRow>, ICollection, IEnumerable
    {
        IRow this[int index] { get; }

        ITable Table { get; }
    }

    public interface ITable
    {
        string Category { get; }

        string Name { get; }

        int Index { get; }

        string HashValue { get; }

        IDataSet DataSet { get; }

        IColumn[] Keys { get; }

        IColumnCollection Columns { get; }

        IRowCollection Rows { get; }
    }

    public interface ITableCollection : IEnumerable<ITable>, ICollection, IEnumerable
    {
        ITable this[int index] { get; }

        ITable this[string tableName] { get; }

        string[] TableNames { get; }

        bool Contains(string tableName);

        [Obsolete]
        bool IsTableLoaded(string tableName);
    }

    public class CremaReader
    {
        public static IDataSet Read(string ipAddress, int port)
        {
            return Read(ipAddress, port, "master");
        }

        public static IDataSet Read(string ipAddress, int port, string dataBase)
        {
            return Read(ipAddress, port, dataBase, "all");
        }

        public static IDataSet Read(string ipAddress, int port, string dataBase, string tags)
        {
            return Read(ipAddress, port, dataBase, tags, string.Empty);
        }

        public static IDataSet Read(string ipAddress, int port, string dataBase, string tags, string filterExpression)
        {
            return Read(ipAddress, port, dataBase, tags, filterExpression, false);
        }

        public static IDataSet Read(string ipAddress, int port, string dataBase, string tags, string filterExpression, bool isDevmode)
        {
            if (dataBase == null)
                throw new ArgumentNullException("database");
            if (tags == null)
                throw new ArgumentNullException("tags");
            if (filterExpression == null)
                throw new ArgumentNullException("filterExpression");
            var dic = new Dictionary<string, object>();

            dic.Add("type", "bin");
            dic.Add("tags", tags);
            dic.Add("database", dataBase);
            dic.Add("devmode", isDevmode);

            if (filterExpression != string.Empty)
                dic.Add("filter", filterExpression);

            var items = dic.Select(i => string.Format("{0}=\"{1}\"", i.Key, i.Value)).ToArray();
            var name = string.Join(";", items);

            using (var stream = new RemoteStream(ipAddress, port + 1, name))
            {
                return CremaReader.Read(stream);
            }
        }

        public static IDataSet Read(string filename)
        {
            return Read(filename, ReadOptions.None);
        }

        public static IDataSet Read(string filename, ReadOptions options)
        {
            Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return CremaReader.Read(stream, options);
        }

        public static IDataSet Read(Stream stream)
        {
            return Read(stream, ReadOptions.None);
        }

        public static IDataSet Read(Stream stream, ReadOptions options)
        {
            var reader = new BinaryReader(stream);
            var magicValue = reader.ReadUInt32();

            if (magicValue == FileHeader.defaultMagicValue)
            {
                var binaryReader = new CremaBinaryReader();
                binaryReader.Read(stream, options);
                return binaryReader;
            }

            throw new NotSupportedException("지원되지 않은 형식입니다.");
        }
    }

    public static class ReaderExtensions
    {
        public const string __RelationID__ = "__RelationID__";
        public const string __ParentID__ = "__ParentID__";

        public static IRow Find(this IRowCollection rows, params object[] keyValues)
        {
            var keys = rows.Table.Keys;
            if (keys.Length != keyValues.Length)
                throw new ArgumentException("키 갯수가 맞지 않습니다.", "keyValues");

            var query = from item in rows
                        where IsEqual(item.GetKeys(), keyValues)
                        select item;

            return query.Single();
        }

        public static IRow GetParentRow(this IRow row)
        {
            var parentTable = row.Table.GetParentTable();
            if (parentTable == null)
                return null;

            var query = from item in parentTable.Rows
                        where item.ToInt32(__RelationID__) == row.ToInt32(__ParentID__)
                        select item;

            return query.Single();
        }

        public static IEnumerable<IRow> GetChildRows(this IRow row, ITable childTable)
        {
            if (row.Table != childTable.GetParentTable())
                throw new ArgumentException("잘못된 자식 테이블입니다.");

            return from item in childTable.Rows
                   where row.ToInt32(__RelationID__) == item.ToInt32(__ParentID__)
                   select item;
        }

        public static IEnumerable<IRow> GetChildRows(this IRow row, string childTableName)
        {
            if (row.Table.DataSet.Tables.Contains(childTableName) == false)
                throw new ArgumentException("존재하지 않은 테이블입니다.");

            var childTable = row.Table.DataSet.Tables[childTableName];
            if (row.Table != childTable.GetParentTable())
                throw new ArgumentException("잘못된 자식 테이블입니다.");

            return from item in childTable.Rows
                   where row.ToInt32(__RelationID__) == item.ToInt32(__ParentID__)
                   select item;
        }

        public static IEnumerable<ITable> GetChildTables(this ITable table)
        {
            return from item in table.DataSet.Tables
                   where GetParentName(item.Name) == table.Name
                   select item;
        }

        public static ITable GetParentTable(this ITable table)
        {
            var parentName = GetParentName(table.Name);
            if (parentName == string.Empty)
                return null;
            return table.DataSet.Tables[parentName];
        }

        public static T Field<T>(this IRow row, string columnName)
        {
            return row.Field<T>(row.Table.Columns[columnName]);
        }

        public static T Field<T>(this IRow row, int columnIndex)
        {
            return row.Field<T>(row.Table.Columns[columnIndex]);
        }

        public static T Field<T>(this IRow row, IColumn column)
        {
            return (T)row[column];
        }

        public static object GetValue(this IRow row, int columnIndex)
        {
            return row[columnIndex];
        }

        public static object GetValue(this IRow row, IColumn column)
        {
            return row[column];
        }

        public static object GetValue(this IRow row, string columnName)
        {
            return row[columnName];
        }

        public static bool ToBoolean(this IRow row, string columnName)
        {
            return (bool)row[columnName];
        }

        public static bool ToBoolean(this IRow row, int columnIndex)
        {
            return (bool)row[columnIndex];
        }

        public static bool ToBoolean(this IRow row, IColumn column)
        {
            return (bool)row[column];
        }

        public static string ToString(this IRow row, string columnName)
        {
            return (string)row[columnName];
        }

        public static string ToString(this IRow row, int columnIndex)
        {
            return (string)row[columnIndex];
        }

        public static string ToString(this IRow row, IColumn column)
        {
            return (string)row[column];
        }

        public static float ToSingle(this IRow row, string columnName)
        {
            return (float)row[columnName];
        }

        public static float ToSingle(this IRow row, int columnIndex)
        {
            return (float)row[columnIndex];
        }

        public static float ToSingle(this IRow row, IColumn column)
        {
            return (float)row[column];
        }

        public static double ToDouble(this IRow row, string columnName)
        {
            return (double)row[columnName];
        }

        public static double ToDouble(this IRow row, int columnIndex)
        {
            return (double)row[columnIndex];
        }

        public static double ToDouble(this IRow row, IColumn column)
        {
            return (double)row[column];
        }

        public static sbyte ToInt8(this IRow row, string columnName)
        {
            return (sbyte)row[columnName];
        }

        public static sbyte ToInt8(this IRow row, int columnIndex)
        {
            return (sbyte)row[columnIndex];
        }

        public static sbyte ToInt8(this IRow row, IColumn column)
        {
            return (sbyte)row[column];
        }

        public static byte ToUInt8(this IRow row, string columnName)
        {
            return (byte)row[columnName];
        }

        public static byte ToUInt8(this IRow row, int columnIndex)
        {
            return (byte)row[columnIndex];
        }

        public static byte ToUInt8(this IRow row, IColumn column)
        {
            return (byte)row[column];
        }

        public static short ToInt16(this IRow row, string columnName)
        {
            return (short)row[columnName];
        }

        public static short ToInt16(this IRow row, int columnIndex)
        {
            return (short)row[columnIndex];
        }

        public static short ToInt16(this IRow row, IColumn column)
        {
            return (short)row[column];
        }

        public static ushort ToUInt16(this IRow row, string columnName)
        {
            return (ushort)row[columnName];
        }

        public static ushort ToUInt16(this IRow row, int columnIndex)
        {
            return (ushort)row[columnIndex];
        }

        public static ushort ToUInt16(this IRow row, IColumn column)
        {
            return (ushort)row[column];
        }

        public static int ToInt32(this IRow row, string columnName)
        {
            return (int)row[columnName];
        }

        public static int ToInt32(this IRow row, int columnIndex)
        {
            return (int)row[columnIndex];
        }

        public static int ToInt32(this IRow row, IColumn column)
        {
            return (int)row[column];
        }

        public static uint ToUInt32(this IRow row, string columnName)
        {
            return (uint)row[columnName];
        }

        public static uint ToUInt32(this IRow row, int columnIndex)
        {
            return (uint)row[columnIndex];
        }

        public static uint ToUInt32(this IRow row, IColumn column)
        {
            return (uint)row[column];
        }

        public static long ToInt64(this IRow row, string columnName)
        {
            return (long)row[columnName];
        }

        public static long ToInt64(this IRow row, int columnIndex)
        {
            return (long)row[columnIndex];
        }

        public static long ToInt64(this IRow row, IColumn column)
        {
            return (long)row[column];
        }

        public static ulong ToUInt64(this IRow row, string columnName)
        {
            return (ulong)row[columnName];
        }

        public static ulong ToUInt64(this IRow row, int columnIndex)
        {
            return (ulong)row[columnIndex];
        }

        public static ulong ToUInt64(this IRow row, IColumn column)
        {
            return (ulong)row[column];
        }

        public static DateTime ToDateTime(this IRow row, string columnName)
        {
            return (DateTime)row[columnName];
        }

        public static DateTime ToDateTime(this IRow row, int columnIndex)
        {
            return (DateTime)row[columnIndex];
        }

        public static DateTime ToDateTime(this IRow row, IColumn column)
        {
            return (DateTime)row[column];
        }

        public static TimeSpan ToDuration(this IRow row, string columnName)
        {
            return (TimeSpan)row[columnName];
        }

        public static TimeSpan ToDuration(this IRow row, int columnIndex)
        {
            return (TimeSpan)row[columnIndex];
        }

        public static TimeSpan ToDuration(this IRow row, IColumn column)
        {
            return (TimeSpan)row[column];
        }

        public static Guid ToGuid(this IRow row, string columnName)
        {
            return (Guid)row[columnName];
        }

        public static Guid ToGuid(this IRow row, int columnIndex)
        {
            return (Guid)row[columnIndex];
        }

        public static Guid ToDurToGuidation(this IRow row, IColumn column)
        {
            return (Guid)row[column];
        }

        public static IEnumerable<object> GetKeys(this IRow row)
        {
            return row.Table.Keys.Select(item => row[item]).ToArray();
        }

        public static DateTime FromTotalSeconds(long seconds)
        {
            var delta = TimeSpan.FromSeconds(Convert.ToDouble(seconds));
            return new DateTime(1970, 1, 1) + delta;
        }

        private static bool IsEqual(IEnumerable<object> k1, object[] k2)
        {
            return k1.SequenceEqual(k2);
        }

        public static string GetPureName(this ITable table)
        {
            var tableName = table.Name;
            var value = tableName.Split('.');
            if (value.Length == 1)
                return tableName;
            return value[1];
        }

        private static string GetParentName(string name)
        {
            var value = name.Split('.');
            if (value.Length == 1)
                return string.Empty;
            return value[0];
        }
    }

    namespace IO
    {
        static class BinaryWriterExtension
        {
            public static void WriteValue<T>(this BinaryWriter writer, T value)
                where T : struct
            {
                byte[] bytes = BinaryWriterExtension.GetBytes<T>(value);
                writer.Write(bytes, 0, bytes.Length);
            }

            public static byte[] GetBytes<TStruct>(TStruct data)
                where TStruct : struct
            {
                int structSize = Marshal.SizeOf(typeof(TStruct));
                byte[] buffer = new byte[structSize];
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                Marshal.StructureToPtr(data, handle.AddrOfPinnedObject(), false);
                handle.Free();
                return buffer;
            }
        }

        static class BinaryReaderExtension
        {
            public static long Seek(this BinaryReader reader, long offset, SeekOrigin origin)
            {
                return reader.BaseStream.Seek(offset, origin);
            }

            public static T[] ReadValues<T>(this BinaryReader reader, int count)
                where T : struct
            {
                List<T> list = new List<T>(count);
                for (int i = 0; i < count; i++)
                {
                    T value;
                    reader.ReadValue(out value);
                    list.Add(value);
                }
                return list.ToArray();
            }

#if UNITY_WEBPLAYER
        public static void ReadValue<T>(this BinaryReader reader, out T value)
            where T : struct
        {
            int size = SizeOf(typeof(T));
            byte[] bytes = new byte[size];
            int readSize = reader.Read(bytes, 0, size);
            if (readSize != size)
                throw new Exception();
            PtrToStructure(bytes, out value);
        }

         private static void PtrToStructure<T>(byte[] bytes, out T ptr)
             where T : struct
        {
            ptr = new T();
            object box = ptr;
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(ptr);

            var query = from PropertyDescriptor item in TypeDescriptor.GetProperties(ptr)
                        select SizeOfPrimitiveType(item.PropertyType);

            int maxSizeOfType = query.Max();

            int offset = 0;
            foreach (PropertyDescriptor prop in props)
            {

                int typeSize = SizeOfPrimitiveType(prop.PropertyType);
                Type type = prop.PropertyType;

                if (offset + typeSize > maxSizeOfType)
                {
                    offset += (offset % maxSizeOfType);
                }

                if (type == typeof(bool))
                    prop.SetValue(box, BitConverter.ToBoolean(bytes, offset));
                else if (type == typeof(char))
                    prop.SetValue(box, BitConverter.ToChar(bytes, offset));
                else if (type == typeof(byte))
                    prop.SetValue(box, BitConverter.ToChar(bytes, offset));
                else if (type == typeof(sbyte))
                    prop.SetValue(box, BitConverter.ToChar(bytes, offset));
                else if (type == typeof(short))
                    prop.SetValue(box, BitConverter.ToInt16(bytes, offset));
                else if (type == typeof(ushort))
                    prop.SetValue(box, BitConverter.ToUInt16(bytes, offset));
                else if (type == typeof(int))
                    prop.SetValue(box, BitConverter.ToInt32(bytes, offset));
                else if (type == typeof(uint))
                    prop.SetValue(box, BitConverter.ToUInt32(bytes, offset));
                else if (type == typeof(long))
                    prop.SetValue(box, BitConverter.ToInt64(bytes, offset));
                else if (type == typeof(ulong))
                    prop.SetValue(box, BitConverter.ToUInt64(bytes, offset));
                else if (type == typeof(float))
                    prop.SetValue(box, BitConverter.ToSingle(bytes, offset));
                else if (type == typeof(double))
                    prop.SetValue(box, BitConverter.ToDouble(bytes, offset));

                offset += typeSize;
            }

            ptr = (T)box;
        }

        private static int SizeOf(Type type)
        {
            var query = from PropertyDescriptor item in TypeDescriptor.GetProperties(type)
                        select SizeOfPrimitiveType(item.PropertyType);

            int maxSizeOfType = query.Max();

            int size = 0;
            foreach (var item in query)
            {
                if (size + item > maxSizeOfType)
                {
                    size += (size % maxSizeOfType);
                }
                size += item;
            }

            return size;
        }

        private static int SizeOfPrimitiveType(Type type)
        {
            if (type == typeof(bool))
                return sizeof(bool);
            else if (type == typeof(char))
                return sizeof(char);
            else if (type == typeof(byte))
                return sizeof(byte);
            else if (type == typeof(sbyte))
                return sizeof(sbyte);
            else if (type == typeof(short))
                return sizeof(short);
            else if (type == typeof(ushort))
                return sizeof(ushort);
            else if (type == typeof(int))
                return sizeof(int);
            else if (type == typeof(uint))
                return sizeof(uint);
            else if (type == typeof(long))
                return sizeof(long);
            else if (type == typeof(ulong))
                return sizeof(ulong);
            else if (type == typeof(float))
                return sizeof(float);
            else if (type == typeof(double))
                return sizeof(double);

            throw new NotSupportedException();
        }
#else
            public static void ReadValue<T>(this BinaryReader reader, out T value)
                where T : struct
            {
                int size = Marshal.SizeOf(typeof(T));
                byte[] bytes = new byte[size];
                int readSize = reader.Read(bytes, 0, size);
                if (readSize != size)
                    throw new Exception();
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                value = (T)Marshal.PtrToStructure(ptr, typeof(T));
                Marshal.FreeHGlobal(ptr);
            }

            public static T ReadValue<T>(this BinaryReader reader)
                where T : struct
            {
                int size = Marshal.SizeOf(typeof(T));
                byte[] bytes = new byte[size];
                int readSize = reader.Read(bytes, 0, size);
                if (readSize != size)
                    throw new Exception();
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                T value = (T)Marshal.PtrToStructure(ptr, typeof(T));
                Marshal.FreeHGlobal(ptr);
                return value;
            }
#endif
        }
    }

    namespace Internal
    {
        class RemoteStream : MemoryStream
        {
            private const int bufferLength = 1024 * 5000;
            //private readonly TcpClient client;
            //private long position;
            //private long length;
            //private byte[] buffer;
            //private long bufferpos;

            public RemoteStream(string ipAddress, int port, string name)
            {
                var client = new TcpClient(ipAddress, port);

                var stream = client.GetStream();

                var writer = new BinaryWriter(stream);
                var reader = new BinaryReader(stream);

                writer.Write((int)HeaderType.Size);
                writer.Write(name);

                var length = (int)reader.ReadInt64();
                this.Capacity = length;
                var buffer = new byte[bufferLength];

                var len = 0;
                while (len < length)
                {
                    var bufferInfo = new BufferInfo(len, bufferLength);
                    //bufferInfo.pos = pos;
                    //bufferInfo.size = SocketStream.bufferLength;
                    //bufferInfo.dummy = 0;

                    writer.Write((int)HeaderType.Buffer);
                    writer.WriteValue(bufferInfo);

                    var read = reader.Read(buffer, 0, bufferLength);
                    this.Write(buffer, 0, read);
                    len += read;
                }
                client.Close();
                this.Position = 0;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            //public override long Length
            //{
            //    get { return this.length; }
            //}

            //public override long Position
            //{
            //    get
            //    {
            //        return this.position;
            //    }
            //    set
            //    {
            //        this.position = value;
            //    }
            //}

            //public override int Read(byte[] buffer, int offset, int count)
            //{
            //    int readCount = 0;
            //    for (int i = 0; i < count; i++)
            //    {
            //        if (this.buffer == null || this.position - this.bufferpos + i >= this.buffer.Length)
            //        {
            //            this.SocketRead(this.position);
            //        }
            //        byte b = this.buffer[this.position - this.bufferpos + i];
            //        buffer[offset + i] = b;
            //        readCount++;
            //    }

            //    this.position += readCount;

            //    return readCount;
            //}

            //public override long Seek(long offset, SeekOrigin origin)
            //{
            //    long p = this.position;
            //    switch (origin)
            //    {
            //        case SeekOrigin.Begin:
            //            p = offset;
            //            break;
            //        case SeekOrigin.Current:
            //            p += offset;
            //            break;
            //        case SeekOrigin.End:
            //            p = this.length - offset;
            //            break;
            //    }
            //    this.position = p;

            //    if ((this.buffer != null && this.position - this.bufferpos >= this.buffer.Length) || this.position < this.bufferpos)
            //    {
            //        this.buffer = null;
            //    }
            //    return p;
            //}

            //public override void SetLength(long value)
            //{
            //    throw new NotImplementedException();
            //}

            //public override void Write(byte[] buffer, int offset, int count)
            //{
            //    throw new NotImplementedException();
            //}

            //protected override void Dispose(bool disposing)
            //{
            //    base.Dispose(disposing);

            //    if (this.stream != null)
            //    {
            //        this.stream.Close();
            //        this.client.Close();
            //        this.stream = null;
            //    }
            //}

            //private int SocketRead(long pos)
            //{
            //    BufferInfo bufferInfo = new BufferInfo(pos, SocketStream.bufferLength);
            //    //bufferInfo.pos = pos;
            //    //bufferInfo.size = SocketStream.bufferLength;
            //    //bufferInfo.dummy = 0;

            //    this.bw.Write((int)HeaderType.Buffer);
            //    this.bw.WriteValue(bufferInfo);

            //    this.buffer = this.br.ReadBytes(SocketStream.bufferLength);
            //    this.bufferpos = pos;
            //    return this.buffer.Length;
            //}

            #region classes

            enum HeaderType
            {
                Identify,
                Size,
                Buffer,
                Compare,
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            struct BufferInfo
            {
                public BufferInfo(long pos, int size)
                    : this()
                {
                    this.pos = pos;
                    this.size = size;
                    //this.dummy = 0;
                }
                public long pos;
                public int size;
                public int dummy;
            }

            #endregion
        }

        class SocketStream : Stream
        {
            private const int bufferLength = 1024 * 5000;
            private readonly TcpClient client;
            private long position;
            private long length;
            private byte[] buffer;
            private long bufferpos;

            private NetworkStream stream;
            private BinaryWriter bw;
            private BinaryReader br;

            public SocketStream(string ipAddress, int port, string name)
            {
                this.client = new TcpClient(ipAddress, port);

                this.stream = this.client.GetStream();

                this.bw = new BinaryWriter(this.stream);
                this.br = new BinaryReader(this.stream);

                this.bw.Write((int)HeaderType.Size);
                this.bw.Write(name);

                this.length = br.ReadInt64();
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override long Length
            {
                get { return this.length; }
            }

            public override long Position
            {
                get
                {
                    return this.position;
                }
                set
                {
                    this.position = value;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int readCount = 0;
                for (int i = 0; i < count; i++)
                {
                    if (this.buffer == null || this.position - this.bufferpos + i >= this.buffer.Length)
                    {
                        this.SocketRead(this.position);
                    }
                    byte b = this.buffer[this.position - this.bufferpos + i];
                    buffer[offset + i] = b;
                    readCount++;
                }

                this.position += readCount;

                return readCount;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long p = this.position;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        p = offset;
                        break;
                    case SeekOrigin.Current:
                        p += offset;
                        break;
                    case SeekOrigin.End:
                        p = this.length - offset;
                        break;
                }
                this.position = p;

                if ((this.buffer != null && this.position - this.bufferpos >= this.buffer.Length) || this.position < this.bufferpos)
                {
                    this.buffer = null;
                }
                return p;
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (this.stream != null)
                {
                    this.stream.Close();
                    this.client.Close();
                    this.stream = null;
                }
            }

            private int SocketRead(long pos)
            {
                BufferInfo bufferInfo = new BufferInfo(pos, SocketStream.bufferLength);
                //bufferInfo.pos = pos;
                //bufferInfo.size = SocketStream.bufferLength;
                //bufferInfo.dummy = 0;

                this.bw.Write((int)HeaderType.Buffer);
                this.bw.WriteValue(bufferInfo);

                this.buffer = this.br.ReadBytes(SocketStream.bufferLength);
                this.bufferpos = pos;
                return this.buffer.Length;
            }

            #region classes

            enum HeaderType
            {
                Identify,
                Size,
                Buffer,
                Compare,
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            struct BufferInfo
            {
                public BufferInfo(long pos, int size)
                    : this()
                {
                    this.pos = pos;
                    this.size = size;
                    //this.dummy = 0;
                }
                public long pos;
                public int size;
                public int dummy;
            }

            #endregion
        }

        static class StringResource
        {
            private static int refCount = 0;
            private static Dictionary<int, string> strings = new Dictionary<int, string>();

            public static void Read(BinaryReader reader)
            {
                int stringCount = reader.ReadInt32();

                for (int i = 0; i < stringCount; i++)
                {
                    int id = reader.ReadInt32();
                    int length = reader.ReadInt32();

                    if (strings.ContainsKey(id) == false)
                    {
                        string text = string.Empty;
                        if (length != 0)
                        {
                            var bytes = reader.ReadBytes(length);
                            text = Encoding.UTF8.GetString(bytes);
                        }

                        strings[id] = text;
                    }
                    else
                    {
                        reader.BaseStream.Seek(length, SeekOrigin.Current);
                    }
                }
            }

            public static StringComparer GetComparer(bool caseSensitive)
            {
                if (caseSensitive == true)
                    return StringComparer.CurrentCulture;
                return StringComparer.CurrentCultureIgnoreCase;
            }

            public static string GetString(int id)
            {
                return StringResource.strings[id];
            }

            public static bool Equals(int id, string s)
            {
                return StringResource.Equals(id, s, true);
            }

            public static bool Equals(int id, string s, bool caseSensitive)
            {
                string s1 = StringResource.GetString(id);

                return StringResource.GetComparer(caseSensitive).Equals(s1, s);
            }

            public static int Ref
            {
                get { return StringResource.refCount; }
                set
                {
                    StringResource.refCount = value;
                    if (StringResource.refCount == 0)
                        StringResource.strings.Clear();
                }
            }
        }

        static class Utility
        {
            public static Type NameToType(string typeName)
            {
                if (typeName == "boolean")
                    return typeof(bool);
                else if (typeName == "string")
                    return typeof(string);
                else if (typeName == "float")
                    return typeof(float);
                else if (typeName == "double")
                    return typeof(double);
                else if (typeName == "int8")
                    return typeof(sbyte);
                else if (typeName == "uint8")
                    return typeof(byte);
                else if (typeName == "int16")
                    return typeof(short);
                else if (typeName == "uint16")
                    return typeof(ushort);
                else if (typeName == "int32")
                    return typeof(int);
                else if (typeName == "uint32")
                    return typeof(uint);
                else if (typeName == "int64")
                    return typeof(long);
                else if (typeName == "uint64")
                    return typeof(ulong);
                else if (typeName == "datetime")
                    return typeof(DateTime);
                else if (typeName == "duration")
                    return typeof(TimeSpan);
                else if (typeName == "guid")
                    return typeof(Guid);

                return typeof(int);
            }
        }
    }

    namespace Binary
    {
        class CremaBinaryColumn : IColumn
        {
            private string columnName;
            private Type type;
            private bool isKey;
            private int index;

            public CremaBinaryColumn(string columnName, Type type, bool isKey)
            {
                this.columnName = columnName;
                this.type = type;
                this.isKey = isKey;
            }

            public string Name
            {
                get { return this.columnName; }
            }

            public Type DataType
            {
                get { return this.type; }
            }

            public bool IsKey
            {
                get { return this.isKey; }
            }

            public int Index
            {
                get { return this.index; }
                internal set { this.index = value; }
            }

            public CremaBinaryTable Table
            {
                get;
                set;
            }

            #region IColumn

            ITable IColumn.Table
            {
                get { return this.Table; }
            }

            #endregion
        }

        class CremaBinaryColumnCollection : IColumnCollection
        {
            private readonly CremaBinaryTable table;
            private OrderedDictionary columns;

            public CremaBinaryColumnCollection(CremaBinaryTable table, int capacity, bool caseSensitive)
            {
                this.table = table;
                this.columns = new OrderedDictionary(capacity, StringResource.GetComparer(caseSensitive));
            }

            public void Add(CremaBinaryColumn item)
            {
                item.Index = this.columns.Count;
                this.columns.Add(item.Name, item);
            }

            public CremaBinaryColumn this[int index]
            {
                get { return this.columns[index] as CremaBinaryColumn; }
            }

            public CremaBinaryColumn this[string columnName]
            {
                get { return this.columns[columnName] as CremaBinaryColumn; }
            }

            private ICollection Collection
            {
                get { return this.columns.Values as ICollection; }
            }

            #region IColumnCollection

            IColumn IColumnCollection.this[int index]
            {
                get { return this[index]; }
            }

            IColumn IColumnCollection.this[string columnName]
            {
                get { return this[columnName]; }
            }

            ITable IColumnCollection.Table
            {
                get { return this.table; }
            }

            #endregion

            #region ICollection

            void ICollection.CopyTo(Array array, int index)
            {
                this.Collection.CopyTo(array, index);
            }

            int ICollection.Count
            {
                get { return this.Collection.Count; }
            }

            bool ICollection.IsSynchronized
            {
                get { return this.Collection.IsSynchronized; }
            }

            object ICollection.SyncRoot
            {
                get { return this.Collection.SyncRoot; }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.Collection.GetEnumerator();
            }

            IEnumerator<IColumn> IEnumerable<IColumn>.GetEnumerator()
            {
                return this.Collection.Cast<IColumn>().GetEnumerator();
            }

            #endregion
        }

        class CremaBinaryReader : IDataSet
        {
            private Stream stream;
            private TableIndex[] tableIndexes;
            private ReadOptions options;
            private CremaBinaryTableCollection tables;
            private int version;
            private string revision;
            private string name;
            private string typesHashValue;
            private string tablesHashValue;
            private string tags;

            public CremaBinaryReader()
            {

            }

            public ITableCollection Tables
            {
                get { return this.tables; }
            }

            public ReadOptions Options
            {
                get { return this.options; }
            }

            public bool CaseSensitive
            {
                get { return (this.options & ReadOptions.CaseSensitive) == ReadOptions.CaseSensitive; }
            }

            public string Revision
            {
                get { return this.revision; }
            }

            public int Version
            {
                get { return this.version; }
            }

            public string TypesHashValue
            {
                get { return this.typesHashValue; }
            }

            public string TablesHashValue
            {
                get { return this.tablesHashValue; }
            }

            public string Tags
            {
                get { return this.tags; }
            }

            public string Name
            {
                get { return this.name; }
            }

            public CremaBinaryTable ReadTable(string tableName)
            {
                int index = -1;

                for (int i = 0; i < this.tableIndexes.Length; i++)
                {
                    TableIndex tableIndex = this.tableIndexes[i];

                    if (StringResource.Equals(tableIndex.TableName, tableName) == true)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                    throw new KeyNotFoundException("테이블을 찾을수 없습니다.");

                CremaBinaryTable table = this.ReadTable(new BinaryReader(this.stream), this.tableIndexes[index].Offset);
                this.tables[index] = table;
                return table;
            }

            public CremaBinaryTable ReadTable(int index)
            {
                TableIndex table_index = this.tableIndexes[index];
                CremaBinaryTable table = this.ReadTable(new BinaryReader(this.stream), table_index.Offset);
                this.tables[index] = table;
                return table;
            }

            public void Read(Stream stream, ReadOptions options)
            {
                this.ReadCore(stream, options);
            }

            protected void ReadCore(Stream stream, ReadOptions options)
            {
                this.stream = stream;
                this.options = options;

                var reader = new BinaryReader(stream);

                stream.Seek(0, SeekOrigin.Begin);
                var fileHeader = reader.ReadValue<FileHeader>();
                this.tableIndexes = reader.ReadValues<TableIndex>(fileHeader.TableCount);
                this.version = fileHeader.MagicValue;

                stream.Seek(fileHeader.StringResourcesOffset, SeekOrigin.Begin);
                StringResource.Read(reader);
                this.revision = StringResource.GetString(fileHeader.Revision);
                this.name = StringResource.GetString(fileHeader.Name);
                this.tables = new CremaBinaryTableCollection(this, this.tableIndexes);
                this.typesHashValue = StringResource.GetString(fileHeader.TypesHashValue);
                this.tablesHashValue = StringResource.GetString(fileHeader.TablesHashValue);
                this.tags = StringResource.GetString(fileHeader.Tags);

                for (var i = 0; i < this.tableIndexes.Length; i++)
                {
                    var tableIndex = this.tableIndexes[i];
                    var table = this.ReadTable(reader, tableIndex.Offset);
                    this.tables[i] = table;
                }
            }

            private CremaBinaryTable ReadTable(BinaryReader reader, long offset)
            {
                TableHeader tableHeader;
                TableInfo tableInfo;

                reader.Seek(offset, SeekOrigin.Begin);
                reader.ReadValue(out tableHeader);

                reader.Seek(tableHeader.TableInfoOffset + offset, SeekOrigin.Begin);
                reader.ReadValue(out tableInfo);

                var table = new CremaBinaryTable(this, tableInfo.RowCount, this.options);

                reader.Seek(tableHeader.StringResourcesOffset + offset, SeekOrigin.Begin);
                StringResource.Read(reader);

                reader.Seek(tableHeader.ColumnsOffset + offset, SeekOrigin.Begin);
                this.ReadColumns(reader, table, tableInfo.ColumnCount);

                reader.Seek(tableHeader.RowsOffset + offset, SeekOrigin.Begin);
                this.ReadRows(reader, table, tableInfo.RowCount);

                table.Name = StringResource.GetString(tableInfo.TableName);
                table.Category = StringResource.GetString(tableInfo.CategoryName);
                table.HashValue = StringResource.GetString(tableHeader.HashValue);

                return table;
            }

            private void ReadRows(BinaryReader reader, CremaBinaryTable table, int rowCount)
            {
                for (var i = 0; i < rowCount; i++)
                {
                    var dataRow = table.Rows[i];
                    var length = reader.ReadInt32();
                    dataRow.fieldbytes = reader.ReadBytes(length);
                    dataRow.Table = table;
                }
            }

            private void ReadColumns(BinaryReader reader, CremaBinaryTable table, int columnCount)
            {
                var keys = new List<IColumn>();
                var columns = new CremaBinaryColumnCollection(table, columnCount, this.CaseSensitive);
                for (var i = 0; i < columnCount; i++)
                {
                    var columninfo = reader.ReadValue<ColumnInfo>();
                    var columnName = StringResource.GetString(columninfo.ColumnName);
                    var typeName = StringResource.GetString(columninfo.DataType);
                    var isKey = columninfo.Iskey == 0 ? false : true;

                    var column = new CremaBinaryColumn(columnName, Utility.NameToType(typeName), isKey);
                    columns.Add(column);
                    if (isKey == true)
                        keys.Add(column);

                    column.Table = table;
                }
                table.Columns = columns;
                table.Keys = keys.ToArray();
            }
        }

        class CremaBinaryRow : IRow
        {
            public byte[] fieldbytes;

            public CremaBinaryTable Table { get; set; }

            public object this[IColumn column]
            {
                get
                {
                    int offset = BitConverter.ToInt32(this.fieldbytes, sizeof(int) * column.Index);

                    if (column.DataType == typeof(string))
                    {
                        if (offset == 0)
                            return string.Empty;
                        int id = BitConverter.ToInt32(this.fieldbytes, offset);
                        return StringResource.GetString(id);
                    }
                    else
                    {
                        if (offset == 0)
                            return null;
                        if (column.DataType == typeof(bool))
                            return BitConverter.ToBoolean(this.fieldbytes, offset);
                        else if (column.DataType == typeof(sbyte))
                            return (sbyte)this.fieldbytes[offset];
                        else if (column.DataType == typeof(byte))
                            return this.fieldbytes[offset];
                        else if (column.DataType == typeof(short))
                            return BitConverter.ToInt16(this.fieldbytes, offset);
                        else if (column.DataType == typeof(ushort))
                            return BitConverter.ToUInt16(this.fieldbytes, offset);
                        else if (column.DataType == typeof(int))
                            return BitConverter.ToInt32(this.fieldbytes, offset);
                        else if (column.DataType == typeof(uint))
                            return BitConverter.ToUInt32(this.fieldbytes, offset);
                        else if (column.DataType == typeof(long))
                            return BitConverter.ToInt64(this.fieldbytes, offset);
                        else if (column.DataType == typeof(ulong))
                            return BitConverter.ToUInt64(this.fieldbytes, offset);
                        else if (column.DataType == typeof(char))
                            return BitConverter.ToChar(this.fieldbytes, offset);
                        else if (column.DataType == typeof(float))
                            return BitConverter.ToSingle(this.fieldbytes, offset);
                        else if (column.DataType == typeof(double))
                            return BitConverter.ToDouble(this.fieldbytes, offset);
                        else if (column.DataType == typeof(DateTime))
                            return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(Convert.ToDouble(BitConverter.ToInt64(this.fieldbytes, offset)));
                        else if (column.DataType == typeof(TimeSpan))
                            return new TimeSpan(BitConverter.ToInt64(this.fieldbytes, offset));
                        else if (column.DataType == typeof(Guid))
                        {
                            if (offset == 0)
                                return Guid.Empty;
                            var bytes = new Byte[16];
                            Array.Copy(this.fieldbytes, offset, bytes, 0, 16);
                            return new Guid(bytes);
                        }
                    }
                    throw new Exception();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public bool HasValue(IColumn column)
            {
                int offset = BitConverter.ToInt32(this.fieldbytes, sizeof(int) * column.Index);
                return offset != 0;
            }

            #region IRow

            ITable IRow.Table
            {
                get { return this.Table; }
            }

            object IRow.this[string columnName]
            {
                get
                {
                    return this[this.Table.Columns[columnName]];
                }
            }

            object IRow.this[int columnIndex]
            {
                get
                {
                    return this[this.Table.Columns[columnIndex]];
                }
            }

            bool IRow.HasValue(string columnName)
            {
                IColumn column = this.Table.Columns[columnName];
                if (column == null)
                    throw new KeyNotFoundException(string.Format("'{0}'은(는) '{1}'에 존재하지 않는 열입니다.", columnName, this.Table.Name));
                return this.HasValue(column);
            }

            bool IRow.HasValue(int columnIndex)
            {
                IColumn column = this.Table.Columns[columnIndex];
                if (column == null)
                    throw new ArgumentOutOfRangeException(string.Format("'{0}'번째 열은 '{1}'에 존재하지 않습니다.", columnIndex, this.Table.Name));
                return this.HasValue(column);
            }

            #endregion
        }

        class CremaBinaryRowCollection : IRowCollection
        {
            private readonly CremaBinaryTable table;
            private readonly List<CremaBinaryRow> rows;

            public CremaBinaryRowCollection(CremaBinaryTable table, int rowCount)
            {
                this.table = table;
                this.rows = new List<CremaBinaryRow>(rowCount);
                for (int i = 0; i < rowCount; i++)
                {
                    this.rows.Add(new CremaBinaryRow());
                }
            }

            public CremaBinaryRow this[int index]
            {
                get { return this.rows[index]; }
            }

            #region IRowCollection

            IRow IRowCollection.this[int index]
            {
                get { return this.rows[index]; }
            }

            ITable IRowCollection.Table
            {
                get { return this.table; }
            }

            #endregion

            #region ICollection

            void ICollection.CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            int ICollection.Count
            {
                get { return this.rows.Count; }
            }

            bool ICollection.IsSynchronized
            {
                get { return (this.rows as ICollection).IsSynchronized; }
            }

            object ICollection.SyncRoot
            {
                get { return (this.rows as ICollection).SyncRoot; }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (this.rows as ICollection).GetEnumerator();
            }

            IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator()
            {
                return this.rows.Cast<IRow>().GetEnumerator();
            }

            #endregion
        }

        class CremaBinaryTable : ITable
        {
            private string tableName;
            private string categoryName;
            private int index;
            private string hashValue;

            private CremaBinaryColumnCollection columns;
            private readonly CremaBinaryRowCollection rows;
            private IColumn[] keys;
            private readonly CremaBinaryReader reader;

            public CremaBinaryTable(CremaBinaryReader reader, int rowCount, ReadOptions options)
            {
                this.reader = reader;
                this.rows = new CremaBinaryRowCollection(this, rowCount);
            }

            public override string ToString()
            {
                return this.tableName ?? base.ToString();
            }

            public string Category
            {
                get { return this.categoryName; }
                set { this.categoryName = value; }
            }

            public string Name
            {
                get { return this.tableName; }
                set { this.tableName = value; }
            }

            public int Index
            {
                get { return this.index; }
                internal set { this.index = value; }
            }

            public string HashValue
            {
                get { return this.hashValue; }
                set { this.hashValue = value; }
            }

            public CremaBinaryColumnCollection Columns
            {
                get { return this.columns; }
                set { this.columns = value; }
            }

            public CremaBinaryRowCollection Rows
            {
                get { return this.rows; }
            }

            public CremaBinaryReader Reader
            {
                get { return this.reader; }
            }

            public IColumn[] Keys
            {
                get { return this.keys; }
                set { this.keys = value; }
            }

            #region ITable

            IColumn[] ITable.Keys
            {
                get { return this.keys; }
            }

            IRowCollection ITable.Rows
            {
                get { return this.rows; }
            }

            IColumnCollection ITable.Columns
            {
                get { return this.columns; }
            }

            IDataSet ITable.DataSet
            {
                get { return this.reader; }
            }

            #endregion
        }

        class CremaBinaryTableCollection : ITableCollection
        {
            private OrderedDictionary tables;
            private string[] tableNames;

            private readonly CremaBinaryReader reader;

            public CremaBinaryTableCollection(CremaBinaryReader reader, TableIndex[] tableIndexes)
            {
                this.reader = reader;

                this.tables = new OrderedDictionary(tableIndexes.Length, StringResource.GetComparer(this.reader.CaseSensitive));

                foreach (TableIndex item in tableIndexes)
                {
                    this.tables.Add(StringResource.GetString(item.TableName), null);
                }

                this.tableNames = this.tables.Keys.Cast<string>().ToArray();
            }

            public CremaBinaryTable this[int index]
            {
                get
                {
                    if (index >= this.tables.Count)
                        throw new ArgumentOutOfRangeException();

                    if (this.tables[index] == null)
                    {
                        return this.reader.ReadTable(index);
                    }
                    return this.tables[index] as CremaBinaryTable;
                }
                set
                {
                    this.tables[index] = value;
                    value.Index = index;
                }
            }

            public CremaBinaryTable this[string tableName]
            {
                get
                {
                    if (this.tables.Contains(tableName) == false)
                    {
                        throw new KeyNotFoundException(string.Format("'{0}'은(는) '{1}'에 존재하지 않는 테이블입니다.", tableName, this.reader.Name));
                    }

                    if (this.tables[tableName] == null)
                    {
                        return this.reader.ReadTable(tableName);
                    }
                    return this.tables[tableName] as CremaBinaryTable;
                }
            }

            private ICollection Collection
            {
                get { return this.tables.Values as ICollection; }
            }

            #region ITableCollection

            bool ITableCollection.Contains(string tableName)
            {
                return this.tables.Contains(tableName);
            }

            bool ITableCollection.IsTableLoaded(string tableName)
            {
                return true;
            }

            ITable ITableCollection.this[int index]
            {
                get { return this[index] as ITable; }
            }

            ITable ITableCollection.this[string tableName]
            {
                get { return this[tableName] as ITable; }
            }

            string[] ITableCollection.TableNames
            {
                get { return this.tableNames; }
            }

            #endregion

            #region ICollection

            void ICollection.CopyTo(Array array, int index)
            {
                this.Collection.CopyTo(array, index);
            }

            int ICollection.Count
            {
                get { return this.tables.Count; }
            }

            bool ICollection.IsSynchronized
            {
                get { return this.Collection.IsSynchronized; }
            }

            object ICollection.SyncRoot
            {
                get { return this.Collection.SyncRoot; }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.Collection.GetEnumerator();
            }

            IEnumerator<ITable> IEnumerable<ITable>.GetEnumerator()
            {
                return this.Collection.Cast<ITable>().GetEnumerator();
            }

            #endregion
        }

        struct TableHeader
        {
            public const int defaultMagicValue = 0x04000000;

            public int MagicValue { get; set; }
            public int HashValue { get; set; }
            public long ModifiedTime { get; set; }
            public long TableInfoOffset { get; set; }
            public long ColumnsOffset { get; set; }
            public long RowsOffset { get; set; }
            public long StringResourcesOffset { get; set; }
            public long UserOffset { get; set; }
        }

        struct TableInfo
        {
            public int TableName { get; set; }
            public int CategoryName { get; set; }
            public int ColumnCount { get; set; }
            public int RowCount { get; set; }
        }

        struct ColumnInfo
        {
            public int ColumnName { get; set; }
            public int DataType { get; set; }
            public int Iskey { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FileHeader
        {
            public const int defaultMagicValue = 0x04000000;

            public int MagicValue { get; set; }
            public int Revision { get; set; }
            public int TypesHashValue { get; set; }
            public int TablesHashValue { get; set; }
            public int Tags { get; set; }
            public int Reserved { get; set; }
            public int TableCount { get; set; }
            public int Name { get; set; }
            public long IndexOffset { get; set; }
            public long TablesOffset { get; set; }
            public long StringResourcesOffset { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TableIndex
        {
            public int TableName { get; set; }
            public int Dummy { get; set; }
            public long Offset { get; set; }
        }
    }
}

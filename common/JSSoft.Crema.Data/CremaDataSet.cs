﻿// Released under the MIT License.
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

//#if !DEBUG
#define USE_PARALLEL
//#endif
using JSSoft.Crema.Data.Properties;
using JSSoft.Crema.Data.Xml;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace JSSoft.Crema.Data
{
    [Serializable]
    public class CremaDataSet : IListSource, IDisposable, IXmlSerializable, ISerializable
    {
        public const string DefaultDataSetName = "Content";

        static CremaDataSet()
        {

        }

        public CremaDataSet()
        {
            this.InternalObject = new InternalDataSet(this, CremaDataSet.DefaultDataSetName);
            this.Tables = new CremaDataTableCollection(this.InternalObject);
            this.Types = new CremaDataTypeCollection(this.InternalObject);
        }

        private CremaDataSet(SerializationInfo info, StreamingContext context)
            : this()
        {
            var keys = info.GetValue("items", typeof(string[])) as string[];
            var items = new Dictionary<string, string>(keys.Length);

            foreach (var item in keys)
            {
                var value = info.GetString(item);
                items.Add(item, value.Decompress());
            }
            this.ReadMany(items);
        }

        private void ReadMany(Dictionary<string, string> items)
        {
            var typeSchemas = items.Keys.Where(item => item.StartsWith(this.TypeNamespace)).ToArray();
            Parallel.ForEach(typeSchemas, item =>
            {
                this.ReadTypeString(items[item]);
            });

            var tableSchemas = items.Keys.Where(item => item.StartsWith(this.TableNamespace) && item.EndsWith(CremaSchema.SchemaExtension)).ToArray();
            Parallel.ForEach(tableSchemas, item =>
            {
                var schema = items[item];

                if (schema.StartsWith(this.TableNamespace) == true)
                {
                    var ns = item.Remove(item.Length - CremaSchema.SchemaExtension.Length);
                    var tableName = CremaDataSet.GetTableName(this, ns);
                    var categoryPath = CremaDataSet.GetTableCategoryPath(this, ns);
                    schema = items[schema + CremaSchema.SchemaExtension];
                    this.ReadXmlSchemaString(schema, new ItemName(categoryPath, tableName));
                }
                else
                {
                    this.ReadXmlSchemaString(schema);
                }
            });

            var tableXmls = items.Keys.Where(item => item.StartsWith(this.TableNamespace) && item.EndsWith(CremaSchema.XmlExtension))
                                      .OrderBy(item => item.Length)
                                      .ToList();

            var threadcount = 8;
            var query = from item in tableXmls
                        let key = tableXmls.IndexOf(item) % threadcount
                        group item by key into g
                        select g;

            var parallellist = new List<string>(tableXmls.Count);

            foreach (var item in query)
            {
                parallellist.AddRange(item);
            }

            this.BeginLoad();
            Parallel.ForEach(parallellist, new ParallelOptions { MaxDegreeOfParallelism = threadcount }, item =>
            {
                var ns = item.Remove(item.Length - CremaSchema.XmlExtension.Length);
                var tableName = CremaDataSet.GetTableName(this, ns);
                var categoryPath = CremaDataSet.GetTableCategoryPath(this, ns);
                var xml = items[item];
                this.ReadXmlString(xml, new ItemName(categoryPath, tableName));
            });
            this.EndLoad();
        }

        private CremaDataSet(SignatureDateProvider modificationProvider)
            : this()
        {
            this.InternalObject.SignatureDateProvider = modificationProvider;
        }

        public static string GenerateHashValue(params TypeInfo[] types)
        {
            return InternalDataSet.GenerateHashValue(types);
        }

        public static string GenerateHashValue(params TableInfo[] tables)
        {
            return InternalDataSet.GenerateHashValue(tables);
        }

        public static CremaDataSet Create(SignatureDateProvider modificationProvider)
        {
            return new CremaDataSet(modificationProvider);
        }

        public override string ToString()
        {
            return this.InternalObject.ToString();
        }

        public event EventHandler Disposed
        {
            add { this.InternalObject.Disposed += value; }
            remove { this.InternalObject.Disposed -= value; }
        }

        public static CremaDataSet ReadSchema(string filename)
        {
            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchema(filename);
            return dataSet;
        }

        public static CremaDataSet ReadSchema(TextReader reader)
        {
            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchema(reader);
            return dataSet;
        }

        public static CremaDataSet ReadSchema(Stream stream)
        {
            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchema(stream);
            return dataSet;
        }

        public static CremaDataSet ReadSchema(XmlReader reader)
        {
            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchema(reader);
            return dataSet;
        }

        public static CremaDataSet ReadSchema(Stream stream, XmlResolver resolver)
        {
            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchema(stream, resolver);
            return dataSet;
        }

        public static CremaDataSet ReadSchema(XmlReader reader, XmlResolver resolver)
        {
            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchema(reader, resolver);
            return dataSet;
        }

        public static CremaDataSet ReadSchema(TextReader textReader, XmlResolver resolver)
        {
            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchema(textReader, resolver);
            return dataSet;
        }

        public static CremaDataSet ReadFromDirectory(string path)
        {
            return ReadFromDirectory(path, CremaDataSetFilter.Default);
        }

        [Obsolete]
        public static CremaDataSet ReadFromDirectory(string path, string filterExpression)
        {
            return ReadFromDirectory(path, filterExpression, ReadTypes.All);
        }

        /// <summary>
        /// 경로내에 타입과 테이블을 읽어들입니다.
        /// </summary>
        /// <param name="path">데이터를 읽어들일 경로입니다.</param>
        /// <param name="filterExpression">필터 표현식입니다. glob 형태를 사용하며 여러개일 경우 구분자 ; 를 사용합니다.
        /// options 값이 TypeOnly 일경우에는 필터가 타입에 영향을 미칩니다.
        /// </param>
        /// <param name="readType"></param>
        /// <returns></returns>
        [Obsolete]
        public static CremaDataSet ReadFromDirectory(string path, string filterExpression, ReadTypes readType)
        {
            var filter = new CremaDataSetFilter();
            if (filterExpression is not null)
            {
                filter.Tables = StringUtility.Split(filterExpression, ';');
            }
            if (readType == ReadTypes.OmitContent)
            {
                filter.OmitContent = true;
            }
            else if (readType == ReadTypes.TypeOnly)
            {
                filter.OmitTable = true;
            }
            return ReadFromDirectory(path, filter);
        }

        public static CremaDataSet ReadFromDirectory(string path, CremaDataSetFilter filter)
        {
            ValidateReadFromDirectory(path, filter);

            var dataSet = new CremaDataSet();
            var fullPath = new DirectoryInfo(path).FullName;
            var tablePath = Path.Combine(fullPath, CremaSchema.TableDirectory);
            var typePath = Path.Combine(fullPath, CremaSchema.TypeDirectory);
            var typeFiles = filter.FilterTypes(typePath, $"*{CremaSchema.SchemaExtension}");
            var tableFiles = filter.FilterTables(tablePath, $"*{CremaSchema.XmlExtension}");

            dataSet.ReadMany(typeFiles, tableFiles, filter.OmitContent);
            return dataSet;
        }

        public void BeginLoad()
        {
            this.InternalObject.BeginLoad();
        }

        public void EndLoad()
        {
            this.InternalObject.EndLoad();
        }

        public void Dispose()
        {
            this.InternalObject.Dispose();
        }

        public void AcceptChanges()
        {
            Parallel.ForEach(this.Tables, item =>
            {
                item.AcceptChanges();
            });

            Parallel.ForEach(this.Types, item =>
            {
                item.AcceptChanges();
            });
        }

        public void Clear()
        {
            this.InternalObject.Clear();
        }

        /// <summary>
        /// 데이터는 형태를 그대로 복사합니다.
        /// </summary>
        public CremaDataSet Copy()
        {
            var schema = this.GetXmlSchema();
            var xml = this.GetXml();

            var dataSet = new CremaDataSet()
            {
                Namespace = this.Namespace
            };
            dataSet.ReadXmlSchemaString(schema);
            dataSet.ReadXmlString(xml);
            dataSet.SignatureDateProvider = this.SignatureDateProvider;

            return dataSet;
        }

        /// <summary>
        /// 데이터는 복사하지 않고 형태만 복사합니다.
        /// </summary>
        public CremaDataSet Clone()
        {
            var schema = this.GetXmlSchema();

            var dataSet = new CremaDataSet();
            dataSet.ReadXmlSchemaString(schema);

            return dataSet;
        }

        public void RejectChanges()
        {
            this.InternalObject.RejectChanges();
            foreach (var item in this.Types)
            {
                item.RejectChanges();
            }
        }

        public bool HasChanges()
        {
            return this.InternalObject.HasChanges();
        }

        public bool HasChanges(DataRowState rowStates)
        {
            return this.InternalObject.HasChanges(rowStates);
        }

        public void ReadXmlString(string xml)
        {
            this.ValidateReadXml(null);
            using var reader = new StringReader(xml);
            this.ReadXml(reader);
        }

        public void ReadXmlString(string xml, ItemName itemName)
        {
            this.ValidateReadXml(itemName);
            using var reader = new StringReader(xml);
            this.ReadXml(reader, itemName);
        }

        public void ReadXmlSchemaString(string xmlSchema)
        {
            using var sr = new StringReader(xmlSchema);
            this.ReadXmlSchema(sr, new CremaTypeXmlResolver(this, this.TableNamespace));
        }

        public void ReadXmlSchemaString(string xmlSchema, ItemName itemName)
        {
            using var reader = new StringReader(xmlSchema);
            this.ReadXmlSchema(reader, itemName, new CremaTypeXmlResolver(this, this.TableNamespace));
        }

        public void ReadXmlSchemaString(string xmlSchema, XmlResolver resolver)
        {
            using var sr = new StringReader(xmlSchema);
            this.ReadXmlSchema(sr, resolver);
        }

        public void ReadXmlSchemaString(string xmlSchema, ItemName itemName, XmlResolver resolver)
        {
            using var sr = new StringReader(xmlSchema);
            this.ReadXmlSchema(sr, itemName, resolver);
        }

        public void ReadTypeString(string schema)
        {
            this.ReadTypeString(schema, null);
        }

        public void ReadTypeString(string schema, ItemName itemName)
        {
            using var sr = new StringReader(schema);
            this.ReadType(sr, itemName);
        }

        public void ReadXml(string filename)
        {
            var reader = new CremaXmlReader(this);
            reader.Read(filename);
        }

        public void ReadXml(string filename, ItemName itemName)
        {
            var xmlReader = new CremaXmlReader(this, itemName);
            xmlReader.Read(filename);
        }

        public void ReadXml(TextReader reader)
        {
            var xmlReader = new CremaXmlReader(this);
            xmlReader.Read(reader);
        }

        public void ReadXml(TextReader reader, ItemName itemName)
        {
            var xmlReader = new CremaXmlReader(this, itemName);
            xmlReader.Read(reader);
        }

        public void ReadXml(Stream stream)
        {
            var xmlReader = new CremaXmlReader(this);
            xmlReader.Read(stream);
        }

        public void ReadXml(Stream stream, ItemName itemName)
        {
            var xmlReader = new CremaXmlReader(this, itemName);
            xmlReader.Read(stream);
        }

        public void ReadXml(XmlReader reader)
        {
            var xmlReader = new CremaXmlReader(this);
            xmlReader.Read(reader);
        }

        public void ReadXml(XmlReader reader, ItemName itemName)
        {
            var xmlReader = new CremaXmlReader(this, itemName);
            xmlReader.Read(reader);
        }

        public void WriteXml(string filename)
        {
            var writer = new CremaXmlWriter(this);
            writer.Write(filename);
        }

        public void WriteXml(TextWriter textWriter)
        {
            var writer = new CremaXmlWriter(this);
            writer.Write(textWriter);
        }

        public void WriteXml(Stream stream)
        {
            var writer = new CremaXmlWriter(this);
            writer.Write(stream);
        }

        public void WriteXml(XmlWriter xmlWriter)
        {
            var writer = new CremaXmlWriter(this);
            writer.Write(xmlWriter);
        }

        /// <summary>
        /// xml 파일을 사용하여 스키마를 찾아 읽어들이고 데이터를 읽어들입니다.
        /// </summary>
        public void ReadTable(string filename)
        {
            var readInfo = new CremaXmlReadInfo(filename);
            this.ReadXmlSchema(readInfo.SchemaPath, readInfo.ItemName);
            this.ReadXml(readInfo.XmlPath, readInfo.ItemName);
        }

        public void ReadMany(string[] typeFiles, string[] tableFiles)
        {
            this.ReadMany(typeFiles, tableFiles, false);
        }

        public void ReadMany(string[] typeFiles, string[] tableFiles, bool schemaOnly)
        {
#if USE_PARALLEL
            Parallel.ForEach(typeFiles, item =>
            {
                this.ReadType(item);
            });
#else
            foreach (var item in typeFiles)
            {
                this.ReadType(item);
            }
#endif

            var readInfos = new List<CremaXmlReadInfo>(tableFiles.Length);
            Parallel.ForEach(tableFiles, item =>
            {
                var info = new CremaXmlReadInfo(item);
                lock (readInfos)
                {
                    readInfos.Add(info);
                }
            });

#if USE_PARALLEL
            Parallel.ForEach(readInfos.Where(i => i.IsInherited == false), item =>
            {
                this.ReadXmlSchema(item.SchemaPath, item.ItemName);
            });
            Parallel.ForEach(readInfos.Where(i => i.IsInherited), item =>
            {
                this.ReadXmlSchema(item.SchemaPath, item.ItemName);
            });
#else
            foreach (var item in readInfos.Where(i => i.IsInherited == false))
            {
                this.ReadXmlSchema(item.SchemaPath, item.ItemName);
            }
            foreach (var item in readInfos.Where(i => i.IsInherited))
            {
                this.ReadXmlSchema(item.SchemaPath, item.ItemName);
            }
#endif

            //if (schemaOnly == false)
            //{
            this.BeginLoad();

#if USE_PARALLEL
            readInfos.Sort((x, y) => y.XmlSize.CompareTo(x.XmlSize));

            var threadcount = 8;
            var query = from item in readInfos
                        let key = readInfos.IndexOf(item) % threadcount
                        group item by key into g
                        select g;

            var parallellist = new List<CremaXmlReadInfo>(readInfos.Count);

            foreach (var item in query)
            {
                parallellist.AddRange(item);
            }

            try
            {
                Parallel.ForEach(parallellist, new ParallelOptions { MaxDegreeOfParallelism = threadcount }, item =>
                {
                    var xmlReader = new CremaXmlReader(this, item.ItemName)
                    {
                        OmitContent = schemaOnly == true
                    };
                    xmlReader.Read(item.XmlPath);
                });
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
#else
            foreach (var item in readInfos)
            {
                var xmlReader = new CremaXmlReader(this, item.ItemName)
                {
                    OmitContent = schemaOnly == true
                };
                xmlReader.Read(item.XmlPath);
            }
#endif
            this.EndLoad();
            //}
            this.Tables.Sort();
        }

        public void ReadType(string filename)
        {
            this.ReadType(filename, null);
        }

        public void ReadType(string filename, ItemName itemName)
        {
            var dataType = new CremaDataType();
            var schemaReader = new CremaSchemaReader(dataType, itemName);
            schemaReader.Read(filename);
            lock (this)
            {
                this.Types.Add(dataType);
            }
        }

        public void ReadType(TextReader reader)
        {
            this.ReadType(reader, null);
        }

        public void ReadType(TextReader reader, ItemName itemName)
        {
            var dataType = new CremaDataType();
            var schemaReader = new CremaSchemaReader(dataType, itemName);
            schemaReader.Read(reader);
            this.Types.Add(dataType);
        }

        public void ReadType(Stream reader)
        {
            this.ReadType(reader, null);
        }

        public void ReadType(Stream stream, ItemName itemName)
        {
            var dataType = new CremaDataType();
            var schemaReader = new CremaSchemaReader(dataType, itemName);
            schemaReader.Read(stream);
            this.Types.Add(dataType);
        }

        public void ReadType(XmlReader reader)
        {
            this.ReadType(reader, null);
        }

        public void ReadType(XmlReader reader, ItemName itemName)
        {
            var dataType = new CremaDataType();
            var schemaReader = new CremaSchemaReader(dataType, itemName);
            schemaReader.Read(reader);
            this.Types.Add(dataType);
        }

        public void ReadXmlSchema(string filename)
        {
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(filename);
        }

        /// <summary>
        /// 파일을 읽어들일때 tableNamespace 형태로 읽어들입니다.
        /// </summary>
        public void ReadXmlSchema(string filename, ItemName itemName)
        {
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(filename);
        }

        public void ReadXmlSchema(string filename, XmlResolver resolver)
        {
            this.ValidateReadXmlSchema();
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(filename, resolver);
        }

        public void ReadXmlSchema(string filename, ItemName itemName, XmlResolver resolver)
        {
            this.ValidateReadXmlSchema();
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(filename, resolver);
        }

        public void ReadXmlSchema(TextReader textReader)
        {
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(textReader);
        }

        public void ReadXmlSchema(TextReader textReader, ItemName itemName)
        {
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(textReader);
        }

        public void ReadXmlSchema(TextReader textReader, XmlResolver resolver)
        {
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(textReader, resolver);
        }

        public void ReadXmlSchema(TextReader textReader, ItemName itemName, XmlResolver resolver)
        {
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(textReader, resolver);
        }

        public void ReadXmlSchema(Stream stream)
        {
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(stream);
        }

        public void ReadXmlSchema(Stream stream, ItemName itemName)
        {
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(stream);
        }

        public void ReadXmlSchema(Stream stream, XmlResolver resolver)
        {
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(stream, resolver);
        }

        public void ReadXmlSchema(Stream stream, ItemName itemName, XmlResolver resolver)
        {
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(stream, resolver);
        }

        public void ReadXmlSchema(XmlReader xmlReader)
        {
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(xmlReader);
        }

        public void ReadXmlSchema(XmlReader xmlReader, ItemName itemName)
        {
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(xmlReader);
        }

        public void ReadXmlSchema(XmlReader xmlReader, XmlResolver resolver)
        {
            var schemaReader = new CremaSchemaReader(this);
            schemaReader.Read(xmlReader, resolver);
        }

        public void ReadXmlSchema(XmlReader xmlReader, ItemName itemName, XmlResolver resolver)
        {
            var schemaReader = new CremaSchemaReader(this, itemName);
            schemaReader.Read(xmlReader, resolver);
        }

        public void WriteXmlSchema(string filename)
        {
            var schemaWriter = new CremaSchemaWriter(this);
            schemaWriter.Write(filename);
        }

        public void WriteXmlSchema(TextWriter writer)
        {
            var schemaWriter = new CremaSchemaWriter(this);
            schemaWriter.Write(writer);
        }

        public void WriteXmlSchema(Stream stream)
        {
            var schemaWriter = new CremaSchemaWriter(this);
            schemaWriter.Write(stream);
        }

        public void WriteXmlSchema(XmlWriter writer)
        {
            var schemaWriter = new CremaSchemaWriter(this);
            schemaWriter.Write(writer);
        }

        /// <summary>
        /// DataSet의 모든 타입과 테이블을 경로에 단위별로 저장합니다.
        /// </summary>
        /// <param name="path"></param>
        public void WriteToDirectory(string path)
        {
            path = PathUtility.GetFullPath(path);
            DirectoryUtility.Prepare(path);
            DirectoryUtility.Prepare(path, CremaSchema.TypeDirectory);
            DirectoryUtility.Prepare(path, CremaSchema.TableDirectory);

            foreach (var item in this.Types)
            {
                var relativePath = UriUtility.MakeRelativeOfDirectory(this.InternalObject.Namespace, item.Namespace);
                var filename = Path.Combine(path, relativePath + CremaSchema.SchemaExtension);
                FileUtility.Prepare(filename);
                item.Write(filename);
            }

            foreach (var item in this.Tables)
            {
                if (item.TemplateNamespace != string.Empty)
                {
                    if (item.TemplatedParent == null)
                    {
                        var relativePath = UriUtility.MakeRelativeOfDirectory(this.InternalObject.Namespace, item.TemplateNamespace);
                        var filename = Path.Combine(path, relativePath + CremaSchema.SchemaExtension);
                        FileUtility.Prepare(filename);
                        item.WriteXmlSchema(filename);
                    }
                }
                else
                {
                    var relativePath = UriUtility.MakeRelativeOfDirectory(this.InternalObject.Namespace, item.Namespace);
                    var filename = Path.Combine(path, relativePath + CremaSchema.SchemaExtension);
                    FileUtility.Prepare(filename);
                    item.WriteXmlSchema(filename);
                }
            }

            foreach (var item in this.Tables)
            {
                var relativePath = UriUtility.MakeRelativeOfDirectory(this.InternalObject.Namespace, item.Namespace);
                var filename = Path.Combine(path, relativePath + CremaSchema.XmlExtension);
                FileUtility.Prepare(filename);
                item.WriteXml(filename);
            }
        }

        public string GetXmlSchema()
        {
            using var sw = new Utf8StringWriter();
            this.WriteXmlSchema(sw);
            return sw.ToString();
        }

        public string GetXml()
        {
            using var sw = new Utf8StringWriter();
            this.WriteXml(sw);
            return sw.ToString();
        }

        public IDictionary<string, object> ToDictionary()
        {
            return this.ToDictionary(false, false);
        }

        public IDictionary<string, object> ToDictionary(bool omitType, bool omitTable)
        {
            var props = new Dictionary<string, object>();

            if (omitType == false)
            {
                var types = new Dictionary<string, object>(this.Types.Count);
                foreach (var item in this.Types.OrderBy(item => item.Name))
                {
                    types.Add(item.Name, item.ToDictionary());
                }
                props.Add(CremaSchema.TypeDirectory, types);
            }

            if (omitTable == false)
            {
                var tables = new Dictionary<string, object>(this.Tables.Count);
                foreach (var item in this.Tables.OrderBy(item => item.Name))
                {
                    tables.Add(item.Name, item.ToDictionary());
                }
                props.Add(CremaSchema.TableDirectory, tables);
            }

            return props;
        }

        [DefaultValue(false)]
        public bool CaseSensitive
        {
            get => this.InternalObject.CaseSensitive;
            set => this.InternalObject.CaseSensitive = value;
        }

        [DefaultValue("")]
        public string DataSetName => this.InternalObject.DataSetName;

        [Browsable(false)]
        public DataViewManager DefaultViewManager => this.InternalObject.DefaultViewManager;

        [Browsable(false)]
        public PropertyCollection ExtendedProperties => this.InternalObject.ExtendedProperties;

        [Browsable(false)]
        public bool HasErrors
        {
            get
            {
                if (this.InternalObject.HasErrors == true)
                    return true;

                var query = from item in this.Tables
                            where item.HasErrors
                            select item;

                return query.Any();
            }
        }

        [DefaultValue("")]
        public string Namespace
        {
            get => this.InternalObject.Namespace;
            set => this.InternalObject.Namespace = value;
        }

        public string TableNamespace => this.InternalObject.TableNamespace;

        public string TypeNamespace => this.InternalObject.TypeNamespace;

        public SignatureDateProvider SignatureDateProvider
        {
            get => this.InternalObject.SignatureDateProvider;
            set => this.InternalObject.SignatureDateProvider = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public CremaDataTableCollection Tables { get; }

        public CremaDataTypeCollection Types { get; }

        public static void ValidateName(string name)
        {
            if (IdentifierValidator.Verify(name) == false)
                throw new ArgumentException($"{name} is invalid name", nameof(name));
        }

        public static bool VerifyName(string name)
        {
            return IdentifierValidator.Verify(name);
        }

        public static void Validate(string filename)
        {
            var errorList = new List<ValidationEventArgs>();
            Validate(filename, (s, e) => errorList.Add(e));

            foreach (var item in errorList)
            {
                if (item.Severity == XmlSeverityType.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(new XmlException(item.Message, item.Exception, item.Exception.LineNumber, item.Exception.LinePosition).Message);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(item.Message);
                }
            };
        }

        public static void Validate(string filename, ValidationEventHandler validationEventHandler)
        {
            var settings = new XmlReaderSettings
            {
                ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints,
                ValidationType = ValidationType.Schema,
            };

            settings.Schemas.Add(ReaderSchema());
            settings.ValidationEventHandler += validationEventHandler;
            using (var reader = XmlReader.Create(filename, settings))
            {
                while (reader.Read())
                {

                }
            }

            XmlSchema ReaderSchema()
            {
                var readInfo = new CremaXmlReadInfo(filename);
                using var reader = XmlReader.Create(readInfo.SchemaPath);
                return XmlSchema.Read(reader, (s, e) => { });
            }
        }

        public static void ValidateDirectory(string path)
        {
            ValidateDirectory(path, CremaDataSetFilter.Default);
        }

        public static void ValidateDirectory(string path, CremaDataSetFilter filter)
        {
            ValidateDirectory(path, filter, DefaultValidationEventHandler);
        }

        public static void ValidateDirectory(string path, ValidationEventHandler validationEventHandler)
        {
            ValidateDirectory(path, CremaDataSetFilter.Default, validationEventHandler);
        }

        public static void ValidateDirectory(string path, CremaDataSetFilter filter, ValidationEventHandler validationEventHandler)
        {
            if (validationEventHandler == null)
                throw new ArgumentNullException(nameof(validationEventHandler));
            ValidateReadFromDirectory(path, filter);

            var tablePath = Path.Combine(path, CremaSchema.TableDirectory);
            var typePath = Path.Combine(path, CremaSchema.TypeDirectory);
            var typeFiles = filter.FilterTypes(typePath, $"*{CremaSchema.SchemaExtension}");
            var tableFiles = filter.FilterTables(tablePath, $"*{CremaSchema.XmlExtension}");

            var errorList = new List<ValidationEventArgs>();
            foreach (var item in tableFiles)
            //Parallel.ForEach(query, item =>
            {
                Validate(item, (s, e) =>
                {
                    lock (errorList)
                    {
                        errorList.Add(e);
                    }
                });
            }
            //});

            foreach (var item in errorList.OrderBy(i => i.Exception.SourceUri))
            {
                validationEventHandler(null, item);
            }
        }

        private static void DefaultValidationEventHandler(object sender, ValidationEventArgs item)
        {
            Console.WriteLine(item.Exception.SourceUri);

            if (item.Severity == XmlSeverityType.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("error: ");
                Console.WriteLine(new XmlException(item.Message, item.Exception, item.Exception.LineNumber, item.Exception.LinePosition).Message);
                Console.ResetColor();
            }
            else
            {
                Console.Write("warning: ");
                Console.WriteLine(item.Message);
            }
        }

        // private static bool Filter(string filterExpression, string basePath, string itemPath)
        // {
        //     if (filterExpression == null)
        //         return true;

        //     var patterns = StringUtility.Split(filterExpression, ';');
        //     var namePattern = string.Join(";", patterns.Where(item => item.IndexOf(PathUtility.SeparatorChar) < 0));
        //     var pathPattern = string.Join(";", patterns.Where(item => item.IndexOf(PathUtility.SeparatorChar) >= 0));
        //     var path = FileUtility.RemoveExtension(itemPath);
        //     var relativePath = UriUtility.MakeRelativeOfDirectory(basePath, path);
        //     var items = StringUtility.SplitPath(relativePath);
        //     var itemName = ItemName.Create(items);

        //     if (namePattern != string.Empty && StringUtility.GlobMany(itemName.Name, namePattern) == true)
        //     {
        //         return true;
        //     }

        //     if (pathPattern != string.Empty && StringUtility.GlobMany(itemName.CategoryPath, pathPattern) == true)
        //     {
        //         return true;
        //     }

        //     return false;
        // }

        private void GetSerializableData(IDictionary<string, string> items)
        {
            var relativeType = UriUtility.MakeRelativeOfDirectory(this.InternalObject.Namespace, this.TypeNamespace);
            var lockobj = new object();

            Parallel.ForEach(this.Types, item =>
            {
                var schema = item.GetXmlSchema().Compress();
                lock (lockobj)
                {
                    items.Add(item.Namespace + CremaSchema.SchemaExtension, schema);
                }
            });

            Parallel.ForEach(this.Tables, item =>
            {
                var schema = string.Empty;
                if (item.TemplateNamespace == string.Empty)
                    schema = item.GetXmlSchema().Compress();
                else
                    schema = item.TemplateNamespace.Compress();
                lock (lockobj)
                {
                    items.Add(item.Namespace + CremaSchema.SchemaExtension, schema);
                }
            });

            Parallel.ForEach(this.Tables, item =>
            {
                var xml = item.GetXml().Compress();
                lock (lockobj)
                {
                    items.Add(item.Namespace + CremaSchema.XmlExtension, xml);
                }
            });
        }

        private static void ValidateReadFromDirectory(string path, CremaDataSetFilter filter)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));

            var typePath = Path.Combine(path, CremaSchema.TypeDirectory);
            if (Directory.Exists(typePath) == false)
                throw new DirectoryNotFoundException(string.Format(Resources.Exception_NotFoundDirectory_Format, typePath));

            var tablePath = Path.Combine(path, CremaSchema.TableDirectory);
            if (Directory.Exists(tablePath) == false)
                throw new DirectoryNotFoundException(string.Format(Resources.Exception_NotFoundDirectory_Format, tablePath));
        }

        private void ValidateReadXmlSchema()
        {

        }

        private void ValidateReadXml(ItemName itemName)
        {
            if (itemName == null)
            {
                foreach (var item in this.Tables)
                {
                    if (item.Rows.Count > 0)
                        throw new CremaDataException();
                }
            }
            else
            {
                foreach (var item in this.Tables.Where(i => i.Namespace == this.TableNamespace))
                {
                    if (item.Rows.Count > 0)
                        throw new CremaDataException();
                }
            }
        }

        internal string GetTableCategoryPath(string tableNamespace)
        {
            return this.InternalObject.GetTableCategoryPath(tableNamespace);
        }

        internal static string GetTableCategoryPath(CremaDataSet dataSet, string tableNamespace)
        {
            return InternalDataSet.GetTableCategoryPath((InternalDataSet)dataSet, tableNamespace);
        }

        internal static string GetTableCategoryPath(string baseNamespace, string tableNamespace)
        {
            return InternalDataSet.GetTableCategoryPath(baseNamespace, tableNamespace);
        }

        internal string GetTableName(string tableNamespace)
        {
            return this.InternalObject.GetTableName(tableNamespace);
        }

        internal static string GetTableName(CremaDataSet dataSet, string tableNamespace)
        {
            return InternalDataSet.GetTableName((InternalDataSet)dataSet, tableNamespace);
        }

        internal static string GetTableName(string baseNamespace, string tableNamespace)
        {
            return InternalDataSet.GetTableName(baseNamespace, tableNamespace);
        }

        internal string GetTypeCategoryPath(string typeNamespace)
        {
            return this.InternalObject.GetTypeCategoryPath(typeNamespace);
        }

        internal static string GetTypeCategoryPath(CremaDataSet dataSet, string typeNamespace)
        {
            return InternalDataSet.GetTypeCategoryPath((InternalDataSet)dataSet, typeNamespace);
        }

        internal static string GetTypeCategoryPath(string baseNamespace, string typeNamespace)
        {
            return InternalDataSet.GetTypeCategoryPath(baseNamespace, typeNamespace);
        }

        internal string GetTypeName(string typeNamespace)
        {
            return this.InternalObject.GetTypeName(typeNamespace);
        }

        internal static string GetTypeName(CremaDataSet dataSet, string typeNamespace)
        {
            return InternalDataSet.GetTypeName((InternalDataSet)dataSet, typeNamespace);
        }

        internal static string GetTypeName(string baseNamespace, string typeNamespace)
        {
            return InternalDataSet.GetTypeName(baseNamespace, typeNamespace);
        }

        internal InternalDataSet InternalObject { get; }

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
            this.Namespace = reader.GetAttribute(nameof(this.Namespace));
            reader.ReadStartElement();
            var schema = reader.ReadElementContentAsString();
            var xml = reader.ReadElementContentAsString();
            this.ReadXmlSchemaString(schema);
            this.ReadXmlString(xml);
            reader.ReadEndElement();
            this.AcceptChanges();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute(nameof(this.Namespace), this.Namespace);
            writer.WriteElementString("Schema", this.GetXmlSchema());
            writer.WriteElementString("Xml", this.GetXml());
        }

        #endregion

        #region ISerializable

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var items = new Dictionary<string, string>();
            this.GetSerializableData(items);
            info.AddValue("items", items.Keys.OrderBy(item => item).ToArray());

            foreach (var item in items)
            {
                info.AddValue(item.Key, item.Value);
            }
        }

        #endregion
    }
}

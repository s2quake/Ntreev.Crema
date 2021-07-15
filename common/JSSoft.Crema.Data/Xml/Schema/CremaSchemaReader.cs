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

#pragma warning disable 0612
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace JSSoft.Crema.Data.Xml.Schema
{
    public class CremaSchemaReader
    {
        private readonly CremaDataTable dataTable;
        private readonly CremaDataType dataType;
        private readonly ItemName itemName;
        private readonly Dictionary<string, CremaDataTable> tables = new();
        private Version version = new();
        private string hashValue;
        private XmlSchema schema;

        public CremaSchemaReader(CremaDataSet dataSet)
            : this(dataSet, null)
        {

        }

        public CremaSchemaReader(CremaDataSet dataSet, ItemName itemName)
        {
            this.DataSet = dataSet ?? throw new ArgumentNullException();
            this.itemName = itemName;
        }

        public CremaSchemaReader(CremaDataTable dataTable, ItemName itemName)
        {
            if (dataTable == null)
                throw new ArgumentNullException();
            if (dataTable.DataSet != null)
                this.DataSet = dataTable.DataSet;
            else
                this.dataTable = dataTable;
            this.itemName = itemName;
        }

        public CremaSchemaReader(CremaDataType type, ItemName itemName)
        {
            this.dataType = type ?? throw new ArgumentNullException();
            this.itemName = itemName;
        }

        public void Read(string filename)
        {
            if (this.hashValue == null)
            {
                this.hashValue = HashUtility.GetHashValueFromFile(filename);
            }
            using var stream = File.OpenRead(filename);
            var schema = XmlSchema.Read(stream, CremaSchema.SchemaValidationEventHandler);
            schema.SourceUri = $"{new Uri(filename)}";
            this.Read(schema, new CremaXmlResolver(filename));
        }

        public void Read(string filename, XmlResolver resolver)
        {
            if (this.hashValue == null)
            {
                this.hashValue = HashUtility.GetHashValueFromFile(filename);
            }
            using var stream = File.OpenRead(filename);
            var schema = XmlSchema.Read(stream, CremaSchema.SchemaValidationEventHandler);
            schema.SourceUri = $"{new Uri(filename)}";
            this.Read(schema, resolver);
        }

        public void Read(TextReader reader)
        {
            this.Read(reader, new CremaXmlResolver());
        }

        public void Read(Stream stream)
        {
            if (this.hashValue == null && stream.CanSeek == true)
            {
                this.hashValue = HashUtility.GetHashValue(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }
            this.Read(stream, new CremaXmlResolver());
        }

        public void Read(XmlReader reader)
        {
            this.Read(reader, new CremaXmlResolver());
        }

        public void Read(Stream stream, XmlResolver resolver)
        {
            if (this.hashValue == null && stream.CanSeek == true)
            {
                this.hashValue = HashUtility.GetHashValue(stream);
                stream.Seek(0, SeekOrigin.Begin);
            }

            this.Read(XmlSchema.Read(stream, CremaSchema.SchemaValidationEventHandler), resolver);
        }

        public void Read(TextReader reader, XmlResolver resolver)
        {
            using var xmlReader = XmlReader.Create(reader);
            this.Read(xmlReader, resolver);
        }

        public void Read(XmlReader reader, XmlResolver resolver)
        {
            this.Read(XmlSchema.Read(reader, CremaSchema.SchemaValidationEventHandler), resolver);
            if (reader.NodeType == XmlNodeType.EndElement)
                reader.ReadEndElement();
        }

        public CremaDataSet DataSet { get; }

        private void Read(XmlSchema schema, XmlResolver resolver)
        {
            var schemaSet = new XmlSchemaSet()
            {
                XmlResolver = resolver
            };
            schemaSet.Add(schema);
            schemaSet.Compile();

            if (Version.TryParse(schema.Version, out this.version) == false)
            {
                this.version = new Version(2, 0);
            }
            this.schema = schema;
            if (this.dataType != null)
            {
                this.ReadDataType(schema);
            }
            else if (this.dataTable != null)
            {
                var element = schema.Elements.Values.OfType<XmlSchemaElement>().First();
                if (element.Name != CremaDataSet.DefaultDataSetName)
                    throw new CremaDataException();
                this.ReadDataTable(element);
            }
            else if (this.DataSet != null)
            {
                var element = schema.Elements.Values.OfType<XmlSchemaElement>().First();
                if (element.Name != CremaDataSet.DefaultDataSetName)
                    throw new CremaDataException();

                this.ReadDataTypes(schema);
                this.ReadDataTables(element);
            }
        }

        private void ReadDataType(XmlSchema schema)
        {
            if (this.version < new Version(3, 0))
            {
                var query = from item in schema.GetSimpleTypes()
                            where item.Name.EndsWith("_Flags") == false
                            select item;
                var simpleType = query.Single();
                this.ReadType(simpleType);
            }
            else
            {
                var query = from item in schema.GetSimpleTypes()
                            where item.Name.EndsWith(CremaSchema.FlagExtension) == false
                            select item;

                var simpleType = query.Single();
                this.ReadType(simpleType);
            }
            if (this.itemName != null)
            {
                this.dataType.InternalName = this.itemName.Name;
                this.dataType.InternalCategoryPath = this.itemName.CategoryPath;

            }
        }

        private void ReadDataTypes(XmlSchema schema)
        {
            if (this.version < new Version(3, 0))
            {
                var query = from item in schema.GetSimpleTypes()
                            where item.Name.EndsWith("_Flags") == false
                            select item;

                foreach (var item in query)
                {
                    this.ReadType(item);
                }
            }
            else
            {
                var query = from item in schema.GetSimpleTypes()
                            where item.Name.EndsWith(CremaSchema.FlagExtension) == false
                            select item;

                foreach (var item in query)
                {
                    if (item.QualifiedName.Name == typeof(Guid).GetSchemaTypeName() && item.QualifiedName.Namespace == schema.TargetNamespace)
                        continue;
                    this.ReadType(item);
                }
            }
        }

        /// <summary>
        /// for version 2.0
        /// </summary>
        private void ReadExtendedProperties(XmlSchemaAnnotated annotated, PropertyCollection properties)
        {
            var annotation = annotated.Annotation;
            if (annotation == null)
                return;

            for (var i = 0; i < annotation.Items.Count; i++)
            {
                var item = annotation.Items[i];
                if (item is XmlSchemaAppInfo == true)
                {
                    var appInfo = item as XmlSchemaAppInfo;

                    foreach (XmlNode xmlNode in appInfo.Markup)
                    {
                        if (xmlNode.Name != CremaSchema.ExtendedProperty)
                            continue;

                        var keyAttr = xmlNode.Attributes["key"];
                        if (keyAttr == null)
                            keyAttr = xmlNode.Attributes["name"];
                        var keyTypeAttr = xmlNode.Attributes["keyType"];
                        var valueTypeAttr = xmlNode.Attributes["valueType"];
                        if (valueTypeAttr == null)
                            valueTypeAttr = xmlNode.Attributes["type"];
                        var valueAttr = xmlNode.Attributes["value"];

                        try
                        {
                            var keyType = typeof(string);
                            if (keyTypeAttr != null)
                            {
                                keyType = Type.GetType(keyTypeAttr.Value);
                            }

                            var valueType = typeof(string);
                            if (valueTypeAttr != null)
                            {
                                valueType = Type.GetType(valueTypeAttr.Value);
                            }

                            object key = null;
                            object value = null;

                            if (keyType == typeof(string))
                            {
                                key = keyAttr.Value;
                            }
                            else
                            {
                                var converter = TypeDescriptor.GetConverter(keyType);
                                key = converter.ConvertFromString(keyAttr.Value);
                            }

                            if (valueType == typeof(string))
                            {
                                value = valueAttr.Value;
                            }
                            else
                            {
                                var converter = TypeDescriptor.GetConverter(valueType);
                                value = converter.ConvertFromString(valueAttr.Value);
                            }

                            properties.Add(key, value);
                        }
                        catch
                        {
                            properties.Add(keyAttr.Value, valueAttr.Value);
                        }
                    }
                }
            }
        }

        private void ReadKey(XmlSchemaKey key)
        {
            var dataTable = this.GetTable(key, CremaSchema.KeyTypeNameExtension);

            lock (CremaSchema.lockobj)
            {
                foreach (var item in key.GetFields())
                {
                    var columnName = item.XPath.Replace(CremaSchema.TableTypePrefix + ":", string.Empty);
                    var dataColumn = dataTable.Columns[columnName];

                    dataColumn.InternalIsKey = true;
                }
            }
        }

        private void ReadUnique(XmlSchemaUnique unique)
        {
            var dataTable = this.GetTable(unique, CremaSchema.UniqueTypeNameExtension);

            lock (CremaSchema.lockobj)
            {
                foreach (var item in unique.GetFields())
                {
                    var columnName = item.XPath.Replace(CremaSchema.TableTypePrefix + ":", string.Empty);
                    var dataColumn = dataTable.Columns[columnName];

                    dataColumn.InternalUnique = true;
                }
            }
        }

        private void ReadTableInfo(XmlSchemaComplexType complexType, CremaDataTable dataTable)
        {
            dataTable.InternalCreationInfo = complexType.ReadAppInfoAsSigunatureDate(CremaSchema.TableInfo, CremaSchema.Creator, CremaSchema.CreatedDateTime);
            dataTable.InternalModificationInfo = complexType.ReadAppInfoAsSigunatureDate(CremaSchema.TableInfo, CremaSchema.Modifier, CremaSchema.ModifiedDateTime);
            dataTable.InternalTableID = complexType.ReadAppInfoAsGuid(CremaSchema.TableInfo, CremaSchema.ID);
            dataTable.InternalTags = complexType.ReadAppInfoAsTagInfo(CremaSchema.TableInfo, CremaSchema.Tags);
            dataTable.InternalComment = complexType.ReadDescription();
        }

        private void ReadAttribute(XmlSchemaAttribute schemaAttribute, CremaDataTable dataTable)
        {
            if (schemaAttribute.Name == CremaSchema.RelationID)
            {
                //dataTable.CreateRelationColumn();
                return;
            }

            if (schemaAttribute.Name == CremaSchema.ParentID)
            {
                //dataTable.CreateParentColumn();
                return;
            }

            var attributeName = schemaAttribute.Name;
            var attribute = dataTable.Attributes[attributeName];

            if (attribute == null)
                attribute = dataTable.Attributes.Add(attributeName);

            if (schemaAttribute.Use == XmlSchemaUse.Required)
                attribute.AllowDBNull = false;

            if (string.IsNullOrEmpty(schemaAttribute.DefaultValue) == false)
                attribute.DefaultValue = CremaXmlConvert.ToValue(schemaAttribute.DefaultValue, attribute.DataType);

            attribute.AutoIncrement = schemaAttribute.ReadAppInfoAsBoolean(CremaSchema.AttributeInfo, CremaSchema.AutoIncrement);
            attribute.Comment = schemaAttribute.ReadAppInfoAsString(CremaSchema.AttributeInfo, CremaSchema.Comment);
        }

        private void ReadColumn(XmlSchemaElement element, CremaDataTable dataTable)
        {
            var dataColumn = new CremaDataColumn()
            {
                InternalColumnName = element.Name,
                InternalComment = element.ReadDescription(),
            };

            this.ReadColumnDataType(element.ElementSchemaType as XmlSchemaSimpleType, dataColumn);

            if (element.MinOccursString == null)
            {
                dataColumn.InternalAllowDBNull = false;
            }

            if (string.IsNullOrEmpty(element.DefaultValue) == false)
            {
                dataColumn.InternalDefaultValue = CremaXmlConvert.ToValue(element.DefaultValue, dataColumn.DataType);
            }

            if (this.version >= new Version(3, 0))
            {
                this.ReadColumnInfo(element, dataColumn);
            }
            else
            {
                throw new NotImplementedException($"not supported version: '{this.version.Major}'");
            }

            dataTable.Columns.Add(dataColumn);
        }

        private void ReadColumnInfo(XmlSchemaAnnotated annotated, CremaDataColumn dataColumn)
        {
            dataColumn.InternalCreationInfo = annotated.ReadAppInfoAsSigunatureDate(CremaSchema.ColumnInfo, CremaSchema.Creator, CremaSchema.CreatedDateTime);
            dataColumn.InternalModificationInfo = annotated.ReadAppInfoAsSigunatureDate(CremaSchema.ColumnInfo, CremaSchema.Modifier, CremaSchema.ModifiedDateTime);
            dataColumn.InternalAutoIncrement = annotated.ReadAppInfoAsBoolean(CremaSchema.ColumnInfo, CremaSchema.AutoIncrement);
            dataColumn.InternalColumnID = annotated.ReadAppInfoAsGuid(CremaSchema.ColumnInfo, CremaSchema.ID);
            dataColumn.InternalTags = annotated.ReadAppInfoAsTagInfo(CremaSchema.ColumnInfo, CremaSchema.Tags);
            dataColumn.InternalReadOnly = annotated.ReadAppInfoAsBoolean(CremaSchema.ColumnInfo, CremaSchema.ReadOnly);
        }

        private void ReadTable(XmlSchemaElement element, CremaDataTable dataTable)
        {
            var complexType = element.ElementSchemaType as XmlSchemaComplexType;
            if (dataTable.InternalName != string.Empty && dataTable.InternalName != element.Name)
                throw new CremaDataException("대상 테이블과 스키마 이름이 일치하지 않습니다.");
            dataTable.InternalName = element.Name;

            if (this.itemName != null)
            {
                dataTable.InternalName = this.itemName.Name;
                dataTable.InternalCategoryPath = this.itemName.CategoryPath;
            }
            else
            {
                if (this.version >= new Version(3, 0))
                {
                    if (element.QualifiedName.Namespace != CremaSchema.BaseNamespace)
                    {
                        if (this.version >= new Version(4, 0))
                        {
                            dataTable.InternalName = element.Name;
                            dataTable.InternalCategoryPath = CremaDataSet.GetTableCategoryPath(this.DataSet, element.QualifiedName.Namespace);
                        }
                        else
                        {
                            dataTable.InternalName = CremaDataSet.GetTableName(this.DataSet, element.QualifiedName.Namespace);
                            dataTable.InternalCategoryPath = CremaDataSet.GetTableCategoryPath(this.DataSet, element.QualifiedName.Namespace);
                        }
                    }
                    else if (this.version == new Version(3, 0))
                    {
                        var categoryName = complexType.ReadAppInfoAsString(CremaSchema.TableInfo, CremaSchema.Category) ?? string.Empty;
                        var categoryPath = categoryName == string.Empty ? PathUtility.Separator : categoryName.WrapSeparator();
                        dataTable.InternalName = element.Name;
                        dataTable.InternalCategoryPath = categoryPath;
                    }
                    else
                    {
                        dataTable.InternalName = element.Name;
                        dataTable.InternalCategoryPath = complexType.ReadAppInfoAsString(CremaSchema.TableInfo, CremaSchema.CategoryPath, PathUtility.Separator);
                    }
                }
                else
                {
                    dataTable.InternalName = CremaDataSet.GetTableName(this.DataSet, element.QualifiedName.Namespace);
                    dataTable.InternalCategoryPath = CremaDataSet.GetTableCategoryPath(this.DataSet, element.QualifiedName.Namespace);
                }
            }

            dataTable.BeginLoadInternal();
            this.ReadTable(element.ElementSchemaType as XmlSchemaComplexType, dataTable);
            if (this.version < new Version(4, 0))
            {
                this.ReadChildTables(element.ElementSchemaType as XmlSchemaComplexType, dataTable);
            }
            dataTable.EndLoadInternal();

            this.tables.Add(dataTable.Name, dataTable);
            if (this.version < new Version(4, 0))
            {
                foreach (var item in dataTable.Childs)
                {
                    this.tables.Add(item.Name, item);
                }
            }

            if (dataTable.ParentName != string.Empty && this.tables.ContainsKey(dataTable.ParentName) == true)
            {
                dataTable.Parent = this.tables[dataTable.ParentName];
            }

            if (this.DataSet != null)
            {
                lock (CremaSchema.lockobj)
                {
                    this.DataSet.Tables.Add(dataTable);

                    if (dataTable.ParentName != string.Empty && dataTable.Parent == null && this.DataSet.Tables.Contains(dataTable.ParentName))
                    {
                        dataTable.Parent = this.DataSet.Tables[dataTable.ParentName];
                    }

                    foreach (var item in this.DataSet.Tables)
                    {
                        if (item.ParentName == dataTable.Name && item.Parent == null)
                        {
                            item.Parent = dataTable;
                        }
                    }

                    if (complexType.ContentModel != null)
                    {
                        var contentModel = complexType.ContentModel as XmlSchemaComplexContent;
                        var content = contentModel.Content as XmlSchemaComplexContentExtension;
                        var templatedParentName = content.BaseTypeName.Name.Substring(0, content.BaseTypeName.Name.Length - CremaSchema.ComplexTypeNameExtension.Length);
                        var baseComplexType = content.GetBaseType();
                        var templateCategoryPath = PathUtility.Separator;
                        if (this.version <= new Version(3, 0))
                            templateCategoryPath = (baseComplexType.ReadAppInfoAsString(CremaSchema.TableInfo, CremaSchema.Category) ?? string.Empty).Wrap(PathUtility.SeparatorChar);
                        else
                            templateCategoryPath = baseComplexType.ReadAppInfoAsString(CremaSchema.TableInfo, CremaSchema.CategoryPath) ?? PathUtility.Separator;
                        var templateNamespace = this.DataSet.TableNamespace + templateCategoryPath + templatedParentName;

                        var templatedParent = this.DataSet.Tables[templatedParentName, templateCategoryPath];
                        if (templatedParent != null)
                        {
                            dataTable.AttachTemplatedParent(templatedParent);
                        }
                        else
                        {
                            dataTable.AttachTemplatedParent(templateNamespace);
                        }
                    }
                    else if (this.itemName != null)
                    {
                        var tableName = this.DataSet.GetTableName(element.QualifiedName.Namespace);
                        var categoryPath = this.DataSet.GetTableCategoryPath(element.QualifiedName.Namespace);
                        var templatedParent = this.DataSet.Tables[tableName, categoryPath];

                        if (dataTable != templatedParent)
                        {
                            if (templatedParent != null)
                            {
                                dataTable.AttachTemplatedParent(templatedParent);
                            }
                            else
                            {
                                dataTable.AttachTemplatedParent(element.QualifiedName.Namespace);
                            }
                        }
                    }
                    else if (complexType.ReadAppInfoAsString(CremaSchema.TableInfo, CremaSchema.TemplateNamespace) != null)
                    {
                        var templateNamespace = complexType.ReadAppInfoAsString(CremaSchema.TableInfo, CremaSchema.TemplateNamespace);
                        dataTable.AttachTemplatedParent(templateNamespace);
                    }
                    else
                    {
                        var query = from item in this.DataSet.Tables
                                    where item != dataTable
                                    where item.TemplateNamespace == dataTable.Namespace
                                    select item;

                        foreach (var item in query.ToArray())
                        {
                            item.AttachTemplatedParent(dataTable);
                        }
                    }
                }
            }
        }

        private void ReadDataTables(XmlSchemaElement element)
        {
            var complexType = element.ElementSchemaType as XmlSchemaComplexType;

            foreach (var item in complexType.GetSequenceElements())
            {
                this.ReadTable(item, new CremaDataTable());
            }

            foreach (var item in element.GetKeyConstraints())
            {
                this.ReadKey(item);
            }

            foreach (var item in element.GetUniqueConstraints())
            {
                this.ReadUnique(item);
            }
        }

        private void ReadDataTable(XmlSchemaElement element)
        {
            var complexType = element.ElementSchemaType as XmlSchemaComplexType;

            foreach (var item in complexType.GetSequenceElements())
            {
                this.ReadTable(item, this.dataTable.Name == string.Empty ? this.dataTable : new CremaDataTable());
            }

            foreach (var item in element.GetKeyConstraints())
            {
                this.ReadKey(item);
            }

            foreach (var item in element.GetUniqueConstraints())
            {
                this.ReadUnique(item);
            }
        }

        private void ReadTable(XmlSchemaComplexType complexType, CremaDataTable dataTable)
        {
            if (this.version >= new Version(3, 0))
            {
                this.ReadTableInfo(complexType, dataTable);
            }
            else
            {
                throw new NotImplementedException($"not supported version: '{this.version.Major}'");
            }

            foreach (var item in complexType.GetAttributes())
            {
                this.ReadAttribute(item, dataTable);
            }

            foreach (var item in complexType.GetSequenceElements())
            {
                if (item.ElementSchemaType is XmlSchemaSimpleType)
                {
                    this.ReadColumn(item, dataTable);
                }
            }

            dataTable.SchemaHashValue = this.hashValue;
        }

        private void ReadChildTables(XmlSchemaComplexType complexType, CremaDataTable dataTable)
        {
            foreach (var item in complexType.GetSequenceElements())
            {
                if (item.ElementSchemaType is XmlSchemaSimpleType == false)
                {
                    var tableName = item.Name;
                    var childTable = new CremaDataTable(tableName, dataTable.CategoryPath);
                    childTable.BeginLoadInternal();
                    this.ReadTable(item.ElementSchemaType as XmlSchemaComplexType, childTable);
                    childTable.EndLoadInternal();
                    childTable.Parent = dataTable;
                }
            }
        }

        private void ReadColumnDataType(XmlSchemaSimpleType simpleType, CremaDataColumn column)
        {
            var typeName = simpleType.QualifiedName.Name;

            if (simpleType.QualifiedName.Namespace == XmlSchema.Namespace)
            {
                column.InternalDataType = CremaSchemaTypeUtility.GetType(typeName) ?? typeof(string);
            }
            else if (simpleType.QualifiedName.Name == typeof(Guid).GetSchemaTypeName() && simpleType.QualifiedName.Namespace == simpleType.GetSchema().TargetNamespace)
            {
                column.InternalDataType = typeof(Guid);
            }
            else
            {
                string categoryPath;
                if (simpleType.QualifiedName.Namespace == CremaSchema.BaseNamespace)
                {
                    if (this.version >= new Version(3, 5))
                    {
                        categoryPath = simpleType.ReadAppInfoAsString(CremaSchema.TypeInfo, CremaSchema.CategoryPath, PathUtility.Separator);
                    }
                    else
                    {
                        if (simpleType.Content is not XmlSchemaSimpleTypeRestriction xmlRestriction)
                        {
                            if (simpleType.Content is XmlSchemaSimpleTypeList == true)
                            {
                                simpleType = (simpleType.Content as XmlSchemaSimpleTypeList).BaseItemType;
                            }
                        }
                        var categoryName = simpleType.ReadAppInfoAsString(CremaSchema.TypeInfo, CremaSchema.Category) ?? string.Empty;
                        categoryPath = categoryName == string.Empty ? PathUtility.Separator : categoryName.WrapSeparator();
                    }
                }
                else
                {
                    if (this.version >= new Version(3, 0))
                    {
                        categoryPath = this.DataSet.GetTypeCategoryPath(simpleType.QualifiedName.Namespace);
                    }
                    else
                    {
                        categoryPath = PathUtility.Separator;
                    }
                }

                if (this.DataSet.Types.Contains(typeName, categoryPath) == false)
                {
                    this.ReadType(simpleType);
                }

                column.InternalCremaType = (InternalDataType)this.DataSet.Types[typeName, categoryPath];
            }
        }

        private void ReadType(XmlSchemaSimpleType simpleType)
        {
            if (this.DataSet != null && this.DataSet.Types.Contains(simpleType.Name) == true)
                return;
            var restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            var dataType = this.dataType ?? new CremaDataType();
            dataType.InternalName = simpleType.Name;
            dataType.BeginLoadData();

            if (restriction == null && simpleType.Content is XmlSchemaSimpleTypeList == true)
            {
                XmlSchemaSimpleType contentType = (simpleType.Content as XmlSchemaSimpleTypeList).BaseItemType;
                restriction = contentType.Content as XmlSchemaSimpleTypeRestriction;
                dataType.IsFlag = true;
            }

            this.ReadTypeMembers(restriction, dataType);
            this.ReadTypeInfo(simpleType, dataType);

            dataType.EndLoadData();
            dataType.AcceptChanges();

            if (simpleType.GetSchema() != this.schema)
            {
                if (Uri.TryCreate(simpleType.GetSchema().SourceUri, UriKind.Absolute, out Uri sourceUri))
                {
                    if (File.Exists(sourceUri.LocalPath) == true)
                        dataType.HashValue = HashUtility.GetHashValueFromFile(sourceUri.LocalPath);
                }
            }
            else
            {
                dataType.HashValue = this.hashValue;
            }

            if (this.DataSet != null)
            {
                this.DataSet.Types.Add(dataType);
            }
        }

        private void ReadTypeInfo(XmlSchemaSimpleType simpleType, CremaDataType dataType)
        {
            var contentType = simpleType;
            if (simpleType.Content as XmlSchemaSimpleTypeRestriction == null && simpleType.Content is XmlSchemaSimpleTypeList == true)
            {
                contentType = (simpleType.Content as XmlSchemaSimpleTypeList).BaseItemType;
            }

            if (this.version < new Version(3, 0))
            {
                throw new NotImplementedException($"not supported version: '{this.version.Major}'");
            }
            else if (this.version == new Version(3, 0))
            {
                simpleType = contentType;
            }

            dataType.InternalCreationInfo = simpleType.ReadAppInfoAsSigunatureDate(CremaSchema.TypeInfo, CremaSchema.Creator, CremaSchema.CreatedDateTime);
            dataType.InternalModificationInfo = simpleType.ReadAppInfoAsSigunatureDate(CremaSchema.TypeInfo, CremaSchema.Modifier, CremaSchema.ModifiedDateTime);
            dataType.InternalTypeID = simpleType.ReadAppInfoAsGuidVersion2(CremaSchema.TypeInfo, CremaSchema.ID, dataType.TypeName);
            if (this.version == new Version(3, 0))
            {
                var category = simpleType.ReadAppInfoAsString(CremaSchema.TypeInfo, CremaSchema.Category);
                if (string.IsNullOrEmpty(category) == true)
                    dataType.InternalCategoryPath = PathUtility.Separator;
                else
                    dataType.InternalCategoryPath = category.Wrap(PathUtility.SeparatorChar);
            }
            else
            {
                dataType.InternalCategoryPath = (simpleType.ReadAppInfoAsString(CremaSchema.TypeInfo, CremaSchema.CategoryPath) ?? PathUtility.Separator);
            }
            dataType.InternalComment = simpleType.ReadDescription();

            if (this.version > new Version(3, 0))
            {
                dataType.InternalTags = simpleType.ReadAppInfoAsTagInfo(CremaSchema.TypeInfo, CremaSchema.Tags);
            }
        }

        private void ReadTypeMembers(XmlSchemaSimpleTypeRestriction restriction, CremaDataType dataType)
        {
            foreach (var item in restriction.Facets)
            {
                if (item is XmlSchemaEnumerationFacet)
                {
                    this.ReadTypeMember(item as XmlSchemaEnumerationFacet, dataType);
                }
            }
        }

        private void ReadTypeMember(XmlSchemaEnumerationFacet facet, CremaDataType dataType)
        {
            if (facet.Annotation == null)
                return;

            var member = dataType.NewMember();
            member.Name = facet.Value;
            member.Comment = facet.ReadDescription();
            member.Value = facet.ReadAppInfoComfortableAsInt64(CremaSchema.TypeInfo, CremaSchema.Value);
            member.CreationInfo = facet.ReadAppInfoAsSigunatureDate(CremaSchema.TypeInfo, CremaSchema.Creator, CremaSchema.CreatedDateTime);
            member.ModificationInfo = facet.ReadAppInfoAsSigunatureDate(CremaSchema.TypeInfo, CremaSchema.Modifier, CremaSchema.ModifiedDateTime);
            member.IsEnabled = facet.ReadAppInfoAsBoolean(CremaSchema.TypeInfo, CremaSchema.Enable, true);
            member.MemberID = facet.ReadAppInfoAsGuidVersion2(CremaSchema.TypeInfo, CremaSchema.ID, dataType.Name + "_" + member.Name);
            member.Tags = facet.ReadAppInfoAsTagInfo(CremaSchema.TypeInfo, CremaSchema.Tags);
            dataType.Members.Add(member);
        }

        private CremaDataTable GetTable(XmlSchemaKey constraint, string extension)
        {
            if (this.version >= new Version(3, 0))
            {
                var keyName = constraint.Name.Substring(0, constraint.Name.Length - extension.Length);
                if (this.itemName == null)
                {
                    return this.tables[keyName];
                }
                else
                {
                    var tableName = CremaDataSet.GetTableName(this.DataSet, constraint.QualifiedName.Namespace);

                    if (keyName == tableName)
                    {
                        return this.tables[this.itemName.Name];
                    }
                    else
                    {
                        tableName = Regex.Replace(keyName, string.Format("(^{0})([.].*)", tableName), this.itemName.Name + "$2");
                        return this.tables[tableName];
                    }
                }
            }
            else
            {
                throw new NotImplementedException($"not supported version: '{this.version.Major}'");
            }
            throw new CremaDataException();
        }

        private CremaDataTable GetTable(XmlSchemaUnique constraint, string extension)
        {
            if (this.version >= new Version(3, 0))
            {
                var keyName = constraint.Name.Substring(0, constraint.Name.Length - extension.Length);
                if (this.version >= new Version(3, 5)) // 3.5
                {
                    var items = StringUtility.Split(keyName, '.');
                    keyName = string.Join(".", items.Take(items.Length - 1));
                }

                if (this.itemName == null)
                {
                    return this.tables[keyName];
                }
                else
                {
                    var tableName = CremaDataSet.GetTableName(this.DataSet, constraint.QualifiedName.Namespace);

                    if (keyName == tableName)
                    {
                        return this.tables[this.itemName.Name];
                    }
                    else
                    {
                        tableName = Regex.Replace(keyName, string.Format("(^{0})([.].*)", tableName), this.itemName.Name + "$2");
                        return this.tables[tableName];
                    }
                }
            }
            else
            {
                throw new NotImplementedException($"not supported version: '{this.version.Major}'");
            }
            throw new CremaDataException();
        }

        private string GetTableNameObsolete(XmlSchemaIdentityConstraint key)
        {
            var xPath = key.Selector.XPath;
            var strArray = xPath.Split(new char[] { '/', ':' });
            var name = strArray[strArray.Length - 1];
            if ((name == null) || (name.Length == 0))
            {
                throw new CremaDataException();
            }
            return XmlConvert.DecodeName(name);
        }
    }
}

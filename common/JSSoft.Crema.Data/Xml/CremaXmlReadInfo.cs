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

using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace JSSoft.Crema.Data.Xml
{
    public class CremaXmlReadInfo
    {
        private readonly Version version = new(2, 0);

        public CremaXmlReadInfo(string xmlPath)
        {
            if (FileUtility.IsAbsolute(xmlPath) == false)
                throw new ArgumentException($"\"{xmlPath}\" 은(는) 절대 경로가 아닙니다.", nameof(xmlPath));
            if (File.Exists(xmlPath) == false)
                throw new FileNotFoundException($"\"{xmlPath}\" 경로를 찾을 수 없습니다.");

            this.XmlPath = xmlPath;
            this.SchemaPath = Path.ChangeExtension(xmlPath, CremaSchema.SchemaExtension);
            this.XmlSize = new FileInfo(xmlPath).Length;

            using (var reader = XmlReader.Create(this.XmlPath))
            {
                reader.MoveToContent();
                var text = reader.GetAttribute(CremaSchema.Version);
                if (string.IsNullOrEmpty(text) == false)
                {
                    Version.TryParse(text, out this.version);
                }
            }

            if (File.Exists(this.SchemaPath) == false)
            {
                using (var reader = XmlReader.Create(xmlPath))
                {
                    reader.MoveToContent();
                    if (Version.TryParse(reader.GetAttribute(CremaSchema.Version), out Version version) == false)
                    {
                        version = new Version(2, 0);
                    }

                    if (version.Major >= 3)
                    {
                        FindSchemaLocation(reader, xmlPath, out var schemaPath, out string tableNamespace);
                        var categoryPath = CremaDataSet.GetTableCategoryPath(CremaSchema.TableNamespace, tableNamespace);
                        var tableName = CremaDataSet.GetTableName(CremaSchema.TableNamespace, tableNamespace);
                        this.SchemaPath = schemaPath;
                        this.ItemName = new ItemName(categoryPath, tableName);
                    }
                    else
                    {
                        throw new NotImplementedException($"not supported version: '{version.Major}'");
                    }
                }
                this.IsInherited = true;
            }

            this.RelativeSchemaPath = UriUtility.MakeRelative(this.XmlPath, this.SchemaPath);
        }

        public string[] GetTypePaths()
        {
            var prefix = "xs";
            var schemaDirectory = System.IO.Path.GetDirectoryName(this.SchemaPath);
            var namespaceResolver = new XmlNamespaceManager(new NameTable());
            namespaceResolver.AddNamespace(prefix, XmlSchema.Namespace);
            var doc = XDocument.Load(this.SchemaPath);
            var query = from item in doc.XPathSelectElements($"/{prefix}:schema/{prefix}:import", namespaceResolver)
                        let schemaLocation = item.Attribute(XName.Get("schemaLocation")).Value
                        let uri = new Uri(UriUtility.Combine(schemaDirectory, schemaLocation))
                        select uri.LocalPath;

            return query.ToArray();
        }

        public string XmlPath { get; }

        public string SchemaPath { get; }

        public string RelativeSchemaPath { get; }

        public ItemName ItemName { get; }

        public bool IsInherited { get; }

        public long XmlSize { get; }

        public Version Version => this.version;

        private static void FindSchemaLocation(XmlReader reader, string xmlPath, out string xsdPath, out string tableNamespace)
        {
            var value = reader.GetAttribute("schemaLocation", XmlSchema.InstanceNamespace);
            var ss = value.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            if (ss.Length != 2)
                throw new CremaDataException();

            tableNamespace = ss[0];
            xsdPath = ss[1];

            if (FileUtility.IsAbsolute(xsdPath) == false)
            {
                xsdPath = new Uri(new Uri(xmlPath), xsdPath).LocalPath;
            }
        }
    }
}

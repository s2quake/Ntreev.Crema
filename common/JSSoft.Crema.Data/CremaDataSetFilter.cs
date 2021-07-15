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

using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace JSSoft.Crema.Data
{
    [Serializable]
    public class CremaDataSetFilter : ISerializable, IXmlSerializable
    {
        private string[] tables = new string[] { };
        private string[] types = new string[] { };

        public CremaDataSetFilter()
        {
        }

        protected CremaDataSetFilter(SerializationInfo info, StreamingContext context)
        {
            this.TypeExpression = info.GetString(nameof(TypeExpression));
            this.TableExpression = info.GetString(nameof(TableExpression));
            this.OmitType = info.GetBoolean(nameof(OmitType));
            this.OmitTable = info.GetBoolean(nameof(OmitTable));
            this.OmitContent = info.GetBoolean(nameof(OmitContent));
        }

        public string[] FilterTypes(string path, string searchPattern)
        {
            var paths = DirectoryUtility.GetAllFiles(path, searchPattern);
            if (this.OmitType == false)
            {
                if (this.Types.Any() == true)
                {
                    var query = from item in paths
                                where Filter(this.Types, path, item)
                                select item;
                    return query.ToArray();
                }
                return paths;
            }
            return new string[] { };
        }

        public string[] FilterTables(string path, string searchPattern)
        {
            var paths = DirectoryUtility.GetAllFiles(path, searchPattern);
            if (this.OmitTable == false)
            {
                if (this.Tables.Any() == true)
                {
                    var query = from item in paths
                                where Filter(this.Tables, path, item)
                                select item;
                    return query.ToArray();
                }
                return paths;
            }
            return new string[] { };
        }

        public string[] Types
        {
            get => this.types;
            set => this.types = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string[] Tables
        {
            get => this.tables;
            set => this.tables = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool OmitType { get; set; }

        public bool OmitTable { get; set; }

        public bool OmitContent { get; set; }

        public string TypeExpression
        {
            get => string.Join(";", this.Types);
            set => this.Types = StringUtility.Split(value, ';');
        }

        public string TableExpression
        {
            get => string.Join(";", this.Tables);
            set => this.Tables = StringUtility.Split(value, ';');
        }

        public static CremaDataSetFilter Default { get; } = new CremaDataSetFilter();

        private static bool Filter(string[] patterns, string basePath, string itemPath)
        {
            var namePattern = string.Join(";", patterns.Where(item => item.IndexOf(PathUtility.SeparatorChar) < 0));
            var pathPattern = string.Join(";", patterns.Where(item => item.IndexOf(PathUtility.SeparatorChar) >= 0));
            var path = FileUtility.RemoveExtension(itemPath);
            var relativePath = UriUtility.MakeRelativeOfDirectory(basePath, path);
            var items = StringUtility.SplitPath(relativePath);
            var itemName = ItemName.Create(items);

            if (namePattern != string.Empty && StringUtility.GlobMany(itemName.Name, namePattern) == true)
            {
                return true;
            }

            if (pathPattern != string.Empty && StringUtility.GlobMany(itemName.CategoryPath, pathPattern) == true)
            {
                return true;
            }

            return false;
        }

        #region ISerializable

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(TypeExpression), this.TypeExpression);
            info.AddValue(nameof(TableExpression), this.TableExpression);
            info.AddValue(nameof(OmitType), this.OmitType);
            info.AddValue(nameof(OmitTable), this.OmitTable);
            info.AddValue(nameof(OmitContent), this.OmitContent);
        }

        #endregion

        #region IXmlSerializable

        XmlSchema IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();

            this.TypeExpression = reader.ReadElementContentAsString();
            this.TableExpression = reader.ReadElementContentAsString();
            this.OmitType = reader.ReadElementContentAsBoolean();
            this.OmitTable = reader.ReadElementContentAsBoolean();
            this.OmitContent = reader.ReadElementContentAsBoolean();

            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(nameof(CremaDataSetFilter));

            writer.WriteElementString(nameof(TypeExpression), this.TypeExpression);
            writer.WriteElementString(nameof(TableExpression), this.TableExpression);
            writer.WriteElementString(nameof(OmitType), XmlConvert.ToString(this.OmitType));
            writer.WriteElementString(nameof(OmitTable), XmlConvert.ToString(this.OmitTable));
            writer.WriteElementString(nameof(OmitContent), XmlConvert.ToString(this.OmitContent));

            writer.WriteEndElement();
        }

        #endregion
    }
}

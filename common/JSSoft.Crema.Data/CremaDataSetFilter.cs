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
    public class CremaDataSetFilter
    {
        public string[] FilterTypes(string path, string searchPattern)
        {
            var paths = DirectoryUtility.GetAllFiles(path, searchPattern);
            if (this.Types.Any() == true)
            {
                if (this.OmitType == false)
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
            if (this.Tables.Any() == true)
            {
                if (this.OmitTable == false)
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

        public string[] Types { get; set; } = new string[] { };

        public string[] Tables { get; set; } = new string[] { };

        public bool OmitType { get; set; }

        public bool OmitTable { get; set; }

        public bool OmitContent { get; set; }

        public static CremaDataSetFilter Default { get; } = new CremaDataSetFilter();

        internal string TypeExpression => string.Join(';', this.Types);

        internal string TableExpression => string.Join(';', this.Tables);

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
    }
}

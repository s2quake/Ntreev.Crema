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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Crema.Data.Random;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.Random;
using System;

namespace JSSoft.Crema.Data.Test
{
    [TestClass]
    public class CremaTemplate_NewTest
    {
        [TestMethod]
        public void New()
        {
            var table = new CremaDataTable();
            var template = new CremaTemplate(table);

            Assert.AreNotEqual(Guid.Empty, template.TableID);
            Assert.AreEqual(string.Empty, template.Name);
            Assert.AreEqual(string.Empty, template.TableName);
            Assert.AreEqual(string.Empty, template.ParentName);
            Assert.AreEqual(PathUtility.Separator, template.CategoryPath);
            Assert.AreEqual(CremaSchema.TableNamespace + template.CategoryPath + template.Name, template.Namespace);
            Assert.AreEqual(string.Empty, template.Comment);
            Assert.AreEqual(TagInfo.All, template.Tags);
            Assert.AreEqual(TagInfo.All, template.DerivedTags);
            Assert.AreEqual(table, template.DataTable);
            Assert.AreEqual(0, template.Columns.Count);
            Assert.AreEqual(table.SignatureDateProvider, template.SignatureDateProvider);
            Assert.IsFalse(template.IsModified);

            Assert.AreNotEqual(template.ModificationInfo, SignatureDate.Empty);
        }

        [TestMethod]
        public void NewWithChildTable()
        {
            var table = Random.CremaDataTableExtensions.CreateRandomTable();
            var child = table.Childs.Add(RandomUtility.NextIdentifier());
            var template = new CremaTemplate(child);

            Assert.AreNotEqual(Guid.Empty, template.TableID);
            Assert.AreEqual(child.Name, template.Name);
            Assert.AreEqual(child.TableName, template.TableName);
            Assert.AreEqual(table.TableName, template.ParentName);
            Assert.AreEqual(PathUtility.Separator, template.CategoryPath);
            Assert.AreEqual(CremaSchema.TableNamespace + template.CategoryPath + template.Name, template.Namespace);
            Assert.AreEqual(string.Empty, template.Comment);
            Assert.AreEqual(TagInfo.All, template.Tags);
            Assert.AreEqual(TagInfo.All, template.DerivedTags);
            Assert.AreEqual(child, template.DataTable);
            Assert.AreEqual(0, template.Columns.Count);
            Assert.AreEqual(child.SignatureDateProvider, template.SignatureDateProvider);
            Assert.IsFalse(template.IsModified);

            Assert.AreNotEqual(template.ModificationInfo, SignatureDate.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NewWithNull()
        {
            new CremaTemplate(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NewWithDerivedTable()
        {
            var dataSet = CremaDataSetExtensions.CreateRandomSet();
            var table = dataSet.Tables.Random(item => item.TemplateNamespace != string.Empty);
            new CremaTemplate(table);
        }
    }
}
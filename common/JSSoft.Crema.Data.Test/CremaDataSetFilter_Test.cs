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
using JSSoft.Crema.Data.Xml.Schema;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace JSSoft.Crema.Data.Test
{
    [TestClass]
    public class CremaDataSetFilter_Test
    {
        [TestMethod]
        public void Serialize_Empty_Test()
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            var filter1 = new CremaDataSetFilter();
            formatter.Serialize(stream, filter1);
            stream.Position = 0;
            var filter2 = formatter.Deserialize(stream) as CremaDataSetFilter;
            Assert.AreEqual(filter1.TypeExpression, filter2.TypeExpression);
            Assert.AreEqual(filter1.TableExpression, filter2.TableExpression);
            Assert.AreEqual(filter1.OmitType, filter2.OmitType);
            Assert.AreEqual(filter1.OmitTable, filter2.OmitTable);
            Assert.AreEqual(filter1.OmitContent, filter2.OmitContent);
        }

        [TestMethod]
        public void Serialize_Full_Test()
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            var filter1 = new CremaDataSetFilter()
            {
                TableExpression = "a;b;c",
                TypeExpression = "c;b;a",
                OmitContent = true,
                OmitTable = false,
                OmitType = true,
            };
            formatter.Serialize(stream, filter1);
            stream.Position = 0;
            var filter2 = formatter.Deserialize(stream) as CremaDataSetFilter;
            Assert.AreEqual(filter1.TypeExpression, filter2.TypeExpression);
            Assert.AreEqual(filter1.TableExpression, filter2.TableExpression);
            Assert.AreEqual(filter1.OmitType, filter2.OmitType);
            Assert.AreEqual(filter1.OmitTable, filter2.OmitTable);
            Assert.AreEqual(filter1.OmitContent, filter2.OmitContent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Serialize_Types_Null_Test()
        {
            var filter = new CremaDataSetFilter();
            filter.Types = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Serialize_Tables_Null_Test()
        {
            var filter = new CremaDataSetFilter();
            filter.Tables = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Serialize_TypeExpression_Null_Test()
        {
            var filter = new CremaDataSetFilter();
            filter.TypeExpression = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Serialize_TableExpression_Null_Test()
        {
            var filter = new CremaDataSetFilter();
            filter.TableExpression = null;
        }
    }
}

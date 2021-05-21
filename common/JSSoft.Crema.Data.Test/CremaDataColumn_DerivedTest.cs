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
using JSSoft.Library.Random;
using System;
using System.Linq;

namespace JSSoft.Crema.Data.Test
{
    [TestClass]
    public class CremaDataColumn_DerivedTest
    {
        private readonly CremaDataSet dataSet = new();
        private readonly CremaDataTable dataTable;
        private readonly CremaDataTable derivedTable;
        private readonly CremaDataColumn[] columns;

        public CremaDataColumn_DerivedTest()
        {
            Random.CremaDataTableExtensions.MinRowCount = 20;
            Random.CremaDataTableExtensions.MaxRowCount = 100;
            this.dataSet.AddRandomType();
            this.dataSet.AddRandomTable();
            this.dataSet.AddDerivedTable();
            this.dataTable = this.dataSet.Tables.Random(item => item.TemplateNamespace == string.Empty);
            this.derivedTable = this.dataTable.DerivedTables.First();
            this.columns = this.dataSet.Tables.SelectMany(item => item.Columns).ToArray();
        }

        [TestMethod]
        public void SetName()
        {
            var columnName = RandomUtility.NextIdentifier();
            var column1 = this.RandomOrDefault(item => item.Table.TemplateNamespace == string.Empty);
            var column2 = this.GetDerivedColumn(column1);
            column1.ColumnName = columnName;
            Assert.AreEqual(column1.ColumnName, column2.ColumnName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetName_Fail()
        {
            var columnName = RandomUtility.NextIdentifier();
            var column1 = this.RandomOrDefault();
            var column2 = this.GetDerivedColumn(column1);
            column2.ColumnName = columnName;
        }

        private CremaDataColumn GetDerivedColumn(CremaDataColumn dataColumn)
        {
            return this.derivedTable.Columns[dataColumn.ColumnName];
        }

        private CremaDataColumn RandomOrDefault()
        {
            return this.RandomOrDefault(item => true);
        }

        private CremaDataColumn RandomOrDefault(Func<CremaDataColumn, bool> predicate)
        {
            var column = this.columns.RandomOrDefault(predicate);
            if (column == null)
                Assert.Inconclusive("column is null");
            return column;
        }

    }
}

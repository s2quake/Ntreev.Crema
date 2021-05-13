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
using System;
using System.Linq;

namespace JSSoft.Crema.Data.Test
{
    [TestClass]
    public class CremaDataColumn_ChangeUInt32ToOtherTypeTest : CremaDataColumn_ChangeTypeTestBase
    {
        private const uint maxOADateValue = 2958465;

        public CremaDataColumn_ChangeUInt32ToOtherTypeTest()
            : base(typeof(uint))
        {

        }

        [TestMethod]
        public void DBNullUInt32ToOther()
        {
            this.AddRows(DBNull.Value);
            foreach (var item in CremaDataTypeUtility.GetBaseTypes().Where(item => item != this.column.DataType))
            {
                try
                {
                    column.DataType = typeof(uint);
                }
                catch (FormatException)
                {

                }
            }
        }

        [TestMethod]
        public void UInt32ToBoolean()
        {
            this.AddRows((uint)0, (uint)1);
            column.DataType = typeof(bool);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt32ToBoolean_Fail()
        {
            this.AddRows((uint)2);
            column.DataType = typeof(bool);
        }

        [TestMethod]
        public void UInt32ToSingle()
        {
            this.AddRows((uint)0, (uint)1);
            column.DataType = typeof(float);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt32ToSingle_Fail()
        {
            this.AddRows(uint.MaxValue);
            column.DataType = typeof(float);
        }

        [TestMethod]
        public void UInt32ToDouble()
        {
            this.AddRows(uint.MinValue, uint.MaxValue);
            column.DataType = typeof(double);
        }

        [TestMethod]
        public void UInt32ToInt8()
        {
            this.AddRows((uint)0, (uint)sbyte.MaxValue);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt32ToInt8_Fail()
        {
            this.AddRows(uint.MaxValue);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        public void UInt32ToUInt8()
        {
            this.AddRows((uint)byte.MinValue, (uint)byte.MaxValue);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt32ToUInt8_Fail()
        {
            this.AddRows(uint.MaxValue);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        public void UInt32ToInt16()
        {
            this.AddRows(uint.MinValue, (uint)short.MaxValue);
            column.DataType = typeof(short);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt32ToInt16_Fail()
        {
            this.AddRows(uint.MaxValue);
            column.DataType = typeof(short);
        }

        [TestMethod]
        public void UInt32ToInt32()
        {
            this.AddRows(uint.MinValue, (uint)int.MaxValue);
            column.DataType = typeof(int);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void UInt32ToInt32_Fail()
        {
            this.AddRows(uint.MaxValue);
            column.DataType = typeof(int);
        }

        [TestMethod]
        public void UInt32ToInt64()
        {
            this.AddRows(uint.MinValue, uint.MaxValue);
            column.DataType = typeof(long);
        }

        [TestMethod]
        public void UInt32ToUInt64()
        {
            this.AddRows(uint.MinValue, uint.MaxValue);
            column.DataType = typeof(ulong);
        }

        [TestMethod]
        public void UInt32ToDateTime()
        {
            this.AddRows(uint.MinValue, maxOADateValue);
            column.DataType = typeof(DateTime);
        }

        [TestMethod]
        public void UInt32ToTimeSpan()
        {
            this.AddRows(uint.MinValue, uint.MaxValue);
            column.DataType = typeof(TimeSpan);
        }

        [TestMethod]
        public void UInt32ToString()
        {
            this.AddRows(uint.MinValue, uint.MaxValue);
            column.DataType = typeof(string);
        }
    }
}

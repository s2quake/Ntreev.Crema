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
    public class CremaDataColumn_ChangeInt64ToOtherTypeTest : CremaDataColumn_ChangeTypeTestBase
    {
        public CremaDataColumn_ChangeInt64ToOtherTypeTest()
            : base(typeof(long))
        {

        }

        [TestMethod]
        public void DBNullInt64ToOther()
        {
            this.AddRows(DBNull.Value);
            foreach (var item in CremaDataTypeUtility.GetBaseTypes().Where(item => item != this.column.DataType))
            {
                try
                {
                    column.DataType = typeof(long);
                }
                catch (FormatException)
                {

                }
            }
        }

        [TestMethod]
        public void Int64ToBoolean()
        {
            this.AddRows((long)0, (long)1);
            column.DataType = typeof(bool);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToBoolean_Fail()
        {
            this.AddRows((long)2);
            column.DataType = typeof(bool);
        }

        [TestMethod]
        public void Int64ToSingle()
        {
            this.AddRows((long)-16777216, (long)16777216);
            column.DataType = typeof(float);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToSingle_Fail1()
        {
            this.AddRows(long.MinValue + 1);
            column.DataType = typeof(float);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToSingle_Fail2()
        {
            this.AddRows(long.MaxValue - 1);
            column.DataType = typeof(float);
        }

        [TestMethod]
        public void Int64ToDouble()
        {
            this.AddRows(long.MinValue);
            column.DataType = typeof(double);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToDouble_Fail()
        {
            this.AddRows(long.MaxValue);
            column.DataType = typeof(double);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToDouble_Fail1()
        {
            this.AddRows(long.MinValue + 1);
            column.DataType = typeof(double);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToDouble_Fail2()
        {
            this.AddRows(long.MaxValue);
            column.DataType = typeof(double);
        }

        [TestMethod]
        public void Int64ToInt8()
        {
            this.AddRows((long)sbyte.MinValue, (long)sbyte.MaxValue);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToInt8_Fail1()
        {
            this.AddRows(long.MinValue);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToInt8_Fail2()
        {
            this.AddRows(long.MaxValue);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        public void Int64ToUInt8()
        {
            this.AddRows((long)byte.MinValue, (long)byte.MaxValue);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToUInt8_Fail1()
        {
            this.AddRows(long.MinValue);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToUInt8_Fail2()
        {
            this.AddRows(long.MaxValue);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        public void Int64ToUInt16()
        {
            this.AddRows((long)0, (long)ushort.MaxValue);
            column.DataType = typeof(ushort);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToUInt16_Fail1()
        {
            this.AddRows(long.MinValue);
            column.DataType = typeof(ushort);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToUInt16_Fail2()
        {
            this.AddRows(long.MaxValue);
            column.DataType = typeof(ushort);
        }

        [TestMethod]
        public void Int64ToInt32()
        {
            this.AddRows((long)int.MinValue, (long)int.MaxValue);
            column.DataType = typeof(int);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToInt32_Fail1()
        {
            this.AddRows(long.MinValue);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToInt32_Fail2()
        {
            this.AddRows(long.MaxValue);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        public void Int64ToUInt32()
        {
            this.AddRows((long)0, (long)uint.MaxValue);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Int64ToUInt32_Fail()
        {
            this.AddRows(long.MinValue);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        public void Int64ToInt64()
        {
            this.AddRows(long.MinValue, long.MaxValue);
            column.DataType = typeof(long);
        }

        [TestMethod]
        public void Int64ToUInt64()
        {
            this.AddRows(long.MinValue, (long)0, long.MaxValue);
            column.DataType = typeof(ulong);
        }

        [TestMethod]
        public void Int64ToDateTime()
        {
            this.AddRows(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
            column.DataType = typeof(DateTime);
        }

        [TestMethod]
        public void Int64ToTimeSpan()
        {
            this.AddRows(TimeSpan.MinValue.Ticks, TimeSpan.MaxValue.Ticks);
            column.DataType = typeof(TimeSpan);
        }

        [TestMethod]
        public void Int64ToString()
        {
            this.AddRows(long.MinValue, long.MaxValue);
            column.DataType = typeof(string);
        }
    }
}

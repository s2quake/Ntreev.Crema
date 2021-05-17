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
using JSSoft.Library.Random;
using System;
using System.Linq;

namespace JSSoft.Crema.Data.Test
{
    [TestClass]
    public class CremaDataColumn_ChangeSingleToOtherTypeTest : CremaDataColumn_ChangeTypeTestBase
    {
        public CremaDataColumn_ChangeSingleToOtherTypeTest()
            : base(typeof(float))
        {

        }

        [TestMethod]
        public void DBNullSingleToOther()
        {
            this.AddRows(DBNull.Value);
            foreach (var item in CremaDataTypeUtility.GetBaseTypes().Where(item => item != this.column.DataType))
            {
                try
                {
                    column.DataType = typeof(float);
                }
                catch (FormatException)
                {

                }
            }
        }

        [TestMethod]
        public void SingleToBoolean()
        {
            this.AddRows((float)0.0f, (float)1.0f);
            column.DataType = typeof(bool);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToBoolean_Fail()
        {
            this.AddRows((float)2.0f);
            column.DataType = typeof(bool);
        }

        [TestMethod]
        public void SingleToDouble()
        {
            this.AddRows(float.MinValue, float.MaxValue, RandomUtility.Next(typeof(float)));
            column.DataType = typeof(double);
        }

        [TestMethod]
        public void SingleToInt8()
        {
            this.AddRows(-128.0f, 127.0f);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToInt8_Fail()
        {
            this.AddRows(float.MaxValue);
            column.DataType = typeof(sbyte);
        }

        [TestMethod]
        public void SingleToUInt8()
        {
            this.AddRows(0.0f, 255.0f);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToUInt8_Fail()
        {
            this.AddRows(float.MaxValue);
            column.DataType = typeof(byte);
        }

        [TestMethod]
        public void SingleToInt16()
        {
            this.AddRows(-32768.0f, 32767.0f);
            column.DataType = typeof(short);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToInt16_Fail()
        {
            this.AddRows(float.MaxValue);
            column.DataType = typeof(short);
        }

        [TestMethod]
        public void SingleToUInt16()
        {
            this.AddRows(0.0f, 65535.0f);
            column.DataType = typeof(ushort);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToUInt16_Fail()
        {
            this.AddRows(float.MinValue, float.MaxValue, RandomUtility.Next(typeof(float)));
            column.DataType = typeof(ushort);
        }

        [TestMethod]
        public void SingleToInt32()
        {
            this.AddRows(0.0f, 1.0f, 2.0f);
            column.DataType = typeof(int);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToInt32_Fail()
        {
            this.AddRows(1.1f);
            column.DataType = typeof(int);
        }

        [TestMethod]
        public void SingleToUInt32()
        {
            this.AddRows(0.0f, 1.0f, 2.0f);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToUInt32_Fail1()
        {
            this.AddRows(1.1f);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToUInt32_Fail2()
        {
            this.AddRows(-1.1f);
            column.DataType = typeof(uint);
        }

        [TestMethod]
        public void SingleToInt64()
        {
            this.AddRows(-1.0f, 0.0f, 1.0f);
            column.DataType = typeof(long);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToInt64_Fail()
        {
            this.AddRows(-1.1f);
            column.DataType = typeof(long);
        }

        [TestMethod]
        public void SingleToUInt64()
        {
            this.AddRows(0.0f, 1.0f, 2.0f);
            column.DataType = typeof(ulong);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToUInt64_Fail1()
        {
            this.AddRows(2.1f);
            column.DataType = typeof(ulong);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToUInt64_Fail2()
        {
            this.AddRows(-2.1f);
            column.DataType = typeof(ulong);
        }

        [TestMethod]
        public void SingleToDateTime()
        {
            this.AddRows(0.0f);
            column.DataType = typeof(DateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToDateTime_Fail1()
        {
            this.AddRows(-1.0f);
            column.DataType = typeof(DateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToDateTime_Fail2()
        {
            this.AddRows(float.MaxValue);
            column.DataType = typeof(DateTime);
        }

        [TestMethod]
        public void SingleToTimeSpan()
        {
            this.AddRows(-1.0f, 0.0f, 1.0f);
            column.DataType = typeof(TimeSpan);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToTimeSpan_Fail1()
        {
            this.AddRows(float.MinValue);
            column.DataType = typeof(TimeSpan);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SingleToTimeSpan_Fail2()
        {
            this.AddRows(float.MaxValue);
            column.DataType = typeof(TimeSpan);
        }

        [TestMethod]
        public void SingleToString()
        {
            this.AddRows(float.MinValue, float.MaxValue, RandomUtility.Next(typeof(float)));
            column.DataType = typeof(string);
        }
    }
}

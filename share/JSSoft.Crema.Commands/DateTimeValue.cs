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

using System;
using System.ComponentModel;
using System.Globalization;

namespace JSSoft.Crema.Commands
{
    [TypeConverter(typeof(DateTimeValueConverter))]
    public class DateTimeValue
    {
        public const string nowString = "now";
        private const string formatString = "HH:mm";

        private readonly string text;

        public DateTimeValue(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (text == DateTimeValue.nowString)
            {
                this.text = text;
                this.Milliseconds = 0;
            }
            else if (text == string.Empty)
            {
                this.text = text;
                this.Milliseconds = 60000;
                this.text = DateTime.Now.AddMilliseconds(this.Milliseconds).ToString(formatString);
                //this.value = DateTime.Now + new TimeSpan(0, 0, 5);
            }
            else
            {
                var dateTime = new DateTime(DateTime.ParseExact(text, formatString, CultureInfo.InvariantCulture).Ticks, DateTimeKind.Local);
                if (dateTime < DateTime.Now)
                    dateTime += new TimeSpan(TimeSpan.TicksPerDay);
                var timeSpan = (dateTime - DateTime.Now);
                this.Milliseconds = (int)timeSpan.TotalMilliseconds;
                this.text = dateTime.ToString(formatString);
            }
        }

        public override string ToString()
        {
            return this.text;
        }

        public int Milliseconds { get; }
    }
}

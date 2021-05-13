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
using System;

namespace JSSoft.Crema.Data
{
    public static class CremaConvert
    {
        public static object ChangeType(object value, Type dataType)
        {
            return ChangeType(value, dataType, false);
        }

        public static bool VerifyChangeType(object value, Type dataType)
        {
            try
            {
                return ChangeType(value, dataType, true) is not DBNull;
            }
            catch
            {
                return false;
            }
        }

        public static string ToString(object value)
        {
            if (value == null)
                return null;
            if (value == DBNull.Value)
                return string.Empty;
            return (string)ChangeType(value, typeof(string));
        }

        private static void ValidateChangeType(object value, Type dataType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            if (CremaDataTypeUtility.IsBaseType(dataType) == false)
                throw new ArgumentException(string.Format(Resources.Exception_TypeCannotBeUsed_Format, dataType.Name), nameof(dataType));
        }

        private static object ConvertTo(string @string, Type dataType, bool verify)
        {
            if (dataType == typeof(bool))
            {
                if (bool.TryParse(@string, out var @bool) == true)
                    return ValidateRoundTrip<string, bool>(@string.ToLower(), @bool, (v) => v.ToString().ToLower(), verify);
            }
            else if (dataType == typeof(Guid))
            {
                if (Guid.TryParse(@string, out var guid) == true)
                    return ValidateRoundTrip<string, Guid>(@string, guid, (v) => v.ToString(), verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (DateTime.TryParse(@string, out var dateTime) == true)
                    return ValidateRoundTrip<string, DateTime>(@string, dateTime, (v) => v.ToString("o"), verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(@string, out var timeSpan) == true)
                    return ValidateRoundTrip<string, TimeSpan>(@string, timeSpan, (v) => @string, verify);
            }
            else if (dataType == typeof(double))
            {
                if (double.TryParse(@string, out var @double) == true)
                    return ValidateRoundTrip<string, double>(@string, @double, (v) => @string, verify);
            }
            else if (dataType == typeof(float))
            {
                if (float.TryParse(@string, out var @float) == true)
                    return ValidateRoundTrip<string, float>(@string, @float, (v) => @string, verify);
            }
            else if (dataType == typeof(long))
            {
                if (long.TryParse(@string, out var @long) == true)
                    return ValidateRoundTrip<string, long>(@string, @long, (v) => @string, verify);
            }
            else if (dataType == typeof(ulong))
            {
                if (ulong.TryParse(@string, out var @ulong) == true)
                    return ValidateRoundTrip<string, ulong>(@string, @ulong, (v) => @string, verify);
            }
            else if (dataType == typeof(int))
            {
                if (int.TryParse(@string, out var @int) == true)
                    return ValidateRoundTrip<string, int>(@string, @int, (v) => @string, verify);
            }
            else if (dataType == typeof(uint))
            {
                if (uint.TryParse(@string, out var @uint) == true)
                    return ValidateRoundTrip<string, uint>(@string, @uint, (v) => @string, verify);
            }
            else if (dataType == typeof(short))
            {
                if (short.TryParse(@string, out var @short) == true)
                    return ValidateRoundTrip<string, short>(@string, @short, (v) => @string, verify);
            }
            else if (dataType == typeof(ushort))
            {
                if (ushort.TryParse(@string, out var @ushort) == true)
                    return ValidateRoundTrip<string, ushort>(@string, @ushort, (v) => @string, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (sbyte.TryParse(@string, out var @sbyte) == true)
                    return ValidateRoundTrip<string, sbyte>(@string, @sbyte, (v) => @string, verify);
            }
            else if (dataType == typeof(byte))
            {
                if (byte.TryParse(@string, out var @byte) == true)
                    return ValidateRoundTrip<string, byte>(@string, @byte, (v) => @string, verify);
            }
            return InvokeException(@string, dataType, verify);
        }

        private static object ConvertTo(bool @bool, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<bool, string>(@bool, @bool.ToString(), (v) => bool.Parse(v), verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<bool, double>(@bool, @bool ? 1 : 0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<bool, float>(@bool, @bool ? 1 : 0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<bool, long>(@bool, @bool ? 1 : 0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(ulong))
            {
                return ValidateRoundTrip<bool, ulong>(@bool, @bool ? (ulong)1 : (ulong)0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<bool, int>(@bool, @bool ? 1 : 0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(uint))
            {
                return ValidateRoundTrip<bool, uint>(@bool, @bool ? (uint)1 : (uint)0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(short))
            {
                return ValidateRoundTrip<bool, short>(@bool, @bool ? (short)1 : (short)0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(ushort))
            {
                return ValidateRoundTrip<bool, ushort>(@bool, @bool ? (ushort)1 : (ushort)0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                return ValidateRoundTrip<bool, sbyte>(@bool, @bool ? (sbyte)1 : (sbyte)0, (v) => v == 1, verify);
            }
            else if (dataType == typeof(byte))
            {
                return ValidateRoundTrip<bool, byte>(@bool, @bool ? (byte)1 : (byte)0, (v) => v == 1, verify);
            }
            return InvokeException(@bool, dataType, verify);
        }

        private static object ConvertTo(Guid guid, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<Guid, string>(guid, guid.ToString(), (v) => Guid.Parse(v), verify);
            }
            if (verify == true)
                return DBNull.Value;
            throw new FormatException();
        }

        private static object ConvertTo(DateTime dateTime, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<DateTime, string>(dateTime, dateTime.ToString("o"), (v) => DateTime.Parse(v), verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<DateTime, double>(dateTime, dateTime.ToOADate(), (v) => DateTime.FromOADate(v), verify);
            }
            else if (dataType == typeof(float))
            {
                var value = dateTime.ToOADate();
                if (value >= float.MinValue && value <= float.MaxValue)
                    return ValidateRoundTrip<DateTime, float>(dateTime, (float)value, (v) => DateTime.FromOADate((double)v), verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<DateTime, long>(dateTime, dateTime.Ticks, (v) => new DateTime(v), verify);
            }
            else if (dataType == typeof(ulong))
            {
                if (dateTime.Ticks >= 0 && dateTime.Ticks <= long.MaxValue)
                    return ValidateRoundTrip<DateTime, ulong>(dateTime, (ulong)dateTime.Ticks, (v) => new DateTime((long)v), verify);
            }
            else if (dataType == typeof(int))
            {
                if (dateTime.Ticks >= int.MinValue && dateTime.Ticks <= int.MaxValue)
                    return ValidateRoundTrip<DateTime, int>(dateTime, (int)dateTime.Ticks, (v) => new DateTime((long)v), verify);
            }
            else if (dataType == typeof(uint))
            {
                if (dateTime.Ticks >= uint.MinValue && dateTime.Ticks <= uint.MaxValue)
                    return ValidateRoundTrip<DateTime, uint>(dateTime, (uint)dateTime.Ticks, (v) => new DateTime((long)v), verify);
            }
            else if (dataType == typeof(short))
            {
                if (dateTime.Ticks >= short.MinValue && dateTime.Ticks <= short.MaxValue)
                    return ValidateRoundTrip<DateTime, short>(dateTime, (short)dateTime.Ticks, (v) => new DateTime((long)v), verify);
            }
            else if (dataType == typeof(ushort))
            {
                if (dateTime.Ticks >= ushort.MinValue && dateTime.Ticks <= ushort.MaxValue)
                    return ValidateRoundTrip<DateTime, ushort>(dateTime, (ushort)dateTime.Ticks, (v) => new DateTime((long)v), verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (dateTime.Ticks >= sbyte.MinValue && dateTime.Ticks <= sbyte.MaxValue)
                    return ValidateRoundTrip<DateTime, sbyte>(dateTime, (sbyte)dateTime.Ticks, (v) => new DateTime((long)v), verify);
            }
            else if (dataType == typeof(byte))
            {
                if (dateTime.Ticks >= byte.MinValue && dateTime.Ticks <= byte.MaxValue)
                    return ValidateRoundTrip<DateTime, byte>(dateTime, (byte)dateTime.Ticks, (v) => new DateTime((long)v), verify);
            }
            return InvokeException(dateTime, dataType, verify);
        }

        private static object ConvertTo(TimeSpan timeSpan, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<TimeSpan, string>(timeSpan, timeSpan.ToString(), (v) => TimeSpan.Parse(v), verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<TimeSpan, double>(timeSpan, timeSpan.TotalMilliseconds, (v) => TimeSpan.FromMilliseconds(v), verify);
            }
            else if (dataType == typeof(float))
            {
                if (timeSpan.TotalMilliseconds >= float.MinValue && timeSpan.TotalMilliseconds <= float.MaxValue)
                    return ValidateRoundTrip<TimeSpan, float>(timeSpan, (float)timeSpan.TotalMilliseconds, (v) => TimeSpan.FromMilliseconds((double)v), verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<TimeSpan, long>(timeSpan, timeSpan.Ticks, (v) => new TimeSpan(v), verify);
            }
            else if (dataType == typeof(ulong))
            {
                if (timeSpan.Ticks >= 0 && timeSpan.Ticks <= long.MaxValue)
                    return ValidateRoundTrip<TimeSpan, ulong>(timeSpan, (ulong)timeSpan.Ticks, (v) => new TimeSpan((long)v), verify);
            }
            else if (dataType == typeof(int))
            {
                if (timeSpan.Ticks >= int.MinValue && timeSpan.Ticks <= int.MaxValue)
                    return ValidateRoundTrip<TimeSpan, int>(timeSpan, (int)timeSpan.Ticks, (v) => new TimeSpan(v), verify);
            }
            else if (dataType == typeof(uint))
            {
                if (timeSpan.Ticks >= uint.MinValue && timeSpan.Ticks <= uint.MaxValue)
                    return ValidateRoundTrip<TimeSpan, uint>(timeSpan, (uint)timeSpan.Ticks, (v) => new TimeSpan(v), verify);
            }
            else if (dataType == typeof(short))
            {
                if (timeSpan.Ticks >= short.MinValue && timeSpan.Ticks <= short.MaxValue)
                    return ValidateRoundTrip<TimeSpan, short>(timeSpan, (short)timeSpan.Ticks, (v) => new TimeSpan(v), verify);
            }
            else if (dataType == typeof(ushort))
            {
                if (timeSpan.Ticks >= ushort.MinValue && timeSpan.Ticks <= ushort.MaxValue)
                    return ValidateRoundTrip<TimeSpan, ushort>(timeSpan, (ushort)timeSpan.Ticks, (v) => new TimeSpan(v), verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (timeSpan.Ticks >= sbyte.MinValue && timeSpan.Ticks <= sbyte.MaxValue)
                    return ValidateRoundTrip<TimeSpan, sbyte>(timeSpan, (sbyte)timeSpan.Ticks, (v) => new TimeSpan(v), verify);
            }
            else if (dataType == typeof(byte))
            {
                if (timeSpan.Ticks >= byte.MinValue && timeSpan.Ticks <= byte.MaxValue)
                    return ValidateRoundTrip<TimeSpan, byte>(timeSpan, (byte)timeSpan.Ticks, (v) => new TimeSpan(v), verify);
            }
            return InvokeException(timeSpan, dataType, verify);
        }

        private static object ConvertTo(float @float, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<float, string>(@float, @float.ToString("R"), (v) => float.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@float == 0)
                    return ValidateRoundTrip<float, bool>(@float, false, (v) => @float, verify);
                else if (@float == 1)
                    return ValidateRoundTrip<float, bool>(@float, true, (v) => @float, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@float >= DateTime.MinValue.ToOADate() && @float <= DateTime.MaxValue.ToOADate())
                    return ValidateRoundTrip<float, DateTime>(@float, DateTime.FromOADate((double)@float), (v) => (float)v.ToOADate(), verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                if (@float >= TimeSpan.MinValue.TotalMilliseconds && @float <= TimeSpan.MaxValue.TotalMilliseconds)
                    return ValidateRoundTrip<float, TimeSpan>(@float, TimeSpan.FromMilliseconds(@float), (v) => (float)v.TotalMilliseconds, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<float, double>(@float, (double)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<float, long>(@float, (long)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                return ValidateRoundTrip<float, ulong>(@float, (ulong)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<float, int>(@float, (int)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                return ValidateRoundTrip<float, uint>(@float, (uint)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(short))
            {
                return ValidateRoundTrip<float, short>(@float, (short)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                return ValidateRoundTrip<float, ushort>(@float, (ushort)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                return ValidateRoundTrip<float, sbyte>(@float, (sbyte)@float, (v) => (float)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                return ValidateRoundTrip<float, byte>(@float, (byte)@float, (v) => (float)v, verify);
            }
            return InvokeException(@float, dataType, verify);
        }

        private static object ConvertTo(double @double, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<double, string>(@double, @double.ToString("R"), (v) => double.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@double == 0)
                    return ValidateRoundTrip<double, bool>(@double, false, (v) => @double, verify);
                else if (@double == 1)
                    return ValidateRoundTrip<double, bool>(@double, true, (v) => @double, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@double >= DateTime.MinValue.ToOADate() && @double <= DateTime.MaxValue.ToOADate())
                    return ValidateRoundTrip<double, DateTime>(@double, DateTime.FromOADate((double)@double), (v) => v.ToOADate(), verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                if (@double >= TimeSpan.MinValue.TotalMilliseconds && @double <= TimeSpan.MaxValue.TotalMilliseconds)
                    return ValidateRoundTrip<double, TimeSpan>(@double, TimeSpan.FromMilliseconds(@double), (v) => v.TotalMilliseconds, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<double, float>(@double, (float)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<double, long>(@double, (long)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                return ValidateRoundTrip<double, ulong>(@double, (ulong)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<double, int>(@double, (int)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                return ValidateRoundTrip<double, uint>(@double, (uint)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(short))
            {
                return ValidateRoundTrip<double, short>(@double, (short)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                return ValidateRoundTrip<double, ushort>(@double, (ushort)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                return ValidateRoundTrip<double, sbyte>(@double, (sbyte)@double, (v) => (double)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                return ValidateRoundTrip<double, byte>(@double, (byte)@double, (v) => (double)v, verify);
            }
            return InvokeException(@double, dataType, verify);
        }

        private static object ConvertTo(long @long, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<long, string>(@long, @long.ToString(), (v) => long.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@long == 0)
                    return ValidateRoundTrip<long, bool>(@long, false, (v) => @long, verify);
                else if (@long == 1)
                    return ValidateRoundTrip<long, bool>(@long, true, (v) => @long, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@long >= DateTime.MinValue.Ticks && @long <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<long, DateTime>(@long, new DateTime(@long), (v) => v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                return ValidateRoundTrip<long, TimeSpan>(@long, new TimeSpan(@long), (v) => v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<long, float>(@long, (float)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<long, double>(@long, (double)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                return ValidateRoundTrip<long, ulong>(@long, (ulong)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<long, int>(@long, (int)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                return ValidateRoundTrip<long, uint>(@long, (uint)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(short))
            {
                return ValidateRoundTrip<long, short>(@long, (short)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                return ValidateRoundTrip<long, ushort>(@long, (ushort)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                return ValidateRoundTrip<long, sbyte>(@long, (sbyte)@long, (v) => (long)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                return ValidateRoundTrip<long, byte>(@long, (byte)@long, (v) => (long)v, verify);
            }
            return InvokeException(@long, dataType, verify);
        }

        private static object ConvertTo(ulong @ulong, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<ulong, string>(@ulong, @ulong.ToString(), (v) => ulong.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@ulong == 0)
                    return ValidateRoundTrip<ulong, bool>(@ulong, false, (v) => @ulong, verify);
                else if (@ulong == 1)
                    return ValidateRoundTrip<ulong, bool>(@ulong, true, (v) => @ulong, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@ulong <= long.MaxValue && (long)@ulong <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<ulong, DateTime>(@ulong, new DateTime((long)@ulong), (v) => (ulong)v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                if (@ulong <= long.MaxValue)
                    return ValidateRoundTrip<ulong, TimeSpan>(@ulong, new TimeSpan((long)@ulong), (v) => (ulong)v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<ulong, float>(@ulong, (float)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<ulong, double>(@ulong, (double)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(long))
            {
                if (@ulong <= long.MaxValue)
                    return ValidateRoundTrip<ulong, long>(@ulong, (long)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(int))
            {
                if (@ulong <= int.MaxValue)
                    return ValidateRoundTrip<ulong, int>(@ulong, (int)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                if (@ulong <= uint.MaxValue)
                    return ValidateRoundTrip<ulong, uint>(@ulong, (uint)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(short))
            {
                if (@ulong <= int.MaxValue && (int)@ulong <= short.MaxValue)
                    return ValidateRoundTrip<ulong, short>(@ulong, (short)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                if (@ulong <= ushort.MaxValue)
                    return ValidateRoundTrip<ulong, ushort>(@ulong, (ushort)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (@ulong <= int.MaxValue && (int)@ulong <= sbyte.MaxValue)
                    return ValidateRoundTrip<ulong, sbyte>(@ulong, (sbyte)@ulong, (v) => (ulong)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                if (@ulong <= byte.MaxValue)
                    return ValidateRoundTrip<ulong, byte>(@ulong, (byte)@ulong, (v) => (ulong)v, verify);
            }
            return InvokeException(@ulong, dataType, verify);
        }

        private static object ConvertTo(int @int, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<int, string>(@int, @int.ToString(), (v) => int.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@int == 0)
                    return ValidateRoundTrip<int, bool>(@int, false, (v) => @int, verify);
                else if (@int == 1)
                    return ValidateRoundTrip<int, bool>(@int, true, (v) => @int, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@int >= DateTime.MinValue.Ticks && @int <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<int, DateTime>(@int, new DateTime((long)@int), (v) => (int)v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                return ValidateRoundTrip<int, TimeSpan>(@int, new TimeSpan((long)@int), (v) => (int)v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<int, float>(@int, (float)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<int, double>(@int, (double)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<int, long>(@int, (long)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                if (@int >= 0)
                    return ValidateRoundTrip<int, ulong>(@int, (ulong)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                if (@int >= uint.MinValue && @int <= int.MaxValue)
                    return ValidateRoundTrip<int, uint>(@int, (uint)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(short))
            {
                return ValidateRoundTrip<int, short>(@int, (short)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                return ValidateRoundTrip<int, ushort>(@int, (ushort)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                return ValidateRoundTrip<int, sbyte>(@int, (sbyte)@int, (v) => (int)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                return ValidateRoundTrip<int, byte>(@int, (byte)@int, (v) => (int)v, verify);
            }
            return InvokeException(@int, dataType, verify);
        }

        private static object ConvertTo(uint @uint, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<uint, string>(@uint, @uint.ToString(), (v) => uint.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@uint == 0)
                    return ValidateRoundTrip<uint, bool>(@uint, false, (v) => @uint, verify);
                else if (@uint == 1)
                    return ValidateRoundTrip<uint, bool>(@uint, true, (v) => @uint, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@uint >= DateTime.MinValue.Ticks && @uint <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<uint, DateTime>(@uint, new DateTime((long)@uint), (v) => (uint)v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                return ValidateRoundTrip<uint, TimeSpan>(@uint, new TimeSpan((long)@uint), (v) => (uint)v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<uint, float>(@uint, (float)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<uint, double>(@uint, (double)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<uint, long>(@uint, (long)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                return ValidateRoundTrip<uint, ulong>(@uint, (ulong)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(int))
            {
                if (@uint <= int.MaxValue)
                    return ValidateRoundTrip<uint, int>(@uint, (int)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(short))
            {
                if (@uint <= short.MaxValue)
                    return ValidateRoundTrip<uint, short>(@uint, (short)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                if (@uint <= ushort.MaxValue)
                    return ValidateRoundTrip<uint, ushort>(@uint, (ushort)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (@uint <= sbyte.MaxValue)
                    return ValidateRoundTrip<uint, sbyte>(@uint, (sbyte)@uint, (v) => (uint)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                if (@uint <= byte.MaxValue)
                    return ValidateRoundTrip<uint, byte>(@uint, (byte)@uint, (v) => (uint)v, verify);
            }
            return InvokeException(@uint, dataType, verify);
        }

        private static object ConvertTo(short @short, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<short, string>(@short, @short.ToString(), (v) => short.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@short == 0)
                    return ValidateRoundTrip<short, bool>(@short, false, (v) => @short, verify);
                else if (@short == 1)
                    return ValidateRoundTrip<short, bool>(@short, true, (v) => @short, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@short >= DateTime.MinValue.Ticks && @short <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<short, DateTime>(@short, new DateTime((long)@short), (v) => (short)v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                return ValidateRoundTrip<short, TimeSpan>(@short, new TimeSpan((long)@short), (v) => (short)v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<short, float>(@short, (float)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<short, double>(@short, (double)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<short, long>(@short, (long)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                if (@short >= 0)
                    return ValidateRoundTrip<short, ulong>(@short, (ulong)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<short, int>(@short, (int)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                if (@short >= 0)
                    return ValidateRoundTrip<short, uint>(@short, (uint)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                if (@short >= 0 && @short <= short.MaxValue)
                    return ValidateRoundTrip<short, ushort>(@short, (ushort)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (@short >= sbyte.MinValue && @short <= sbyte.MaxValue)
                    return ValidateRoundTrip<short, sbyte>(@short, (sbyte)@short, (v) => (short)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                if (@short >= 0 && @short <= byte.MaxValue)
                    return ValidateRoundTrip<short, byte>(@short, (byte)@short, (v) => (short)v, verify);
            }
            return InvokeException(@short, dataType, verify);
        }

        private static object ConvertTo(ushort @ushort, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<ushort, string>(@ushort, @ushort.ToString(), (v) => ushort.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@ushort == 0)
                    return ValidateRoundTrip<ushort, bool>(@ushort, false, (v) => @ushort, verify);
                else if (@ushort == 1)
                    return ValidateRoundTrip<ushort, bool>(@ushort, true, (v) => @ushort, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@ushort >= DateTime.MinValue.Ticks && @ushort <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<ushort, DateTime>(@ushort, new DateTime((long)@ushort), (v) => (ushort)v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                return ValidateRoundTrip<ushort, TimeSpan>(@ushort, new TimeSpan((long)@ushort), (v) => (ushort)v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<ushort, float>(@ushort, (float)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<ushort, double>(@ushort, (double)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<ushort, long>(@ushort, (long)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                return ValidateRoundTrip<ushort, ulong>(@ushort, (ulong)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<ushort, int>(@ushort, (int)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                return ValidateRoundTrip<ushort, uint>(@ushort, (uint)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(short))
            {
                if (@ushort <= short.MaxValue)
                    return ValidateRoundTrip<ushort, short>(@ushort, (short)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (@ushort <= sbyte.MaxValue)
                    return ValidateRoundTrip<ushort, sbyte>(@ushort, (sbyte)@ushort, (v) => (ushort)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                if (@ushort <= byte.MaxValue)
                    return ValidateRoundTrip<ushort, byte>(@ushort, (byte)@ushort, (v) => (ushort)v, verify);
            }
            return InvokeException(@ushort, dataType, verify);
        }

        private static object ConvertTo(sbyte @sbyte, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<sbyte, string>(@sbyte, @sbyte.ToString(), (v) => sbyte.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@sbyte == 0)
                    return ValidateRoundTrip<sbyte, bool>(@sbyte, false, (v) => @sbyte, verify);
                else if (@sbyte == 1)
                    return ValidateRoundTrip<sbyte, bool>(@sbyte, true, (v) => @sbyte, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@sbyte >= DateTime.MinValue.Ticks && @sbyte <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<sbyte, DateTime>(@sbyte, new DateTime((long)@sbyte), (v) => (sbyte)v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                return ValidateRoundTrip<sbyte, TimeSpan>(@sbyte, new TimeSpan((long)@sbyte), (v) => (sbyte)v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<sbyte, float>(@sbyte, (float)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<sbyte, double>(@sbyte, (double)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<sbyte, long>(@sbyte, (long)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                if (@sbyte >= 0)
                    return ValidateRoundTrip<sbyte, ulong>(@sbyte, (ulong)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<sbyte, int>(@sbyte, (int)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                if (@sbyte >= 0)
                    return ValidateRoundTrip<sbyte, uint>(@sbyte, (uint)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(short))
            {
                return ValidateRoundTrip<sbyte, short>(@sbyte, (short)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                if (@sbyte >= 0)
                    return ValidateRoundTrip<sbyte, ushort>(@sbyte, (ushort)@sbyte, (v) => (sbyte)v, verify);
            }
            else if (dataType == typeof(byte))
            {
                if (@sbyte >= 0)
                    return ValidateRoundTrip<sbyte, byte>(@sbyte, (byte)@sbyte, (v) => (sbyte)v, verify);
            }
            return InvokeException(@sbyte, dataType, verify);
        }

        private static object ConvertTo(byte @byte, Type dataType, bool verify)
        {
            if (dataType == typeof(string))
            {
                return ValidateRoundTrip<byte, string>(@byte, @byte.ToString(), (v) => byte.Parse(v), verify);
            }
            else if (dataType == typeof(bool))
            {
                if (@byte == 0)
                    return ValidateRoundTrip<byte, bool>(@byte, false, (v) => @byte, verify);
                else if (@byte == 1)
                    return ValidateRoundTrip<byte, bool>(@byte, true, (v) => @byte, verify);
            }
            else if (dataType == typeof(DateTime))
            {
                if (@byte >= DateTime.MinValue.Ticks && @byte <= DateTime.MaxValue.Ticks)
                    return ValidateRoundTrip<byte, DateTime>(@byte, new DateTime((long)@byte), (v) => (byte)v.Ticks, verify);
            }
            else if (dataType == typeof(TimeSpan))
            {
                return ValidateRoundTrip<byte, TimeSpan>(@byte, new TimeSpan((long)@byte), (v) => (byte)v.Ticks, verify);
            }
            else if (dataType == typeof(float))
            {
                return ValidateRoundTrip<byte, float>(@byte, (float)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(double))
            {
                return ValidateRoundTrip<byte, double>(@byte, (double)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(long))
            {
                return ValidateRoundTrip<byte, long>(@byte, (long)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(ulong))
            {
                return ValidateRoundTrip<byte, ulong>(@byte, (ulong)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(int))
            {
                return ValidateRoundTrip<byte, int>(@byte, (int)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(uint))
            {
                return ValidateRoundTrip<byte, uint>(@byte, (uint)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(short))
            {
                return ValidateRoundTrip<byte, short>(@byte, (short)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(ushort))
            {
                return ValidateRoundTrip<byte, ushort>(@byte, (ushort)@byte, (v) => (byte)v, verify);
            }
            else if (dataType == typeof(sbyte))
            {
                if (@byte <= sbyte.MaxValue)
                    return ValidateRoundTrip<byte, sbyte>(@byte, (sbyte)@byte, (v) => (byte)v, verify);
            }
            return InvokeException(@byte, dataType, verify);
        }

        private static object ValidateRoundTrip<T, U>(T var1, U var2, Func<U, T> func, bool verify)
        {
            var v = func(var2);
            if (object.Equals(var1, v) == true)
                return var2;
            else if (verify == true)
                return DBNull.Value;
            throw new FormatException();
        }

        private static object ChangeType(object value, Type dataType, bool verify)
        {
            ValidateChangeType(value, dataType);

            if (value.GetType() == dataType)
                return value;

            if (value is string @string)
            {
                return ConvertTo(@string, dataType, verify);
            }
            else if (value is bool @bool)
            {
                return ConvertTo(@bool, dataType, verify);
            }
            else if (value is Guid guid)
            {
                return ConvertTo(guid, dataType, verify);
            }
            else if (value is DateTime dateTime)
            {
                return ConvertTo(dateTime, dataType, verify);
            }
            else if (value is TimeSpan timeSpan)
            {
                return ConvertTo(timeSpan, dataType, verify);
            }
            else if (value is float @float)
            {
                return ConvertTo(@float, dataType, verify);
            }
            else if (value is double @double)
            {
                return ConvertTo(@double, dataType, verify);
            }
            else if (value is long @long)
            {
                return ConvertTo(@long, dataType, verify);
            }
            else if (value is ulong @ulong)
            {
                return ConvertTo(@ulong, dataType, verify);
            }
            else if (value is int @int)
            {
                return ConvertTo(@int, dataType, verify);
            }
            else if (value is uint @uint)
            {
                return ConvertTo(@uint, dataType, verify);
            }
            else if (value is short @short)
            {
                return ConvertTo(@short, dataType, verify);
            }
            else if (value is ushort @ushort)
            {
                return ConvertTo(@ushort, dataType, verify);
            }
            else if (value is sbyte @sbyte)
            {
                return ConvertTo(@sbyte, dataType, verify);
            }
            else if (value is byte @byte)
            {
                return ConvertTo(@byte, dataType, verify);
            }
            else
            {
                throw new FormatException();
            }
        }

        private static object InvokeException(object value, Type dataType, bool verify)
        {
            if (verify == true)
                return DBNull.Value;
            throw new FormatException($"'{value}' cannot be converted to type '{dataType}'.");
        }
    }
}

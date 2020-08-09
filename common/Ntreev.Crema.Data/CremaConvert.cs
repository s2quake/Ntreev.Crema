//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Crema.Data.Properties;
using System;

namespace Ntreev.Crema.Data
{
    public static class CremaConvert
    {
        public static object ChangeType(object value, Type dataType)
        {
            ValidateChangeType(value, dataType);

            if (value.GetType() == dataType)
                return value;

            string textValue;
            if (value is DateTime time)
            {
                if (dataType == typeof(string))
                    return time.ToString();
                textValue = time.ToOADate().ToString();
            }
            else if (value is TimeSpan span)
            {
                if (dataType == typeof(string))
                    return span.ToString();
                textValue = span.TotalSeconds.ToString();
            }
            else if (value is float single)
            {
                textValue = single.ToString("R");
            }
            else if (value is double @double)
            {
                textValue = @double.ToString("R");
            }
            else
            {
                textValue = value.ToString();
            }

            try
            {
                if (dataType == typeof(bool))
                {
                    return bool.Parse(textValue);
                }
                else if (dataType == typeof(float))
                {
                    return float.Parse(textValue);
                }
                else if (dataType == typeof(double))
                {
                    return double.Parse(textValue);
                }
                else if (dataType == typeof(sbyte))
                {
                    return sbyte.Parse(textValue);
                }
                else if (dataType == typeof(byte))
                {
                    return byte.Parse(textValue);
                }
                else if (dataType == typeof(short))
                {
                    return short.Parse(textValue);
                }
                else if (dataType == typeof(ushort))
                {
                    return ushort.Parse(textValue);
                }
                else if (dataType == typeof(int))
                {
                    return int.Parse(textValue);
                }
                else if (dataType == typeof(uint))
                {
                    return uint.Parse(textValue);
                }
                else if (dataType == typeof(long))
                {
                    if (long.TryParse(textValue, out var l) == true)
                    {
                        return l;
                    }
                    else if (double.TryParse(textValue, out var d) == true)
                    {
                        return (long)d;
                    }
                    else
                    {
                        return long.Parse(textValue);
                    }
                }
                else if (dataType == typeof(ulong))
                {
                    if (ulong.TryParse(textValue, out var u) == true)
                    {
                        return u;
                    }
                    else if (double.TryParse(textValue, out var d) == true)
                    {
                        return (ulong)d;
                    }
                    else
                    {
                        return ulong.Parse(textValue);
                    }
                }
                else if (dataType == typeof(DateTime))
                {
                    if (double.TryParse(textValue, out double d) == true)
                        return DateTime.FromOADate(d);
                    return DateTime.Parse(textValue);
                }
                else if (dataType == typeof(TimeSpan))
                {
                    if (double.TryParse(textValue, out double d) == true)
                    {
                        if (d == TimeSpan.MinValue.TotalSeconds)
                            return TimeSpan.MinValue;
                        if (d == TimeSpan.MaxValue.TotalSeconds)
                            return TimeSpan.MaxValue;
                        return TimeSpan.FromSeconds(d);
                    }
                    return TimeSpan.Parse(textValue);
                }
                else if (dataType == typeof(Guid))
                {
                    return Guid.Parse(textValue);
                }

                return textValue;
            }
            catch (Exception e)
            {
                throw new FormatException(string.Format(Resources.Exception_CannotConvert_Format, value, dataType.GetTypeName()), e);
            }
        }

        public static bool VerifyChangeType(object value, Type dataType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));
            if (CremaDataTypeUtility.IsBaseType(dataType) == false)
                throw new ArgumentException(string.Format(Resources.Exception_TypeCannotBeUsed_Format, dataType.Name), nameof(dataType));

            if (value.GetType() == dataType)
                return true;

            string textValue;
            if (value is DateTime time)
                textValue = time.ToOADate().ToString();
            else if (value is TimeSpan span)
                textValue = span.Ticks.ToString();
            else if (value is float single)
                textValue = single.ToString("R");
            else if (value is double @double)
                textValue = @double.ToString("R");
            else
                textValue = value.ToString();

            if (dataType == typeof(bool))
            {
                return bool.TryParse(textValue, out _);
            }
            else if (dataType == typeof(float))
            {
                return float.TryParse(textValue, out _);
            }
            else if (dataType == typeof(double))
            {
                return double.TryParse(textValue, out _);
            }
            else if (dataType == typeof(sbyte))
            {
                return sbyte.TryParse(textValue, out _);
            }
            else if (dataType == typeof(byte))
            {
                return byte.TryParse(textValue, out _);
            }
            else if (dataType == typeof(short))
            {
                return short.TryParse(textValue, out _);
            }
            else if (dataType == typeof(ushort))
            {
                return ushort.TryParse(textValue, out _);
            }
            else if (dataType == typeof(int))
            {
                return int.TryParse(textValue, out _);
            }
            else if (dataType == typeof(uint))
            {
                return uint.TryParse(textValue, out _);
            }
            else if (dataType == typeof(long))
            {
                return long.TryParse(textValue, out _);
            }
            else if (dataType == typeof(ulong))
            {
                return ulong.TryParse(textValue, out _);
            }
            else if (dataType == typeof(DateTime))
            {
                try
                {
                    if (double.TryParse(textValue, out double l) == true)
                    {
                        DateTime.FromOADate(l);
                        return true;
                    }
                    return DateTime.TryParse(textValue, out DateTime s);
                }
                catch
                {
                    return false;
                }
            }
            else if (dataType == typeof(TimeSpan))
            {
                try
                {
                    if (double.TryParse(textValue, out double l) == true)
                    {
                        if (l == TimeSpan.MinValue.TotalSeconds || l == TimeSpan.MaxValue.TotalSeconds)
                            return true;
                        TimeSpan.FromSeconds(l);
                        return true;
                    }
                    return TimeSpan.TryParse(textValue, out TimeSpan s);
                }
                catch
                {
                    return false;
                }
            }
            else if (dataType == typeof(Guid))
            {
                return Guid.TryParse(textValue, out _);
            }

            return true;
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
    }
}

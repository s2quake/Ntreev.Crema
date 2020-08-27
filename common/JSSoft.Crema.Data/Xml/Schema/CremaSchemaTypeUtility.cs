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

using System;

namespace Ntreev.Crema.Data.Xml.Schema
{
    public static class CremaSchemaTypeUtility
    {
        public static string GetSchemaTypeName(this Type type)
        {
            return (type.GetTypeName()) switch
            {
                CremaDataTypeUtility.booleanType => "boolean",
                CremaDataTypeUtility.stringType => "string",
                CremaDataTypeUtility.floatType => "float",
                CremaDataTypeUtility.doubleType => "double",
                CremaDataTypeUtility.int8Type => "byte",
                CremaDataTypeUtility.uint8Type => "unsignedByte",
                CremaDataTypeUtility.int16Type => "short",
                CremaDataTypeUtility.uint16Type => "unsignedShort",
                CremaDataTypeUtility.int32Type => "int",
                CremaDataTypeUtility.uint32Type => "unsignedInt",
                CremaDataTypeUtility.int64Type => "long",
                CremaDataTypeUtility.uint64Type => "unsignedLong",
                CremaDataTypeUtility.datetimeType => "dateTime",
                CremaDataTypeUtility.durationType => "duration",
                CremaDataTypeUtility.guidType => "guid",
                _ => throw new NotImplementedException(),
            };
        }

        public static Type GetType(string typeName)
        {
            return typeName switch
            {
                "boolean" => typeof(bool),
                "string" => typeof(string),
                "float" => typeof(float),
                "double" => typeof(double),
                "byte" => typeof(sbyte),
                "unsignedByte" => typeof(byte),
                "short" => typeof(short),
                "unsignedShort" => typeof(ushort),
                "int" => typeof(int),
                "unsignedInt" => typeof(uint),
                "long" => typeof(long),
                "unsignedLong" => typeof(ulong),
                "dateTime" => typeof(DateTime),
                "duration" => typeof(TimeSpan),
                "guid" => typeof(Guid),
                _ => throw new NotImplementedException(),
            };
        }
    }
}

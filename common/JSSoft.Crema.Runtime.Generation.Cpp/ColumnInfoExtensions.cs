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

using JSSoft.Crema.Data;

/* 'JSSoft.Crema.Runtime.Generation.Cpp (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
using System;
이후:
using JSSoft.Library.ObjectModel;
using System;
*/
using JSSoft.Library.ObjectModel;
using System;

/* 'JSSoft.Crema.Runtime.Generation.Cpp (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
using System.Threading.Tasks;
using JSSoft.Library.ObjectModel;
이후:
using System.Threading.Tasks;
*/
using System.CodeDom;

namespace JSSoft.Crema.Runtime.Generation.Cpp
{
    public static class ColumnInfoExtensions
    {
        public static string TypeNamespace { get; set; }

        public static string GetFieldName(this ColumnInfo columnInfo)
        {
            return columnInfo.Name;
        }

        public static string GetPropertyName(this ColumnInfo columnInfo)
        {
            return columnInfo.Name;
        }

        public static CodeTypeReference GetPropertyType(this ColumnInfo columnInfo)
        {
            CodeTypeReference codeTypeRef = null;
            if (CremaDataTypeUtility.IsBaseType(columnInfo.DataType) == true)
            {
                var runtimeType = CremaDataTypeUtility.GetType(columnInfo.DataType);
                if (runtimeType == typeof(Guid))
                    runtimeType = typeof(string);
                codeTypeRef = new CodeTypeReference(runtimeType);
                if (runtimeType == typeof(string))
                {
                    codeTypeRef.SetCodeType(CodeType.Reference | CodeType.Const);
                }
            }
            else
            {
                var itemName = new ItemName(columnInfo.DataType);
                codeTypeRef = Utility.GenerateCodeType(TypeNamespace, itemName.Name);
            }
            return codeTypeRef;
        }

        public static CodeTypeReference GetCodeType(this ColumnInfo columnInfo, CodeType codeType)
        {
            if (CremaDataTypeUtility.IsBaseType(columnInfo.DataType) == true)
            {
                if (columnInfo.DataType == typeof(Guid).GetTypeName())
                    return new CodeTypeReference(typeof(string));
                return new CodeTypeReference(CremaDataTypeUtility.GetType(columnInfo.DataType));
            }
            var itemName = new ItemName(columnInfo.DataType);
            return Utility.GenerateCodeType(TypeNamespace, itemName.Name);
        }

        public static bool IsCustomType(this ColumnInfo columnInfo)
        {
            return CremaDataTypeUtility.IsBaseType(columnInfo.DataType) == false;
        }

        public static CodeExpression GetFieldExpression(this ColumnInfo columnInfo)
        {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), columnInfo.GetFieldName());
        }

        public static CodeExpression GetInitExpression(this ColumnInfo columnInfo)
        {
            if (columnInfo.DataType != typeof(string).GetTypeName() && columnInfo.DataType != typeof(Guid).GetTypeName())
                return new CodeCastExpression(columnInfo.GetCodeType(CodeType.None), new CodePrimitiveExpression(0));
            return null;
        }

        public static CodeExpression GetGetValueMethodExpression(this ColumnInfo columnInfo)
        {
            var row = new CodeVariableReferenceExpression("row");
            var getValue = new CodeMethodReferenceExpression(row, GetMethodName(columnInfo));
            var columnName = new CodePrimitiveExpression(columnInfo.Name);
            var getValueInvoke = new CodeMethodInvokeExpression(getValue, columnName);

            if (columnInfo.IsCustomType() == false)
                return getValueInvoke;

            return new CodeCastExpression(columnInfo.GetCodeType(CodeType.None), getValueInvoke);
        }

        public static CodeExpression GetGetValueMethodExpression(this ColumnInfo columnInfo, int index, bool isDevmode)
        {
            if (isDevmode == true)
            {
                return GetGetValueMethodExpression(columnInfo);
            }
            else
            {
                var row = new CodeVariableReferenceExpression("row");
                var getValue = new CodeMethodReferenceExpression(row, GetMethodName(columnInfo));
                var indexVar = new CodePrimitiveExpression(index);
                var getValueInvoke = new CodeMethodInvokeExpression(getValue, indexVar);

                if (columnInfo.IsCustomType() == false)
                    return getValueInvoke;

                return new CodeCastExpression(columnInfo.GetCodeType(CodeType.None), getValueInvoke);
            }
        }

        public static CodeExpression GetHasValueMethodExpression(this ColumnInfo columnInfo)
        {
            var rowVarRefExp = new CodeVariableReferenceExpression("row");
            var columnNameExp = new CodePrimitiveExpression(columnInfo.Name);
            return new CodeMethodInvokeExpression(rowVarRefExp, "has_value", columnNameExp);
        }

        public static CodeExpression GetHasValueMethodExpression(this ColumnInfo columnInfo, int index, bool isDevmode)
        {
            if (isDevmode == true)
            {
                return GetHasValueMethodExpression(columnInfo);
            }
            else
            {
                var rowVarRefExp = new CodeVariableReferenceExpression("row");
                var columnNameExp = new CodePrimitiveExpression(index);
                return new CodeMethodInvokeExpression(rowVarRefExp, "has_value", columnNameExp);
            }
        }

        public static string GetMethodName(this ColumnInfo columnInfo)
        {
            if (columnInfo.DataType == "boolean")
                return "to_boolean";
            else if (columnInfo.DataType == "string")
                return "to_string";
            else if (columnInfo.DataType == "float")
                return "to_single";
            else if (columnInfo.DataType == "double")
                return "to_double";
            else if (columnInfo.DataType == "int8")
                return "to_int8";
            else if (columnInfo.DataType == "uint8")
                return "to_uint8";
            else if (columnInfo.DataType == "int16")
                return "to_int16";
            else if (columnInfo.DataType == "uint16")
                return "to_uint16";
            else if (columnInfo.DataType == "int32")
                return "to_int32";
            else if (columnInfo.DataType == "uint32")
                return "to_uint32";
            else if (columnInfo.DataType == "int64")
                return "to_int64";
            else if (columnInfo.DataType == "uint64")
                return "to_uint64";
            else if (columnInfo.DataType == "datetime")
                return "to_datetime";
            else if (columnInfo.DataType == "duration")
                return "to_duration";
            else if (columnInfo.DataType == "guid")
                return "to_string";

            return "to_int32";
        }
    }
}

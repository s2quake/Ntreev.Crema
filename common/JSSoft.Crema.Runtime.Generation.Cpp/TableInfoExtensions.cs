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

using JSSoft.Crema.Data;
using JSSoft.Crema.Runtime.Generation.Cpp.CodeDom;
using JSSoft.Library;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JSSoft.Crema.Runtime.Generation.Cpp
{
    public static class TableInfoExtensions
    {
        private const string separator = "";
        private const string tablePrefix = "Table";
        private const string rowPostfix = "Row";

        public static string GetFieldName(this TableInfo tableInfo)
        {
            return tableInfo.TableName;
        }

        public static string GetVariableName(this TableInfo tableInfo)
        {
            return Regex.Replace(tableInfo.GetPropertyName(), "^[a-zA-Z]", i => i.Value.ToLower());
        }

        public static string GetPropertyName(this TableInfo tableInfo)
        {
            return tableInfo.TableName;
        }

        public static string GetClassName(this TableInfo tableInfo)
        {
            var tableName = tableInfo.TemplatedParent == string.Empty ? tableInfo.Name : tableInfo.TemplatedParent;
            var items = StringUtility.Split(tableName, '.');
            return tablePrefix + string.Join(separator, items);
        }

        public static string GetParentClassName(this TableInfo tableInfo)
        {
            if (tableInfo.ParentName == string.Empty)
                return string.Empty;
            var items = StringUtility.Split(tableInfo.ParentName, '.');
            return tablePrefix + string.Join(separator, items);
        }

        public static string GetRowClassName(this TableInfo tableInfo)
        {
            var tableName = tableInfo.TemplatedParent == string.Empty ? tableInfo.Name : tableInfo.TemplatedParent;
            var itemList = StringUtility.Split(tableName, '.').ToList();
            itemList.Add(rowPostfix);
            return rowPostfix + string.Join(separator, itemList);
        }

        public static string GetParentRowClassName(this TableInfo tableInfo)
        {
            if (tableInfo.ParentName == string.Empty)
                return string.Empty;
            var itemList = StringUtility.Split(tableInfo.ParentName, '.').ToList();
            itemList.Add(rowPostfix);
            return rowPostfix + string.Join(separator, itemList);
        }

        public static CodeTypeReference GetCodeType(this TableInfo tableInfo, CodeType codeType)
        {
            var codeTypeRef = new CodeTypeReference(tableInfo.GetClassName());
            codeTypeRef.SetCodeType(codeType);
            return codeTypeRef;
        }

        public static CodeTypeReference GetCodeType(this TableInfo tableInfo, string tableNamespace, CodeType codeType)
        {
            var codeTypeRef = new CodeTypeReference(tableNamespace + "::" + tableInfo.GetClassName());
            codeTypeRef.SetCodeType(codeType);
            return codeTypeRef;
        }

        public static CodeTypeReference GetRowCodeType(this TableInfo tableInfo, CodeType codeType)
        {
            var codeTypeRef = new CodeTypeReference(tableInfo.GetRowClassName());
            codeTypeRef.SetCodeType(codeType);
            return codeTypeRef;
        }

        public static CodeTypeReferenceExpression GetRowCodeTypeExpression(this TableInfo tableInfo, CodeType codeType)
        {
            return new CodeTypeReferenceExpression(tableInfo.GetRowCodeType(codeType));
        }

        public static CodeTypeReference GetParentRowCodeType(this TableInfo tableInfo, CodeType codeType)
        {
            var codeTypeRef = new CodeTypeReference(tableInfo.GetParentRowClassName());
            codeTypeRef.SetCodeType(codeType);
            return codeTypeRef;
        }

        public static CodeExpression GetFieldExpression(this TableInfo tableInfo)
        {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), tableInfo.GetFieldName());
        }

        public static CodeExpression GetFieldExpressionPtr(this TableInfo tableInfo)
        {
            return new CodeFieldPointerExpression(new CodeThisReferenceExpression(), tableInfo.GetFieldName());
        }

        public static CodeExpression GetFieldExpression(this TableInfo tableInfo, CodeVariableReferenceExpression varRefExp)
        {
            return new CodeFieldReferenceExpression(varRefExp, tableInfo.GetFieldName());
        }
    }
}

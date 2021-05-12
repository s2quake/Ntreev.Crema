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

using JSSoft.Crema.Runtime.Generation.TypeScript.Properties;
using System.CodeDom;
using System.
/* 'JSSoft.Crema.Runtime.Generation.TypeScript (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using JSSoft.Crema.Runtime.Generation.TypeScript.Properties;
using System.Reflection;
using System.IO;
이후:
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
*/
Reflection;

namespace JSSoft.Crema.Runtime.Generation.TypeScript
{
    static class CremaDataClassCreator
    {
        private readonly static CodeThisReferenceExpression thisRef = new();

        public static void Create(CodeNamespace codeNamespace, CodeGenerationInfo generationInfo)
        {
            codeNamespace.Comments.Add(new CodeCommentStatement($"------------------------------------------------------------------------------"));
            codeNamespace.Comments.Add(new CodeCommentStatement($"dataBase: {generationInfo.DataBaseName}"));
            codeNamespace.Comments.Add(new CodeCommentStatement($"revision: {generationInfo.Revision}"));
            codeNamespace.Comments.Add(new CodeCommentStatement($"requested revision: {generationInfo.RequestedRevision}"));
            codeNamespace.Comments.Add(new CodeCommentStatement($"hash value: {generationInfo.TablesHashValue}"));
            codeNamespace.Comments.Add(new CodeCommentStatement($"tags: {generationInfo.Tags}"));
            codeNamespace.Comments.Add(new CodeCommentStatement($"------------------------------------------------------------------------------"));

            foreach (var item in generationInfo.GetTables())
            {
                CremaTableClassCreator.Create(codeNamespace, item, generationInfo);
            }

            var classType = new CodeTypeDeclaration
            {
                Attributes = MemberAttributes.Public,
                Name = generationInfo.ClassName,
                IsClass = true
            };
            classType.TypeAttributes |= TypeAttributes.Sealed;
            classType.BaseTypes.Add("base", "CremaData");

            CreateNameField(classType, generationInfo);
            CreateRevisionField(classType, generationInfo);
            CreateTypesHashValueField(classType, generationInfo);
            CreateTablesHashValueField(classType, generationInfo);
            CreateTagsHashValueField(classType, generationInfo);
            CreateNameProperty(classType, generationInfo);
            CreateRevisionProperty(classType, generationInfo);
            CreateTypesHashValueProperty(classType, generationInfo);
            CreateTablesHashValueProperty(classType, generationInfo);
            CreateTagsProperty(classType, generationInfo);
            CreateTableFields(classType, generationInfo);
            CreateCreateFromDataSetMethod(classType, generationInfo);
            CreateCreateFromFileMethod(classType, generationInfo);
            CreateTableProperties(classType, generationInfo);
            CreateReadFromDataSetMethod(classType, generationInfo);
            CreateReadFromFileMethod(classType, generationInfo);

            codeNamespace.Types.Add(classType);
        }

        private static void CreateCreateFromDataSetMethod(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
                Name = "createFromDataSet"
            };
            cmm.Parameters.Add("reader", "IDataSet", "dataSet");
            cmm.Parameters.Add(typeof(bool), "verifyRevision");
            cmm.ReturnType = new CodeTypeReference(generationInfo.ClassName);

            // create instance
            {
                var create = new CodeObjectCreateExpression(cmm.ReturnType);
                cmm.Statements.AddVariableDeclaration(cmm.ReturnType.BaseType, "instance", create);
            }

            // invoke readFromDataSet
            {
                var instance = new CodeVariableReferenceExpression("instance");
                var dataSet = new CodeVariableReferenceExpression("dataSet");
                var verifyRevision = new CodeVariableReferenceExpression("verifyRevision");
                var readFromDataSet = new CodeMethodReferenceExpression(instance, "readFromDataSet");
                cmm.Statements.Add(new CodeMethodInvokeExpression(readFromDataSet, dataSet, verifyRevision));
            }

            // return instance;
            {
                var instance = new CodeVariableReferenceExpression("instance");
                cmm.Statements.AddMethodReturn(instance);
            }

            classType.Members.Add(cmm);
        }

        private static void CreateCreateFromFileMethod(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
                Name = "createFromFile"
            };
            cmm.Parameters.Add(typeof(string), "filename");
            cmm.Parameters.Add(typeof(bool), "verifyRevision");
            cmm.ReturnType = new CodeTypeReference(generationInfo.ClassName);

            // create instance
            {
                var create = new CodeObjectCreateExpression(cmm.ReturnType);
                cmm.Statements.AddVariableDeclaration(cmm.ReturnType.BaseType, "instance", create);
            }

            // invoke readFromFile
            {
                var instance = new CodeVariableReferenceExpression("instance");
                var filename = new CodeVariableReferenceExpression("filename");
                var verifyRevision = new CodeVariableReferenceExpression("verifyRevision");
                var readFromDataSet = new CodeMethodReferenceExpression(instance, "readFromFile");
                cmm.Statements.Add(new CodeMethodInvokeExpression(readFromDataSet, filename, verifyRevision));
            }

            // return instance;
            {
                var instance = new CodeVariableReferenceExpression("instance");
                cmm.Statements.AddMethodReturn(instance);
            }

            classType.Members.Add(cmm);
        }

        private static void CreateReadFromDataSetMethod(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public,
                Name = "readFromDataSet"
            };
            cmm.Parameters.Add("reader", "IDataSet", "dataSet");
            cmm.Parameters.Add(typeof(bool), "verifyRevision");

            var dataSet = new CodeVariableReferenceExpression("dataSet");

            {
                var ccs = CreateCompareDataBaseStatement(classType, generationInfo);
                cmm.Statements.Add(ccs);
            }

            {
                var ccs = CreateCompareTypesHashValueStatement(classType, generationInfo);
                cmm.Statements.Add(ccs);
            }

            {
                var ccs = CreateCompareTablesHashValueStatement(classType, generationInfo);
                cmm.Statements.Add(ccs);
            }

            {
                var ccs = CreateCompareRevisionStatement(classType, generationInfo);
                cmm.Statements.Add(ccs);
            }

            {
                var field = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_name");
                var property = new CodePropertyReferenceExpression(dataSet, "name");
                cmm.Statements.AddAssign(field, property);
            }

            {
                var field = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_revision");
                var property = new CodePropertyReferenceExpression(dataSet, "revision");
                cmm.Statements.AddAssign(field, property);
            }

            {
                var field = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_typesHashValue");
                var property = new CodePropertyReferenceExpression(dataSet, "typesHashValue");
                cmm.Statements.AddAssign(field, property);
            }

            {
                var field = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_tablesHashValue");
                var property = new CodePropertyReferenceExpression(dataSet, "tablesHashValue");
                cmm.Statements.AddAssign(field, property);
            }

            {
                var field = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_tags");
                var property = new CodePropertyReferenceExpression(dataSet, "tags");
                cmm.Statements.AddAssign(field, property);
            }

            foreach (var item in generationInfo.GetTables(true))
            {
                var tableName = new CodePrimitiveExpression(item.Name);
                var tablesProp = new CodePropertyReferenceExpression(dataSet, "tables");
                var table = new CodePropertyReferenceExpression(tablesProp, item.Name);

                var field = item.GetFieldExpression();
                var createFromTable = new CodeMethodReferenceExpression(item.GetCodeTypeExpression(), "createFromTable");
                var invoke = new CodeMethodInvokeExpression(createFromTable, table);
                var assignStatement = new CodeAssignStatement(field, invoke);

                cmm.Statements.Add(assignStatement);
            }

            classType.Members.Add(cmm);
        }

        private static void CreateReadFromFileMethod(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public,
                Name = "readFromFile"
            };
            cmm.Parameters.Add(typeof(string), "filename");
            cmm.Parameters.Add(typeof(bool), "verifyRevision");

            var filename = new CodeVariableReferenceExpression("filename");
            var verifyRevision = new CodeVariableReferenceExpression("verifyRevision");
            var cremaReader = new CodeTypeReferenceExpression(Utility.GenerateCodeType("reader", "CremaReader"));
            var readFromFile = new CodeMethodInvokeExpression(cremaReader, "readFromFile", filename);
            var readFromDataSet = new CodeMethodInvokeExpression(thisRef, "readFromDataSet", readFromFile, verifyRevision);

            cmm.Statements.Add(readFromDataSet);

            classType.Members.Add(cmm);
        }

        private static void CreateTableFields(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            foreach (var item in generationInfo.GetTables(true))
            {
                var cmf = new CodeMemberField
                {
                    Attributes = MemberAttributes.Private,
                    Name = item.GetFieldName(),
                    Type = item.GetCodeType()
                };

                classType.Members.Add(cmf);
            }
        }

        private static void CreateTableProperties(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            foreach (var item in generationInfo.GetTables(true))
            {
                var cmp = new CodeMemberProperty
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Final,
                    Name = item.Name,
                    Type = item.GetCodeType(),
                    HasGet = true,
                    HasSet = false
                };
                cmp.Comments.AddSummary(item.Comment);
                cmp.GetStatements.AddMethodReturn(item.GetFieldExpression());

                classType.Members.Add(cmp);
            }
        }

        private static void CreateNameField(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmf = new CodeMemberField
            {
                Attributes = MemberAttributes.Private,
                Name = "_name",
                Type = new CodeTypeReference(typeof(string))
            };

            classType.Members.Add(cmf);
        }

        private static void CreateRevisionField(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmf = new CodeMemberField
            {
                Attributes = MemberAttributes.Private,
                Name = "_revision",
                Type = new CodeTypeReference(typeof(string))
            };

            classType.Members.Add(cmf);
        }

        private static void CreateTypesHashValueField(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmf = new CodeMemberField
            {
                Attributes = MemberAttributes.Private,
                Name = "_typesHashValue",
                Type = new CodeTypeReference(typeof(string))
            };

            classType.Members.Add(cmf);
        }

        private static void CreateTablesHashValueField(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmf = new CodeMemberField
            {
                Attributes = MemberAttributes.Private,
                Name = "_tablesHashValue",
                Type = new CodeTypeReference(typeof(string))
            };

            classType.Members.Add(cmf);
        }

        private static void CreateTagsHashValueField(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmf = new CodeMemberField
            {
                Attributes = MemberAttributes.Private,
                Name = "_tags",
                Type = new CodeTypeReference(typeof(string))
            };

            classType.Members.Add(cmf);
        }

        private static void CreateNameProperty(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "name",
                HasGet = true,
                Type = new CodeTypeReference(typeof(string))
            };
            cmm.GetStatements.AddMethodReturn(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "_name"));

            classType.Members.Add(cmm);
        }

        private static void CreateRevisionProperty(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "revision",
                HasGet = true,
                Type = new CodeTypeReference(typeof(string))
            };
            cmm.GetStatements.AddMethodReturn(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "_revision"));

            classType.Members.Add(cmm);
        }

        private static void CreateTypesHashValueProperty(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "typesHashValue",
                HasGet = true,
                Type = new CodeTypeReference(typeof(string))
            };
            cmm.GetStatements.AddMethodReturn(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "_typesHashValue"));

            classType.Members.Add(cmm);
        }

        private static void CreateTablesHashValueProperty(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "tablesHashValue",
                HasGet = true,
                Type = new CodeTypeReference(typeof(string))
            };
            cmm.GetStatements.AddMethodReturn(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "_tablesHashValue"));

            classType.Members.Add(cmm);
        }

        private static void CreateTagsProperty(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var cmm = new CodeMemberProperty
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "tags",
                HasGet = true,
                Type = new CodeTypeReference(typeof(string))
            };
            cmm.GetStatements.AddMethodReturn(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "_tags"));

            classType.Members.Add(cmm);
        }

        private static CodeStatement CreateCompareDataBaseStatement(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var ccs = new CodeConditionStatement();

            var left = new CodePrimitiveExpression(generationInfo.DataBaseName);
            var right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataSet"), "name");
            ccs.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, right);

            var message = string.Format("데이터의 이름이 코드 이름({0})과 다릅니다.", generationInfo.DataBaseName);
            var exception = new CodeObjectCreateExpression("Error", new CodePrimitiveExpression(message));
            ccs.TrueStatements.Add(new CodeThrowExceptionStatement(exception));

            return ccs;
        }

        private static CodeStatement CreateCompareRevisionStatement(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var ccs = new CodeConditionStatement();

            var left1 = new CodeVariableReferenceExpression("verifyRevision");
            var right1 = new CodePrimitiveExpression(true);
            var left = new CodeBinaryOperatorExpression(left1, CodeBinaryOperatorType.IdentityEquality, right1);

            var left2 = new CodeCastExpression(typeof(string), new CodePrimitiveExpression(generationInfo.Revision));
            var right2 = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataSet"), "revision");
            var right = new CodeBinaryOperatorExpression(left2, CodeBinaryOperatorType.IdentityInequality, right2);

            ccs.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanAnd, right);

            var message = string.Format("데이터의 리비전 코드 리비전({0})과 다릅니다.", generationInfo.Revision);
            var exception = new CodeObjectCreateExpression("Error", new CodePrimitiveExpression(message));
            ccs.TrueStatements.Add(new CodeThrowExceptionStatement(exception));

            return ccs;
        }

        private static CodeStatement CreateCompareTypesHashValueStatement(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var ccs = new CodeConditionStatement();

            var left = new CodePrimitiveExpression(generationInfo.TypesHashValue);
            var right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataSet"), "typesHashValue");
            ccs.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, right);

            var message = string.Format("타입 해시값이 '{0}'이 아닙니다.", generationInfo.TypesHashValue);
            var exception = new CodeObjectCreateExpression("Error", new CodePrimitiveExpression(message));
            ccs.TrueStatements.Add(new CodeThrowExceptionStatement(exception));

            return ccs;
        }

        private static CodeStatement CreateCompareTablesHashValueStatement(CodeTypeDeclaration classType, CodeGenerationInfo generationInfo)
        {
            var ccs = new CodeConditionStatement();

            var left = new CodePrimitiveExpression(generationInfo.TablesHashValue);
            var right = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("dataSet"), "tablesHashValue");
            ccs.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, right);

            var message = string.Format("테이블 해시값이 '{0}'이 아닙니다.", generationInfo.TablesHashValue);
            var exception = new CodeObjectCreateExpression("Error", new CodePrimitiveExpression(message));
            ccs.TrueStatements.Add(new CodeThrowExceptionStatement(exception));

            return ccs;
        }

        public static CodeStatement CreateTryCatchStatement(CodeTypeDeclaration classType, CodeStatement statement, bool isDevmode)
        {
            if (isDevmode == false)
                return statement;

            var tryStatement = new CodeTryCatchFinallyStatement();

            tryStatement.TryStatements.Add(statement);

            var catchClause = new CodeCatchClause("e");
            var methodRefExp = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "onErrorOccured");
            var methodInvokeExp = new CodeMethodInvokeExpression(methodRefExp, new CodeVariableReferenceExpression("e"));

            var conditionStatement = new CodeConditionStatement
            {
                Condition = new CodeBinaryOperatorExpression(methodInvokeExp, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(false))
            };
            conditionStatement.TrueStatements.Add(new CodeThrowExceptionStatement(new CodeVariableReferenceExpression("e")));

            catchClause.Statements.Add(conditionStatement);
            tryStatement.CatchClauses.Add(catchClause);

            return tryStatement;
        }
    }
}

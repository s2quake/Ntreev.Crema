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

using Microsoft.CSharp;
using Ntreev.Crema.Runtime.Generation;
using Ntreev.Crema.Runtime.Generation.Cpp.Properties;
using Ntreev.Library;
using Ntreev.Library.IO;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Runtime.Generation.Cpp
{
    [Export(typeof(ICodeGenerator))]
    sealed class CremaCodeGenerator : ICodeGenerator
    {
        public void Generate(string outputPath, GenerationSet metaData, CodeGenerationSettings settings)
        {
            var path = PathUtility.GetFullPath(outputPath);
            var generationInfo = new CodeGenerationInfo(metaData, settings) { RelativePath = string.Empty, };

            if (settings.BasePath != string.Empty)
            {
                var relativePath = UriUtility.MakeRelativeOfDirectory(path, DirectoryUtility.GetAbsolutePath(path, settings.BasePath));
                generationInfo.RelativePath = relativePath + "/";
            }

            {
                var codes = this.Generate(generationInfo);
                var dirInfo = new DirectoryInfo(path);
                var rootPath = generationInfo.Namespace.Replace('.', Path.DirectorySeparatorChar);
                foreach (var item in codes)
                {
                    var ext = Path.GetExtension(item.Key);
                    var filename = FileUtility.RemoveExtension(item.Key).Replace('.', Path.DirectorySeparatorChar) + ext;
                    filename = Path.Combine(dirInfo.FullName, filename);
                    FileUtility.Prepare(filename);

                    using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
                    {
                        sw.WriteLine(item.Value);
                        this.PrintResult(filename);
                    }
                }
            }

            if (settings.Options.HasFlag(CodeGenerationOptions.OmitBaseCode) == false)
            {
                var codes = this.GenerateBases(generationInfo);
                var codesPath = DirectoryUtility.GetAbsolutePath(path, settings.BasePath);
                foreach (var item in codes)
                {
                    var codePath = FileUtility.Prepare(codesPath, item.Key);
                    using (var writer = new StreamWriter(codePath, false, Encoding.UTF8))
                    {
                        writer.WriteLine(item.Value);
                        this.PrintResult(codePath);
                    }
                }
            }
        }

        private IDictionary<string, string> GenerateBases(CodeGenerationInfo generationInfo)
        {
            var codes = new Dictionary<string, string>();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            codes.Add("crema_base.h", this.GenerateBaseHeader(generationInfo));
            codes.Add("crema_base.cpp", this.GenerateBaseCpp(generationInfo));
            codes.Add("crema_reader.h", this.GenerateReaderHeader(generationInfo));
            codes.Add("crema_reader.cpp", this.GenerateReaderCpp(generationInfo));

            return codes;
        }

        public IDictionary<string, string> Generate(CodeGenerationInfo generationInfo)
        {
            var codes = new Dictionary<string, string>();
            var codeDomProvider = new CodeDom.NativeCCodeProvider();
            var options = new CodeGeneratorOptions();
            options.BlankLinesBetweenMembers = generationInfo.BlankLinesBetweenMembers;
            options.BracingStyle = "C";
            options.ElseOnClosing = false;
            options.IndentString = IndentedTextWriter.DefaultTabString;
            options["CustomGeneratorOptionStringExampleID"] = "BuildFlags: /A /B /C /D /E";

            ColumnInfoExtensions.TypeNamespace = generationInfo.Namespace;

            var cremaTypes = GenerateTypes(codeDomProvider, options, generationInfo);
            var cremaTables = GenerateTables(codeDomProvider, options, generationInfo);

            SplitCode(cremaTypes, $"{generationInfo.Prefix}types{generationInfo.Postfix}.h", out var cremaTypesHeader, out var cremaTypesCpp);
            SplitCode(cremaTables, $"{generationInfo.Prefix}tables{generationInfo.Postfix}.h", out var cremaTablesHeader, out var cremaTablesCpp);

            codes.Add($"{generationInfo.Prefix}types{generationInfo.Postfix}.h", cremaTypesHeader);
            codes.Add($"{generationInfo.Prefix}types{generationInfo.Postfix}.cpp", cremaTypesCpp);
            codes.Add($"{generationInfo.Prefix}tables{generationInfo.Postfix}.h", cremaTablesHeader);
            codes.Add($"{generationInfo.Prefix}tables{generationInfo.Postfix}.cpp", cremaTablesCpp);

            return codes;
        }

        public IDictionary<string, object> Compile(IDictionary<string, string> sources, string target)
        {
            throw new NotImplementedException();
        }

        public bool SupportsCompile
        {
            get { return false; }
        }

        public string Name
        {
            get { return "cpp"; }
        }

        private string GenerateBaseHeader(CodeGenerationInfo generationInfo)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = string.Join(".", assembly.GetName().Name, "code", "crema_base.h");
            var code = this.GetResourceString(resourceName);
            return this.ChangeBaseNamespace(code, generationInfo);
        }

        private string GenerateBaseCpp(CodeGenerationInfo generationInfo)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = string.Join(".", assembly.GetName().Name, "code", "crema_base.cpp");
            var code = this.GetResourceString(resourceName);
            return this.ChangeBaseNamespace(code, generationInfo);
        }

        private string GenerateReaderHeader(CodeGenerationInfo generationInfo)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = string.Join(".", assembly.GetName().Name, "code", "crema_reader.h");
            var code = this.GetResourceString(resourceName);
            return this.ChangeReadNamespace(code, generationInfo);
        }

        private string GenerateReaderCpp(CodeGenerationInfo generationInfo)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = string.Join(".", assembly.GetName().Name, "code", "crema_reader.cpp");
            var code = this.GetResourceString(resourceName);
            return this.ChangeReadNamespace(code, generationInfo);
        }

        private string ChangeBaseNamespace(string text, CodeGenerationInfo generationInfo)
        {
            var segments = generationInfo.BaseNamespace.Split(new string[] { "::", }, StringSplitOptions.RemoveEmptyEntries);
            var firstNamespace = string.Join(" { ", segments.Select(item => "namespace " + item));
            var lastNamespace = string.Join(" ", segments.Select(item => "} /*namespace " + item + "*/"));

            var first = "namespace CremaCode";
            var last = "} /*namespace CremaCode*/";

            text = text.Replace(last, lastNamespace);
            text = text.Replace(first, firstNamespace);
            return text;
        }

        private string ChangeReadNamespace(string text, CodeGenerationInfo generationInfo)
        {
            var readerNamespace = generationInfo.ReaderNamespace;
            var segments = readerNamespace.Split(new string[] { "::", }, StringSplitOptions.RemoveEmptyEntries);
            var firstNamespace = string.Join(" { ", segments.Select(item => "namespace " + item));
            var lastNamespace = string.Join(" ", segments.Select(item => "} /*namespace " + item + "*/"));

            var first = "namespace CremaReader";
            var last = "} /*namespace CremaReader*/";

            text = text.Replace(last, lastNamespace);
            text = text.Replace(first, firstNamespace);
            return text;
        }

        private string GenerateTables(CodeDomProvider codeDomProvider, CodeGeneratorOptions options, CodeGenerationInfo generationInfo)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var codeGenerator = codeDomProvider.CreateGenerator(sw);
                var compileUnit = new CodeCompileUnit();

                compileUnit.AddCustomInclude($"{generationInfo.RelativePath}crema_reader");
                compileUnit.AddCustomInclude($"{generationInfo.Prefix}types{generationInfo.Postfix}");
                compileUnit.AddCustomInclude($"{generationInfo.RelativePath}crema_base");

                var codeNamespace = new CodeNamespace(generationInfo.Namespace);
                codeNamespace.Imports.Add(new CodeNamespaceImport(generationInfo.ReaderNamespace));

                CremaDataClassCreator.Create(codeNamespace, generationInfo);

                compileUnit.Namespaces.Add(codeNamespace);

                codeGenerator.GenerateCodeFromCompileUnit(compileUnit, sw, options);
            }

            return sb.ToString();
        }

        private string GenerateTypes(CodeDomProvider codeDomProvider, CodeGeneratorOptions options, CodeGenerationInfo generationInfo)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var codeGenerator = codeDomProvider.CreateGenerator(sw);
                var compileUnit = new CodeCompileUnit();

                var codeNamespace = new CodeNamespace(generationInfo.Namespace);
                codeNamespace.Imports.Add(new CodeNamespaceImport(generationInfo.ReaderNamespace));

                CremaTypeEnumCreator.Create(codeNamespace, generationInfo);

                compileUnit.Namespaces.Add(codeNamespace);

                codeGenerator.GenerateCodeFromCompileUnit(compileUnit, sw, options);
            }

            return sb.ToString();
        }

        private static void SplitCode(string code, string filename, out string header, out string cpp)
        {
            int index = code.IndexOf("/// cpp start");
            if (index >= 0)
            {
                header = code.Substring(0, index);
                cpp = code.Substring(index);

                var sb = new StringBuilder();
                sb.AppendFormat("#include \"{0}\"", Path.GetFileName(filename));
                sb.AppendLine();
                sb.Append(cpp);
                cpp = sb.ToString();
                cpp = cpp.Replace("/// cpp start", string.Empty);
                cpp = cpp.Replace("/// cpp end", string.Empty);
            }
            else
            {
                header = code;
                cpp = null;
            }
        }

        private string GetResourceString(string resourceName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return stream.ReadToEnd();
            }
        }

        private void PrintResult(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            Console.WriteLine("created: {0}", fileInfo.FullName);
        }
    }
}

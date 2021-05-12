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

using JSSoft.Crema.Runtime.Generation;
using JSSoft.Library.Commands;
using System.ComponentModel;

namespace JSSoft.Crema.Commands
{
    static class CodeSettings
    {
        [CommandProperty("lang")]
        [DefaultValue("cs")]
        public static string LanguageType
        {
            get; set;
        }

        [CommandProperty("no-comment")]
        [Description("주석을 생성하지 않습니다.")]
        [DefaultValue(false)]
        public static bool OmitComment
        {
            get;
            set;
        }

        [CommandProperty("no-signature-date")]
        [Description("항목의 변경 정보를 생성하지 않습니다.")]
        [DefaultValue(false)]
        public static bool OmitSignatureDate
        {
            get;
            set;
        }

        [CommandProperty("namespace", 'n')]
        [Description("네임스페이스를 설정합니다.")]
        [DefaultValue("")]
        public static string Namespace
        {
            get;
            set;
        }

        [CommandProperty]
        [Description("기본 코드의 네임스페이스를 설정합니다.")]
        [DefaultValue("")]
        public static string BaseNamespace
        {
            get;
            set;
        }

        [CommandProperty]
        [Description("기본 코드와 데이터를 읽어들이는 코드의 출력 경로를 설정합니다.")]
        [DefaultValue("")]
        public static string BasePath
        {
            get;
            set;
        }

        [CommandProperty("no-base-code")]
        [Description("기본 코드와 데이터를 읽어들이는 코드를 출력하지 않습니다..")]
        [DefaultValue(false)]
        public static bool OmitBaseCode
        {
            get;
            set;
        }

        [CommandProperty("no-code")]
        [Description("코드를 출력하지 않습니다..")]
        [DefaultValue(false)]
        public static bool OmitCode
        {
            get;
            set;
        }

        [CommandProperty]
        [Description("생성되는 파일명에 붙는 접두사를 설정합니다.")]
        [DefaultValue("")]
        public static string Prefix
        {
            get;
            set;
        }

        [CommandProperty]
        [Description("생성되는 파일명에 붙는 접미사를 설정합니다.")]
        [DefaultValue("")]
        public static string Postfix
        {
            get;
            set;
        }

        [CommandProperty("build-target")]
        [Description("소스코드 컴파일시 대상 버전을 설정합니다.")]
        [DefaultValue("")]
        public static string BuildTarget
        {
            get;
            set;
        }

        [CommandProperty("build")]
        [DefaultValue(false)]
        public static bool IsBuildMode
        {
            get;
            set;
        }

        [CommandProperty]
        [Description("기본 클래스의 이름을 설정합니다.")]
        [DefaultValue("")]
        public static string ClassName
        {
            get;
            set;
        }

        public static CodeGenerationOptions Options
        {
            get
            {
                var options = CodeGenerationOptions.None;
                if (OmitComment == true)
                    options |= CodeGenerationOptions.OmitComments;
                if (OmitSignatureDate == true)
                    options |= CodeGenerationOptions.OmitSignatureDate;
                if (OmitBaseCode == true)
                    options |= CodeGenerationOptions.OmitBaseCode;
                if (OmitCode == true)
                    options |= CodeGenerationOptions.OmitCode;
                return options;
            }
        }
    }
}

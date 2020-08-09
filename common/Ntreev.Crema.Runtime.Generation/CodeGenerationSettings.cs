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

using Ntreev.Library;
using System.Collections.Generic;

namespace Ntreev.Crema.Runtime.Generation
{
    public class CodeGenerationSettings
    {
        private string className;
        private string classNamespace;
        private string baseNamespace;
        private string prefix;
        private string postfix;
        private string basePath;
        private TagInfo tags;
        private string revision;
        private readonly Dictionary<string, object> arguments = new Dictionary<string, object>();

        public string ClassName
        {
            get => this.className ?? string.Empty;
            set => this.className = value;
        }

        public string Namespace
        {
            get => this.classNamespace ?? string.Empty;
            set => this.classNamespace = value;
        }

        public string BaseNamespace
        {
            get => this.baseNamespace ?? string.Empty;
            set => this.baseNamespace = value;
        }

        public string Prefix
        {
            get => this.prefix ?? string.Empty;
            set => this.prefix = value;
        }

        public string Postfix
        {
            get => this.postfix ?? string.Empty;
            set => this.postfix = value;
        }

        public string BasePath
        {
            get => this.basePath ?? string.Empty;
            set => this.basePath = value;
        }

        public TagInfo Tags
        {
            get => this.tags;
            set => this.tags = value;
        }

        public string Revision
        {
            get => this.revision;
            set => this.revision = value;
        }

        public CodeGenerationOptions Options { get; set; }

        public IDictionary<string, object> Arguments => this.arguments;

        public readonly static CodeGenerationSettings Default = new CodeGenerationSettings()
        {
            ClassName = string.Empty,
            Namespace = string.Empty,
            BaseNamespace = string.Empty,
            prefix = string.Empty,
            postfix = string.Empty,
            BasePath = string.Empty,
            Options = CodeGenerationOptions.None,
            Revision = null,
        };
    }
}

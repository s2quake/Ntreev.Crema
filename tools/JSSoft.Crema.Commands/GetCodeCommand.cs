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
using JSSoft.Crema.RuntimeService;
using JSSoft.Library;
using JSSoft.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands
{
    [Export(typeof(ICommand))]
    [CommandStaticProperty(typeof(CodeSettings))]
    [CommandStaticProperty(typeof(FilterSettings))]
    [CommandStaticProperty(typeof(DataBaseSettings))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:읽기 전용 한정자 추가", Justification = "<보류 중>")]
    class GetCodeCommand : CommandAsyncBase
    {
        [Import]
        private IRuntimeService service = null;
        [ImportMany]
        private IEnumerable<ICodeGenerator> generators = null;
        [ImportMany]
        private IEnumerable<ICodeCompiler> compilers = null;

        public GetCodeCommand()
            : base("get-code")
        {
        }

        [CommandPropertyRequired]
        public string Address
        {
            get; set;
        }

        [CommandPropertyRequired]
        public string OutputPath
        {
            get; set;
        }

        [CommandProperty]
        public string Revision
        {
            get; set;
        }

        [CommandProperty]
#if DEBUG
        [DefaultValue("en-US")]
#else
        [DefaultValue("")]
#endif
        public string Culture
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            if (this.Culture != string.Empty)
            {
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(this.Culture);
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(this.Culture);
            }

            if (CodeSettings.IsBuildMode == true)
            {
                await this.CompileCodeAsync();
            }
            else
            {
                await this.GenerateCodeAsync();
            }
        }

        private async Task GenerateCodeAsync()
        {
            var generator = this.generators.FirstOrDefault(item => item.Name == CodeSettings.LanguageType);
            if (generator == null)
                throw new InvalidOperationException($"'{CodeSettings.LanguageType}'은(는) 존재하지 언어입니다.");

            this.Out.WriteLine("receiving info");
            var metaData = await this.service.GetMetaDataAsync(this.Address, DataBaseSettings.DataBaseName, DataBaseSettings.Tags, FilterSettings.FilterExpression, this.Revision);

            var generationSettings = new CodeGenerationSettings()
            {
                ClassName = CodeSettings.ClassName,
                Namespace = CodeSettings.Namespace,
                BaseNamespace = CodeSettings.BaseNamespace,
                Prefix = CodeSettings.Prefix,
                Postfix = CodeSettings.Postfix,
                BasePath = CodeSettings.BasePath,
                Options = CodeSettings.Options,
                Tags = (TagInfo)DataBaseSettings.Tags,
                Revision = this.Revision,
            };

            this.Out.WriteLine("generating code.");
            generator.Generate(this.OutputPath, metaData.Item1, generationSettings);
            this.Out.WriteLine("code generated.");
        }

        private async Task CompileCodeAsync()
        {
            var compiler = this.compilers.FirstOrDefault(item => item.Name == CodeSettings.LanguageType);
            if (compiler == null)
                throw new InvalidOperationException($"'{CodeSettings.LanguageType}'은(는) 존재하지 언어입니다.");

            this.Out.WriteLine("receiving info");
            var metaData = await this.service.GetMetaDataAsync(this.Address, DataBaseSettings.DataBaseName, DataBaseSettings.Tags, FilterSettings.FilterExpression, null);

            var generationSettings = new CodeGenerationSettings()
            {
                ClassName = CodeSettings.ClassName,
                Namespace = CodeSettings.Namespace,
                BaseNamespace = CodeSettings.BaseNamespace,
                Prefix = CodeSettings.Prefix,
                Postfix = CodeSettings.Postfix,
                BasePath = CodeSettings.BasePath,
                Options = CodeSettings.Options,
                Tags = (TagInfo)DataBaseSettings.Tags,
                Revision = this.Revision,
            };

            this.Out.WriteLine("code compiling.");
            compiler.Compile(this.OutputPath, metaData.Item1, generationSettings, CodeSettings.BuildTarget);
            this.Out.WriteLine("code compiled.");
        }
    }
}

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

using JSSoft.Crema.Commands.Consoles;
using JSSoft.Library.Commands;
using JSSoft.Library.IO;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

namespace JSSoft.Crema.Javascript.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class RunCommand : ConsoleCommandBase
    {
        private readonly Lazy<ScriptContext> scriptContext;

        [ImportingConstructor]
        public RunCommand(Lazy<ScriptContext> scriptContext)
            : base("run")
        {
            this.scriptContext = scriptContext;
        }

        [CommandPropertyRequired]
        [CommandPropertyTrigger(nameof(Filename), "")]
        [CommandPropertyTrigger(nameof(List), false)]
        [DefaultValue("")]
        public string Scripts
        {
            get; set;
        }

        [CommandProperty()]
        [DefaultValue("")]
        [CommandPropertyTrigger(nameof(Scripts), "")]
        [CommandPropertyTrigger(nameof(List), false)]
        public string Filename
        {
            get; set;
        }

        [CommandProperty("list", 'l')]
        [CommandPropertyTrigger(nameof(Scripts), "")]
        [CommandPropertyTrigger(nameof(Filename), "")]
        [DefaultValue(false)]
        public bool List
        {
            get; set;
        }

        [CommandProperty("async")]
        [CommandPropertyTrigger(nameof(List), false)]
        public bool IsAsync
        {
            get; set;
        }

        [CommandPropertyArray]
        public string[] Arguments
        {
            get;
            set;
        }

        protected override void OnExecute()
        {
            if (this.List == true)
            {
                this.CommandContext.Out.Write(this.ScriptContext.GenerateDeclaration(ScriptContextBase.GetArgumentTypes(this.Arguments)));
            }
            else
            {
                var oldPath = Directory.GetCurrentDirectory();
                try
                {
                    DirectoryUtility.Prepare(this.CommandContext.BaseDirectory);
                    Directory.SetCurrentDirectory(this.CommandContext.BaseDirectory);
                    if (this.Filename != string.Empty)
                    {
                        this.Scripts = File.ReadAllText(this.Filename);
                    }

                    var authentication = this.CommandContext.GetAuthenticationInternal(this);
                    if (this.IsAsync == false)
                        this.ScriptContext.RunInternal(this.Scripts, authentication);
                    else
                        this.ScriptContext.RunAsyncInternal(this.Scripts, authentication);
                }
                finally
                {
                    Directory.SetCurrentDirectory(oldPath);
                }
            }
        }

        private ScriptContext ScriptContext => this.scriptContext.Value;
    }
}
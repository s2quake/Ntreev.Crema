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

using JSSoft.Crema.ConsoleHost.Commands.Consoles;
using JSSoft.Crema.Javascript;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost.Commands
{
    [Export(typeof(ICommand))]
    [Export]
    [ResourceUsageDescription]
    class RunCommand : CommandAsyncBase
    {
        private readonly CremaApplication application;

        [ImportingConstructor]
        public RunCommand(CremaApplication application, ScriptContext scriptContext)
        {
            this.application = application;
            // this.cremaHost = cremaHost;
            this.ScriptContext = scriptContext;
        }

        [CommandProperty(InitValue = "localhost")]
        public string Address
        {
            get;
            set;
        }

        [CommandPropertySwitch("prompt", 'p')]
        [CommandPropertyTrigger(nameof(ScriptPath), "")]
        [CommandPropertyTrigger(nameof(Script), "")]
        public bool IsPromptMode
        {
            get;
            set;
        }

        [CommandProperty]
        [CommandPropertyTrigger(nameof(IsPromptMode), false)]
        [CommandPropertyTrigger(nameof(Script), "")]
        public string ScriptPath
        {
            get;
            set;
        }

        [CommandProperty]
        [CommandPropertyTrigger(nameof(IsPromptMode), false)]
        [CommandPropertyTrigger(nameof(ScriptPath), "")]
        public string Script
        {
            get;
            set;
        }

        // [CommandProperty("list", 'l')]
        // [CommandPropertyTrigger(nameof(ScriptPath), "")]
        // [DefaultValue(false)]
        // public bool List
        // {
        //     get; set;
        // }

        [CommandProperty]
        [CommandPropertyTrigger(nameof(IsPromptMode), false)]
        public string ScriptEntry
        {
            get;
            set;
        }

        [CommandPropertyArray]
        public string[] Arguments
        {
            get;
            set;
        }

#if DEBUG
        [CommandProperty(InitValue = "en-US")]
#else
        [CommandProperty]
#endif
        public string Culture
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            this.application.Culture = this.Culture;
            this.application.Verbose = LogVerbose.None;
            this.application.Address = this.Address;
            // this.CommandContext.SetAddress(this.Address);
            // this.CommandContext.SetAddress(this.Address);
            await this.application.OpenAsync();
            await this.WaitAsync();

            if (this.application.ServiceState == ServiceState.Open)
            {
                await this.application.CloseAsync();
            }

            // if (this.List == true)
            // {
            //     this.Out.Write(this.scriptContext.GenerateDeclaration(this.GetArgumentTypes()));
            // }
            // else
            // {
            //     if (File.Exists(this.ScriptPath) == false)
            //         throw new FileNotFoundException(this.ScriptPath);
            //     this.scriptContext.RunFromFile(this.ScriptPath, this.ScriptEntry, this.GetProperties(), null);
            // }
        }

        protected ScriptContext ScriptContext { get; }

        private async Task WaitAsync()
        {
            if (this.IsPromptMode == true)
            {
                await this.Terminal.StartAsync();
            }
            else if (this.ScriptPath != string.Empty)
            {
                // var cremaHost = this.application.GetService(typeof(ICremaHost)) as ICremaHost;
                // var script = File.ReadAllText(this.ScriptPath);
                // var basePath = cremaHost.GetPath(CremaPath.Documents);
                // var oldPath = Directory.GetCurrentDirectory();
                // try
                // {
                //     DirectoryUtility.Prepare(basePath);
                //     Directory.SetCurrentDirectory(basePath);
                //     this.scriptContext.RunInternal(script, null);
                // }
                // finally
                // {
                //     Directory.SetCurrentDirectory(oldPath);
                // }
            }
            else
            {
                // Console.WriteLine(Resources.ConnectionMessage);
                // if (Console.IsInputRedirected == false)
                // {
                //     while (Console.ReadKey().Key != ConsoleKey.Q) ;
                // }
                // else
                // {
                //     while (Console.ReadLine() != "exit") ;
                // }
            }
        }

        private ConsoleTerminal Terminal => this.application.GetService(typeof(ConsoleTerminal)) as ConsoleTerminal;
    }
}
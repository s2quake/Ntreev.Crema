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
using JSSoft.Crema.ConsoleHost.Properties;
using JSSoft.Crema.Javascript;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using JSSoft.Library.IO;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
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
        private readonly ScriptContext scriptContext;

        [ImportingConstructor]
        public RunCommand(CremaApplication application, ScriptContext scriptContext)
        {
            this.application = application;
            this.scriptContext = scriptContext;
        }

        [CommandPropertyRequired("path")]
        public string Path
        {
            get;
            set;
        }

        [CommandProperty("port", InitValue = AddressUtility.DefaultPort)]
        public int Port
        {
            get;
            set;
        }

#if DEBUG
        [CommandProperty("timeout", InitValue = -1)]
#else
        [CommandProperty("timeout", InitValue = 60000)]
#endif
        public int Timeout
        {
            get;
            set;
        }

        [CommandProperty("repo-module")]
        public string RepositoryModule { get; set; }

        [CommandProperty("file-type")]
        public string FileType
        {
            get;
            set;
        }

        [CommandPropertySwitch("prompt", 'p')]
        [CommandPropertyTrigger(nameof(ScriptPath), "")]
        public bool IsPromptMode
        {
            get;
            set;
        }

        [CommandProperty]
        [CommandPropertyTrigger(nameof(IsPromptMode), false)]
        public string ScriptPath
        {
            get;
            set;
        }

        [CommandProperty]
        public bool Verbose
        {
            get; set;
        }

        [CommandProperty]
        public bool NoCache
        {
            get;
            set;
        }

        [CommandProperty]
        public bool OmitRestore
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

        [CommandPropertyArray]
        [Description("database list to load")]
        public string[] DataBaseList
        {
            get; set;
        }

#if DEBUG
        [CommandProperty("validation", 'v')]
        public bool ValidationMode
        {
            get;
            set;
        }
#endif

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            CremaLog.Verbose = this.Verbose ? LogVerbose.Debug : LogVerbose.Info;
            this.application.BasePath = this.Path;
            this.application.Verbose = this.Verbose ? LogVerbose.Debug : LogVerbose.Info;
            this.application.NoCache = this.NoCache;
            this.application.Culture = this.Culture;
            this.application.DataBaseList = this.DataBaseList;
#if DEBUG
            this.application.ValidationMode = this.ValidationMode;
#endif
            this.application.Port = this.Port;
            this.application.Timeout = this.Timeout;
            await this.application.OpenAsync();
            this.application.Title = $"{this.application.BasePath} --port {this.application.Port}";
            await this.WaitAsync();
            if (this.application.ServiceState == ServiceState.Open)
            {
                await this.Out.WriteLineAsync(Resources.StoppingServer);
                await this.application.CloseAsync();
            }
            await this.Out.WriteLineAsync(Resources.ServerHasBeenStopped);
        }

        private async Task WaitAsync()
        {
            if (this.IsPromptMode == true)
            {
                await this.Terminal.StartAsync();
            }
            else if (this.ScriptPath != string.Empty)
            {
                var cremaHost = this.application.GetService(typeof(ICremaHost)) as ICremaHost;
                var script = File.ReadAllText(this.ScriptPath);
                var basePath = cremaHost.GetPath(CremaPath.Documents);
                var oldPath = Directory.GetCurrentDirectory();
                try
                {
                    DirectoryUtility.Prepare(basePath);
                    Directory.SetCurrentDirectory(basePath);
                    this.scriptContext.RunInternal(script, null);
                }
                finally
                {
                    Directory.SetCurrentDirectory(oldPath);
                }
            }
            else
            {
                Console.WriteLine(Resources.ConnectionMessage);
                if (Console.IsInputRedirected == false)
                {
                    while (Console.ReadKey().Key != ConsoleKey.Q) ;
                }
                else
                {
                    while (Console.ReadLine() != "exit") ;
                }
            }
        }

        private ConsoleTerminal Terminal => this.application.GetService(typeof(ConsoleTerminal)) as ConsoleTerminal;
    }
}

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
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost.Commands
{
    [Export(typeof(ICommand))]
    [Export]
    [ResourceDescription]
    class RunCommand : CommandAsyncBase
    {
        private readonly CremaApplication application;
        [Import]
        private readonly Lazy<ScriptContext> scriptContext = null;

        [ImportingConstructor]
        public RunCommand(CremaApplication application)
            : base("run")
        {
            this.application = application;
        }

        public void Cancel()
        {
            var terminal = this.application.GetService(typeof(ConsoleTerminal)) as ConsoleTerminal;
            terminal.Cancel();
        }

        [CommandPropertyRequired("path")]
        public string Path
        {
            get;
            set;
        }

        [CommandProperty("port")]
        [DefaultValue(AddressUtility.DefaultPort)]
        public int Port
        {
            get;
            set;
        }

        [CommandProperty("timeout")]
#if DEBUG
        [DefaultValue(-1)]
#else
        [DefaultValue(60000)]
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

        [CommandProperty("prompt", 'p')]
        [CommandPropertyTrigger(nameof(ScriptPath), "")]
        public bool IsPromptMode
        {
            get;
            set;
        }

        [CommandProperty]
        [DefaultValue("")]
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

        [CommandPropertyArray]
        [Description("database list to load")]
        public string[] DataBaseList
        {
            get; set;
        }

#if DEBUG

        [CommandProperty('l', DefaultValue = "admin:admin")]
        public string LoginAuthentication
        {
            get;
            set;
        }

        [CommandProperty("validation", 'v')]
        public bool ValidationMode
        {
            get;
            set;
        }
#endif

        protected override async Task OnExecuteAsync()
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
            this.application.Closed += Application_Closed;
            Console.Title = $"{this.application.BasePath} --port {this.application.Port}";
            var cremaHost = this.application.GetService(typeof(ICremaHost)) as ICremaHost;
            await this.WaitAsync(cremaHost);
            if (this.application.ServiceState == ServiceState.Open)
            {
                Console.WriteLine(Resources.StoppingServer);
                await this.application.CloseAsync();
            }
            Console.WriteLine(Resources.ServerHasBeenStopped);
        }

        private void Application_Closed(object sender, ClosedEventArgs e)
        {
            if (e.Reason == CloseReason.Shutdown)
            {
                this.Cancel();
            }
        }

        private async Task WaitAsync(ICremaHost cremaHost)
        {
            if (this.IsPromptMode == true)
            {
                var terminal = this.application.GetService(typeof(ConsoleTerminal)) as ConsoleTerminal;
#if DEBUG
                await terminal.StartAsync(this.LoginAuthentication);
#else
                await Task.Delay(1);
                terminal.Start();
#endif
            }
            else if (this.ScriptPath != string.Empty)
            {
                var script = File.ReadAllText(this.ScriptPath);
                var basePath = cremaHost.GetPath(CremaPath.Documents);
                var oldPath = Directory.GetCurrentDirectory();
                try
                {
                    DirectoryUtility.Prepare(basePath);
                    Directory.SetCurrentDirectory(basePath);
                    this.ScriptContext.RunInternal(script, null);
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

        private ScriptContext ScriptContext => this.scriptContext.Value;
    }
}

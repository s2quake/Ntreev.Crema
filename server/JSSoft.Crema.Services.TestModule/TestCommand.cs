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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using JSSoft.Crema.ConsoleHost;
using System;
using JSSoft.Crema.Services.TestModule.TestCommands;

namespace JSSoft.Crema.Services.TestModule
{
    [Export(typeof(ICommand))]
    class TestCommand : CommandAsyncBase
    {
        private readonly ICremaApplication application;
        private readonly TestCommandContext testCommandContext;

        [ImportingConstructor]
        public TestCommand(ICremaApplication application, TestCommandContext testCommandContext)
            : base("test")
        {
            this.application = application;
            this.testCommandContext = testCommandContext;
        }

        [CommandPropertyRequired]
        public string Path
        {
            get;
            set;
        }

        [CommandProperty("repo-module")]
        public string RepositoryModule
        {
            get;
            set;
        }

        [CommandProperty("file-type")]
        public string FileType
        {
            get;
            set;
        }

        [CommandProperty("database-url")]
        public string DataBaseUrl
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

        [CommandProperty]
        public string StartupMessage
        {
            get; set;
        }

        [CommandProperty(InitValue = "exit")]
        public string CloseCommand
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellation)
        {
            CremaBootstrapper.CreateRepository(this.application, this.Path, this.RepositoryModule, this.FileType, this.DataBaseUrl);

            this.application.BasePath = this.Path;
            this.application.Port = this.Port;
            await this.application.OpenAsync();
            await this.Out.WriteLineAsync(this.StartupMessage);
            while (this.testCommandContext.IsCancellationRequested == false && Console.ReadLine() is string line)
            {
                await this.testCommandContext.ExecuteAsync(line);
            }
            await this.application.CloseAsync();
        }

        private async Task GenerateDataBasesAsync(int count)
        {

        }

        private async Task LoginRandomManyAsync()
        {

        }

        private async Task LoadRandomDataBasesAsync()
        {

        }

        private async Task LockRandomDataBasesAsync()
        {

        }

        private async Task SetPrivateRandomDataBasesAsync()
        {

        }
    }
}

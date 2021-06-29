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
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Data;
using JSSoft.Library.Commands;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using JSSoft.Crema.ConsoleHost;
using System;
using JSSoft.Crema.Services.TestModule.TestCommands;
using JSSoft.Library.IO;
using JSSoft.Crema.Services.Test.Common;
using JSSoft.Library.Random;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.IO.Pipes;

namespace JSSoft.Crema.Services.TestModule
{
    [Export(typeof(ICommand))]
    class TestCommand : CommandAsyncBase
    {
        private readonly ICremaApplication application;
        private readonly TestCommandContext testCommandContext;
        private readonly Cancellation cancellation;

        [ImportingConstructor]
        public TestCommand(ICremaApplication application, TestCommandContext testCommandContext, Cancellation cancellation)
            : base("test")
        {
            this.application = application;
            this.testCommandContext = testCommandContext;
            this.cancellation = cancellation;
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

        [CommandProperty(InitValue = "8c93fab8-d772-44ef-836c-e491f03b300e")]
        public string Separator
        {
            get; set;
        }

        [CommandProperty]
        public string PipeOutput
        {
            get; set;
        }

        [CommandProperty]
        public string PipeError
        {
            get; set;
        }

        [CommandProperty]
        public string PipeInput
        {
            get; set;
        }

        [CommandPropertySwitch]
        public bool Force
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellation)
        {
            this.CreateRepository();
            try
            {
                using var inputStream = new AnonymousPipeClientStream(PipeDirection.In, this.PipeInput);
                using var outputStream = new AnonymousPipeClientStream(PipeDirection.Out, this.PipeOutput);
                using var errorStream = new AnonymousPipeClientStream(PipeDirection.Out, this.PipeError);

                using var inputReader = new StreamReader(inputStream);
                using var outputWriter = new StreamWriter(outputStream) { AutoFlush = true };
                using var errorWriter = new StreamWriter(errorStream) { AutoFlush = true };

                this.application.BasePath = this.Path;
                this.application.Port = this.Port;
                await this.application.OpenAsync();
                await outputWriter.WriteLineAsync(this.Separator);

                this.testCommandContext.Out = outputWriter;
                while (this.cancellation.IsCancellationRequested == false && (await inputReader.ReadLineAsync()) is string line)
                {
                    try
                    {
                        await this.testCommandContext.ExecuteAsync(line);
                    }
                    catch (Exception e)
                    {
                        await errorWriter.WriteLineAsync(e.Message);
                    }
                }
                await errorWriter.WriteLineAsync();
                await outputWriter.WriteLineAsync();
                await this.application.CloseAsync();
            }
            catch (Exception e)
            {
                await this.Out.WriteLineAsync(e.Message);
                await this.Error.WriteLineAsync(e.Message);
            }
            finally
            {
                this.DeleteRepository();
            }
        }

        private void CreateRepository()
        {
            var userInfos = UserInfoGenerator.Generate(0, 0);
            var dataSet = new CremaDataSet();
            try
            {
                CremaBootstrapper.CreateRepositoryInternal(this.application, this.Path, this.RepositoryModule, this.FileType, this.DataBaseUrl, userInfos, dataSet);
            }
            catch
            {
                if (this.Force == true)
                {
                    DirectoryUtility.Delete(this.Path);
                }
                CremaBootstrapper.CreateRepositoryInternal(this.application, this.Path, this.RepositoryModule, this.FileType, this.DataBaseUrl, userInfos, dataSet);
            }
        }

        private void DeleteRepository()
        {
            DirectoryUtility.Delete(this.Path);
        }
    }
}

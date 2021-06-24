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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Test.Extensions;
using JSSoft.Crema.Services.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSSoft.Library.IO;
using JSSoft.Crema.Data;
using JSSoft.Library;
using System.IO;

namespace JSSoft.Crema.Services.Test
{
    partial class TestApplication : CremaBootstrapper
    {
        private static readonly object obj = new();
        private static readonly int startPort = 4004;
        private static readonly HashSet<int> reservedPort = new();
        private readonly TestServerHost serverHost = new ();

        public async Task InitializeAsync(TestContext context)
        {
            var repositoryPath = DirectoryUtility.Prepare(context.TestRunDirectory, "repo", context.FullyQualifiedTestClassName);
            var solutionPath = Path.GetFullPath(Path.Combine(context.DeploymentDirectory, "..", "..", "..", "..", ".."));
            var executablePath = Path.Combine(solutionPath, "server", "JSSoft.Crema.Services.TestModule", "bin", "Debug", "netcoreapp3.1", "cremaserver.dll");
            var port = ReservePort();
            this.Address = $"localhost:{port}";
            this.cremaHost = this.GetService(typeof(ICremaHost)) as ICremaHost;
            this.serverHost.ExecutablePath = executablePath;
            this.serverHost.RepositoryPath = repositoryPath;
            this.serverHost.WorkingPath = solutionPath;
            this.serverHost.Port = port;
            await this.serverHost.StartAsync();
        }

        public async Task ReleaseAsync()
        {
            if (this.serverHost.IsOpen == true)
                await this.serverHost.StopAsync();
            this.Dispose();
            ReleasePort(this.serverHost.Port);
        }

        private static int ReservePort()
        {
            lock (obj)
            {
                for (var i = startPort; i < int.MaxValue; i++)
                {
                    if (reservedPort.Contains(i) == false)
                    {
                        reservedPort.Add(i);
                        return i;
                    }
                }
                throw new NotImplementedException();
            }
        }

        private static void ReleasePort(int port)
        {
            lock (obj)
            {
                reservedPort.Remove(port);
            }
        }
    }
}

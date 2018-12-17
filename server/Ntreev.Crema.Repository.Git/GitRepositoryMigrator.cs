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

using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Repository.Git
{
    [Export(typeof(IRepositoryMigrator))]
    class GitRepositoryMigrator : IRepositoryMigrator
    {
        private readonly GitRepositoryProvider repositoryProvider;

        [ImportingConstructor]
        public GitRepositoryMigrator(GitRepositoryProvider repositoryProvider)
        {
            this.repositoryProvider = repositoryProvider;
        }

        public IRepositoryProvider RepositoryProvider => this.repositoryProvider;

        public string Name => this.repositoryProvider.Name;

        public string Migrate(string sourcePath)
        {
            var repositoryPath2 = PathUtility.GetTempPath(false);
            var repositoryUri = new Uri(sourcePath).ToString();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                repositoryUri = Regex.Replace(repositoryUri, "(file:///\\w):(.+)", "$1$2");
            }

            var cloneCommand = new GitCommand(null, "svn clone")
            {
                (GitPath)repositoryUri,
                (GitPath)repositoryPath2,
                new GitCommandItem('T', "trunk"),
                new GitCommandItem('b', "branches"),
                new GitCommandItem('b', "tags")
            };
            cloneCommand.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            cloneCommand.Run();

            var remoteBranches = GitBranchCollection.GetRemoteBranches(repositoryPath2);
            var branches = GitBranchCollection.Run(repositoryPath2);

            foreach (var item in remoteBranches)
            {
                if (item != "trunk" && branches.Contains(item) == false)
                {
                    var checkoutCommand = new GitCommand(repositoryPath2, "checkout")
                    {
                        new GitCommandItem('b'),
                        item,
                        $"remotes/origin/{item}"
                    };
                    checkoutCommand.Run();
                }
            }

            var configCommand = new GitCommand(repositoryPath2, "config")
            {
                "receive.denyCurrentBranch",
                "ignore"
            };
            configCommand.Run();
            return repositoryPath2;
        }
    }
}

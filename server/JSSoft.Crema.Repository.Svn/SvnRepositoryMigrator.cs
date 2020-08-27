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

using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace JSSoft.Crema.Repository.Svn
{
    [Export(typeof(IRepositoryMigrator))]
    class SvnRepositoryMigrator : IRepositoryMigrator
    {
        private readonly SvnRepositoryProvider repositoryProvider;

        [ImportingConstructor]
        public SvnRepositoryMigrator(SvnRepositoryProvider repositoryProvider)
        {
            this.repositoryProvider = repositoryProvider;
        }

        public IRepositoryProvider RepositoryProvider => this.repositoryProvider;

        public string Name => this.repositoryProvider.Name;

        public string Migrate(string sourcePath)
        {
            this.PrepareBranches(sourcePath);
            this.MoveTagsToBranches(sourcePath);
            this.DeleteUsers(sourcePath);
            this.Pack(sourcePath);
            return null;
        }

        private void Pack(string sourcePath)
        {
            var info = SvnInfo.Run(sourcePath);
            var rootPath = info.RepositoryRoot.LocalPath;
            if (rootPath.EndsWith($"{Path.DirectorySeparatorChar}") == true)
                rootPath = Path.GetDirectoryName(rootPath);
            var packCommand = new SvnAdminCommand("pack") { (SvnPath)rootPath };
            packCommand.Run();
        }

        private void MoveTagsToBranches(string dataBasesPath)
        {
            var dataBaseUrl = new Uri(dataBasesPath);
            var tagsUrl = UriUtility.Combine(dataBaseUrl, SvnString.Tags);
            var branchesUri = UriUtility.Combine(dataBaseUrl, SvnString.Branches);
            var listCommand = new SvnCommand("list") { (SvnPath)tagsUrl };
            var list = listCommand.ReadLines();

            foreach (var item in list)
            {
                if (item.EndsWith(PathUtility.Separator) == true)
                {
                    var name = item.Remove(item.Length - PathUtility.Separator.Length);
                    var sourceUri = UriUtility.Combine(tagsUrl, name);
                    var destUri = UriUtility.Combine(branchesUri, name);
                    //var log = SvnLogInfo.Run(sourceUri.ToString(), null, 1).First();
                    var moveCommand = new SvnCommand("mv")
                    {
                        (SvnPath)sourceUri,
                        (SvnPath)destUri,
                        SvnCommandItem.FromMessage($"Migrate: move {name} from tags to branches"),
                        SvnCommandItem.FromUsername(nameof(SvnRepositoryMigrator)),
                    };
                    moveCommand.Run();
                    //var propText = string.Join(" ", log.Properties.Select(i => $"--with-revprop \"{i.Prefix}{i.Key}={i.Value}\""));
                    //SvnClientHost.Run($"mv \"{sourceUri}\" \"{destUri}\" -m \"Migrate: move {name} from tags to branches\"", propText, $"--username {nameof(SvnRepositoryMigrator)}");
                }
            }
        }

        private void PrepareBranches(string dataBasesPath)
        {
            var dataBaseUrl = new Uri(dataBasesPath);
            var listCommand = new SvnCommand("list") { (SvnPath)dataBaseUrl };
            var list = listCommand.ReadLines();
            if (list.Contains($"{SvnString.Branches}{PathUtility.Separator}") == false)
            {
                var branchesUrl = UriUtility.Combine(dataBaseUrl, SvnString.Branches);
                var mkdirCommand = new SvnCommand("mkdir")
                {
                    (SvnPath)branchesUrl,
                    SvnCommandItem.FromMessage("Migrate: create branches"),
                    SvnCommandItem.FromUsername(nameof(SvnRepositoryMigrator)),
                };
                mkdirCommand.Run();
            }
        }

        private void DeleteUsers(string dataBasesPath)
        {
            var usersUrl = UriUtility.Combine(new Uri(dataBasesPath), "users.xml");
            var deleteCommand = new SvnCommand("rm")
            {
                (SvnPath)usersUrl,
                SvnCommandItem.FromMessage("Migrate: delete users"),
                SvnCommandItem.FromUsername(nameof(SvnRepositoryMigrator)),
            };
            deleteCommand.Run();
        }

        private string[] GetLines(string text)
        {
            var sr = new StringReader(text);
            var lineList = new List<string>();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                lineList.Add(line);
            }
            return lineList.ToArray();
        }
    }
}

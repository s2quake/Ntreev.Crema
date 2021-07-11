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
using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace JSSoft.Crema.Repository.Git
{
    [Export]
    [Export(typeof(IRepositoryProvider))]
    class GitRepositoryProvider : IRepositoryProvider
    {
        public const string KeepExtension = ".keep";

        private static readonly ISerializer propertySerializer = new SerializerBuilder().Build();

        public void CopyRepository(string author, string basePath, string repositoryName, string newRepositoryName, string comment, params LogPropertyInfo[] properties)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;
            var branchCommand = new GitCommand(repositoryPath, "branch")
            {
                newRepositoryName,
                repositoryName,
            };
            branchCommand.Run();
            this.SetID(repositoryPath, newRepositoryName, Guid.NewGuid());
            this.SetDescription(repositoryPath, newRepositoryName, comment);
            this.SetCreationInfo(repositoryPath, newRepositoryName, new SignatureDate(author));
        }

        public IRepository CreateInstance(RepositorySettings settings)
        {
            var baseUri = new Uri(settings.BasePath);
            var workingUri = new Uri(settings.WorkingPath);
            var originUri = UriUtility.MakeRelativeOfDirectory(workingUri, baseUri);
            var repositoryName = settings.RepositoryName == string.Empty ? "master" : settings.RepositoryName;

            if (Directory.Exists(settings.WorkingPath) == false)
            {
                var cloneCommand = new GitCommand(null, "clone")
                {
                    (GitPath)baseUri,
                    new GitCommandItem('b'),
                    repositoryName,
                    (GitPath)settings.WorkingPath,
                    new GitCommandItem("single-branch")
                };
                cloneCommand.Run();

                var fetchCommand = new GitCommand(settings.WorkingPath, "fetch")
                {
                    "origin",
                    "refs/notes/commits:refs/notes/commits",
                };
                fetchCommand.Run();

                var id = this.GetID(settings.BasePath, repositoryName);
                this.SetID(settings.WorkingPath, repositoryName, id);
                GitConfig.SetValue(settings.WorkingPath, "remote.origin.url", originUri);
                var repositoryInfo = this.GetRepositoryInfo(settings.BasePath, repositoryName);
                return new GitRepository(settings.LogService, settings.WorkingPath, settings.TransactionPath, repositoryInfo);
            }
            else
            {
                var repositoryInfo = this.GetRepositoryInfo(settings.WorkingPath, repositoryName);
                return new GitRepository(settings.LogService, settings.WorkingPath, settings.TransactionPath, repositoryInfo);
            }
        }

        public void CreateRepository(string author, string basePath, string initPath, string comment, params LogPropertyInfo[] properties)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;
            var repositoryName = Path.GetFileName(initPath);
            var checkoutCommand = new GitCommand(repositoryPath, "checkout")
            {
                new GitCommandItem("orphan"), repositoryName
            };
            checkoutCommand.Run();
            var removeCommand = new GitCommand(repositoryPath, "rm")
            {
                "-rf", "."
            };
            removeCommand.Run();

            DirectoryUtility.Copy(initPath, repositoryPath);
            foreach (var item in GetEmptyDirectories(repositoryPath))
            {
                File.WriteAllText(Path.Combine(item, KeepExtension), string.Empty);
            }

            var statusItems = GitItemStatusInfo.Run(repositoryPath);
            var addCommand = new GitCommand(repositoryPath, "add");
            foreach (var item in statusItems)
            {
                if (item.Status == RepositoryItemStatus.Untracked)
                {
                    addCommand.Add((GitPath)item.Path);
                }
            }
            addCommand.Run();

            var commitCommand = new GitCommitCommand(repositoryPath, author, comment);
            commitCommand.Run();

            var props = properties.Select(item => (GitPropertyValue)item).ToArray();
            var propText = propertySerializer.Serialize(props);
            var addNotesCommand = new GitCommand(basePath, "notes")
            {
                "add",
                GitCommandItem.FromMessage(propText)
            };
            addNotesCommand.Run();
            this.SetID(repositoryPath, repositoryName, Guid.NewGuid());
            this.SetDescription(repositoryPath, repositoryName, comment);
        }

        public void RenameRepository(string author, string basePath, string repositoryName, string newRepositoryName, string comment, params LogPropertyInfo[] properties)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;
            var renameCommand = new GitCommand(repositoryPath, "branch")
            {
                new GitCommandItem('m'),
                repositoryName,
                newRepositoryName
            };

            var repositoryID = this.GetID(repositoryPath, repositoryName);
            renameCommand.Run();
            this.SetID(repositoryPath, newRepositoryName, repositoryID);
        }

        public void DeleteRepository(string author, string basePath, string repositoryName, string comment, params LogPropertyInfo[] properties)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;
            var branchName = repositoryName;
            var branchCollection = GitBranchCollection.Run(repositoryPath);
            if (branchCollection.Count <= 1)
                throw new InvalidOperationException();

            if (branchCollection.CurrentBranch == branchName)
            {
                var nextBranchName = branchCollection.First(item => item != branchCollection.CurrentBranch);
                this.CheckoutBranch(repositoryPath, nextBranchName);
            }

            var deleteCommand = new GitCommand(repositoryPath, "branch")
            {
                new GitCommandItem('D'),
                branchName
            };
            deleteCommand.Run();
            this.UnsetID(repositoryPath, repositoryName);
        }

        public void RevertRepository(string author, string basePath, string repositoryName, string revision, string comment)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;

            this.CheckoutBranch(repositoryPath, repositoryName);

            try
            {
                var revisionsCommand = new GitCommand(repositoryPath, "log")
                {
                    GitCommandItem.FromPretty("format:%H"),
                };
                var revisions = revisionsCommand.ReadLines();
                if (revisions.Contains(revision) == false)
                {
                    throw new ArgumentException($"'{revision}' is invalid revision.", nameof(revision));
                }
                foreach (var item in revisions)
                {
                    if (item == revision)
                        break;
                    var revertCommand = new GitCommand(repositoryPath, "revert")
                    {
                        new GitCommandItem('n'),
                        item,
                    };
                    revertCommand.Run();
                }
                var statusCommand = new GitCommand(repositoryPath, "status")
                {
                    new GitCommandItem('s')
                };
                var items = statusCommand.ReadLines(true);
                if (items.Length != 0)
                {
                    var commitCommand = new GitCommitCommand(basePath, author, comment);
                    commitCommand.Run();
                }
                else
                {
                    throw new InvalidOperationException("nothing to revert.");
                }
            }
            catch
            {
                var abortCommand = new GitCommand(repositoryPath, "revert")
                {
                    new GitCommandItem("abort"),
                };
                abortCommand.TryRun();
                throw;
            }
        }

        public LogInfo[] GetLog(string basePath, string repositoryName, string revision)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;
            var logs = GitLogInfo.GetRepositoryLogs(repositoryPath, repositoryName, revision);
            return logs.Select(item => (LogInfo)item).ToArray();
        }

        public string[] GetRepositories(string basePath)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;
            var branchCommand = new GitCommand(repositoryPath, "branch")
            {
                new GitCommandItem("list")
            };
            var lines = branchCommand.ReadLines();
            var itemList = new List<string>(lines.Length);
            foreach (var line in lines)
            {
                var match = Regex.Match(line, "^[*]*\\s*(\\S+)");
                if (match.Success)
                {
                    var branchName = match.Groups[1].Value;
                    itemList.Add(branchName);
                }
            }

            return itemList.ToArray();
        }

        public RepositoryInfo GetRepositoryInfo(string basePath, string repositoryName)
        {
            var baseUri = new Uri(basePath);
            var repositoryInfo = new RepositoryInfo();
            var repositoryPath = baseUri.LocalPath;
            var revisions = GitLogInfo.GetRepositoryRevisions(repositoryPath, repositoryName);
            var latestLog = GitLogInfo.GetRepositoryLatestLog(repositoryPath, revisions.First());
            var refLogItems = GitLogInfo.GetReflogs(basePath, repositoryName);

            var firstLog = refLogItems.Last();

            repositoryInfo.Name = repositoryName;
            repositoryInfo.Revision = latestLog.CommitID;
            repositoryInfo.CreationInfo = new SignatureDate(firstLog.Author, firstLog.AuthorDate);
            repositoryInfo.ModificationInfo = new SignatureDate(latestLog.Author, latestLog.AuthorDate);

            if (this.HasID(repositoryPath, repositoryName) == false)
            {
                this.SetID(repositoryPath, repositoryName, Guid.NewGuid());
            }
            repositoryInfo.ID = this.GetID(repositoryPath, repositoryName);

            if (this.HasDescription(repositoryPath, repositoryName) == true)
            {
                repositoryInfo.Comment = this.GetDescription(repositoryPath, repositoryName);
            }
            else
            {
                repositoryInfo.Comment = string.Empty;
            }

            if (this.HasCreationInfo(repositoryPath, repositoryName) == true)
            {
                repositoryInfo.CreationInfo = this.GetCreationInfo(repositoryPath, repositoryName);
            }

            return repositoryInfo;
        }

        public string[] GetRepositoryItemList(string basePath, string repositoryName)
        {
            var baseUri = new Uri(basePath);
            var repositoryPath = baseUri.LocalPath;
            var listCommand = new GitCommand(repositoryPath, "ls-tree")
            {
                new GitCommandItem('r'),
                new GitCommandItem("name-only"),
                repositoryName
            };
            var lines = listCommand.ReadLines(true);
            var itemList = new List<string>(lines.Length);

            foreach (var item in lines)
            {
                if (item.EndsWith(KeepExtension) == true)
                {
                    itemList.Add(PathUtility.Separator + item.Substring(0, item.Length - KeepExtension.Length));
                }
                else
                {
                    itemList.Add(PathUtility.Separator + item);
                }
            }
            return itemList.ToArray();
        }

        public string GetRevision(string basePath, string repositoryName)
        {
            var revparseCommand = new GitCommand(basePath, "rev-parse")
            {
                repositoryName,
            };
            return revparseCommand.ReadLine();
        }

        public void InitializeRepository(string basePath, string repositoryPath, params LogPropertyInfo[] properties)
        {
            var cloneCommand = new GitCommand(null, "clone")
            {
                (GitPath)repositoryPath,
                (GitPath)basePath,
            };

            if (cloneCommand.TryRun() == true)
            {
                var fetchCommand = new GitCommand(basePath, "fetch")
                {
                    "origin",
                    "refs/notes/commits:refs/notes/commits",
                };
                fetchCommand.Run();
                GitConfig.SetValue(basePath, "receive.denyCurrentBranch", "ignore");
                return;
            }

            var initCommand = new GitCommand(null, "init")
            {
                (GitPath)basePath
            };
            initCommand.Run();

            var configCommand = new GitCommand(basePath, "config")
            {
                "receive.denyCurrentBranch",
                "ignore"
            };
            configCommand.Run();

            DirectoryUtility.Copy(repositoryPath, basePath);
            foreach (var item in GetEmptyDirectories(basePath))
            {
                File.WriteAllText(Path.Combine(item, KeepExtension), string.Empty);
            }

            var query = from item in DirectoryUtility.GetAllFiles(basePath, "*", true)
                        select (GitPath)PathUtility.GetFullPath(item);
            var itemList = query.ToList();

            var addCommand = new GitAddCommand(basePath, query.ToArray());
            addCommand.Run();

            var commitCommand = new GitCommitCommand(basePath, Environment.UserName, "first commit");
            commitCommand.Run();

            var props = properties.Select(item => (GitPropertyValue)item).ToArray();
            var propText = propertySerializer.Serialize(props);
            var addNotesCommand = new GitCommand(basePath, "notes")
            {
                "add",
                GitCommandItem.FromMessage(propText)
            };
            addNotesCommand.Run();

            this.SetID(basePath, "master", Guid.NewGuid());
        }

        public string Name => "git";

        private void SetID(string repositoryPath, string repositoryName, Guid guid)
        {
            GitConfig.SetValue(repositoryPath, $"branch.{repositoryName}.id", $"{guid}");
        }

        private void UnsetID(string repositoryPath, string repositoryName)
        {
            GitConfig.UnsetValue(repositoryPath, $"branch.{repositoryName}.id");
        }

        private bool HasID(string repositoryPath, string repositoryName)
        {
            return GitConfig.HasValue(repositoryPath, $"branch.{repositoryName}.id");
        }

        private Guid GetID(string repositoryPath, string repositoryName)
        {
            return Guid.Parse(GitConfig.GetValue(repositoryPath, $"branch.{repositoryName}.id"));
        }

        private void SetCreationInfo(string repositoryPath, string repositoryName, SignatureDate signatureDate)
        {
            GitConfig.SetValue(repositoryPath, $"branch.{repositoryName}.createdDateTime", $"{signatureDate.ToString(CultureInfo.GetCultureInfo("en-US"))}");
        }

        private bool HasCreationInfo(string repositoryPath, string repositoryName)
        {
            return GitConfig.HasValue(repositoryPath, $"branch.{repositoryName}.createdDateTime");
        }

        private SignatureDate GetCreationInfo(string repositoryPath, string repositoryName)
        {
            return SignatureDate.Parse(GitConfig.GetValue(repositoryPath, $"branch.{repositoryName}.createdDateTime"), CultureInfo.GetCultureInfo("en-US"));
        }

        private void SetDescription(string repositoryPath, string repositoryName, string description)
        {
            GitConfig.SetValue(repositoryPath, $"branch.{repositoryName}.description", $"{description}");
        }

        private bool HasDescription(string repositoryPath, string repositoryName)
        {
            return GitConfig.HasValue(repositoryPath, $"branch.{repositoryName}.description");
        }

        private string GetDescription(string repositoryPath, string repositoryName)
        {
            return GitConfig.GetValue(repositoryPath, $"branch.{repositoryName}.description");
        }

        private string[] GetEmptyDirectories(string path)
        {
            var items = DirectoryUtility.GetAllDirectories(path, "*", true);
            var itemList = new List<string>(items.Length);
            foreach (var item in items)
            {
                if (Directory.GetFiles(item).Length == 0)
                {
                    itemList.Add(item);
                }
            }
            return itemList.ToArray();
        }

        private void CheckoutBranch(string repositoryPath, string branchName)
        {
            var resetCommand = new GitCommand(repositoryPath, "reset")
            {
                new GitCommandItem("hard")
            };
            resetCommand.Run();
            var checkoutCommand = new GitCommand(repositoryPath, "checkout")
            {
                branchName
            };
            checkoutCommand.Run();
        }
    }
}

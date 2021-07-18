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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace JSSoft.Crema.Repository.Git
{
    class GitRepository : IRepository
    {
        private static readonly ISerializer propertySerializer = new SerializerBuilder().Build();
        private readonly GitRepositoryProvider repositoryProvider;
        private readonly RepositorySettings settings;
        private readonly ILogService logService;
        private readonly GitCommand resetCommand;
        private readonly GitCommand cleanCommand;
        private string transactionAuthor;
        private string transactionName;
        private List<string> transactionMessageList;
        private List<LogPropertyInfo> transactionPropertyList;
        private string transactionPatchPath;
        private RepositoryInfo repositoryInfo;

        public GitRepository(GitRepositoryProvider repositoryProvider, RepositorySettings settings, RepositoryInfo repositoryInfo)
        {
            this.repositoryProvider = repositoryProvider;
            this.settings = settings;
            this.logService = settings.LogService;
            this.repositoryInfo = repositoryInfo;
            this.resetCommand = new GitCommand(this.BasePath, "reset")
            {
                new GitCommandItem("hard")
            };
            this.cleanCommand = new GitCommand(this.BasePath, "clean")
            {
                new GitCommandItem('f'),
                new GitCommandItem('d')
            };
            var statusCommand = new GitCommand(this.BasePath, "status")
            {
                new GitCommandItem('s'),
            };
            var items = statusCommand.ReadLines(true);
            if (items.Length != 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Repository is dirty. Please fix the problem before running the service.");
                sb.AppendLine();
                foreach (var item in items)
                {
                    sb.AppendLine(item);
                }
                throw new Exception($"{sb}");
            }
        }

        public RepositoryInfo RepositoryInfo => this.repositoryInfo;

        public string BasePath => this.settings.BasePath;

        public void Add(string path)
        {
            var addCommand = new GitCommand(this.BasePath, "add");

            if (DirectoryUtility.IsDirectory(path) == true)
            {
                var keepPath = Path.Combine(path, GitRepositoryProvider.KeepExtension);
                if (File.Exists(keepPath) == false)
                {
                    File.WriteAllText(keepPath, string.Empty);
                    addCommand.Add((GitPath)keepPath);
                }
            }
            else
            {
                addCommand.Add((GitPath)path);
            }

            if (addCommand.Items.Any() == true)
                addCommand.Run(this.logService);
        }

        public void BeginTransaction(string author, string name)
        {
            this.transactionAuthor = author;
            this.transactionName = name;
            this.transactionMessageList = new List<string>();
            this.transactionPropertyList = new List<LogPropertyInfo>();
            this.transactionPatchPath = Path.Combine(this.settings.TransactionPath, this.transactionName + ".patch");
        }

        public void EndTransaction()
        {
            var transactionMessage = string.Join(Environment.NewLine, this.transactionMessageList);
            var statusCommand = new GitCommand(this.BasePath, "status")
            {
                new GitCommandItem('s')
            };
            var items = statusCommand.ReadLines(true);
            if (items.Length != 0)
            {
                var commitCommand = new GitCommitCommand(this.BasePath, this.transactionAuthor, transactionMessage);
                var result = commitCommand.Run(this.logService);
                this.logService?.Debug(result);
                var log = GitLogInfo.GetLatestLog(this.BasePath);
                this.repositoryInfo.Revision = log.CommitID;
                this.repositoryInfo.ModificationInfo = new SignatureDate(this.transactionAuthor, log.CommitDate);
                this.SetNotes(this.transactionPropertyList.ToArray());
                FileUtility.Delete(this.transactionPatchPath);
                this.transactionAuthor = null;
                this.transactionName = null;
                this.transactionMessageList = null;
                this.transactionPropertyList = null;
                this.transactionPatchPath = null;
                this.Pull();
                this.Push();
                this.PushNotes();
            }
            else
            {
                this.logService?.Debug("repository has no changes.");
            }
        }

        public void CancelTransaction()
        {
            this.resetCommand.Run(this.logService);
            this.cleanCommand.Run(this.logService);
            FileUtility.Delete(this.transactionPatchPath);
            this.transactionAuthor = null;
            this.transactionName = null;
            this.transactionMessageList = null;
            this.transactionPropertyList = null;
            this.transactionPatchPath = null;
        }

        public void Clone(string author, string newRepositoryName, string comment, string revision, params LogPropertyInfo[] properties)
        {
            var repositoryName = this.settings.RepositoryName;
            var basePath = this.settings.RemotePath;
            this.Sync();
            this.repositoryProvider.CloneRepository(author, basePath, repositoryName, newRepositoryName, comment, revision, properties);
        }

        public void Commit(string author, string comment, params LogPropertyInfo[] properties)
        {
            if (this.transactionName != null)
            {
                var diffCommand = new GitCommand(this.BasePath, "diff")
                {
                    "HEAD",
                    new GitCommandItem("stat"),
                    new GitCommandItem("binary")
                };
                var output = diffCommand.ReadLine();
                FileUtility.Prepare(this.transactionPatchPath);
                File.WriteAllText(this.transactionPatchPath, output);
                this.transactionMessageList.Add(comment);
                this.transactionPropertyList.AddRange(properties);
                return;
            }

            try
            {
                var statusCommand = new GitCommand(this.BasePath, "status")
                {
                    new GitCommandItem('s')
                };
                var items = statusCommand.ReadLines(true);
                if (items.Length != 0)
                {
                    var authorValue = new GitAuthor(author);
                    GitConfig.SetValue(this.BasePath, "user.email", authorValue.Email == string.Empty ? "<>" : authorValue.Email);
                    GitConfig.SetValue(this.BasePath, "user.name", authorValue.Name);

                    var commitCommand = new GitCommitCommand(this.BasePath, author, comment);
                    var result = commitCommand.Run(this.logService);
                    this.logService?.Debug(result);
                    var log = GitLogInfo.GetLatestLog(this.BasePath);
                    this.repositoryInfo.Revision = log.CommitID;
                    this.repositoryInfo.ModificationInfo = new SignatureDate(author, log.CommitDate);

                    this.SetNotes(properties);
                    //this.isModified = true;
                    //this.Pull();
                    //this.Push();
                    //this.PushNotes();
                }
                else
                {
                    this.logService?.Debug("repository no changes. \"{0}\"", this.BasePath);
                }
            }
            catch (Exception e)
            {
                this.logService?.Warn(e);
                throw;
            }
        }

        public void Copy(string srcPath, string toPath)
        {
            if (DirectoryUtility.IsDirectory(srcPath) == true)
            {
                throw new NotImplementedException();
            }
            else
            {
                var copyCommand = new GitCommand(this.BasePath, "add")
                {
                    (GitPath)toPath
                };
                File.Copy(srcPath, toPath);
                copyCommand.Run(this.logService);
            }
        }

        public void Delete(string path)
        {
            var removeCommand = new GitCommand(this.BasePath, "rm")
            {
                (GitPath)path,
            };
            if (Directory.Exists(path) == true)
            {
                removeCommand.Add(new GitCommandItem('r'));
            }
            removeCommand.Run(this.logService);
        }

        public void Dispose()
        {
            this.Sync();
        }

        public string Export(Uri uri, string exportPath)
        {
            var match = Regex.Match(uri.LocalPath, "(?<path>.+)@(?<keep>.*)(?<revision>[a-f0-9]{40})", RegexOptions.ExplicitCapture);
            var path = match.Groups["path"].Value;
            var keep = match.Groups["keep"].Value;
            var revision = match.Groups["revision"].Value;

            var tempPath = PathUtility.GetTempFileName();
            try
            {
                if (Directory.Exists(exportPath) == false)
                    Directory.CreateDirectory(exportPath);
                if (DirectoryUtility.IsEmpty(exportPath) == true)
                    new CremaDataSet().WriteToDirectory(exportPath);
                var relativePath = UriUtility.MakeRelativeOfDirectory(this.BasePath, path);
                var archiveCommand = new GitCommand(this.BasePath, "archive")
                {
                    new GitCommandItem($"output={(GitPath)tempPath}"),
                    new GitCommandItem("format=zip"),
                    revision,
                    GitCommandItem.Separator,
                    (GitPath)path,
                };
                archiveCommand.Run(this.logService);
                ZipFile.ExtractToDirectory(tempPath, exportPath);
                var exportUri = new Uri(UriUtility.Combine(exportPath, relativePath));
                return exportUri.LocalPath;
            }
            finally
            {
                FileUtility.Delete(tempPath);
            }
        }

        public LogInfo[] GetLog(string[] paths, string revision)
        {
            var logs = GitLogInfo.GetLogs(this.BasePath, revision, paths);
            return logs.Select(item => (LogInfo)item).ToArray();
        }

        public Uri GetUri(string path, string revision)
        {
            var revisionValue = revision == string.Empty ? this.repositoryInfo.Revision : revision;
            if (DirectoryUtility.IsDirectory(path) == true)
            {
                var uri = new Uri($"{path}@{revisionValue}");
                var uriString = uri.ToString();
                var text = Regex.Replace(uriString, "file:///", "dir:///");
                return new Uri(text);
            }
            return new Uri($"{path}@{revisionValue}");
        }

        public void Move(string srcPath, string toPath)
        {
            var moveCommand = new GitCommand(this.BasePath, "mv")
            {
                (GitPath)srcPath,
                (GitPath)toPath,
            };
            moveCommand.Run(this.logService);
        }

        public void Revert()
        {
            this.resetCommand.Run(this.logService);
            this.cleanCommand.Run(this.logService);

            if (File.Exists(this.transactionPatchPath) == true)
            {
                var applyCommand = new GitCommand(this.BasePath, "apply")
                {
                    (GitPath)transactionPatchPath,
                    new GitCommandItem("index")
                };
                applyCommand.Run(this.logService);
            }
        }

        public RepositoryItem[] Status(params string[] paths)
        {
            var items = GitItemStatusInfo.Run(this.BasePath, paths);
            var itemList = new List<RepositoryItem>(items.Length);
            foreach (var item in items)
            {
                var repositoryItem = new RepositoryItem()
                {
                    Path = new Uri(UriUtility.Combine(this.BasePath, item.Path)).LocalPath,
                    OldPath = new Uri(UriUtility.Combine(this.BasePath, item.OldPath)).LocalPath,
                    Status = item.Status,
                };

                itemList.Add(repositoryItem);
            }
            return itemList.ToArray();
        }

        private void Sync()
        {
            this.Pull();
            this.PullNotes();
            this.Push();
            try
            {
                this.PushNotes();
            }
            catch (Exception e)
            {
                this.logService.Error(e);
            }
        }

        private void Pull()
        {
            var pullCommand = new GitCommand(this.BasePath, "pull");
            pullCommand.Run(this.logService);
        }

        private void Push()
        {
            var pushCommand = new GitCommand(this.BasePath, "push");
            pushCommand.Run(this.logService);
        }

        private void PullNotes()
        {
            var fetchCommand = new GitCommand(this.BasePath, "fetch")
            {
                "origin",
                "refs/notes/commits:refs/notes/origin/commits",
            };
            fetchCommand.Run();

            var mergeCommand = new GitCommand(this.BasePath, "notes")
            {
                "merge",
                new GitCommandItem('v'),
                "origin/commits",
            };
            mergeCommand.Run();
        }

        private void SetNotes(params LogPropertyInfo[] properties)
        {
            var props = properties.Select(item => (GitPropertyValue)item).ToArray();
            var propText = propertySerializer.Serialize(props);
            var notesCommand = new GitCommand(this.BasePath, "notes")
            {
                "add",
                GitCommandItem.FromMessage(propText),
            };
            notesCommand.Run(this.logService);
        }

        private void PushNotes()
        {
            var pushNotesCommand = new GitCommand(this.BasePath, "push")
            {
                "origin",
                "refs/notes/commits",
            };
            pushNotesCommand.Run(this.logService);
        }
    }
}

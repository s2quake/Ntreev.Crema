﻿// Released under the MIT License.
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JSSoft.Crema.Repository.Svn
{
    class SvnRepository : IRepository
    {
        private readonly string transactionPath;
        private readonly ILogService logService;
        private readonly SvnCommand revertCommand;
        private readonly SvnCommand cleanupCommand;
        private readonly SvnCommand statCommand;
        private string transactionAuthor;
        private string transactionName;
        private List<string> transactionMessageList;
        private List<LogPropertyInfo> transactionPropertyList;
        private string transactionPatchPath;
        private bool needToUpdate;
        private readonly Uri repositoryUri;
        private RepositoryInfo repositoryInfo;
        private SvnInfo info;

        public SvnRepository(ILogService logService, string repositoryPath, string transactionPath, RepositoryInfo repositoryInfo)
        {
            this.logService = logService;
            this.BasePath = repositoryPath;
            this.transactionPath = transactionPath;
            this.repositoryInfo = repositoryInfo;
            this.revertCommand = new SvnCommand("revert")
            {
                (SvnPath)this.BasePath,
                SvnCommandItem.Recursive,
            };

            this.cleanupCommand = new SvnCommand("cleanup") { (SvnPath)this.BasePath };

            this.statCommand = new SvnCommand("stat")
            {
                (SvnPath)this.BasePath,
                SvnCommandItem.Quiet
            };
            var items = this.statCommand.ReadLines(true);
            if (items.Length != 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Repository is dirty. Please fix the problem before running the service.");
                sb.AppendLine();
                sb.AppendLine(string.Join(Environment.NewLine, items));
                throw new Exception($"{sb}");
            }

            this.info = SvnInfo.Run(this.BasePath);
            this.RepositoryRoot = this.info.RepositoryRoot;
            this.repositoryUri = this.info.Uri;
        }

        public string Name => "svn";

        public RepositoryInfo RepositoryInfo => this.repositoryInfo;

        public string BasePath { get; }

        public Uri RepositoryRoot { get; }

        public void Add(string path)
        {
            var addCommand = new SvnCommand("add")
            {
                new SvnCommandItem("depth", "files"),
                (SvnPath)path,
            };
            addCommand.Run(this.logService);
        }

        public void BeginTransaction(string author, string name)
        {
            this.transactionAuthor = author;
            this.transactionName = name;
            this.transactionMessageList = new List<string>();
            this.transactionPropertyList = new List<LogPropertyInfo>();
            this.transactionPatchPath = Path.Combine(this.transactionPath, this.transactionName + ".patch");
        }

        public void EndTransaction()
        {
            var transactionMessage = string.Join(Environment.NewLine, this.transactionMessageList);
            var messagePath = FileUtility.WriteAllText(transactionMessage, Encoding.UTF8, PathUtility.GetTempFileName());
            try
            {
                var items = this.statCommand.ReadLines(true);
                if (items.Length != 0)
                {
                    var propText = SvnRepositoryProvider.GeneratePropertiesArgument(this.transactionPropertyList.ToArray());
                    var updateCommand = new SvnCommand("update") { (SvnPath)this.BasePath };
                    var commitCommand = new SvnCommand("commit")
                    {
                        (SvnPath)this.BasePath,
                        SvnCommandItem.FromFile(messagePath),
                        propText,
                        SvnCommandItem.FromEncoding(Encoding.UTF8),
                        SvnCommandItem.FromUsername(this.transactionAuthor),
                    };
                    updateCommand.Run(this.logService);
                    commitCommand.Run(this.logService);
                    FileUtility.Delete(this.transactionPatchPath);
                    this.transactionAuthor = null;
                    this.transactionName = null;
                    this.transactionMessageList = null;
                    this.transactionPropertyList = null;
                    this.transactionPatchPath = null;
                }
                else
                {
                    this.logService?.Debug("repository has no changes.");
                }
            }
            finally
            {
                FileUtility.Delete(messagePath);
            }
        }

        public void CancelTransaction()
        {
            this.revertCommand.Run(this.logService);
            FileUtility.Delete(this.transactionPatchPath);
            this.transactionAuthor = null;
            this.transactionName = null;
            this.transactionMessageList = null;
            this.transactionPropertyList = null;
            this.transactionPatchPath = null;
        }

        public void Commit(string author, string comment, params LogPropertyInfo[] properties)
        {
            if (this.transactionName != null)
            {
                var diffCommand = new SvnCommand("diff")
                {
                    (SvnPath)this.BasePath,
                    new SvnCommandItem("patch-compatible")
                };
                var output = diffCommand.ReadLine();
                File.WriteAllText(this.transactionPatchPath, output);
                this.transactionMessageList.Add(comment);
                this.transactionPropertyList.AddRange(properties);
                return;
            }

            this.logService?.Debug($"repository committing {(SvnPath)this.BasePath}");
            var result = string.Empty;
            var commentPath = PathUtility.GetTempFileName();
            var propText = SvnRepositoryProvider.GeneratePropertiesArgument(properties);
            var updateCommand = new SvnCommand("update") { (SvnPath)this.BasePath };
            var commitCommand = new SvnCommand("commit")
            {
                (SvnPath)this.BasePath,
                SvnCommandItem.FromMessage(comment),
                propText,
                SvnCommandItem.FromEncoding(Encoding.UTF8),
                SvnCommandItem.FromUsername(author),
            };

            try
            {
                if (this.needToUpdate == true)
                {
                    updateCommand.Run(this.logService);
                }

                result = commitCommand.Run(this.logService);
            }
            catch (Exception e)
            {
                this.logService?.Warn(e);
                updateCommand.Run(this.logService);
                result = commitCommand.Run(this.logService);
            }
            finally
            {
                this.needToUpdate = false;
                FileUtility.Delete(commentPath);
            }

            if (result.Trim() != string.Empty)
            {
                this.logService?.Debug(result);
                this.logService?.Debug($"repository committed {(SvnPath)this.BasePath}");
                this.info = SvnInfo.Run(this.BasePath);
                this.repositoryInfo.Revision = this.info.LastChangedRevision;
                this.repositoryInfo.ModificationInfo = new SignatureDate(this.info.LastChangedAuthor, this.info.LastChangedDate);
            }
            else
            {
                this.logService?.Debug("repository no changes. \"{0}\"", this.BasePath);
            }
        }

        public void Copy(string srcPath, string toPath)
        {
            var copyCommand = new SvnCommand("copy")
            {
                (SvnPath)srcPath,
                (SvnPath)toPath
            };
            copyCommand.Run(this.logService);
        }

        public void Delete(string path)
        {
            var deleteCommand = new SvnCommand("delete")
            {
                (SvnPath)path,
                SvnCommandItem.Force,
            };

            if (DirectoryUtility.IsDirectory(path) == true)
            {
                var updateCommand = new SvnCommand("update") { (SvnPath)path };
                updateCommand.Run(this.logService);
            }

            deleteCommand.Run(this.logService);
        }

        public string Export(Uri uri, string exportPath)
        {
            var pureUri = new Uri(Regex.Replace($"{uri}", "@\\d+$", string.Empty));
            var relativeUri = UriUtility.MakeRelativeOfDirectory(this.repositoryUri, pureUri);
            var uriTarget = uri.LocalPath;
            var filename = FileUtility.Prepare(exportPath, $"{relativeUri}");
            var exportCommand = new SvnCommand("export") { (SvnPath)uri, (SvnPath)filename };
            var result = exportCommand.Run(this.logService);
            return new FileInfo(Path.Combine(exportPath, $"{relativeUri}")).FullName;
        }

        public LogInfo[] GetLog(string[] paths, string revision)
        {
            var logs = SvnLogInfo.GetLogs(paths, revision);
            return logs.Select(item => (LogInfo)item).ToArray();
        }

        public string GetRevision(string path)
        {
            var info = SvnInfo.Run(path);
            var repositoryInfo = SvnInfo.Run($"{info.Uri}");
            return repositoryInfo.LastChangedRevision;
        }

        public Uri GetUri(string path, string revision)
        {
            var revisionValue = revision ?? this.repositoryInfo.Revision;
            var info = SvnInfo.Run(path, revisionValue);
            return new Uri($"{info.Uri}@{revisionValue}");
        }

        public RepositoryItem[] Status(params string[] paths)
        {
            var args = SvnStatusInfo.Run(paths);
            var itemList = new List<RepositoryItem>(args.Length);
            foreach (var item in args)
            {
                itemList.Add(new RepositoryItem()
                {
                    Path = item.Path,
                    OldPath = item.OldPath,
                    Status = item.Status,
                });
            }
            return itemList.ToArray();
        }

        public void Move(string srcPath, string toPath)
        {
            var moveCommand = new SvnCommand("move")
            {
                (SvnPath)srcPath,
                (SvnPath)toPath,
            };

            if (DirectoryUtility.IsDirectory(srcPath) == true)
            {
                var updateCommand = new SvnCommand("update") { (SvnPath)srcPath };
                updateCommand.Run(this.logService);
            }

            moveCommand.Run(this.logService);
        }

        public void Revert()
        {
            this.revertCommand.Run(this.logService);
            this.cleanupCommand.Run(this.logService);

            if (File.Exists(this.transactionPatchPath) == true)
            {
                var patchCommand = new SvnCommand("patch")
                {
                    (SvnPath)this.transactionPatchPath,
                    (SvnPath)this.BasePath,
                };
                patchCommand.Run(this.logService);
            }
        }

        public void Dispose()
        {
            DirectoryUtility.Delete(this.BasePath);
        }

        public void Clone(string author, string newRepositoryName, string comment, string revision, params LogPropertyInfo[] properties)
        {
            throw new NotImplementedException();
        }
    }
}

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

using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JSSoft.Crema.Services
{
    class RepositoryMigrator
    {
        private const string nameString = "svn";

        private readonly string basePath;
        private readonly string repositoryPath;
        private readonly Uri sourceUrl;
        private readonly string sourceRelativeUrl;
        private readonly Uri sourceRootUrl;
        private readonly IRepositoryMigrator repositoryMigrator;
        private readonly LogService logService;

        private RepositoryMigrator(LogService logService, IRepositoryMigrator repositoryMigrator, string basePath, Uri repositoryUrl)
        {
            this.logService = logService;
            this.repositoryMigrator = repositoryMigrator;
            this.basePath = basePath;
            this.repositoryPath = DirectoryUtility.Prepare(basePath, CremaString.Repository);
            if (repositoryUrl == null)
            {
                this.sourceUrl = new Uri(Path.Combine(this.basePath, nameString));
            }
            else if (repositoryUrl.IsAbsoluteUri)
            {
                this.sourceUrl = repositoryUrl;
            }
            else
            {
                this.sourceUrl = UriUtility.Combine(new Uri(this.basePath), nameString, repositoryUrl.ToString());
            }

            this.sourceRootUrl = new Uri(this.Run($"info \"{this.sourceUrl}\" --show-item repos-root-url").Trim());
            this.sourceRelativeUrl = UriUtility.MakeRelativeString(this.sourceRootUrl, this.sourceUrl);
        }

        public static void Migrate(IServiceProvider serviceProvider, string basePath, string migrationModule, string repositoryUrl, bool force)
        {
            Validate(serviceProvider, basePath, migrationModule, repositoryUrl, force);

            var repositoryMigrator = CremaBootstrapper.GetRepositoryMigrator(serviceProvider, migrationModule ?? "svn");
            var logService = new LogService("migrate", basePath) { LogLevel = LogLevel.Info };
            var repositoryPath = Path.Combine(basePath, CremaString.Repository);
            var repositoryExisted = Directory.Exists(repositoryPath);
            try
            {
                if (repositoryExisted == true)
                {
                    DirectoryUtility.Backup(repositoryPath);
                }
                var url = repositoryUrl == null ? null : new Uri(repositoryUrl, UriKind.RelativeOrAbsolute);
                var migrator = new RepositoryMigrator(logService, repositoryMigrator, basePath, url);
                migrator.Migrate();
                FileUtility.WriteAllText(Resources.Text_README, basePath, "README.md");
            }
            catch (Exception e)
            {
                logService.Error(e);
                throw;
            }
            finally
            {
                if (repositoryExisted == true)
                {
                    DirectoryUtility.Clean(repositoryPath);
                }
            }
        }

        private static void Validate(IServiceProvider serviceProvider, string basePath, string migrationModule, string repositoryUrl, bool force)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));
            if (Directory.Exists(basePath) == false)
                throw new DirectoryNotFoundException($"not found directory: \'{basePath}\'");

            var repositoryPath = Path.Combine(basePath, CremaString.Repository);
            if (Directory.Exists(repositoryPath) == true && force == false)
                throw new InvalidOperationException($"path is existed: \'{repositoryPath}\'");
        }

        private void Migrate()
        {
            try
            {
                this.MigrateUsers();
                this.MigrateDataBases();
                this.WriteRepositoryInfo();
            }
            catch (Exception e)
            {
                this.logService.Error(e);
                DirectoryUtility.SetVisible(this.repositoryPath, true);
                DirectoryUtility.Delete(this.repositoryPath);
                throw e;
            }
        }

        private void MigrateUsers()
        {
            this.logService.Info(nameof(MigrateUsers));
            var repositoryProvider = this.repositoryMigrator.RepositoryProvider;
            var usersPath = Path.Combine(this.repositoryPath, CremaString.Users);
            var userUrl = UriUtility.Combine(this.sourceUrl, $"{CremaString.Users}.xml");
            var userPath = Path.Combine(this.basePath, $"{CremaString.Users}.xml");

            this.logService.Info($" - export {CremaString.Users}.xml");
            this.Run($"export \"{userUrl}\" \"{this.basePath}\" --force");

            var userContext = JSSoft.Library.Serialization.DataContractSerializerUtility.Read<UserContextSerializationInfo>(userPath);
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                this.logService.Info($" - write users information");
                userContext.WriteToDirectory(tempPath);
                this.logService.Info($" - initialize users repository");
                repositoryProvider.InitializeRepository(usersPath, tempPath);
            }
            finally
            {
                this.logService.Info($" - delete temp files");
                FileUtility.Delete(userPath);
                DirectoryUtility.Delete(tempPath);
            }
        }

        private void MigrateDataBases()
        {
            this.logService.Info(nameof(MigrateDataBases));
            var dataBasesPath = Path.Combine(this.repositoryPath, CremaString.DataBases);
            this.logService.Info($" - copy databases repository");
            DirectoryUtility.Copy(this.sourceRootUrl.LocalPath, dataBasesPath);

            this.logService.Info($" - migrate databases repository");
            var dataBaseUrl = this.sourceRelativeUrl == string.Empty ? new Uri(dataBasesPath) : UriUtility.Combine(new Uri(dataBasesPath), this.sourceRelativeUrl);
            var destPath = this.repositoryMigrator.Migrate(dataBaseUrl.ToString());
            if (destPath != null)
            {
                DirectoryUtility.Backup(dataBasesPath);
                DirectoryUtility.Copy(destPath, dataBasesPath);
                DirectoryUtility.Delete(destPath);
                DirectoryUtility.Clean(dataBasesPath);
            }
        }

        private void WriteRepositoryInfo()
        {
            this.logService.Info(nameof(WriteRepositoryInfo));
            var repositoryProvider = this.repositoryMigrator.RepositoryProvider;
            var repoModulePath = FileUtility.WriteAllText(repositoryProvider.Name, this.repositoryPath, CremaString.Repo);
            var fileTypePath = FileUtility.WriteAllText("xml", this.repositoryPath, CremaString.File);

            FileUtility.SetReadOnly(repoModulePath, true);
            FileUtility.SetReadOnly(fileTypePath, true);
            DirectoryUtility.SetVisible(this.repositoryPath, false);

            if (this.sourceRelativeUrl != string.Empty)
            {
                FileUtility.WriteAllText(this.sourceRelativeUrl, this.repositoryPath, "databasesUrl");
            }
        }

        private string Run(params object[] args)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = "svn";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = string.Join(" ", args);
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.OutputDataReceived += (s, e) =>
            {
                outputBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                errorBuilder.AppendLine(e.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception(errorBuilder.ToString());

            return outputBuilder.ToString();
        }
    }
}

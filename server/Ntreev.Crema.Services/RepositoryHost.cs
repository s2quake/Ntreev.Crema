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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    abstract class RepositoryHost
    {
        private readonly HashSet<string> paths = new HashSet<string>();

        public RepositoryHost(IRepository repository)
        {
            this.Repository = repository;
            this.Dispatcher = new CremaDispatcher(this);
            this.RepositoryPath = repository.BasePath;
        }

        public void Add(string path)
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.Add(path);
        }

        public void AddRange(string[] paths)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in paths)
            {
                this.Repository.Add(item);
            }
        }

        public void Add(string path, string contents)
        {
            this.Dispatcher.VerifyAccess();
            File.WriteAllText(path, contents, Encoding.UTF8);
            this.Repository.Add(path);
        }

        public void Modify(string path, string contents)
        {
            this.Dispatcher.VerifyAccess();
            File.WriteAllText(path, contents, Encoding.UTF8);
        }

        public void Move(string srcPath, string toPath)
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.Move(srcPath, toPath);
        }

        public void Delete(string path)
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.Delete(path);
        }

        public void DeleteRange(string[] paths)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in paths)
            {
                this.Repository.Delete(item);
            }
        }

        public void Copy(string srcPath, string toPath)
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.Copy(srcPath, toPath);
        }

        public void Revert()
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.Revert();
        }

        public void BeginTransaction(string author, string name)
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.BeginTransaction(author, name);
        }

        public void EndTransaction()
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.EndTransaction();
        }

        public virtual void CancelTransaction()
        {
            this.Dispatcher.VerifyAccess();
            this.Repository.CancelTransaction();
            this.paths.Clear();
        }

        public Uri GetUri(string path, string revision)
        {
            return this.Repository.GetUri(path, revision);
        }

        public string Export(Uri uri, string exportPath)
        {
            return this.Repository.Export(uri, exportPath);
        }

        public void Commit(Authentication authentication, string comment, params LogPropertyInfo[] properties)
        {
            this.Dispatcher.VerifyAccess();
            var propList = new List<LogPropertyInfo>
            {
                new LogPropertyInfo() { Key = LogPropertyInfo.VersionKey, Value = AppUtility.ProductVersion},
            };

            if (properties != null)
                propList.AddRange(properties);

            this.Repository.Commit(authentication.ID, comment, propList.ToArray());
            this.OnChanged(EventArgs.Empty);
        }

        public LogInfo[] GetLog(string[] paths, string revision)
        {
            return this.Repository.GetLog(paths, revision);
        }

        public string GetDataBaseUri(string repoUri, string itemUri)
        {
            var pattern = "(@\\d+)$";
            var pureRepoUri = Regex.Replace(repoUri, pattern, string.Empty);
            var pureItemUri = Regex.Replace(itemUri, pattern, string.Empty);
            var relativeUri = UriUtility.MakeRelativeOfDirectory(pureRepoUri, pureItemUri);
            return pureRepoUri;
        }

        public RepositoryItem[] Status(params string[] paths)
        {
            this.Dispatcher.VerifyAccess();
            return this.Repository.Status(paths);
        }

        public void Dispose()
        {
            //this.Repository.Dispose();
            if (this.Dispatcher.Owner == this)
                this.Dispatcher.Dispose();
        }

        public void Lock(params string[] paths)
        {
            this.Dispatcher.VerifyAccess();
            if (paths.Distinct().Count() != paths.Length)
            {
                System.Diagnostics.Debugger.Break();
            }
            foreach (var item in paths)
            {
                NameValidator.ValidatePath(item);
                if (this.paths.Contains(item) == true)
                    throw new ItemAlreadyExistsException(item);
            }
            foreach (var item in paths)
            {
                this.paths.Add(item);
            }

            this.CremaHost.Debug($"{this.GetType().Name} Lock{Environment.NewLine}{string.Join(Environment.NewLine, paths)}");
        }

        public void Unlock(params string[] paths)
        {
            this.Dispatcher.VerifyAccess();
            if (paths.Distinct().Count() != paths.Length)
            {
                System.Diagnostics.Debugger.Break();
            }
            foreach (var item in paths)
            {
                NameValidator.ValidatePath(item);
                if (this.paths.Contains(item) == false)
                {
                    System.Diagnostics.Debugger.Break();
                    throw new ItemNotFoundException(item);
                }
            }
            foreach (var item in paths)
            {
                this.paths.Remove(item);
            }
            this.CremaHost.Debug($"{this.GetType().Name} Unlock{Environment.NewLine}{string.Join(Environment.NewLine, paths)}");
        }

        public Task LockAsync(params string[] paths)
        {
            return this.Dispatcher.InvokeAsync(() => this.Lock(paths));
        }

        public Task UnlockAsync(params string[] paths)
        {
            return this.Dispatcher.InvokeAsync(() => this.Unlock(paths));
        }

        public Task BeginTransactionAsync(string author, string name)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.BeginTransaction(author, name);
            });
        }

        public Task EndTransactionAsync()
        {
            return this.Dispatcher.InvokeAsync(this.EndTransaction);
        }

        public Task CancelTransactionAsync()
        {
            return this.Dispatcher.InvokeAsync(this.CancelTransaction);
        }

        public RepositoryInfo RepositoryInfo => this.Repository.RepositoryInfo;

        public CremaDispatcher Dispatcher { get; }

        public abstract CremaHost CremaHost { get; }

        public event EventHandler Changed;

        protected IRepository Repository { get; }

        protected string RepositoryPath { get; }

        protected void OnChanged(EventArgs e)
        {
            this.Changed?.Invoke(this, e);
        }
    }
}

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

#pragma warning disable 0612
using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Ntreev.Crema.Data.Xml.Schema;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    sealed class DataBaseRepositoryHost : RepositoryHost
    {
        private readonly DataBase dataBase;
        private readonly CremaSettings settings;
        private Version version;
        private readonly HashSet<string> types = new HashSet<string>();
        private readonly HashSet<string> tables = new HashSet<string>();
        private readonly HashSet<string> itemPaths = new HashSet<string>();

        public DataBaseRepositoryHost(DataBase dataBase, IRepository repository)
            : base(repository, null)
        {
            this.dataBase = dataBase;
            this.settings = this.dataBase.GetService(typeof(CremaSettings)) as CremaSettings;
            this.RefreshItems();
        }

        public void Lock(params string[] itemPaths)
        {
            this.Dispatcher.VerifyAccess();
            if (itemPaths.Distinct().Count() != itemPaths.Length)
            {
                System.Diagnostics.Debugger.Launch();
            }
            foreach (var item in itemPaths)
            {
                if (this.itemPaths.Contains(item) == true)
                    throw new ItemAlreadyExistsException(item);
            }
            foreach (var item in itemPaths)
            {
                this.itemPaths.Add(item);
            }
        }

        public void Unlock(params string[] itemPaths)
        {
            this.Dispatcher.VerifyAccess();
            if (itemPaths.Distinct().Count() != itemPaths.Length)
            {
                System.Diagnostics.Debugger.Launch();
            }
            foreach (var item in itemPaths)
            {
                if (this.itemPaths.Contains(item) == false)
                {
                    System.Diagnostics.Debugger.Launch();
                    throw new ItemNotFoundException(item);
                }
            }
            foreach (var item in itemPaths)
            {
                this.itemPaths.Remove(item);
            }
        }

        public Task LockAsync(params string[] itemPaths)
        {
            return this.Dispatcher.InvokeAsync(() => this.Lock(itemPaths));
        }

        public Task UnlockAsync(params string[] itemPaths)
        {
            return this.Dispatcher.InvokeAsync(() => this.Unlock(itemPaths));
        }

        public void RefreshItems()
        {
            var typeDirectory = Path.Combine(this.dataBase.BasePath, CremaSchema.TypeDirectory);
            var typesItemPaths = this.dataBase.Serializer.GetItemPaths(typeDirectory, typeof(CremaDataType), ObjectSerializerSettings.Empty);
            this.types.Clear();
            foreach (var item in typesItemPaths)
            {
                this.types.Add(Path.GetFileName(item));
            }

            var tableDirectory = Path.Combine(this.dataBase.BasePath, CremaSchema.TableDirectory);
            var tablesItemPaths = this.dataBase.Serializer.GetItemPaths(tableDirectory, typeof(CremaDataTable), ObjectSerializerSettings.Empty);
            this.tables.Clear();
            foreach (var item in tablesItemPaths)
            {
                this.tables.Add(Path.GetFileName(item));
            }
        }

        public void Commit(Authentication authentication, string comment)
        {
            this.Dispatcher.VerifyAccess();
            var props = new List<LogPropertyInfo>
            {
                //new LogPropertyInfo() { Key = LogPropertyInfo.BranchRevisionKey, Value = $"{this.RepositoryInfo.BranchRevision}"},
                //new LogPropertyInfo() { Key = LogPropertyInfo.BranchSourceKey, Value = $"{this.RepositoryInfo.BranchSource}"},
                //new LogPropertyInfo() { Key = LogPropertyInfo.BranchSourceRevisionKey, Value = $"{this.RepositoryInfo.BranchSourceRevision}"},
            };

#if DEBUG
            if (this.settings.ValidationMode == true)
            {
                try
                {
                    var dataSet = Ntreev.Crema.Data.CremaDataSet.ReadFromDirectory(this.dataBase.BasePath);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debugger.Launch();
                    this.dataBase.CremaHost.Error("DataSet Error");
                    this.dataBase.CremaHost.Error(e);
                }
            }
#endif
            try
            {
                base.Commit(authentication, comment, props.ToArray());
                this.RefreshItems();
            }
            catch
            {
                throw;
            }
        }

        public CremaDataSet GetTypeData(IObjectSerializer serializer, string itemPath, string revision)
        {
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var itemList = new List<string>();
                var files = serializer.GetPath(itemPath, typeof(CremaDataType), ObjectSerializerSettings.Empty);
                foreach (var item in files)
                {
                    var exportPath = this.ExportItem(item, tempPath, revision);
                    itemList.Add(FileUtility.RemoveExtension(exportPath));
                }

                var referencedFiles = serializer.GetReferencedPath(itemPath, typeof(CremaDataType), ObjectSerializerSettings.Empty);
                foreach (var item in referencedFiles)
                {
                    this.ExportItem(item, tempPath, revision);
                }

                var props = new CremaDataSetSerializerSettings(itemList.ToArray(), null);
                return serializer.Deserialize(tempPath, typeof(CremaDataSet), props) as CremaDataSet;
            }
            finally
            {
                DirectoryUtility.Delete(tempPath);
            }
        }

        public CremaDataSet GetTypeCategoryData(IObjectSerializer serializer, string itemPath, string revision)
        {
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var revisionValue = revision ?? this.RepositoryInfo.Revision;
                var repoUri = this.GetUri(this.RepositoryPath, revisionValue);
                var categoryUri = this.GetUri(itemPath, revisionValue);
                var categoryPath = this.Export(categoryUri, tempPath);
                var baseUri = this.GetDataBaseUri($"{repoUri}", $"{categoryUri}");
                var items = serializer.GetItemPaths(categoryPath, typeof(CremaDataType), ObjectSerializerSettings.Empty);
                var props = new CremaDataSetSerializerSettings(items, null);
                return serializer.Deserialize(tempPath, typeof(CremaDataSet), ObjectSerializerSettings.Empty) as CremaDataSet;
            }
            finally
            {
                DirectoryUtility.Delete(tempPath);
            }
        }

        public CremaDataSet GetTableData(IObjectSerializer serializer, string itemPath, string templateItemPath, string revision)
        {
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var props = new CremaDataTableSerializerSettings(itemPath, templateItemPath);
                var files = serializer.GetPath(itemPath, typeof(CremaDataTable), props);
                foreach (var item in files)
                {
                    this.ExportItem(item, tempPath, revision);
                }

                var referencedFiles = serializer.GetReferencedPath(itemPath, typeof(CremaDataTable), props);
                foreach (var item in referencedFiles)
                {
                    this.ExportItem(item, tempPath, revision);
                }

                return serializer.Deserialize(tempPath, typeof(CremaDataSet), ObjectSerializerSettings.Empty) as CremaDataSet;
            }
            finally
            {
                DirectoryUtility.Delete(tempPath);
            }
        }

        public CremaDataSet GetTableCategoryData(IObjectSerializer serializer, string itemPath, string revision)
        {
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var revisionValue = revision ?? this.RepositoryInfo.Revision;
                var repoUri = this.GetUri(this.RepositoryPath, revisionValue);
                var categoryUri = this.GetUri(itemPath, revisionValue);
                var categoryPath = this.Export(categoryUri, tempPath);
                var baseUri = this.GetDataBaseUri($"{repoUri}", $"{categoryUri}");

                var items = serializer.GetItemPaths(categoryPath, typeof(CremaDataTable), ObjectSerializerSettings.Empty);
                foreach (var item in items)
                {
                    var files = serializer.GetPath(item, typeof(CremaDataTable), ObjectSerializerSettings.Empty);
                    foreach (var f in files)
                    {
                        this.ExportRevisionItem(f, tempPath, revision);
                    }

                    var referencedFiles = serializer.GetReferencedPath(item, typeof(CremaDataTable), ObjectSerializerSettings.Empty);
                    foreach (var f in referencedFiles)
                    {
                        this.ExportRevisionItem(f, tempPath, revision);
                    }
                }

                return serializer.Deserialize(tempPath, typeof(CremaDataSet), ObjectSerializerSettings.Empty) as CremaDataSet;
            }
            finally
            {
                DirectoryUtility.Delete(tempPath);
            }
        }

        private string ExportItem(string path, string exportPath, string revision)
        {
            var revisionValue = revision ?? this.RepositoryInfo.Revision;
            var relativeItemUri = UriUtility.MakeRelativeOfDirectory(this.RepositoryPath, path);
            var itemUri = UriUtility.Combine(exportPath, relativeItemUri);
            var itemTempPath = new Uri(itemUri).LocalPath;
            if (File.Exists(itemTempPath) == false)
            {
                var itemRevisionUri = this.GetUri(path, revisionValue);
                return this.Export(itemRevisionUri, exportPath);
            }
            return null;
        }

        private string ExportRevisionItem(string path, string exportPath, string revision)
        {
            var revisionValue = revision ?? this.RepositoryInfo.Revision;
            var relativeItemUri = UriUtility.MakeRelativeOfDirectory(exportPath, path);
            var itemUri = UriUtility.Combine(exportPath, relativeItemUri);
            var itemTempPath = new Uri(itemUri).LocalPath;
            if (File.Exists(itemTempPath) == false)
            {
                var itemRevisionUri = new Uri(UriUtility.Combine(this.RepositoryPath, $"{relativeItemUri}@{revisionValue}"));
                return this.Export(itemRevisionUri, exportPath);
            }
            return null;
        }

        public void CreateTypeCategory(string itemPath)
        {
            var directoryName = PathUtility.GetDirectoryName(itemPath);
            if (Directory.Exists(directoryName) == false)
                throw new DirectoryNotFoundException();
            if (Directory.Exists(itemPath) == true)
                throw new IOException();
            Directory.CreateDirectory(itemPath);
            this.Add(itemPath);
        }

        public void RenameTypeCategory(DataBaseSet dataBaseSet, string categoryPath, string newCategoryPath)
        {
            dataBaseSet.SetTypeCategoryPath(categoryPath, newCategoryPath);
        }

        public void MoveTypeCategory(DataBaseSet dataBaseSet, string categoryPath, string newCategoryPath)
        {
            dataBaseSet.SetTypeCategoryPath(categoryPath, newCategoryPath);
        }

        public void DeleteTypeCategory(DataBaseSet dataBaseSet, string categoryPath)
        {
            dataBaseSet.DeleteTypeCategory(categoryPath);
        }

        public void CreateTableCategory(string itemPath)
        {
            var directoryName = PathUtility.GetDirectoryName(itemPath);
            if (Directory.Exists(directoryName) == false)
                throw new DirectoryNotFoundException();
            if (Directory.Exists(itemPath) == true)
                throw new IOException();
            Directory.CreateDirectory(itemPath);
            this.Add(itemPath);
        }

        public void RenameTableCategory(DataBaseSet dataBaseSet, string categoryPath, string newCategoryPath)
        {
            dataBaseSet.SetTableCategoryPath(categoryPath, newCategoryPath);
        }

        public void MoveTableCategory(DataBaseSet dataBaseSet, string categoryPath, string newCategoryPath)
        {
            dataBaseSet.SetTableCategoryPath(categoryPath, newCategoryPath);
        }

        public void DeleteTableCategory(DataBaseSet dataBaseSet, string categoryPath)
        {
            dataBaseSet.DeleteTableCategory(categoryPath);
        }

        public void CreateType(DataBaseSet dataBaseSet, string[] typePaths)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in typePaths)
            {
                var name = Path.GetFileName(item);
                if (this.types.Contains(name) == true)
                    throw new ItemAlreadyExistsException(item);
            }
            dataBaseSet.CreateType();
        }

        public void RenameType(DataBaseSet dataBaseSet, string typePath, string typeName)
        {
            this.Dispatcher.VerifyAccess();
            if (this.types.Contains(typeName))
                throw new ItemAlreadyExistsException(typeName);
            dataBaseSet.RenameType(typePath, typeName);
        }

        public void MoveType(DataBaseSet dataBaseSet, string typePath, string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            dataBaseSet.MoveType(typePath, categoryPath);
        }

        public void DeleteType(DataBaseSet dataBaseSet, string typePath)
        {
            this.Dispatcher.VerifyAccess();
            dataBaseSet.DeleteType(typePath);
        }

        public void ModifyType(DataBaseSet dataBaseSet)
        {
            this.Dispatcher.VerifyAccess();
            dataBaseSet.ModifyType();
        }

        public void CreateTable(DataBaseSet dataBaseSet, string[] tablePaths)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in tablePaths)
            {
                var name = Path.GetFileName(item);
                if (this.tables.Contains(name) == true)
                    throw new ItemAlreadyExistsException(item);
            }
            dataBaseSet.CreateTable();
        }

        public void RenameTable(DataBaseSet dataBaseSet, string tablePath, string tableName)
        {
            this.Dispatcher.VerifyAccess();
            if (this.tables.Contains(tableName))
                throw new ItemAlreadyExistsException(tableName);
            dataBaseSet.RenameTable(tablePath, tableName);
        }

        public void MoveTable(DataBaseSet dataBaseSet, string tablePath, string categoryPath)
        {
            this.Dispatcher.VerifyAccess();
            dataBaseSet.MoveTable(tablePath, categoryPath);
        }

        public void DeleteTable(DataBaseSet dataBaseSet, string tablePath)
        {
            this.Dispatcher.VerifyAccess();
            dataBaseSet.DeleteTable(tablePath);
        }

        public void ModifyTable(DataBaseSet dataBaseSet)
        {
            this.Dispatcher.VerifyAccess();
            dataBaseSet.ModifyTable();
        }

        public void Modify(CremaDataSet dataSet)
        {
            this.Dispatcher.VerifyAccess();
            DataBaseSet.Modify(dataSet, this.dataBase);
        }

        public Version Version
        {
            get
            {
                if (this.version == null)
                {
                    var versionPath = Path.Combine(this.RepositoryPath, ".version");
                    if (File.Exists(versionPath) == true)
                    {
                        this.version = new Version(File.ReadAllText(versionPath).Trim());
                    }
                    else
                    {
                        this.version = new Version(0, 0);
                    }
                }
                return this.version;
            }
        }
    }
}

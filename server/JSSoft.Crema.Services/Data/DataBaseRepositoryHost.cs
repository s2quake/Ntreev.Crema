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

#pragma warning disable 0612
using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JSSoft.Crema.Services.Data
{
    sealed class DataBaseRepositoryHost : RepositoryHost
    {
        private readonly DataBase dataBase;
        private readonly CremaSettings settings;
        private Version version;

        private readonly HashSet<string> types = new HashSet<string>();
        private readonly HashSet<string> tables = new HashSet<string>();

        public DataBaseRepositoryHost(DataBase dataBase, IRepository repository)
            : base(repository)
        {
            this.dataBase = dataBase;
            this.settings = this.dataBase.GetService(typeof(CremaSettings)) as CremaSettings;
            this.RefreshItems();
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

        public CremaDataSet ReadDataSet(Authentication authentication, string[] fullPaths)
        {
            return this.ReadDataSet(authentication, fullPaths, false);
        }

        public CremaDataSet ReadDataSet(Authentication authentication, string[] fullPaths, bool schemaOnly)
        {
            var typeFiles = this.GetTypeFiles(fullPaths);
            var tableFiles = this.GetTableFiles(fullPaths);
            var props = new CremaDataSetSerializerSettings(authentication, typeFiles, tableFiles) { SchemaOnly = schemaOnly };
            var dataSet = this.Serializer.Deserialize(this.dataBase.BasePath, typeof(CremaDataSet), props) as CremaDataSet;
            dataSet.SetItemPaths(fullPaths);
            return dataSet;
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
                    var dataSet = JSSoft.Crema.Data.CremaDataSet.ReadFromDirectory(this.dataBase.BasePath);
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

        public CremaDataSet GetTypeData(IObjectSerializer serializer, string path, string revision)
        {
            var repositoryPath = new RepositoryPath(this.TypeContext, path);
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var itemList = new List<string>();
                var files = serializer.GetPath(repositoryPath.Path, typeof(CremaDataType), ObjectSerializerSettings.Empty);
                foreach (var item in files)
                {
                    var exportPath = this.ExportItem(item, tempPath, revision);
                    itemList.Add(FileUtility.RemoveExtension(exportPath));
                }

                var referencedFiles = serializer.GetReferencedPath(repositoryPath.Path, typeof(CremaDataType), ObjectSerializerSettings.Empty);
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

        public CremaDataSet GetTypeCategoryData(IObjectSerializer serializer, string path, string revision)
        {
            var repositoryPath = new RepositoryPath(this.TypeContext, path);
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var revisionValue = revision ?? this.RepositoryInfo.Revision;
                var repoUri = this.GetUri(this.RepositoryPath, revisionValue);
                var categoryUri = this.GetUri(repositoryPath.Path, revisionValue);
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

        public CremaDataSet GetTableData(IObjectSerializer serializer, string path, string templatedPath, string revision)
        {
            var repositoryPath = new RepositoryPath(this.TableContext, path);
            var templatedItemPath = templatedPath != null ? new RepositoryPath(this.TableContext, templatedPath).Path : null;
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var props = new CremaDataTableSerializerSettings(repositoryPath.Path, templatedItemPath);
                var files = repositoryPath.GetFiles();
                foreach (var item in files)
                {
                    this.ExportItem(item, tempPath, revision);
                }

                var referencedFiles = serializer.GetReferencedPath(repositoryPath.Path, typeof(CremaDataTable), props);
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

        public CremaDataSet GetTableCategoryData(IObjectSerializer serializer, string path, string revision)
        {
            var repositoryPath = new RepositoryPath(this.TableContext, path);
            var tempPath = PathUtility.GetTempPath(true);
            try
            {
                var revisionValue = revision ?? this.RepositoryInfo.Revision;
                var repoUri = this.GetUri(this.RepositoryPath, revisionValue);
                var categoryUri = this.GetUri(repositoryPath.Path, revisionValue);
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

        public void CreateTypeCategory(string categoryPath)
        {
            var repositoryPath = new RepositoryPath(this.TypeContext, categoryPath);
            var parentPath = repositoryPath.ParentPath;

            parentPath.ValidateExists();
            repositoryPath.ValidateNotExists();

            Directory.CreateDirectory(repositoryPath.Path);
            this.Add(repositoryPath);
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

        public void CreateTableCategory(string categoryPath)
        {
            var repositoryPath = new RepositoryPath(this.TableContext, categoryPath);
            var parentPath = repositoryPath.ParentPath;

            parentPath.ValidateExists();
            repositoryPath.ValidateNotExists();

            Directory.CreateDirectory(repositoryPath.Path);
            this.Add(repositoryPath);
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

        public void RenameTable(DataBaseSet dataBaseSet, string tablePath, string name)
        {
            this.Dispatcher.VerifyAccess();
            if (this.tables.Contains(name))
                throw new ItemAlreadyExistsException(name);
            dataBaseSet.RenameTable(tablePath, name);
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

        public override CremaHost CremaHost => this.dataBase.CremaHost;

        private string[] GetTypeFiles(string[] fullPath)
        {
            var query = from item in fullPath
                        where item.StartsWith(DataBase.TypePathPrefix) && item.EndsWith(PathUtility.Separator) == false
                        let repositoryPath = new RepositoryPath(this.dataBase.BasePath, item)
                        where repositoryPath.IsExists
                        select repositoryPath.Path;
            return query.ToArray();
        }

        private string[] GetTableFiles(string[] fullPath)
        {
            var query = from item in fullPath
                        where item.StartsWith(DataBase.TablePathPrefix) && item.EndsWith(PathUtility.Separator) == false
                        let repositoryPath = new RepositoryPath(this.dataBase.BasePath, item)
                        where repositoryPath.IsExists
                        select repositoryPath.Path;
            return query.ToArray();
        }

        private IObjectSerializer Serializer => this.dataBase.Serializer;

        private TypeContext TypeContext => this.dataBase.TypeContext;

        private TableContext TableContext => this.dataBase.TableContext;
    }
}

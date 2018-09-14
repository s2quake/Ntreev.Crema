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

namespace Ntreev.Crema.Services.Data
{
    sealed class DataBaseRepositoryHost : RepositoryHost
    {
        private readonly DataBase dataBase;
        private readonly CremaSettings settings;
        private Version version;
        private readonly HashSet<string> types = new HashSet<string>();
        private readonly HashSet<string> typesToAdd = new HashSet<string>();
        private readonly HashSet<string> typesToRemove = new HashSet<string>();

        public DataBaseRepositoryHost(DataBase dataBase, IRepository repository)
            : base(repository, null)
        {
            this.dataBase = dataBase;
            this.settings = this.dataBase.GetService(typeof(CremaSettings)) as CremaSettings;
        }

        protected override void OnReverted()
        {
            base.OnReverted();
            this.typesToAdd.Clear();
            this.typesToRemove.Clear();
        }

        public void Initialize()
        {
            foreach (var item in this.dataBase.TypeContext.Types)
            {
                this.types.Add(item.Name);
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
                    this.dataBase.CremaHost.Error("DataSet Error");
                    this.dataBase.CremaHost.Error(e);
                }
            }
#endif
            try
            {
                base.Commit(authentication, comment, props.ToArray());
                foreach (var item in this.typesToRemove)
                {
                    this.types.Remove(item);
                }
                foreach (var item in this.typesToAdd)
                {
                    this.types.Add(item);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                this.typesToAdd.Clear();
                this.typesToRemove.Clear();
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

        public void RenameTableCategory(DataBaseSet dataBaseSet, string categoryPath, string newCategoryPath)
        {
            dataBaseSet.SetTableCategoryPath(categoryPath, newCategoryPath);
        }

        public void MoveTableCategory(DataBaseSet dataBaseSet, string categoryPath, string newCategoryPath)
        {
            dataBaseSet.SetTableCategoryPath(categoryPath, newCategoryPath);
        }

        public void CreateType(DataBaseSet dataBaseSet, string typeName)
        {
            if (this.types.Contains(typeName) || this.typesToAdd.Contains(typeName))
                throw new Exception("123");
            this.typesToAdd.Add(typeName);
            dataBaseSet.CreateType();
        }

        public void RenameType(DataBaseSet dataBaseSet, string typePath, string typeName)
        {
            if (this.types.Contains(typeName) || this.typesToAdd.Contains(typeName))
                throw new Exception("123");
            this.typesToAdd.Add(typeName);
            this.typesToRemove.Add(Path.GetFileName(typePath));
            dataBaseSet.RenameType(typePath, typeName);
        }

        public void MoveType(DataBaseSet dataBaseSet, string typePath, string categoryPath)
        {
            dataBaseSet.MoveType(typePath, categoryPath);
        }

        public void DeleteType(DataBaseSet dataBaseSet, string typePath)
        {
            this.typesToRemove.Add(Path.GetFileName(typePath));
            dataBaseSet.DeleteType(typePath);
        }

        public void ModifyType(DataBaseSet dataBaseSet)
        {
            dataBaseSet.ModifyType();
        }

        public void CreateTable(DataBaseSet dataBaseSet)
        {
            dataBaseSet.CreateTable();
        }

        public void RenameTable(DataBaseSet dataBaseSet, string tablePath, string tableName)
        {
            dataBaseSet.RenameTable(tablePath, tableName);
        }

        public void MoveTable(DataBaseSet dataBaseSet, string tablePath, string categoryPath)
        {
            dataBaseSet.MoveTable(tablePath, categoryPath);
        }

        public void DeleteTable(DataBaseSet dataBaseSet, string tablePath)
        {
            dataBaseSet.DeleteTable(tablePath);
        }

        public void ModifyTable(DataBaseSet dataBaseSet)
        {
            dataBaseSet.ModifyType();
        }

        public void Modify(CremaDataSet dataSet)
        {
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

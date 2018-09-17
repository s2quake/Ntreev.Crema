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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library.ObjectModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Ntreev.Library;
using Ntreev.Library.IO;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseSet
    {
        private readonly DataBase dataBase;
        private readonly CremaDataSet dataSet;
        private readonly List<CremaDataType> types = new List<CremaDataType>();
        private readonly List<CremaDataTable> tables = new List<CremaDataTable>();

        public DataBaseSet(DataBase dataBase, CremaDataSet dataSet)
        {
            this.dataBase = dataBase ?? throw new ArgumentNullException(nameof(dataBase));
            this.dataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
            this.dataBase.Dispatcher.VerifyAccess();

            foreach (var item in dataSet.Types)
            {
                this.types.Add(item);
                var type = dataBase.TypeContext.Types[item.Name, item.CategoryPath];
                if (type != null)
                {
                    item.ExtendedProperties[typeof(TypeInfo)] = type.TypeInfo;
                }
            }

            foreach (var item in dataSet.Tables)
            {
                this.tables.Add(item);
                var table = dataBase.TableContext.Tables[item.Name, item.CategoryPath];
                if (table != null)
                {
                    item.ExtendedProperties[typeof(TableInfo)] = table.TableInfo;
                    item.ExtendedProperties[nameof(TableInfo.TemplatedParent)] = table.TemplatedParent?.TableInfo;
                }
            }
        }

        public void SetTypeCategoryPath(string categoryPath, string newCategoryPath)
        {
            var itemPath1 = this.TypeContext.GenerateCategoryPath(categoryPath);
            var itemPath2 = this.TypeContext.GenerateCategoryPath(newCategoryPath);

            if (Directory.Exists(itemPath1) == false)
                throw new DirectoryNotFoundException();
            if (Directory.Exists(itemPath2) == true)
                throw new IOException();

            foreach (var item in this.types)
            {
                var typeInfo = (TypeInfo)item.ExtendedProperties[typeof(TypeInfo)];
                if (typeInfo.Path.StartsWith(categoryPath) == false)
                    continue;

                this.ValidateTypeExists(item.Path);
                item.CategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
                this.ValidateTypeNotExists(item.Path);
            }

            this.Serialize();
            this.Repository.Move(itemPath1, itemPath2);
        }

        public void SetTableCategoryPath(string categoryPath, string newCategoryPath)
        {
            foreach (var item in this.tables)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                var tablePath = tableInfo.CategoryPath + tableInfo.Name;
                if (tablePath.StartsWith(categoryPath) == false)
                    continue;
                if (tableInfo.ParentName != string.Empty)
                    continue;

                item.CategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
            }

            this.Serialize();

            var itemPath1 = this.TableContext.GenerateCategoryPath(categoryPath);
            var itemPath2 = this.TableContext.GenerateCategoryPath(newCategoryPath);
            this.Repository.Move(itemPath1, itemPath2);
        }

        public void CreateType()
        {
            this.Serialize();
            this.AddTypesRepositoryPath();
        }

        public void RenameType(string typePath, string typeName)
        {
            var dataType = this.types.First(item => item.Path == typePath);
            this.ValidateTypeExists(dataType.Path);
            dataType.TypeName = typeName;
            this.ValidateTypeNotExists(dataType.Path);
            this.Serialize();
            this.MoveTypesRepositoryPath();
        }

        public void MoveType(string typePath, string categoryPath)
        {
            var dataType = this.types.First(item => item.Path == typePath);
            this.ValidateTypeExists(dataType.Path);
            dataType.CategoryPath = categoryPath;
            this.ValidateTypeNotExists(dataType.Path);
            this.Serialize();
            this.MoveTypesRepositoryPath();
        }

        public void DeleteType(string typePath)
        {
            var dataType = this.types.First(item => item.Path == typePath);
            var dataSet = dataType.DataSet;
            dataSet.Types.Remove(dataType);
            this.ValidateTypeExists(dataType.Path);
            this.DeleteTypesRepositoryPath();
        }

        public void ModifyType()
        {
            foreach (var item in this.types)
            {
                this.ValidateTypeExists(item.Path);
            }
            this.Serialize();
        }

        public void CreateTable()
        {
            this.Serialize();
            this.AddTablesRepositoryPath();
        }

        public void RenameTable(string tablePath, string tableName)
        {
            var dataTable = this.tables.First(item => item.Path == tablePath);
            this.ValidateTableExists(dataTable.Path);
            dataTable.TableName = tableName;
            this.ValidateTableNotExists(dataTable.Path);
            this.Serialize();
            this.MoveTablesRepositoryPath();
        }

        public void MoveTable(string tablePath, string categoryPath)
        {
            var dataTable = this.tables.First(item => item.Path == tablePath);
            this.ValidateTableExists(dataTable.Path);
            dataTable.CategoryPath = categoryPath;
            this.ValidateTableNotExists(dataTable.Path);
            this.Serialize();
            this.MoveTablesRepositoryPath();
        }

        public void DeleteTable(string tablePath)
        {
            var dataTable = this.tables.First(item => item.Path == tablePath);
            var dataSet = dataTable.DataSet;
            dataSet.Tables.Remove(dataTable);
            this.DeleteTablesRepositoryPath();
        }

        public void ModifyTable()
        {
            foreach (var item in this.tables)
            {
                this.ValidateTableExists(item.Path);
            }
            this.Serialize();
        }

        public static void Modify(CremaDataSet dataSet, DataBase dataBase)
        {
            var dataBaseSet = new DataBaseSet(dataBase, dataSet);
            dataBaseSet.Serialize();
        }

        public IEnumerable<CremaDataType> Types => this.types;

        public DataBaseItemState GetTypeState(CremaDataType dataType)
        {
            if (dataType.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
            {
                var typeInfo = (TypeInfo)dataType.ExtendedProperties[typeof(TypeInfo)];
                var itemPath1 = this.TypeContext.GeneratePath(dataType.Path);
                var itemPath2 = this.TypeContext.GeneratePath(typeInfo.Path);

                if (dataType.Name != typeInfo.Name)
                {
                    return DataBaseItemState.Rename;
                }
                else if (itemPath1 != itemPath2)
                {
                    return DataBaseItemState.Move;
                }
                else
                {
                    return DataBaseItemState.None;
                }
            }
            else
            {
                return DataBaseItemState.Create;
            }
        }

        private void Serialize()
        {
            foreach (var item in this.types)
            {
                if (item.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
                {
                    var typeInfo = (TypeInfo)item.ExtendedProperties[typeof(TypeInfo)];
                    var itemPath1 = this.TypeContext.GeneratePath(item.Path);
                    var itemPath2 = this.TypeContext.GeneratePath(typeInfo.Path);

                    if (itemPath1 != itemPath2)
                    {
                        this.ValidateTypeNotExists(item.Path);
                        this.ValidateTypeExists(typeInfo.Path);
                    }
                    else
                    {
                        this.ValidateTypeExists(item.Path);
                    }

                    this.Serializer.Serialize(itemPath2, item, ObjectSerializerSettings.Empty);
                }
                else
                {
                    this.ValidateTypeNotExists(item.Path);
                    var itemPath = this.TypeContext.GenerateTypePath(item.CategoryPath, item.Name);
                    var sss = Path.GetFileName(itemPath);

                    var files = DirectoryUtility.GetAllFiles(this.dataBase.BasePath, sss + ".*");
                    if (files.Any())

                    {
                        int qwer = 0;
                    }


                    this.Serializer.Serialize(itemPath, item, ObjectSerializerSettings.Empty);
                }
            }

            foreach (var item in this.tables)
            {
                if (item.ExtendedProperties.ContainsKey(typeof(TableInfo)) == true)
                {
                    var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                    var tablePath = tableInfo.CategoryPath + tableInfo.Name;
                    var itemPath = this.TableContext.GeneratePath(tablePath);
                    if (item.ExtendedProperties.ContainsKey(nameof(TableInfo.TemplatedParent)) == true && item.ExtendedProperties[nameof(TableInfo.TemplatedParent)] != null)
                    {
                        var templateInfo = (TableInfo)item.ExtendedProperties[nameof(TableInfo.TemplatedParent)];
                        var templatedItemPath = this.TableContext.GeneratePath(templateInfo.CategoryPath + templateInfo.Name);
                    }
                    else
                    {
                        var props = new CremaDataTableSerializerSettings(itemPath, null);
                        this.Serializer.Serialize(itemPath, item, props);
                    }
                }
                else
                {
                    var itemPath = this.TableContext.GenerateTablePath(item.CategoryPath, item.Name);
                    var props = new CremaDataTableSerializerSettings(item.Namespace, item.TemplateNamespace);
                    this.Serializer.Serialize(itemPath, item, props);
                }
            }
        }

        private void AddTypesRepositoryPath()
        {
            foreach (var item in this.types)
            {
                if (item.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
                    continue;

                this.AddRepositoryPath(item);
            }
        }

        private void MoveTypesRepositoryPath()
        {
            foreach (var item in this.types)
            {
                var typeInfo = (TypeInfo)item.ExtendedProperties[typeof(TypeInfo)];
                if (typeInfo.Path == item.Path)
                    continue;

                this.MoveRepositoryPath(item, typeInfo.Path);
            }
        }

        private void DeleteTypesRepositoryPath()
        {
            foreach (var item in this.types)
            {
                var typeInfo = (TypeInfo)item.ExtendedProperties[typeof(TypeInfo)];
                if (item.DataSet != null)
                    continue;

                this.DeleteRepositoryPath(item, typeInfo.Path);
            }
        }

        private void AddTablesRepositoryPath()
        {
            foreach (var item in this.tables)
            {
                if (item.ExtendedProperties.ContainsKey(typeof(TableInfo)) == true)
                    continue;

                this.AddRepositoryPath(item);
            }
        }

        private void MoveTablesRepositoryPath()
        {
            foreach (var item in this.tables)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                if (tableInfo.Path == item.Path)
                    continue;

                this.MoveRepositoryPath(item, tableInfo.Path);
            }
        }

        private void DeleteTablesRepositoryPath()
        {
            foreach (var item in this.tables)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                if (item.DataSet != null)
                    continue;

                this.DeleteRepositoryPath(item, tableInfo.Path);
            }
        }

        private void AddRepositoryPath(CremaDataType dataType)
        {
            var itemPath = this.TypeContext.GenerateTypePath(dataType.CategoryPath, dataType.Name);
            var files = this.TypeContext.GetFiles(itemPath);
            var status = this.Repository.Status(files);

            foreach (var item in status)
            {
                if (item.Status == RepositoryItemStatus.Untracked)
                {
                    if (dataType.SourceType == null)
                    {
                        this.Repository.Add(item.Path);
                    }
                    else
                    {
                        var extension = Path.GetExtension(item.Path);
                        var sourceType = dataType.SourceType;
                        var sourcePath = this.TypeContext.GenerateTypePath(sourceType.CategoryPath, sourceType.Name) + extension;
                        FileUtility.Backup(item.Path);
                        try
                        {
                            this.Repository.Copy(sourcePath, item.Path);
                        }
                        finally
                        {
                            FileUtility.Restore(item.Path);
                        }
                    }
                }
            }
        }

        private void MoveRepositoryPath(CremaDataType dataType, string typePath)
        {
            var itemPath = this.TypeContext.GeneratePath(typePath);
            var files = this.TypeContext.GetFiles(itemPath);

            for (var i = 0; i < files.Length; i++)
            {
                var path1 = files[i];
                var extension = Path.GetExtension(path1);
                var path2 = this.TypeContext.GeneratePath(dataType.CategoryPath + dataType.Name) + extension;
                this.Repository.Move(path1, path2);
            }
        }

        private void DeleteRepositoryPath(CremaDataType dataType, string typePath)
        {
            var itemPath = this.TypeContext.GeneratePath(typePath);
            var files = this.TypeContext.GetFiles(itemPath);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
            this.Repository.DeleteRange(files);
        }

        private void AddRepositoryPath(CremaDataTable dataTable)
        {
            var context = this.dataBase.TableContext;
            var itemPath = context.GenerateTablePath(dataTable.CategoryPath, dataTable.Name);
            var files = context.GetFiles(itemPath);
            var status = this.Repository.Status(files);

            foreach (var item in status)
            {
                if (item.Status == RepositoryItemStatus.Untracked)
                {
                    if (dataTable.SourceTable == null)
                    {
                        this.Repository.Add(item.Path);
                    }
                    else
                    {
                        var extension = Path.GetExtension(item.Path);
                        var sourceTable = dataTable.SourceTable;
                        var sourcePath = context.GenerateTablePath(sourceTable.CategoryPath, sourceTable.Name) + extension;
                        FileUtility.Backup(item.Path);
                        try
                        {
                            this.Repository.Copy(sourcePath, item.Path);
                        }
                        finally
                        {
                            FileUtility.Restore(item.Path);
                        }
                    }
                }
            }
        }

        private void MoveRepositoryPath(CremaDataTable dataTable, string tablePath)
        {
            var itemPath = this.TableContext.GeneratePath(tablePath);
            var files = this.TableContext.GetFiles(itemPath);

            for (var i = 0; i < files.Length; i++)
            {
                var path1 = files[i];
                var extension = Path.GetExtension(path1);
                var path2 = this.TableContext.GeneratePath(dataTable.CategoryPath + dataTable.Name) + extension;
                this.Repository.Move(path1, path2);
            }
        }

        private void DeleteRepositoryPath(CremaDataTable dataTable, string tablePath)
        {
            var itemPath = this.TableContext.GeneratePath(tablePath);
            var files = this.TableContext.GetFiles(itemPath);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
            this.Repository.DeleteRange(files);
        }

        private void ValidateTypeExists(string typePath)
        {
            var itemPath = this.TypeContext.GeneratePath(typePath);
            var files = this.Serializer.GetPath(itemPath, typeof(CremaDataType), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
        }

        private void ValidateTypeNotExists(string typePath)
        {
            var itemPath = this.TypeContext.GeneratePath(typePath);
            var files = this.Serializer.GetPath(itemPath, typeof(CremaDataType), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == true)
                    throw new FileNotFoundException();
            }
        }

        private void ValidateTableExists(string tablePath)
        {
            var itemPath = this.TableContext.GeneratePath(tablePath);
            var files = this.Serializer.GetPath(itemPath, typeof(CremaDataTable), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == false)
                    throw new FileNotFoundException();
            }
        }

        private void ValidateTableNotExists(string tablePath)
        {
            var itemPath = this.TableContext.GeneratePath(tablePath);
            var files = this.Serializer.GetPath(itemPath, typeof(CremaDataTable), ObjectSerializerSettings.Empty);
            foreach (var item in files)
            {
                if (File.Exists(item) == true)
                    throw new FileNotFoundException();
            }
        }

        private DataBaseRepositoryHost Repository => this.dataBase.Repository;

        private IObjectSerializer Serializer => this.dataBase.Serializer;

        private TypeContext TypeContext => this.dataBase.TypeContext;

        private TableContext TableContext => this.dataBase.TableContext;
    }
}

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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class DataBaseSet : IDisposable
    {
        private readonly DataBase dataBase;
        private readonly List<CremaDataType> types = new();
        private readonly List<CremaDataTable> tables = new();
        private readonly Dictionary<string, string> hashByPath = new();
        private readonly DataBaseSetOptions options;

        private DataBaseSet(DataBase dataBase, CremaDataSet dataSet, DataBaseSetOptions options)
        {
            this.dataBase = dataBase ?? throw new ArgumentNullException(nameof(dataBase));
            this.DataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
            this.options = options;
            this.dataBase.Dispatcher.VerifyAccess();

            try
            {
                foreach (var item in dataSet.Types)
                {
                    var type = dataBase.TypeContext.Types[item.Name, item.CategoryPath];
                    if (type == null && this.options.HasFlag(DataBaseSetOptions.AllowTypeCreation) == false)
                    {
                        throw new TypeNotFoundException(item.Name);
                    }
                    if (type != null)
                    {
                        var repositoryPath = new RepositoryPath(dataBase.TypeContext, type.Path);
                        foreach (var i in repositoryPath.GetFiles())
                        {
                            hashByPath.Add(i, HashUtility.GetHashValueFromFile(i));
                        }
                        item.ExtendedProperties[typeof(TypeInfo)] = type.TypeInfo;
                    }
                    this.types.Add(item);
                }

                foreach (var item in dataSet.Tables)
                {
                    var table = dataBase.TableContext.Tables[item.Name, item.CategoryPath];
                    if (table == null && this.options.HasFlag(DataBaseSetOptions.AllowTableCreation) == false)
                    {
                        throw new TableNotFoundException(item.Name);
                    }
                    if (table != null)
                    {
                        item.ExtendedProperties[typeof(TableInfo)] = table.TableInfo;
                        item.ExtendedProperties[nameof(TableInfo.TemplatedParent)] = table.TemplatedParent?.TableInfo;
                    }
                    this.tables.Add(item);
                }
            }
            catch
            {
                this.Repository.Dispatcher.Invoke(() => this.Repository.Unlock(Authentication.System, this, nameof(DataBaseSet), dataSet.GetItemPaths()));
                throw;
            }
        }

        public static Task<DataBaseSet> CreateAsync(DataBase dataBase, CremaDataSet dataSet)
        {
            return dataBase.Dispatcher.InvokeAsync(() => new DataBaseSet(dataBase, dataSet, DataBaseSetOptions.None));
        }

        public static Task<DataBaseSet> CreateAsync(DataBase dataBase, CremaDataSet dataSet, DataBaseSetOptions options)
        {
            return dataBase.Dispatcher.InvokeAsync(() => new DataBaseSet(dataBase, dataSet, options));
        }

        public static DataBaseSet Create(DataBase dataBase, CremaDataSet dataSet, DataBaseSetOptions options)
        {
            return new DataBaseSet(dataBase, dataSet, options);
        }

        public static Task<DataBaseSet> CreateEmptyAsync(DataBase dataBase, string[] fullPaths)
        {
            var dataSet = new CremaDataSet();
            dataSet.SetItemPaths(fullPaths);
            return dataBase.Dispatcher.InvokeAsync(() => new DataBaseSet(dataBase, dataSet, DataBaseSetOptions.None));
        }

        public void SetTypeCategoryPath(string categoryPath, string newCategoryPath)
        {
            var repositoryPath1 = new RepositoryPath(this.TypeContext, categoryPath);
            var repositoryPath2 = new RepositoryPath(this.TypeContext, newCategoryPath);

            repositoryPath1.ValidateExists();
            repositoryPath2.ValidateNotExists();

            foreach (var item in this.types)
            {
                var typeInfo = (TypeInfo)item.ExtendedProperties[typeof(TypeInfo)];
                if (typeInfo.CategoryPath.StartsWith(categoryPath) == false)
                    continue;

                var newItemCategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
                var itemPath1 = new RepositoryPath(this.TypeContext, item.Path);
                var itemPath2 = new RepositoryPath(this.TypeContext, newItemCategoryPath + item.Name);
                itemPath1.ValidateExists();
                itemPath2.ValidateNotExists();
                item.CategoryPath = newItemCategoryPath;
            }

            this.Serialize();
            this.Repository.Move(repositoryPath1, repositoryPath2);
        }

        public void DeleteTypeCategory(string categoryPath)
        {
            var itemPath = new RepositoryPath(this.TypeContext, categoryPath);
            this.Repository.Delete(itemPath);
        }

        public void CreateType()
        {
            this.Serialize();
            foreach (var item in this.types)
            {
                if (item.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
                    continue;

                this.AddRepositoryPath(item);
            }
        }

        public void RenameType(string typePath, string name)
        {
            var dataType = this.types.First(item => item.Path == typePath);
            var repositoryPath1 = new RepositoryPath(this.TypeContext, typePath);
            var repositoryPath2 = new RepositoryPath(this.TypeContext, dataType.CategoryPath + name);

            repositoryPath1.ValidateExists();
            repositoryPath2.ValidateNotExists();
            dataType.TypeName = name;
            this.Serialize();
            this.Repository.Move(repositoryPath1, repositoryPath2);
        }

        public void MoveType(string typePath, string categoryPath)
        {
            var dataType = this.types.First(item => item.Path == typePath);
            var repositoryPath1 = new RepositoryPath(this.TypeContext, typePath);
            var repositoryPath2 = new RepositoryPath(this.TypeContext, categoryPath + dataType.Name);

            repositoryPath1.ValidateExists();
            repositoryPath2.ValidateNotExists();
            dataType.CategoryPath = categoryPath;
            this.Serialize();
            this.Repository.Move(repositoryPath1, repositoryPath2);
        }

        public void DeleteType(string typePath)
        {
            var dataType = this.types.First(item => item.Path == typePath);
            var repositoryPath = new RepositoryPath(this.TypeContext, typePath);
            var dataSet = dataType.DataSet;

            repositoryPath.ValidateExists();
            dataSet.Types.Remove(dataType);
            this.Repository.Delete(repositoryPath);
        }

        public void ModifyType()
        {
            foreach (var item in this.types)
            {
                var repositoryPath = new RepositoryPath(this.TypeContext, item.Path);
                repositoryPath.ValidateExists();
            }
            this.Serialize();
        }

        public void SetTableCategoryPath(string categoryPath, string newCategoryPath)
        {
            var repositoryPath1 = new RepositoryPath(this.TableContext, categoryPath);
            var repositoryPath2 = new RepositoryPath(this.TableContext, newCategoryPath);

            repositoryPath1.ValidateExists();
            repositoryPath2.ValidateNotExists();

            var query = from item in this.tables
                        let tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)]
                        where tableInfo.CategoryPath.StartsWith(categoryPath) == true
                        select item;

            foreach (var item in query)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                var newItemCategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
                var itemPath1 = new RepositoryPath(this.TableContext, item.Path);
                var itemPath2 = new RepositoryPath(this.TableContext, newItemCategoryPath + tableInfo.Name);
                itemPath1.ValidateExists();
                itemPath2.ValidateNotExists();
            }

            foreach (var item in query)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                var newItemCategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
                item.CategoryPath = newItemCategoryPath;
            }

            this.Serialize();
            this.Repository.Move(repositoryPath1, repositoryPath2);
        }

        public void DeleteTableCategory(string categoryPath)
        {
            var itemPath = new RepositoryPath(this.TableContext, categoryPath);
            this.Repository.Delete(itemPath);
        }

        public void CreateTable()
        {
            this.Serialize();
            foreach (var item in this.tables)
            {
                if (item.ExtendedProperties.ContainsKey(typeof(TableInfo)) == true)
                    continue;

                this.AddRepositoryPath(item);
            }

            foreach (var item in this.types)
            {
                var repositoryPath = new RepositoryPath(this.TypeContext, item.Path);
                repositoryPath.ValidateExists();
                // var status = this.Repository.Status(repositoryPath.GetFiles());
                foreach (var i in repositoryPath.GetFiles())
                {
                    var h1 = HashUtility.GetHashValueFromFile(i);
                    var h2 = hashByPath[i];
                    if (h1 != h2)
                        throw new CremaException("타입이 변경되었습니다.");
                    int qwer = 0;
                }
                // foreach (var file in status)
                // {
                //     if (file.Status != RepositoryItemStatus.None)
                //     {
                //         throw new CremaException("타입이 변경되었습니다.");
                //     }
                // }
            }
        }

        public void RenameTable(string tablePath, string name)
        {
            var tableName = CremaDataTable.GetTableName(name);
            var dataTable = this.tables.First(item => item.Path == tablePath);
            var repositoryPath1 = new RepositoryPath(this.TableContext, tablePath);
            var repositoryPath2 = new RepositoryPath(this.TableContext, dataTable.CategoryPath + name);
            repositoryPath1.ValidateExists();
            repositoryPath2.ValidateNotExists();
            dataTable.TableName = tableName;
            this.Serialize();
            foreach (var item in this.tables)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                if (tableInfo.Path == item.Path)
                    continue;
                var itemPath1 = new RepositoryPath(this.TableContext, tableInfo.Path);
                var itemPath2 = new RepositoryPath(this.TableContext, item.Path);
                itemPath1.ValidateExists();
                itemPath2.ValidateNotExists();
                this.Repository.Move(itemPath1, itemPath2);
            }
        }

        public void MoveTable(string tablePath, string categoryPath)
        {
            var dataTable = this.tables.First(item => item.Path == tablePath);
            var repositoryPath1 = new RepositoryPath(this.TableContext, tablePath);
            var repositoryPath2 = new RepositoryPath(this.TableContext, categoryPath + dataTable.Name);

            repositoryPath1.ValidateExists();
            repositoryPath2.ValidateNotExists();
            dataTable.CategoryPath = categoryPath;
            this.Serialize();
            foreach (var item in this.tables)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                if (tableInfo.Path == item.Path)
                    continue;
                var itemPath1 = new RepositoryPath(this.TableContext, tableInfo.Path);
                var itemPath2 = new RepositoryPath(this.TableContext, item.Path);
                itemPath1.ValidateExists();
                itemPath2.ValidateNotExists();
                this.Repository.Move(itemPath1, itemPath2);
            }
        }

        public void DeleteTable(string tablePath)
        {
            var dataTable = this.tables.First(item => item.Path == tablePath);
            var repositoryPath = new RepositoryPath(this.TableContext, tablePath);
            var dataSet = dataTable.DataSet;

            repositoryPath.ValidateExists();
            dataSet.Tables.Remove(dataTable);
            this.Repository.Delete(repositoryPath);
        }

        public void ModifyTable()
        {
            var query = from table in this.tables
                        from column in table.Columns
                        where column.CremaType != null
                        select column.CremaType;

            var types = query.Distinct();
            foreach (var item in this.tables)
            {
                var repositoryPath = new RepositoryPath(this.TableContext, item.Path);
                repositoryPath.ValidateExists();
            }
            this.Serialize();

            foreach (var item in types)
            {
                var repositoryPath = new RepositoryPath(this.TypeContext, item.Path);
                repositoryPath.ValidateExists();

                foreach (var i in repositoryPath.GetFiles())
                {
                    var h1 = HashUtility.GetHashValueFromFile(i);
                    var h2 = hashByPath[i];

                    int qwer = 0;
                }
                var status = this.Repository.Status(repositoryPath.GetFiles());
                foreach (var file in status)
                {
                    if (file.Status != RepositoryItemStatus.None)
                    {
                        throw new CremaException("타입이 변경되었습니다.");
                    }
                }
            }
        }

        public void Modify()
        {
            this.Serialize();
        }

        public CremaDataSet DataSet { get; }

        public CremaDataTable[] TablesToCreate
        {
            get
            {
                var tableList = new List<CremaDataTable>(this.tables.Count);
                foreach (var item in this.tables)
                {
                    if (item.ExtendedProperties.ContainsKey(typeof(TableInfo)) == true)
                        continue;
                    tableList.Add(item);
                }
                var query = from item in tableList
                            orderby item.Name
                            orderby item.TemplatedParentName != string.Empty
                            select item;
                return query.ToArray();
            }
            set
            {
                foreach (var item in value)
                {
                    if (this.DataSet.Tables.Contains(item) == false)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    item.ExtendedProperties.Remove(typeof(TableInfo));
                }
            }
        }

        public CremaDataType[] TypesToCreate
        {
            get
            {
                var typeList = new List<CremaDataType>(this.types.Count);
                foreach (var item in this.types)
                {
                    if (item.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
                        continue;
                    typeList.Add(item);
                }
                return typeList.ToArray();
            }
            set
            {
                foreach (var item in value)
                {
                    if (this.DataSet.Types.Contains(item) == false)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    item.ExtendedProperties.Remove(typeof(TypeInfo));
                }
            }
        }

        public string[] ItemPaths => this.DataSet.GetItemPaths();

        private void Serialize()
        {
            foreach (var item in this.types)
            {
                var itemPath1 = new RepositoryPath(this.TypeContext, item.Path);
                if (item.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
                {
                    var typeInfo = (TypeInfo)item.ExtendedProperties[typeof(TypeInfo)];
                    var itemPath2 = new RepositoryPath(this.TypeContext, typeInfo.Path);
                    if (itemPath1 != itemPath2)
                    {
                        itemPath1.ValidateNotExists();
                    }
                    itemPath2.ValidateExists();

                    this.Serializer.Serialize(itemPath2, item, ObjectSerializerSettings.Empty);
                }
                else
                {
                    itemPath1.ValidateNotExists();
                    this.Serializer.Serialize(itemPath1, item, ObjectSerializerSettings.Empty);
                }
            }

            foreach (var item in this.tables)
            {
                var itemPath1 = new RepositoryPath(this.TableContext, item.Path);
                if (item.ExtendedProperties.ContainsKey(typeof(TableInfo)) == true)
                {
                    var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                    var itemPath2 = new RepositoryPath(this.TableContext, tableInfo.Path);
                    if (itemPath1 != itemPath2)
                    {
                        itemPath1.ValidateNotExists();
                    }
                    itemPath2.ValidateExists();

                    var parentItemPath = string.Empty;
                    if (item.ExtendedProperties[nameof(TableInfo.TemplatedParent)] is TableInfo parentInfo)
                    {
                        parentItemPath = new RepositoryPath(this.TableContext, parentInfo.Path).Path;
                    }

                    var props = new CremaDataTableSerializerSettings(itemPath2.Path, parentItemPath);
                    this.Serializer.Serialize(itemPath2, item, props);
                }
                else
                {
                    itemPath1.ValidateNotExists();
                    var props = new CremaDataTableSerializerSettings(item.Namespace, item.TemplateNamespace);
                    this.Serializer.Serialize(itemPath1, item, props);
                }
            }
        }

        private void AddRepositoryPath(CremaDataType dataType)
        {
            var itemPath = new RepositoryPath(this.TypeContext, dataType.Path);
            var files = itemPath.GetFiles();
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
                        var sourcePath = new RepositoryPath(this.TypeContext, sourceType.Path) + extension;
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

        private void AddRepositoryPath(CremaDataTable dataTable)
        {
            var repositoryPath = new RepositoryPath(this.TableContext, dataTable.Path);
            var files = repositoryPath.GetFiles();
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
                        var sourceRepositoryPath = new RepositoryPath(this.TableContext, sourceTable.Path);
                        var sourcePath = sourceRepositoryPath.Path + extension;
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

        private DataBaseRepositoryHost Repository => this.dataBase.Repository;

        private IObjectSerializer Serializer => this.dataBase.Serializer;

        private TypeContext TypeContext => this.dataBase.TypeContext;

        private TableContext TableContext => this.dataBase.TableContext;

        private CremaHost CremaHost => this.dataBase.CremaHost;

        #region IDisposable

        async void IDisposable.Dispose()
        {
            if (this.options.HasFlag(DataBaseSetOptions.OmitUnlock) == true)
                return;
            try
            {
                await this.Repository.Dispatcher.InvokeAsync(() => this.Repository.Unlock(Authentication.System, this, nameof(IDisposable.Dispose), this.ItemPaths));
            }
            catch (Exception e)
            {
                this.CremaHost.Fatal(e);
                throw;
            }
        }

        #endregion
    }
}

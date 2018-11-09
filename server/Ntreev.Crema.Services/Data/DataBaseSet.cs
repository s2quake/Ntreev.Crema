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
using Ntreev.Crema.Data.Xml.Schema;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseSet
    {
        private readonly DataBase dataBase;
        private readonly List<CremaDataType> types = new List<CremaDataType>();
        private readonly List<CremaDataTable> tables = new List<CremaDataTable>();

        private DataBaseSet(DataBase dataBase, CremaDataSet dataSet, bool typeCreation, bool tableCreation)
        {
            this.dataBase = dataBase ?? throw new ArgumentNullException(nameof(dataBase));
            this.DataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
            this.dataBase.Dispatcher.VerifyAccess();

            try
            {
                foreach (var item in dataSet.Types)
                {
                    var type = dataBase.TypeContext.Types[item.Name, item.CategoryPath];
                    if (type == null && typeCreation == false)
                    {
                        throw new TypeNotFoundException(item.Name);
                    }
                    if (type != null)
                    {
                        item.ExtendedProperties[typeof(TypeInfo)] = type.TypeInfo;
                    }
                    this.types.Add(item);
                }

                foreach (var item in dataSet.Tables)
                {
                    var table = dataBase.TableContext.Tables[item.Name, item.CategoryPath];
                    if (table == null && tableCreation == false)
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
                this.Repository.Dispatcher.Invoke(() => this.Repository.Unlock(dataSet.GetItemPaths()));
                throw;
            }
        }

        public static Task<DataBaseSet> CreateAsync(DataBase dataBase, CremaDataSet dataSet, bool typeCreation, bool tableCreation)
        {
            return dataBase.Dispatcher.InvokeAsync(() => new DataBaseSet(dataBase, dataSet, typeCreation, tableCreation));
        }

        public static DataBaseSet Create(DataBase dataBase, CremaDataSet dataSet, bool typeCreation, bool tableCreation)
        {
            return new DataBaseSet(dataBase, dataSet, typeCreation, tableCreation);
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

        public void RenameType(string typePath, string typeName)
        {
            var dataType = this.types.First(item => item.Path == typePath);
            var repositoryPath1 = new RepositoryPath(this.TypeContext, typePath);
            var repositoryPath2 = new RepositoryPath(this.TypeContext, dataType.CategoryPath + typeName);

            repositoryPath1.ValidateExists();
            repositoryPath2.ValidateNotExists();
            dataType.TypeName = typeName;
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

            foreach (var item in this.tables)
            {
                var tableInfo = (TableInfo)item.ExtendedProperties[typeof(TableInfo)];
                if (tableInfo.CategoryPath.StartsWith(categoryPath) == false)
                    continue;

                var newItemCategoryPath = Regex.Replace(item.CategoryPath, "^" + categoryPath, newCategoryPath);
                var itemPath1 = new RepositoryPath(this.TableContext, item.Path);
                var itemPath2 = new RepositoryPath(this.TableContext, newItemCategoryPath + tableInfo.Name);
                itemPath1.ValidateExists();
                itemPath2.ValidateNotExists();
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
            var query = from table in this.tables
                        from column in table.Columns
                        where column.CremaType != null
                        select column.CremaType;
            var types = query.Distinct();

            this.Serialize();
            foreach (var item in this.tables)
            {
                if (item.ExtendedProperties.ContainsKey(typeof(TableInfo)) == true)
                    continue;

                this.AddRepositoryPath(item);
            }

            foreach (var item in types)
            {
                var repositoryPath = new RepositoryPath(this.TypeContext, item.Path);
                repositoryPath.ValidateExists();
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

        // TODO: 자식 테이블 이름 변경 해결 해야됨
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
            this.Repository.Move(repositoryPath1, repositoryPath2);
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
            this.Repository.Move(repositoryPath1, repositoryPath2);
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

        public static void Modify(CremaDataSet dataSet, DataBase dataBase)
        {
            var dataBaseSet = new DataBaseSet(dataBase, dataSet, false, false);
            dataBaseSet.Serialize();
        }

        public DataBaseItemState GetTypeState(CremaDataType dataType)
        {
            if (dataType.ExtendedProperties.ContainsKey(typeof(TypeInfo)) == true)
            {
                var typeInfo = (TypeInfo)dataType.ExtendedProperties[typeof(TypeInfo)];
                var itemPath1 = new RepositoryPath(this.TypeContext, dataType.Path);
                var itemPath2 = new RepositoryPath(this.TypeContext, typeInfo.Path);

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

        public IEnumerable<CremaDataType> Types => this.types;

        public CremaDataSet DataSet { get; }

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
    }
}

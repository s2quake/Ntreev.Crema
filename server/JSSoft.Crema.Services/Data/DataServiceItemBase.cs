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
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data.Serializations;
using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    public abstract class DataServiceItemBase : IDisposable
    {
        private const string dataDirectory = "data";
        private const string infoDirectory = "info";
        private readonly ILogService logService;
        private bool initialized;
        private string workingPath;
        private Dictionary<string, TableInfo> tableInfos = new();
        private Dictionary<string, TypeInfo> typeInfos = new();
        private Dictionary<string, object> tableDatas = new();
        private Dictionary<string, object> typeDatas = new();
        private DataServiceItemInfo info;

        protected DataServiceItemBase(IDataBase dataBase)
        {
            this.DataBase = dataBase;
            this.DataBaseName = dataBase.Name;
            // this.DataBase.Loaded += DataBase_Loaded;
            // this.DataBase.Renamed += DataBase_Renamed;
            // this.DataBase.Unloaded += DataBase_Unloaded;
            this.logService = dataBase.GetService(typeof(ILogService)) as ILogService;
            this.Serializer = dataBase.GetService(typeof(IObjectSerializer)) as IObjectSerializer;

            if (dataBase is DataBase dataBaseInternal)
            {
                this.NoCache = dataBaseInternal.CremaHost.NoCache;
            }

            if (this.NoCache == false)
                this.ReadInfo();

            if (this.info.Revision != dataBase.DataBaseInfo.Revision)
                this.info = new DataServiceItemInfo();
        }

        public void Dispose()
        {
            DirectoryUtility.Delete(this.BasePath);
        }

        public void Commit()
        {
            if (this.initialized == false)
                return;

            DirectoryUtility.Backup(this.BasePath);

            DirectoryUtility.Prepare(this.BasePath);
            foreach (var item in this.tableInfos)
            {
                var itemPath = Path.Combine(this.BasePath, CremaSchema.TableDirectory, infoDirectory, item.Key);
                this.Serializer.Serialize(itemPath, item.Value, ObjectSerializerSettings.Empty);

            }
            foreach (var item in this.tableDatas)
            {
                var itemPath = Path.Combine(this.BasePath, CremaSchema.TableDirectory, dataDirectory, item.Key);
                this.Serializer.Serialize(itemPath, item.Value, ObjectSerializerSettings.Empty);
            }

            foreach (var item in this.typeInfos)
            {
                var itemPath = Path.Combine(this.BasePath, CremaSchema.TypeDirectory, infoDirectory, item.Key);
                this.Serializer.Serialize(itemPath, item.Value, ObjectSerializerSettings.Empty);
            }
            foreach (var item in this.typeDatas)
            {
                var itemPath = Path.Combine(this.BasePath, CremaSchema.TypeDirectory, dataDirectory, item.Key);
                this.Serializer.Serialize(itemPath, item.Value, ObjectSerializerSettings.Empty);
            }

            var tableInfos = this.tableInfos.Select(item => Path.Combine(CremaSchema.TableDirectory, item.Key));
            var typeInfos = this.typeInfos.Select(item => Path.Combine(CremaSchema.TypeDirectory, item.Key));

            this.info.ItemList = tableInfos.Concat(typeInfos).ToArray();
            this.info.Version = new Version(CremaSchema.MajorVersion, CremaSchema.MinorVersion);
            this.WriteInfo();
            DirectoryUtility.Clean(this.BasePath);
        }

        public async Task ResetAsync()
        {
            await this.DataBase.Dispatcher.InvokeAsync(() =>
            {
                this.DataBase.Loaded -= DataBase_Loaded;
                this.DataBase.Renamed -= DataBase_Renamed;
                this.DataBase.Unloaded -= DataBase_Unloaded;
            });

            if (this.DataBase.IsLoaded == false)
                await this.DataBase.LoadAsync(this.Authentication);
            var contains = await this.DataBase.Dispatcher.InvokeAsync(() => this.DataBase.Contains(this.Authentication));
            if (contains == false)
                await this.DataBase.EnterAsync(this.Authentication);

            var dataSet = null as CremaDataSet;
            try
            {
                dataSet = await this.DataBase.GetDataSetAsync(this.Authentication, CremaDataSetFilter.Default, null);
            }
            finally
            {
                if (contains == false)
                    await this.DataBase.LeaveAsync(this.Authentication);
            }

            DirectoryUtility.Delete(this.BasePath);
            this.Serialize(dataSet, this.info.Revision);
            this.initialized = true;
            await this.DataBase.Dispatcher.InvokeAsync(() =>
            {
                this.DataBase.Loaded += DataBase_Loaded;
                this.DataBase.Renamed += DataBase_Renamed;
                this.DataBase.Unloaded += DataBase_Unloaded;
            });
        }

        public abstract CremaDispatcher Dispatcher { get; }

        public IDataBase DataBase { get; }

        public abstract string Name { get; }

        public string BasePath
        {
            get
            {
                if (this.workingPath == null)
                {
                    if (this.DataBase.GetService(typeof(ICremaHost)) is CremaHost cremaHost)
                    {
                        this.workingPath = cremaHost.GetPath(CremaPath.Caches, this.Name, $"{this.DataBase.ID}");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                return this.workingPath;
            }
        }

        public string DataBaseName { get; private set; }

        public string Revision => this.info.Revision;

        public bool NoCache { get; private set; }

        public DataServiceItemInfo DataServiceItemInfo => this.info;

        public IObjectSerializer Serializer { get; }

        public event EventHandler Changed;

        public Guid DataBaseID { get; private set; }

        protected virtual void OnChanged(EventArgs e)
        {
            this.Changed?.Invoke(this, e);
        }

        protected virtual bool CanSerialize(CremaDataTable dataTable)
        {
            return true;
        }

        protected virtual bool CanSerialize(CremaDataType dataType)
        {
            return true;
        }

        protected abstract object GetObject(CremaDataTable dataTable);

        protected abstract object GetObject(CremaDataType dataType);

        protected abstract System.Type TableDataType { get; }

        protected abstract System.Type TypeDataType { get; }

        protected object ReadTable(string name)
        {
            return this.tableDatas[name];
        }

        protected object ReadType(string name)
        {
            return this.typeDatas[name];
        }

        protected TableInfo GetTableInfo(string name)
        {
            return this.tableInfos[name];
        }

        protected TypeInfo GetTypeInfo(string name)
        {
            return this.typeInfos[name];
        }

        protected IEnumerable<string> GetTables()
        {
            return this.tableInfos.Keys;
        }

        protected IEnumerable<string> GetTypes()
        {
            return this.typeInfos.Keys;
        }

        protected abstract Authentication Authentication
        {
            get;
        }

        private async void DataBase_Loaded(object sender, EventArgs e)
        {
            if (sender is DataBase dataBase)
            {
                var typeContext = dataBase.GetService(typeof(ITypeContext)) as ITypeContext;
                var tableContext = dataBase.GetService(typeof(ITableContext)) as ITableContext;
                await dataBase.Dispatcher.InvokeAsync(() =>
                {
                    typeContext.ItemsCreated += TypeContext_ItemCreated;
                    typeContext.ItemsRenamed += TypeContext_ItemRenamed;
                    typeContext.ItemsMoved += TypeContext_ItemMoved;
                    typeContext.ItemsDeleted += TypeContext_ItemDeleted;
                    typeContext.ItemsChanged += TypeContext_ItemsChanged;

                    tableContext.ItemsCreated += TableContext_ItemCreated;
                    tableContext.ItemsRenamed += TableContext_ItemRenamed;
                    tableContext.ItemsMoved += TableContext_ItemMoved;
                    tableContext.ItemsDeleted += TableContext_ItemDeleted;
                    tableContext.ItemsChanged += TableContext_ItemsChanged;
                    this.DataBaseID = this.DataBase.ID;
                });
                this.NoCache = dataBase.CremaHost.NoCache;
            }

            if (this.NoCache == true)
            {
                this.info.Revision = null;
            }

            await this.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    this.Initialize();
                }
                catch
                {

                }
            });
        }

        private void Domains_DomainRowRemoved(object sender, DomainRowEventArgs e)
        {
            this.DeleteDomainFiles(e.DomainInfo);
        }

        private void Domains_DomainRowChanged(object sender, DomainRowEventArgs e)
        {
            this.DeleteDomainFiles(e.DomainInfo);
        }

        private void Domains_DomainRowAdded(object sender, DomainRowEventArgs e)
        {
            this.DeleteDomainFiles(e.DomainInfo);
        }

        private void DataBase_Renamed(object sender, EventArgs e)
        {
            this.DataBaseName = this.DataBase.Name;
        }

        private async void DataBase_Unloaded(object sender, EventArgs e)
        {
            if (sender is IDataBase dataBase && dataBase.GetService(typeof(IDomainContext)) is IDomainContext domainContext)
            {
                domainContext.Dispatcher.Invoke(() =>
                {
                    //domainContext.Domains.DomainsCreated -= Domains_DomainsCreated;
                    //domainContext.Domains.DomainsDeleted -= Domains_DomainsDeleted;
                });
            }

            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Commit();

                this.tableInfos.Clear();
                this.typeInfos.Clear();
                this.tableDatas.Clear();
                this.typeDatas.Clear();
                this.initialized = false;
            });
        }

        private void DeleteDomainFiles(DomainInfo domainInfo)
        {

        }

        private async void TypeContext_ItemCreated(object sender, Services.ItemsCreatedEventArgs<ITypeItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() => this.Serialize(dataSet, revision));
        }

        private async void TypeContext_ItemRenamed(object sender, Services.ItemsRenamedEventArgs<ITypeItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() =>
            {
                for (var i = 0; i < e.Items.Length; i++)
                {
                    this.typeDatas.Remove(e.OldNames[i]);
                    this.typeInfos.Remove(e.OldNames[i]);
                }
                this.Serialize(dataSet, revision);
            });
        }

        private async void TypeContext_ItemMoved(object sender, Services.ItemsMovedEventArgs<ITypeItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() => this.Serialize(dataSet, revision));
        }

        private async void TypeContext_ItemDeleted(object sender, Services.ItemsDeletedEventArgs<ITypeItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() =>
            {
                for (var i = 0; i < e.Items.Length; i++)
                {
                    this.typeDatas.Remove(e.Items[i].Name);
                    this.typeInfos.Remove(e.Items[i].Name);
                }
                this.Serialize(dataSet, revision);
            });
        }

        private async void TypeContext_ItemsChanged(object sender, Services.ItemsEventArgs<ITypeItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() => this.Serialize(dataSet, revision));
        }

        private async void TableContext_ItemCreated(object sender, Services.ItemsCreatedEventArgs<ITableItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() => this.Serialize(dataSet, revision));
        }

        private async void TableContext_ItemRenamed(object sender, Services.ItemsRenamedEventArgs<ITableItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() =>
            {
                for (var i = 0; i < e.Items.Length; i++)
                {
                    this.tableDatas.Remove(e.OldNames[i]);
                    this.tableInfos.Remove(e.OldNames[i]);
                }
                this.Serialize(dataSet, revision);
            });
        }

        private async void TableContext_ItemMoved(object sender, Services.ItemsMovedEventArgs<ITableItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() => this.Serialize(dataSet, revision));
        }

        private async void TableContext_ItemDeleted(object sender, Services.ItemsDeletedEventArgs<ITableItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() =>
            {
                for (var i = 0; i < e.Items.Length; i++)
                {
                    this.tableDatas.Remove(e.Items[i].Name);
                    this.tableInfos.Remove(e.Items[i].Name);
                }
                this.Serialize(dataSet, revision);
            });
        }

        private async void TableContext_ItemsChanged(object sender, Services.ItemsEventArgs<ITableItem> e)
        {
            var dataSet = e.MetaData as CremaDataSet;
            var revision = this.DataBase.DataBaseInfo.Revision;

            await this.Dispatcher.InvokeAsync(() => this.Serialize(dataSet, revision));
        }

        private async void Initialize()
        {
            this.Dispatcher.CheckAccess();
            var error = false;
            try
            {
                if (this.info.Revision != null)
                {
                    foreach (var item in this.info.ItemList)
                    {
                        if (item.StartsWith(CremaSchema.TableDirectory) == true)
                        {
                            var tableName = Path.GetFileName(item);
                            {
                                var itemPath = Path.Combine(this.BasePath, CremaSchema.TableDirectory, infoDirectory, tableName);
                                var tableInfo = (TableInfo)this.Serializer.Deserialize(itemPath, typeof(TableInfo), ObjectSerializerSettings.Empty);
                                this.tableInfos.Add(tableName, tableInfo);
                            }
                            {
                                var itemPath = Path.Combine(this.BasePath, CremaSchema.TableDirectory, dataDirectory, tableName);
                                var tableData = this.Serializer.Deserialize(itemPath, this.TableDataType, ObjectSerializerSettings.Empty);
                                this.tableDatas.Add(tableName, tableData);
                            }
                        }
                        else if (item.StartsWith(CremaSchema.TypeDirectory) == true)
                        {
                            var typeName = Path.GetFileName(item);
                            {
                                var itemPath = Path.Combine(this.BasePath, CremaSchema.TypeDirectory, infoDirectory, typeName);
                                var typeInfo = (TypeInfo)this.Serializer.Deserialize(itemPath, typeof(TypeInfo), ObjectSerializerSettings.Empty);
                                this.typeInfos.Add(typeName, typeInfo);
                            }
                            {
                                var itemPath = Path.Combine(this.BasePath, CremaSchema.TypeDirectory, dataDirectory, typeName);
                                var typeData = this.Serializer.Deserialize(itemPath, this.TypeDataType, ObjectSerializerSettings.Empty);
                                this.typeDatas.Add(typeName, typeData);
                            }
                        }
                    }
                }
                else
                {
                    error = true;
                }
            }
            catch
            {
                error = true;
            }

            if (error == true)
            {
                try
                {

                    var result = await Task.Run(async () =>
                    {
                        var revision = this.DataBase.DataBaseInfo.Revision;
                        var contains = await this.DataBase.Dispatcher.InvokeAsync(() => this.DataBase.Contains(this.Authentication));
                        var filter = CremaDataSetFilter.Default;
                        if (contains == false)
                            await this.DataBase.EnterAsync(this.Authentication);
                        var dataSet = await this.DataBase.GetDataSetAsync(this.Authentication, filter, null);
                        if (contains == false && this.DataBase.DataBaseState == DataBaseState.Loaded)
                            await this.DataBase.LeaveAsync(this.Authentication);
                        return new Tuple<string, CremaDataSet>(revision, dataSet);
                    });
                    this.Serialize(result.Item2, result.Item1);
                }
                catch
                {
                    return;
                }
            }
            this.initialized = true;
        }

        private void Serialize(CremaDataSet dataSet, string revision)
        {
            if (dataSet == null)
                return;

            this.SerializeTables(dataSet);
            this.SerializeTypes(dataSet);

            this.info.Revision = revision;
            this.info.Version = new Version(CremaSchema.MajorVersion, CremaSchema.MinorVersion);
            this.info.DateTime = DateTime.Now;

            this.OnChanged(EventArgs.Empty);
        }

        private void SerializeTables(CremaDataSet dataSet)
        {
            var tableInfos = new Dictionary<string, TableInfo>(this.tableInfos);
            var tableDatas = new Dictionary<string, object>(this.tableDatas);
            var tables = dataSet.Tables.OrderBy(item => item.Name);

            foreach (var item in tables)
            {
                if (this.CanSerialize(item) == true)
                {
                    tableDatas[item.Name] = this.GetObject(item);
                    tableInfos[item.Name] = item.TableInfo;
                }
                else
                {
                    tableInfos.Remove(item.Name);
                    tableDatas.Remove(item.Name);
                }
            }

            foreach (var item in this.tableInfos.Keys.Except(tableInfos.Keys).ToArray())
            {
                FileUtility.Delete(this.BasePath, item);
                tableInfos.Remove(item);
                tableDatas.Remove(item);
            }

            this.tableInfos = tableInfos;
            this.tableDatas = tableDatas;
        }

        private void SerializeTypes(CremaDataSet dataSet)
        {
            var typeInfos = new Dictionary<string, TypeInfo>(this.typeInfos);
            var typeDatas = new Dictionary<string, object>(this.typeDatas);
            var types = dataSet.Types.OrderBy(item => item.Name);

            foreach (var item in types)
            {
                if (this.CanSerialize(item) == true)
                {
                    typeDatas[item.Name] = this.GetObject(item);
                    typeInfos[item.Name] = item.TypeInfo;
                }
                else
                {
                    typeInfos.Remove(item.Name);
                    typeDatas.Remove(item.Name);
                }
            }

            foreach (var item in this.typeInfos.Keys.Except(typeInfos.Keys).ToArray())
            {
                FileUtility.Delete(this.BasePath, item);
                typeInfos.Remove(item);
                typeDatas.Remove(item);
            }

            this.typeInfos = typeInfos;
            this.typeDatas = typeDatas;
        }

        private void WriteInfo()
        {
            var itemPath = Path.Combine(this.BasePath, "info");
            this.Serializer.Serialize(itemPath, (DataServiceItemSerializationInfo)this.info, ObjectSerializerSettings.Empty);
        }

        private void ReadInfo()
        {
            var itemPath = Path.Combine(this.BasePath, "info");

            if (this.Serializer.Exists(itemPath, typeof(DataServiceItemSerializationInfo), ObjectSerializerSettings.Empty) == true)
            {
                try
                {
                    var value = (DataServiceItemSerializationInfo)this.Serializer.Deserialize(itemPath, typeof(DataServiceItemSerializationInfo), ObjectSerializerSettings.Empty);
                    this.info = (DataServiceItemInfo)value;
                }
                catch (Exception e)
                {
                    this.logService.Error(e);
                }
            }
        }
    }
}

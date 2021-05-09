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

using JSSoft.Crema.Data;
using JSSoft.Crema.Runtime.Generation;
using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Data;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.RuntimeService
{
    class RuntimeServiceItem : DataServiceItemBase
    {
        private readonly Dictionary<string, SerializationSet> caches = new();
        private readonly CremaDispatcher dispatcher;
        private readonly Authentication authentication;

        public RuntimeServiceItem(IDataBase dataBase, CremaDispatcher dispatcher, Authentication authentication)
            : base(dataBase)
        {
            this.dispatcher = dispatcher;
            this.authentication = authentication;
        }

        public Task<GenerationSet> GernerationAsync(TagInfo tags, string filterExpression, string revision)
        {
            return this.dispatcher.Invoke(async () =>
            {
                if (filterExpression == null)
                    throw new ArgumentNullException(nameof(filterExpression));

                if (revision == null)
                {
                    var tables = this.GetTables().Select(item => this.GetTableInfo(item))
                                                 .ToArray();
                    var types = this.GetTypes().Select(item => this.GetTypeInfo(item))
                                               .ToArray();

                    var codeSet = new GenerationSet(types, tables)
                    {
                        Name = this.DataBaseName,
                        Revision = this.Revision,
                    };

                    codeSet = codeSet.Filter(tags);
                    if (filterExpression != string.Empty)
                        codeSet = codeSet.Filter(filterExpression);
                    return codeSet;
                }
                else
                {
                    var dataSet = await this.DataBase.GetDataSetAsync(this.authentication, DataSetType.All, filterExpression, revision);
                    var tables = dataSet.Tables.Select(item => item.TableInfo).ToArray();
                    var types = dataSet.Types.Select(item => item.TypeInfo).ToArray();
                    var codeSet = new GenerationSet(types, tables)
                    {
                        Name = this.DataBaseName,
                        Revision = this.Revision,
                    };
                    codeSet = codeSet.Filter(tags);
                    if (filterExpression != string.Empty)
                        codeSet = codeSet.Filter(filterExpression);
                    return codeSet;
                }
            });
        }

        public Task<SerializationSet> SerializeAsync(TagInfo tags, string filterExpression, string revision)
        {
            return this.dispatcher.Invoke(async () =>
            {



                if (filterExpression == null)
                    throw new ArgumentNullException(nameof(filterExpression));

                if (revision == null)
                {
                    var cacheKey = tags.ToString() + filterExpression;

                    if (this.caches.ContainsKey(cacheKey) == true)
                    {
                        return this.caches[cacheKey];
                    }

                    var dataSet = new SerializationSet()
                    {
                        Name = this.DataBaseName,
                        Revision = this.Revision,
                    };

                    var tableItems = this.ReadTables();
                    dataSet.Tables = tableItems.Cast<SerializationTable>().ToArray();

                    var typeItems = this.ReadTypes();
                    dataSet.Types = typeItems.Cast<SerializationType>().ToArray();

                    dataSet = dataSet.Filter(tags);
                    if (filterExpression != string.Empty)
                        dataSet = dataSet.Filter(filterExpression);

                    this.caches[cacheKey] = dataSet;

                    return dataSet;
                }
                else
                {
                    var dataSet = await this.DataBase.GetDataSetAsync(this.authentication, DataSetType.All, filterExpression, revision);
                    var serializedSet = new SerializationSet(dataSet)
                    {
                        Name = this.DataBaseName,
                        Revision = this.Revision,
                    };
                    serializedSet = serializedSet.Filter(tags);
                    if (filterExpression != string.Empty)
                        serializedSet = serializedSet.Filter(filterExpression);
                    return serializedSet;
                }
            });
        }

        public override CremaDispatcher Dispatcher => this.dispatcher;

        public override string Name => "serialization";

        protected override bool CanSerialize(CremaDataTable dataTable)
        {
            return dataTable.DerivedTags != TagInfo.Unused;
        }

        protected override object GetObject(CremaDataTable dataTable)
        {
            return new SerializationTable(dataTable);
        }

        protected override object GetObject(CremaDataType dataType)
        {
            return new SerializationType(dataType);
        }

        //protected override void OnSerializeTable(Stream stream, object tableData)
        //{
        //    this.formatter.Serialize(stream, tableData);
        //}

        //protected override void OnSerializeType(Stream stream, object typeData)
        //{
        //    this.formatter.Serialize(stream, typeData);
        //}

        //protected override object OnDeserializeTable(Stream stream)
        //{
        //    return this.formatter.Deserialize(stream);
        //}

        //protected override object OnDeserializeType(Stream stream)
        //{
        //    return this.formatter.Deserialize(stream);
        //}

        protected override void OnChanged(EventArgs e)
        {
            base.OnChanged(e);
            this.caches.Clear();
        }

        protected override Type TableDataType => typeof(SerializationTable);

        protected override Type TypeDataType => typeof(SerializationType);

        protected override Authentication Authentication => this.authentication;

        private object[] ReadTables()
        {
            var tableNames = this.GetTables().ToArray();
            var items = new List<object>(tableNames.Length);
            for (var i = 0; i < tableNames.Length; i++)
            {
                var tableName = tableNames[i];
                items.Add(this.ReadTable(tableName));
            }
            return items.ToArray();
        }

        private object[] ReadTypes()
        {
            var typeNames = this.GetTypes().ToArray();
            var items = new List<object>(typeNames.Length);
            for (var i = 0; i < typeNames.Length; i++)
            {
                var typeName = typeNames[i];
                items.Add(this.ReadType(typeName));
            }
            return items.ToArray();
        }
    }
}

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

//#if !DEBUG
//#define USE_PARALLEL
//#endif
using JSSoft.Library;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Data.Diff
{
    public class DiffDataSet
    {
        private readonly CremaDataSet dataSet1;
        private readonly CremaDataSet dataSet2;
        private readonly DiffDataResolver resolver = new();
        private string header1;
        private string header2;

        public DiffDataSet(CremaDataSet dataSet1, CremaDataSet dataSet2)
            : this(dataSet1, dataSet2, DiffMergeTypes.None)
        {

        }

        public DiffDataSet(CremaDataSet dataSet1, CremaDataSet dataSet2, DiffMergeTypes mergeType)
            : this(dataSet1, dataSet2, mergeType, new Progress())
        {

        }

        private DiffDataSet(CremaDataSet dataSet1, CremaDataSet dataSet2, DiffMergeTypes mergeType, IProgress progress)
        {
            this.DataSet1 = new CremaDataSet();
            this.DataSet2 = new CremaDataSet();
            this.dataSet1 = dataSet1;
            this.dataSet2 = dataSet2;
            this.MergeType = mergeType;

            this.InitializeTypes(progress);
            this.InitializeTables(progress);
        }
        public static Task<DiffDataSet> CreateAsync(CremaDataSet dataSet1, CremaDataSet dataSet2, DiffMergeTypes mergeType)
        {
            return CreateAsync(dataSet1, dataSet2, mergeType, new Progress());
        }

        public static Task<DiffDataSet> CreateAsync(CremaDataSet dataSet1, CremaDataSet dataSet2, DiffMergeTypes mergeType, IProgress progress)
        {
            return Task.Run(() => new DiffDataSet(dataSet1, dataSet2, mergeType, progress));
        }

        public DiffDataTable[] Tables { get; private set; }

        public DiffDataType[] Types { get; private set; }

        public CremaDataSet DataSet1 { get; private set; }

        public CremaDataSet DataSet2 { get; private set; }

        public string Header1
        {
            get => this.header1 ?? string.Empty;
            set => this.header1 = value;
        }

        public string Header2
        {
            get => this.header2 ?? string.Empty;
            set => this.header2 = value;
        }

        public DiffMergeTypes MergeType { get; private set; }

        private void InitializeTables(IProgress progress)
        {
            var tables1 = this.dataSet1.Tables.Where(item => item.Parent == null && item.TemplatedParent == null)
                                              .OrderBy(item => item.Name)
                                              .ToList();
            var tables2 = this.dataSet2.Tables.Where(item => item.Parent == null && item.TemplatedParent == null)
                                              .OrderBy(item => item.Name)
                                              .ToList();
            var tableList = new List<DiffDataTable>();

            foreach (var item in tables1.ToArray())
            {
                var count = tables2.Count(i => i.TableID == item.TableID);
                if (count == 1)
                {
                    var dataTable1 = item;
                    var dataTable2 = tables2.Single(i => i.TableID == item.TableID);
                    var tableName1 = dataTable1.TableName != dataTable2.TableName ? DiffUtility.DiffDummyKey + dataTable1.TableName : dataTable2.TableName;
                    var tableName2 = dataTable2.TableName;
                    var diffTable1 = DiffDataTable.Create(this.DataSet1, tableName1);
                    var diffTable2 = DiffDataTable.Create(this.DataSet2, tableName2);
                    DiffInternalUtility.SyncColumns(diffTable1, diffTable2, dataTable1, dataTable2);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, dataTable2, this);
                    var diffTemplate = new DiffTemplate(diffTable1, diffTable2, dataTable1, dataTable2) { DiffTable = diffTable };
                    diffTable.Template = diffTemplate;
                    diffTable.InitializeChilds();
                    tableList.Add(diffTable);
                    tables1.Remove(dataTable1);
                    tables2.Remove(dataTable2);
                }
            }

            foreach (var item in tables1)
            {
                var dataTable1 = item;
                if (this.dataSet2.Tables.Contains(dataTable1.TableName) == true)
                {
                    var dataTable2 = this.dataSet2.Tables[dataTable1.TableName];
                    var diffTable1 = DiffDataTable.Create(this.DataSet1, dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Create(this.DataSet2, dataTable2.TableName);
                    DiffInternalUtility.SyncColumns(diffTable1, diffTable2, dataTable1, dataTable2);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, dataTable2, this);
                    var diffTemplate = new DiffTemplate(diffTable1, diffTable2, dataTable1, dataTable2) { DiffTable = diffTable };
                    diffTable.Template = diffTemplate;
                    diffTable.InitializeChilds();
                    tableList.Add(diffTable);
                    tables2.Remove(dataTable2);
                }
                else
                {
                    var diffTable1 = DiffDataTable.Create(new CremaDataSet(), dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Create(new CremaDataSet(), dataTable1.TableName);
                    DiffInternalUtility.SyncColumns(diffTable1, diffTable2, dataTable1, null);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, null, this);
                    var diffTemplate = new DiffTemplate(diffTable1, diffTable2, dataTable1, null) { DiffTable = diffTable };
                    diffTable.Template = diffTemplate;
                    diffTable.InitializeChilds();
                    tableList.Add(diffTable);
                }
            }

            foreach (var item in tables2)
            {
                var dataTable2 = item;
                var diffTable1 = DiffDataTable.Create(this.DataSet1, dataTable2.TableName);
                var diffTable2 = DiffDataTable.Create(this.DataSet2, dataTable2.TableName);
                DiffInternalUtility.SyncColumns(diffTable1, diffTable2, null, dataTable2);
                var diffTable = new DiffDataTable(diffTable1, diffTable2, null, dataTable2, this);
                var diffTemplate = new DiffTemplate(diffTable1, diffTable2, null, dataTable2) { DiffTable = diffTable };
                diffTable.Template = diffTemplate;
                diffTable.InitializeChilds();
                tableList.Add(diffTable);
            }

            foreach (var item in tableList.ToArray())
            {
                item.InitializeDerivedTables();
            }

            foreach (var item in tableList.ToArray())
            {
                tableList.AddRange(item.DerivedTables);
            }

            tableList.AsParallel().ForAll(item => item.DiffTemplate());
            tableList.AsParallel().ForAll(item => item.Diff());

            this.Tables = tableList.OrderBy(item => item.SourceItem1.TableName).ToArray();
        }

        private void InitializeTypes(IProgress progress)
        {
            var types1 = this.dataSet1.Types.ToList();
            var types2 = this.dataSet2.Types.ToList();
            var typeList = new List<DiffDataType>();

            foreach (var item in types1.ToArray())
            {
                var targetItems = types2.ToArray();
                var dataType2 = this.resolver.Resolve(item, targetItems);
                if (dataType2 == null)
                    continue;

                var dataType1 = item;
                var diffType1 = DiffDataType.Create(this.DataSet1, dataType1.TypeName);
                var diffType2 = DiffDataType.Create(this.DataSet2, dataType2.TypeName);
                var diffType = new DiffDataType(diffType1, diffType2, dataType1, dataType2);
                typeList.Add(diffType);
                types1.Remove(dataType1);
                types2.Remove(dataType2);
            }

            foreach (var item in types1)
            {
                var dataType1 = item;
                if (this.dataSet2.Types.Contains(dataType1.TypeName) == true)
                {
                    var dataType2 = this.dataSet2.Types[dataType1.TypeName];
                    var diffType1 = DiffDataType.Create(this.DataSet1, dataType1.TypeName);
                    var diffType2 = DiffDataType.Create(this.DataSet2, DiffUtility.DiffDummyKey + dataType2.TypeName);
                    var diffType = new DiffDataType(diffType1, diffType2, dataType1, dataType2);
                    typeList.Add(diffType);
                    types2.Remove(dataType2);
                }
                else
                {
                    var diffType1 = DiffDataType.Create(new CremaDataSet(), dataType1.TypeName);
                    var diffType2 = DiffDataType.Create(new CremaDataSet(), dataType1.TypeName);
                    var diffType = new DiffDataType(diffType1, diffType2, dataType1, null);
                    typeList.Add(diffType);
                }
            }

            foreach (var item in types2)
            {
                var dataType2 = item;
                var diffType1 = DiffDataType.Create(new CremaDataSet(), dataType2.TypeName);
                var diffType2 = DiffDataType.Create(this.DataSet2, dataType2.TypeName);
                var diffType = new DiffDataType(diffType1, diffType2, null, dataType2);
                typeList.Add(diffType);
            }

            foreach (var item in typeList)
            {
                item.DiffSet = this;
            }

#if USE_PARALLEL
            Parallel.ForEach(typeList, item =>
            {
                item.Diff();
            });
#else
            foreach (var item in typeList)
            {
                item.Diff();
            }
#endif

            this.Types = typeList.OrderBy(item => item.SourceItem1.TypeName).ToArray();
        }
    }
}
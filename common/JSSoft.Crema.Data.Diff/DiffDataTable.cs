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

using JSSoft.Crema.Data.Properties;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace JSSoft.Crema.Data.Diff
{
    public class DiffDataTable : INotifyPropertyChanged
    {
        private readonly CremaDataTable dataTable1;
        private readonly CremaDataTable dataTable2;
        private readonly DiffMergeTypes mergeType;
        private readonly ObservableCollection<object> unresolvedItemList = new();
        private DiffDataTable parent;
        private readonly List<DiffDataRow> itemList = new();
        private string header1;
        private string header2;
        private string[] filters = new string[] { };

        [Obsolete]
        public DiffDataTable(CremaDataTable dataTable1, CremaDataTable dataTable2, DiffMergeTypes mergeType)
        {
            this.SourceItem1 = dataTable1 == null ? new CremaDataTable() : new CremaDataTable(dataTable1.Name, dataTable1.CategoryPath);
            this.SourceItem2 = dataTable2 == null ? new CremaDataTable() : new CremaDataTable(dataTable2.Name, dataTable2.CategoryPath);
            this.dataTable1 = dataTable1;
            this.dataTable2 = dataTable2;
            this.mergeType = mergeType;
        }

        internal DiffDataTable(CremaDataTable diffTable1, CremaDataTable diffTable2, CremaDataTable dataTable1, CremaDataTable dataTable2, DiffDataSet dataSet)
        {
            this.SourceItem1 = diffTable1;
            this.SourceItem2 = diffTable2;
            this.dataTable1 = dataTable1;
            this.dataTable2 = dataTable2;
            this.SourceItem1.ExtendedProperties[typeof(DiffDataTable)] = this;
            this.SourceItem2.ExtendedProperties[typeof(DiffDataTable)] = this;
            this.SourceItem1.InternalComment = (dataTable1 ?? dataTable2).Comment;
            this.SourceItem1.InternalTableID = (dataTable1 ?? dataTable2).TableID;
            this.SourceItem1.InternalTags = (dataTable1 ?? dataTable2).Tags;
            this.SourceItem1.InternalCreationInfo = (dataTable1 ?? dataTable2).CreationInfo;
            this.SourceItem1.InternalModificationInfo = (dataTable1 ?? dataTable2).ModificationInfo;
            this.SourceItem1.InternalContentsInfo = (dataTable1 ?? dataTable2).InternalContentsInfo;
            this.SourceItem2.InternalComment = (dataTable2 ?? dataTable1).Comment;
            this.SourceItem2.InternalTableID = (dataTable2 ?? dataTable1).TableID;
            this.SourceItem2.InternalTags = (dataTable2 ?? dataTable1).Tags;
            this.SourceItem2.InternalCreationInfo = (dataTable2 ?? dataTable1).CreationInfo;
            this.SourceItem2.InternalModificationInfo = (dataTable2 ?? dataTable1).ModificationInfo;
            this.SourceItem2.InternalContentsInfo = (dataTable2 ?? dataTable1).InternalContentsInfo;
            this.DiffSet = dataSet;
        }

        public void Resolve()
        {
            this.ValidateResolve();

            if (this.DiffState == DiffState.Modified)
            {
                this.SourceItem1.RowChanged -= DiffSource1_RowChanged;
                this.SourceItem2.RowChanged -= DiffSource2_RowChanged;
                this.SourceItem1.ReadOnly = false;
                this.SourceItem2.ReadOnly = false;
                this.SourceItem1.DeleteItems();
                this.SourceItem2.DeleteItems();
                this.SourceItem1.IsDiffMode = false;
                this.SourceItem2.IsDiffMode = false;
                this.SourceItem1.AcceptChanges();
                this.SourceItem2.AcceptChanges();
                this.SourceItem1.ReadOnly = true;
                this.SourceItem2.ReadOnly = true;
                this.IsResolved = true;
            }
            else if (this.DiffState == DiffState.Deleted)
            {
                this.MergeDelete();
            }
            else if (this.DiffState == DiffState.Inserted)
            {
                this.MergeInsert();
            }

            this.InvokePropertyChangedEvent(nameof(IsResolved));
        }

        public void ResolveAll()
        {
            this.ValidateResolveInternal();
            foreach (var item in this.Childs)
            {
                item.ValidateResolveInternal();
            }
            foreach (var item in this.Childs)
            {
                item.Resolve();
            }
            this.Resolve();
        }

        public void AcceptChanges()
        {
            this.Resolve();
        }

        public void RejectChanges()
        {
            this.SourceItem1.RowChanged -= DiffSource1_RowChanged;
            this.SourceItem2.RowChanged -= DiffSource2_RowChanged;
            this.itemList.Clear();
            this.SourceItem1.RejectChanges();
            this.SourceItem2.RejectChanges();
            this.itemList.Capacity = this.SourceItem1.Rows.Count;
            for (var i = 0; i < this.SourceItem1.Rows.Count; i++)
            {
                var item = new DiffDataRow(this, i);
                this.itemList.Add(item);
                item.UpdateValidation();
            }
            this.SourceItem1.RowChanged += DiffSource1_RowChanged;
            this.SourceItem2.RowChanged += DiffSource2_RowChanged;
        }

        public bool HasChanges()
        {
            if (this.SourceItem1.HasChanges(true) == true)
                return true;
            if (this.SourceItem2.HasChanges(true) == true)
                return true;
            return false;
        }

        public DiffMergeTypes MergeType
        {
            get
            {
                if (this.DiffSet != null)
                    return this.DiffSet.MergeType;
                return this.mergeType;
            }
        }

        public CremaDataTable ExportTable1()
        {
            return this.ExportTable1(new CremaDataSet());
        }

        public CremaDataTable ExportTable1(CremaDataSet exportSet)
        {
            var dataTable = CreateExportTable(exportSet, this.SourceItem1);
            ExportTable(exportSet, this.SourceItem1);
            return dataTable;
        }

        public CremaDataTable ExportTable2()
        {
            return this.ExportTable2(new CremaDataSet());
        }

        public CremaDataTable ExportTable2(CremaDataSet exportSet)
        {
            var dataTable = CreateExportTable(exportSet, this.SourceItem2);
            ExportTable(exportSet, this.SourceItem2);
            return dataTable;
        }

        public override string ToString()
        {
            return this.SourceItem2.TableName;
        }

        public DiffDataSet DiffSet
        {
            get;
            internal set;
        }

        public CremaDataTable SourceItem1 { get; private set; }

        public CremaDataTable SourceItem2 { get; private set; }

        public TableInfo TableInfo1
        {
            get
            {
                var tableInfo = ((InternalDataTable)this.SourceItem1).TableInfo;
                tableInfo.Name = DiffUtility.GetOriginalName(tableInfo.Name);
                return tableInfo;
            }
        }

        public TableInfo TableInfo2
        {
            get
            {
                var tableInfo = ((InternalDataTable)this.SourceItem2).TableInfo;
                tableInfo.Name = DiffUtility.GetOriginalName(tableInfo.Name);
                return tableInfo;
            }
        }

        public string ItemName1
        {
            get => DiffUtility.GetOriginalName(this.SourceItem1.TableName);
            set => this.SourceItem1.TableName = value;
        }

        public string ItemName2
        {
            get => DiffUtility.GetOriginalName(this.SourceItem2.TableName);
            set => this.SourceItem2.TableName = value;
        }

        public string Header1
        {
            get
            {
                if (this.header1 == null && this.DiffSet != null)
                    return this.DiffSet.Header1;
                return this.header1 ?? string.Empty;
            }
            set => this.header1 = value;
        }

        public string Header2
        {
            get
            {
                if (this.header2 == null && this.DiffSet != null)
                    return this.DiffSet.Header2;
                return this.header2 ?? string.Empty;
            }
            set => this.header2 = value;
        }

        public DiffState DiffState { get; private set; }

        public IReadOnlyList<DiffDataRow> Rows => this.itemList;

        public bool IsResolved { get; private set; }

        public DiffTemplate Template { get; internal set; }

        public DiffDataTable TemplatedParent { get; private set; }

        public DiffDataTable[] DerivedTables { get; private set; } = new DiffDataTable[] { };

        public DiffDataTable[] Childs { get; private set; } = new DiffDataTable[] { };

        public IEnumerable<object> UnresolvedItems => this.unresolvedItemList;

        public string[] Filters
        {
            get => this.filters ?? new string[] { };
            set
            {
                this.filters = value;
                for (var i = 0; i < this.itemList.Count; i++)
                {
                    var item = this.itemList[i];
                    item.Update();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void InitializeRows()
        {
            this.SourceItem1.RowChanged -= DiffSource1_RowChanged;
            this.SourceItem2.RowChanged -= DiffSource2_RowChanged;
            this.SourceItem1.ReadOnly = false;
            this.SourceItem2.ReadOnly = false;

            if (this.unresolvedItemList.Any() == true)
            {
                if (this.parent == null)
                {
                    DiffInternalUtility.InitializeRows(this.SourceItem1, this.SourceItem2, this.dataTable1, this.dataTable2);
                }
                else
                {
                    DiffInternalUtility.InitializeChildRows(this.SourceItem1, this.SourceItem2, this.dataTable1, this.dataTable2);
                }
            }
            else
            {
                if (this.parent == null)
                {
                    DiffInternalUtility.InitializeRows(this.SourceItem1, this.SourceItem2, this.dataTable1 ?? this.dataTable2, this.dataTable2 ?? this.dataTable1);
                }
                else
                {
                    DiffInternalUtility.InitializeChildRows(this.SourceItem1, this.SourceItem2, this.dataTable1 ?? this.dataTable2, this.dataTable2 ?? this.dataTable1);
                }
            }

            this.itemList.Clear();
            this.itemList.Capacity = this.SourceItem1.Rows.Count;
            for (var i = 0; i < this.SourceItem1.Rows.Count; i++)
            {
                var item = new DiffDataRow(this, i);
                this.itemList.Add(item);
            }

            for (var i = 0; i < this.itemList.Count; i++)
            {
                var item = this.itemList[i];
                item.Update();
            }

            this.SourceItem1.AcceptChanges();
            this.SourceItem2.AcceptChanges();
            this.SourceItem1.RowChanged += DiffSource1_RowChanged;
            this.SourceItem2.RowChanged += DiffSource2_RowChanged;
        }

        internal void InitializeChilds()
        {
            var tables1 = (this.dataTable1 != null ? this.dataTable1.Childs.OrderBy(item => item.TableName) : Enumerable.Empty<CremaDataTable>()).ToList();
            var tables2 = (this.dataTable2 != null ? this.dataTable2.Childs.OrderBy(item => item.TableName) : Enumerable.Empty<CremaDataTable>()).ToList();
            var tableList = new List<DiffDataTable>();

            foreach (var item in tables1.ToArray())
            {
                var count = tables2.Count(i => i.TableID == item.TableID);
                if (count == 1)
                {
                    var dataTable1 = item;
                    var dataTable2 = tables2.Single(i => i.TableID == item.TableID);
                    var diffTable1 = DiffDataTable.Create(this.SourceItem1, dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Create(this.SourceItem2, dataTable2.TableName);
                    DiffInternalUtility.SyncColumns(diffTable1, diffTable2, dataTable1, dataTable2);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, dataTable2, this.DiffSet)
                    {
                        parent = this,
                    };
                    var diffTemplate = new DiffTemplate(diffTable1, diffTable2, dataTable1, dataTable2) { DiffTable = diffTable };
                    diffTable.Template = diffTemplate;
                    tableList.Add(diffTable);
                    tables1.Remove(dataTable1);
                    tables2.Remove(dataTable2);
                }
            }

            foreach (var item in tables1)
            {
                var dataTable1 = item;
                if (this.dataTable2 != null && this.dataTable2.Childs.Contains(dataTable1.TableName) == true)
                {
                    var dataTable2 = this.dataTable2.Childs[dataTable1.TableName];
                    var diffTable1 = DiffDataTable.Create(this.SourceItem1, dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Create(this.SourceItem2, dataTable2.TableName);
                    DiffInternalUtility.SyncColumns(diffTable1, diffTable2, dataTable1, dataTable2);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, dataTable2, this.DiffSet)
                    {
                        parent = this,
                    };
                    var diffTemplate = new DiffTemplate(diffTable1, diffTable2, dataTable1, dataTable2) { DiffTable = diffTable };
                    diffTable.Template = diffTemplate;
                    tableList.Add(diffTable);
                    tables2.Remove(dataTable2);
                }
                else
                {
                    var diffTable1 = DiffDataTable.Create(this.SourceItem1, dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Create(this.SourceItem2, dataTable1.TableName);
                    DiffInternalUtility.SyncColumns(diffTable1, diffTable2, dataTable1, null);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, null, this.DiffSet)
                    {
                        parent = this,
                    };
                    var diffTemplate = new DiffTemplate(diffTable1, diffTable2, dataTable1, null) { DiffTable = diffTable };
                    diffTable.Template = diffTemplate;
                    tableList.Add(diffTable);
                }
            }

            foreach (var item in tables2)
            {
                var dataTable2 = item;
                var diffTable1 = DiffDataTable.Create(this.SourceItem1, dataTable2.TableName);
                var diffTable2 = DiffDataTable.Create(this.SourceItem2, dataTable2.TableName);
                DiffInternalUtility.SyncColumns(diffTable1, diffTable2, null, dataTable2);
                var diffTable = new DiffDataTable(diffTable1, diffTable2, null, dataTable2, this.DiffSet)
                {
                    parent = this,
                };
                var diffTemplate = new DiffTemplate(diffTable1, diffTable2, null, dataTable2) { DiffTable = diffTable };
                diffTable.Template = diffTemplate;
                tableList.Add(diffTable);
            }

            this.Childs = tableList.OrderBy(item => item.SourceItem1.TableName).ToArray();
        }

        internal void InitializeDerivedTables()
        {
            var tables1 = (this.dataTable1 != null ? this.dataTable1.DerivedTables.OrderBy(item => item.TableName) : Enumerable.Empty<CremaDataTable>()).ToList();
            var tables2 = (this.dataTable2 != null ? this.dataTable2.DerivedTables.OrderBy(item => item.TableName) : Enumerable.Empty<CremaDataTable>()).ToList();
            var tableList = new List<DiffDataTable>();

            foreach (var item in tables1.ToArray())
            {
                var count = tables2.Count(i => i.TableID == item.TableID);
                if (count == 1)
                {
                    var dataTable1 = item;
                    var dataTable2 = tables2.Single(i => i.TableID == item.TableID);
                    var diffTable1 = DiffDataTable.Inherit(this.SourceItem1, dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Inherit(this.SourceItem2, dataTable2.TableName);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, dataTable2, this.DiffSet) { TemplatedParent = this };
                    var childList = new List<DiffDataTable>();
                    foreach (var i in this.Childs)
                    {
                        var d1 = diffTable1.Childs[i.SourceItem1.TableName];
                        var d2 = diffTable2.Childs[i.SourceItem2.TableName];
                        var t1 = dataTable1.Childs[i.SourceItem1.TableName];
                        var t2 = dataTable2.Childs[i.SourceItem2.TableName];
                        childList.Add(new DiffDataTable(d1, d2, t1, t2, this.DiffSet) { parent = diffTable, TemplatedParent = i });
                    }
                    diffTable.Childs = childList.ToArray();
                    tableList.Add(diffTable);
                    tables1.Remove(dataTable1);
                    tables2.Remove(dataTable2);
                }
            }

            foreach (var item in tables1)
            {
                var dataTable1 = item;
                var dataTable2 = this.dataTable2?.DerivedTables.FirstOrDefault(i => i.TableName == dataTable1.TableName);
                if (this.dataTable2 != null && dataTable2 != null)
                {
                    var diffTable1 = DiffDataTable.Inherit(this.SourceItem1, dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Inherit(this.SourceItem2, dataTable2.TableName);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, dataTable2, this.DiffSet) { TemplatedParent = this };
                    var childList = new List<DiffDataTable>();
                    foreach (var i in this.Childs)
                    {
                        var d1 = diffTable1.Childs[i.SourceItem1.TableName];
                        var d2 = diffTable2.Childs[i.SourceItem2.TableName];
                        var t1 = dataTable1.Childs[i.SourceItem1.TableName];
                        var t2 = dataTable2.Childs[i.SourceItem2.TableName];
                        childList.Add(new DiffDataTable(d1, d2, t1, t2, this.DiffSet) { parent = diffTable, TemplatedParent = i });
                    }
                    diffTable.Childs = childList.ToArray();
                    tableList.Add(diffTable);
                    tables2.Remove(dataTable2);
                }
                else
                {
                    var diffTable1 = DiffDataTable.Inherit(this.SourceItem1, DiffUtility.DiffDummyKey + dataTable1.TableName);
                    var diffTable2 = DiffDataTable.Inherit(this.SourceItem2, DiffUtility.DiffDummyKey + dataTable1.TableName);
                    var diffTable = new DiffDataTable(diffTable1, diffTable2, dataTable1, null, this.DiffSet) { TemplatedParent = this };
                    var childList = new List<DiffDataTable>();
                    foreach (var i in this.Childs)
                    {
                        var d1 = diffTable1.Childs[i.SourceItem1.TableName];
                        var d2 = diffTable2.Childs[i.SourceItem2.TableName];
                        var t1 = dataTable1.Childs[i.SourceItem1.TableName];
                        childList.Add(new DiffDataTable(d1, d2, t1, null, this.DiffSet) { parent = diffTable, TemplatedParent = i });
                    }
                    diffTable.Childs = childList.ToArray();
                    tableList.Add(diffTable);
                }
            }

            foreach (var item in tables2)
            {
                var dataTable2 = item;
                var diffTable1 = DiffDataTable.Inherit(this.SourceItem1, DiffUtility.DiffDummyKey + dataTable2.TableName);
                var diffTable2 = DiffDataTable.Inherit(this.SourceItem2, DiffUtility.DiffDummyKey + dataTable2.TableName);
                var diffTable = new DiffDataTable(diffTable1, diffTable2, null, dataTable2, this.DiffSet) { TemplatedParent = this };
                var childList = new List<DiffDataTable>();
                foreach (var i in this.Childs)
                {
                    var d1 = diffTable1.Childs[i.SourceItem1.TableName];
                    var d2 = diffTable2.Childs[i.SourceItem2.TableName];
                    var t2 = dataTable2.Childs[i.SourceItem2.TableName];
                    childList.Add(new DiffDataTable(d1, d2, null, t2, this.DiffSet) { parent = diffTable, TemplatedParent = i });
                }
                diffTable.Childs = childList.ToArray();
                tableList.Add(diffTable);
            }

            this.DerivedTables = tableList.OrderBy(item => item.SourceItem1.TableName).ToArray();
        }

        internal void InitializeTemplate()
        {
            if (this.TemplatedParent == null)
            {
                this.Template = new DiffTemplate(this.SourceItem1, this.SourceItem2, this.dataTable1, this.dataTable2)
                {
                    DiffTable = this,
                };

                this.Template.Diff();
            }
            else
            {
                this.Template = this.TemplatedParent.Template;
            }

            foreach (var item in this.Childs)
            {
                item.InitializeTemplate();
            }

            foreach (var item in this.DerivedTables)
            {
                item.InitializeTemplate();
            }

            if (this.Template.IsResolved == false)
            {
                this.unresolvedItemList.Add(this.Template);
                this.Template.PropertyChanged += Template_PropertyChanged;
            }

            foreach (var item in this.Childs)
            {
                if (item.Template.IsResolved == false)
                {
                    this.unresolvedItemList.Add(item.Template);
                    item.Template.PropertyChanged += Template_PropertyChanged;
                }
            }

            if (this.parent != null && this.parent.Template.IsResolved == false)
            {
                this.unresolvedItemList.Add(this.parent.Template);
                this.parent.Template.PropertyChanged += Template_PropertyChanged;
            }
        }

        private void InvokePropertyChangedEvent(params string[] propertyNames)
        {
            foreach (var item in propertyNames)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(item));
            }
        }

        private void Validate(CremaDataTable dataTable)
        {
            foreach (var item in dataTable.Rows)
            {
                this.Validate(dataTable, item);
            }
        }

        private void Validate(CremaDataTable dataTable, CremaDataRow dataRow)
        {
            foreach (var item in dataTable.Columns)
            {
                Validate(dataRow, item);
            }
        }

        public static void Validate(CremaDataRow dataRow, CremaDataColumn dataColumn)
        {
            var dataTypeName = dataColumn.DataTypeName;
            if (dataColumn.ExtendedProperties.Contains(nameof(CremaDataColumn.DataTypeName)) == true)
                dataTypeName = dataColumn.ExtendedProperties[nameof(CremaDataColumn.DataTypeName)] as string;

            if (CremaDataTypeUtility.IsBaseType(dataTypeName) == true)
            {
                var type = CremaDataTypeUtility.GetType(dataTypeName);
                var value = dataRow[dataColumn];
                if (value is string textValue && CremaConvert.VerifyChangeType(textValue, type) == false)
                {
                    dataRow.SetColumnError(dataColumn, $"'{value}' 은(는) {type.GetTypeName()} 으로 변경할 수 없습니다.");
                }
                else
                {
                    dataRow.SetColumnError(dataColumn, string.Empty);
                }
            }
            else
            {
                var dataType = GetCremaType();
                var value = dataRow[dataColumn];
                if (value is string textValue && dataType.VerifyValue(textValue) == false)
                {
                    dataRow.SetColumnError(dataColumn, $"'{value}' 은(는) {dataType.Name} 으로 변경할 수 없습니다.");
                }
                else
                {
                    dataRow.SetColumnError(dataColumn, string.Empty);
                }
            }

            CremaDataType GetCremaType()
            {
                var dataSet = dataRow.Table.DataSet;
                if (NameValidator.VerifyItemPath(dataTypeName) == true)
                {
                    var itemName = new ItemName(dataTypeName);
                    return dataSet.Types[itemName.Name, itemName.CategoryPath];
                }
                return dataSet.Types[dataTypeName];
            }
        }

        private void MergeDelete()
        {
            Validate();

            this.SourceItem2.ReadOnly = false;
            for (var i = 0; i < this.SourceItem1.Rows.Count; i++)
            {
                var item1 = this.SourceItem1.Rows[i];
                var item2 = this.SourceItem2.Rows[i];
                item2.CopyFrom(item1);
            }
            this.SourceItem2.ReadOnly = true;
            this.IsResolved = true;

            void Validate()
            {
                if (this.DiffState != DiffState.Deleted)
                    throw new Exception();
            }
        }

        private void MergeInsert()
        {
            Validate();

            this.SourceItem1.ReadOnly = false;
            this.SourceItem2.ReadOnly = false;
            for (var i = 0; i < this.SourceItem2.Rows.Count; i++)
            {
                var item1 = this.SourceItem1.Rows[i];
                var item2 = this.SourceItem2.Rows[i];
                item1.CopyFrom(item2);
            }
            this.SourceItem1.ReadOnly = true;
            this.SourceItem2.ReadOnly = true;
            this.IsResolved = true;

            void Validate()
            {
                if (this.DiffState != DiffState.Inserted)
                    throw new Exception();
            }
        }

        private void ValidateResolveInternal()
        {
            if (this.DiffState == DiffState.Modified)
            {
                if (this.SourceItem1.TableName != this.SourceItem2.TableName)
                    throw new Exception();
                if (this.SourceItem1.Tags != this.SourceItem2.Tags)
                    throw new Exception();
                if (this.SourceItem1.Comment != this.SourceItem2.Comment)
                    throw new Exception();
                foreach (var item in this.itemList)
                {
                    if (item.DiffState1 == DiffState.Imaginary && item.DiffState2 == DiffState.Imaginary)
                        continue;
                    if (item.DiffState1 != DiffState.Unchanged)
                        throw new Exception(Resources.Exception_UnresolvedProblemsExist);
                    if (item.DiffState2 != DiffState.Unchanged)
                        throw new Exception(Resources.Exception_UnresolvedProblemsExist);
                }
            }
        }

        private void ValidateResolve()
        {
            this.ValidateResolveInternal();
            foreach (var item in this.Childs)
            {
                if (item.IsResolved == false)
                    throw new Exception(Resources.Exception_UnresolvedChildTablesExist);
            }
        }

        private bool VerifyModified()
        {
            if (this.SourceItem1.TableName != this.SourceItem2.TableName)
                return true;
            if (this.SourceItem1.Tags != this.SourceItem2.Tags)
                return true;
            if (this.SourceItem1.Comment != this.SourceItem2.Comment)
                return true;

            foreach (var item in this.SourceItem1.Rows)
            {
                if (DiffUtility.GetDiffState(item) != DiffState.Unchanged)
                    return true;
            }

            foreach (var item in this.SourceItem2.Rows)
            {
                if (DiffUtility.GetDiffState(item) != DiffState.Unchanged)
                    return true;
            }
            return false;
        }

        private void SyncTemplate(CremaTemplate template, CremaDataTable dataTable)
        {
            dataTable.TableName = template.TableName;
            dataTable.Tags = template.Tags;
            dataTable.Comment = template.Comment;
            foreach (var item in template.Columns)
            {
                if (item.InternalObject[DiffUtility.DiffIDKey] is Guid columnID && object.Equals(columnID, item.ColumnID) == false)
                {
                    var destColumn = dataTable.Columns[columnID];
                    destColumn.ColumnID = item.ColumnID;
                    destColumn.ColumnName = item.Name;
                    destColumn.ExtendedProperties[nameof(destColumn.IsKey)] = item.IsKey;
                    destColumn.ExtendedProperties[nameof(destColumn.DataTypeName)] = item.DataTypeName;
                    destColumn.ExtendedProperties[nameof(destColumn.DefaultValue)] = item.DefaultValue;
                    destColumn.ExtendedProperties[nameof(destColumn.AutoIncrement)] = item.AutoIncrement;
                    destColumn.ExtendedProperties[nameof(destColumn.AllowDBNull)] = item.AllowNull;
                    destColumn.ExtendedProperties[nameof(destColumn.ReadOnly)] = item.ReadOnly;
                    destColumn.ExtendedProperties[nameof(destColumn.Unique)] = item.Unique;
                    destColumn.Tags = item.Tags;
                    destColumn.Comment = item.Comment;
                    destColumn.ModificationInfo = item.ModificationInfo;
                    destColumn.CreationInfo = item.CreationInfo;
                }
            }

            foreach (var item in dataTable.Columns.ToArray())
            {
                if (template.Columns.Contains(item.ColumnID) == false)
                {
                    dataTable.Columns.Remove(item);
                }
            }

            foreach (var item in template.Columns)
            {
                if (dataTable.Columns.Contains(item.ColumnID) == false)
                {
                    var destColumn = dataTable.Columns.Add();
                    destColumn.ColumnID = item.ColumnID;
                    destColumn.ColumnName = item.Name;
                    destColumn.ExtendedProperties[nameof(destColumn.IsKey)] = item.IsKey;
                    destColumn.ExtendedProperties[nameof(destColumn.DataTypeName)] = item.DataTypeName;
                    destColumn.ExtendedProperties[nameof(destColumn.DefaultValue)] = item.DefaultValue;
                    destColumn.ExtendedProperties[nameof(destColumn.AutoIncrement)] = item.AutoIncrement;
                    destColumn.ExtendedProperties[nameof(destColumn.AllowDBNull)] = item.AllowNull;
                    destColumn.ExtendedProperties[nameof(destColumn.ReadOnly)] = item.ReadOnly;
                    destColumn.ExtendedProperties[nameof(destColumn.Unique)] = item.Unique;
                    destColumn.Tags = item.Tags;
                    destColumn.Comment = item.Comment;
                    destColumn.ModificationInfo = item.ModificationInfo;
                    destColumn.CreationInfo = item.CreationInfo;
                }
                else
                {
                    if (item.InternalObject[DiffUtility.DiffIDKey] is Guid columnID && object.Equals(columnID, item.ColumnID) == true)
                    {
                        var destColumn = dataTable.Columns[item.ColumnID];
                        destColumn.ColumnName = item.Name;
                        destColumn.ExtendedProperties[nameof(destColumn.IsKey)] = item.IsKey;
                        destColumn.ExtendedProperties[nameof(destColumn.DataTypeName)] = item.DataTypeName;
                        destColumn.ExtendedProperties[nameof(destColumn.DefaultValue)] = item.DefaultValue;
                        destColumn.ExtendedProperties[nameof(destColumn.AutoIncrement)] = item.AutoIncrement;
                        destColumn.ExtendedProperties[nameof(destColumn.AllowDBNull)] = item.AllowNull;
                        destColumn.ExtendedProperties[nameof(destColumn.ReadOnly)] = item.ReadOnly;
                        destColumn.ExtendedProperties[nameof(destColumn.Unique)] = item.Unique;
                        destColumn.Tags = item.Tags;
                        destColumn.Comment = item.Comment;
                        destColumn.ModificationInfo = item.ModificationInfo;
                        destColumn.CreationInfo = item.CreationInfo;
                    }
                }
            }

            for (var i = template.Columns.Count - 1; i >= 0; i--)
            {
                dataTable.Columns[template.Columns[i].ColumnID].Index = i;
            }

            var attributes = dataTable.Attributes.ToArray();
            var relationColumn = dataTable.ColumnRelation;
            var columns = dataTable.Columns.ToArray();

            this.Validate(dataTable);
        }

        private void SyncTemplate()
        {
            if (this.Template.DiffState == DiffState.Inserted)
            {
                this.SyncTemplate(this.Template.SourceItem1, this.SourceItem1);
            }
            else if (this.Template.DiffState == DiffState.Deleted)
            {
                this.SyncTemplate(this.Template.SourceItem2, this.SourceItem2);
            }
            else if (this.Template.DiffState == DiffState.Modified)
            {
                this.SyncTemplate(this.Template.SourceItem1, this.SourceItem1);
                this.SyncTemplate(this.Template.SourceItem2, this.SourceItem2);
            }
        }

        private void Template_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is DiffTemplate diffTemplate)
            {
                if (e.PropertyName == nameof(this.Template.IsResolved) && diffTemplate.IsResolved == true)
                {
                    if (this.Template == diffTemplate)
                    {
                        this.SyncTemplate();
                    }

                    this.unresolvedItemList.Remove(sender);
                    if (this.unresolvedItemList.Any() == false)
                    {
                        if (this.parent == null)
                            this.Diff();
                    }
                }
            }
        }

        private void DiffSource1_RowChanged(object sender, CremaDataRowChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Detached)
                return;

            var index = e.Row.Index;
            if (index >= this.itemList.Count)
            {
                this.itemList.Add(new DiffDataRow(this, index));
            }
            else
            {
                var item = this.itemList[index];
                item.Item1 = e.Row;
                item.Update();
            }
        }

        private void DiffSource2_RowChanged(object sender, CremaDataRowChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Detached)
                return;

            var index = e.Row.Index;
            if (index >= this.itemList.Count)
            {
                this.itemList.Add(new DiffDataRow(this, index));
            }
            else
            {
                var item = this.itemList[index];
                item.Item2 = e.Row;
                item.Update();
            }
        }

        private static CremaDataTable CreateExportTable(CremaDataSet exportSet, CremaDataTable diffTable)
        {
            var dataSet = diffTable.DataSet;
            //var query = from item in diffTable.Columns
            //            where item.ExtendedProperties.Contains(nameof(item.DataTypeName)) == true
            //            let dataTypeName = item.ExtendedProperties[nameof(item.DataTypeName)] as string
            //            join type in dataSet.Types on dataTypeName equals type.Path
            //            select type;

            //var types = query.Distinct();
            //foreach (var item in types)
            //{
            //    if (exportSet.Types.Contains(item.Name, item.CategoryPath) == false)
            //    {
            //        item.CopyTo(exportSet);
            //    }
            //}

            var exportTable = diffTable.Parent == null ? exportSet.Tables.Add(diffTable.Name) : exportSet.Tables[diffTable.ParentName].Childs.Add(diffTable.TableName);
            exportTable.InternalTableID = diffTable.TableID;
            exportTable.InternalTags = diffTable.Tags;
            exportTable.InternalComment = diffTable.Comment;
            exportTable.InternalCreationInfo = diffTable.CreationInfo;
            exportTable.InternalModificationInfo = diffTable.ModificationInfo;
            exportTable.InternalContentsInfo = diffTable.ContentsInfo;

            var keyList = new List<CremaDataColumn>(diffTable.PrimaryKey.Length);
            foreach (var item in diffTable.Columns)
            {
                var exportColumn = exportTable.Columns.Add(item.ColumnName);
                if (item.ExtendedProperties[nameof(item.IsKey)] is bool isKey && isKey == true)
                {
                    keyList.Add(exportColumn);
                }
                exportColumn.InternalDataTypeName = (string)item.ExtendedProperties[nameof(item.DataTypeName)];
                exportColumn.InternalDefaultValue = item.ExtendedProperties[nameof(item.DefaultValue)];
                exportColumn.InternalAutoIncrement = (bool)item.ExtendedProperties[nameof(item.AutoIncrement)];
                exportColumn.InternalAllowDBNull = (bool)item.ExtendedProperties[nameof(item.AllowDBNull)];
                exportColumn.InternalReadOnly = (bool)item.ExtendedProperties[nameof(item.ReadOnly)];
                exportColumn.InternalUnique = (bool)item.ExtendedProperties[nameof(item.Unique)];
                exportColumn.InternalTags = item.Tags;
                exportColumn.InternalComment = item.Comment;
                exportColumn.InternalModificationInfo = item.ModificationInfo;
                exportColumn.InternalCreationInfo = item.CreationInfo;
            }

            exportTable.PrimaryKey = keyList.ToArray();

            foreach (var item in diffTable.Childs)
            {
                CreateExportTable(exportSet, item);
            }

            return exportTable;
        }

        private static void ExportTable(CremaDataSet dataSet, CremaDataTable diffTable)
        {
            var dataTable = dataSet.Tables[diffTable.Name];
            var internalTable = dataTable.InternalObject;
            foreach (var item in diffTable.Rows)
            {
                if (DiffUtility.GetDiffState(item) == DiffState.Imaginary)
                    continue;

                internalTable.OmitSignatureDate = true;
                var sourceRow = item.InternalObject;
                var dataRow = internalTable.NewRow();
                for (var i = 0; i < internalTable.Columns.Count; i++)
                {
                    var dataColumn = internalTable.Columns[i];
                    dataRow[dataColumn] = sourceRow[dataColumn.ColumnName];
                }
                internalTable.Rows.Add(dataRow);
                internalTable.OmitSignatureDate = false;
            }

            foreach (var item in diffTable.Childs)
            {
                ExportTable(dataSet, item);
            }
        }

        internal static CremaDataTable Create(CremaDataSet dataSet, string tableName)
        {
            var dataTable = dataSet.Tables.Add(tableName);

            dataTable.Attributes.Add(DiffUtility.DiffStateKey, typeof(string), DBNull.Value);
            dataTable.Attributes.Add(DiffUtility.DiffFieldsKey, typeof(string), DBNull.Value);
            dataTable.Attributes.Add(DiffUtility.DiffEnabledKey, typeof(bool), true);

            return dataTable;
        }

        internal static CremaDataTable Create(CremaDataTable dataTable, string childName)
        {
            var childTable = dataTable.Childs.Add(childName);

            childTable.Attributes.Add(DiffUtility.DiffStateKey, typeof(string), DBNull.Value);
            childTable.Attributes.Add(DiffUtility.DiffFieldsKey, typeof(string), DBNull.Value);
            childTable.Attributes.Add(DiffUtility.DiffEnabledKey, typeof(bool), true);

            return childTable;
        }

        internal static CremaDataTable Inherit(CremaDataTable dataTable, string tableName)
        {
            var dataSet = dataTable.DataSet;

            var derivedTable = dataSet.Tables.Add(tableName);

            derivedTable.Attributes.Add(DiffUtility.DiffStateKey, typeof(string), DBNull.Value);
            derivedTable.Attributes.Add(DiffUtility.DiffFieldsKey, typeof(string), DBNull.Value);
            derivedTable.Attributes.Add(DiffUtility.DiffEnabledKey, typeof(bool), true);

            derivedTable.InternalTableID = dataTable.TableID;
            derivedTable.Comment = dataTable.Comment;
            derivedTable.Tags = dataTable.Tags;

            foreach (var item in dataTable.Columns)
            {
                var derivedColumn = derivedTable.Columns.Add();
                derivedColumn.CopyFrom(item);
            }

            foreach (var item in dataTable.Childs)
            {
                var derivedChild = derivedTable.Childs.Add(item.TableName);
                derivedChild.Attributes.Add(DiffUtility.DiffStateKey, typeof(string), DBNull.Value);
                derivedChild.Attributes.Add(DiffUtility.DiffFieldsKey, typeof(string), DBNull.Value);
                derivedChild.Attributes.Add(DiffUtility.DiffEnabledKey, typeof(bool), true);
                derivedChild.InternalTableID = item.TableID;
                derivedChild.Comment = item.Comment;
                derivedChild.Tags = item.Tags;

                foreach (var i in item.Columns)
                {
                    var derivedColumn = derivedChild.Columns.Add();
                    derivedColumn.CopyFrom(i);
                }
            }

            derivedTable.TemplatedParent = dataTable;

            return derivedTable;
        }

        internal void DiffTemplate()
        {
            foreach (var item in this.Childs)
            {
                item.DiffTemplateInternal();

                if (this.TemplatedParent == null)
                {
                    if (item.Template != null && item.Template.IsResolved == false)
                    {
                        this.unresolvedItemList.Add(item.Template);
                        item.Template.PropertyChanged += Template_PropertyChanged;
                    }
                    else
                    {

                    }
                }
            }
            this.DiffTemplateInternal();
        }

        internal void DiffTemplateInternal()
        {
            if (this.TemplatedParent == null)
            {
                this.Template.Diff();
                if (this.Template.IsResolved == false)
                {
                    this.unresolvedItemList.Add(this.Template);
                    this.Template.PropertyChanged += Template_PropertyChanged;
                }
                else
                {
                    this.SyncTemplate();
                }

                if (this.parent != null && this.parent.Template.IsResolved == false)
                {
                    this.unresolvedItemList.Add(this.parent.Template);
                    this.parent.Template.PropertyChanged += Template_PropertyChanged;
                }
            }
            else
            {
                if (this.TemplatedParent.Template.IsResolved == false)
                {
                    this.unresolvedItemList.Add(this.TemplatedParent.Template);
                    this.TemplatedParent.Template.PropertyChanged += Template_PropertyChanged;
                }
            }
            this.SourceItem1.IsDiffMode = true;
            this.SourceItem2.IsDiffMode = true;
            this.SourceItem1.InternalObject.OmitSignatureDate = true;
            this.SourceItem2.InternalObject.OmitSignatureDate = true;
        }

        internal void Diff()
        {
            this.DiffInternal();
            foreach (var item in this.Childs)
            {
                item.DiffInternal();
            }
        }

        internal void DiffInternal()
        {
            this.InitializeRows();

            if (this.dataTable1 == null)
            {
                this.SourceItem1.ReadOnly = true;
                this.SourceItem2.ReadOnly = true;
                this.DiffState = DiffState.Inserted;
                this.IsResolved = this.unresolvedItemList.Any() == false;
            }
            else if (this.dataTable2 == null)
            {
                this.SourceItem1.ReadOnly = true;
                this.SourceItem2.ReadOnly = true;
                this.DiffState = DiffState.Deleted;
                this.IsResolved = this.unresolvedItemList.Any() == false;
            }
            else if (this.VerifyModified() == true)
            {
                this.SourceItem1.ReadOnly = false;
                this.SourceItem2.ReadOnly = false;
                this.SourceItem1.IsDiffMode = true;
                this.SourceItem2.IsDiffMode = true;
                this.DiffState = DiffState.Modified;
                this.IsResolved = false;
            }
            else
            {
                this.DiffState = DiffState.Unchanged;
                this.IsResolved = this.unresolvedItemList.Any() == false;
            }
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsResolved)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.DiffState)));
        }

        internal HashSet<object> ItemSet { get; } = new();
    }
}

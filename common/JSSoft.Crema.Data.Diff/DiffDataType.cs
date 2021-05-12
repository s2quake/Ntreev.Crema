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
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace JSSoft.Crema.Data.Diff
{
    public sealed class DiffDataType : INotifyPropertyChanged
    {
        private readonly CremaDataType dataType1;
        private readonly CremaDataType dataType2;
        private readonly DiffMergeTypes mergeType;
        private readonly bool dummy1;
        private readonly bool dummy2;
        private readonly List<DiffDataTypeMember> itemList = new();
        private string header1;
        private string header2;

        [Obsolete]
        public DiffDataType(CremaDataType dataType1, CremaDataType dataType2, DiffMergeTypes mergeType)
        {
            this.SourceItem1 = dataType1 == null ? new CremaDataType() : new CremaDataType(dataType1.TypeName, dataType1.CategoryPath);
            this.SourceItem2 = dataType2 == null ? new CremaDataType() : new CremaDataType(dataType2.TypeName, dataType2.CategoryPath);
            this.mergeType = mergeType;
            this.SourceItem1.ExtendedProperties[typeof(DiffDataType)] = this;
            this.SourceItem2.ExtendedProperties[typeof(DiffDataType)] = this;
            this.SourceItem1.InternalIsFlag = dataType1.IsFlag;
            this.SourceItem1.InternalComment = dataType1.Comment;
            this.SourceItem1.InternalTypeID = dataType1.TypeID;
            this.SourceItem1.Tags = dataType1.Tags;
            this.SourceItem1.InternalCreationInfo = dataType1.CreationInfo;
            this.SourceItem1.InternalModificationInfo = dataType1.ModificationInfo;
            this.SourceItem2.InternalIsFlag = dataType2.IsFlag;
            this.SourceItem2.InternalComment = dataType2.Comment;
            this.SourceItem2.InternalTypeID = dataType2.TypeID;
            this.SourceItem2.Tags = dataType2.Tags;
            this.SourceItem2.InternalCreationInfo = dataType2.CreationInfo;
            this.SourceItem2.InternalModificationInfo = dataType2.ModificationInfo;

            this.dataType1 = dataType1;
            this.dataType2 = dataType2;
            //this.isSame = dataType1.HashValue != null && dataType1.HashValue == dataType2.HashValue;
        }

        internal DiffDataType(CremaDataType diffType1, CremaDataType diffType2, CremaDataType dataType1, CremaDataType dataType2)
        {
            this.SourceItem1 = diffType1;
            this.SourceItem2 = diffType2;
            this.SourceItem1.ExtendedProperties[typeof(DiffDataType)] = this;
            this.SourceItem2.ExtendedProperties[typeof(DiffDataType)] = this;
            this.SourceItem1.InternalIsFlag = dataType1 == null ? dataType2.IsFlag : dataType1.IsFlag;
            this.SourceItem1.InternalComment = dataType1 == null ? dataType2.Comment : dataType1.Comment;
            this.SourceItem1.InternalTypeID = dataType1 == null ? dataType2.TypeID : dataType1.TypeID;
            this.SourceItem1.InternalCreationInfo = dataType1 == null ? dataType2.CreationInfo : dataType1.CreationInfo;
            this.SourceItem1.InternalModificationInfo = dataType1 == null ? dataType2.ModificationInfo : dataType1.ModificationInfo;
            this.SourceItem2.InternalIsFlag = dataType2 == null ? dataType1.IsFlag : dataType2.IsFlag;
            this.SourceItem2.InternalComment = dataType2 == null ? dataType1.Comment : dataType2.Comment;
            this.SourceItem2.InternalTypeID = dataType2 == null ? dataType1.TypeID : dataType2.TypeID;
            this.SourceItem2.InternalCreationInfo = dataType2 == null ? dataType1.CreationInfo : dataType2.CreationInfo;
            this.SourceItem2.InternalModificationInfo = dataType2 == null ? dataType1.ModificationInfo : dataType2.ModificationInfo;
            this.dummy1 = this.SourceItem1.TypeName.StartsWith(DiffUtility.DiffDummyKey);
            this.dummy2 = this.SourceItem2.TypeName.StartsWith(DiffUtility.DiffDummyKey);

            this.dataType1 = dataType1;
            this.dataType2 = dataType2;
            //this.isSame = dataType1.HashValue != null && dataType1.HashValue == dataType2.HashValue;
        }

        public override string ToString()
        {
            if (this.SourceItem1.TypeName != this.SourceItem2.TypeName)
                return $"{this.SourceItem1.TypeName} => {this.SourceItem2.TypeName}";
            return this.SourceItem1.TypeName;
        }

        public void AcceptChanges()
        {
            this.Resolve();
        }

        public void RejectChanges()
        {
            this.SourceItem1.RejectChanges();
            this.SourceItem2.RejectChanges();
        }

        public bool HasChanges()
        {
            if (this.SourceItem1.HasChanges(true) == true)
                return true;
            if (this.SourceItem2.HasChanges(true) == true)
                return true;
            if (this.SourceItem1.TypeName != this.dataType1.TypeName)
                return true;
            if (this.SourceItem2.TypeName != this.dataType2.TypeName)
                return true;
            if (this.SourceItem1.Tags != this.dataType1.Tags)
                return true;
            if (this.SourceItem2.Tags != this.dataType2.Tags)
                return true;
            if (this.SourceItem1.IsFlag != this.dataType1.IsFlag)
                return true;
            if (this.SourceItem2.IsFlag != this.dataType2.IsFlag)
                return true;
            if (this.SourceItem1.Comment != this.dataType1.Comment)
                return true;
            if (this.SourceItem2.Comment != this.dataType2.Comment)
                return true;
            return false;
        }

        public void Resolve()
        {
            this.ValidateResolve();

            if (this.DiffState == DiffState.Modified)
            {
                this.SourceItem1.ReadOnly = false;
                this.SourceItem2.ReadOnly = false;
                this.SourceItem1.DeleteItems();
                this.SourceItem2.DeleteItems();
                this.SourceItem1.MemberChanged -= DiffSource1_MemberChanged;
                this.SourceItem2.MemberChanged -= DiffSource2_MemberChanged;
                this.SourceItem1.MemberDeleted -= DiffSource1_MemberDeleted;
                this.SourceItem2.MemberDeleted -= DiffSource2_MemberDeleted;
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

        public void Merge()
        {
            if (this.DiffState == DiffState.Modified)
            {
                this.MergeModify();
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

        public void Refresh()
        {
            if (this.IsResolved == true)
                return;
            this.SourceItem1.MemberChanged -= DiffSource1_MemberChanged;
            this.SourceItem2.MemberChanged -= DiffSource2_MemberChanged;
            this.SourceItem1.MemberDeleted -= DiffSource1_MemberDeleted;
            this.SourceItem2.MemberDeleted -= DiffSource2_MemberDeleted;
            for (var i = 0; i < this.itemList.Count; i++)
            {
                var item = this.itemList[i];
                item.Update();
            }
            this.SourceItem1.MemberChanged += DiffSource1_MemberChanged;
            this.SourceItem2.MemberChanged += DiffSource2_MemberChanged;
            this.SourceItem1.MemberDeleted += DiffSource1_MemberDeleted;
            this.SourceItem2.MemberDeleted += DiffSource2_MemberDeleted;
        }

        public CremaDataType SourceItem1 { get; private set; }

        public CremaDataType SourceItem2 { get; private set; }

        public string ItemName1
        {
            get
            {
                if (this.dummy1 == true)
                    return this.SourceItem1.TypeName.Replace(DiffUtility.DiffDummyKey, string.Empty);
                return this.SourceItem1.TypeName;
            }
            set
            {
                if (this.dummy1 == true && this.SourceItem2.TypeName != value)
                    this.SourceItem1.TypeName = DiffUtility.DiffDummyKey + value;
                else
                    this.SourceItem1.TypeName = value;
                this.InvokePropertyChangedEvent(nameof(this.ItemName1));
            }
        }

        public string ItemName2
        {
            get
            {
                if (this.dummy2 == true)
                    return this.SourceItem2.TypeName.Replace(DiffUtility.DiffDummyKey, string.Empty);
                return this.SourceItem2.TypeName;
            }
            set
            {
                if (this.dummy2 == true && this.SourceItem1.TypeName != value)
                    this.SourceItem2.TypeName = DiffUtility.DiffDummyKey + value;
                else
                    this.SourceItem2.TypeName = value;
                this.InvokePropertyChangedEvent(nameof(this.ItemName2));
            }
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

        public bool IsFlag1
        {
            get => this.SourceItem1.IsFlag;
            set
            {
                this.SourceItem1.IsFlag = value;
                this.InvokePropertyChangedEvent(nameof(this.IsFlag1));
            }
        }

        public bool IsFlag2
        {
            get => this.SourceItem2.IsFlag;
            set
            {
                this.SourceItem2.IsFlag = value;
                this.InvokePropertyChangedEvent(nameof(this.IsFlag2));
            }
        }

        public TagInfo Tags1
        {
            get => this.SourceItem1.Tags;
            set
            {
                this.SourceItem1.Tags = value;
                this.InvokePropertyChangedEvent(nameof(this.Tags1));
            }
        }

        public TagInfo Tags2
        {
            get => this.SourceItem2.Tags;
            set
            {
                this.SourceItem2.Tags = value;
                this.InvokePropertyChangedEvent(nameof(this.Tags2));
            }
        }

        public string Comment1
        {
            get => this.SourceItem1.Comment;
            set
            {
                this.SourceItem1.Comment = value;
                this.InvokePropertyChangedEvent(nameof(this.Comment1));
            }
        }

        public string Comment2
        {
            get => this.SourceItem2.Comment;
            set
            {
                this.SourceItem2.Comment = value;
                this.InvokePropertyChangedEvent(nameof(this.Comment2));
            }
        }

        public DiffDataSet DiffSet
        {
            get;
            internal set;
        }

        public IReadOnlyList<DiffDataTypeMember> Items => this.itemList;

        public bool IsResolved { get; private set; }

        public DiffState DiffState { get; private set; }

        public DiffMergeTypes MergeType
        {
            get
            {
                if (this.DiffSet != null)
                    return this.DiffSet.MergeType;
                return this.mergeType;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool VerifyModified()
        {
            if (this.SourceItem1.TypeName != this.SourceItem2.TypeName)
                return true;
            if (this.SourceItem1.IsFlag != this.SourceItem2.IsFlag)
                return true;
            if (this.SourceItem1.Comment != this.SourceItem2.Comment)
                return true;

            foreach (var item in this.SourceItem1.Items)
            {
                if (DiffUtility.GetDiffState(item) != DiffState.Unchanged)
                    return true;
            }

            foreach (var item in this.SourceItem2.Items)
            {
                if (DiffUtility.GetDiffState(item) != DiffState.Unchanged)
                    return true;
            }
            return false;
        }

        private void ValidateResolve()
        {
            if (this.DiffState == DiffState.Modified)
            {
                if (this.SourceItem1.TypeName != this.SourceItem2.TypeName)
                    throw new Exception();
                if (this.SourceItem1.IsFlag != this.SourceItem2.IsFlag)
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

        private void MergeDelete()
        {
            Validate();

            var dataSet = this.SourceItem1.DataSet;
            foreach (var item in this.SourceItem1.ReferencedColumns.ToArray())
            {
                item.CremaType = null;
            }

            this.SourceItem2.ReadOnly = false;
            for (var i = 0; i < this.SourceItem1.Items.Count; i++)
            {
                var item1 = this.SourceItem1.Items[i];
                var item2 = this.SourceItem2.Items[i];
                item2.CopyFrom(item1);
            }
            this.SourceItem2.ReadOnly = true;
            this.IsResolved = true;

            if (this.DiffSet != null && this.DiffSet.DataSet1.Types.Contains(this.SourceItem1))
                this.DiffSet.DataSet1.Types.Remove(this.SourceItem1);

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
            for (var i = 0; i < this.SourceItem2.Items.Count; i++)
            {
                var item1 = this.SourceItem1.Items[i];
                var item2 = this.SourceItem2.Items[i];
                item1.CopyFrom(item2);
            }
            this.SourceItem1.ReadOnly = true;
            this.SourceItem2.ReadOnly = true;

            if (this.DiffSet != null)
            {
                var dataSet = this.DiffSet.DataSet1;
                this.SourceItem1.CopyTo(this.DiffSet.DataSet1);
            }

            this.IsResolved = true;

            void Validate()
            {
                if (this.DiffState != DiffState.Inserted)
                    throw new Exception();
            }
        }

        private void MergeModify()
        {
            Validate();

            for (var i = this.itemList.Count - 1; i >= 0; i--)
            {
                var item = this.itemList[i];
                if (item.DiffState2 == DiffState.Modified)
                {
                    var item2 = item.Item2;
                    var item1 = item.GetTarget1();
                    item1.CopyFrom(item2);
                }
            }

            for (var i = 0; i < this.itemList.Count; i++)
            {
                var item = this.itemList[i];
                if (item.DiffState2 == DiffState.Inserted && item.DiffState1 == DiffState.Imaginary)
                {
                    var item2 = item.Item2;
                    var item1 = item.Item1;
                    item1.CopyFrom(item2);
                }
            }

            for (var i = 0; i < this.itemList.Count; i++)
            {
                var item = this.itemList[i];
                if (item.DiffState1 == DiffState.Deleted && item.DiffState2 == DiffState.Imaginary)
                {
                    var item1 = item.Item1;
                    item1.EmptyFields();
                }
            }

            foreach (var item in this.itemList)
            {
                if (item.DiffState1 != DiffState.Unchanged)
                    return;
                if (item.DiffState2 != DiffState.Unchanged)
                    return;
            }

            this.SourceItem1.ReadOnly = false;
            this.SourceItem2.ReadOnly = false;
            this.SourceItem1.DeleteItems();
            this.SourceItem2.DeleteItems();
            this.SourceItem1.AcceptChanges();
            this.SourceItem2.AcceptChanges();
            this.SourceItem1.IsDiffMode = false;
            this.SourceItem2.IsDiffMode = false;
            this.SourceItem1.ReadOnly = true;
            this.SourceItem2.ReadOnly = true;
            this.IsResolved = true;

            void Validate()
            {
                if (this.DiffState != DiffState.Modified)
                    throw new Exception();
            }
        }

        private void InitializeItems()
        {
            this.SourceItem1.MemberChanged -= DiffSource1_MemberChanged;
            this.SourceItem2.MemberChanged -= DiffSource2_MemberChanged;
            this.SourceItem1.MemberDeleted -= DiffSource1_MemberDeleted;
            this.SourceItem2.MemberDeleted -= DiffSource2_MemberDeleted;
            DiffInternalUtility.InitializeMembers(this.SourceItem1, this.SourceItem2, this.dataType1, this.dataType2);
            {
                this.itemList.Clear();
                this.itemList.Capacity = this.SourceItem1.Items.Count;
                for (var i = 0; i < this.SourceItem1.Items.Count; i++)
                {
                    var item = new DiffDataTypeMember(this, i);
                    this.itemList.Add(item);
                }

                for (var i = 0; i < this.itemList.Count; i++)
                {
                    var item = this.itemList[i];
                    item.Update();
                }

                this.SourceItem1.AcceptChanges();
                this.SourceItem2.AcceptChanges();
            }
            this.SourceItem1.MemberChanged += DiffSource1_MemberChanged;
            this.SourceItem2.MemberChanged += DiffSource2_MemberChanged;
            this.SourceItem1.MemberDeleted += DiffSource1_MemberDeleted;
            this.SourceItem2.MemberDeleted += DiffSource2_MemberDeleted;
        }

        private void DiffSource1_MemberChanged(object sender, CremaDataTypeMemberChangeEventArgs e)
        {
            if (e.Item.ItemState == DataRowState.Detached)
                return;

            if (e.PropertyName == string.Empty)
            {
                var index = e.Item.Index;
                if (index >= this.itemList.Count)
                {
                    this.itemList.Add(new DiffDataTypeMember(this, index));
                }
                else
                {
                    var item = this.itemList[index];
                    item.Item1 = e.Item;
                }
            }
        }

        private void DiffSource2_MemberChanged(object sender, CremaDataTypeMemberChangeEventArgs e)
        {
            if (e.Item.ItemState == DataRowState.Detached)
                return;

            if (e.PropertyName == string.Empty)
            {
                var index = e.Item.Index;
                if (index >= this.itemList.Count)
                {
                    this.itemList.Add(new DiffDataTypeMember(this, index));
                }
                else
                {
                    var item = this.itemList[index];
                    item.Item2 = e.Item;
                }
            }
        }

        private void DiffSource1_MemberDeleted(object sender, CremaDataTypeMemberChangeEventArgs e)
        {
            for (var i = 0; i < this.itemList.Count; i++)
            {
                if (this.itemList[i].Item1 == e.Item && this.itemList.Count >= this.SourceItem1.Items.Count)
                {
                    this.itemList.RemoveAt(i);
                    break;
                }
            }
        }

        private void DiffSource2_MemberDeleted(object sender, CremaDataTypeMemberChangeEventArgs e)
        {
            for (var i = 0; i < this.itemList.Count; i++)
            {
                if (this.itemList[i].Item2 == e.Item && this.itemList.Count >= this.SourceItem2.Items.Count)
                {
                    this.itemList.RemoveAt(i);
                    break;
                }
            }
        }

        private void InvokePropertyChangedEvent(params string[] propertyNames)
        {
            foreach (var item in propertyNames)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(item));
            }
        }

        internal static CremaDataType Create(CremaDataSet dataSet, string typeName)
        {
            var dataType = dataSet == null ? new CremaDataType(typeName) : dataSet.Types.Add(typeName);

            dataType.Attributes.Add(DiffUtility.DiffStateKey, typeof(string), $"{DiffState.Imaginary}");
            dataType.Attributes.Add(DiffUtility.DiffFieldsKey, typeof(string), DBNull.Value);
            dataType.Attributes.Add(DiffUtility.DiffIDKey, typeof(Guid), DBNull.Value);
            dataType.Attributes.Add(DiffUtility.DiffEnabledKey, typeof(bool), true);

            dataType.IsDiffMode = true;

            return dataType;
        }

        internal void Diff()
        {
            this.InitializeItems();

            if (this.dataType1 == null)
            {
                this.SourceItem1.IsDiffMode = false;
                this.SourceItem2.IsDiffMode = false;
                this.SourceItem1.ReadOnly = true;
                this.SourceItem2.ReadOnly = true;
                this.SourceItem1.SetDiffState(DiffState.Imaginary);
                this.SourceItem2.SetDiffState(DiffState.Inserted);
                this.DiffState = DiffState.Inserted;
                this.IsResolved = true;
            }
            else if (this.dataType2 == null)
            {
                this.SourceItem1.IsDiffMode = false;
                this.SourceItem2.IsDiffMode = false;
                this.SourceItem1.ReadOnly = true;
                this.SourceItem2.ReadOnly = true;
                this.SourceItem1.SetDiffState(DiffState.Deleted);
                this.SourceItem2.SetDiffState(DiffState.Imaginary);
                this.DiffState = DiffState.Deleted;
                this.IsResolved = true;
            }
            else if (this.VerifyModified() == true)
            {
                this.SourceItem1.ReadOnly = this.MergeType == DiffMergeTypes.ReadOnly1;
                this.SourceItem2.ReadOnly = this.MergeType == DiffMergeTypes.ReadOnly2;
                this.SourceItem1.SetDiffState(DiffState.Modified);
                this.SourceItem2.SetDiffState(DiffState.Modified);
                this.DiffState = DiffState.Modified;
                this.IsResolved = false;
            }
            else
            {
                this.SourceItem1.IsDiffMode = false;
                this.SourceItem2.IsDiffMode = false;
                this.SourceItem1.ReadOnly = true;
                this.SourceItem2.ReadOnly = true;
                this.SourceItem1.SetDiffState(DiffState.Unchanged);
                this.SourceItem2.SetDiffState(DiffState.Unchanged);
                this.DiffState = DiffState.Unchanged;
                this.IsResolved = true;
            }
            this.InvokePropertyChangedEvent(nameof(this.IsResolved), nameof(this.DiffState));
        }

        internal HashSet<object> ItemSet { get; } = new();
    }
}

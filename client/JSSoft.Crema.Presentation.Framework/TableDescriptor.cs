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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace JSSoft.Crema.Presentation.Framework
{
    // TODO: 필드를 없애고 속성으로 바꾸는 바람에 변수 갱신이 되지 않음.
    public class TableDescriptor : DescriptorBase, ITableDescriptor, ITableItemDescriptor, ILockableDescriptor, IPermissionDescriptor, IAccessibleDescriptor
    {
        private ITable table;
        private readonly object owner;
        private readonly ObservableCollection<TableDescriptor> childs = new ObservableCollection<TableDescriptor>();

        public TableDescriptor(Authentication authentication, ITableDescriptor descriptor)
            : this(authentication, descriptor, false)
        {

        }

        public TableDescriptor(Authentication authentication, ITableDescriptor descriptor, bool isSubscriptable)
            : this(authentication, descriptor, isSubscriptable, null)
        {

        }

        public TableDescriptor(Authentication authentication, ITableDescriptor descriptor, bool isSubscriptable, object owner)
            : base(authentication, descriptor.Target, descriptor, isSubscriptable)
        {
            this.table = descriptor.Target;
            this.owner = owner ?? this;
            this.Childs = new ReadOnlyObservableCollection<TableDescriptor>(this.childs);
        }

        public TableDescriptor(Authentication authentication, ITable table, DescriptorTypes descriptorTypes, object owner)
            : base(authentication, table, descriptorTypes)
        {
            this.table = table;
            this.owner = owner ?? this;
            this.table.Dispatcher.VerifyAccess();
            this.TableInfo = table.TableInfo;
            this.TableState = table.TableState;
            this.TableAttribute = TableAttribute.None;
            if (this.table.DerivedTables.Any() == true)
                this.TableAttribute |= TableAttribute.BaseTable;
            if (this.table.TemplatedParent != null)
                this.TableAttribute |= TableAttribute.DerivedTable;
            if (this.table.Parent != null)
                this.TableAttribute |= TableAttribute.HasParent;
            this.LockInfo = table.LockInfo;
            this.AccessInfo = table.AccessInfo;
            this.AccessType = table.GetAccessType(this.authentication);
            this.TemplateDescriptor = new TableTemplateDescriptor(authentication, table.Template, descriptorTypes, owner);
            this.ContentDescriptor = new TableContentDescriptor(authentication, table.Content, descriptorTypes, owner);
            this.Childs = new ReadOnlyObservableCollection<TableDescriptor>(this.childs);
            this.table.ExtendedProperties[this.owner] = this;

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
            {
                this.table.Deleted += Table_Deleted;
                this.table.LockChanged += Table_LockChanged;
                this.table.AccessChanged += Table_AccessChanged;
                this.table.TableInfoChanged += Table_TableInfoChanged;
                this.table.TableStateChanged += Table_TableStateChanged;
                this.table.DerivedTables.CollectionChanged += DerivedTables_CollectionChanged;
                this.ContentDescriptor.EditorsChanged += ContentDescriptor_EditorsChanged;
            }

            if (this.descriptorTypes.HasFlag(DescriptorTypes.IsRecursive) == true)
            {
                if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                {
                    this.table.Childs.CollectionChanged += Childs_CollectionChanged;
                }

                foreach (var item in this.table.Childs)
                {
                    var descriptor = new TableDescriptor(this.authentication, item, this.descriptorTypes, this.owner);
                    this.childs.Add(descriptor);
                }
            }
        }

        [DescriptorProperty]
        public string Name => this.TableInfo.Name;

        [DescriptorProperty]
        public string TableName => this.TableInfo.TableName;

        [DescriptorProperty]
        public string Path => this.TableInfo.CategoryPath + this.TableInfo.Name;

        [DescriptorProperty]
        public string DisplayName => this.TableInfo.Name;

        public ReadOnlyObservableCollection<TableDescriptor> Childs { get; }

        [DescriptorProperty]
        public TableInfo TableInfo { get; private set; } = TableInfo.Default;

        [DescriptorProperty]
        public TableState TableState { get; private set; }

        [DescriptorProperty]
        public TableAttribute TableAttribute { get; private set; }

        [DescriptorProperty]
        public LockInfo LockInfo { get; private set; } = LockInfo.Empty;

        [DescriptorProperty]
        public AccessInfo AccessInfo { get; private set; } = AccessInfo.Empty;

        [DescriptorProperty]
        public AccessType AccessType { get; private set; }

        [DescriptorProperty]
        public bool IsLocked => LockableDescriptorUtility.IsLocked(this.authentication, this);

        [DescriptorProperty]
        public bool IsLockInherited => LockableDescriptorUtility.IsLockInherited(this.authentication, this);

        [DescriptorProperty]
        public bool IsLockOwner => LockableDescriptorUtility.IsLockOwner(this.authentication, this);

        [DescriptorProperty]
        public bool IsPrivate => AccessibleDescriptorUtility.IsPrivate(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessInherited => AccessibleDescriptorUtility.IsAccessInherited(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessOwner => AccessibleDescriptorUtility.IsAccessOwner(this.authentication, this);

        [DescriptorProperty]
        public bool IsAccessMember => AccessibleDescriptorUtility.IsAccessMember(this.authentication, this);

        [DescriptorProperty]
        public bool IsBeingEdited => TableDescriptorUtility.IsBeingEdited(this.authentication, this);

        [DescriptorProperty]
        public bool IsContentEditor => TableDescriptorUtility.IsBeingEdited(this.authentication, this) && this.ContentDescriptor != null && this.ContentDescriptor.IsEditor;

        [DescriptorProperty]
        public bool IsContentOwner => TableDescriptorUtility.IsBeingEdited(this.authentication, this) && this.ContentDescriptor != null && this.ContentDescriptor.IsOwner;

        [DescriptorProperty]
        public bool IsBeingSetup => TableDescriptorUtility.IsBeingSetup(this.authentication, this);

        [DescriptorProperty]
        public bool IsTemplateEditor => TableDescriptorUtility.IsBeingSetup(this.authentication, this) && this.TemplateDescriptor != null && this.TemplateDescriptor.Editor == this.authentication.ID;

        [DescriptorProperty]
        public bool IsInherited => TableDescriptorUtility.IsInherited(this.authentication, this);

        [DescriptorProperty]
        public bool IsBaseTemplate => TableDescriptorUtility.IsBaseTemplate(this.authentication, this);

        [DescriptorProperty]
        public TableTemplateDescriptor TemplateDescriptor { get; }

        [DescriptorProperty]
        public TableContentDescriptor ContentDescriptor { get; }

        protected async override void OnDisposed(EventArgs e)
        {
            if (this.referenceTarget == null && this.table != null)
            {
                await this.table.Dispatcher.InvokeAsync(() =>
                {
                    if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                    {
                        this.table.Deleted -= Table_Deleted;
                        this.table.LockChanged -= Table_LockChanged;
                        this.table.AccessChanged -= Table_AccessChanged;
                        this.table.TableInfoChanged -= Table_TableInfoChanged;
                        this.table.TableStateChanged -= Table_TableStateChanged;
                        this.table.DerivedTables.CollectionChanged -= DerivedTables_CollectionChanged;
                    }

                    if (this.descriptorTypes.HasFlag(DescriptorTypes.IsRecursive) == true)
                    {
                        if (this.descriptorTypes.HasFlag(DescriptorTypes.IsSubscriptable) == true)
                        {
                            this.table.Childs.CollectionChanged -= Childs_CollectionChanged;
                        }
                    }
                });
            }
            base.OnDisposed(e);
        }

        private void Table_Deleted(object sender, EventArgs e)
        {
            this.table = null;
            this.Dispatcher.InvokeAsync(() =>
            {
                this.OnDisposed(EventArgs.Empty);
            });
        }

        private async void Table_LockChanged(object sender, EventArgs e)
        {
            this.LockInfo = this.table.LockInfo;
            this.AccessType = this.table.GetAccessType(this.authentication);
            await this.RefreshAsync();
        }

        private async void Table_AccessChanged(object sender, EventArgs e)
        {
            this.AccessInfo = this.table.AccessInfo;
            this.AccessType = this.table.GetAccessType(this.authentication);
            await this.RefreshAsync();
        }

        private async void Table_TableInfoChanged(object sender, EventArgs e)
        {
            this.TableInfo = this.table.TableInfo;
            if (this.table.DerivedTables.Any() == true)
                this.TableAttribute |= TableAttribute.BaseTable;
            else
                this.TableAttribute &= ~TableAttribute.BaseTable;
            if (this.table.TemplatedParent != null)
                this.TableAttribute |= TableAttribute.DerivedTable;
            else
                this.TableAttribute &= ~TableAttribute.DerivedTable;
            await this.RefreshAsync();
        }

        private async void Table_TableStateChanged(object sender, EventArgs e)
        {
            this.TableState = this.table.TableState;
            await this.RefreshAsync();
        }

        private async void ContentDescriptor_EditorsChanged(object sender, EventArgs e)
        {
            await this.RefreshAsync();
        }

        private async void DerivedTables_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.table.DerivedTables.Any() == true)
                this.TableAttribute |= TableAttribute.BaseTable;
            else
                this.TableAttribute &= ~TableAttribute.BaseTable;
            await this.RefreshAsync();
        }

        private void Childs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var descriptorList = new List<TableDescriptor>(e.NewItems.Count);
                        foreach (ITable item in e.NewItems)
                        {
                            if (item.ExtendedProperties.ContainsKey(this.owner) == true)
                            {
                                var descriptor = item.ExtendedProperties[this.owner] as TableDescriptor;
                                descriptorList.Add(descriptor);
                            }
                            else
                            {
                                var descriptor = new TableDescriptor(this.authentication, item, this.descriptorTypes, this.owner);
                                descriptorList.Add(descriptor);
                            }

                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var item in descriptorList)
                            {
                                this.childs.Add(item);
                            }
                        });
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var descriptorList = new List<TableDescriptor>(e.OldItems.Count);
                        foreach (ITable item in e.OldItems)
                        {
                            var descriptor = item.ExtendedProperties[this.owner] as TableDescriptor;
                            descriptorList.Add(descriptor);
                        }
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var item in descriptorList)
                            {
                                this.childs.Remove(item);
                            }
                        });
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {

                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            this.childs.Clear();
                        });
                    }
                    break;
            }
        }

        #region ITableItemDescriptor

        ITableItem ITableItemDescriptor.Target => this.table as ITableItem;

        #endregion

        #region ITableDescriptor

        ITable ITableDescriptor.Target => this.table;

        #endregion

        #region ILockableDescriptor

        ILockable ILockableDescriptor.Target => this.table as ILockable;

        #endregion

        #region IAccessibleDescriptor

        IAccessible IAccessibleDescriptor.Target => this.table as IAccessible;

        #endregion

        #region IPermissionDescriptor

        IPermission IPermissionDescriptor.Target => this.table as IPermission;

        #endregion
    }
}

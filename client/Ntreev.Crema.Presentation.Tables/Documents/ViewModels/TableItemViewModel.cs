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

using Caliburn.Micro;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Extensions;
using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels;
using System.Windows.Input;
using Ntreev.ModernUI.Framework;
using Ntreev.Crema.Presentation.Framework.Controls;
using System.ComponentModel.Composition;
using Xceed.Wpf.DataGrid;

namespace Ntreev.Crema.Presentation.Tables.Documents.ViewModels
{
    class TableItemViewModel : TableListItemBase, ITableDocumentItem, ITableContentDescriptor
    {
        private readonly TableContentDescriptor contentDescriptor;
        private readonly ICommand insertCommand;
        private CremaDataTable dataTable;
        private IDomain domain;
        private object selectedItem;
        private int selectedIndex;
        private string selectedColumn;
        private DataGridContext currentContext;

#pragma warning disable IDE0044 // 읽기 전용 한정자 추가
        [Import]
        private IServiceProvider serviceProvider = null;
#pragma warning restore IDE0044 // 읽기 전용 한정자 추가

        public TableItemViewModel(Authentication authentication, TableDescriptor descriptor, object owner)
            : base(authentication, descriptor, owner)
        {
            this.contentDescriptor = descriptor.ContentDescriptor;

            this.insertCommand = new DelegateCommand(async (p) => await this.NewRowAsync());

        }

        public async Task NewRowAsync()
        {
            var dialog = await NewRowViewModel.CreateAsync(this.authentication, this.descriptor.ContentDescriptor.Target as ITableContent);
            if (await dialog.ShowDialogAsync() == true)
            {
                this.SelectItem(dialog.Keys);
            }
        }

        public async Task SelectParentAsync()
        {
            var content = this.descriptor.ContentDescriptor.Target as ITableContent;
            var keysExpression = CremaDataRowUtility.GetKeysExpression(this.SelectedItem);
            var rows = await content.SelectAsync(this.authentication, keysExpression);
            var row = rows.FirstOrDefault();
            if (row == null)
                return;

            var dialog = await ChangeParentViewModel.CreateAsync(authentication, row);
            await dialog.ShowDialogAsync();
        }

        public async Task InsertManyAsync()
        {
                //var gridContext = DataGridControl.GetDataGridContext(this);
                //var gridControl = gridContext.DataGridControl as TableSourceDataGridControl;
                //var inserter = new DomainTextClipboardInserter(gridContext);

                //var textData = ClipboardUtility.GetData(true);

                //if (textData.Length == 1)
                //{
                //    //var textLine = textData.First();
                //    //var index = gridContext.CurrentColumn == null ? -1 : gridContext.VisibleColumns.IndexOf(gridContext.CurrentColumn);

                //    //for (var i = 0; i < textLine.Length; i++)
                //    //{
                //    //    var text = textLine[i];
                //    //    var column = gridContext.VisibleColumns[index + i];
                //    //    if (this.Cells[column] is TableSourceDataCell cell)
                //    //    {
                //    //        cell.BeginEdit();
                //    //        cell.EditingContent = text;
                //    //        cell.EndEdit();
                //    //    }
                //    //}
                //}
                //else
                //{
                //    inserter.Parse(textData);

                //    var domainRows = inserter.DomainRows;
                //    var domain = gridControl.Domain;
                //    var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;

                //    (gridContext.Items as INotifyCollectionChanged).CollectionChanged += Items_CollectionChanged;
                //    try
                //    {
                //        this.results = domain.Dispatcher.Invoke(() => domain.NewRow(authenticator, domainRows));
                //    }
                //    catch (Exception e)
                //    {
                //        this.results = null;
                //        (gridContext.Items as INotifyCollectionChanged).CollectionChanged -= Items_CollectionChanged;
                //        throw e;
                //    }
                //}
        }

        public override string DisplayName => this.descriptor.TableName;

        public bool IsReadOnly => this.domain == null;

        public CremaDataTable Source
        {
            get => this.dataTable;
            set
            {
                this.dataTable = value;
                this.NotifyOfPropertyChange(nameof(this.Source));
                this.NotifyOfPropertyChange(nameof(this.HasParent));
                this.NotifyOfPropertyChange(nameof(this.CanSelectParent));
            }
        }

        public object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
                this.NotifyOfPropertyChange(nameof(this.CanSelectParent));
            }
        }

        public int SelectedItemIndex
        {
            get => this.selectedIndex;
            set
            {
                this.selectedIndex = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItemIndex));
            }
        }

        public string SelectedColumn
        {
            get => this.selectedColumn;
            set
            {
                this.selectedColumn = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedColumn));
            }
        }

        public bool HasParent => this.dataTable != null && this.dataTable.ParentName != string.Empty;

        public IDomain Domain
        {
            get => this.domain;
            set
            {
                this.domain = value;
                this.NotifyOfPropertyChange(nameof(this.Domain));
                this.NotifyOfPropertyChange(nameof(this.IsReadOnly));
            }
        }

        public bool CanSelectParent => this.selectedItem != null;

        public virtual IEnumerable<IToolBarItem> ToolBarItems
        {
            get
            {
                if (this.serviceProvider == null)
                    return Enumerable.Empty<IToolBarItem>();
                return ToolBarItemUtility.GetToolBarItems(this, this.serviceProvider);
            }
        }

        protected override void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);

            this.dataTable = null;
            this.NotifyOfPropertyChange(nameof(this.Source));
            this.NotifyOfPropertyChange(nameof(this.Domain));
        }

        private void SelectItem(object[] keys)
        {
            try
            {
                var view = this.dataTable.DefaultView;
                for (var i = view.Count - 1; i >= 0; i--)
                {
                    var item = view[i];
                    var itemKeys = CremaDataRowUtility.GetKeys(item);
                    if (itemKeys.SequenceEqual(keys) == true)
                    {
                        this.SelectedItem = item;
                        this.SelectedColumn = null;
                        break;
                    }
                }
            }
            catch
            {

            }
        }

        #region ITableDocumentItem

        ITable ITableDocumentItem.Target => this.Target as ITable;

        #endregion

        #region ITableContentDescriptor

        string ITableContentDescriptor.DisplayName => this.contentDescriptor.DisplayName;

        bool ITableContentDescriptor.IsModified => this.contentDescriptor.IsModified;

        DomainAccessType ITableContentDescriptor.AccessType => this.contentDescriptor.AccessType;

        IDomain ITableContentDescriptor.TargetDomain => this.contentDescriptor.TargetDomain;

        ITableContent ITableContentDescriptor.Target => this.contentDescriptor.Target as ITableContent;

        #endregion

        #region Commands

        public ICommand InsertCommand => this.insertCommand;

        #endregion
    }
}

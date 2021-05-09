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

using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.DataGrid.Controls;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.DataGrid;

namespace JSSoft.Crema.Presentation.Framework.Controls
{
    public class DomainDataCell : ModernDataCell
    {
        private static readonly DependencyPropertyKey DisplayContentPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DisplayContent), typeof(object), typeof(DomainDataCell), new UIPropertyMetadata(null));
        public static readonly DependencyProperty DisplayContentProperty = DisplayContentPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsContentUpdatingPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsContentUpdating), typeof(bool), typeof(DomainDataCell), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsContentUpdatingProperty = IsContentUpdatingPropertyKey.DependencyProperty;

        public static readonly DependencyProperty UserIDProperty =
            DependencyProperty.Register("UserID", typeof(string), typeof(DomainDataCell));

        public static readonly DependencyProperty UserBrushProperty =
            DependencyProperty.Register(nameof(UserBrush), typeof(Brush), typeof(DomainDataCell));

        public static readonly DependencyProperty HasUserProperty =
            DependencyProperty.Register(nameof(HasUser), typeof(bool), typeof(DomainDataCell));

        public static readonly DependencyProperty IsUserEditingProperty =
            DependencyProperty.Register(nameof(IsUserEditing), typeof(bool), typeof(DomainDataCell));

        public static readonly DependencyProperty IsClientAloneProperty =
            DependencyProperty.Register(nameof(IsClientAlone), typeof(bool), typeof(DomainDataCell));

        private readonly DomainDataUserCollection users = new();

        private DomainDataRow parentRow;

        public DomainDataCell()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, this.Reset_Execute, this.Reset_CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, this.PasteFromClipboard_Execute, this.PasteFromClipboard_CanExecute));
            this.users.CollectionChanged += Users_CollectionChanged;
        }

        public Task ResetAsync()
        {
            var domain = this.GridControl.Domain;
            var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
            var item = this.DataContext;
            var fieldName = this.FieldName;
            return domain.SetRowAsync(authenticator, item, fieldName, DBNull.Value);
        }

        public async void PasteFromClipboard()
        {
            if (Clipboard.ContainsText() == false)
                return;

            var gridContext = DataGridControl.GetDataGridContext(this);
            var gridControl = gridContext.DataGridControl as DomainDataGridControl;

            var parser = new DomainTextClipboardPaster(gridContext);
            parser.Parse(ClipboardUtility.GetData());
            var rowInfos = parser.DomainRows;
            var domain = gridControl.Domain;
            var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
            await domain.SetRowAsync(authenticator, rowInfos);
            parser.SelectRange();
        }

        public object DisplayContent
        {
            get => (object)this.GetValue(DisplayContentProperty);
            private set => this.SetValue(DisplayContentPropertyKey, value);
        }

        public bool IsContentUpdating
        {
            get => (bool)this.GetValue(IsContentUpdatingProperty);
            private set => this.SetValue(IsContentUpdatingPropertyKey, value);
        }

        public IEnumerable Users => this.users;

        public Brush UserBrush
        {
            get => (Brush)this.GetValue(UserBrushProperty);
            private set => this.SetValue(UserBrushProperty, value);
        }

        public bool HasUser
        {
            get => (bool)this.GetValue(HasUserProperty);
            private set => this.SetValue(HasUserProperty, value);
        }

        public bool IsUserEditing
        {
            get => (bool)this.GetValue(IsUserEditingProperty);
            private set => this.SetValue(IsUserEditingProperty, value);
        }

        public bool IsClientAlone
        {
            get => (bool)this.GetValue(IsClientAloneProperty);
            private set => this.SetValue(IsClientAloneProperty, value);
        }

        public bool CanReset
        {
            get
            {
                if (this.ReadOnly == true)
                    return false;

                if (this.IsBeingEdited == true)
                    return false;

                return this.IsSingleSelection;
            }
        }

        public bool CanPaste
        {
            get
            {
                if (this.IsBeingEdited == true)
                    return false;
                if (this.ReadOnly == true)
                    return false;
                if (Clipboard.ContainsText() == false)
                    return false;

                return this.IsSingleSelection;
            }
        }

        public new DomainDataGridControl GridControl => (DomainDataGridControl)base.GridControl;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override async void OnEditBegun()
        {
            base.OnEditBegun();
            await this.RequestBeginEditAsync();
        }

        protected override async void OnEditEnded()
        {
            var editingContent = this.EditingContent;
            var content = this.Content;
            base.OnEditEnded();
            var parentRow = this.ParentRow as DomainDataRow;
            if (parentRow.IsBeginEnding == false)
                parentRow.CancelEdit();
            if (editingContent != content)
            {
                await this.RequestSetRowAsync(editingContent);
            }
            await this.RequestEditEndAsync();
        }

        protected override void OnEditEnding(CancelRoutedEventArgs e)
        {
            base.OnEditEnding(e);
        }

        protected override async void OnEditCanceled()
        {
            base.OnEditCanceled();
            var parentRow = this.ParentRow as DomainDataRow;
            if (parentRow.IsBeginEnding == false)
                this.ParentRow.CancelEdit();
            this.Focus();
            await this.RequestEditEndAsync();

        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            if (this.IsContentUpdating == true)
            {
                this.IsContentUpdating = false;
            }

            if (object.Equals(oldContent, newContent) == false && this.ParentRow is DomainDataRow dataRow)
            {
                dataRow.UpdateKeys();
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        protected override void InitializeCore(DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn)
        {
            base.InitializeCore(dataGridContext, parentRow, parentColumn);

            if (this.parentRow != null)
                this.parentRow.UserInfos.CollectionChanged -= UserInfos_CollectionChanged;

            this.parentRow = parentRow as DomainDataRow;

            if (this.parentRow != null)
                this.parentRow.UserInfos.CollectionChanged += UserInfos_CollectionChanged;
        }

        protected override void PrepareContainer(DataGridContext dataGridContext, object item)
        {
            base.PrepareContainer(dataGridContext, item);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (this.IsBeingEdited == true && e.NewFocus != null)
            {
                var gridContext = DataGridControl.GetDataGridContext(e.NewFocus as DependencyObject);
                if (gridContext == null)
                {
                    this.EndEdit();
                }
            }
        }

        private void UserInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.users.Clear();
            }
            else
            {
                if (e.NewItems != null)
                {
                    foreach (DomainDataUser item in e.NewItems)
                    {
                        if (item.Location.ColumnName == this.FieldName)
                        {
                            if (item.UserID == this.parentRow.UserID)
                                this.users.Insert(0, item);
                            else
                                this.users.Add(item);
                        }
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (DomainDataUser item in e.OldItems)
                    {
                        if (item.Location.ColumnName == this.FieldName)
                        {
                            this.users.Remove(item.UserID);
                        }
                    }
                }
            }
        }

        private void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                if (this.users.Any() == true)
                {
                    var domainUser = this.users.First();
                    this.UserBrush = domainUser.Background;
                    this.HasUser = true;
                    this.IsUserEditing = domainUser.UserID == this.parentRow.UserID ? false : domainUser.IsBeingEdited;
                    if (this.users.Count == 1 && domainUser.UserID == this.parentRow.UserID)
                        this.IsClientAlone = true;
                    else
                        this.IsClientAlone = false;
                }
                else
                {
                    this.UserBrush = null;
                    this.HasUser = false;
                    this.IsUserEditing = false;
                    this.IsClientAlone = false;
                }
            });
        }

        private void PasteFromClipboard_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanPaste;
        }

        private void PasteFromClipboard_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            this.RequestPasteFromClipboard();
        }

        private void Reset_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanReset;
        }

        private async void Reset_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (await AppMessageBox.ShowQuestion(Properties.Resources.Message_ConfirmToResetField) == false)
                return;

            this.RequestReset();
        }

        private async Task RequestBeginEditAsync()
        {
            try
            {
                var domain = this.GridControl.Domain;
                if (domain != null)
                {
                    var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                    var item = this.DataContext;
                    var fieldName = this.FieldName;
                    await domain.BeginEditAsync(authenticator, item, fieldName);
                }
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        private async Task RequestEditEndAsync()
        {
            try
            {
                var domain = this.GridControl.Domain;
                if (domain == null)
                    return;

                var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                if (domain != null)
                    await domain.EndUserEditAsync(authenticator);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        private async void RequestReset()
        {
            try
            {
                await this.ResetAsync();
            }
            catch (Exception ex)
            {
                await AppMessageBox.ShowErrorAsync(ex);
            }
        }

        private async void RequestPasteFromClipboard()
        {
            try
            {
                this.PasteFromClipboard();
            }
            catch (Exception ex)
            {
                await AppMessageBox.ShowErrorAsync(ex);
            }
        }

        private async Task RequestSetRowAsync(object value)
        {
            var isReadOnly = this.ReadOnly;
            try
            {
                var domain = this.GridControl.Domain;
                var item = this.DataContext;
                var fieldName = this.FieldName;
                var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
                this.ReadOnly = true;
                this.DisplayContent = value;
                this.EditingContent = value;
                this.IsContentUpdating = true;
                await domain.SetRowAsync(authenticator, item, fieldName, value);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                this.DisplayContent = null;
                this.EditingContent = this.Content;
                this.IsContentUpdating = false;
                this.ReadOnly = isReadOnly;
            }
            finally
            {
                this.ReadOnly = isReadOnly;
            }
        }

        private bool IsSingleSelection
        {
            get
            {
                if (this.GridControl.SelectedContexts.Count != 1)
                    return false;

                var itemIndex = this.GridContext.Items.IndexOf(this.DataContext);
                var columnIndex = this.GridContext.VisibleColumns.IndexOf(this.ParentColumn);

                foreach (var item in this.GridContext.SelectedCellRanges)
                {
                    if (item.ColumnRange.Length != 1)
                        return false;
                    if (item.ItemRange.Length != 1)
                        return false;
                    if (item.ColumnRange.StartIndex != columnIndex)
                        return false;
                    if (item.ItemRange.StartIndex != itemIndex)
                        return false;
                }
                return true;
            }
        }
    }
}

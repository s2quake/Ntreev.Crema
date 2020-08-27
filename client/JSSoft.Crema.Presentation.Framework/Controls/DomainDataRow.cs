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
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.DataGrid.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xceed.Wpf.DataGrid;

namespace JSSoft.Crema.Presentation.Framework.Controls
{
    public class DomainDataRow : ModernDataRow
    {
        private IDomain domain;
        private object[] keys;
        private string tableName;

        public DomainDataRow()
        {
            this.CommandBindings.Insert(0, new CommandBinding(ApplicationCommands.Delete, this.Delete_Execute, this.Delete_CanExecute));
        }

        public Task DeleteAsync()
        {
            var domain = this.GridControl.Domain;
            var authenticator = domain.GetService(typeof(Authenticator)) as Authenticator;
            var item = this.DataContext;
            return domain.RemoveRowAsync(authenticator, item);
        }

        public new DomainDataGridControl GridControl => (DomainDataGridControl)base.GridControl;

        public bool CanDelete
        {
            get
            {
                if (this.ReadOnly == true)
                    return false;

                if (this.GridControl.SelectedContexts.Count > 1)
                    return false;

                if (this.DataContext is System.Data.DataRowView == false)
                    return false;

                if (this.IsBeingEdited == true)
                    return false;

                var index = this.GridContext.Items.IndexOf(this.DataContext);

                foreach (var item in this.GridContext.SelectedCellRanges)
                {
                    if (item.ItemRange.Length != 1)
                        return false;
                    if (item.ItemRange.StartIndex != index)
                        return false;
                }

                if (this.GridContext.GetSelectedColumns().Count() != this.GridContext.VisibleColumns.Count)
                    return false;

                return true;
            }
        }

        // null 예외 발생함. 연속으로
        protected async override void PrepareContainer(DataGridContext dataGridContext, object item)
        {
            base.PrepareContainer(dataGridContext, item);
            var gridControl = dataGridContext.DataGridControl as DomainDataGridControl;

            if (this.domain == null)
            {
                var domain = gridControl.Domain;
                if (domain != null)
                {
                    await domain.Dispatcher.InvokeAsync(() =>
                    {
                        this.domain = domain;
                        this.domain.UserLocationChanged += Domain_UserLocationChanged;
                        this.domain.UserEditBegun += Domain_UserEditBegun;
                        this.domain.UserEditEnded += Domain_UserEditEnded;
                        this.domain.UserRemoved += Domain_UserRemoved;
                        this.domain.Deleted += Domain_Deleted;
                        if (this.domain.GetService(typeof(ICremaHost)) is ICremaHost cremaHost)
                        {
                            this.UserID = cremaHost.UserID;
                        }
                    });
                }
            }

            this.UserInfos.Clear();

            if (this.domain != null)
            {
                var domain = this.domain;
                var infos = await domain.Dispatcher.InvokeAsync(() => domain.Users.Select(i => new DomainUserMetaData()
                {
                    DomainUserInfo = i.DomainUserInfo,
                    DomainUserState = i.DomainUserState,
                }).ToArray());

                foreach (var i in infos)
                {
                    if (HashUtility.Equals(this.keys, i.DomainLocationInfo.Keys) == true && this.tableName == i.DomainLocationInfo.TableName)
                    {
                        this.UserInfos.Set(i.DomainUserInfo, i.DomainUserState, i.DomainLocationInfo);
                    }
                }
            }
        }

        protected override void SetDataContext(object item)
        {
            var changed = this.DataContext != item;
            base.SetDataContext(item);
            this.keys = CremaDataRowUtility.GetKeys(item);
            this.tableName = CremaDataRowUtility.GetTableName(item);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override Cell CreateCell(ColumnBase column)
        {
            return new DomainDataCell();
        }

        protected override void EndEditCore()
        {
            try
            {
                this.IsBeginEnding = true;
                base.EndEditCore();
            }
            finally
            {
                this.IsBeginEnding = false;
            }
        }

        protected override void CancelEditCore()
        {
            try
            {
                this.IsBeginEnding = true;
                base.CancelEditCore();
            }
            finally
            {
                this.IsBeginEnding = false;
            }
        }

        private async void Domain_UserLocationChanged(object sender, DomainUserLocationEventArgs e)
        {
            var domainUserInfo = e.DomainUserInfo;
            var domainLocationInfo = e.DomainLocationInfo;
            var domainUserState = e.DomainUserState;
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.DataContext == null)
                    return;

                if (HashUtility.Equals(this.keys, domainLocationInfo.Keys) == true && this.tableName == domainLocationInfo.TableName)
                {
                    this.UserInfos.Set(domainUserInfo, domainUserState, domainLocationInfo);
                }
                else
                {
                    this.UserInfos.Remove(domainUserInfo.UserID);
                }
            });
        }

        private void Domain_UserEditBegun(object sender, DomainUserLocationEventArgs e)
        {

        }

        private void Domain_UserEditEnded(object sender, DomainUserEventArgs e)
        {

        }

        private async void Domain_UserRemoved(object sender, DomainUserRemovedEventArgs e)
        {
            var domainUserInfo = e.DomainUserInfo;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.UserInfos.Remove(domainUserInfo.UserID);
            });
        }

        private void Domain_Deleted(object sender, EventArgs e)
        {
            this.domain.UserLocationChanged -= Domain_UserLocationChanged;
            this.domain.UserEditBegun -= Domain_UserEditBegun;
            this.domain.UserEditEnded -= Domain_UserEditEnded;
            this.domain.UserRemoved -= Domain_UserRemoved;
            this.domain.Deleted -= Domain_Deleted;
            this.domain = null;
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.CanDelete;
        }

        private async void Delete_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (await AppMessageBox.ShowQuestionAsync(Properties.Resources.Message_ConfirmToDeleteRow) == false)
                return;

            try
            {
                await this.DeleteAsync();
            }
            catch (Exception ex)
            {
                await AppMessageBox.ShowErrorAsync(ex);
            }
        }

        internal void UpdateKeys()
        {
            if (this.DataContext != null)
            {
                this.keys = CremaDataRowUtility.GetKeys(this.DataContext);
                this.tableName = CremaDataRowUtility.GetTableName(this.DataContext);
            }
        }

        internal bool IsBeginEnding { get; set; }

        internal DomainDataUserCollection UserInfos { get; } = new DomainDataUserCollection();

        internal string UserID { get; private set; }
    }
}

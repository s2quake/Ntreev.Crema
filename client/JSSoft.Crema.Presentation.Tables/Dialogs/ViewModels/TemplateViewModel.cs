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
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Tables.Properties;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public abstract class TemplateViewModel : ModalDialogAppBase, INotifyDataErrorInfo
    {
        private readonly Authentication authentication;
        private bool isReadOnly;
        private bool isModified;
        private bool isValid;
        private string tableName;
        private string comment;
        private TagInfo tags;
        private string[] selectableTypes;
        private int count;
        [Import]
        private readonly IFlashService flashService = null;

        private EventHandler<DataErrorsChangedEventArgs> errorsChanged;
        private string tableNameError;

        protected TemplateViewModel(Authentication authentication, ITableTemplate template)
            : this(authentication, template, false)
        {

        }

        protected TemplateViewModel(Authentication authentication, ITableTemplate template, bool isNew)
        {
            this.authentication = authentication;
            this.IsNew = isNew;
            this.Template = template;
            this.Template.EditEnded += Template_EditEnded;
            this.Template.EditCanceled += Template_EditCanceled;
            this.Template.Changed += Template_Changed;
            this.DisplayName = Resources.Title_TableTemplateEditing;
        }

        public async Task ChangeAsync()
        {
            try
            {
                this.BeginProgress(this.IsNew ? Resources.Message_Creating : Resources.Message_Changing);
                await this.Template.EndEditAsync(this.authentication);
                await this.Template.Dispatcher.InvokeAsync(() =>
                {
                    this.Template.EditEnded -= Template_EditEnded;
                    this.Template.EditCanceled -= Template_EditCanceled;
                    this.Template.Changed -= Template_Changed;
                });
                this.Domain = null;
                this.Template = null;
                this.isModified = false;
                this.EndProgress();
                await this.TryCloseAsync(true);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public async Task NewColumnAsync()
        {
            var items = await this.Template.Dispatcher.InvokeAsync(() => this.Template.Select(item => item.Name).ToArray());
            var selectableTypes = await this.Template.Dispatcher.InvokeAsync(() => this.Template.SelectableTypes);
            var name = NameUtility.GenerateNewName("Column", items);
            var dialog = new NewColumnViewModel(selectableTypes)
            {
                Name = name,
                IsKey = items.Any() == false,
                DataType = typeof(string).GetTypeName()
            };
            if (await dialog.ShowDialogAsync() == true)
            {
                var member = await this.Template.AddNewAsync(this.authentication);
                await member.SetNameAsync(this.authentication, dialog.Name);
                await member.SetDataTypeAsync(this.authentication, dialog.DataType);
                await member.SetCommentAsync(this.authentication, dialog.Comment);
                await member.SetIsKeyAsync(this.authentication, dialog.IsKey);
                await this.Template.EndNewAsync(this.authentication, member);
            }
        }

        public bool IsReadOnly
        {
            get => this.isReadOnly;
            set
            {
                this.isReadOnly = value;
                this.NotifyOfPropertyChange(nameof(this.IsReadOnly));
            }
        }

        public bool IsNew { get; }

        public ITableTemplate Template { get; private set; }

        public object Source { get; private set; }

        public IEnumerable SelectableTypes
        {
            get
            {
                if (this.selectableTypes != null)
                {
                    foreach (var item in this.selectableTypes)
                    {
                        yield return item;
                    }
                }
            }
        }

        public IDomain Domain { get; private set; }

        public string TableName
        {
            get => this.tableName ?? string.Empty;
            set
            {
                this.tableName = value;
                InvokeAsync();
                async void InvokeAsync()
                {
                    try
                    {
                        await this.Template.SetTableNameAsync(this.authentication, value);
                        this.tableNameError = null;
                    }
                    catch (Exception e)
                    {
                        this.tableNameError = e.Message;
                    }
                    finally
                    {
                        this.Verify(this.VerifyAction);
                        this.errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(this.TableName)));
                    }
                }
            }
        }

        public string Comment
        {
            get => this.comment ?? string.Empty;
            set => this.comment = value;
        }

        public bool CanChange
        {
            get
            {
                if (this.IsProgressing == true || this.isReadOnly == true)
                    return false;
                if (this.Template == null)
                    return false;
                if (this.count == 0)
                    return false;
                if (this.IsModified == false)
                    return false;
                if (this.tableNameError != null)
                    return false;
                return this.isValid;
            }
        }

        public bool IsModified
        {
            get => this.isModified;
            set
            {
                this.isModified = value;
                this.NotifyOfPropertyChange(nameof(this.IsModified));
                this.NotifyOfPropertyChange(nameof(this.CanChange));
            }
        }

        public TagInfo Tags
        {
            get => this.tags;
            set
            {
                InvokeAsync();
                async void InvokeAsync()
                {
                    await this.Template.SetTagsAsync(this.authentication, value);
                    this.tags = value;
                    this.NotifyOfPropertyChange(nameof(this.Tags));
                }
            }
        }

        public async override Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            if (this.Template == null || this.IsModified == false)
            {
                return true;
            }

            var result = await AppMessageBox.ConfirmSaveOnClosingAsync();

            if (result == null)
                return false;

            if (this.Template != null && result == true)
            {
                if (this.tableNameError != null)
                {
                    await AppMessageBox.ShowAsync(this.tableNameError);
                    return false;
                }
                this.BeginProgress(this.IsNew ? Resources.Message_Creating : Resources.Message_Changing);
                try
                {
                    await this.Template.EndEditAsync(this.authentication);
                    await this.Template.Dispatcher.InvokeAsync(() =>
                    {
                        this.Template.EditEnded -= Template_EditEnded;
                        this.Template.EditCanceled -= Template_EditCanceled;
                        this.Template.Changed -= Template_Changed;
                    });
                    this.Template = null;
                    this.EndProgress();
                }
                catch (Exception e)
                {
                    await AppMessageBox.ShowErrorAsync(e);
                    this.EndProgress();
                    return false;
                }
            }

            //this.DialogResult = result.Value;
            return true;
        }

        protected abstract void Verify(Action<bool> isValid);

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
            await this.Template.Dispatcher.InvokeAsync(() =>
            {
                this.Domain = this.Template.Domain;
                this.tableName = this.Template.TableName;
                this.comment = this.Template.Comment;
                this.tags = this.Template.Tags;
                this.selectableTypes = this.Template.SelectableTypes;
                this.count = this.Template.Count;
                this.Source = this.Domain.Source;
                this.isModified = this.Template.IsModified;
            });
            this.Refresh();
            this.Verify(this.VerifyAction);
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (this.Template != null)
            {
                await this.Template.Dispatcher.InvokeAsync(() =>
                {
                    this.Template.EditEnded -= Template_EditEnded;
                    this.Template.EditCanceled -= Template_EditCanceled;
                    this.Template.Changed -= Template_Changed;
                });
                await this.Template.CancelEditAsync(this.authentication);
            }
            this.Template = null;
        }

        protected override void OnCancel()
        {
            this.Template = null;
            base.OnCancel();
        }

        private async void Template_EditEnded(object sender, EventArgs e)
        {
            if (e is DomainDeletedEventArgs ex)
            {
                this.Template.EditEnded -= Template_EditEnded;
                this.Template.EditCanceled -= Template_EditCanceled;
                this.Template.Changed -= Template_Changed;

                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Template = null;
                    this.flashService?.Flash();
                });
                await AppMessageBox.ShowInfoAsync(Resources.Message_ExitEditByUser_Format, ex.UserID);
                await this.TryCloseAsync();
            }
        }

        private async void Template_EditCanceled(object sender, EventArgs e)
        {
            if (e is DomainDeletedEventArgs ex)
            {
                this.Template.EditEnded -= Template_EditEnded;
                this.Template.EditCanceled -= Template_EditCanceled;
                this.Template.Changed -= Template_Changed;

                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Template = null;
                    this.flashService?.Flash();
                });
                await AppMessageBox.ShowInfoAsync(Resources.Message_ExitEditByUser_Format, ex.UserID);
                await this.TryCloseAsync();
            }
        }

        private void Template_Changed(object sender, EventArgs e)
        {
            this.isModified = this.Template.IsModified;
            this.count = this.Template.Count;
            this.Dispatcher.InvokeAsync(() =>
            {
                this.Verify(this.VerifyAction);
            });
        }

        private void VerifyAction(bool isValid)
        {
            this.isValid = isValid;
            this.NotifyOfPropertyChange(nameof(this.CanChange));
        }

        #region INotifyDataErrorInfo

        bool INotifyDataErrorInfo.HasErrors => this.tableNameError != null;

        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add { this.errorsChanged += value; }
            remove { this.errorsChanged -= value; }
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            if (propertyName == nameof(this.TableName))
            {
                return new string[] { this.tableNameError };
            }
            return Enumerable.Empty<string>();
        }

        #endregion
    }
}

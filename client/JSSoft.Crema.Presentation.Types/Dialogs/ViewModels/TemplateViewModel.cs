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

using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Types.Properties;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Types.Dialogs.ViewModels
{
    public abstract class TemplateViewModel : ModalDialogAppBase, INotifyDataErrorInfo
    {
        private readonly Authentication authentication;
        private bool isReadOnly;
        private bool isModified;
        private bool isValid;
        private bool isFlag;
        private string typeName;
        private string comment;
        private int count;
        [Import]
        private readonly IFlashService flashService = null;

        private EventHandler<DataErrorsChangedEventArgs> errorsChanged;
        private string typeNameError;

        protected TemplateViewModel(Authentication authentication, ITypeTemplate template)
            : this(authentication, template, false)
        {

        }

        protected TemplateViewModel(Authentication authentication, ITypeTemplate template, bool isNew)
        {
            this.authentication = authentication;
            this.IsNew = isNew;
            this.Template = template;
            this.Template.EditEnded += Template_EditEnded;
            this.Template.EditCanceled += Template_EditCanceled;
            this.Template.Changed += Template_Changed;
            this.DisplayName = Resources.Title_EditTypeTemplate;
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

        public async Task NewMemberAsync()
        {
            var items = await this.Template.Dispatcher.InvokeAsync(() => this.Template.Select(item => item.Name).ToArray());
            var name = NameUtility.GenerateNewName("Member", items);
            var dialog = new NewMemberViewModel()
            {
                Name = name,
                Value = items.Length,
            };
            if (await dialog.ShowDialogAsync() == true)
            {
                var member = await this.Template.AddNewAsync(this.authentication);
                await member.SetNameAsync(this.authentication, dialog.Name);
                await member.SetValueAsync(this.authentication, dialog.Value);
                await member.SetCommentAsync(this.authentication, dialog.Comment);
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

        public bool IsNew { get; private set; }

        public ITypeTemplate Template { get; private set; }

        public object Source { get; private set; }

        public IDomain Domain { get; private set; }

        public string TypeName
        {
            get => this.typeName ?? string.Empty;
            set
            {
                this.typeName = value;
                this.NotifyOfPropertyChange(nameof(this.TypeName));
                InvokeAsync();
                async void InvokeAsync()
                {
                    try
                    {
                        await this.Template.SetTypeNameAsync(this.authentication, value);
                        this.typeNameError = null;
                    }
                    catch (Exception e)
                    {
                        this.typeNameError = e.Message;
                    }
                    finally
                    {
                        this.Verify(this.VerifyAction);
                        this.errorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(this.TypeName)));
                    }
                }
            }
        }

        public string Comment
        {
            get => this.comment ?? string.Empty;
            set
            {
                this.comment = value;
                this.NotifyOfPropertyChange(nameof(this.Comment));
            }
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
                if (this.typeNameError != null)
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

        public bool IsFlag
        {
            get => this.isFlag;
            set
            {
                InvokeAsync();
                async void InvokeAsync()
                {
                    await this.Template.SetIsFlagAsync(this.authentication, value);
                    this.isFlag = value;
                    this.NotifyOfPropertyChange(nameof(this.IsFlag));
                }
            }
        }

        public async override Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            if (this.Template == null || this.IsModified == false)
            {
                return true;
            }

            var result = await AppMessageBox.ConfirmCreateOnClosingAsync();

            if (result == null)
                return false;

            if (this.Template != null && result == true)
            {
                if (this.typeNameError != null)
                {
                    await AppMessageBox.ShowAsync(this.typeNameError);
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

            return result.Value;
        }

        protected abstract void Verify(Action<bool> isValid);

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);
            await this.Template.Dispatcher.InvokeAsync((Action)(() =>
            {
                this.Domain = this.Template.Domain;
                this.typeName = this.Template.TypeName;
                this.comment = this.Template.Comment;
                this.isFlag = this.Template.IsFlag;
                this.count = this.Template.Count;
                this.Source = this.Domain.Source;
                this.isModified = this.Template.IsModified;
            }));
            this.Refresh();
            this.Verify(this.VerifyAction);
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (this.Template != null)
            {
                await this.Template.CancelEditAsync(this.authentication);
                await this.Template.Dispatcher.InvokeAsync(() =>
                {
                    this.Template.EditEnded -= Template_EditEnded;
                    this.Template.EditCanceled -= Template_EditCanceled;
                    this.Template.Changed -= Template_Changed;
                });
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
                this.Template = null;

                await this.Dispatcher.InvokeAsync(() =>
                {
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
                this.Template = null;

                await this.Dispatcher.InvokeAsync(() =>
                {
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

        bool INotifyDataErrorInfo.HasErrors => this.typeNameError != null;

        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add { this.errorsChanged += value; }
            remove { this.errorsChanged -= value; }
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            if (propertyName == nameof(this.TypeName))
            {
                return new string[] { this.typeNameError };
            }
            return Enumerable.Empty<string>();
        }

        #endregion
    }
}
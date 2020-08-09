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

using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Presentation.Types.Properties;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Types.Dialogs.ViewModels
{
    public abstract class TemplateViewModel : ModalDialogAppBase, INotifyDataErrorInfo
    {
        private readonly Authentication authentication;
        private ITypeTemplate template;
        private IDomain domain;
        private readonly bool isNew;
        private bool isReadOnly;
        private bool isModified;
        private bool isValid;
        private string typeName;
        private string comment;
        private bool isFlag;
        private int count;
        private object source;

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
            this.isNew = isNew;
            this.template = template;
            this.template.EditEnded += Template_EditEnded;
            this.template.EditCanceled += Template_EditCanceled;
            this.template.Changed += Template_Changed;
            this.DisplayName = Resources.Title_EditTypeTemplate;
        }

        public async Task ChangeAsync()
        {
            try
            {
                this.BeginProgress(this.IsNew ? Resources.Message_Creating : Resources.Message_Changing);
                await this.template.EndEditAsync(this.authentication);
                await this.template.Dispatcher.InvokeAsync(() =>
                {
                    this.template.EditEnded -= Template_EditEnded;
                    this.template.EditCanceled -= Template_EditCanceled;
                    this.template.Changed -= Template_Changed;
                });
                this.domain = null;
                this.template = null;
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
            var items = await this.template.Dispatcher.InvokeAsync(() => this.template.Select(item => item.Name).ToArray());
            var name = NameUtility.GenerateNewName("Member", items);
            var dialog = new NewMemberViewModel()
            {
                Name = name,
                Value = items.Length,
            };
            if (await dialog.ShowDialogAsync() == true)
            {
                var member = await this.template.AddNewAsync(this.authentication);
                await member.SetNameAsync(this.authentication, dialog.Name);
                await member.SetValueAsync(this.authentication, dialog.Value);
                await member.SetCommentAsync(this.authentication, dialog.Comment);
                await this.template.EndNewAsync(this.authentication, member);
            }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
            set
            {
                this.isReadOnly = value;
                this.NotifyOfPropertyChange(nameof(this.IsReadOnly));
            }
        }

        public bool IsNew
        {
            get { return this.isNew; }
        }

        public ITypeTemplate Template
        {
            get { return this.template; }
        }

        public object Source
        {
            get { return this.source; }
        }

        public IDomain Domain
        {
            get { return this.domain; }
        }

        public string TypeName
        {
            get { return this.typeName ?? string.Empty; }
            set
            {
                this.typeName = value;
                this.NotifyOfPropertyChange(nameof(this.TypeName));
                InvokeAsync();
                async void InvokeAsync()
                {
                    try
                    {
                        await this.template.SetTypeNameAsync(this.authentication, value);
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
            get { return this.comment ?? string.Empty; }
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
                if (this.template == null)
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
            get { return this.isModified; }
            set
            {
                this.isModified = value;
                this.NotifyOfPropertyChange(nameof(this.IsModified));
                this.NotifyOfPropertyChange(nameof(this.CanChange));
            }
        }

        public bool IsFlag
        {
            get { return this.isFlag; }
            set
            {
                InvokeAsync();
                async void InvokeAsync()
                {
                    await this.template.SetIsFlagAsync(this.authentication, value);
                    this.isFlag = value;
                    this.NotifyOfPropertyChange(nameof(this.IsFlag));
                }
            }
        }

        public async override Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            if (this.template == null || this.IsModified == false)
            {
                return true;
            }

            var result = await AppMessageBox.ConfirmCreateOnClosingAsync();

            if (result == null)
                return false;

            if (this.template != null && result == true)
            {
                if (this.typeNameError != null)
                {
                    await AppMessageBox.ShowAsync(this.typeNameError);
                    return false;
                }
                this.BeginProgress(this.IsNew ? Resources.Message_Creating : Resources.Message_Changing);
                try
                {
                    await this.template.EndEditAsync(this.authentication);
                    await this.template.Dispatcher.InvokeAsync(() =>
                    {
                        this.template.EditEnded -= Template_EditEnded;
                        this.template.EditCanceled -= Template_EditCanceled;
                        this.template.Changed -= Template_Changed;
                    });
                    this.template = null;
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
            await this.template.Dispatcher.InvokeAsync(() =>
            {
                this.domain = this.template.Domain;
                this.typeName = this.template.TypeName;
                this.comment = this.template.Comment;
                this.isFlag = this.template.IsFlag;
                this.count = this.template.Count;
                this.source = this.domain.Source;
                this.isModified = this.template.IsModified;
            });
            this.Refresh();
            this.Verify(this.VerifyAction);
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (this.template != null)
            {
                await this.template.CancelEditAsync(this.authentication);
                await this.template.Dispatcher.InvokeAsync(() =>
                {
                    this.template.EditEnded -= Template_EditEnded;
                    this.template.EditCanceled -= Template_EditCanceled;
                    this.template.Changed -= Template_Changed;
                });
            }
            this.template = null;
        }

        protected override void OnCancel()
        {
            this.template = null;
            base.OnCancel();
        }

        private async void Template_EditEnded(object sender, EventArgs e)
        {
            if (e is DomainDeletedEventArgs ex)
            {
                this.template.EditEnded -= Template_EditEnded;
                this.template.EditCanceled -= Template_EditCanceled;
                this.template.Changed -= Template_Changed;
                this.template = null;

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
                this.template.EditEnded -= Template_EditEnded;
                this.template.EditCanceled -= Template_EditCanceled;
                this.template.Changed -= Template_Changed;
                this.template = null;

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
            this.isModified = this.template.IsModified;
            this.count = this.template.Count;
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
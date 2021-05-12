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
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.ObjectModel;
using JSSoft.ModernUI.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Types.Dialogs.ViewModels
{
    public class CopyTypeViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly IType type;
        private readonly ITypeCollection types;
        private readonly ITypeCategoryCollection categories;
        private bool isValid;
        private string categoryPath;
        private string newName;

        private CopyTypeViewModel(Authentication authentication, IType type)
        {
            this.authentication = authentication;
            this.type = type;
            this.type.Dispatcher.VerifyAccess();
            this.types = type.GetService(typeof(ITypeCollection)) as ITypeCollection;
            this.categories = type.GetService(typeof(ITypeCategoryCollection)) as ITypeCategoryCollection;
            this.CategoryPaths = this.categories.Select(item => item.Path).ToArray();
            this.categoryPath = this.type.Category.Path;
            this.TypeName = type.Name;
            this.NewName = type.Name;
            this.DisplayName = Resources.Title_CopyType;
        }

        public static Task<CopyTypeViewModel> CreateInstanceAsync(Authentication authentication, ITypeDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is IType type)
            {
                return type.Dispatcher.InvokeAsync(() =>
                {
                    return new CopyTypeViewModel(authentication, type);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        public async Task CopyAsync()
        {
            try
            {
                this.BeginProgress(Resources.Message_CopingType);
                await this.type.CopyAsync(this.authentication, this.NewName, this.CategoryPath);
                this.EndProgress();
                await this.TryCloseAsync(true);
                await AppMessageBox.ShowAsync(Resources.Message_TypeCopied);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        public string[] CategoryPaths { get; private set; }

        public string NewName
        {
            get => this.newName ?? string.Empty;
            set
            {
                this.newName = value;
                this.NotifyOfPropertyChange(nameof(this.NewName));
                this.VerifyCopy(this.VerifyAction);
            }
        }

        public string CategoryPath
        {
            get => this.categoryPath ?? string.Empty;
            set
            {
                this.categoryPath = value;
                this.NotifyOfPropertyChange(nameof(this.CategoryPath));
                this.VerifyCopy(this.VerifyAction);
            }
        }

        public string TypeName { get; private set; }

        public bool CanCopy
        {
            get
            {
                if (this.IsProgressing == true)
                    return false;

                if (this.CategoryPath == string.Empty)
                    return false;

                if (NameValidator.VerifyName(this.NewName) == false)
                    return false;

                return this.isValid;
            }
        }

        private async void VerifyCopy(Action<bool> isValid)
        {
            if (await this.types.ContainsAsync(this.NewName) == true)
                return;
            var result = await this.categories.ContainsAsync(this.CategoryPath) == true;
            isValid(result);
        }

        private void VerifyAction(bool isValid)
        {
            this.isValid = isValid;
            this.NotifyOfPropertyChange(nameof(this.CanCopy));
        }
    }
}

﻿// Released under the MIT License.
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
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.Services;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.ObjectModel;
using JSSoft.ModernUI.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class CopyTableViewModel : ModalDialogBase
    {
        private readonly Authentication authentication;
        private readonly ITable table;
        private readonly ITableCollection tables;
        private readonly ITableCategoryCollection categories;
        private string categoryPath;
        private bool useTemplate;
        private bool copyData = true;
        private string newName;
        private bool isVerify;

        private CopyTableViewModel(Authentication authentication, ITable table, bool useTemplate)
        {
            this.authentication = authentication;
            this.table = table;
            this.table.Dispatcher.VerifyAccess();
            this.useTemplate = useTemplate;
            this.tables = table.GetService(typeof(ITableCollection)) as ITableCollection;
            this.categories = table.GetService(typeof(ITableCategoryCollection)) as ITableCategoryCollection;
            this.categoryPath = this.table.Category.Path;
            this.Categories = this.categories.Select(item => item.Path).OrderBy(item => item).ToArray();
            this.TableName = table.Name;
            this.newName = table.Name;
            this.DisplayName = Resources.Title_CopyTable;
        }

        public static Task<CopyTableViewModel> CreateInstanceAsync(Authentication authentication, ITableDescriptor descriptor)
        {
            return CreateInstanceAsync(authentication, descriptor, false);
        }

        public static Task<CopyTableViewModel> CreateInstanceAsync(Authentication authentication, ITableDescriptor descriptor, bool useTemplate)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITable table)
            {
                return table.Dispatcher.InvokeAsync(() =>
                {
                    return new CopyTableViewModel(authentication, table, useTemplate);
                });
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        public string[] Categories { get; private set; }

        public string TableName { get; private set; }

        public string NewName
        {
            get => this.newName ?? string.Empty;
            set
            {
                this.newName = value;
                this.NotifyOfPropertyChange(nameof(this.NewName));
                this.VerfiyAction();
            }
        }

        public string CategoryPath
        {
            get => this.categoryPath ?? string.Empty;
            set
            {
                this.categoryPath = value;
                this.NotifyOfPropertyChange(nameof(this.CategoryPath));
                this.VerfiyAction();
            }
        }

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

                return this.isVerify;
            }
        }

        public bool UseTemplate
        {
            get => this.useTemplate;
            set
            {
                this.useTemplate = value;
                this.DisplayName = this.useTemplate == false ? Resources.Title_CopyTable : Resources.Title_InheritTable;
                this.NotifyOfPropertyChange(nameof(this.UseTemplate));
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
        }

        public bool CopyData
        {
            get => this.copyData;
            set
            {
                this.copyData = value;
                this.NotifyOfPropertyChange(nameof(this.CopyData));
            }
        }

        public async Task CopyAsync()
        {
            try
            {
                this.BeginProgress(Resources.Message_Coping);
                if (this.UseTemplate == true)
                    await this.table.InheritAsync(this.authentication, this.NewName, this.CategoryPath, this.CopyData);
                else
                    await this.table.CopyAsync(this.authentication, this.NewName, this.CategoryPath, this.CopyData);
                this.EndProgress();
                await this.TryCloseAsync(true);
            }
            catch (Exception e)
            {
                this.EndProgress();
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        private async void VerfiyAction()
        {
            if (await this.tables.ContainsAsync(this.NewName) == true)
                return;
            this.isVerify = await this.categories.ContainsAsync(this.CategoryPath) == true;
            this.NotifyOfPropertyChange(nameof(this.CanCopy));
        }
    }
}

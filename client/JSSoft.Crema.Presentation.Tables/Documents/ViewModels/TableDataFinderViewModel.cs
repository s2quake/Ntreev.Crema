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
using JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels;
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.IO;
using JSSoft.Library.Linq;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Documents.ViewModels
{
    class TableDataFinderViewModel : DocumentBase, IDocument, IPartImportsSatisfiedNotification
    {
        private readonly Authentication authentication;
        private readonly TableBrowserViewModel browser = null;
        private FindResultItemViewModel selectedItem;
        private string findingText;
        private string findingTarget;
        private ITableItemDescriptor descriptor;

        [Import]
        private readonly TableDocumentServiceViewModel documentService = null;

        public TableDataFinderViewModel(Authentication authentication, TableBrowserViewModel browser, ITableItemDescriptor descriptor)
        {
            this.authentication = authentication;
            this.browser = browser;
            this.findingTarget = descriptor == null ? PathUtility.Separator : descriptor.Path;
            this.descriptor = browser.GetDescriptor(this.findingTarget);
            this.DisplayName = Resources.Title_Find;
        }

        public void MoveToTable(FindResultItemViewModel item)
        {
            if (this.documentService != null)
            {
                var viewModel = this.browser.GetTableViewModel(item.TableName);
                var document = this.documentService.Find(viewModel) ?? documentService.ViewTable(authentication, viewModel);
                var documentItem = this.documentService.FindItem(viewModel);
                documentItem.SelectedColumn = item.ColumnName;
                documentItem.SelectedItemIndex = item.Row;
                document.SelectedTable = documentItem;
                this.documentService.SelectedDocument = document;
            }
        }

        public string FindingText
        {
            get => this.findingText ?? string.Empty;
            set
            {
                this.findingText = value;
                this.descriptor = this.browser.GetDescriptor(this.findingTarget);
                this.NotifyOfPropertyChange(nameof(this.FindingText));
                this.NotifyOfPropertyChange(nameof(this.CanFind));
            }
        }

        public string[] FindingTargets
        {
            get
            {
                var paths = this.browser.Items.FamilyTree(item => item.Items)
                                        .OfType<ITableItemDescriptor>()
                                        .Select(item => item.Path)
                                        .ToArray();
                return paths;
            }
        }

        public string FindingTarget
        {
            get => this.findingTarget ?? string.Empty;
            set
            {
                this.findingTarget = value;
                this.NotifyOfPropertyChange(nameof(this.FindingTarget));
            }
        }

        public async Task FindAsync()
        {
            this.BeginProgress();
            this.Items.Clear();

            try
            {
                var results = await TableItemDescriptorUtility.FindAsync(this.authentication, this.descriptor, this.findingText, FindOptions.None);
                foreach (var item in results)
                {
                    this.Items.Add(new FindResultItemViewModel(item));
                }

                this.DisplayName = $"{Resources.Title_Find} - {this.findingText}";
                this.NotifyOfPropertyChange(nameof(this.DisplayName));
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }

            this.EndProgress();
        }

        public ObservableCollection<FindResultItemViewModel> Items { get; } = new();

        public FindResultItemViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.NotifyOfPropertyChange(nameof(this.SelectedItem));
            }
        }

        public bool CanFind => this.IsProgressing == false && this.FindingText != string.Empty;

        #region IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {

        }

        #endregion
    }
}

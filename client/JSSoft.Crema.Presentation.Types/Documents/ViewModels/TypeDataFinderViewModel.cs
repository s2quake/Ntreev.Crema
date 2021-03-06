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

using JSSoft.Crema.Presentation.Types.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Presentation.Types.Documents.ViewModels
{
    class TypeDataFinderViewModel : DocumentBase, IDocument
    {
        private readonly Authentication authentication;
        private readonly IDataBase dataBase;
        private string findingText;
        private string findingTarget;
        private FindResultItemViewModel selectedItem;

        [Import]
        private readonly TypeDocumentServiceViewModel documentService = null;

        public TypeDataFinderViewModel(Authentication authentication, IDataBase dataBase, string findingTarget)
        {
            this.authentication = authentication;
            this.dataBase = dataBase;
            this.findingTarget = findingTarget;
            this.DisplayName = Resources.Title_Find;
            this.Initialize();
        }

        public async void MoveToType(FindResultItemViewModel item)
        {
            if (this.documentService != null)
            {
                var type = await this.dataBase.Dispatcher.InvokeAsync(() => this.dataBase.TypeContext.Types[item.TypeName]);
                documentService.MoveToType(this.authentication, type, item.ColumnName, item.Row);
            }
        }

        public string FindingText
        {
            get => this.findingText ?? string.Empty;
            set
            {
                this.findingText = value;
                this.NotifyOfPropertyChange(nameof(this.FindingText));
                this.NotifyOfPropertyChange(nameof(this.CanFind));
            }
        }

        public string[] FindingTargets { get; private set; }

        public string FindingTarget
        {
            get => this.findingTarget ?? string.Empty;
            set
            {
                this.findingTarget = value;
                this.NotifyOfPropertyChange(nameof(this.FindingTarget));
            }
        }

        public async void Find()
        {
            this.BeginProgress();
            this.ItemsSource.Clear();

            try
            {
                var typeItem = await this.dataBase.Dispatcher.InvokeAsync(() => this.dataBase.TypeContext[this.findingTarget]);
                var results = await typeItem.FindAsync(this.authentication, this.findingText, FindOptions.None);

                foreach (var item in results)
                {
                    this.ItemsSource.Add(new FindResultItemViewModel(item));
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

        public ObservableCollection<FindResultItemViewModel> ItemsSource { get; } = new();

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

        private async void Initialize()
        {
            this.BeginProgress();

            try
            {
                this.FindingTargets = await this.dataBase.Dispatcher.InvokeAsync(() =>
                {
                    return this.dataBase.TypeContext.Where(item => item.VerifyAccessType(this.authentication, AccessType.Guest)).Select(item => item.Path).ToArray();
                });
                if (this.findingTarget == null)
                    this.findingTarget = this.FindingTargets.First(item => item == this.findingTarget);
                this.EndProgress();
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                this.EndProgress();
                await this.TryCloseAsync();
            }
        }
    }
}

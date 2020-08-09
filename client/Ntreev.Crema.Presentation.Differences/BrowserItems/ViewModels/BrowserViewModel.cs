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

using Ntreev.Crema.Data.Diff;
using Ntreev.Crema.Presentation.Differences.Documents.ViewModels;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ntreev.Crema.Presentation.Differences.BrowserItems.ViewModels
{
    class BrowserViewModel : TreeViewBase, IBrowserItem
    {
        private readonly DiffDataSet dataSet;
        private readonly Authenticator authenticator;
        private readonly ICremaHost cremaHost;
        private readonly ICremaAppHost cremaAppHost;
        private readonly PropertyService propertyService;
        private readonly BrowserService browserService;
        private readonly DocumentServiceViewModel documentService;
        private readonly Lazy<IServiceProvider> serviceProvider;

        public BrowserViewModel(Authenticator authenticator, ICremaHost cremaHost, ICremaAppHost cremaAppHost, PropertyService propertyService, BrowserService browserService, DocumentServiceViewModel documentService, Lazy<IServiceProvider> serviceProvider)
        {
            this.authenticator = authenticator;
            this.cremaHost = cremaHost;
            this.cremaAppHost = cremaAppHost;
            this.propertyService = propertyService;
            this.browserService = browserService;
            this.documentService = documentService;
            this.serviceProvider = serviceProvider;
        }

        private TableTreeViewItemViewModel selectedTable;

        public BrowserViewModel(DiffDataSet dataSet)
        {
            this.dataSet = dataSet;
            this.CloseCommand = new DelegateCommand(async (p) => await this.CloseAsync(), (p) => this.CanClose);
            this.DisplayName = dataSet.Header1;
            this.Dispatcher.InvokeAsync(() => this.AttachPropertyService(this.propertyService));
        }

        public async Task CloseAsync()
        {
            if (await AppMessageBox.ConfirmDeleteAsync() == false)
                return;
            this.BeginProgress();
            await this.CloseDocumentsAsync(false);
            this.EndProgress();
            this.browserService.ItemsSource.Remove(this);
        }

        public void Import()
        {
            if (this.SelectedItem is TableTreeViewItemViewModel viewModel)
            {
                this.Import(viewModel);
            }
        }

        public bool CanClose
        {
            get { return true; }
        }

        public bool CanImport
        {
            get
            {
                if (this.SelectedItem is TableTreeViewItemViewModel viewModel)
                {
                    if (viewModel.IsActivated == false)
                        return false;
                    return viewModel.IsResolved && viewModel.Parent is TableTreeViewItemViewModel == false;
                }
                return false;
            }
        }

        public ICommand CloseCommand { get; }

        private async Task CloseDocumentsAsync(bool save)
        {
            var query = from item in this.documentService.Documents
                        let document = item as DifferenceDocumentBase
                        where document != null && document.Target.Browser == this
                        select document;

            var documentList = query.ToList();
            foreach (var item in documentList.ToArray())
            {
                item.Disposed += (s, e) => documentList.Remove(item);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    if (item.IsModified == true && save == false)
                        item.IsModified = false;
                    item.Dispose();
                });
                await Task.Delay(1);
            }

            while (documentList.Any())
            {
                await Task.Delay(1);
            }
        }

        public void UpdateItemsSource()
        {
            var compositionService = this.ServiceProvider.GetService(typeof(ICompositionService)) as ICompositionService;

            var typesCategory = new CategoryTreeViewItemViewModel("types");
            compositionService.SatisfyImportsOnce(typesCategory);
            foreach (var item in this.dataSet.Types)
            {
                var viewModel = new TypeTreeViewItemViewModel(this, item);
                compositionService.SatisfyImportsOnce(viewModel);
                typesCategory.Items.Add(viewModel);
            }
            this.Items.Add(typesCategory);

            var templatesCategory = new CategoryTreeViewItemViewModel("templates");
            compositionService.SatisfyImportsOnce(templatesCategory);
            foreach (var item in this.dataSet.Tables)
            {
                if (item.TemplatedParent != null)
                    continue;
                var viewModel = new TemplateTreeViewItemViewModel(this, item.Template);
                compositionService.SatisfyImportsOnce(viewModel);
                templatesCategory.Items.Add(viewModel);
            }
            this.Items.Add(templatesCategory);

            var tablesCategory = new CategoryTreeViewItemViewModel("tables");
            compositionService.SatisfyImportsOnce(tablesCategory);
            foreach (var item in this.dataSet.Tables)
            {
                var viewModel = new TableTreeViewItemViewModel(this, item);
                compositionService.SatisfyImportsOnce(viewModel);
                tablesCategory.Items.Add(viewModel);
            }
            this.Items.Add(tablesCategory);
        }

        public bool IsVisible => true;

        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);

            if (this.selectedTable != null)
            {
                this.selectedTable.PropertyChanged -= SelectedTable_PropertyChanged;
            }
            this.propertyService.SelectedObject = this.SelectedItem;
            this.selectedTable = this.SelectedItem as TableTreeViewItemViewModel;
            if (this.selectedTable != null)
            {
                this.selectedTable.PropertyChanged += SelectedTable_PropertyChanged;
            }
            this.NotifyOfPropertyChange(nameof(this.CanImport));
        }

        private void Shell_Loaded(object sender, EventArgs e)
        {
            this.UpdateItemsSource();
        }

        private void Shell_Unloaded(object sender, EventArgs e)
        {
            this.Items.Clear();
        }

        private void SelectedTable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TableTreeViewItemViewModel.IsResolved))
            {
                this.NotifyOfPropertyChange(nameof(this.CanImport));
            }
        }

        private async void Import(TableTreeViewItemViewModel viewModel)
        {
            try
            {
                var dataTable = viewModel.Source.ExportTable2();
                var dataBaseName = this.cremaAppHost.DataBaseName;
                var dataBase = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext[dataBaseName]);

                var comment = await this.GetCommentAsync(viewModel.DisplayName);
                if (comment == null)
                    return;

                var dialog = new ProgressViewModel
                {
                    DisplayName = viewModel.DisplayName
                };
                await dialog.ShowDialogAsync(() => dataBase.ImportAsync(this.authenticator, dataTable.DataSet, comment));
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
        }

        private async Task<string> GetCommentAsync(string displayName)
        {
            var dialog = new CommentViewModel()
            {
                DisplayName = displayName,
            };
            if (await dialog.ShowDialogAsync() == true)
                return dialog.Comment;
            return null;
        }

#pragma warning disable CS0108 // 'BrowserViewModel.ServiceProvider'은(는) 상속된 'ViewModelBase.ServiceProvider' 멤버를 숨깁니다. 숨기려면 new 키워드를 사용하세요.
        private IServiceProvider ServiceProvider => this.serviceProvider.Value;
#pragma warning restore CS0108 // 'BrowserViewModel.ServiceProvider'은(는) 상속된 'ViewModelBase.ServiceProvider' 멤버를 숨깁니다. 숨기려면 new 키워드를 사용하세요.

        private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    }
}

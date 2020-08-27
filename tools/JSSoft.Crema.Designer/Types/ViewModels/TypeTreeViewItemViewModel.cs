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

using System;
using System.Linq;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using JSSoft.Library.ObjectModel;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Windows.Input;
using JSSoft.ModernUI.Framework;
using JSSoft.Crema.Data;
using JSSoft.Library.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using JSSoft.ModernUI.Framework.ViewModels;
using System.Collections;
using JSSoft.Library;
using System.ComponentModel;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Library.IO;

namespace JSSoft.Crema.Designer.Types.ViewModels
{
    public class TypeTreeViewItemViewModel : TreeViewItemViewModel, ITypeDescriptor
    {
        [Import]
        private IServiceProvider serviceProvider = null;
        private CremaDataType dataType;
        private ISelector selector;

        private ICommand renameCommand;
        private ICommand deleteCommand;
        private ICommand viewCommand;

        private TypeInfo typeInfo;
        private TypeAttribute typeAttribute;
        private TypeState typeState;

        public TypeTreeViewItemViewModel(CremaDataType dataType, ISelector selector)
        {
            this.Target = dataType;
            this.dataType = dataType;
            this.dataType.ExtendedProperties[selector] = this;
            this.selector = selector;
            this.typeInfo = dataType.TypeInfo;
            this.typeAttribute = TypeAttribute.None;
            this.typeState = TypeState.None;
            this.renameCommand = new DelegateCommand(async () => await this.RenameAsync());
            this.deleteCommand = new DelegateCommand(async () => await this.DeleteAsync());
            this.viewCommand = new DelegateCommand(async () => await this.ViewContentAsync());
            this.dataType.PropertyChanged += DataType_PropertyChanged;
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        public async Task ViewContentAsync()
        {
            var service = this.serviceProvider.GetService(typeof(TypeDocumentViewModel)) as TypeDocumentViewModel;
            await service.OpenTypeAsync(this.dataType);
        }

        public async Task EditTemplateAsync()
        {
            var dataSet = new CremaDataSet();
            var dataType = this.dataType.CopyTo(dataSet);

            var dialog = new EditTemplateViewModel(dataType);
            if (dialog != null)
                await dialog.ShowDialogAsync();
        }

        public async Task DeleteAsync()
        {
            var dialog = new DeleteViewModel();
            if (await dialog.ShowDialogAsync() != true)
                return;

            var dataSet = this.dataType.DataSet;
            dataSet.Types.Remove(this.dataType);
            this.Parent = null;
        }

        public void Copy()
        {
            //var dialog = await this.type.Dispatcher.InvokeAsync(() => new CopyTypeViewModel(this.authenticator, this.type));
            //if (dialog.ShowDialog() == true)
            //{

            //}
        }

        public void Inherit()
        {
            //var dialog = await this.type.Dispatcher.InvokeAsync(() => new CopyTypeViewModel(this.authenticator, this.type));
            //dialog.UseTemplate = true;
            //dialog.ShowDialog();
        }

        public async Task RenameAsync()
        {
            var dialog = new RenameTypeViewModel(this.dataType);
            if (await dialog.ShowDialogAsync() != true)
                return;

            this.dataType.TypeName = dialog.NewName;
        }

        public async Task MoveAsync()
        {
            var dataSet = this.dataType.DataSet;
            var categoryPaths = dataSet.ExtendedProperties[CremaSchema.TypeDirectory] as string[];
            var dialog = new MoveViewModel(this.dataType.CategoryPath + this.dataType.Name, categoryPaths);
            if (await dialog.ShowDialogAsync() != true)
                return;

            var targetViewModel = FindCategory(dialog.TargetPath);

            this.Parent = null;
            this.Parent = targetViewModel;

            if (this.selector != null)
                this.ExpandAncestors();
        }

        public void ViewLog()
        {
            //var dialog = new LogViewModel(this.authenticator, this.type);
            //dialog.ShowDialog();
        }

        //public void AddChildType()
        //{
        //    //var comment = await this.LockAsync("AddChildType");
        //    //if (comment == null)
        //    //    return;

        //    var dataSet = new CremaDataSet();
        //    var dataType = this.dataType.CopyTo(dataSet);
        //    var tempalte = CremaTemplate.CreateChild(dataType);
        //    var dialog = new NewChildTypeViewModel(dataType, tempalte);
        //    if (dialog != null)
        //        dialog.ShowDialog();

        //    //await this.UnlockAsync(comment);
        //}

        public CremaDataType Type => this.dataType;

        public override string DisplayName
        {
            get
            {
                if (this.Parent is TypeTreeViewItemViewModel)
                    return this.TypeName;
                return this.TypeInfo.Name;
            }
        }

        public string TypeName => this.typeInfo.Name;

        public string Path
        {
            get { return this.typeInfo.CategoryPath + this.typeInfo.Name; }
        }

        public TypeInfo TypeInfo
        {
            get { return this.typeInfo; }
        }

        public TypeAttribute TypeAttribute => this.typeAttribute;

        public TypeState TypeState => this.typeState;

        public TagInfo Tags
        {
            get { return this.typeInfo.Tags; }
        }

        public bool IsFlag => this.TypeInfo.IsFlag;

        public bool CanEditTemplate => true;

        //public bool IsInherited
        //{
        //    get { return this.typeInfo.TemplatedParent != string.Empty; }
        //}

        //public bool IsBaseTemplate
        //{
        //    get { return this.dataType.DerivedTypes.Any(); }
        //}

        public ICommand RenameCommand
        {
            get { return this.renameCommand; }
        }

        public ICommand DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        public ICommand EditCommand
        {
            get { return this.viewCommand; }
        }

        protected override void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);
        }

        private void DataType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CremaDataType.TypeName))
            {
                this.typeInfo = this.dataType.TypeInfo;
                this.Refresh();
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (this.serviceProvider != null)
                {
                    var compositionService = this.serviceProvider.GetService(typeof(ICompositionService)) as ICompositionService;
                    foreach (var item in e.NewItems)
                    {
                        compositionService.SatisfyImportsOnce(item);
                    }
                }
            }
        }

        private CategoryTreeViewItemViewModel FindCategory(string categoryPath)
        {
            var viewModel = this as TreeViewItemViewModel;

            while (viewModel is DataBaseTreeViewItemViewModel == false)
            {
                viewModel = viewModel.Parent;
            }

            var segments = StringUtility.Split(categoryPath, PathUtility.SeparatorChar);

            foreach (var item in segments)
            {
                viewModel = viewModel.Items.Single(i => i.DisplayName == item);
            }

            return viewModel as CategoryTreeViewItemViewModel;
        }

        #region ITypeDescriptor

        CremaDataType ITypeDescriptor.Target
        {
            get { return this.dataType; }
        }

        ITypeDescriptor ITypeDescriptor.Parent
        {
            get { return this.Parent as ITypeDescriptor; }
        }

        IEnumerable<ITypeDescriptor> ITypeDescriptor.Childs
        {
            get
            {
                foreach (var item in this.Items)
                {
                    if (item is ITypeDescriptor == true)
                        yield return item as ITypeDescriptor;
                }
            }
        }

        #endregion
    }
}

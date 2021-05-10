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
using JSSoft.Crema.Presentation.Converters.Properties;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Converters.Dialogs.ViewModels
{
    public class ImportViewModel : ModalDialogAppBase
    {
        private readonly IDataBase dataBase;
        private readonly IImportService importService;
        private readonly Authentication authentication;
        private readonly IAppConfiguration configs;
        private IImporter selectedImporter;
        private bool isImporting;
        private CancellationTokenSource cancelToken;
        private string comment;

        private ImportViewModel(Authentication authentication, IDataBase dataBase)
            : base(dataBase)
        {
            this.DisplayName = Resources.Title_Import;
            this.authentication = authentication;
            this.dataBase = dataBase;
            this.dataBase.Dispatcher.VerifyAccess();
            this.importService = dataBase.GetService(typeof(IImportService)) as IImportService;
            this.configs = dataBase.GetService(typeof(IAppConfiguration)) as IAppConfiguration;
            this.Importers = new ObservableCollection<IImporter>(this.importService.Importers);
            this.configs.Update(this);
        }

        public static Task<ImportViewModel> CreateInstanceAsync(Authentication authentication, IDataBase dataBase)
        {
            return dataBase.Dispatcher.InvokeAsync(() =>
            {
                return new ImportViewModel(authentication, dataBase);
            });
        }

        [Obsolete("Progress")]
        public async Task ImportAsync()
        {
            this.cancelToken = new CancellationTokenSource();
            this.CanImport = false;

            try
            {
                this.BeginProgress(Resources.Message_Importing);
                var tableNames = this.selectedImporter.GetTableNames();
                var dataSet = new CremaDataSet() { SignatureDateProvider = new SignatureDateProvider(this.authentication.ID), };
                await this.dataBase.Dispatcher.InvokeAsync(() => this.CreateTables(dataSet, tableNames));
                await Task.Run(() => this.selectedImporter.Import(dataSet));
                await this.dataBase.ImportAsync(this.authentication, dataSet, this.Comment);
                this.configs.Commit(this);
                await AppMessageBox.ShowAsync(Resources.Message_Imported);
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
            }
            finally
            {
                this.EndProgress();
                this.CanImport = true;
                this.cancelToken = null;
            }
        }

        public void Cancel()
        {
            this.cancelToken.Cancel();
        }

        public ObservableCollection<IImporter> Importers { get; private set; }

        public IImporter SelectedImporter
        {
            get => this.selectedImporter;
            set
            {
                if (this.selectedImporter != null && this.selectedImporter is INotifyPropertyChanged == true)
                {
                    (this.selectedImporter as INotifyPropertyChanged).PropertyChanged -= SelectedImporter_PropertyChanged;
                }
                this.selectedImporter = value;
                if (this.selectedImporter != null && this.selectedImporter is INotifyPropertyChanged == true)
                {
                    (this.selectedImporter as INotifyPropertyChanged).PropertyChanged += SelectedImporter_PropertyChanged;
                }

                this.NotifyOfPropertyChange(nameof(this.SelectedImporter));
                this.NotifyOfPropertyChange(nameof(this.CanImport));
            }
        }

        public string Comment
        {
            get => this.comment ?? string.Empty;
            set
            {
                this.comment = value;
                this.NotifyOfPropertyChange(nameof(this.Comment));
                this.NotifyOfPropertyChange(nameof(this.CanImport));
            }
        }

        public bool CanImport
        {
            get
            {
                if (this.selectedImporter == null)
                    return false;

                if (this.Comment == string.Empty)
                    return false;

                return this.selectedImporter.CanImport;
            }
            private set
            {
                this.isImporting = !value;
                this.NotifyOfPropertyChange(nameof(this.CanImport));
                this.NotifyOfPropertyChange(nameof(this.CanTryClose));
                this.NotifyOfPropertyChange(nameof(this.CanCancel));
                this.NotifyOfPropertyChange(nameof(this.IsImporting));
            }
        }

        public bool CanTryClose => this.isImporting == false;

        public bool CanCancel => this.isImporting == true;

        public bool IsImporting => this.isImporting == true;

        private void SelectedImporter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IImporter.CanImport))
            {
                this.NotifyOfPropertyChange(nameof(this.CanImport));
            }
        }

        private void CreateTables(CremaDataSet dataSet, string[] names)
        {
            var tableCollection = this.dataBase.TableContext.Tables;
            var tableList = new List<ITable>(names.Length);
            var tablesByName = tableCollection.Dispatcher.Invoke(() => tableCollection.ToDictionary(item => item.Name));

            foreach (var item in names)
            {
                if (tablesByName.ContainsKey(item) == false)
                    throw new TableNotFoundException(item);
                tableList.Add(tableCollection[item]);
            }

            var tables = tableList.ToArray();

            var types = from item in tables
                        from i in EnumerableUtility.Friends(item, item.Childs)
                        from c in i.TableInfo.Columns
                        where c.DataType.StartsWith("/")
                        select c.DataType;

            var types1 = types.Distinct().ToArray();

            dataSet.BeginLoad();
            foreach (var item in types1)
            {
                var typeItem = dataBase.TypeContext[item];
                if (typeItem is IType == false)
                    continue;

                var type = typeItem as IType;

                dataSet.Types.Add(type.TypeInfo);
            }
            dataSet.EndLoad();
            foreach (var item in tables)
            {
                if (item.Parent != null)
                    continue;

                this.CreateTable(dataSet, item);
            }
        }

        private void CreateTable(CremaDataSet dataSet, ITable table)
        {
            var dataTable = dataSet.Tables.Add(GetTableInfo(table));
            foreach (var item in table.Childs)
            {
                dataTable.Childs.Add(GetTableInfo(item));
            }

            static TableInfo GetTableInfo(ITable tableItem)
            {
                var tableInfo = tableItem.TableInfo;
                if (tableInfo.TemplatedParent != string.Empty)
                    tableInfo.TemplatedParent = string.Empty;
                return tableInfo;
            }
        }

        [ConfigurationProperty("SelectedImporter")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:사용되지 않는 private 멤버 제거", Justification = "<보류 중>")]
        private string SelectedImporterName
        {
            get => this.selectedImporter?.Name;
            set
            {
                if (value != null)
                {
                    this.SelectedImporter = this.Importers.FirstOrDefault(item => item.Name == (string)value);
                }
            }
        }
    }
}

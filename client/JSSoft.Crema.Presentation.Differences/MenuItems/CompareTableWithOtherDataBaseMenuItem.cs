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

using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Diff;
using JSSoft.Crema.Presentation.Differences.Dialogs.ViewModels;
using JSSoft.Crema.Presentation.Differences.Properties;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Home.Dialogs.ViewModels;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Differences.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType("JSSoft.Crema.Presentation.Tables.BrowserItems.ViewModels.TableTreeViewItemViewModel, JSSoft.Crema.Presentation.Tables, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class CompareTableWithOtherDataBaseMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;
        private readonly ICremaHost cremaHost;
        private readonly ICremaAppHost cremaAppHost;

        [ImportingConstructor]
        public CompareTableWithOtherDataBaseMenuItem(Authenticator authenticator, ICremaHost cremaHost, ICremaAppHost cremaAppHost)
        {
            this.authenticator = authenticator;
            this.cremaHost = cremaHost;
            this.cremaAppHost = cremaAppHost;
            this.DisplayName = Resources.MenuItem_CompareWithOtherDataBase;
        }

        protected override async void OnExecute(object parameter)
        {
            var tableDescriptor = parameter as ITableDescriptor;
            var table = tableDescriptor.Target;
            var tableName = tableDescriptor.TableInfo.Name;

            var dataBaseName = await this.SelectDataBaseAsync();
            if (dataBaseName == null)
                return;

            var dataSet1 = await this.PreviewOtherTableAsync(dataBaseName, tableName);
            if (dataSet1 != null)
            {
                var dataSet2 = await table.GetDataSetAsync(this.authenticator, null);
                var dataSet = new DiffDataSet(dataSet1, dataSet2)
                {
                    Header1 = $"{dataBaseName}: {tableName}",
                    Header2 = $"{this.cremaAppHost.DataBaseName}: {tableName}",
                };

                var dialog = new DiffDataTableViewModel(dataSet.Tables.First())
                {
                    DisplayName = Resources.Title_CompareWithOtherDataBase,
                };
                await dialog.ShowDialogAsync();
            }
            else
            {
                await AppMessageBox.ShowAsync(string.Format(Resources.Message_TableNotFound_Format, tableName));
            }
        }

        private async Task<string> SelectDataBaseAsync()
        {
            var dialog = new SelectDataBaseViewModel(this.authenticator, this.cremaAppHost, (info) => info.Name != this.cremaAppHost.DataBaseName);
            if (await dialog.ShowDialogAsync() == true)
            {
                return dialog.SelectedValue;
            }
            return null;
        }

        private async Task<CremaDataSet> PreviewOtherTableAsync(string _1, string _2)
        {
            try
            {
                throw new NotImplementedException();
                //var dataBase = this.cremaHost item.DataBase;
                //if (dataBase.TableContext.Tables.Contains(tableName) == true)
                //{
                //    var table2 = dataBase.TableContext.Tables[tableName];
                //    return table2.GetDataSet(this.authenticator, null);
                //}
                //return null;

                //return this.cremaHost.Dispatcher.Invoke(() =>
                //{
                //    using (var item = UsingDataBase.Set(this.cremaHost, dataBaseName, this.authenticator))
                //    {
                //        var dataBase = item.DataBase;
                //        if (dataBase.TableContext.Tables.Contains(tableName) == true)
                //        {
                //            var table2 = dataBase.TableContext.Tables[tableName];
                //            return table2.GetDataSet(this.authenticator, null);
                //        }
                //        return null;
                //    }
                //});
            }
            catch (Exception e)
            {
                await AppMessageBox.ShowErrorAsync(e);
                return null;
            }
        }
    }
}

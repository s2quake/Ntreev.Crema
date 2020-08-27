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

using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Diff;
using Ntreev.Crema.Presentation.Differences.Dialogs.ViewModels;
using Ntreev.Crema.Presentation.Differences.Properties;
using Ntreev.Crema.Presentation.Framework;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Presentation.Differences.MenuItems
{
    [Export(typeof(IMenuItem))]
    [ParentType("Ntreev.Crema.Presentation.Types.BrowserItems.ViewModels.TypeTreeViewItemViewModel, Ntreev.Crema.Presentation.Types, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null")]
    class CompareTypeWithPrevRevisionMenuItem : MenuItemBase
    {
        private readonly Authenticator authenticator;

        [ImportingConstructor]
        public CompareTypeWithPrevRevisionMenuItem(Authenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public CompareTypeWithPrevRevisionMenuItem()
        {
            this.DisplayName = Resources.MenuItem_CompareWithPreviousResivision;
        }

        protected async override void OnExecute(object parameter)
        {
            var typeDescriptor = parameter as ITypeDescriptor;
            var type = typeDescriptor.Target;

            var dialog = new DiffDataTypeViewModel(this.Initialize(type))
            {
                DisplayName = Resources.Title_CompareWithPreviousResivision,
            };
            await dialog.ShowDialogAsync();
        }

        private async Task<DiffDataType> Initialize(IType type)
        {
            var logs = await type.GetLogAsync(this.authenticator, null);
            var hasRevision = logs.Length >= 2;
            var dataSet1 = hasRevision ? await type.GetDataSetAsync(this.authenticator, logs[1].Revision) : new CremaDataSet();
            var dataSet2 = await type.GetDataSetAsync(this.authenticator, null);
            var header1 = hasRevision ? $"[{logs[1].DateTime}] {logs[1].Revision}" : string.Empty;
            var dataSet = new DiffDataSet(dataSet1, dataSet2)
            {
                Header1 = header1,
                Header2 = Resources.Text_Current,
            };
            return dataSet.Types.First();
        }
    }
}

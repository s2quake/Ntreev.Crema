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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class LogInfoViewModel : ListBoxItemViewModel, IInfoProvider
    {
        private readonly Authentication authentication;
        private readonly ITableItem tableItem;
        private readonly LogInfo logInfo;

        public LogInfoViewModel(Authentication authentication, ITableItem tableItem, LogInfo logInfo)
        {
            this.authentication = authentication;
            this.tableItem = tableItem;
            this.logInfo = logInfo;
            this.Target = tableItem;
        }

        public async Task PreviewAsync()
        {
            if (this.tableItem is ITable table)
            {
                var dialog = new PreviewTableViewModel(this.authentication, table, this.logInfo.Revision);
                await dialog.ShowDialogAsync();
            }
            else if (this.tableItem is ITableCategory category)
            {
                var dialog = new PreviewTableCategoryViewModel(this.authentication, category, this.logInfo.Revision);
                await dialog.ShowDialogAsync();
            }
        }

        public LogInfo LogInfo => this.logInfo;

        public string UserID => this.logInfo.UserID;

        public string Revision => this.logInfo.Revision;

        public string Message => this.logInfo.Comment;

        public DateTime DateTime => this.logInfo.DateTime;

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => new Dictionary<string, object>()
                {
                    { nameof(UserID), this.UserID },
                    { nameof(Revision), this.Revision },
                    { nameof(Message), this.Message },
                    { nameof(DateTime), this.DateTime }
                };

        #endregion
    }
}

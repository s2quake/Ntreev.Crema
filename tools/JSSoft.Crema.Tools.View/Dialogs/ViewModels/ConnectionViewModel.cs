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

using JSSoft.Crema.Tools.Framework.Dialogs.ViewModels;
using JSSoft.Library;
using JSSoft.ModernUI.Framework;
using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Tools.View.Dialogs.ViewModels
{
    class ConnectionViewModel : ModalDialogBase
    {
        private string address = "localhost";
        private string dataBase = "master";
        private string tags = "All";
        private bool isDevmode;
        private string filterExpression;

        public ConnectionViewModel()
        {
            this.DisplayName = "연결 설정";
        }

        public async Task SelectDataBaseAsync()
        {
            var dialog = new DataBaseListViewModel(this.Address);
            if (await dialog.ShowDialogAsync() == true)
            {
                this.DataBase = dialog.SelectedItem.Value.Name;
            }
        }

        public async Task EditFilterExpressionAsync()
        {
            var dialog = new EditFilterExpressionViewModel()
            {
                FilterExpression = this.FilterExpression,
            };
            if (await dialog.ShowDialogAsync() == true)
            {
                this.FilterExpression = dialog.FilterExpression;
            }
        }

        public async Task ConnectAsync()
        {
            await this.TryCloseAsync(true);
        }

        [ConfigurationProperty("address")]
        public string Address
        {
            get => this.address;
            set
            {
                this.address = value;
                this.NotifyOfPropertyChange(() => this.Address);
                this.NotifyOfPropertyChange(() => this.CanConnect);
            }
        }

        [ConfigurationProperty("database")]
        public string DataBase
        {
            get => this.dataBase;
            set
            {
                this.dataBase = value;
                this.NotifyOfPropertyChange(() => this.DataBase);
                this.NotifyOfPropertyChange(() => this.CanConnect);
            }
        }

        [ConfigurationProperty("tags")]
        public string Tags
        {

/* 'JSSoft.Crema.Tools.View (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
            get { return this.tags; }
이후:
            get => this.tags; }
*/
            get => this.tags;
            set
            {
                this.tags = value;
                this.NotifyOfPropertyChange(() => this.Tags);
                this.NotifyOfPropertyChange(() => this.CanConnect);
            }
        }

        public bool CanConnect
        {
            get
            {
                if (this.tags == string.Empty)
                    return false;
                if (this.tags == TagInfo.Unused.ToString())
                    return false;
                if (this.dataBase == string.Empty)
                    return false;
                if (this.address == string.Empty)
                    return false;
                return true;
            }
        }

        [ConfigurationProperty("devmode")]
        [Obsolete]
        public bool IsDevmode
        {

/* 'JSSoft.Crema.Tools.View (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
            get { return this.isDevmode; }
이후:
            get => this.isDevmode; }
*/
            get => this.isDevmode;
            set
            {
                this.isDevmode = value;
                this.NotifyOfPropertyChange(() => this.IsDevmode);
            }
        }

        [ConfigurationProperty("filter")]
        public string FilterExpression
        {
            get => this.filterExpression ?? string.Empty;
            set
            {
                this.filterExpression = value;
                this.NotifyOfPropertyChange(() => this.FilterExpression);
            }
        }
    }
}

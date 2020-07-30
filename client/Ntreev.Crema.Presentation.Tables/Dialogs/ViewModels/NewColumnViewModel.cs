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

using System.Linq;
using Ntreev.Crema.Presentation.Tables.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using System.IO;
using Ntreev.Crema.Presentation.Framework;
using System.Threading.Tasks;
using System;
using Ntreev.Library.IO;
using Ntreev.ModernUI.Framework;
using Ntreev.Library.ObjectModel;
using Ntreev.Crema.Services.Extensions;

namespace Ntreev.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class NewColumnViewModel : ModalDialogBase
    {
        private string name;
        private string dataType;
        private bool isKey;

        private string comment;

        public NewColumnViewModel(string[] dataTypes)
        {
            this.DataTypes = dataTypes;
            this.DisplayName = "New Column";
        }

        public string Name
        {
            get => this.name ?? string.Empty;
            set
            {
                this.name = value;
                this.NotifyOfPropertyChange(nameof(this.Name));
            }
        }

        public string DataType
        {
            get => this.dataType;
            set
            {
                this.dataType = value;
                this.NotifyOfPropertyChange(nameof(this.DataType));
            }
        }

        public string[] DataTypes { get; }

        public string Comment
        {
            get => this.comment ?? string.Empty;
            set
            {
                this.comment = value;
                this.NotifyOfPropertyChange(nameof(this.Comment));
            }
        }

        public bool IsKey
        {
            get => this.isKey;
            set
            {
                this.isKey = value;
                this.NotifyOfPropertyChange(nameof(this.IsKey));
            }
        }

        public async Task InsertAsync()
        {
            await this.TryCloseAsync(true);
        }
    }
}

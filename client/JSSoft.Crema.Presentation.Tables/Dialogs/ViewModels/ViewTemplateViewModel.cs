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
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Presentation.Tables.Properties;
using JSSoft.Crema.Services;
using JSSoft.Library;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Tables.Dialogs.ViewModels
{
    public class ViewTemplateViewModel : ModalDialogAppBase
    {
        private readonly Authentication authentication;
        private readonly CremaTemplate template;
        private readonly string tableName;
        private readonly string comment;
        private TagInfo tags;

        public ViewTemplateViewModel(Authentication authentication, CremaTemplate template)
        {
            this.DisplayName = Resources.Title_ViewTableTemplate;
            this.authentication = authentication;
            this.template = template;
            this.tableName = this.template.TableName;
            this.comment = this.template.Comment;
            this.tags = this.template.Tags;
        }

        public static async Task<ViewTemplateViewModel> CreateInstanceAsync(Authentication authentication, ITableDescriptor descriptor)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.Target is ITable table)
            {
                var dataSet = await table.GetDataSetAsync(authentication, null);
                var dataTable = dataSet.Tables[table.Name];
                var template = new CremaTemplate(dataTable);
                return new ViewTemplateViewModel(authentication, template);
            }
            else
            {
                throw new ArgumentException("Invalid Target of Descriptor", nameof(descriptor));
            }
        }

        public object Source => this.template;

        public string TableName => this.tableName ?? string.Empty;

        public string Comment => this.comment ?? string.Empty;

        public TagInfo Tags => this.tags;
    }
}

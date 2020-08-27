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
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data;
using JSSoft.Crema.Services.Domains.Serializations;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace JSSoft.Crema.Services.Domains
{
    class TableTemplateDomain : Domain
    {
        public const string TypeName = nameof(TableTemplate);

        private readonly CremaTemplate template;
        private readonly DataView view;
        private byte[] data;

        public TableTemplateDomain(DomainSerializationInfo serializationInfo, object source)
            : base(serializationInfo, source)
        {
            this.IsNew = (bool)serializationInfo.GetProperty(nameof(IsNew));
            this.template = source as CremaTemplate;
            this.view = this.template.View;

            var dataSet = this.template.DataTable.DataSet;
            var itemPaths = (string)serializationInfo.GetProperty(nameof(ItemPaths));
            dataSet.SetItemPaths(StringUtility.Split(itemPaths, ';'));
        }

        public TableTemplateDomain(Authentication authentication, CremaTemplate templateSource, DataBase dataBase, string itemPath, string itemType)
            : base(authentication.ID, templateSource, dataBase.ID, itemPath, itemType)
        {
            this.template = templateSource;
            this.view = this.template.View;
        }

        public bool IsNew { get; set; }

        public string[] ItemPaths => this.template.DataTable.DataSet.GetItemPaths();

        protected override byte[] SerializeSource(object source)
        {
            if (this.data == null)
            {
                var xml = XmlSerializerUtility.GetString(source);
                this.data = Encoding.UTF8.GetBytes(xml.Compress());
            }
            return this.data;
        }

        protected override object DerializeSource(byte[] data)
        {
            var xml = Encoding.UTF8.GetString(data).Decompress();
            return XmlSerializerUtility.ReadString<CremaTemplate>(xml);
        }

        protected override void OnSerializaing(IDictionary<string, object> properties)
        {
            base.OnSerializaing(properties);
            properties.Add(nameof(IsNew), this.IsNew);
            properties.Add(nameof(this.ItemPaths), string.Join(";", this.ItemPaths));
        }

        protected override DomainRowInfo[] OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.template.SignatureDateProvider = signatureProvider;
            for (var i = 0; i < rows.Length; i++)
            {
                var rowView = CremaDomainUtility.AddNew(this.view, rows[i].Fields);
                rows[i].Keys = CremaDomainUtility.GetKeys(rowView);
                rows[i].Fields = CremaDomainUtility.GetFields(rowView);
            }
            this.data = null;
            return rows;
        }

        protected override DomainRowInfo[] OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.template.SignatureDateProvider = signatureProvider;
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i].Fields = CremaDomainUtility.SetFields(this.view, rows[i].Keys, rows[i].Fields);
            }
            this.data = null;
            return rows;
        }

        protected override DomainRowInfo[] OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.template.SignatureDateProvider = signatureProvider;
            foreach (var item in rows)
            {
                CremaDomainUtility.Delete(this.view, item.Keys);
            }
            this.data = null;
            return rows;
        }

        protected override void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDateProvider signatureProvider)
        {
            if (propertyName == CremaSchema.TableName)
            {
                if (this.IsNew == false)
                    throw new InvalidOperationException(Resources.Exception_CannotRename);
                this.template.TableName = (string)value;
            }
            else if (propertyName == CremaSchema.Comment)
            {
                this.template.Comment = (string)value;
            }
            else if (propertyName == CremaSchema.Tags)
            {
                this.template.Tags = (TagInfo)((string)value);
            }
            else
            {
                throw new NotSupportedException();
            }
            this.data = null;
        }
    }
}

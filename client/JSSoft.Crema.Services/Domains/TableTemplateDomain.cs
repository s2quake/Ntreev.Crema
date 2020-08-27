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
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.Serialization;
using System;
using System.Data;
using System.Text;

namespace JSSoft.Crema.Services.Domains
{
    class TableTemplateDomain : Domain
    {
        private DataView view;

        public TableTemplateDomain(DomainInfo domainInfo)
            : base(domainInfo)
        {
            if (domainInfo.ItemType == nameof(NewTableTemplate))
                this.IsNew = true;
        }

        public bool IsNew { get; set; }

        public override object Source => this.TemplateSource;

        public CremaTemplate TemplateSource { get; private set; }

        protected override byte[] SerializeSource()
        {
            var xml = XmlSerializerUtility.GetString(this.TemplateSource);
            return Encoding.UTF8.GetBytes(xml.Compress());
        }

        protected override void DerializeSource(byte[] data)
        {
            var xml = Encoding.UTF8.GetString(data).Decompress();
            this.TemplateSource = XmlSerializerUtility.ReadString<CremaTemplate>(xml);
        }

        protected override void OnInitialize(byte[] data)
        {
            base.OnInitialize(data);

            var xml = Encoding.UTF8.GetString(data).Decompress();
            this.TemplateSource = XmlSerializerUtility.ReadString<CremaTemplate>(xml);
            this.view = this.TemplateSource.View;
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            this.TemplateSource = null;
            this.view = null;
        }

        protected override void OnDeleted(EventArgs e)
        {
            base.OnDeleted(e);
        }

        protected override void OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.TemplateSource.SignatureDateProvider = new InternalSignatureDateProvider(signatureDate);
            foreach (var item in rows)
            {
                CremaDomainUtility.AddNew(this.view, item.Fields);
            }
            this.TemplateSource.AcceptChanges();
        }

        protected override void OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.TemplateSource.SignatureDateProvider = new InternalSignatureDateProvider(signatureDate);
            foreach (var item in rows)
            {
                CremaDomainUtility.Delete(this.view, item.Keys);
            }
            this.TemplateSource.AcceptChanges();
        }

        protected override void OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.TemplateSource.SignatureDateProvider = new InternalSignatureDateProvider(signatureDate);
            foreach (var item in rows)
            {
                CremaDomainUtility.SetFieldsForce(this.view, item.Keys, item.Fields);
            }
            this.TemplateSource.AcceptChanges();
        }

        protected override void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDate signatureDate)
        {
            this.TemplateSource.SignatureDateProvider = new InternalSignatureDateProvider(signatureDate);
            if (propertyName == CremaSchema.TableName)
            {
                if (this.IsNew == false)
                    throw new InvalidOperationException(Resources.Exception_CannotRename);
                this.TemplateSource.TableName = (string)value;
            }
            else if (propertyName == CremaSchema.Comment)
            {
                this.TemplateSource.Comment = (string)value;
            }
            else if (propertyName == CremaSchema.Tags)
            {
                this.TemplateSource.Tags = (TagInfo)((string)value);
            }
            else
            {
                if (propertyName == null)
                    throw new ArgumentNullException(nameof(propertyName));
                throw new ArgumentException(string.Format(Resources.Exception_InvalidProperty_Format, propertyName), nameof(propertyName));
            }
        }
    }
}

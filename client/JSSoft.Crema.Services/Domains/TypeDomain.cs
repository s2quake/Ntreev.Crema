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
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Serialization;
using System;
using System.Data;
using System.Text;

namespace JSSoft.Crema.Services.Domains
{
    class TypeDomain : Domain
    {
        private CremaDataType dataType;
        private DataView view;

        public TypeDomain(DomainInfo domainInfo)
            : base(domainInfo)
        {
            if (domainInfo.ItemType == nameof(NewTypeTemplate))
                this.IsNew = true;
        }

        public override object Source => this.dataType;

        public bool IsNew { get; set; }

        protected override byte[] SerializeSource()
        {
            var text = this.dataType.Path + ";" + XmlSerializerUtility.GetString(this.dataType.DataSet);
            return Encoding.UTF8.GetBytes(text.Compress());
        }

        protected override void DerializeSource(byte[] data)
        {
            var text = Encoding.UTF8.GetString(data).Decompress();
            var index = text.IndexOf(";");
            var path = text.Remove(index);
            var itemName = new ItemName(path);
            var xml = text.Substring(index + 1);
            var dataSet = XmlSerializerUtility.ReadString<CremaDataSet>(xml);
            this.dataType = dataSet.Types[itemName.Name];
        }

        protected override void OnInitialize(byte[] data)
        {
            base.OnInitialize(data);

            var text = Encoding.UTF8.GetString(data).Decompress();
            var index = text.IndexOf(";");
            var path = text.Remove(index);
            var itemName = new ItemName(path);
            var xml = text.Substring(index + 1);
            var dataSet = XmlSerializerUtility.ReadString<CremaDataSet>(xml);
            this.dataType = dataSet.Types[itemName.Name];
            this.view = this.dataType.AsDataView();
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            this.dataType = null;
            this.view = null;
        }

        protected override void OnDeleted(EventArgs e)
        {
            base.OnDeleted(e);
        }

        protected override void OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.dataType.BeginLoadData();
            try
            {
                foreach (var item in rows)
                {
                    CremaDomainUtility.AddNew(this.view, item.Fields);
                }
            }
            finally
            {
                this.dataType.EndLoadData();
            }
            this.dataType.ModificationInfo = signatureDate;
            this.dataType.AcceptChanges();
        }

        protected override void OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.dataType.BeginLoadData();
            try
            {
                foreach (var item in rows)
                {
                    CremaDomainUtility.Delete(this.view, item.Keys);
                }
            }
            finally
            {
                this.dataType.EndLoadData();
            }
            this.dataType.ModificationInfo = signatureDate;
            this.dataType.AcceptChanges();
        }

        protected override void OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {
            this.dataType.BeginLoadData();
            try
            {
                foreach (var item in rows)
                {
                    CremaDomainUtility.SetFieldsForce(this.view, item.Keys, item.Fields);
                }
            }
            finally
            {
                this.dataType.EndLoadData();
            }
            this.dataType.ModificationInfo = signatureDate;
            this.dataType.AcceptChanges();
        }

        protected override void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDate signatureDate)
        {
            if (propertyName == CremaSchema.TypeName)
            {
                if (this.IsNew == false)
                    throw new InvalidOperationException(Resources.Exception_CannotRename);
                this.dataType.TypeName = (string)value;
            }
            else if (propertyName == CremaSchema.IsFlag)
            {
                this.dataType.IsFlag = (bool)value;
            }
            else if (propertyName == CremaSchema.Comment)
            {
                this.dataType.Comment = (string)value;
            }
            else
            {
                if (propertyName == null)
                    throw new ArgumentNullException(nameof(propertyName));
                throw new ArgumentException(string.Format(Resources.Exception_InvalidProperty_Format, propertyName), nameof(propertyName));
            }
            this.dataType.ModificationInfo = signatureDate;
        }
    }
}

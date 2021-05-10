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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data;
using JSSoft.Crema.Services.Domains.Serializations;
using JSSoft.Crema.Services.Properties;
using JSSoft.Library;
using JSSoft.Library.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace JSSoft.Crema.Services.Domains
{
    class TableContentDomain : Domain
    {
        private readonly CremaDataSet dataSet;
        private readonly Dictionary<string, DataView> views = new();
        private byte[] data;

        public TableContentDomain(DomainSerializationInfo serializationInfo, object source)
            : base(serializationInfo, source)
        {
            this.dataSet = source as CremaDataSet;
            foreach (var item in this.dataSet.Tables)
            {
                var view = item.AsDataView();
                this.views.Add(item.Name, view);
            }

            var itemPaths = (string)serializationInfo.GetProperty(nameof(ItemPaths));
            this.dataSet.SetItemPaths(StringUtility.Split(itemPaths, ';'));
        }

        public TableContentDomain(Authentication authentication, CremaDataSet dataSet, DataBase dataBase, string itemPath, string itemType, IDomainHost domainHost)
            : base(authentication.ID, dataSet, dataBase.ID, itemPath, itemType)
        {
            if (dataSet.HasChanges() == true)
                throw new ArgumentException(Resources.Exception_UnsavedDataCannotEdit, nameof(dataSet));
            this.dataSet = dataSet;
            foreach (var item in this.dataSet.Tables)
            {
                var view = item.AsDataView();
                this.views.Add(item.Name, view);
            }
            this.Host = domainHost;
        }

        public string[] ItemPaths => this.dataSet.GetItemPaths();

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
            return XmlSerializerUtility.ReadString<CremaDataSet>(xml);
        }

        protected override void OnSerializaing(IDictionary<string, object> properties)
        {
            base.OnSerializaing(properties);
            properties.Add(nameof(this.ItemPaths), string.Join(";", this.ItemPaths));
        }

        protected override DomainRowInfo[] OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.dataSet.SignatureDateProvider = signatureProvider;
            try
            {
                for (var i = 0; i < rows.Length; i++)
                {
                    var view = this.views[rows[i].TableName];
                    var rowView = CremaDomainUtility.AddNew(view, rows[i].Fields);
                    rows[i].Keys = CremaDomainUtility.GetKeys(rowView);
                    rows[i].Fields = CremaDomainUtility.GetFields(rowView);
                    rows[i].Target = rowView.Row;
                }
                this.dataSet.AcceptChanges();
                this.data = null;
                return rows;
            }
            catch
            {
                this.dataSet.RejectChanges();
                throw;
            }
        }

        protected override DomainRowInfo[] OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.dataSet.SignatureDateProvider = signatureProvider;
            try
            {
                for (var i = 0; i < rows.Length; i++)
                {
                    var view = this.views[rows[i].TableName];
                    rows[i].Fields = CremaDomainUtility.SetFields(view, rows[i].Keys, rows[i].Fields);
                }
                this.dataSet.AcceptChanges();
                this.data = null;
                return rows;
            }
            catch
            {
                this.dataSet.RejectChanges();
                throw;
            }
        }

        protected override DomainRowInfo[] OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDateProvider signatureProvider)
        {
            this.dataSet.SignatureDateProvider = signatureProvider;
            try
            {
                for (var i = 0; i < rows.Length; i++)
                {
                    var view = this.views[rows[i].TableName];
                    if (DomainRowInfo.ClearKey.SequenceEqual(rows[i].Keys) == true)
                    {
                        view.Table.Clear();
                    }
                    else
                    {
                        CremaDomainUtility.Delete(view, rows[i].Keys);
                    }
                }
                this.dataSet.AcceptChanges();
                this.data = null;
                return rows;
            }
            catch
            {
                this.dataSet.RejectChanges();
                throw;
            }
        }
    }
}

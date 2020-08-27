﻿//Released under the MIT License.
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
using JSSoft.Crema.Data.Xml;
using JSSoft.Library;
using System;
using System.Runtime.Serialization;

namespace JSSoft.Crema.ServiceModel
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    public struct DomainFieldInfo
    {
        public DomainFieldInfo(object field)
        {
            if (field is DBNull)
            {
                this.Type = nameof(DBNull);
                this.Value = null;
            }
            else if (field != null)
            {
                if (field is Guid)
                    this.Type = nameof(Guid);
                else
                    this.Type = field.GetType().GetTypeName();
                this.Value = CremaXmlConvert.ToString(field);
            }
            else
            {
                this.Type = null;
                this.Value = null;
            }
        }

        public object ToValue()
        {
            if (this.Value != null)
            {
                var type = this.Type == nameof(Guid) ? typeof(Guid) : CremaDataTypeUtility.GetType(this.Type);
                return CremaXmlConvert.ToValue(this.Value, type);
            }
            else if (this.Type == nameof(DBNull))
            {
                return DBNull.Value;
            }
            else
            {
                return null;
            }
        }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}

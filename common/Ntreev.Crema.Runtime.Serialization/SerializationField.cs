﻿using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Runtime.Serialization
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    public struct SerializationField
    {
        private static readonly CultureInfo cultureInfo = new CultureInfo("en-US");

        public SerializationField(object field)
        {
            if (field is DBNull)
            {
                this.Type = nameof(DBNull);
                this.Value = null;
            }
            else if (field is DateTime dateTime)
            {
                this.Type = field.GetType().GetTypeName();
                this.Value = dateTime.ToString(cultureInfo);
            }
            else if (field != null)
            {
                this.Type = field.GetType().GetTypeName();
                this.Value = CremaConvert.ToString(field);
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
                var type = CremaDataTypeUtility.GetType(this.Type);
                return CremaConvert.ChangeType(this.Value, type);
            }
            else if (this.Type == typeof(DateTime).GetTypeName())
            {
                return DateTime.Parse(this.Value, cultureInfo);
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

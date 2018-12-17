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

using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains.Serializations
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    struct DomainCompletionItemSerializationInfo
    {
        public DomainCompletionItemSerializationInfo(long id, string userID, DateTime dateTime, Type type)
        {
            this.ID = id;
            this.UserID = userID;
            this.DateTime = dateTime;
            this.Type = type.AssemblyQualifiedName;
        }

        public override string ToString()
        {
            return $"{this.ID}\t{this.UserID}\t{this.DateTime:o}\t{this.Type}";
        }

        public static DomainCompletionItemSerializationInfo Parse(string text)
        {
            var items = StringUtility.Split(text, '\t');
            return new DomainCompletionItemSerializationInfo()
            {
                ID = long.Parse(items[0]),
                UserID = items[1],
                DateTime = DateTime.Parse(items[1]),
                Type = items[2],
            };
        }

        [DataMember]
        public long ID { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}

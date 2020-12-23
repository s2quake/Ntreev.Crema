﻿// Released under the MIT License.
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

using JSSoft.Library;
using System.Linq;
using System.Runtime.Serialization;

namespace JSSoft.Crema.ServiceModel
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    public struct DomainRowInfo
    {
        [DataMember]
        public string TableName { get; set; }

        [IgnoreDataMember]
        public object[] Fields { get; set; }

        [IgnoreDataMember]
        public object[] Keys { get; set; }

        [DataMember]
        public DomainFieldInfo[] FieldInfos
        {
            get => this.Fields != null ? this.Fields.Select(item => new DomainFieldInfo(item)).ToArray() : new DomainFieldInfo[] { };
            set => this.Fields = value?.Select(item => item.ToValue()).ToArray();
        }

        [DataMember]
        public DomainFieldInfo[] KeyInfos
        {
            get => this.Keys != null ? this.Keys.Select(item => new DomainFieldInfo(item)).ToArray() : new DomainFieldInfo[] { };
            set => this.Keys = value?.Select(item => item.ToValue()).ToArray();
        }

        [IgnoreDataMember]
        public object Target { get; set; }

        public static readonly DomainRowInfo Empty;

        internal static readonly string[] ClearKey = new string[] { "6B924529-8134-463D-A040-1632BCE6813A", "F1405371-7961-4A6D-9DDA-D66838617F41" };
    }
}
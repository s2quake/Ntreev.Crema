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

using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.Library.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JSSoft.Crema.ServiceModel
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    public struct UserInfo
    {
        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public Authority Authority { get; set; }

        [DataMember]
        public SignatureDate CreationInfo { get; set; }

        [DataMember]
        public SignatureDate ModificationInfo { get; set; }

        [IgnoreDataMember]
        public string CategoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(this.CategoryName) == true)
                    return PathUtility.Separator;
                return PathUtility.Separator + this.CategoryName + PathUtility.Separator;
            }
            set
            {
                NameValidator.ValidateCategoryPath(value);
                if (value == PathUtility.Separator)
                    this.CategoryName = string.Empty;
                else
                    this.CategoryName = value.Trim(PathUtility.SeparatorChar);
            }
        }

        public string Path => this.CategoryPath + this.ID;

        public IDictionary<string, object> ToDictionary()
        {
            var props = new Dictionary<string, object>
            {
                { nameof(this.ID), this.ID },
                { nameof(this.Name), this.Name },
                { nameof(this.CategoryName), this.CategoryName },
                { nameof(this.Authority), $"{this.Authority}" },
                { CremaSchema.Creator, this.CreationInfo.ID },
                { CremaSchema.CreatedDateTime, this.CreationInfo.DateTime },
                { CremaSchema.Modifier, this.ModificationInfo.ID },
                { CremaSchema.ModifiedDateTime, this.ModificationInfo.DateTime }
            };
            return props;
        }

        public static readonly UserInfo Empty = new UserInfo()
        {
            ID = string.Empty,
            Name = string.Empty,
            CategoryName = string.Empty,
            CreationInfo = SignatureDate.Empty,
            ModificationInfo = SignatureDate.Empty
        };
    }
}

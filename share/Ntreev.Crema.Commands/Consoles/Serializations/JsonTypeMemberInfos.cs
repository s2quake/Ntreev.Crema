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

using Newtonsoft.Json;
using Ntreev.Library;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ntreev.Crema.Commands.Consoles.Serializations
{
    struct JsonTypeMemberInfos
    {
        private static readonly ItemInfo[] emptyItems = new ItemInfo[] { };
        private ItemInfo[] items;

        [JsonProperty]
        public ItemInfo[] Items
        {
            get => this.items ?? emptyItems;
            set => this.items = value;
        }

        public struct ItemInfo
        {
            private string comment;

            [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public Guid ID { get; set; }

            [JsonProperty(Required = Required.Always)]
            [RegularExpression(IdentifierValidator.IdentiFierPattern)]
            public string Name { get; set; }

            [JsonProperty(Required = Required.Always)]
            public long Value { get; set; }

            [JsonProperty(Required = Required.DisallowNull, DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue("")]
            public string Comment
            {
                get => this.comment ?? string.Empty;
                set => this.comment = value;
            }
        }
    }
}
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
using JSSoft.Crema.ServiceModel;
using JSSoft.Library;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class TypeMember : DomainBasedRow, ITypeMember
    {
        private readonly TypeTemplateBase template;

        public TypeMember(TypeTemplateBase template, DataRow row)
            : base(template.Domain, row)
        {
            this.template = template;
        }

        private TypeMember(TypeTemplateBase template, DataTable table)
            : base(template.Domain, table)
        {
            this.template = template;
        }

        public static async Task<TypeMember> CreateAsync(Authentication authentication, TypeTemplateBase template, DataTable table)
        {
            var domain = template.Domain;
            var tuple = await domain.Dispatcher.InvokeAsync(() =>
            {
                var member = new TypeMember(template, table);
                var query = from DataRow item in table.Rows
                            select item.Field<string>(CremaSchema.Name);

                var newName = NameUtility.GenerateNewName("Member", query);
                return (member, newName);
            });
            await tuple.member.SetFieldAsync(authentication, CremaSchema.Name, tuple.newName);
            return tuple.member;
        }

        public Task SetIndexAsync(Authentication authentication, int value)
        {
            return this.SetFieldAsync(authentication, CremaSchema.Index, value);
        }

        public Task SetNameAsync(Authentication authentication, string value)
        {
            return this.SetFieldAsync(authentication, CremaSchema.Name, value);
        }

        public Task SetValueAsync(Authentication authentication, long value)
        {
            return this.SetFieldAsync(authentication, CremaSchema.Value, value);
        }

        public Task SetCommentAsync(Authentication authentication, string value)
        {
            return this.SetFieldAsync(authentication, CremaSchema.Comment, value);
        }

        public int Index => this.GetField<int>(CremaSchema.Index);

        public string Name => this.GetField<string>(CremaSchema.Name);

        public long Value => this.GetField<long>(CremaSchema.Value);

        public string Comment => this.GetField<string>(CremaSchema.Comment);

        public override DataBase DataBase => this.template.DataBase;

        public override CremaDispatcher Dispatcher => this.template.Dispatcher;

        public override CremaHost CremaHost => this.template.CremaHost;

        #region ITypeTemplate

        ITypeTemplate ITypeMember.Template => this.template;

        #endregion
    }
}

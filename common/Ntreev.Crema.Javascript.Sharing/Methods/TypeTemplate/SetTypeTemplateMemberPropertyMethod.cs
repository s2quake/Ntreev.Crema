//Released under the MIT License.
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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Javascript.Methods.TypeTemplate
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(TypeTemplate))]
    class SetTypeTemplateMemberPropertyMethod : DomainScriptMethodBase
    {
        [ImportingConstructor]
        public SetTypeTemplateMemberPropertyMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override Delegate CreateDelegate()
        {
            return new Action<string, string, TypeMemberProperties, object>(this.SetTypeMemberTemplateProperty);
        }

        private void SetTypeMemberTemplateProperty(string domainID, string memberName, TypeMemberProperties propertyName, object value)
        {
            var template = this.GetDomainHost<ITypeTemplate>(domainID);
            var authentication = this.Context.GetAuthentication(this);
            var task = InvokeAsync();
            task.Wait();

            async Task InvokeAsync()
            {
                var member = template[memberName];
                if (member == null)
                    throw new ItemNotFoundException(memberName);
                if (propertyName == TypeMemberProperties.Name)
                {
                    await member.SetNameAsync(authentication, (string)value);
                }
                else if (propertyName == TypeMemberProperties.Value)
                {
                    await member.SetValueAsync(authentication, Convert.ToInt64(value));
                }
                else if (propertyName == TypeMemberProperties.Comment)
                {
                    await member.SetCommentAsync(authentication, (string)value);
                }
                else
                {
                    throw new NotImplementedException();
                }
            };
        }
    }
}

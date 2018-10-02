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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class NewTypeTemplate : TypeTemplateBase
    {
        private readonly TypeCategory category;
        private Type type;

        public NewTypeTemplate(TypeCategory category)
        {
            this.category = category ?? throw new ArgumentNullException(nameof(category));
            this.DispatcherObject = category;
            this.category.Attach(this);
            this.IsNew = true;
        }

        public override AccessType GetAccessType(Authentication authentication)
        {
            return this.category.GetAccessType(authentication);
        }

        public override void OnValidateBeginEdit(Authentication authentication, object target)
        {
            base.OnValidateBeginEdit(authentication, target);
            if (this.category == null)
                throw new InvalidOperationException(Resources.Exception_Expired);
            if (this.Domain != null)
                throw new InvalidOperationException(Resources.Exception_ItIsAlreadyBeingEdited);
        }

        public override void OnValidateEndEdit(Authentication authentication, object target)
        {
            base.OnValidateEndEdit(authentication, target);
        }

        public override void OnValidateCancelEdit(Authentication authentication, object target)
        {
            base.OnValidateCancelEdit(authentication, target);
        }

        public override IType Type => this.type;

        public override DomainContext DomainContext => this.category.GetService(typeof(DomainContext)) as DomainContext;

        public override string Path => this.category.Path;

        public override CremaHost CremaHost => this.category.CremaHost;

        public override DataBase DataBase => this.category.DataBase;

        public override IPermission Permission => this.category;

        public override IDispatcherObject DispatcherObject { get; }

        public TypeCollection Types => this.category.Context.Types;

        protected override async Task OnBeginEditAsync(Authentication authentication)
        {
            await base.OnBeginEditAsync(authentication);
        }

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            this.type = await this.Types.AddNewAsync(authentication, this.TypeSource);
            await base.OnEndEditAsync(authentication);
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
        }

        protected override async Task<CremaDataType> CreateSourceAsync(Authentication authentication)
        {
            var dataSet = await this.category.ReadDataForNewTemplateAsync(authentication);
            var typeName = NameUtility.GenerateNewName(nameof(Type), this.Types.Select((Type item) => item.Name).ToArray());
            var dataType = dataSet.Types.Add();
            dataType.TypeName = typeName;
            dataType.CategoryPath = this.category.Path;
            return dataType;
        }

        private DataBaseRepositoryHost Repository => this.category.Repository;
    }
}

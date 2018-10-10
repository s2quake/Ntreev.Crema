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
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class TypeTemplate : TypeTemplateBase
    {
        private readonly Type type;
        private readonly Type[] types;
        private TypeInfo typeInfo;
        private string[] itemPaths;

        public TypeTemplate(Type type)
        {
            this.type = type;
            this.types = new Type[] { type };
        }

        public override AccessType GetAccessType(Authentication authentication)
        {
            return this.type.GetAccessType(authentication);
        }

        public override DomainContext DomainContext => this.type.GetService(typeof(DomainContext)) as DomainContext;

        public override string Path => this.type.Path;

        public override CremaHost CremaHost => this.type.CremaHost;

        public override IType Type => this.type;

        public override DataBase DataBase => this.type.DataBase;

        public override IDispatcherObject DispatcherObject => this.type;

        public override IPermission Permission => this.type;

        protected override async Task OnBeginEditAsync(Authentication authentication)
        {
            await base.OnBeginEditAsync(authentication);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.type.IsBeingEdited = true;
                this.Container.InvokeTypesStateChangedEvent(authentication, this.types);
            });
        }

        protected override async Task<TypeInfo[]> OnEndEditAsync(Authentication authentication, TypeInfo[] typeInfos)
        {
            var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, this.TypeSource.DataSet, false);
            typeInfos = new TypeInfo[] { this.TypeSource.TypeInfo };
            await this.Container.InvokeTypeEndTemplateEditAsync(authentication, this.type.Name, dataBaseSet);
            await base.OnEndEditAsync(authentication, typeInfos);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.type.UpdateTypeInfo(this.typeInfo);
                this.type.IsBeingEdited = false;
                this.Container.InvokeTypesStateChangedEvent(authentication, this.types);
                this.Container.InvokeTypesChangedEvent(authentication, this.types, dataBaseSet.DataSet);
            });
            return typeInfos;
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
            await this.Repository.UnlockAsync(this.itemPaths);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.type.IsBeingEdited = false;
                this.Container.InvokeTypesStateChangedEvent(authentication, new Type[] { this.type });
            });
        }

        protected override async Task OnRestoreAsync(Domain domain)
        {
            this.type.IsBeingEdited = true;
            await base.OnRestoreAsync(domain);
        }

        protected override async Task<CremaDataType> CreateSourceAsync(Authentication authentication)
        {
            var typePath = this.type.Path;
            var dataSet = await this.type.ReadDataForTypeTemplateAsync(authentication);
            var dataType = dataSet.Types[this.type.Name, this.type.Category.Path];
            if (dataType == null)
                throw new TypeNotFoundException(typePath);
            this.typeInfo = this.type.TypeInfo;
            this.itemPaths = dataSet.ExtendedProperties[nameof(DataBaseSet.ItemPaths)] as string[] ?? new string[] { };
            return dataType;
        }

        private TypeCollection Container => this.type.Container;

        private DataBaseRepositoryHost Repository => this.type.Repository;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateBeginEdit(Authentication authentication, object target)
        {
            base.OnValidateBeginEdit(authentication, target);
            this.type.ValidateIsNotBeingEdited();
            this.type.ValidateAccessType(authentication, AccessType.Master);
            this.type.ValidateUsingTables(authentication);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateEndEdit(Authentication authentication, object target)
        {
            base.OnValidateEndEdit(authentication, target);
            this.type.ValidateIsBeingEdited();
            if (this.TypeSource == null)
                throw new InvalidOperationException(Resources.Exception_CannotEndEdit);
            this.type.ValidateUsingTables(authentication);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateCancelEdit(Authentication authentication, object target)
        {
            base.OnValidateCancelEdit(authentication, target);
            this.type.ValidateIsBeingEdited();
            this.type.ValidateAccessType(authentication, AccessType.Master);
        }
    }
}

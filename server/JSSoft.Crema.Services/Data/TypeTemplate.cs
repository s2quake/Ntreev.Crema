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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class TypeTemplate : TypeTemplateBase
    {
        private readonly Type type;
        private readonly Type[] types;

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
                this.type.TypeState = TypeState.IsBeingEdited;
                this.Container.InvokeTypesStateChangedEvent(authentication, this.types);
            });
        }

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, this.TypeSource.DataSet);
            var typeInfo = this.TypeSource.TypeInfo;
            var taskID = this.Domain.ID;
            this.Domain.Result = new TypeInfo[] { typeInfo };
            await this.Container.InvokeTypeEndTemplateEditAsync(authentication, this.type.Name, dataBaseSet);
            await base.OnEndEditAsync(authentication);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.type.UpdateTypeInfo(typeInfo);
                this.type.TypeState = TypeState.None;
                this.Container.InvokeTypesStateChangedEvent(authentication, this.types);
                this.Container.InvokeTypesChangedEvent(authentication, this.types, dataBaseSet.DataSet);
                this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
            });
            await this.Repository.UnlockAsync(authentication, this, nameof(OnEndEditAsync), this.ItemPaths);
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.type.TypeState = TypeState.None;
                this.Container.InvokeTypesStateChangedEvent(authentication, this.types);
            });
            await this.Repository.UnlockAsync(authentication, this, nameof(OnCancelEditAsync), this.ItemPaths);
        }

        protected override void OnAttach(Domain domain)
        {
            this.type.TypeState = TypeState.IsBeingEdited;
            base.OnAttach(domain);
        }

        protected override async Task<CremaDataType> CreateSourceAsync(Authentication authentication)
        {
            var typePath = this.type.Path;
            var dataSet = await this.type.ReadDataForTypeTemplateAsync(authentication);
            var dataType = dataSet.Types[this.type.Name, this.type.Category.Path];
            if (dataType == null)
                throw new TypeNotFoundException(typePath);
            return dataType;
        }

        private TypeCollection Container => this.type.Container;

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

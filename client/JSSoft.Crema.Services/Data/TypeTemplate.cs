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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceHosts.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class TypeTemplate : TypeTemplateBase
    {
        private readonly Type type;

        public TypeTemplate(Type type)
        {
            this.type = type;
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
            this.type.TypeState = TypeState.IsBeingEdited;
            this.Container.InvokeTypesStateChangedEvent(authentication, new Type[] { this.type, });
        }

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            var domain = this.Domain;
            var taskID = domain.ID;
            await base.OnEndEditAsync(authentication);
            await this.DataBase.WaitAsync(taskID);
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
        }

        protected override void OnAttach(Domain domain)
        {
            this.type.TypeState = TypeState.IsBeingEdited;
            base.OnAttach(domain);
        }

        protected override Task<ResultBase<DomainMetaData>> OnBeginDomainAsync(Authentication authentication)
        {
            return this.Service.BeginTypeTemplateEditAsync(authentication.Token, this.type.Name);
        }

        protected override async Task<ResultBase<TypeInfo[]>> OnEndDomainAsync(Authentication authentication)
        {
            return await this.Service.EndTypeTemplateEditAsync(authentication.Token, this.Domain.ID);
        }

        protected override async Task<ResultBase> OnCancelDomainAsync(Authentication authentication)
        {
            return await this.Service.CancelTypeTemplateEditAsync(authentication.Token, this.Domain.ID);
        }

        private TypeCollection Container => this.type.Container;

        private IDataBaseService Service => this.type.Service;
    }
}

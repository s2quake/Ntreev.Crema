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
    class NewTableTemplate : TableTemplateBase
    {
        private object parent;
        private Table[] tables;

        public NewTableTemplate(TableCategory category)
        {
            this.parent = category ?? throw new ArgumentNullException(nameof(category));
            this.DispatcherObject = category;
            this.DomainContext = category.GetService(typeof(DomainContext)) as DomainContext;
            this.ItemPath = category.Path;
            this.CremaHost = category.CremaHost;
            this.DataBase = category.DataBase;
            this.Permission = category;
            this.IsNew = true;
            category.Attach(this);
        }

        public NewTableTemplate(Table parent)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.DispatcherObject = parent;
            this.DomainContext = parent.GetService(typeof(DomainContext)) as DomainContext;
            this.ItemPath = parent.Path;
            this.CremaHost = parent.CremaHost;
            this.DataBase = parent.DataBase;
            this.Permission = parent;
            this.IsNew = true;
            parent.Attach(this);
        }

        public override void OnValidateBeginEdit(Authentication authentication, object target)
        {
            base.OnValidateBeginEdit(authentication, target);
            if (this.parent == null)
                throw new InvalidOperationException(Resources.Exception_Expired);
            if (this.Domain != null)
                throw new InvalidOperationException(Resources.Exception_ItIsAlreadyBeingEdited);
            if (this.parent is TableCategory category)
            {
                category.ValidateAccessType(authentication, AccessType.Master);
            }
            else if (this.parent is Table table)
            {
                table.ValidateAccessType(authentication, AccessType.Master);
            }
        }

        public override void OnValidateEndEdit(Authentication authentication, object target)
        {
            base.OnValidateEndEdit(authentication, target);
            if (this.parent is TableCategory category)
            {
                category.ValidateAccessType(authentication, AccessType.Master);
            }
            else if (this.parent is Table table)
            {
                table.ValidateAccessType(authentication, AccessType.Master);
            }
            this.TemplateSource.Validate();
        }

        public override void OnValidateCancelEdit(Authentication authentication, object target)
        {
            base.OnValidateCancelEdit(authentication, target);
            if (this.parent is TableCategory category)
            {
                category.ValidateAccessType(authentication, AccessType.Master);
            }
            else if (this.parent is Table table)
            {
                table.ValidateAccessType(authentication, AccessType.Master);
            }
        }

        public override object Target => this.tables;

        public override DomainContext DomainContext { get; }

        public override string ItemPath { get; }

        public override CremaHost CremaHost { get; }

        public override DataBase DataBase { get; }

        public override IPermission Permission { get; }

        public override IDispatcherObject DispatcherObject { get; }

        protected override async Task OnBeginEditAsync(Authentication authentication)
        {
            await base.OnBeginEditAsync(authentication);
        }

        protected override async Task OnEndEditAsync(Authentication authentication, CremaTemplate template)
        {
            await base.OnEndEditAsync(authentication, template);
            if (this.parent is TableCategory category)
            {
                var tables = category.GetService(typeof(TableCollection)) as TableCollection;
                this.tables = await tables.AddNewAsync(authentication, template.TargetTable.DataSet.Copy());
            }
            else if (this.parent is Table table)
            {
                var tables = table.GetService(typeof(TableCollection)) as TableCollection;
                this.tables = await tables.AddNewAsync(authentication, template.TargetTable.DataSet.Copy());
            }
            this.parent = null;
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
            this.parent = null;
        }

        protected override async Task<CremaTemplate> CreateSourceAsync(Authentication authentication)
        {
            if (this.parent is TableCategory category)
            {
                var typeContext = category.GetService(typeof(TypeContext)) as TypeContext;
                var dataSet = await typeContext.Root.ReadDataAsync(authentication, true);
                var newName = NameUtility.GenerateNewName(nameof(Target), category.Context.Tables.Select((Table item) => item.Name));
                var templateSource = CremaTemplate.Create(dataSet, newName, category.Path);
                return templateSource;
            }
            else if (this.parent is Table table)
            {
                var dataSet = await table.ReadAllAsync(authentication, true);
                var dataTable = dataSet.Tables[table.Name, table.Category.Path];
                return CremaTemplate.Create(dataTable);
            }
            throw new NotImplementedException();
        }
    }
}

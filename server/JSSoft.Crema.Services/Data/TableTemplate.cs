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
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class TableTemplate : TableTemplateBase
    {
        private readonly Table table;
        private readonly Table[] tables;

        public TableTemplate(Table table)
        {
            this.table = table;
            this.tables = new Table[] { table };
        }

        public override AccessType GetAccessType(Authentication authentication)
        {
            return this.table.GetAccessType(authentication);
        }

        public override object Target => this.table;

        public override DomainContext DomainContext => this.table.GetService(typeof(DomainContext)) as DomainContext;

        public override string ItemPath => this.table.Path;

        public override CremaHost CremaHost => this.table.CremaHost;

        public override DataBase DataBase => this.table.DataBase;

        public override IDispatcherObject DispatcherObject => this.table;

        public override IPermission Permission => this.table;

        protected override async Task OnBeginEditAsync(Authentication authentication)
        {
            await base.OnBeginEditAsync(authentication);
            this.table.TableState = TableState.IsBeingSetup;
            this.Container.InvokeTablesStateChangedEvent(authentication, this.tables);
        }

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            var template = this.TemplateSource;
            var dataSet = template.DataTable.DataSet;
            var dataBaseSet = await DataBaseSet.CreateAsync(this.table.DataBase, dataSet);
            var tableInfo = template.TableInfo;
            var taskID = this.Domain.ID;
            this.Domain.Result = new TableInfo[] { tableInfo };
            await this.Container.InvokeTableEndTemplateEditAsync(authentication, tableInfo, dataBaseSet);
            await base.OnEndEditAsync(authentication);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.table.UpdateTemplate(template.TableInfo);
                this.table.UpdateTags(template.Tags);
                this.table.UpdateComment(template.Comment);
                this.table.TableState = TableState.None;
                this.Container.InvokeTablesStateChangedEvent(authentication, this.tables);
                this.Container.InvokeTablesTemplateChangedEvent(authentication, this.tables, dataSet);
                this.DataBase.InvokeTaskCompletedEvent(authentication, taskID);
            });
            await this.Repository.UnlockAsync(authentication, this, nameof(OnEndEditAsync), this.ItemPaths);
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.table.TableState = TableState.None;
                this.Container.InvokeTablesStateChangedEvent(authentication, this.tables);
            });
            await this.Repository.UnlockAsync(authentication, this, nameof(OnCancelEditAsync), this.ItemPaths);
        }

        protected override void OnAttach(Domain domain)
        {
            this.table.TableState = TableState.IsBeingSetup;
            base.OnAttach(domain);
        }

        protected override async Task<CremaTemplate> CreateSourceAsync(Authentication authentication)
        {
            var tablePath = this.table.Path;
            var dataSet = await this.table.ReadDataForTemplateAsync(authentication);
            var dataTable = dataSet.Tables[this.table.Name, this.table.Category.Path];
            if (dataTable == null)
                throw new TableNotFoundException(tablePath);
            return new CremaTemplate(dataTable);
        }

        private TableCollection Container => this.table.Container;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateBeginEdit(Authentication authentication, object target)
        {
            base.OnValidateBeginEdit(authentication, target);

            if (target == this && this.table.TemplatedParent != null)
                throw new InvalidOperationException(Resources.Exception_InheritedTableCannotEditTemplate);

            this.table.ValidateAccessType(authentication, AccessType.Master);
            this.table.ValidateIsNotBeingEdited();
            this.table.ValidateHasNotBeingEditedType();

            var templates = this.table.Childs.Select(item => item.Template);
            foreach (var item in templates)
            {
                item.OnValidateBeginEdit(authentication, target);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateEndEdit(Authentication authentication, object target)
        {
            base.OnValidateEndEdit(authentication, target);

            if (target == this)
            {
                if (this.table.TemplatedParent != null)
                    throw new InvalidOperationException(Resources.Exception_InheritedTableTemplateCannotEdit);
                this.TemplateSource.Validate();
            }

            this.table.ValidateAccessType(authentication, AccessType.Master);

            var templates = this.table.GetRelations().Select(item => item.Template).ToArray();
            foreach (var item in templates)
            {
                if (target != this)
                    item.OnValidateEndEdit(authentication, target);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void OnValidateCancelEdit(Authentication authentication, object target)
        {
            base.OnValidateCancelEdit(authentication, target);
            this.table.ValidateAccessType(authentication, AccessType.Master);

            var templates = this.table.GetRelations().Select(item => item.Template).ToArray();
            foreach (var item in templates)
            {
                if (target != this)
                    item.OnValidateCancelEdit(authentication, target);
            }
        }
    }
}

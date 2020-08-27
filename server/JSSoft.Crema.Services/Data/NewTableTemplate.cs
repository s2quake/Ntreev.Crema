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
using Ntreev.Library.Linq;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class NewTableTemplate : TableTemplateBase
    {
        private const string ChildString = "Child";
        private object parent;
        private Table[] tables;
        private IPermission permission;

        public NewTableTemplate(TableCategory category)
        {
            this.parent = category ?? throw new ArgumentNullException(nameof(category));
            this.permission = category;
            this.DispatcherObject = category;
            this.DomainContext = category.GetService(typeof(DomainContext)) as DomainContext;
            this.ItemPath = category.Path;
            this.CremaHost = category.CremaHost;
            this.DataBase = category.DataBase;
            this.Permission = category;
            this.IsNew = true;
            this.Container = category.GetService(typeof(TableCollection)) as TableCollection;
            category.Attach(this);
        }

        public NewTableTemplate(Table parent)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.permission = parent;
            this.DispatcherObject = parent;
            this.DomainContext = parent.GetService(typeof(DomainContext)) as DomainContext;
            this.ItemPath = parent.Path;
            this.CremaHost = parent.CremaHost;
            this.DataBase = parent.DataBase;
            this.Permission = parent;
            this.IsNew = true;
            this.Container = parent.GetService(typeof(TableCollection)) as TableCollection;
            parent.Attach(this);
        }

        public override AccessType GetAccessType(Authentication authentication)
        {
            return this.permission.GetAccessType(authentication);
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

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            var dataSet = this.TemplateSource.DataSet;
            var dataTable = this.TemplateSource.DataTable;
            var taskID = this.Domain.ID;
            var dataTables = EnumerableUtility.Friends(dataTable, dataTable.DerivedTables).ToArray();
            var itemPaths = dataTables.Select(item => item.FullPath).ToArray();
            var dataBaseSet = await DataBaseSet.CreateAsync(this.DataBase, dataSet, DataBaseSetOptions.OmitUnlock | DataBaseSetOptions.AllowTableCreation);

            await this.Repository.LockAsync(authentication, this, nameof(OnEndEditAsync), itemPaths);
            try
            {
                dataBaseSet.TablesToCreate = dataTables;
                this.tables = await this.Container.AddNewAsync(authentication, dataBaseSet);
                this.Domain.Result = dataTables.Select(item => item.TableInfo).ToArray();
                await base.OnEndEditAsync(authentication);
                await this.Dispatcher.InvokeAsync(() => this.DataBase.InvokeTaskCompletedEvent(authentication, taskID));
                this.parent = null;
                this.permission = null;
            }
            finally
            {
                await this.Repository.UnlockAsync(authentication, this, nameof(OnEndEditAsync), itemPaths);
            }
            await this.Repository.UnlockAsync(authentication, this, nameof(OnCancelEditAsync), this.ItemPaths);
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await this.Repository.UnlockAsync(authentication, this, nameof(OnCancelEditAsync), this.ItemPaths);
            await base.OnCancelEditAsync(authentication);
            this.parent = null;
            this.permission = null;
        }

        protected override async Task<CremaTemplate> CreateSourceAsync(Authentication authentication)
        {
            if (this.parent is TableCategory category)
            {
                var typeContext = category.GetService(typeof(TypeContext)) as TypeContext;
                var tableContext = category.GetService(typeof(TableContext)) as TableContext;
                var dataSet = await category.ReadDataForNewTemplateAsync(authentication);
                var tableNames = await tableContext.Dispatcher.InvokeAsync(() => tableContext.Tables.Select((Table item) => item.Name).ToArray());
                var newName = NameUtility.GenerateNewName(nameof(Table), tableNames);
                var templateSource = CremaTemplate.Create(dataSet, newName, category.Path);
                return templateSource;
            }
            else if (this.parent is Table table)
            {
                var dataSet = await table.ReadDataForNewTemplateAsync(authentication);
                var dataTable = dataSet.Tables[table.Name, table.Category.Path];
                var childNames = await table.Dispatcher.InvokeAsync(() => table.Childs.Select(item => item.TableName).Concat(new string[] { table.TableName }).ToArray());
                var newName = NameUtility.GenerateNewName(ChildString, childNames);
                var template = CremaTemplate.Create(dataTable);
                template.TableName = newName;
                return template;
            }
            throw new NotImplementedException();
        }

        private TableCollection Container { get; }

        public TableContext Context => this.Container.Context;
    }
}
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
using Ntreev.Crema.Services.DataBaseService;
using Ntreev.Crema.Services.Domains;
using System;
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
            this.Service = category.Service;
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
            this.Service = parent.Service;
        }

        public override AccessType GetAccessType(Authentication authentication)
        {
            return this.Permission.GetAccessType(authentication);
        }

        public override object Target => this.tables;

        public override DomainContext DomainContext { get; }

        public override string ItemPath { get; }

        public override CremaHost CremaHost { get; }

        public override DataBase DataBase { get; }

        public override IPermission Permission { get; }

        public override IDispatcherObject DispatcherObject { get; }
		
		public IDataBaseService Service { get; }

        protected override async Task OnBeginEditAsync(Authentication authentication)
        {
            await base.OnBeginEditAsync(authentication);
        }

        protected override async Task<TableInfo[]> OnEndEditAsync(Authentication authentication, object args)
        {
            var tableInfos = await base.OnEndEditAsync(authentication, args);
            if (this.parent is TableCategory category)
            {
                var tables = category.GetService(typeof(TableCollection)) as TableCollection;
                this.tables = tables.AddNew(authentication, tableInfos);
            }
            else if (this.parent is Table table)
            {
                var tables = table.GetService(typeof(TableCollection)) as TableCollection;
                this.tables = tables.AddNew(authentication, tableInfos);
            }
            this.parent = null;
            return tableInfos;
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
            this.parent = null;
        }

        protected override Task<ResultBase<DomainMetaData>> BeginDomainAsync(Authentication authentication)
        {
            return Task.Run(() => this.Service.BeginNewTable(this.ItemPath));
        }

        protected override async Task<TableInfo[]> EndDomainAsync(Authentication authentication, object args)
        {
            if (args is Guid domainID)
            {
                var result = await Task.Run(() => this.Service.EndTableTemplateEdit(domainID));
                var value = result.GetValue();
                return value;
            }
            return args as TableInfo[];
        }

        protected override Task<ResultBase> CancelDomainAsync(Authentication authentication, Guid domainID)
        {
            return Task.Run(() => this.Service.CancelTableTemplateEdit(domainID));
        }
    }
}

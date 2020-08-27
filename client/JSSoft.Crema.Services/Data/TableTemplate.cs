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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceHosts.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class TableTemplate : TableTemplateBase
    {
        private readonly Table table;

        public TableTemplate(Table table)
        {
            this.table = table;
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
            this.Container.InvokeTablesStateChangedEvent(authentication, new Table[] { this.table, });
        }

        protected override async Task OnEndEditAsync(Authentication authentication)
        {
            await base.OnEndEditAsync(authentication);
        }

        protected override async Task OnCancelEditAsync(Authentication authentication)
        {
            await base.OnCancelEditAsync(authentication);
        }

        protected override void OnAttach(Domain domain)
        {
            this.table.TableState = TableState.IsBeingSetup;
            base.OnAttach(domain);
        }

        protected override Task<ResultBase<DomainMetaData>> OnBeginDomainAsync(Authentication authentication)
        {
            return this.Service.BeginTableTemplateEditAsync(this.table.Name);
        }

        protected override async Task<ResultBase<TableInfo[]>> OnEndDomainAsync(Authentication authentication)
        {
            return await this.Service.EndTableTemplateEditAsync(this.Domain.ID);
        }

        protected override async Task<ResultBase> OnCancelDomainAsync(Authentication authentication)
        {
            return await this.Service.CancelTableTemplateEditAsync(this.Domain.ID);
        }

        private TableCollection Container => this.table.Container;

        private IDataBaseService Service => this.table.Service;
    }
}

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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    // [Export(typeof(IConsoleCommandProvider))]
    [PartialCommand]
    class DomainDeleteCommand : ConsoleCommandMethodBase
    {
        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        public DomainDeleteCommand(ICremaHost cremaHost)
            : base("domain")
        {
            this.cremaHost = cremaHost;
        }

        [CommandMethod]
        [CommandMethodProperty(nameof(DomainID), nameof(IsForce))]
        public Task DeleteAsync()
        {
            if (this.DataBaseName == null)
            {
                return this.DeleteAsync(this.DomainID);
            }
            else
            {
                return this.DeleteDomainsAsync(this.DataBaseName);
            }
        }

        [CommandPropertyRequired(DefaultValue = "")]
        //[CommandPropertyTrigger(nameof(DataBaseName), null)]
        [CommandCompletion(nameof(GetDomainIDs))]
        public string DomainID
        {
            get; set;
        }

        [CommandProperty("cancel", 'c')]
        public bool IsCancelled
        {
            get; set;
        }

        [CommandProperty("force", 'f')]
        public bool IsForce
        {
            get; set;
        }

        [CommandPropertyRequired("database", IsExplicit = true)]
        [CommandPropertyTrigger(nameof(DomainID), "")]
        [CommandCompletion(nameof(GetDataBaseNames))]
        public string DataBaseName
        {
            get; set;
        }

        private async Task DeleteAsync(string domainID)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var domain = await this.GetDomainAsync(Guid.Parse(domainID));
            var dataBase = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext.FirstOrDefault(item => item.ID == domain.DataBaseID));
            var isLoaded = dataBase.Dispatcher.Invoke(() => dataBase.IsLoaded);

            if (isLoaded == false && this.IsForce == false)
                throw new ArgumentException($"'{dataBase}' database is not loaded.");

            await domain.DeleteAsync(authentication, this.IsCancelled);
        }

        private async Task DeleteDomainsAsync(string dataBaseName)
        {
            var dataBase = await this.DataBaseContext.Dispatcher.InvokeAsync(() => this.DataBaseContext[dataBaseName]);
            if (dataBase == null)
                throw new DataBaseNotFoundException(dataBaseName);

            var isLoaded = await dataBase.Dispatcher.InvokeAsync(() => dataBase.IsLoaded);
            if (isLoaded == false && this.IsForce == false)
                throw new ArgumentException($"'{dataBase}' database is not loaded.");

            var domains = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains.Where(item => item.DataBaseID == dataBase.ID).ToArray());
            var authentication = this.CommandContext.GetAuthentication(this);

            foreach (var item in domains)
            {
                await item.DeleteAsync(authentication, this.IsCancelled);
            }
        }

        public IDomainContext DomainContext => this.cremaHost.GetService(typeof(IDomainContext)) as IDomainContext;

        private string[] GetDataBaseNames()
        {
            return this.DataBaseContext.Dispatcher.Invoke(() =>
            {
                var query = from item in this.DataBaseContext
                            select item.Name;
                return query.ToArray();
            });
        }

        private string[] GetDomainIDs()
        {
            return this.DomainContext.Dispatcher.Invoke(() =>
            {
                return this.DomainContext.Domains.Select(item => $"{item.ID}").ToArray();
            });
        }

        private async Task<IDomain> GetDomainAsync(Guid domainID)
        {
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            if (domain == null)
                throw new DomainNotFoundException(domainID);
            return domain;
        }

        private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    }
}
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

using JSSoft.Crema.Commands.Consoles.Properties;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class DomainCommand : ConsoleCommandMethodBase, IConsoleCommand
    {
        [ImportingConstructor]
        public DomainCommand(ICremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
        }

        //[CommandMethod]
        //[CommandMethodProperty(nameof(IsCancelled), nameof(IsForce))]
        //public void Delete([CommandCompletion(nameof(GetDomainIDs))]string domainID = null)
        //{
        //    var domain = this.GetDomain(Guid.Parse(domainID));
        //    var dataBase = this.cremaHost.Dispatcher.Invoke(() => this.DataBases.FirstOrDefault(item => item.ID == domain.DataBaseID));
        //    var isLoaded = dataBase.Dispatcher.Invoke(() => dataBase.IsLoaded);

        //    if (isLoaded == false && this.IsForce == false)
        //        throw new ArgumentException($"'{dataBase}' database is not loaded.");

        //    var authentication = this.CommandContext.GetAuthentication(this);
        //    domain.Dispatcher.Invoke(() => domain.Delete(authentication, this.IsCancelled));
        //}

        //[CommandMethod]
        //[CommandMethodProperty(nameof(IsCancelled), nameof(IsForce))]
        //public void DeleteAll([CommandCompletion(nameof(GetDataBaseNames))]string dataBaseName)
        //{
        //    var dataBase = this.cremaHost.Dispatcher.Invoke(() => this.DataBases[dataBaseName]);
        //    if (dataBase == null)
        //        throw new DataBaseNotFoundException(dataBaseName);

        //    var isLoaded = dataBase.Dispatcher.Invoke(() => dataBase.IsLoaded);
        //    if (isLoaded == false && this.IsForce == false)
        //        throw new ArgumentException($"'{dataBase}' database is not loaded.");

        //    var domains = this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.Domains.Where(item => item.DataBaseID == dataBase.ID).ToArray());
        //    var authentication = this.CommandContext.GetAuthentication(this);

        //    foreach (var item in domains)
        //    {
        //        item.Dispatcher.Invoke(() => item.Delete(authentication, this.IsCancelled));
        //    }
        //}

        [CommandMethod("list")]
        [CommandMethodProperty(nameof(DataBaseName))]
        public void List()
        {
            var domainInfos = this.DomainContext.Dispatcher.Invoke(() =>
            {
                var domainInfoList = new List<DomainInfo>();
                foreach (var item in this.DomainContext.Domains)
                {
                    var domainInfo = item.Dispatcher.Invoke(() => item.DomainInfo);
                    domainInfoList.Add(domainInfo);
                }
                return domainInfoList.ToArray();
            });

            var dataBaseInfos = this.CremaHost.Dispatcher.Invoke(() =>
            {
                return this.DataBaseContext.Select(item => item.DataBaseInfo).ToArray();
            });

            var query = from domainInfo in domainInfos
                        join dataBaseInfo in dataBaseInfos on domainInfo.DataBaseID equals dataBaseInfo.ID
                        where this.DataBaseName == string.Empty || (dataBaseInfo.Name == this.DataBaseName)
                        group $"{domainInfo.DomainID}" by dataBaseInfo.Name into g
                        select g;

            if (query.Any())
            {
                foreach (var item in query)
                {
                    this.CommandContext.Out.WriteLine($"{item.Key}:");
                    foreach (var i in item)
                    {
                        this.CommandContext.Out.WriteLine(i);
                    }
                    this.CommandContext.Out.WriteLine();
                }
            }
            else
            {
                this.CommandContext.Out.WriteLine("no domains");
            }
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public void Info([CommandCompletion(nameof(GetDomainIDs))] Guid domainID)
        {
            var domain = this.GetDomain(domainID);
            var authentication = this.CommandContext.GetAuthentication(this);
            var domainInfo = domain.Dispatcher.Invoke(() => domain.DomainInfo);
            this.CommandContext.WriteObject(domainInfo.ToDictionary(), FormatProperties.Format);
        }

        //[CommandProperty("cancel", 'c')]
        //public bool IsCancelled
        //{
        //    get; set;
        //}

        //[CommandProperty("force", 'f')]
        //public bool IsForce
        //{
        //    get; set;
        //}

        [CommandProperty("database")]
        [DefaultValue("")]
        [CommandCompletion(nameof(GetDataBaseNames))]
        public string DataBaseName
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.Drive is DomainsConsoleDrive;

        public IDomainContext DomainContext => this.CremaHost.GetService(typeof(IDomainContext)) as IDomainContext;

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
                var query = from item in this.DomainContext.Domains
                            let domainID = item.ID.ToString()
                            select domainID;
                return query.ToArray();
            });
        }

        private IDomain GetDomain(Guid domainID)
        {
            var domain = this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.Domains[domainID]);
            if (domain == null)
                throw new DomainNotFoundException(domainID);
            return domain;
        }

        private ICremaHost CremaHost { get; }

        private IDataBaseContext DataBaseContext => this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    }
}

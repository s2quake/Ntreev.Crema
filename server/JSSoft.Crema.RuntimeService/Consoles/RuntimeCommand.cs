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

using JSSoft.Crema.Commands.Consoles;
using JSSoft.Crema.Commands.Consoles.Properties;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.RuntimeService.Consoles
{
    [Export(typeof(IConsoleCommand))]
    class RuntimeCommand : ConsoleCommandMethodBase
    {
        private readonly ICremaHost cremaHost;
        private readonly RuntimeService runtimeService;

        [ImportingConstructor]
        public RuntimeCommand(ICremaHost cremaHost, RuntimeService runtimeService)
            : base("rt")
        {
            this.cremaHost = cremaHost;
            this.runtimeService = runtimeService;
        }

        [CommandMethod]
        public async Task ResetAsync(string dataBaseName)
        {
            var dataBaseID = await this.DataBases.Dispatcher.InvokeAsync(() =>
            {
                var dataBase = this.DataBases[dataBaseName];
                if (dataBase == null)
                    throw new Exception();
                return dataBase.ID;
            });

            var serviceItem = this.runtimeService.GetServiceItem(dataBaseID);
            await serviceItem.ResetAsync();
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task InfoAsync(string dataBaseName)
        {
            var sb = new StringBuilder();
            var dataBaseID = this.DataBases.Dispatcher.Invoke(() =>
            {
                var dataBase = this.DataBases[dataBaseName];
                if (dataBase == null)
                    throw new Exception();
                return dataBase.ID;
            });

            var serviceItem = this.runtimeService.GetServiceItem(dataBaseID);
            var info = serviceItem.Dispatcher.Invoke(() => serviceItem.DataServiceItemInfo);
            var props = info.ToDictionary();
            var format = FormatProperties.Format;
            sb.AppendLine(props, format);
            await this.Out.WriteAsync(sb.ToString());
        }

        public override bool IsEnabled => this.cremaHost.ServiceState == ServiceState.Open;

        private IDataBaseContext DataBases => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
    }
}

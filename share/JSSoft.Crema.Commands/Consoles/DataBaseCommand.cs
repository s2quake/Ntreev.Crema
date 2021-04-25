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
using JSSoft.Library;
using JSSoft.Library.Commands;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class DataBaseCommand : ConsoleCommandMethodBase, IConsoleCommand
    {
        private const string dataBaseNameParameter = "dataBaseName";

        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        public DataBaseCommand(ICremaHost cremaHost)
            : base("database")
        {
            this.cremaHost = cremaHost;
        }

        public override string[] GetCompletions(CommandMethodDescriptor methodDescriptor, CommandMemberDescriptor memberDescriptor, string find)
        {
            return base.GetCompletions(methodDescriptor, memberDescriptor, find);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(MessageProperties))]
        public Task CreateAsync(string dataBaseName)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            return this.DataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, MessageProperties.Message);
        }

        [CommandMethod]
        public Task RenameAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName, string newDataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            return dataBase.RenameAsync(authentication, newDataBaseName);
        }

        [CommandMethod]
        public async Task DeleteAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            if (this.CommandContext.ConfirmToDelete() == true)
            {
                await dataBase.DeleteAsync(authentication);
            }
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(MessageProperties))]
        [CommandMethodProperty(nameof(Force))]
        public async Task CopyAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName, string newDataBaseName)
        {
            var dataBase = GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.CopyAsync(authentication, newDataBaseName, MessageProperties.Message, this.Force);
        }

        [CommandMethod]
        public async Task LoadAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.LoadAsync(authentication);
        }

        [CommandMethod]
        public async Task UnloadAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.UnloadAsync(authentication);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(MessageProperties))]
        public async Task LockAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.LockAsync(authentication, MessageProperties.Message);
        }

        [CommandMethod]
        public async Task UnlockAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.UnlockAsync(authentication);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        public async Task ListAsync()
        {
            var items = await this.DataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.DataBaseContext
                            where StringUtility.GlobMany(item.Name, FilterProperties.FilterExpression)
                            select new { item.IsLoaded, item.Name };
                return query.ToArray();
            });

            var tb = new TerminalStringBuilder();
            foreach (var item in items)
            {
                if (item.IsLoaded == false)
                {
                    tb.Foreground = TerminalColor.BrightBlack;
                    tb.AppendLine(item.Name);
                    tb.Foreground = null;
                }
                else
                {
                    tb.AppendLine(item.Name);
                }
            }
            tb.AppendEnd();
            this.Out.WriteLine(tb.ToString());
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task InfoAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName)
        {
            var sb = new StringBuilder();
            var dataBase = this.GetDataBase(dataBaseName);
            var dataBaseInfo = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo);
            var props = dataBaseInfo.ToDictionary();
            var format = FormatProperties.Format;
            sb.AppendLine(props, format);
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandMethod]
        public async Task RevertAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName, string revision)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.RevertAsync(authentication, revision);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(LogProperties))]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task LogAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName, string revision = null)
        {
            var sb = new StringBuilder();
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var logs = await dataBase.GetLogAsync(authentication, revision);
            var format = FormatProperties.Format;
            foreach (var item in logs)
            {
                var props = item.ToDictionary();
                sb.AppendLine(props, format);
            }
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        [CommandMethodStaticProperty(typeof(DataSetTypeProperties))]
        public async Task ViewAsync([CommandCompletion(nameof(GetDataBaseNamesAsync))] string dataBaseName, string revision = null)
        {
            var sb = new StringBuilder();
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var dataSet = await dataBase.GetDataSetAsync(authentication, DataSetTypeProperties.DataSetType, FilterProperties.FilterExpression, revision);
            var props = dataSet.ToDictionary(DataSetTypeProperties.TableOnly == true, DataSetTypeProperties.TypeOnly == true);
            var format = FormatProperties.Format;
            sb.AppendLine(props, format);
            await this.Out.WriteAsync(sb.ToString());
        }

        [CommandProperty('f', AllowName = true)]
        public bool Force
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.Drive is DataBasesConsoleDrive && this.CommandContext.IsOnline == true;

        private Task<string[]> GetDataBaseNamesAsync()
        {
            return this.DataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in this.DataBaseContext
                            select item.Name;
                return query.ToArray();
            });
        }

        private IDataBase GetDataBase(string dataBaseName)
        {
            var dataBase = this.DataBaseContext.Dispatcher.Invoke(GetDataBase);
            if (dataBase == null)
                throw new DataBaseNotFoundException(dataBaseName);
            return dataBase;

            IDataBase GetDataBase()
            {
                return this.DataBaseContext[dataBaseName];
            }
        }

        private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;

        // #region classes

        // class ItemObject : TerminalTextItem
        // {
        //     private readonly string name;

        //     public ItemObject(string name, bool isLoaded)
        //         : base(name)
        //     {
        //         this.name = name;
        //         this.IsLoaded = isLoaded;
        //     }

        //     public bool IsLoaded { get; }

        //     protected override void OnDraw(TextWriter writer, string text)
        //     {
        //         if (this.IsLoaded == false)
        //         {
        //             using (TerminalColor.SetForeground(ConsoleColor.DarkGray))
        //             {
        //                 base.OnDraw(writer, text);
        //             }
        //         }
        //         else
        //         {
        //             base.OnDraw(writer, text);
        //         }
        //     }

        //     public override string ToString()
        //     {
        //         return this.name;
        //     }
        // }

        // #endregion
    }
}

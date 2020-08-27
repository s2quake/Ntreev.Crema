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
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceDescription("Resources", IsShared = true)]
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
        public Task RenameAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName, string newDataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            return dataBase.RenameAsync(authentication, newDataBaseName);
        }

        [CommandMethod]
        public async Task DeleteAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName)
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
        public async Task CopyAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName, string newDataBaseName)
        {
            var dataBase = GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.CopyAsync(authentication, newDataBaseName, MessageProperties.Message, this.Force);
        }

        [CommandMethod]
        public async Task LoadAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.LoadAsync(authentication);
        }

        [CommandMethod]
        public async Task UnloadAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.UnloadAsync(authentication);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(MessageProperties))]
        public async Task LockAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.LockAsync(authentication, MessageProperties.Message);
        }

        [CommandMethod]
        public async Task UnlockAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.UnlockAsync(authentication);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        public void List()
        {
            throw new NotImplementedException("dotnet");
            // var items = this.DataBaseContext.Dispatcher.Invoke(() =>
            // {
            //     var query = from item in this.DataBaseContext
            //                 where StringUtility.GlobMany(item.Name, FilterProperties.FilterExpression)
            //                 select new ItemObject(item.Name, item.IsLoaded);
            //     return query.ToArray();
            // });

            // this.CommandContext.WriteList(items);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public void Info([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var dataBaseInfo = dataBase.Dispatcher.Invoke(() => dataBase.DataBaseInfo);
            var props = dataBaseInfo.ToDictionary();
            this.CommandContext.WriteObject(props, FormatProperties.Format);
        }

        [CommandMethod]
        public async Task RevertAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName, string revision)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            await dataBase.RevertAsync(authentication, revision);
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(LogProperties))]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        public async Task LogAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName, string revision = null)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var logs = await dataBase.GetLogAsync(authentication, revision);

            foreach (var item in logs)
            {
                this.CommandContext.WriteObject(item.ToDictionary(), FormatProperties.Format);
                this.Out.WriteLine();
            }
        }

        [CommandMethod]
        [CommandMethodStaticProperty(typeof(FilterProperties))]
        [CommandMethodStaticProperty(typeof(FormatProperties))]
        [CommandMethodStaticProperty(typeof(DataSetTypeProperties))]
        public async Task ViewAsync([CommandCompletion(nameof(GetDataBaseNames))] string dataBaseName, string revision = null)
        {
            var dataBase = this.GetDataBase(dataBaseName);
            var authentication = this.CommandContext.GetAuthentication(this);
            var dataSet = await dataBase.GetDataSetAsync(authentication, DataSetTypeProperties.DataSetType, FilterProperties.FilterExpression, revision);
            var props = dataSet.ToDictionary(DataSetTypeProperties.TableOnly == true, DataSetTypeProperties.TypeOnly == true);
            this.CommandContext.WriteObject(props, FormatProperties.Format);
        }

        [CommandProperty('f', AllowName = true)]
        public bool Force
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.Drive is DataBasesConsoleDrive && this.CommandContext.IsOnline == true;

        private string[] GetDataBaseNames()
        {
            return this.DataBaseContext.Dispatcher.Invoke(() =>
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

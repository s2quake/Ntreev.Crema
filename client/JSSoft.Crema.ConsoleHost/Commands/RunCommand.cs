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

using JSSoft.Crema.ConsoleHost.Commands.Consoles;
using JSSoft.Crema.Javascript;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost.Commands
{
    [Export(typeof(ICommand))]
    [Export]
    [ResourceUsageDescription]
    class RunCommand : CommandAsyncBase
    {
        private readonly CremaApplication application;
        // private readonly ICremaHost cremaHost;
        private readonly ScriptContext scriptContext;

        [ImportingConstructor]
        public RunCommand(CremaApplication application, ScriptContext scriptContext)
        {
            this.application = application;
            // this.cremaHost = cremaHost;
            this.scriptContext = scriptContext;
        }

        [CommandPropertySwitch("prompt", 'p')]
        [CommandPropertyTrigger(nameof(ScriptPath), "")]
        public bool IsPromptMode
        {
            get;
            set;
        }

        [CommandProperty(InitValue = "")]
        [CommandPropertyTrigger(nameof(IsPromptMode), false)]
        public string ScriptPath
        {
            get;
            set;
        }

        [CommandProperty("list", 'l')]
        [CommandPropertyTrigger(nameof(ScriptPath), "")]
        [DefaultValue(false)]
        public bool List
        {
            get; set;
        }

        [CommandProperty("entry")]
        [CommandPropertyTrigger(nameof(List), false)]
        [DefaultValue("")]
        public string ScriptEntry
        {
            get;
            set;
        }

        [CommandPropertyArray]
        public string[] Arguments
        {
            get;
            set;
        }

#if DEBUG
        [CommandProperty(InitValue = "en-US")]
#else
        [CommandProperty(InitValue = "")]
#endif
        public string Culture
        {
            get; set;
        }

        [CommandProperty]
        public string Address
        {
            get;
            set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            this.application.Culture = this.Culture;
            this.application.Verbose = LogVerbose.None;
            // this.application.Address = this.Address

            // this.CommandContext.SetAddress(this.Address);
            // this.CommandContext.SetAddress(this.Address);
            await Task.Delay(100);
            if (this.IsPromptMode == true)
            {
                await this.Terminal.StartAsync();
            }

            // if (this.List == true)
            // {
            //     this.Out.Write(this.scriptContext.GenerateDeclaration(this.GetArgumentTypes()));
            // }
            // else
            // {
            //     if (File.Exists(this.ScriptPath) == false)
            //         throw new FileNotFoundException(this.ScriptPath);
            //     this.scriptContext.RunFromFile(this.ScriptPath, this.ScriptEntry, this.GetProperties(), null);
            // }
        }

        // private IDictionary<string, object> GetProperties()
        // {
        //     return CommandStringUtility.ArgumentsToDictionary(this.Arguments);
        // }

        // private Dictionary<string, Type> GetArgumentTypes()
        // {
        //     var properties = new Dictionary<string, Type>(this.Arguments.Length);
        //     foreach (var item in this.Arguments)
        //     {
        //         if (CommandStringUtility.TryGetKeyValue(item, out var key, out var value) == true)
        //         {
        //             var typeName = value;
        //             if (CommandStringUtility.IsWrappedOfQuote(value))
        //             {
        //                 value = CommandStringUtility.TrimQuot(value);
        //             }

        //             if (value == "number")
        //             {
        //                 properties.Add(key, typeof(decimal));
        //             }
        //             else if (value == "boolean")
        //             {
        //                 properties.Add(key, typeof(bool));
        //             }
        //             else if (value == "string")
        //             {
        //                 properties.Add(key, typeof(string));
        //             }
        //             else
        //             {
        //                 throw new ArgumentException(typeName);
        //             }
        //         }
        //         else
        //         {
        //             throw new ArgumentException(item);
        //         }
        //     }
        //     return properties;
        // }

        private ConsoleTerminal Terminal => this.application.GetService(typeof(ConsoleTerminal)) as ConsoleTerminal;
    }
}
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

using JSSoft.Crema.Javascript;
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

namespace JSSoft.Crema.ConsoleHost.Commands
{
    [Export(typeof(ICommand))]
    [Export]
    [ResourceDescription]
    class RunCommand : CommandBase
    {
        private readonly CremaBootstrapper application;
        private readonly ICremaHost cremaHost;
        private readonly ScriptContext scriptContext;

        [ImportingConstructor]
        public RunCommand(CremaBootstrapper application, ICremaHost cremaHost, ScriptContext scriptContext)
            : base("run")
        {
            this.application = application;
            this.cremaHost = cremaHost;
            this.scriptContext = scriptContext;
        }

        [CommandPropertyRequired]
        [CommandPropertyTrigger(nameof(List), false)]
        [DefaultValue("")]
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

        [CommandProperty]
#if DEBUG
        [DefaultValue("en-US")]
#else
        [DefaultValue("")]
#endif
        public string Culture
        {
            get; set;
        }

        protected override void OnExecute()
        {
            this.application.Culture = this.Culture;
            this.application.Verbose = LogVerbose.None;

            if (this.List == true)
            {
                this.Out.Write(this.scriptContext.GenerateDeclaration(this.GetArgumentTypes()));
            }
            else
            {
                if (File.Exists(this.ScriptPath) == false)
                    throw new FileNotFoundException(this.ScriptPath);
                this.scriptContext.RunFromFile(this.ScriptPath, this.ScriptEntry, this.GetProperties(), null);
            }
        }

        private IDictionary<string, object> GetProperties()
        {
            return CommandStringUtility.ArgumentsToDictionary(this.Arguments);
        }

        private Dictionary<string, Type> GetArgumentTypes()
        {
            var properties = new Dictionary<string, Type>(this.Arguments.Length);
            foreach (var item in this.Arguments)
            {
                if (CommandStringUtility.TryGetKeyValue(item, out var key, out var value) == true)
                {
                    var typeName = value;
                    if (CommandStringUtility.IsWrappedOfQuote(value))
                    {
                        value = CommandStringUtility.TrimQuot(value);
                    }

                    if (value == "number")
                    {
                        properties.Add(key, typeof(decimal));
                    }
                    else if (value == "boolean")
                    {
                        properties.Add(key, typeof(bool));
                    }
                    else if (value == "string")
                    {
                        properties.Add(key, typeof(string));
                    }
                    else
                    {
                        throw new ArgumentException(typeName);
                    }
                }
                else
                {
                    throw new ArgumentException(item);
                }
            }
            return properties;
        }
    }
}
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

using Ntreev.Crema.Commands.Consoles.Properties;
using Ntreev.Crema.Services;
using Ntreev.Library.Commands;
using System;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceDescription("Resources", IsShared = true)]
    [CommandStaticProperty(typeof(FormatProperties))]
    class InformationCommand : ConsoleCommandBase
    {
        [ImportingConstructor]
        public InformationCommand()
            : base("info")
        {
        }

        public override string[] GetCompletions(CommandCompletionContext completionContext)
        {
            if (completionContext.MemberDescriptor == null || completionContext.MemberDescriptor.DescriptorName == nameof(Path) == true)
                return this.CommandContext.GetCompletion(completionContext.Find, true);
            return base.GetCompletions(completionContext);
        }

        [CommandPropertyRequired(DefaultValue = "")]
        public string Path
        {
            get; set;
        }

        public string AbsolutePath
        {
            get
            {
                if (this.Path == string.Empty)
                    return this.CommandContext.Path;
                return this.CommandContext.GetAbsolutePath(this.Path);
            }
        }

        public override bool IsEnabled => this.CommandContext.IsOnline;

        protected override void OnExecute()
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var drive = this.CommandContext.Drive;
            var provider = this.GetObject(authentication, this.AbsolutePath);
            var info = this.Invoke(provider, () => provider.Info);
            this.CommandContext.WriteObject(info, FormatProperties.Format);
        }

        private IInfoProvider GetObject(Authentication authentication, string path)
        {
            var drive = this.CommandContext.Drive;
            if (drive.GetObjectAsync(authentication, path) is IInfoProvider provider)
            {
                return provider;
            }
            throw new InvalidOperationException($"'{path}' does not have information.");
        }

        private T Invoke<T>(IInfoProvider provider, Func<T> func)
        {
            if (provider is IDispatcherObject dispatcherObject)
            {
                return dispatcherObject.Dispatcher.Invoke(func);
            }
            else
            {
                return func();
            }
        }
    }
}

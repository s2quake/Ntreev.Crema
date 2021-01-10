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

using JSSoft.Library.Commands;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class LockCommand : LockCommandBase, IConsoleCommand
    {
        [ImportingConstructor]
        public LockCommand()
        {

        }

        public override string[] GetCompletions(CommandCompletionContext completionContext)
        {
            if (completionContext.MemberDescriptor != null && completionContext.MemberDescriptor.DescriptorName == nameof(this.Path))
            {
                return this.CommandContext.GetCompletion(completionContext.Find, true);
            }
            return base.GetCompletions(completionContext);
        }

        [CommandPropertyRequired(DefaultValue = "")]
        public string Path
        {
            get; set;
        }

        [CommandPropertyRequired('m', AllowName = true, IsExplicit = true)]
        [CommandPropertyTrigger(nameof(Information), false)]
        public string Message
        {
            get; set;
        }

        [CommandProperty('i')]
        [CommandPropertyTrigger(nameof(Message), "")]
        public bool Information
        {
            get; set;
        }

        [CommandProperty("format", InitValue = TextSerializerType.Yaml)]
        [CommandPropertyTrigger(nameof(Information), true)]
        public TextSerializerType FormatType
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var lockable = await this.GetObjectAsync(authentication, this.GetAbsolutePath(this.Path));

            if (this.Information == true)
            {
                var lockInfo = this.Invoke(authentication, lockable, () => lockable.LockInfo);
                var prop = lockInfo.ToDictionary();
                this.CommandContext.WriteObject(prop, this.FormatType);
            }
            else
            {
                if (this.Message == string.Empty)
                {
                    throw new ArgumentException($"'{this.GetDescriptor(nameof(this.Message))}' 가 필요합니다.");
                }
                await lockable.LockAsync(authentication, this.Message);
            }
        }
    }
}

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
using JSSoft.Library.Commands;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class PrivateCommand : AccessCommandBase, IConsoleCommand
    {
        public PrivateCommand()
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

        [CommandProperty("add")]
        [CommandPropertyTrigger(nameof(MemberIDToSet), "", Group = 0)]
        [CommandPropertyTrigger(nameof(MemberIDToRemove), "", Group = 0)]
        [CommandPropertyTrigger(nameof(Information), false, Group = 0)]
        [DefaultValue("")]
        public string MemberIDToAdd
        {
            get; set;
        }

        [CommandProperty("set")]
        [CommandPropertyTrigger(nameof(MemberIDToAdd), "", Group = 0)]
        [CommandPropertyTrigger(nameof(MemberIDToRemove), "", Group = 0)]
        [CommandPropertyTrigger(nameof(Information), false, Group = 0)]
        [DefaultValue("")]
        public string MemberIDToSet
        {
            get; set;
        }

        [CommandProperty("remove")]
        [CommandPropertyTrigger(nameof(MemberIDToAdd), "", Group = 0)]
        [CommandPropertyTrigger(nameof(MemberIDToSet), "", Group = 0)]
        [CommandPropertyTrigger(nameof(Information), false, Group = 0)]
        [DefaultValue("")]
        public string MemberIDToRemove
        {
            get; set;
        }

        [CommandProperty("type")]
        [CommandPropertyTrigger(nameof(MemberIDToAdd), "", IsInequality = true, Group = 0)]
        [CommandPropertyTrigger(nameof(MemberIDToSet), "", IsInequality = true, Group = 1)]
        [CommandPropertyTrigger(nameof(MemberIDToRemove), "", IsInequality = true, Group = 2)]
        [DefaultValue(AccessType.Editor)]
        public AccessType AccessType
        {
            get; set;
        }

        [CommandProperty('i')]
        [CommandPropertyTrigger(nameof(MemberIDToAdd), "", IsInequality = true)]
        [CommandPropertyTrigger(nameof(MemberIDToSet), "", IsInequality = true)]
        [CommandPropertyTrigger(nameof(MemberIDToRemove), "", IsInequality = true)]
        public bool Information
        {
            get; set;
        }

        [CommandProperty("format")]
        [CommandPropertyTrigger(nameof(Information), true)]
        [DefaultValue(TextSerializerType.Yaml)]
        public TextSerializerType FormatType
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var authentication = this.CommandContext.GetAuthentication(this);
            var accessible = await this.GetObjectAsync(authentication, this.Path);

            if (this.MemberIDToAdd != string.Empty)
            {
                await accessible.AddAccessMemberAsync(authentication, this.MemberIDToAdd, this.AccessType);
            }
            else if (this.MemberIDToSet != string.Empty)
            {
                await accessible.SetAccessMemberAsync(authentication, this.MemberIDToSet, this.AccessType);
            }
            else if (this.MemberIDToRemove != string.Empty)
            {
                await accessible.RemoveAccessMemberAsync(authentication, this.MemberIDToRemove);
            }
            else if (this.Information == true)
            {
                var accessInfo = this.Invoke(authentication, accessible, () => accessible.AccessInfo);
                this.CommandContext.WriteObject(accessInfo.ToDictionary(), this.FormatType);
            }
            else
            {
                await accessible.SetPrivateAsync(authentication);
            }
        }
    }
}

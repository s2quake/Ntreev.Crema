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

using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles.TableContent
{
    [Export(typeof(IConsoleCommand))]
    [Category(nameof(ITableContent))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [ResourceUsageDescription("../Resources")]
    class LeaveCommand : ContentCommandBase
    {
        public LeaveCommand()
            : base("leave")
        {

        }

        [CommandProperty('s')]
        public bool IsSilent
        {
            get; set;
        }

        [CommandProperty('q')]
        public bool IsLeaveOnly
        {
            get; set;
        }

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var leave = this.IsSilent == true || this.IsLeaveOnly == true || this.CommandContext.ReadYesOrNo("leave content edit. do you proceed?");
            if (leave == true)
            {
                if (this.IsLeaveOnly == false)
                {
                    var authentication = this.CommandContext.GetAuthentication(this);
                    var domain = this.Content.Domain;
                    await this.Content.LeaveEditAsync(authentication);
                    var isAny = this.Content.Dispatcher.Invoke(() => domain.Users.Any());
                    if (isAny == false)
                    {
                        await this.Content.EndEditAsync(authentication);
                    }
                }

                // this.CommandContext.Category = null;
                this.CommandContext.Target = null;
            }
        }
    }
}

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

using JSSoft.Crema.ApplicationHost.Dialogs.ViewModels;
using JSSoft.Crema.Commands.Consoles;
using JSSoft.Library.Commands;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.ApplicationHost.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceDescription("Resources", IsShared = true)]
    class LoginCommand : ConsoleCommandAsyncBase
    {
        [CommandPropertyRequired(DefaultValue = "")]
        public string UserID
        {
            get; set;
        }

        [CommandPropertyRequired(DefaultValue = "")]
        public string Password
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.IsOnline == false;

        public new ConsoleCommandContext CommandContext => base.CommandContext as ConsoleCommandContext;

        protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                var dialog = new LoginViewModel();
                if (await dialog.ShowDialogAsync() != true)
                    return;
                await this.CommandContext.LoginAsync(dialog.UserID, dialog.Password);
            }
            catch (OperationCanceledException e)
            {
                throw new Exception("login is cancelled.", e);
            }
        }
    }
}

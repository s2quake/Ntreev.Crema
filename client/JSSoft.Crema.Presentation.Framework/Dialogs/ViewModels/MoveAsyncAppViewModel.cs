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

using JSSoft.ModernUI.Framework.Dialogs.ViewModels;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Presentation.Framework.Dialogs.ViewModels
{
    public abstract class MoveAsyncAppViewModel : MoveAsyncViewModel
    {
        private ICremaAppHost cremaAppHost;

        protected MoveAsyncAppViewModel(IServiceProvider serviceProvider, string currentPath, string[] targetPaths)
            : base(currentPath, targetPaths)
        {
            if (serviceProvider.GetService(typeof(ICremaAppHost)) is ICremaAppHost cremaAppHost)
            {
                this.cremaAppHost = cremaAppHost;
                this.cremaAppHost.Closed += CremaAppHost_Closed;
                this.cremaAppHost.Unloaded += CremaAppHost_Unloaded;
            }
        }

        protected async override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if (close == true)
            {
                if (this.cremaAppHost != null)
                {
                    this.cremaAppHost.Closed -= CremaAppHost_Closed;
                    this.cremaAppHost.Unloaded -= CremaAppHost_Unloaded;
                }
                this.cremaAppHost = null;
            }
        }

        protected virtual async Task OnCancelAsync()
        {
            await this.TryCloseAsync();
        }

        protected virtual ModalDialogAppScope Scope => ModalDialogAppScope.Loaded;

        private async void CremaAppHost_Unloaded(object sender, EventArgs e)
        {
            if (this.cremaAppHost != null && this.Scope == ModalDialogAppScope.Loaded)
            {
                await this.OnCancelAsync();
            }
        }

        private async void CremaAppHost_Closed(object sender, EventArgs e)
        {
            if (this.cremaAppHost != null)
            {
                await this.OnCancelAsync();
            }
        }
    }
}

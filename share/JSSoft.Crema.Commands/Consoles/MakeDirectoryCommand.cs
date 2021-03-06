﻿// Released under the MIT License.
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
using JSSoft.Library.ObjectModel;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Commands.Consoles
{
    [Export(typeof(IConsoleCommand))]
    [ResourceUsageDescription("Resources")]
    class MakeDirectoryCommand : ConsoleCommandAsyncBase
    {
        public MakeDirectoryCommand()
            : base("mkdir")
        {

        }

        [CommandPropertyRequired]
        public string Path
        {
            get; set;
        }

        public override bool IsEnabled => this.CommandContext.IsOnline;

        protected override async Task OnExecuteAsync(CancellationToken cancellation)
        {
            var path = this.CommandContext.GetAbsolutePath(this.Path);
            await this.MakeDirectoryAsync(path);
        }

        private async Task MakeDirectoryAsync(string path)
        {
            var drive = this.CommandContext.GetDrive(path);
            if (drive == null)
                throw new ArgumentException(string.Format(JSSoft.Library.Properties.Resources.Exception_InvalidPath_Format, path), nameof(path));
            var absolutePath = this.CommandContext.GetAbsolutePath(path);
            var authentication = this.CommandContext.GetAuthentication(this);
            if (NameValidator.VerifyCategoryPath(absolutePath))
            {
                var categoryName = new CategoryName(absolutePath);
                await drive.CreateAsync(authentication, categoryName.ParentPath, categoryName.Name);
            }
            else
            {
                var itemName = new ItemName(absolutePath);
                await drive.CreateAsync(authentication, itemName.CategoryPath, itemName.Name);
            }
        }
    }
}

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

using JSSoft.Library.IO;
using System.Collections.Generic;
using System.Linq;

namespace JSSoft.Crema.Repository.Git
{
    class GitAddCommand
    {
        private readonly List<GitCommand> commandList;

        public GitAddCommand(string basePath, GitPath[] items)
        {
            var itemList = new Queue<string>(items.Select(item => $"{item}"));
            var commandList = new List<GitCommand>();
            var command = new GitCommand(basePath, "add");
            var commandLength = command.ToString().Length;

            while (itemList.Any())
            {
                var item = itemList.Dequeue();
                if (commandLength + item.Length + 1 > 1024)
                {
                    commandList.Add(command);
                    command = new GitCommand(basePath, "add");
                    commandLength = command.ToString().Length;
                }
                commandLength += item.Length + 1;
                command.Add(item);
            }

            commandList.Add(command);
            this.commandList = commandList;
        }

        public void Run()
        {
            foreach (var item in this.commandList)
            {
                item.Run();
            }
        }
    }
}

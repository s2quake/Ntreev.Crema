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
using JSSoft.Library;
using System;
using System.ComponentModel;

namespace JSSoft.Crema.Repository.Git
{
    class GitCommand : CommandHostBase
    {
        public GitCommand(string basePath, string commandName)
            : base(GitCommand.ExecutablePath ?? "git", basePath, commandName)
        {
        }

        public string ReadLine()
        {
            return this.Run();
        }

        public string[] ReadLines()
        {
            return this.ReadLines(false);
        }

        public string[] ReadLines(bool removeEmptyLine)
        {
            if (this.Run() is string text)
                return text.Split(new string[] { Environment.NewLine }, removeEmptyLine == true ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
            return null;
        }

        public bool TryRun()
        {
            this.RunProcess();
            return this.ExitCode == 0;
        }

        public string Run()
        {
            this.RunProcess();
            if (this.ExitCode != 0)
                throw new System.Exception(this.ErrorMessage);
            return this.Message;
        }

        public string Run(ILogService logService)
        {
            logService.Debug(this.ToString());
            return this.Run();
        }

        public static string ExecutablePath { get; set; }

        /// <summary>
        /// 윈도우 경우 원인을 알 수 없는 예외 및 에러 코드가 발생
        /// 같은 명령라인으로 다시 실행하면 잘됨.
        /// </summary>
        private void RunProcess()
        {
            try
            {
                this.InvokeRun();
                if (this.ExitCode != 0)
                    this.InvokeRun();
            }
            catch (Win32Exception)
            {
                this.InvokeRun();
            }
        }
    }
}

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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JSSoft.Crema.Commands.Consoles
{
    public abstract class ConsoleTerminalBase : CommandContextTerminal
    {
        private static readonly string postfix = Terminal.IsWin32NT == true ? ">" : "$ ";
        private readonly ConsoleCommandContextBase commandContext;

        protected ConsoleTerminalBase(ConsoleCommandContextBase commandContext)
            : base(commandContext)
        {
            this.commandContext = commandContext;
            this.commandContext.PathChanged += CommandContext_PathChanged;
            this.commandContext.Executed += CommandContext_Executed;
            this.commandContext.Terminal = this;
            this.IsCommandMode = true;
        }

        public bool IsCommandMode
        {
            get; set;
        }

        protected override string FormatPrompt(string prompt)
        {
            if (this.IsCommandMode == false)
            {
                return base.FormatPrompt(prompt);
            }
            else
            {
                var postfixPattern = string.Join(string.Empty, postfix.Select(item => $"[{item}]"));
                var tb = new TerminalStringBuilder();
                if (this.commandContext.IsOnline == false)
                {
                    var match = Regex.Match(prompt, $"(.+)(?<postfix>{postfixPattern})$");
                    tb.Foreground = TerminalColor.BrightGreen;
                    tb.Append(match.Groups[1].Value);
                    tb.Foreground = null;
                    tb.Append(match.Groups[2].Value);
                }
                else
                {
                    var p1 = prompt.TrimStart();
                    var p2 = prompt.TrimEnd();
                    var prefix = prompt.Substring(p1.Length);
                    var postfix = prompt.Substring(p2.Length);
                    var uri = new Uri(prompt.Trim());

                    tb.Append(prefix);
                    tb.Foreground = TerminalColor.BrightGreen;
                    tb.Append(uri.Scheme);
                    tb.Foreground = null;
                    tb.Append(Uri.SchemeDelimiter);
                    tb.Foreground = TerminalColor.BrightCyan;
                    tb.Append(uri.UserInfo);
                    tb.Foreground = null;
                    tb.Append("@");
                    tb.Foreground = TerminalColor.BrightCyan;
                    tb.Append(uri.Authority);
                    tb.Foreground = null;
                    tb.Append(uri.LocalPath);
                    tb.Append(postfix);
                }
                tb.AppendEnd();
                return tb.ToString();
            }
        }

        protected void SetPrompt()
        {
            base.Prompt = this.commandContext.Prompt + postfix;
        }

        protected override bool OnPreviewExecute(string command)
        {
            var result = base.OnPreviewExecute(command);
            this.commandContext.PreExecute();
            return result;
        }

        protected override void OnExecuted(Exception e)
        {
            base.OnExecuted(e);
            this.commandContext.PostExecute();
        }

        private void CommandContext_Executed(object sender, EventArgs e)
        {
            this.SetPrompt();
        }

        private void CommandContext_PathChanged(object sender, EventArgs e)
        {
            this.SetPrompt();
        }
    }
}

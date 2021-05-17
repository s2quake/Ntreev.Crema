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

using JSSoft.Crema.ApplicationHost.Commands.Consoles;
using JSSoft.ModernUI.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace JSSoft.Crema.ApplicationHost.Controls
{
    class ConsoleCommandControl : TerminalControl
    {
        public ConsoleCommandControl()
        {
            this.SetResourceReference(StyleProperty, typeof(TerminalControl));
        }

        public ConsoleCommandContext CommandContext { get; set; }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override Inline[] GetPrompt(string prompt)
        {
            if (prompt == string.Empty)
            {
                return base.GetPrompt(prompt);
            }
            else
            {
                var match = Regex.Match(prompt, $"(.+)(?<postfix>[>] )$");
                if (Uri.TryCreate(match.Groups[1].Value, UriKind.Absolute, out var uri) == true)
                {
                    var runList = new List<Run>
                    {
                        new Run() { Text = uri.Scheme, Foreground = Brushes.Green },
                        new Run() { Text = Uri.SchemeDelimiter },
                        new Run() { Text = uri.UserInfo, Foreground = Brushes.Cyan },
                        new Run() { Text = "@" },
                        new Run() { Text = uri.Authority, Foreground = Brushes.Cyan },
                        new Run() { Text = uri.LocalPath },
                        new Run() { Text = match.Groups[2].Value }
                    };
                    return runList.ToArray();
                }
                else
                {
                    return new Run[]
                    {
                        new Run() { Text = match.Groups[1].Value, Foreground = Brushes.Green },
                        new Run() { Text = match.Groups[2].Value },
                    };
                }
            }
        }
    }
}

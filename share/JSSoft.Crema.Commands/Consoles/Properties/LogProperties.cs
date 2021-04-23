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
using System;
using System.ComponentModel;
using System.IO;

namespace JSSoft.Crema.Commands.Consoles.Properties
{
    [ResourceUsageDescription("../Resources")]
    static class LogProperties
    {
        [CommandProperty("quiet", 'q')]
        [DefaultValue(false)]
        public static bool IsQuiet
        {
            get; set;
        }

        [CommandProperty("limit", 'l')]
        [DefaultValue(-1)]
        public static int Limit
        {
            get; set;
        }

        public static void Print(TextWriter writer, LogInfo[] logs)
        {
            var count = 0;

            var tb = new TerminalStringBuilder();
            tb.AppendLine();
            tb.AppendLine(string.Empty.PadRight(Console.BufferWidth - 1, '='));

            foreach (var item in logs)
            {
                if (LogProperties.Limit >= 0 && LogProperties.Limit <= count)
                    break;

                tb.Foreground = TerminalColor.Red;
                tb.AppendLine($"Revision: {item.Revision}");
                tb.Foreground = null;
                tb.AppendLine($"Author  : {item.UserID}");
                tb.AppendLine($"Date    : {item.DateTime}");
                if (IsQuiet == false)
                {
                    tb.AppendLine();
                    tb.AppendLine(item.Comment);
                }
                tb.AppendLine(string.Empty.PadRight(Console.BufferWidth - 1, '='));
                count++;
            }
            tb.AppendLine();

            writer.Write(tb.ToString());
        }
    }
}

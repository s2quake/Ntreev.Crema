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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JSSoft.Crema.Services
{
    public static class CremaLog
    {
        private static LogService log;

        public static void Debug(object message)
        {
            LogService.Debug(message);
        }

        public static void Info(object message)
        {
            LogService.Info(message);
        }

        public static void Error(object message)
        {
            LogService.Error(message);
        }

        public static void Warn(object message)
        {
            LogService.Warn(message);
        }

        public static void Fatal(object message)
        {
            LogService.Fatal(message);
        }

        public static void Debug(string format, params object[] args)
        {
            LogService.Debug(string.Format(format, args));
        }

        public static void Info(string format, params object[] args)
        {
            LogService.Info(string.Format(format, args));
        }

        public static void Error(string format, params object[] args)
        {
            LogService.Error(string.Format(format, args));
        }

        public static void Error(Exception e)
        {
            LogService.Error(e.ToString());
        }

        public static void Warn(string format, params object[] args)
        {
            LogService.Warn(string.Format(format, args));
        }

        public static void Fatal(string format, params object[] args)
        {
            LogService.Fatal(string.Format(format, args));
        }

        public static void AddRedirection(TextWriter writer, LogLevel verbose)
        {
            LogService.AddRedirection(writer, verbose);
        }

        public static void RemoveRedirection(TextWriter writer)
        {
            LogService.RemoveRedirection(writer);
        }

        public static LogLevel Verbose
        {
            get => LogService.LogLevel;
            set => LogService.LogLevel = value;
        }

        internal static LogService LogService
        {
            get
            {
                if (log == null)
                    log = new LogService();
                return log;
            }
        }
    }
}

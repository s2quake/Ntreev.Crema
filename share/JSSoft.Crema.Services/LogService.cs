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

using JSSoft.Library;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JSSoft.Crema.Services
{
    class LogService : IDisposable
    {
        private const string conversionPattern = "%message%newline%exception";
        private static readonly LoggingConfiguration configuration = new();
        private readonly Dictionary<TextWriter, TextWriterTarget> targetByTextWriter = new();
        private readonly LoggingRule consoleRule;
        private ILogger log;
        private LogLevel logLevel = LogLevel.Info;

        static LogService()
        {
            LogManager.Configuration = configuration;
            AppDomain.CurrentDomain.ProcessExit += (s, e) => LogManager.Shutdown();
        }

        public LogService()
            : this("global", Path.Combine(AppUtility.UserAppDataPath, "logs"))
        {

        }

        public LogService(string name, string path)
        {
            var fileName = Path.Combine(path, $"{name}.txt");
            var archiveFileName = Path.Combine(path, $"{name}.{{#}}.txt");
            var fileLayout = new SimpleLayout("${longdate}|${level:uppercase=true}|${message}${onexception:${newline}${exception:format=tostring}}");
            var fileTarget = new FileTarget($"{name}_file")
            {
                FileName = fileName,
                ArchiveFileName = archiveFileName,
                ArchiveEvery = FileArchivePeriod.Day,
                Layout = fileLayout
            };
            var consoleLayout = new SimpleLayout("${message}");
            var consoleTarget = new ConsoleTarget($"{name}_console")
            {
                Layout = consoleLayout,
            };
            var fileRule = new LoggingRule(name, NLog.LogLevel.Debug, NLog.LogLevel.Fatal, fileTarget)
            {
                RuleName = $"{name}_file"
            };
            var consoleRule = new LoggingRule(name, GetLevel(this.logLevel), NLog.LogLevel.Fatal, consoleTarget)
            {
                RuleName = $"{name}_console"
            };
            configuration.LoggingRules.Add(fileRule);
            configuration.LoggingRules.Add(consoleRule);
            this.Refresh();
            this.log = NLog.LogManager.GetLogger(name);
            this.consoleRule = consoleRule;
            this.Name = name;
            this.FileName = fileName;
        }

        public override string ToString()
        {
            return $"{nameof(LogService)}: {this.Name}";
        }

        public void Debug(object message)
        {
            this.log.Debug(message);
        }

        public void Info(object message)
        {
            this.log.Info(message);
        }

        public void Error(object message)
        {
            if (message is Exception e)
            {
                this.log.Error(e, e.Message);
            }
            else
            {
                this.log.Error(message);
            }
        }

        public void Warn(object message)
        {
            this.log.Warn(message);
        }

        public void Fatal(object message)
        {
            this.log.Fatal(message);
        }

        public void AddRedirection(TextWriter writer, LogLevel verbose)
        {
            if (this.targetByTextWriter.ContainsKey(writer) == true)
                throw new InvalidOperationException();

            var name = $"{this.Name}_{writer.GetHashCode()}";
            var target = new TextWriterTarget(writer)
            {
                Name = name,
                Layout = new SimpleLayout("${message}")
            };
            var rule = new LoggingRule(this.Name, GetLevel(verbose), NLog.LogLevel.Fatal, target)
            {
                RuleName = name
            };
            configuration.LoggingRules.Add(rule);
            this.targetByTextWriter.Add(writer, target);
            this.Refresh();
        }

        public void RemoveRedirection(TextWriter writer)
        {
            if (this.targetByTextWriter.ContainsKey(writer) == false)
                throw new InvalidOperationException();

            var target = this.targetByTextWriter[writer];
            var name = $"{this.Name}_{writer.GetHashCode()}";
            configuration.RemoveTarget(name);
            configuration.RemoveRuleByName(name);
            this.targetByTextWriter.Remove(writer);
            this.Refresh();
        }

        public void Dispose()
        {
            this.log = null;
        }

        public LogLevel LogLevel
        {
            get => this.logLevel;
            set
            {
                if (this.logLevel == value)
                    return;
                this.logLevel = value;
                this.consoleRule.SetLoggingLevels(GetLevel(value), NLog.LogLevel.Fatal);
                this.Refresh();
            }
        }

        public string Name { get; set; }

        public string FileName { get; }

        private static NLog.LogLevel GetLevel(LogLevel verbose)
        {
            if (verbose == LogLevel.Debug)
                return NLog.LogLevel.Debug;
            else if (verbose == LogLevel.Info)
                return NLog.LogLevel.Info;
            else if (verbose == LogLevel.Error)
                return NLog.LogLevel.Error;
            else if (verbose == LogLevel.Warn)
                return NLog.LogLevel.Warn;
            else if (verbose == LogLevel.Fatal)
                return NLog.LogLevel.Fatal;
            return NLog.LogLevel.Off;
        }

        private void Refresh()
        {
            NLog.LogManager.Configuration = configuration.Reload();
        }

        #region TextWriterTarget

        sealed class TextWriterTarget : TargetWithLayout
        {
            private readonly TextWriter textWriter;

            public TextWriterTarget(TextWriter textWriter)
            {
                this.textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
                this.OptimizeBufferReuse = true;
            }

            protected override void Write(LogEventInfo logEvent)
            {
                var logMessage = this.RenderLogEvent(Layout, logEvent);
                this.textWriter.WriteLine(logMessage);
            }
        }

        #endregion
    }
}

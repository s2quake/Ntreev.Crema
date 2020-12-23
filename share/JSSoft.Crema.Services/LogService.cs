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

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using JSSoft.Library;
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
        private readonly ILog log;
        private LogVerbose verbose = LogVerbose.Info | LogVerbose.Error | LogVerbose.Warn | LogVerbose.Fatal;
        private ConsoleAppender consoleAppender;
        private readonly RollingFileAppender rollingAppender;
        private Hierarchy hierarchy;
        //private TextWriter redirectionWriter;
        //private TextWriterAppender textAppender;
        private readonly Dictionary<TextWriter, TextWriterAppender> appendersByWriter = new Dictionary<TextWriter, TextWriterAppender>();

        public LogService()
        {
            var repositoryName = AppUtility.ProductName;
            var name = "global";

            var repository = LogManager.GetAllRepositories().Where(item => item.Name == repositoryName).FirstOrDefault();
            if (repository != null)
                throw new InvalidOperationException();

            this.hierarchy = (Hierarchy)LogManager.CreateRepository(repositoryName);

            var xmlLayout = new XmlLayoutSchemaLog4j()
            {
                LocationInfo = true
            };
            xmlLayout.ActivateOptions();

            this.rollingAppender = new RollingFileAppender()
            {
                AppendToFile = true,
                File = Path.Combine(AppUtility.UserAppDataPath, "logs", "log"),
                DatePattern = "_yyyy-MM-dd'.xml'",
                Layout = xmlLayout,
                Encoding = Encoding.UTF8,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                RollingStyle = RollingFileAppender.RollingMode.Date,
                StaticLogFileName = false,
                LockingModel = new RollingFileAppender.MinimalLock()
            };
            this.rollingAppender.ActivateOptions();

            var patternLayout = new PatternLayout()
            {
                ConversionPattern = conversionPattern
            };
            patternLayout.ActivateOptions();

            this.consoleAppender = new ConsoleAppender()
            {
                Layout = patternLayout
            };
            this.consoleAppender.SetVerbose(this.verbose);
            this.consoleAppender.ActivateOptions();

            this.hierarchy.Root.AddAppender(this.consoleAppender);
            this.hierarchy.Root.AddAppender(this.rollingAppender);

            this.hierarchy.Root.Level = Level.All;
            this.hierarchy.Root.Additivity = false;
            this.hierarchy.Configured = true;

            this.log = log4net.LogManager.GetLogger(repositoryName, name);
            this.Name = name;
        }

        public LogService(string name, string path, bool isSingle)
        {
            var repository = LogManager.GetAllRepositories().Where(item => item.Name == name).FirstOrDefault();
            if (repository != null)
            {
                this.hierarchy = (Hierarchy)LogManager.GetRepository(name);
            }
            else
            {
                this.hierarchy = (Hierarchy)LogManager.CreateRepository(name);
            }

            var xmlLayout = new XmlLayoutSchemaLog4j()
            {
                LocationInfo = true
            };
            xmlLayout.ActivateOptions();

            this.rollingAppender = new RollingFileAppender()
            {
                AppendToFile = true,
                File = Path.Combine(path, name),
                DatePattern = "-yyyy-MM-dd'.log'",
                Layout = xmlLayout,
                Encoding = Encoding.UTF8,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                StaticLogFileName = false
            };

            this.rollingAppender.File = isSingle == true ? Path.Combine(path, $"{name}-{DateTime.Now:yyyy-MM-d--HH-mm-ss}.log") : Path.Combine(path, name);
            this.rollingAppender.RollingStyle = isSingle == true ? RollingFileAppender.RollingMode.Once : RollingFileAppender.RollingMode.Date;
            this.rollingAppender.ActivateOptions();

            var patternLayout = new PatternLayout()
            {
                ConversionPattern = conversionPattern
            };
            patternLayout.ActivateOptions();

            this.consoleAppender = new ConsoleAppender()
            {
                Layout = patternLayout
            };
            this.consoleAppender.SetVerbose(this.verbose);
            this.consoleAppender.ActivateOptions();

            this.hierarchy.Root.AddAppender(this.consoleAppender);
            this.hierarchy.Root.AddAppender(this.rollingAppender);

            this.hierarchy.Root.Level = Level.All;
            this.hierarchy.Configured = true;

            this.log = log4net.LogManager.GetLogger(name, typeof(LogService));
            this.Name = name;
        }

        public LogService(string address, string userID, string path)
        {
            var repositoryName = $"{address}_{userID}";
            var repository = LogManager.GetAllRepositories().Where(item => item.Name == repositoryName).FirstOrDefault();
            if (repository != null)
            {
                //this.log = log4net.LogManager.GetLogger(repositoryName, name);
                this.hierarchy = (Hierarchy)LogManager.GetRepository(repositoryName);
            }
            else
            {
                this.hierarchy = (Hierarchy)LogManager.CreateRepository(repositoryName);
            }

            var xmlLayout = new XmlLayoutSchemaLog4j()
            {
                LocationInfo = true
            };
            xmlLayout.ActivateOptions();

            this.rollingAppender = new RollingFileAppender()
            {
                AppendToFile = true,
                File = Path.Combine(path, @"logs", $"{repositoryName}"),
                DatePattern = "_yyyy-MM-dd'.xml'",
                Layout = xmlLayout,
                Encoding = Encoding.UTF8,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                RollingStyle = RollingFileAppender.RollingMode.Date,
                StaticLogFileName = false
            };
            this.rollingAppender.ActivateOptions();

            var patternLayout = new PatternLayout()
            {
                ConversionPattern = conversionPattern
            };
            patternLayout.ActivateOptions();

            this.consoleAppender = new ConsoleAppender()
            {
                Layout = patternLayout
            };
            this.consoleAppender.SetVerbose(this.verbose);
            this.consoleAppender.ActivateOptions();

            this.hierarchy.Root.AddAppender(this.consoleAppender);
            this.hierarchy.Root.AddAppender(this.rollingAppender);

            this.hierarchy.Root.Level = Level.All;
            this.hierarchy.Configured = true;

            this.log = log4net.LogManager.GetLogger(repositoryName, userID);
            this.Name = userID;
        }

        public override string ToString()
        {
            return $"{nameof(LogService)}: {this.Name}";
        }

        public void Debug(object message)
        {
            if (CremaLog.Dispatcher != null)
            {
                CremaLog.Dispatcher.InvokeAsync(() => this.log.Debug(message));
            }
            else
            {
                this.log.Debug(message);
            }
        }

        public void Info(object message)
        {
            if (CremaLog.Dispatcher != null)
            {
                CremaLog.Dispatcher.InvokeAsync(() => this.log.Info(message));
            }
            else
            {
                this.log.Info(message);
            }
        }

        public void Error(object message)
        {
            if (CremaLog.Dispatcher != null)
            {
                CremaLog.Dispatcher?.InvokeAsync(() =>
                {
                    if (message is Exception e)
                    {
                        this.log.Error(e.Message, e);
                    }
                    else
                    {
                        this.log.Error(message);
                    }
                });
            }
            else
            {
                if (message is Exception e)
                {
                    this.log.Error(e.Message, e);
                }
                else
                {
                    this.log.Error(message);
                }
            }
        }

        public void Warn(object message)
        {
            if (CremaLog.Dispatcher != null)
            {
                CremaLog.Dispatcher.InvokeAsync(() => this.log.Warn(message));
            }
            else
            {
                this.log.Warn(message);
            }
        }

        public void Fatal(object message)
        {
            if (CremaLog.Dispatcher != null)
            {
                CremaLog.Dispatcher.InvokeAsync(() => this.log.Fatal(message));
            }
            else
            {
                this.log.Fatal(message);
            }
        }

        public void Dispose()
        {
            this.consoleAppender = null;
            this.appendersByWriter.Clear();
            this.hierarchy?.Shutdown();
            this.hierarchy = null;
        }

        public void AddRedirection(TextWriter writer, LogVerbose verbose)
        {
            var patternLayout = new PatternLayout()
            {
                ConversionPattern = conversionPattern
            };
            patternLayout.ActivateOptions();

            var textAppender = new TextWriterAppender()
            {
                Layout = patternLayout,
                Writer = writer,
            };
            textAppender.SetVerbose(verbose);
            textAppender.ActivateOptions();
            this.hierarchy.Root.AddAppender(textAppender);
            this.appendersByWriter.Add(writer, textAppender);
        }

        public void RemoveRedirection(TextWriter writer)
        {
            if (this.appendersByWriter.ContainsKey(writer) == false)
                throw new ArgumentException("not found writer", nameof(writer));
            var textAppender = this.appendersByWriter[writer];
            this.hierarchy.Root.RemoveAppender(textAppender);
            this.appendersByWriter.Remove(writer);
        }

        public LogVerbose Verbose
        {
            get => this.verbose;
            set
            {
                if (this.verbose == value)
                    return;
                this.verbose = value;
                this.consoleAppender.SetVerbose(this.verbose);
                this.consoleAppender.ActivateOptions();
            }
        }

        //public TextWriter RedirectionWriter
        //{
        //    get { return this.redirectionWriter; }
        //    set
        //    {
        //        if (this.redirectionWriter == value)
        //            return;

        //        var oldWriter = this.redirectionWriter;

        //        this.redirectionWriter = value;

        //        if (this.redirectionWriter != null)
        //        {
        //            var patternLayout = new PatternLayout()
        //            {
        //                ConversionPattern = conversionPattern
        //            };
        //            patternLayout.ActivateOptions();

        //            this.textAppender = new TextWriterAppender()
        //            {
        //                Layout = patternLayout,
        //                Writer = this.redirectionWriter,
        //            };
        //            this.textAppender.ActivateOptions();
        //            this.hierarchy.Root.AddAppender(this.textAppender);
        //        }
        //        else
        //        {
        //            if (this.textAppender != null)
        //            {
        //                this.hierarchy.Root.RemoveAppender(this.textAppender);
        //            }
        //        }
        //    }
        //}

        public string Name { get; set; }

        public string FileName
        {
            get
            {
                if (this.rollingAppender == null)
                    return string.Empty;
                return this.rollingAppender.File;
            }
        }

        private void InitializeVerbose()
        {

        }
    }

    static class LogServiceExtensions
    {
        public static void SetVerbose(this AppenderSkeleton appender, LogVerbose verbose)
        {
            appender.ClearFilters();
            foreach (var item in Enum.GetValues(typeof(LogVerbose)))
            {
                var member = (LogVerbose)item;
                var level = GetLevel(member);
                if (level == null || verbose.HasFlag(member) == false)
                    continue;

                appender.AddFilter(new log4net.Filter.LevelMatchFilter()
                {
                    AcceptOnMatch = true,
                    LevelToMatch = level,
                });
            }
            appender.AddFilter(new log4net.Filter.DenyAllFilter());
        }

        private static Level GetLevel(LogVerbose verbose)
        {
            if (verbose == LogVerbose.Debug)
                return Level.Debug;
            else if (verbose == LogVerbose.Info)
                return Level.Info;
            else if (verbose == LogVerbose.Error)
                return Level.Error;
            else if (verbose == LogVerbose.Warn)
                return Level.Warn;
            else if (verbose == LogVerbose.Fatal)
                return Level.Fatal;
            return null;
        }
    }
}
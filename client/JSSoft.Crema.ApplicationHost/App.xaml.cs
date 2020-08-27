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
using JSSoft.Library.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace JSSoft.Crema.ApplicationHost
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            CremaLog.AddRedirection(Writer, LogVerbose.Debug);
        }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (this.ParseCommandLine() == false)
            {
                this.Shutdown();
                return;
            }

            var splash = new Views.SplashWindow()
            {
                Title = AppUtility.ProductName,
                ThemeColor = JSSoft.Crema.ApplicationHost.Properties.Settings.Default.ThemeColor,
                Background = JSSoft.Crema.ApplicationHost.Properties.Settings.Default.Background,
                Foreground = JSSoft.Crema.ApplicationHost.Properties.Settings.Default.Foreground
            };
            splash.Show();

            if (this.FindResource("bootstrapper") is AppBootstrapper bootstrapper)
            {
                bootstrapper.Initialize();
            }
            base.OnStartup(e);
            splash.Close();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            CremaLog.RemoveRedirection(Writer);
            base.OnExit(e);
        }

        private bool ParseCommandLine()
        {
            try
            {
                if (this.FindResource("bootstrapper") is AppBootstrapper bootstrapper)
                {
                    var sb = new StringBuilder();
                    var parser = new CommandLineParser(bootstrapper.Settings)
                    {
                        Out = new StringWriter(sb)
                    };
                    var (name, arguments) = CommandStringUtility.Split(Environment.CommandLine);
                    if (parser.TryParse(name, arguments) == false && arguments != string.Empty)
                    {
                        MessageBox.Show(sb.ToString(), "Usage", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        internal static LogWriter Writer { get; } = new LogWriter();

        #region classes

        class DebugTraceListener : TraceListener
        {
            public override void Write(string message)
            {
            }

            public override void WriteLine(string message)
            {
                Debugger.Break();
            }
        }

        #endregion
    }
}

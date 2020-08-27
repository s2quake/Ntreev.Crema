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
using JSSoft.Crema.Services;
using JSSoft.Library.Commands;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.WindowsServiceHost
{
    public partial class WindowCremaService : ServiceBase
    {
        private CremaApplication cremaApp;
        private ICremaHost cremaHost;

        public WindowCremaService()
        {
            this.ServiceName = "Crema Service";
            this.EventLog.Log = "Crema";

            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            var baseArgs = Environment.GetCommandLineArgs();
            var path = baseArgs[1];
            var port = int.Parse(baseArgs[2]);

            this.cremaApp = new CremaApplication()
            {
                BasePath = path,
                Port = port,
            };

            this.EventLog.WriteEntry($"service base path : {path}");
            this.EventLog.WriteEntry($"service port : {port}");

            try
            {
                this.EventLog.WriteEntry($"args length: {args.Length}");
                if (args.Any() == true)
                {
                    var settings = new Settings()
                    {
                        BasePath = this.cremaApp.BasePath,
                        Port = this.cremaApp.Port,
                    };
                    var parser = new CommandLineParser("setting", settings);
                    parser.Parse(string.Join(" ", "setting", string.Join(" ", args)));

                    this.cremaApp.BasePath = settings.BasePath;
                    this.cremaApp.Port = settings.Port;

                    this.EventLog.WriteEntry($"=========================================================");
                    this.EventLog.WriteEntry($"new settings");
                    this.EventLog.WriteEntry($"service base path : {settings.BasePath}");
                    this.EventLog.WriteEntry($"service port : {settings.Port}");
                    this.EventLog.WriteEntry($"service repo module : {settings.RepositoryModule}");
                    this.EventLog.WriteEntry($"=========================================================");
                }
            }
            catch (Exception e)
            {
                CremaLog.Error(e);
                throw e;
            }

            this.EventLog.WriteEntry("service open");
            this.cremaApp.Open();
            this.cremaHost = this.cremaApp.GetService(typeof(ICremaHost)) as ICremaHost;
            this.cremaApp.Closed += CremaApp_Closed;
            this.EventLog.WriteEntry("service opened.");
        }

        protected override void OnStop()
        {
            base.OnStop();
            this.EventLog.WriteEntry("service close");
            this.cremaApp.Close();
            this.EventLog.WriteEntry("service closed.");
        }

        private void CremaApp_Closed(object sender, ClosedEventArgs e)
        {
            this.EventLog.WriteEntry($"{nameof(CremaApp_Closed)} {e.Reason}");
            if (e.Reason == CloseReason.Shutdown)
            {
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    this.Stop();
                });
            }
        }
    }
}

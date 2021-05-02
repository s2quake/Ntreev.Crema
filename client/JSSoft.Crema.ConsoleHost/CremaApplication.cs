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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSSoft.Crema.ConsoleHost
{
    class CremaApplication : CremaBootstrapper
    {
        private readonly CremaService service;

        public CremaApplication()
        {
            this.service = this.GetService(typeof(CremaService)) as CremaService;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public async Task RunAsync()
        {
            var configs = this.GetService(typeof(ConsoleConfiguration)) as ConsoleConfiguration;
            var commandContext = this.GetService(typeof(CommandContext)) as CommandContext;
            await commandContext.ExecuteAsync(Environment.CommandLine);
            configs.Commit();
        }

        public override IEnumerable<Tuple<System.Type, object>> GetParts()
        {
            var service = new CremaService(this);
            foreach (var item in base.GetParts())
            {
                yield return item;
            }
            yield return new Tuple<Type, object>(typeof(CremaApplication), this);
            yield return new Tuple<Type, object>(typeof(CremaService), service);
            yield return new Tuple<Type, object>(typeof(ICremaService), service);
        }

        public Task OpenAsync()
        {
            return this.service.OpenAsync();
        }

        public Task CloseAsync()
        {
            return this.service.CloseAsync();
        }
        
        public ServiceState ServiceState => this.service.ServiceState;

        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public event EventHandler Opening
        {
            add { this.service.Opening += value; }
            remove { this.service.Opening -= value; }
        }

        public event EventHandler Opened
        {
            add { this.service.Opened += value; }
            remove { this.service.Opened -= value; }
        }

        public event EventHandler Closing
        {
            add { this.service.Closing += value; }
            remove { this.service.Closing -= value; }
        }

        public event ClosedEventHandler Closed
        {
            add { this.service.Closed += value; }
            remove { this.service.Closed -= value; }
        }

        protected override void OnDisposed(EventArgs e)
        {
            base.OnDisposed(e);
            this.service.Dispose();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var publishers = this.GetService(typeof(IEnumerable<IExceptionPublisher>)) as IEnumerable<IExceptionPublisher>;
                foreach (var item in publishers)
                {
                    item.Publish(e.ExceptionObject);
                }
            }
            catch (Exception)
            {
            }

            Environment.Exit(-1);
        }
    }
}

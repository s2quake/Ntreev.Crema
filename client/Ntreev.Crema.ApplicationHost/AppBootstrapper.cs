﻿//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Ntreev.Crema.Presentation.Framework;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Diagnostics;
using Ntreev.Crema.ServiceModel;
using System.IO;
using Ntreev.Crema.ApplicationHost.ViewModels;
using System.Threading.Tasks;
using Ntreev.ModernUI.Framework;
using Ntreev.Crema.Services;
using System.Globalization;
using Ntreev.Library;
using Ntreev.Library.Commands;
using System.Windows.Threading;
using System.Text;
using Ntreev.Library.IO;

namespace Ntreev.Crema.ApplicationHost
{
    class AppBootstrapper : AppBootstrapperBase
    {
        public AppBootstrapper()
            : base(new AppBootstrapperDescriptor())
        {
            AppMessageBox.MessageSelector = MessageSelector;
        }

        public AppSettings Settings => this.Descriptor.Settings;

        protected new AppBootstrapperDescriptor Descriptor => base.Descriptor as AppBootstrapperDescriptor;

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            if (this.Settings.Culture != string.Empty)
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(this.Settings.Culture);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(this.Settings.Culture);
            }

            base.OnStartup(sender, e);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);
            this.PublishException(e);
            e.Handled = true;
            if (Application.Current != null)
                Application.Current.Shutdown(-1);
            else
                Environment.Exit(-1);
        }

        protected override bool AutoInitialize => false;

        private void PublishException(DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                var publishers = this.GetService(typeof(IEnumerable<IExceptionPublisher>)) as IEnumerable<IExceptionPublisher>;
                foreach (var item in publishers)
                {
                    if (e.Exception == null)
                        item.Publish(null);
                    else
                        item.Publish(e.Exception);

                }
            }
            catch (Exception)
            {
            }
        }

        private static string MessageSelector(object message)
        {
            if (message is Exception exception)
            {
                CremaLog.Error(exception);
            }
            return AppMessageBox.DefaultMessageSelector(message);
        }
    }
}

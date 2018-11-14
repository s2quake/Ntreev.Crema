using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ntreev.Crema.ApplicationHost
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (this.FindResource("bootstrapper") is AppBootstrapper bootstrapper)
            {
                bootstrapper.Initialize();
            }
            base.OnStartup(e);
        }
    }
}

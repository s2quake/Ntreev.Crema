using Ntreev.Crema.Services;
using Ntreev.Library.Commands;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
                if (this.ParseCommandLine(bootstrapper.Settings) == false || bootstrapper.ApplySettings() == false)
                {
                    this.Shutdown();
                    return;
                }
                bootstrapper.Initialize();
            }
            base.OnStartup(e);
        }

        private bool ParseCommandLine(AppSettings settings)
        {
            try
            {
                var sb = new StringBuilder();
                var parser = new CommandLineParser(settings)
                {
                    Out = new StringWriter(sb)
                };
                if (parser.Parse(Environment.CommandLine) == false)
                {
                    MessageBox.Show(sb.ToString(), "Usage", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}

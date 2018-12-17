//Released under the MIT License.
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

using Ntreev.Crema.ServiceHosts;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ntreev.Crema.ApplicationHost
{
    class AppBootstrapper : AppBootstrapper<IShell>
    {
        private readonly static Dictionary<string, Uri> themes = new Dictionary<string, Uri>(StringComparer.CurrentCultureIgnoreCase);
        private readonly CremaService service;

        static AppBootstrapper()
        {
            themes.Add("dark", new Uri("/Ntreev.ModernUI.Framework;component/Assets/ModernUI.Dark.xaml", UriKind.Relative));
            themes.Add("light", new Uri("/Ntreev.ModernUI.Framework;component/Assets/ModernUI.Light.xaml", UriKind.Relative));
            //FirstFloor.ModernUI.Presentation.AppearanceManager.Current.AccentColor = value;
        }

        public AppBootstrapper()
        {
            this.service = new CremaService(this);
        }

        public AppSettings Settings { get; } = new AppSettings();

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
        }

        public bool ApplySettings()
        {
            try
            {
                if (this.Settings.Culture != string.Empty)
                {
                    System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(this.Settings.Culture);
                    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(this.Settings.Culture);
                }

                if (this.Settings.Theme != string.Empty)
                {
                    FirstFloor.ModernUI.Presentation.AppearanceManager.Current.ThemeSource = themes[this.Settings.Theme];
                }

                if (this.Settings.ThemeColor != string.Empty)
                {
                    var themeColor = (Color)ColorConverter.ConvertFromString(this.Settings.ThemeColor);
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
            this.service.Dispose();
        }

        protected override IEnumerable<Tuple<Type, object>> GetParts()
        {
            foreach (var item in base.GetParts())
            {
                yield return item;
            }
            yield return new Tuple<Type, object>(typeof(CremaService), this.service);
            yield return new Tuple<Type, object>(typeof(ICremaService), this.service);
            yield return new Tuple<Type, object>(typeof(AppSettings), this.Settings);
        }

        protected override IEnumerable<string> SelectPath()
        {
            var items = base.SelectPath().Concat(CremaBootstrapper.SelectPath(AppDomain.CurrentDomain.BaseDirectory)).Distinct();
            foreach (var item in items)
            {
                yield return item;
            }

            if (this.Settings.PluginsPath != null)
            {
                foreach (var item in this.Settings.PluginsPath)
                {
                    yield return item;
                }
            }
        }

        protected override bool AutoInitialize => false;
    }
}

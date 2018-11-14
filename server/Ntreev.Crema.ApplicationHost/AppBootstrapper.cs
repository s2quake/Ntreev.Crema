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

namespace Ntreev.Crema.ApplicationHost
{
    class AppBootstrapper : AppBootstrapper<IShell>
    {
        private readonly CremaService service;

        public AppBootstrapper()
        {
            this.service = new CremaService(this);
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
        }

        protected override IEnumerable<string> SelectPath()
        {
            return base.SelectPath().Concat(CremaBootstrapper.SelectPath(AppDomain.CurrentDomain.BaseDirectory)).Distinct();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
        }

        protected override bool AutoInitialize => false;
    }
}

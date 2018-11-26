using Ntreev.Crema.Commands;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.ApplicationHost
{
    [Export(typeof(IConfigurationProperties))]
    class ConfigurationProperties : IConfigurationProperties
    {
        private readonly ICremaHost cremaHost;
        private readonly List<ConfigurationPropertyDescriptor> disabledProperties;

        [ImportingConstructor]
        public ConfigurationProperties(ICremaHost cremaHost, [ImportMany]IEnumerable<IConfigurationPropertyProvider> providers)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaHost.Closed += CremaHost_Closed;
            this.Properties = new ConfigurationPropertyDescriptorCollection(providers);
            this.disabledProperties = new List<ConfigurationPropertyDescriptor>();

            if (this.cremaHost.ServiceState == ServiceState.None)
                this.DetachCremaConfigs();
        }

        public ConfigurationPropertyDescriptorCollection Properties { get; }

        private void CremaHost_Closed(object sender, EventArgs e)
        {
            this.DetachCremaConfigs();
        }

        private void CremaHost_Opened(object sender, EventArgs e)
        {
            this.AttachCremaConfigs();
        }

        private void AttachCremaConfigs()
        {
            foreach (var item in this.disabledProperties)
            {
                this.Properties.Add(item);
            }
            this.disabledProperties.Clear();
        }

        private void DetachCremaConfigs()
        {
            foreach (var item in this.Properties.ToArray())
            {
                if (item.ScopeType == typeof(IRepositoryConfiguration))
                {
                    this.Properties.Remove(item);
                    this.disabledProperties.Add(item);
                }
            }
        }

        #region IConfigurationProperties

        IContainer<ConfigurationPropertyDescriptor> IConfigurationProperties.Properties => this.Properties;

        #endregion
    }
}

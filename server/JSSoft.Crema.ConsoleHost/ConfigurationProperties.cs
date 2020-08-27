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

using JSSoft.Crema.Commands;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace JSSoft.Crema.ConsoleHost
{
    [Export(typeof(IConfigurationProperties))]
    public class ConfigurationProperties : IConfigurationProperties
    {
        private readonly ICremaHost cremaHost;
        private readonly List<ConfigurationPropertyDescriptor> disabledProperties;

        [ImportingConstructor]
        public ConfigurationProperties(ICremaHost cremaHost, [ImportMany] IEnumerable<IConfigurationPropertyProvider> providers)
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
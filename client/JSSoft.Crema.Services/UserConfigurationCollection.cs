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

using JSSoft.Communication;
using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceHosts;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Data;
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users;
using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    class UserConfigurationCollection
    {
        private readonly ICremaHost cremaHost;
        private readonly IConfigurationPropertyProvider[] propertiesProviders;
        private readonly Dictionary<string, UserConfiguration> configByID = new();

        public UserConfigurationCollection(ICremaHost cremaHost, IConfigurationPropertyProvider[] propertiesProviders)
        {
            this.cremaHost = cremaHost;
            this.propertiesProviders = propertiesProviders;
        }

        public Task AddAsync(string userID, string address)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var propertiesProviders = this.propertiesProviders;
                var configPath = GetConfigPath(userID, address);
                var config = new UserConfiguration(configPath, propertiesProviders);
                this.configByID.Add(userID, config);
            });
        }

        public Task RemoveAsync(string userID)
        {
            return this.Dispatcher.InvokeAsync(() => this.Remove(userID));
        }

        public Task RemoveManyAsync(string[] userIDs)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in userIDs)
                {
                    this.Remove(item);
                }
            });
        }

        public static string GetConfigPath(string userID, string address)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var productName = AppUtility.ProductName;
            var addressName = address.Replace(':', '-');
            return Path.Combine(path, productName, $"{userID}@{addressName}.config");
        }

        public CremaDispatcher Dispatcher => this.cremaHost.Dispatcher;

        private void Remove(string userID)
        {
            var config = this.configByID[userID];
            config.Commit();
            this.configByID.Remove(userID);
        }
    }
}

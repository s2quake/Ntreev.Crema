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
using JSSoft.Crema.ServiceHosts;
using JSSoft.Crema.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace JSSoft.Crema.Tools.Framework
{
    class CremaServiceContext : ClientContextBase
    {
        private readonly CremaServiceHost serviceHost;

        private CremaServiceContext(CremaServiceHost serviceHost)
            : base(serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        public static CremaServiceContext Create(string address)
        {
            var serviceHost = new CremaServiceHost();
            return new CremaServiceContext(serviceHost)
            {
                Host = AddressUtility.GetIPAddress(address),
                Port = AddressUtility.GetPort(address)
            };
        }

        public async Task<DataBaseInfo[]> GetDataBaseInfosAsync()
        {
            return (await this.Service.GetDataBaseInfosAsync()).Value;
        }

        //public async Task<GenerationSet> GetCodeGenerationDataAsync(string dataBaseName, string tags, string filterExpression, string revision)
        //{
        //    return (await this.Service.GetCodeGenerationDataAsync(dataBaseName, tags, filterExpression, revision)).Value;
        //}

        //public async Task<SerializationSet> GetDataGenerationDataAsync(string dataBaseName, string tags, string filterExpression, string revision)
        //{
        //    return (await this.Service.GetDataGenerationDataAsync(dataBaseName, tags, filterExpression, revision)).Value;
        //}

        //public async Task<(GenerationSet, SerializationSet)> GetMetaDataAsync(string dataBaseName, string tags, string filterExpression, string revision)
        //{
        //    var value = (await this.Service.GetMetaDataAsync(dataBaseName, tags, filterExpression, revision));
        //    return (value.Value1, value.Value2);
        //}

        //public async Task ResetDataAsync(string dataBaseName)
        //{
        //    await this.Service.ResetDataAsync(dataBaseName);
        //}

        //public async Task<string> GetRevisionAsync(string dataBaseName)
        //{
        //    return (await this.Service.GetRevisionAsync(dataBaseName)).Value;
        //}

        public ICremaHostService Service => this.serviceHost.Service;
    }
}

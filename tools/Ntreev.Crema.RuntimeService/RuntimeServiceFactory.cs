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

using Ntreev.Crema.RuntimeService.CremaHostService;
using Ntreev.Crema.RuntimeService.ServiceClient;
using Ntreev.Crema.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Ntreev.Crema.RuntimeService
{
    class RuntimeServiceFactory : ICremaHostServiceCallback
    {
        private static readonly RuntimeServiceFactory empty = new RuntimeServiceFactory();

        public static async Task<RuntimeServiceInternalClient> CreateServiceClientAsync(string address)
        {
            var binding = CreateBinding(TimeSpan.MaxValue);
            var ip = AddressUtility.GetIPAddress(address);
            var port = await GetServicePortAsync(address);
            var endPointAddress = new EndpointAddress($"net.tcp://{ip}:{port}/RuntimeService");

            return new RuntimeServiceInternalClient(binding, endPointAddress);
        }

        private static async Task<int> GetServicePortAsync(string address)
        {
            var binding = CreateBinding(TimeSpan.MaxValue);
            var endPointAddress = new EndpointAddress($"net.tcp://{AddressUtility.ConnectionAddress(address)}/CremaHostService");
            var instanceContext = new InstanceContext(empty);
            var serviceClient = new CremaHostServiceClient(instanceContext, binding, endPointAddress);

            try
            {
                var result = await InvokeServiceAsync(() => serviceClient.GetServiceInfos());
                var serviceInfos = result.Value;
                foreach (var item in serviceInfos)
                {
                    if (item.Name == nameof(RuntimeService))
                    {
                        return item.Port;
                    }
                }

                throw new InvalidOperationException();
            }
            finally
            {
                serviceClient.CloseService();
            }
        }

        private static async Task<ResultBase<TResult>> InvokeServiceAsync<TResult>(Func<ResultBase<TResult>> func)
        {
            var result = await Task.Run(func);
            result.Validate();
            return result;
        }

        private static Binding CreateBinding(TimeSpan timeSpan)
        {
            var binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            binding.ReceiveTimeout = TimeSpan.MaxValue;
            binding.MaxBufferPoolSize = long.MaxValue;
            binding.MaxBufferSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;

            return binding;
        }

        #region ICremaHostServiceCallback

        void ICremaHostServiceCallback.OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo)
        {
            //throw new NotImplementedException();
        }

        void ICremaHostServiceCallback.OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}

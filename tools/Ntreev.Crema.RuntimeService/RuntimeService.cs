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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ntreev.Crema.Runtime.Serialization;
using Ntreev.Crema.Runtime.Generation;
using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;

namespace Ntreev.Crema.RuntimeService
{
    [Export(typeof(IRuntimeService))]
    class RuntimeService : IRuntimeService
    {
        public async Task<GenerationSet> GetCodeGenerationDataAsync(string address, string dataBaseName, string tags, string filterExpression, string revision)
        {
            var service = await RuntimeServiceFactory.CreateServiceClientAsync(address);
            service.Open();
            try
            {
                var result = await this.InvokeServiceAsync(() => service.GetCodeGenerationData(dataBaseName, tags, filterExpression, revision));
                return result.Value;
            }
            finally
            {
                service.Close();
            }
        }

        public async Task<SerializationSet> GetDataGenerationDataAsync(string address, string dataBaseName, string tags, string filterExpression, string revision)
        {
            var service = await RuntimeServiceFactory.CreateServiceClientAsync(address);
            service.Open();
            try
            {
                var result = await this.InvokeServiceAsync(() => service.GetDataGenerationData(dataBaseName, tags, filterExpression, revision));
                return result.Value;
            }
            finally
            {
                service.Close();
            }
        }

        public async Task<Tuple<GenerationSet, SerializationSet>> GetMetaDataAsync(string address, string dataBaseName, string tags, string filterExpression, string revision)
        {
            var service = await RuntimeServiceFactory.CreateServiceClientAsync(address);
            service.Open();
            try
            {
                var result = await this.InvokeServiceAsync(() => service.GetMetaData(dataBaseName, tags, filterExpression, revision));
                return new Tuple<GenerationSet, SerializationSet>(result.Value1, result.Value2);
            }
            finally
            {
                service.Close();
            }
        }

        public async Task ResetDataAsync(string address, string dataBaseName)
        {
            var service = await RuntimeServiceFactory.CreateServiceClientAsync(address);
            service.Open();
            try
            {
                await this.InvokeServiceAsync(() => service.ResetData(dataBaseName));
            }
            finally
            {
                service.Close();
            }
        }

        public async Task<string> GetRevisionAsync(string address, string dataBaseName)
        {
            var service = await RuntimeServiceFactory.CreateServiceClientAsync(address);
            service.Open();
            try
            {
                var result = await this.InvokeServiceAsync(() => service.GetRevision(dataBaseName));
                return result.Value;
            }
            finally
            {
                service.Close();
            }
        }

        public async Task<ResultBase<TResult>> InvokeServiceAsync<TResult>(Func<ResultBase<TResult>> func)
        {
            var result = await Task.Run(func);
            result.Validate();
            return result;
        }

        public async Task<ResultBase<T1, T2>> InvokeServiceAsync<T1, T2>(Func<ResultBase<T1, T2>> func)
        {
            var result = await Task.Run(func);
            result.Validate();
            return result;
        }

        public async Task<ResultBase> InvokeServiceAsync(Func<ResultBase> func)
        {
            var result = await Task.Run(func);
            result.Validate();
            return result;
        }
    }
}

﻿// Released under the MIT License.
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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.Runtime.Generation;
using JSSoft.Crema.Data;

namespace JSSoft.Crema.RuntimeService
{
    [Export(typeof(IRuntimeService))]
    class RuntimeService : IRuntimeService
    {
        public async Task<GenerationSet> GetCodeGenerationDataAsync(string address, string dataBaseName, string tags, string filterExpression, string revision)
        {
            var serviceContext = RuntimeServiceContext.Create(address);
            var token = Guid.Empty;
            try
            {
                token = await serviceContext.OpenAsync();
                return await serviceContext.GetCodeGenerationDataAsync(dataBaseName, tags, filterExpression, revision);
            }
            finally
            {
                await serviceContext.CloseAsync(token, 0);
            }
        }

        public async Task<SerializationSet> GetDataGenerationDataAsync(string address, string dataBaseName, string tags, string filterExpression, string revision)
        {
            var serviceContext = RuntimeServiceContext.Create(address);
            var token = Guid.Empty;
            try
            {
                token = await serviceContext.OpenAsync();
                return await serviceContext.GetDataGenerationDataAsync(dataBaseName, tags, filterExpression, revision);
            }
            finally
            {
                await serviceContext.CloseAsync(token, 0);
            }
        }

        public async Task<(GenerationSet, SerializationSet)> GetMetaDataAsync(string address, string dataBaseName, string tags, string filterExpression, string revision)
        {
            var serviceContext = RuntimeServiceContext.Create(address);
            var token = Guid.Empty;
            try
            {
                token = await serviceContext.OpenAsync();
                return await serviceContext.GetMetaDataAsync(dataBaseName, tags, filterExpression, revision);
            }
            finally
            {
                await serviceContext.CloseAsync(token, 0);
            }
        }

        public async Task ResetDataAsync(string address, string dataBaseName)
        {
            var serviceContext = RuntimeServiceContext.Create(address);
            var token = Guid.Empty;
            try
            {
                token = await serviceContext.OpenAsync();
                await serviceContext.ResetDataAsync(dataBaseName);
            }
            finally
            {
                await serviceContext.CloseAsync(token, 0);
            }
        }

        public async Task<string> GetRevisionAsync(string address, string dataBaseName)
        {
            var serviceContext = RuntimeServiceContext.Create(address);
            var token = Guid.Empty;
            try
            {
                token = await serviceContext.OpenAsync();
                return await serviceContext.GetRevisionAsync(dataBaseName);
            }
            finally
            {
                await serviceContext.CloseAsync(token, 0);
            }
        }
    }
}

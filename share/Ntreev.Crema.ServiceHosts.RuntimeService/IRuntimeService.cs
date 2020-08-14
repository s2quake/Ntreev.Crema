﻿//Released under the MIT License.
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

using Ntreev.Crema.Runtime.Generation;
using Ntreev.Crema.Runtime.Serialization;
using Ntreev.Crema.ServiceModel;
using System.Threading.Tasks;

namespace Ntreev.Crema.ServiceHosts.RuntimeService
{
    public interface IRuntimeService
    {
        Task<ResultBase<GenerationSet>> GetCodeGenerationDataAsync(string dataBaseName, string tags, string filterExpression, string revision);

        Task<ResultBase<SerializationSet>> GetDataGenerationDataAsync(string dataBaseName, string tags, string filterExpression, string revision);

        Task<ResultBase<GenerationSet, SerializationSet>> GetMetaDataAsync(string dataBaseName, string tags, string filterExpression, string revision);

        Task<ResultBase> ResetDataAsync(string dataBaseName);

        Task<ResultBase<string>> GetRevisionAsync(string dataBaseName);
    }
}
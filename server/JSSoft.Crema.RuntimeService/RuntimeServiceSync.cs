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

using JSSoft.Crema.Runtime.Generation;
using JSSoft.Crema.Runtime.Serialization;
using JSSoft.Crema.ServiceModel;
using System.Threading.Tasks;

namespace JSSoft.Crema.RuntimeService
{
    partial class RuntimeService
    {
        public ResultBase<GenerationSet> GetCodeGenerationData(string dataBaseName, string tags, string filterExpression, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetCodeGenerationDataAsync(dataBaseName, tags, filterExpression, revision)));
        }

        public ResultBase<SerializationSet> GetDataGenerationData(string dataBaseName, string tags, string filterExpression, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetDataGenerationDataAsync(dataBaseName, tags, filterExpression, revision)));
        }

        public ResultBase<GenerationSet, SerializationSet> GetMetaData(string dataBaseName, string tags, string filterExpression, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetMetaDataAsync(dataBaseName, tags, filterExpression, revision)));
        }

        public ResultBase ResetData(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.ResetDataAsync(dataBaseName)));
        }

        public ResultBase<string> GetRevision(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.GetRevisionAsync(dataBaseName)));
        }

        private T InvokeTask<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}

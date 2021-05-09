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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains.Serializations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace JSSoft.Crema.Services.Domains
{
    class DomainLogger : IDispatcherObject
    {
        public const string HeaderItemPath = "header";
        public const string SourceItemPath = "source";
        public const string PostedItemPath = "posted";
        public const string CompletedItemPath = "completed";

        private static readonly XmlWriterSettings writerSettings = new() { OmitXmlDeclaration = true, Indent = true };

        private DomainInfo domainInfo;
        private DomainCompletionItemSerializationInfo currentCompletion;
        private readonly List<DomainCompletionItemSerializationInfo> completionList = new();

        public DomainLogger(Domain domain)
        {
            this.domainInfo = domain.DomainInfo;
            this.completionList = new List<DomainCompletionItemSerializationInfo>();
            this.Dispatcher = new CremaDispatcher(this);
        }

        public override string ToString()
        {
            return $"{this.domainInfo.CategoryPath}{this.domainInfo.DomainID}";
        }

        public void Dispose()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
        }

        public async Task DisposeAsync()
        {
            await this.Dispatcher.DisposeAsync();
            this.Dispatcher = null;
        }

        public void Complete(long id, DomainActionBase action)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.currentCompletion = new DomainCompletionItemSerializationInfo(id, action.UserID, action.AcceptTime, action.GetType());
                this.completionList.Add(this.currentCompletion);
                this.CompletionID = id;
            });
        }

        public long ID { get; set; }

        public long PostID { get; set; }

        public long CompletionID { get; set; }

        public CremaDispatcher Dispatcher { get; private set; }

        public IReadOnlyList<DomainCompletionItemSerializationInfo> CompletionList => this.completionList;

        internal DomainCompletionItemSerializationInfo CurrentCompletion { get => currentCompletion; set => currentCompletion = value; }
    }
}

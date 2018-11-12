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

using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Domains.Actions;
using Ntreev.Crema.Services.Domains.Serializations;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Ntreev.Crema.Services.Domains
{
    class DomainLogger : IDispatcherObject
    {
        public const string HeaderItemPath = "header";
        public const string SourceItemPath = "source";
        public const string PostedItemPath = "posted";
        public const string CompletedItemPath = "completed";

        private static readonly XmlWriterSettings writerSettings = new XmlWriterSettings() { OmitXmlDeclaration = true, Indent = true };

        //private readonly string basePath;

        private readonly string completedPath;
        private DomainInfo domainInfo;

        private DomainCompletionItemSerializationInfo currentCompletion;
        private readonly List<DomainCompletionItemSerializationInfo> completionList = new List<DomainCompletionItemSerializationInfo>();

        public DomainLogger(Domain domain)
        {
            this.domainInfo = domain.DomainInfo;

            //this.basePath = Path.Combine(AppUtility.UserAppDataPath, "domains", $"{domain.CremaHost.Address.Replace(':', '_')}_{domain.CremaHost.UserID}", domain.DataBaseID.ToString(), domain.Name);
            //DirectoryUtility.Delete(this.basePath);
            //Directory.CreateDirectory(this.basePath);

            //this.completedPath = Path.Combine(this.basePath, CompletedItemPath);
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
                //DirectoryUtility.Delete(this.basePath);
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
        }

        public async Task DisposeAsync()
        {
            //DirectoryUtility.Delete(this.basePath);
            await this.Dispatcher.DisposeAsync();
            this.Dispatcher = null;
        }

        public void Complete(long id, DomainActionBase action)
        {
            this.Dispatcher.Invoke(() =>
            {
                //var itemPath = Path.Combine(this.basePath, $"{id}");
                this.currentCompletion = new DomainCompletionItemSerializationInfo(id, action.UserID, action.AcceptTime, action.GetType());
                this.completionList.Add(this.currentCompletion);
                File.AppendAllText(this.completedPath, $"{this.currentCompletion}{Environment.NewLine}");
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

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

        private readonly string basePath;

        //private readonly string headerPath;
        //private readonly string sourcePath;
        //private readonly string postedPath;
        private readonly string completedPath;
        private DomainInfo domainInfo;

        private DomainCompletionItemSerializationInfo currentCompletion;
        private readonly List<DomainCompletionItemSerializationInfo> completionList = new List<DomainCompletionItemSerializationInfo>();

        public DomainLogger(Domain domain)
        {
            //this.serializer = serializer;
            this.domainInfo = domain.DomainInfo;
            
            //this.source = domain.Source;
            //this.Address.Replace(':', '_'), userID, 
            this.basePath = Path.Combine(AppUtility.UserAppDataPath, "domains", $"{domain.CremaHost.Address.Replace(':', '_')}_{domain.CremaHost.UserID}", domain.DataBaseID.ToString(), domain.Name);
            DirectoryUtility.Delete(this.basePath);
            Directory.CreateDirectory(this.basePath);

            //this.headerPath = Path.Combine(this.basePath, HeaderItemPath);
            //this.sourcePath = Path.Combine(this.basePath, SourceItemPath);
            //this.postedPath = Path.Combine(this.basePath, PostedItemPath);
            this.completedPath = Path.Combine(this.basePath, CompletedItemPath);

                //this.serializer.Serialize(this.headerPath, this.domainInfo, ObjectSerializerSettings.Empty);
                //this.serializer.Serialize(this.sourcePath, this.source, ObjectSerializerSettings.Empty);
            this.completionList = new List<DomainCompletionItemSerializationInfo>();
            //this.completedList = new List<DomainCompleteItemSerializationInfo>();
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
                DirectoryUtility.Delete(this.basePath);
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
        }

        public async Task DisposeAsync()
        {
            DirectoryUtility.Delete(this.basePath);
            await this.Dispatcher.DisposeAsync();
            this.Dispatcher = null;
        }

        public Task NewRowAsync(Authentication authentication, DomainRowResultInfo info)
        {
            return this.CompleteAsync(info.ID, new NewRowAction()
            {
                UserID = authentication.ID,
                Rows = info.Rows,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public Task SetRowAsync(Authentication authentication, DomainRowResultInfo info)
        {
            return this.CompleteAsync(info.ID, new SetRowAction()
            {
                UserID = authentication.ID,
                Rows = info.Rows,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public Task RemoveRowAsync(Authentication authentication, DomainRowResultInfo info)
        {
            return this.CompleteAsync(info.ID, new RemoveRowAction()
            {
                UserID = authentication.ID,
                Rows = info.Rows,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        //public Task SetPropertyAsync(Authentication authentication, string propertyName, object value)
        //{
        //    return this.CompleteAsync(new SetPropertyAction()
        //    {
        //        UserID = authentication.ID,
        //        PropertyName = propertyName,
        //        Value = value,
        //        AcceptTime = authentication.SignatureDate.DateTime
        //    });
        //}

        //public Task JoinAsync(Authentication authentication, DomainAccessType accessType)
        //{
        //    return this.CompleteAsync(new JoinAction()
        //    {
        //        UserID = authentication.ID,
        //        AccessType = accessType,
        //        AcceptTime = authentication.SignatureDate.DateTime
        //    });
        //}

        //public Task DisjoinAsync(Authentication authentication, RemoveInfo removeInfo)
        //{
        //    return this.CompleteAsync(new DisjoinAction()
        //    {
        //        UserID = authentication.ID,
        //        RemoveInfo = removeInfo,
        //        AcceptTime = authentication.SignatureDate.DateTime
        //    });
        //}

        //public Task KickAsync(Authentication authentication, string userID, string comment)
        //{
        //    return this.CompleteAsync(new KickAction()
        //    {
        //        UserID = authentication.ID,
        //        TargetID = userID,
        //        Comment = comment,
        //        AcceptTime = authentication.SignatureDate.DateTime
        //    });
        //}

        //public Task SetOwnerAsync(Authentication authentication, string userID)
        //{
        //    return this.CompleteAsync(new SetOwnerAction()
        //    {
        //        UserID = authentication.ID,
        //        TargetID = userID,
        //        AcceptTime = authentication.SignatureDate.DateTime
        //    });
        //}

        public Task CompleteAsync(long id, DomainActionBase action)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var itemPath = Path.Combine(this.basePath, $"{id}");
                this.currentCompletion = new DomainCompletionItemSerializationInfo(id, action.UserID, action.AcceptTime, action.GetType());
                this.completionList.Add(this.currentCompletion);
                //this.serializer.Serialize(itemPath, action, ObjectSerializerSettings.Empty);
                File.AppendAllText(this.completedPath, $"{this.currentCompletion}{Environment.NewLine}");
                this.CompletionID = id;
            });
        }

        public long ID { get; set; }

        public long PostID { get; set; }

        public long CompletionID { get; set; }

        //public bool IsEnabled { get; set; } = true;

        public CremaDispatcher Dispatcher { get; private set; }

        public IReadOnlyList<DomainCompletionItemSerializationInfo> CompletionList => this.completionList;

        internal DomainCompletionItemSerializationInfo CurrentCompletion { get => currentCompletion; set => currentCompletion = value; }

        //public IReadOnlyList<DomainCompleteItemSerializationInfo> CompletedList => this.completedList;

        //public DomainSerializationInfo DomainInfo => this.domainInfo;

        //public object Source => this.source;
    }
}

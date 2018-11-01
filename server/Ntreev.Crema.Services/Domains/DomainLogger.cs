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

        private readonly string headerPath;
        private readonly string sourcePath;
        private readonly string postedPath;
        private readonly string completedPath;

        private readonly IObjectSerializer serializer;
        private readonly DomainSerializationInfo domainInfo;
        private readonly object source;

        private DomainPostItemSerializationInfo currentPost;
        private readonly List<DomainPostItemSerializationInfo> postedList = new List<DomainPostItemSerializationInfo>();
        private readonly List<DomainCompleteItemSerializationInfo> completedList = new List<DomainCompleteItemSerializationInfo>();

        public DomainLogger(IObjectSerializer serializer, Domain domain)
        {
            this.serializer = serializer;
            this.domainInfo = domain.GetSerializationInfo();
            this.source = domain.Source;
            this.basePath = DirectoryUtility.Prepare(domain.Context.BasePath, domain.DataBaseID.ToString(), domain.Name);
            this.headerPath = Path.Combine(this.basePath, HeaderItemPath);
            this.sourcePath = Path.Combine(this.basePath, SourceItemPath);
            this.postedPath = Path.Combine(this.basePath, PostedItemPath);
            this.completedPath = Path.Combine(this.basePath, CompletedItemPath);

            this.serializer.Serialize(this.headerPath, this.domainInfo, ObjectSerializerSettings.Empty);
            this.serializer.Serialize(this.sourcePath, this.source, ObjectSerializerSettings.Empty);
            this.postedList = new List<DomainPostItemSerializationInfo>();
            this.completedList = new List<DomainCompleteItemSerializationInfo>();
            this.Dispatcher = new CremaDispatcher(this);
        }

        public DomainLogger(IObjectSerializer serializer, string basePath)
        {
            this.serializer = serializer;
            this.basePath = basePath;
            this.headerPath = Path.Combine(this.basePath, HeaderItemPath);
            this.sourcePath = Path.Combine(this.basePath, SourceItemPath);
            this.postedPath = Path.Combine(this.basePath, PostedItemPath);
            this.completedPath = Path.Combine(this.basePath, CompletedItemPath);

            this.domainInfo = (DomainSerializationInfo)this.serializer.Deserialize(this.headerPath, typeof(DomainSerializationInfo), ObjectSerializerSettings.Empty);
            this.source = this.serializer.Deserialize(this.sourcePath, Type.GetType(this.domainInfo.SourceType), ObjectSerializerSettings.Empty);

            {
                var items = File.ReadAllLines(this.completedPath);
                this.completedList = new List<DomainCompleteItemSerializationInfo>(items.Length);
                foreach (var item in items)
                {
                    this.completedList.Add(DomainCompleteItemSerializationInfo.Parse(item));
                }
            }

            {
                var items = File.ReadAllLines(this.postedPath);
                this.postedList = new List<DomainPostItemSerializationInfo>(items.Length);
                foreach (var item in items)
                {
                    this.postedList.Add(DomainPostItemSerializationInfo.Parse(item));
                }
            }
            this.Dispatcher = new CremaDispatcher(this);
        }

        public override string ToString()
        {
            return $"{this.domainInfo.DomainInfo.CategoryPath}{this.domainInfo.DomainInfo.DomainID}";
        }

        public Task DisposeAsync(bool delete)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (delete == true)
                {
                    DirectoryUtility.Delete(this.basePath);
                }
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
            });
        }

        public long NewRow(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.Post(new NewRowAction()
            {
                UserID = authentication.ID,
                Rows = rows,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long SetRow(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.Post(new SetRowAction()
            {
                UserID = authentication.ID,
                Rows = rows,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long RemoveRow(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.Post(new RemoveRowAction()
            {
                UserID = authentication.ID,
                Rows = rows,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long SetProperty(Authentication authentication, string propertyName, object value)
        {
            return this.Post(new SetPropertyAction()
            {
                UserID = authentication.ID,
                PropertyName = propertyName,
                Value = value,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long Join(Authentication authentication, DomainAccessType accessType)
        {
            return this.Post(new EnterAction()
            {
                UserID = authentication.ID,
                AccessType = accessType,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long Disjoin(Authentication authentication, RemoveInfo removeInfo)
        {
            return this.Post(new LeaveAction()
            {
                UserID = authentication.ID,
                RemoveInfo = removeInfo,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long Kick(Authentication authentication, string userID, string comment)
        {
            return this.Post(new KickAction()
            {
                UserID = authentication.ID,
                TargetID = userID,
                Comment = comment,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long SetOwner(Authentication authentication, string userID)
        {
            return this.Post(new SetOwnerAction()
            {
                UserID = authentication.ID,
                TargetID = userID,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long BeginUserEdit(Authentication authentication, DomainLocationInfo location)
        {
            return this.Post(new BeginUserEditAction()
            {
                UserID = authentication.ID,
                Location = location,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long EndUserEdit(Authentication authentication)
        {
            return this.Post(new EndUserEditAction()
            {
                UserID = authentication.ID,
                AcceptTime = authentication.SignatureDate.DateTime
            });
        }

        public long Delete(Authentication authentication)
        {
            return this.Post(new DeleteAction()
            {
                UserID = authentication.ID,
                AcceptTime = authentication.SignatureDate.DateTime
            }); 
        }

        public long Post(DomainActionBase action)
        {
            this.Dispatcher.VerifyAccess();
            if (this.IsEnabled == false)
                return this.ID;

            var id = this.ID++;
            var itemPath = Path.Combine(this.basePath, $"{id}");
            this.currentPost = new DomainPostItemSerializationInfo(id, action.UserID, action.GetType());
            this.postedList.Add(this.currentPost);
            this.serializer.Serialize(itemPath, action, ObjectSerializerSettings.Empty);
            File.AppendAllText(this.postedPath, $"{this.currentPost}{Environment.NewLine}");
            this.PostID = id;
            return id;
        }

        public void Complete(long id, Domain domain)
        {
            domain.Dispatcher.VerifyAccess();
            if (this.IsEnabled == false)
                return;

            this.completedList.Add(new DomainCompleteItemSerializationInfo(id));
            File.AppendAllText(this.completedPath, $"{new DomainCompleteItemSerializationInfo(id)}{Environment.NewLine}");
            this.CompletionID = id;
        }

        //public Task CompleteAsync(long id)
        //{
        //    return this.Dispatcher.InvokeAsync(() => this.Complete(id));
        //}

        public long ID { get; set; }

        public long PostID { get; set; } = -1;

        public long CompletionID { get; set; } = -1;

        public bool IsEnabled { get; set; } = true;

        public CremaDispatcher Dispatcher { get; private set; }

        public IReadOnlyList<DomainPostItemSerializationInfo> PostedList => this.postedList;

        public IReadOnlyList<DomainCompleteItemSerializationInfo> CompletedList => this.completedList;

        public DomainSerializationInfo DomainInfo => this.domainInfo;

        public object Source => this.source;
    }
}

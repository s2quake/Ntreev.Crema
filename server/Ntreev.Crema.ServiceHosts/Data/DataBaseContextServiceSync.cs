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
using Ntreev.Crema.Services;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.Data.Xml;
using Ntreev.Crema.Data;
using System.Collections.Generic;
using Ntreev.Library.Linq;
using System.Collections.Specialized;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts.Data
{
    partial class DataBaseContextService
    {
        public ResultBase DefinitionType(LogInfo[] param1)
        {
            return this.InvokeTask(Task.Run(() => this.DefinitionTypeAsync(param1)));
        }

        public ResultBase<DataBaseCollectionMetaData> Subscribe(Guid authenticationToken)
        {
            return this.InvokeTask(Task.Run(() => this.SubscribeAsync(authenticationToken)));
        }

        public ResultBase Unsubscribe()
        {
            return this.InvokeTask(Task.Run(() => this.UnsubscribeAsync()));
        }

        public ResultBase SetPublic(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.SetPublicAsync(dataBaseName)));
        }

        public ResultBase<AccessInfo> SetPrivate(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.SetPrivateAsync(dataBaseName)));
        }

        public ResultBase<AccessMemberInfo> AddAccessMember(string dataBaseName, string memberID, AccessType accessType)
        {
            return this.InvokeTask(Task.Run(() => this.AddAccessMemberAsync(dataBaseName, memberID, accessType)));
        }

        public ResultBase<AccessMemberInfo> SetAccessMember(string dataBaseName, string memberID, AccessType accessType)
        {
            return this.InvokeTask(Task.Run(() => this.SetAccessMemberAsync(dataBaseName, memberID, accessType)));
        }

        public ResultBase RemoveAccessMember(string dataBaseName, string memberID)
        {
            return this.InvokeTask(Task.Run(() => this.RemoveAccessMemberAsync(dataBaseName, memberID)));
        }

        public ResultBase<LockInfo> Lock(string dataBaseName, string comment)
        {
            return this.InvokeTask(Task.Run(() => this.LockAsync(dataBaseName, comment)));
        }

        public ResultBase Unlock(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.UnlockAsync(dataBaseName)));
        }

        public ResultBase Load(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.LoadAsync(dataBaseName)));
        }

        public ResultBase Unload(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.UnloadAsync(dataBaseName)));
        }

        public ResultBase<DataBaseInfo> Create(string dataBaseName, string comment)
        {
            return this.InvokeTask(Task.Run(() => this.CreateAsync(dataBaseName, comment)));
        }

        public ResultBase<DataBaseInfo> Copy(string dataBaseName, string newDataBaseName, string comment, bool force)
        {
            return this.InvokeTask(Task.Run(() => this.CopyAsync(dataBaseName, newDataBaseName, comment, force)));
        }

        public ResultBase Rename(string dataBaseName, string newDataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.RenameAsync(dataBaseName, newDataBaseName)));
        }

        public ResultBase Delete(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.DeleteAsync(dataBaseName)));
        }

        public ResultBase<LogInfo[]> GetLog(string dataBaseName, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetLogAsync(dataBaseName, revision)));
        }

        public ResultBase<DataBaseInfo> Revert(string dataBaseName, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.RevertAsync(dataBaseName, revision)));
        }

        public ResultBase<Guid> BeginTransaction(string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.BeginTransactionAsync(dataBaseName)));
        }

        public ResultBase EndTransaction(Guid transactionID)
        {
            return this.InvokeTask(Task.Run(() => this.EndTransactionAsync(transactionID)));
        }

        public ResultBase<DataBaseMetaData> CancelTransaction(Guid transactionID)
        {
            return this.InvokeTask(Task.Run(() => this.CancelTransactionAsync(transactionID)));
        }

        public bool IsAlive()
        {
            return this.InvokeTask(Task.Run(() => this.IsAliveAsync()));
        }

        private T InvokeTask<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
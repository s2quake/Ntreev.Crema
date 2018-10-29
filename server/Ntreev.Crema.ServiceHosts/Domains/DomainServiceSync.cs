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
using System.ServiceModel;
using System.Text;
using System.Threading;
using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts.Domains
{
    partial class DomainService
    {
        public ResultBase<DomainContextMetaData> Subscribe(Guid authenticationToken)
        {
            return this.InvokeTask(Task.Run(() => this.SubscribeAsync(authenticationToken)));
        }

        public ResultBase Unsubscribe()
        {
            return this.InvokeTask(Task.Run(() => this.UnsubscribeAsync()));
        }

        public ResultBase<DomainMetaData[]> GetMetaData(Guid dataBaseID)
        {
            return this.InvokeTask(Task.Run(() => this.GetMetaDataAsync(dataBaseID)));
        }

        public ResultBase Enter(Guid domainID, DomainAccessType accessType)
        {
            return this.InvokeTask(Task.Run(() => this.EnterAsync(domainID, accessType)));
        }

        public ResultBase Leave(Guid domainID)
        {
            return this.InvokeTask(Task.Run(() => this.LeaveAsync(domainID)));
        }

        public ResultBase SetUserLocation(Guid domainID, DomainLocationInfo location)
        {
            return this.InvokeTask(Task.Run(() => this.SetUserLocationAsync(domainID, location)));
        }

        public ResultBase<DomainRowInfo[]> NewRow(Guid domainID, DomainRowInfo[] rows)
        {
            return this.InvokeTask(Task.Run(() => this.NewRowAsync(domainID, rows)));
        }

        public ResultBase<DomainRowInfo[]> RemoveRow(Guid domainID, DomainRowInfo[] rows)
        {
            return this.InvokeTask(Task.Run(() => this.RemoveRowAsync(domainID, rows)));
        }

        public ResultBase<DomainRowInfo[]> SetRow(Guid domainID, DomainRowInfo[] rows)
        {
            return this.InvokeTask(Task.Run(() => this.SetRowAsync(domainID, rows)));
        }

        public ResultBase SetProperty(Guid domainID, string propertyName, object value)
        {
            return this.InvokeTask(Task.Run(() => this.SetPropertyAsync(domainID, propertyName, value)));
        }

        public ResultBase BeginUserEdit(Guid domainID, DomainLocationInfo location)
        {
            return this.InvokeTask(Task.Run(() => this.BeginUserEditAsync(domainID, location)));
        }

        public ResultBase EndUserEdit(Guid domainID)
        {
            return this.InvokeTask(Task.Run(() => this.EndUserEditAsync(domainID)));
        }

        public ResultBase Kick(Guid domainID, string userID, string comment)
        {
            return this.InvokeTask(Task.Run(() => this.KickAsync(domainID, userID, comment)));
        }

        public ResultBase SetOwner(Guid domainID, string userID)
        {
            return this.InvokeTask(Task.Run(() => this.SetOwnerAsync(domainID, userID)));
        }

        public ResultBase<object> DeleteDomain(Guid domainID, bool force)
        {
            return this.InvokeTask(Task.Run(() => this.DeleteDomainAsync(domainID, force)));
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
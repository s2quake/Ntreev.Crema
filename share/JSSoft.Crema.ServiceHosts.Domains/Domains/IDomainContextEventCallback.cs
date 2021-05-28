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

using JSSoft.Communication;
using JSSoft.Crema.ServiceModel;
using System;

namespace JSSoft.Crema.ServiceHosts.Domains
{
    public interface IDomainContextEventCallback
    {
        [OperationContract]
        void OnDomainsCreated(CallbackInfo callbackInfo, DomainMetaData[] metaDatas);

        [OperationContract]
        void OnDomainsDeleted(CallbackInfo callbackInfo, Guid[] domainIDs, bool[] IsCanceleds, object[] results);

        [OperationContract]
        void OnDomainInfoChanged(CallbackInfo callbackInfo, Guid domainID, DomainInfo domainInfo);

        [OperationContract]
        void OnDomainStateChanged(CallbackInfo callbackInfo, Guid domainID, DomainState domainState);

        [OperationContract]
        void OnUserAdded(CallbackInfo callbackInfo, Guid domainID, DomainUserInfo domainUserInfo, DomainUserState domainUserState, byte[] data, Guid taskID);

        [OperationContract]
        void OnUserRemoved(CallbackInfo callbackInfo, Guid domainID, string userID, string ownerID, RemoveInfo removeInfo, Guid taskID);

        [OperationContract]
        void OnUserLocationChanged(CallbackInfo callbackInfo, Guid domainID, DomainLocationInfo domainLocationInfo);

        [OperationContract]
        void OnUserStateChanged(CallbackInfo callbackInfo, Guid domainID, DomainUserState domainUserState);

        [OperationContract]
        void OnUserEditBegun(CallbackInfo callbackInfo, Guid domainID, DomainLocationInfo domainLocationInfo, Guid taskID);

        [OperationContract]
        void OnUserEditEnded(CallbackInfo callbackInfo, Guid domainID, Guid taskID);

        [OperationContract]
        void OnOwnerChanged(CallbackInfo callbackInfo, Guid domainID, string ownerID, Guid taskID);

        [OperationContract]
        void OnRowAdded(CallbackInfo callbackInfo, Guid domainID, DomainRowInfo[] rows, Guid taskID);

        [OperationContract]
        void OnRowChanged(CallbackInfo callbackInfo, Guid domainID, DomainRowInfo[] rows, Guid taskID);

        [OperationContract]
        void OnRowRemoved(CallbackInfo callbackInfo, Guid domainID, DomainRowInfo[] rows, Guid taskID);

        [OperationContract]
        void OnPropertyChanged(CallbackInfo callbackInfo, Guid domainID, string propertyName, object value, Guid taskID);

        [OperationContract]
        void OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs);
    }
}

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
using JSSoft.Communication;
using JSSoft.Crema.ServiceModel;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Domains
{
    [ServiceContract(PerPeer = true)]
    public interface IDomainContextService
    {
        [OperationContract]
        Task<ResultBase<DomainContextMetaData>> SubscribeAsync(Guid token);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync(Guid token);

        [OperationContract]
        Task<ResultBase<DomainMetaData[]>> GetMetaDataAsync(Guid dataBaseID);

        [OperationContract]
        Task<ResultBase> SetUserLocationAsync(Guid authenticationToken, Guid domainID, DomainLocationInfo location);

        [OperationContract]
        Task<ResultBase<DomainRowInfo[]>> NewRowAsync(Guid authenticationToken, Guid domainID, DomainRowInfo[] rows);

        [OperationContract]
        Task<ResultBase<DomainRowInfo[]>> RemoveRowAsync(Guid authenticationToken, Guid domainID, DomainRowInfo[] rows);

        [OperationContract]
        Task<ResultBase<DomainRowInfo[]>> SetRowAsync(Guid authenticationToken, Guid domainID, DomainRowInfo[] rows);

        [OperationContract]
        Task<ResultBase> SetPropertyAsync(Guid authenticationToken, Guid domainID, string propertyName, object value);

        [OperationContract]
        Task<ResultBase> BeginUserEditAsync(Guid authenticationToken, Guid domainID, DomainLocationInfo location);

        [OperationContract]
        Task<ResultBase> EndUserEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase> KickAsync(Guid authenticationToken, Guid domainID, string userID, string comment);

        [OperationContract]
        Task<ResultBase> SetOwnerAsync(Guid authenticationToken, Guid domainID, string userID);

        [OperationContract]
        Task<ResultBase<object>> DeleteDomainAsync(Guid authenticationToken, Guid domainID, bool force);
    }
}

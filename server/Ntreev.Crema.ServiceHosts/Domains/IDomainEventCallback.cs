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
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Crema.ServiceModel;
using System.Threading.Tasks;
using Ntreev.Library;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.ServiceHosts.Domains
{
    [ServiceKnownType(typeof(DBNull))]
    public interface IDomainEventCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo);

        [OperationContract(IsOneWay = true)]
        void OnDomainsCreated(SignatureDate signatureDate, DomainMetaData[] metaDatas);

        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(TableInfo))]
        [ServiceKnownType(typeof(TableInfo[]))]
        [ServiceKnownType(typeof(TypeInfo))]
        [ServiceKnownType(typeof(TypeInfo[]))]
        void OnDomainsDeleted(SignatureDate signatureDate, Guid[] domainIDs, bool[] IsCanceleds, object[] results);

        [OperationContract(IsOneWay = true)]
        void OnDomainInfoChanged(SignatureDate signatureDate, Guid domainID, DomainInfo domainInfo);

        [OperationContract(IsOneWay = true)]
        void OnDomainStateChanged(SignatureDate signatureDate, Guid domainID, DomainState domainState);

        [OperationContract(IsOneWay = true)]
        void OnUserAdded(SignatureDate signatureDate, Guid domainID, DomainUserInfo domainUserInfo, DomainUserState domainUserState, byte[] data, long id);

        [OperationContract(IsOneWay = true)]
        void OnUserRemoved(SignatureDate signatureDate, Guid domainID, string userID, string ownerID, RemoveInfo removeInfo, long id);

        [OperationContract(IsOneWay = true)]
        void OnUserLocationChanged(SignatureDate signatureDate, Guid domainID, DomainLocationInfo domainLocationInfo);

        [OperationContract(IsOneWay = true)]
        void OnUserStateChanged(SignatureDate signatureDate, Guid domainID, DomainUserState domainUserState);

        [OperationContract(IsOneWay = true)]
        void OnUserEditBegun(SignatureDate signatureDate, Guid domainID, DomainLocationInfo domainLocationInfo, long id);

        [OperationContract(IsOneWay = true)]
        void OnUserEditEnded(SignatureDate signatureDate, Guid domainID, long id);

        [OperationContract(IsOneWay = true)]
        void OnOwnerChanged(SignatureDate signatureDate, Guid domainID, string ownerID, long id);

        [OperationContract(IsOneWay = true)]
        void OnRowAdded(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows, long id);

        [OperationContract(IsOneWay = true)]
        void OnRowChanged(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows, long id);

        [OperationContract(IsOneWay = true)]
        void OnRowRemoved(SignatureDate signatureDate, Guid domainID, DomainRowInfo[] rows, long id);

        [OperationContract(IsOneWay = true)]
        void OnPropertyChanged(SignatureDate signatureDate, Guid domainID, string propertyName, object value, long id);
    }
}

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
using Ntreev.Crema.Data.Xml.Schema;

namespace Ntreev.Crema.ServiceHosts.Domains
{
    [ServiceContract(Namespace = CremaService.Namespace, SessionMode = SessionMode.Required, CallbackContract = typeof(IDomainEventCallback))]
    [ServiceKnownType(typeof(DBNull))]
    public interface IDomainService
    {
        [OperationContract]
        Task<ResultBase<DomainContextMetaData>> SubscribeAsync(Guid authenticationToken);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync();

        [OperationContract]
        Task<ResultBase<DomainContextMetaData>> GetMetaDataAsync();

        [OperationContract]
        Task<ResultBase> SetUserLocationAsync(Guid domainID, DomainLocationInfo location);

        [OperationContract]
        Task<ResultBase<DomainRowInfo[]>> NewRowAsync(Guid domainID, DomainRowInfo[] rows);

        [OperationContract]
        Task<ResultBase> RemoveRowAsync(Guid domainID, DomainRowInfo[] rows);

        [OperationContract]
        Task<ResultBase<DomainRowInfo[]>> SetRowAsync(Guid domainID, DomainRowInfo[] rows);

        [OperationContract]
        Task<ResultBase> SetPropertyAsync(Guid domainID, string propertyName, object value);

        [OperationContract]
        Task<ResultBase> BeginUserEditAsync(Guid domainID, DomainLocationInfo location);

        [OperationContract]
        Task<ResultBase> EndUserEditAsync(Guid domainID);

        [OperationContract]
        Task<ResultBase<DomainUserInfo>> KickAsync(Guid domainID, string userID, string comment);

        [OperationContract]
        Task<ResultBase> SetOwnerAsync(Guid domainID, string userID);

        [OperationContract]
        Task<ResultBase> DeleteDomainAsync(Guid domainID, bool force);

        [OperationContract]
        bool IsAlive();
    }
}

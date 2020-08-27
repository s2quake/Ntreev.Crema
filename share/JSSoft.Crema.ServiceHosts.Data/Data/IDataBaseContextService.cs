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
using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Data
{
    [ServiceContract(PerPeer = true)]
    public interface IDataBaseContextService
    {
        [OperationContract]
        Task<ResultBase<DataBaseContextMetaData>> SubscribeAsync(Guid authenticationToken);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync();

        [OperationContract]
        Task<ResultBase<CremaDataSet>> GetDataSetAsync(string dataBaseName, DataSetType dataSetType, string filterExpression, string revision);

        [OperationContract]
        Task<ResultBase> SetPublicAsync(string dataBaseName);

        [OperationContract]
        Task<ResultBase<AccessInfo>> SetPrivateAsync(string dataBaseName);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> AddAccessMemberAsync(string dataBaseName, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> SetAccessMemberAsync(string dataBaseName, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase> RemoveAccessMemberAsync(string dataBaseName, string memberID);

        [OperationContract]
        Task<ResultBase<LockInfo>> LockAsync(string dataBaseName, string comment);

        [OperationContract]
        Task<ResultBase> UnlockAsync(string dataBaseName);

        [OperationContract]
        Task<ResultBase> LoadAsync(string dataBaseName);

        [OperationContract]
        Task<ResultBase> UnloadAsync(string dataBaseName);

        [OperationContract]
        Task<ResultBase<DataBaseInfo>> CreateAsync(string dataBaseName, string comment);

        [OperationContract]
        Task<ResultBase<DataBaseInfo>> CopyAsync(string dataBaseName, string newDataBaseName, string comment, bool force);

        [OperationContract]
        Task<ResultBase> RenameAsync(string dataBaseName, string newDataBaseName);

        [OperationContract]
        Task<ResultBase> DeleteAsync(string dataBaseName);

        [OperationContract]
        Task<ResultBase<LogInfo[]>> GetLogAsync(string dataBaseName, string revision);

        [OperationContract]
        Task<ResultBase<DataBaseInfo>> RevertAsync(string dataBaseName, string revision);

        [OperationContract]
        Task<ResultBase<Guid>> BeginTransactionAsync(string dataBaseName);

        [OperationContract]
        Task<ResultBase> EndTransactionAsync(Guid transactionID);

        [OperationContract]
        Task<ResultBase<DataBaseMetaData>> CancelTransactionAsync(Guid transactionID);

        [OperationContract]
        Task<bool> IsAliveAsync();
    }
}
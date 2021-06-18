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
    public interface IDataBaseService
    {
        [OperationContract]
        Task<ResultBase<DataBaseMetaData>> SubscribeAsync(Guid token, string dataBaseName);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync(Guid token);

        [OperationContract]
        Task<ResultBase<DataBaseMetaData>> GetMetaDataAsync();

        [OperationContract]
        Task<ResultBase> ImportDataSetAsync(Guid authenticationToken, CremaDataSet dataSet, string comment);

        [OperationContract]
        Task<ResultBase> NewTableCategoryAsync(Guid authenticationToken, string categoryPath);

        [OperationContract]
        Task<ResultBase<CremaDataSet>> GetTableItemDataSetAsync(Guid authenticationToken, string itemPath, string revision);

        [OperationContract]
        Task<ResultBase> RenameTableItemAsync(Guid authenticationToken, string itemPath, string newName);

        [OperationContract]
        Task<ResultBase> MoveTableItemAsync(Guid authenticationToken, string itemPath, string parentPath);

        [OperationContract]
        Task<ResultBase> DeleteTableItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase> SetPublicTableItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<AccessInfo>> SetPrivateTableItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> AddAccessMemberTableItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> SetAccessMemberTableItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase> RemoveAccessMemberTableItemAsync(Guid authenticationToken, string itemPath, string memberID);

        [OperationContract]
        Task<ResultBase<LockInfo>> LockTableItemAsync(Guid authenticationToken, string itemPath, string comment);

        [OperationContract]
        Task<ResultBase> UnlockTableItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<LogInfo[]>> GetTableItemLogAsync(Guid authenticationToken, string itemPath, string revision);

        [OperationContract]
        Task<ResultBase<FindResultInfo[]>> FindTableItemAsync(Guid authenticationToken, string itemPath, string text, FindOptions options);

        [OperationContract]
        Task<ResultBase<TableInfo[]>> CopyTableAsync(Guid authenticationToken, string tableName, string newTableName, string categoryPath, bool copyXml);

        [OperationContract]
        Task<ResultBase<TableInfo[]>> InheritTableAsync(Guid authenticationToken, string tableName, string newTableName, string categoryPath, bool copyXml);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> EnterTableContentEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> LeaveTableContentEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginTableContentEditAsync(Guid authenticationToken, string tableName);

        [OperationContract]
        Task<ResultBase<TableInfo[]>> EndTableContentEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase> CancelTableContentEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginTableTemplateEditAsync(Guid authenticationToken, string tableName);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginNewTableAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<TableInfo[]>> EndTableTemplateEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase> CancelTableTemplateEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase> NewTypeCategoryAsync(Guid authenticationToken, string categoryPath);

        [OperationContract]
        Task<ResultBase<CremaDataSet>> GetTypeItemDataSetAsync(Guid authenticationToken, string itemPath, string revision);

        [OperationContract]
        Task<ResultBase> RenameTypeItemAsync(Guid authenticationToken, string itemPath, string newName);

        [OperationContract]
        Task<ResultBase> MoveTypeItemAsync(Guid authenticationToken, string itemPath, string parentPath);

        [OperationContract]
        Task<ResultBase> DeleteTypeItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<TypeInfo>> CopyTypeAsync(Guid authenticationToken, string typeName, string newTypeName, string categoryPath);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginTypeTemplateEditAsync(Guid authenticationToken, string typeName);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginNewTypeAsync(Guid authenticationToken, string categoryPath);

        [OperationContract]
        Task<ResultBase<TypeInfo[]>> EndTypeTemplateEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase> CancelTypeTemplateEditAsync(Guid authenticationToken, Guid domainID);

        [OperationContract]
        Task<ResultBase> SetPublicTypeItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<AccessInfo>> SetPrivateTypeItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> AddAccessMemberTypeItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> SetAccessMemberTypeItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase> RemoveAccessMemberTypeItemAsync(Guid authenticationToken, string itemPath, string memberID);

        [OperationContract]
        Task<ResultBase<LockInfo>> LockTypeItemAsync(Guid authenticationToken, string itemPath, string comment);

        [OperationContract]
        Task<ResultBase> UnlockTypeItemAsync(Guid authenticationToken, string itemPath);

        [OperationContract]
        Task<ResultBase<LogInfo[]>> GetTypeItemLogAsync(Guid authenticationToken, string itemPath, string revision);

        [OperationContract]
        Task<ResultBase<FindResultInfo[]>> FindTypeItemAsync(Guid authenticationToken, string itemPath, string text, FindOptions options);
    }
}
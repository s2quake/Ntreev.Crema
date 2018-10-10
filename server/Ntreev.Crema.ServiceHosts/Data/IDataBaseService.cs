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
using Ntreev.Crema.Data;
using Ntreev.Library;

namespace Ntreev.Crema.ServiceHosts.Data
{
    [ServiceContract(Namespace = CremaService.Namespace, SessionMode = SessionMode.Required, CallbackContract = typeof(IDataBaseEventCallback))]
    public interface IDataBaseService
    {
        /// <summary>
        /// 특정 타입의 배열 형식이 메소드의 인자로 설정되지 않으면 반환값으로 사용될때 클라이언트의 코드에서 재사용되지 않고 임시 코드가 생성됨
        /// </summary>
        [OperationContract]
        Task<ResultBase> DefinitionTypeAsync(LogInfo[] param1, FindResultInfo[] param2);

        [OperationContract]
        Task<ResultBase<DataBaseMetaData>> SubscribeAsync(Guid authenticationToken, string dataBaseName);

        [OperationContract]
        Task<ResultBase> UnsubscribeAsync();

        [OperationContract]
        Task<ResultBase<DataBaseMetaData>> GetMetaDataAsync();

        [OperationContract]
        Task<ResultBase<CremaDataSet>> GetDataSetAsync(DataSetType dataSetType, string filterExpression, string revision);

        [OperationContract]
        Task<ResultBase> ImportDataSetAsync(CremaDataSet dataSet, string comment);

        [OperationContract]
        Task<ResultBase> NewTableCategoryAsync(string categoryPath);

        [OperationContract]
        Task<ResultBase<CremaDataSet>> GetTableItemDataSetAsync(string itemPath, string revision);

        [OperationContract]
        Task<ResultBase> RenameTableItemAsync(string itemPath, string newName);

        [OperationContract]
        Task<ResultBase> MoveTableItemAsync(string itemPath, string parentPath);

        [OperationContract]
        Task<ResultBase> DeleteTableItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase> SetPublicTableItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<AccessInfo>> SetPrivateTableItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> AddAccessMemberTableItemAsync(string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> SetAccessMemberTableItemAsync(string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase> RemoveAccessMemberTableItemAsync(string itemPath, string memberID);

        [OperationContract]
        Task<ResultBase<LockInfo>> LockTableItemAsync(string itemPath, string comment);

        [OperationContract]
        Task<ResultBase> UnlockTableItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<LogInfo[]>> GetTableItemLogAsync(string itemPath, string revision);

        [OperationContract]
        Task<ResultBase<FindResultInfo[]>> FindTableItemAsync(string itemPath, string text, FindOptions options);

        [OperationContract]
        Task<ResultBase<TableInfo[]>> CopyTableAsync(string tableName, string newTableName, string categoryPath, bool copyXml);

        [OperationContract]
        Task<ResultBase<TableInfo[]>> InheritTableAsync(string tableName, string newTableName, string categoryPath, bool copyXml);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> EnterTableContentEditAsync(string tableName);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> LeaveTableContentEditAsync(string tableName);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginTableContentEditAsync(string tableName);

        [OperationContract]
        Task<ResultBase<TableInfo[]>> EndTableContentEditAsync(string tableName);

        [OperationContract]
        Task<ResultBase> CancelTableContentEditAsync(string tableName);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginTableTemplateEditAsync(string tableName);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginNewTableAsync(string itemPath);

        [OperationContract(IsInitiating = true)]
        Task<ResultBase<TableInfo[]>> EndTableTemplateEditAsync(Guid domainID);

        [OperationContract]
        Task<ResultBase> CancelTableTemplateEditAsync(Guid domainID);

        [OperationContract]
        Task<ResultBase> NewTypeCategoryAsync(string categoryPath);

        [OperationContract]
        Task<ResultBase<CremaDataSet>> GetTypeItemDataSetAsync(string itemPath, string revision);

        [OperationContract]
        Task<ResultBase> RenameTypeItemAsync(string itemPath, string newName);

        [OperationContract]
        Task<ResultBase> MoveTypeItemAsync(string itemPath, string parentPath);

        [OperationContract]
        Task<ResultBase> DeleteTypeItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<TypeInfo>> CopyTypeAsync(string typeName, string newTypeName, string categoryPath);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginTypeTemplateEditAsync(string typeName);

        [OperationContract]
        Task<ResultBase<DomainMetaData>> BeginNewTypeAsync(string categoryPath);

        [OperationContract]
        Task<ResultBase<TypeInfo[]>> EndTypeTemplateEditAsync(Guid domainID);

        [OperationContract]
        Task<ResultBase> CancelTypeTemplateEditAsync(Guid domainID);

        [OperationContract]
        Task<ResultBase> SetPublicTypeItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<AccessInfo>> SetPrivateTypeItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> AddAccessMemberTypeItemAsync(string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase<AccessMemberInfo>> SetAccessMemberTypeItemAsync(string itemPath, string memberID, AccessType accessType);

        [OperationContract]
        Task<ResultBase> RemoveAccessMemberTypeItemAsync(string itemPath, string memberID);

        [OperationContract]
        Task<ResultBase<LockInfo>> LockTypeItemAsync(string itemPath, string comment);

        [OperationContract]
        Task<ResultBase> UnlockTypeItemAsync(string itemPath);

        [OperationContract]
        Task<ResultBase<LogInfo[]>> GetTypeItemLogAsync(string itemPath, string revision);

        [OperationContract]
        Task<ResultBase<FindResultInfo[]>> FindTypeItemAsync(string itemPath, string text, FindOptions options);

        [OperationContract]
        Task<bool> IsAliveAsync();
    }
}
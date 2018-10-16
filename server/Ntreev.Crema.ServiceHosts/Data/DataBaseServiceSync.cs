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
using System.Windows.Threading;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.Data.Xml;
using Ntreev.Crema.Data;
using System.Collections.Generic;
using Ntreev.Library.Linq;
using Ntreev.Library;
using Ntreev.Library.Serialization;
using System.IO;
using System.Text;

namespace Ntreev.Crema.ServiceHosts.Data
{
    partial class DataBaseService
    {
        public ResultBase DefinitionType(LogInfo[] param1, FindResultInfo[] param2)
        {
            return this.InvokeTask(Task.Run(() => this.DefinitionTypeAsync(param1, param2)));
        }

        public ResultBase<DataBaseMetaData> Subscribe(Guid authenticationToken, string dataBaseName)
        {
            return this.InvokeTask(Task.Run(() => this.SubscribeAsync(authenticationToken, dataBaseName)));
        }

        public ResultBase Unsubscribe()
        {
            return this.InvokeTask(Task.Run(() => this.UnsubscribeAsync()));
        }

        public ResultBase<DataBaseMetaData> GetMetaData()
        {
            return this.InvokeTask(Task.Run(() => this.GetMetaDataAsync()));
        }

        public ResultBase<CremaDataSet> GetDataSet(DataSetType dataSetType, string filterExpression, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetDataSetAsync(dataSetType, filterExpression, revision)));
        }

        public ResultBase ImportDataSet(CremaDataSet dataSet, string comment)
        {
            return this.InvokeTask(Task.Run(() => this.ImportDataSetAsync(dataSet, comment)));
        }

        public ResultBase NewTableCategory(string categoryPath)
        {
            return this.InvokeTask(Task.Run(() => this.NewTableCategoryAsync(categoryPath)));
        }

        public ResultBase<CremaDataSet> GetTableItemDataSet(string itemPath, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetTableItemDataSetAsync(itemPath, revision)));
        }

        public ResultBase RenameTableItem(string itemPath, string newName)
        {
            return this.InvokeTask(Task.Run(() => this.RenameTableItemAsync(itemPath, newName)));
        }

        public ResultBase MoveTableItem(string itemPath, string parentPath)
        {
            return this.InvokeTask(Task.Run(() => this.MoveTableItemAsync(itemPath, parentPath)));
        }

        public ResultBase DeleteTableItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.DeleteTableItemAsync(itemPath)));
        }

        public ResultBase SetPublicTableItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.SetPublicTableItemAsync(itemPath)));
        }

        public ResultBase<AccessInfo> SetPrivateTableItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.SetPrivateTableItemAsync(itemPath)));
        }

        public ResultBase<AccessMemberInfo> AddAccessMemberTableItem(string itemPath, string memberID, AccessType accessType)
        {
            return this.InvokeTask(Task.Run(() => this.AddAccessMemberTableItemAsync(itemPath, memberID, accessType)));
        }

        public ResultBase<AccessMemberInfo> SetAccessMemberTableItem(string itemPath, string memberID, AccessType accessType)
        {
            return this.InvokeTask(Task.Run(() => this.SetAccessMemberTableItemAsync(itemPath, memberID, accessType)));
        }

        public ResultBase RemoveAccessMemberTableItem(string itemPath, string memberID)
        {
            return this.InvokeTask(Task.Run(() => this.RemoveAccessMemberTableItemAsync(itemPath, memberID)));
        }

        public ResultBase<LockInfo> LockTableItem(string itemPath, string comment)
        {
            return this.InvokeTask(Task.Run(() => this.LockTableItemAsync(itemPath, comment)));
        }

        public ResultBase UnlockTableItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.UnlockTableItemAsync(itemPath)));
        }

        public ResultBase<LogInfo[]> GetTableItemLog(string itemPath, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetTableItemLogAsync(itemPath, revision)));
        }

        public ResultBase<FindResultInfo[]> FindTableItem(string itemPath, string text, FindOptions options)
        {
            return this.InvokeTask(Task.Run(() => this.FindTableItemAsync(itemPath, text, options)));
        }

        public ResultBase<TableInfo[]> CopyTable(string tableName, string newTableName, string categoryPath, bool copyXml)
        {
            return this.InvokeTask(Task.Run(() => this.CopyTableAsync(tableName, newTableName, categoryPath, copyXml)));
        }

        public ResultBase<TableInfo[]> InheritTable(string tableName, string newTableName, string categoryPath, bool copyXml)
        {
            return this.InvokeTask(Task.Run(() => this.InheritTableAsync(tableName, newTableName, categoryPath, copyXml)));
        }

        public ResultBase<DomainMetaData> EnterTableContentEdit(string tableName)
        {
            return this.InvokeTask(Task.Run(() => this.EnterTableContentEditAsync(tableName)));
        }

        public ResultBase<DomainMetaData> LeaveTableContentEdit(string tableName)
        {
            return this.InvokeTask(Task.Run(() => this.LeaveTableContentEditAsync(tableName)));
        }

        public ResultBase<DomainMetaData> BeginTableContentEdit(string tableName)
        {
            return this.InvokeTask(Task.Run(() => this.BeginTableContentEditAsync(tableName)));
        }

        public ResultBase<TableInfo[]> EndTableContentEdit(string tableName)
        {
            return this.InvokeTask(Task.Run(() => this.EndTableContentEditAsync(tableName)));
        }

        public ResultBase CancelTableContentEdit(string tableName)
        {
            return this.InvokeTask(Task.Run(() => this.CancelTableContentEditAsync(tableName)));
        }

        public ResultBase<DomainMetaData> BeginTableTemplateEdit(string tableName)
        {
            return this.InvokeTask(Task.Run(() => this.BeginTableTemplateEditAsync(tableName)));
        }

        public ResultBase<DomainMetaData> BeginNewTable(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.BeginNewTableAsync(itemPath)));
        }

        public ResultBase<TableInfo[]> EndTableTemplateEdit(Guid domainID)
        {
            return this.InvokeTask(Task.Run(() => this.EndTableTemplateEditAsync(domainID)));
        }

        public ResultBase CancelTableTemplateEdit(Guid domainID)
        {
            return this.InvokeTask(Task.Run(() => this.CancelTableTemplateEditAsync(domainID)));
        }

        public ResultBase NewTypeCategory(string categoryPath)
        {
            return this.InvokeTask(Task.Run(() => this.NewTypeCategoryAsync(categoryPath)));
        }

        public ResultBase<CremaDataSet> GetTypeItemDataSet(string itemPath, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetTypeItemDataSetAsync(itemPath, revision)));
        }

        public ResultBase RenameTypeItem(string itemPath, string newName)
        {
            return this.InvokeTask(Task.Run(() => this.RenameTypeItemAsync(itemPath, newName)));
        }

        public ResultBase MoveTypeItem(string itemPath, string parentPath)
        {
            return this.InvokeTask(Task.Run(() => this.MoveTypeItemAsync(itemPath, parentPath)));
        }

        public ResultBase DeleteTypeItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.DeleteTypeItemAsync(itemPath)));
        }

        public ResultBase<TypeInfo> CopyType(string typeName, string newTypeName, string categoryPath)
        {
            return this.InvokeTask(Task.Run(() => this.CopyTypeAsync(typeName, newTypeName, categoryPath)));
        }

        public ResultBase<DomainMetaData> BeginTypeTemplateEdit(string typeName)
        {
            return this.InvokeTask(Task.Run(() => this.BeginTypeTemplateEditAsync(typeName)));
        }

        public ResultBase<DomainMetaData> BeginNewType(string categoryPath)
        {
            return this.InvokeTask(Task.Run(() => this.BeginNewTypeAsync(categoryPath)));
        }

        public ResultBase<TypeInfo[]> EndTypeTemplateEdit(Guid domainID)
        {
            return this.InvokeTask(Task.Run(() => this.EndTypeTemplateEditAsync(domainID)));
        }

        public ResultBase CancelTypeTemplateEdit(Guid domainID)
        {
            return this.InvokeTask(Task.Run(() => this.CancelTypeTemplateEditAsync(domainID)));
        }

        public ResultBase SetPublicTypeItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.SetPublicTypeItemAsync(itemPath)));
        }

        public ResultBase<AccessInfo> SetPrivateTypeItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.SetPrivateTypeItemAsync(itemPath)));
        }

        public ResultBase<AccessMemberInfo> AddAccessMemberTypeItem(string itemPath, string memberID, AccessType accessType)
        {
            return this.InvokeTask(Task.Run(() => this.AddAccessMemberTypeItemAsync(itemPath, memberID, accessType)));
        }

        public ResultBase<AccessMemberInfo> SetAccessMemberTypeItem(string itemPath, string memberID, AccessType accessType)
        {
            return this.InvokeTask(Task.Run(() => this.SetAccessMemberTypeItemAsync(itemPath, memberID, accessType)));
        }

        public ResultBase RemoveAccessMemberTypeItem(string itemPath, string memberID)
        {
            return this.InvokeTask(Task.Run(() => this.RemoveAccessMemberTypeItemAsync(itemPath, memberID)));
        }

        public ResultBase<LockInfo> LockTypeItem(string itemPath, string comment)
        {
            return this.InvokeTask(Task.Run(() => this.LockTypeItemAsync(itemPath, comment)));
        }

        public ResultBase UnlockTypeItem(string itemPath)
        {
            return this.InvokeTask(Task.Run(() => this.UnlockTypeItemAsync(itemPath)));
        }

        public ResultBase<LogInfo[]> GetTypeItemLog(string itemPath, string revision)
        {
            return this.InvokeTask(Task.Run(() => this.GetTypeItemLogAsync(itemPath, revision)));
        }

        public ResultBase<FindResultInfo[]> FindTypeItem(string itemPath, string text, FindOptions options)
        {
            return this.InvokeTask(Task.Run(() => this.FindTypeItemAsync(itemPath, text, options)));
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

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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Data
{
    class DataBaseService : CremaServiceItemBase<IDataBaseEventCallback>, IDataBaseService
    {
        private IDataBase dataBase;
        private string dataBaseName;
        private long index = 0;
        private Peer peer;

        public DataBaseService(CremaService service, IDataBaseEventCallback callback)
            : base(service, callback)
        {
            this.UserCollection = this.CremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            this.DomainContext = this.CremaHost.GetService(typeof(IDomainContext)) as IDomainContext;
            this.DataBasesContext = this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;

            this.LogService.Debug($"{nameof(DataBaseService)} Constructor");
        }

        public async Task DisposeAsync()
        {
            if (this.peer != null)
            {
                await this.DetachEventHandlersAsync(this.peer.ID);
                this.peer = null;
            }
        }

        public async Task<ResultBase<DataBaseMetaData>> SubscribeAsync(Guid token, string dataBaseName)
        {
            if (this.peer is not null)
                throw new InvalidOperationException();
            await this.DataBasesContext.Dispatcher.InvokeAsync(() =>
            {
                this.dataBase = this.DataBasesContext[dataBaseName];
                this.dataBaseName = dataBaseName;
            });
            var value = await this.AttachEventHandlersAsync(token);
            this.peer = Peer.GetPeer(token);
            this.LogService.Debug($"[{token}] {nameof(DataBaseService)} {nameof(SubscribeAsync)} : {dataBaseName}");
            return new ResultBase<DataBaseMetaData>()
            {
                Value = value,
                SignatureDate = new SignatureDateProvider($"{token}")
            };
        }

        public async Task<ResultBase> UnsubscribeAsync(Guid token)
        {
            if (this.peer == null)
                throw new InvalidOperationException();
            if (this.peer.ID != token)
                throw new ArgumentException("invalid token", nameof(token));
            await this.DetachEventHandlersAsync(token);
            this.LogService.Debug($"[{token}] {nameof(DataBaseService)} {nameof(UnsubscribeAsync)} : {this.dataBaseName}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider($"{token}")
            };
        }

        public async Task<ResultBase<DataBaseMetaData>> GetMetaDataAsync()
        {
            var result = new ResultBase<DataBaseMetaData>();
            result.Value = await this.dataBase.Dispatcher.InvokeAsync(() => this.dataBase.GetMetaData());
            result.SignatureDate = SignatureDate.Empty;
            return result;
        }

        public async Task<ResultBase> ImportDataSetAsync(Guid authenticationToken, CremaDataSet dataSet, string comment)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            result.TaskID = await (Task<Guid>)this.dataBase.ImportAsync(authentication, dataSet, comment);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> NewTableCategoryAsync(Guid authenticationToken, string categoryPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var categoryName = new JSSoft.Library.ObjectModel.CategoryName(categoryPath);
            var category = await this.GetTableCategoryAsync(categoryName.ParentPath);
            await category.AddNewCategoryAsync(authentication, categoryName.Name);
            result.TaskID = GuidUtility.FromName(categoryPath);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<CremaDataSet>> GetTableItemDataSetAsync(Guid authenticationToken, string itemPath, string revision)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<CremaDataSet>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.Value = await tableItem.GetDataSetAsync(authentication, revision);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> RenameTableItemAsync(Guid authenticationToken, string itemPath, string newName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.RenameAsync(authentication, newName);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> MoveTableItemAsync(Guid authenticationToken, string itemPath, string parentPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.MoveAsync(authentication, parentPath);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> DeleteTableItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.DeleteAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> SetPublicTableItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.SetPublicAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessInfo>> SetPrivateTableItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessInfo>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.SetPrivateAsync(authentication);
            result.Value = await tableItem.Dispatcher.InvokeAsync(() => tableItem.AccessInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> AddAccessMemberTableItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessMemberInfo>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.AddAccessMemberAsync(authentication, memberID, accessType);
            var accessInfo = await tableItem.Dispatcher.InvokeAsync(() => tableItem.AccessInfo);
            result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> SetAccessMemberTableItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessMemberInfo>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.SetAccessMemberAsync(authentication, memberID, accessType);
            var accessInfo = await tableItem.Dispatcher.InvokeAsync(() => tableItem.AccessInfo);
            result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> RemoveAccessMemberTableItemAsync(Guid authenticationToken, string itemPath, string memberID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.RemoveAccessMemberAsync(authentication, memberID);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<LockInfo>> LockTableItemAsync(Guid authenticationToken, string itemPath, string comment)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<LockInfo>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.LockAsync(authentication, comment);
            result.Value = await tableItem.Dispatcher.InvokeAsync(() => tableItem.LockInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> UnlockTableItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)tableItem.UnlockAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<LogInfo[]>> GetTableItemLogAsync(Guid authenticationToken, string itemPath, string revision)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<LogInfo[]>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.Value = await tableItem.GetLogAsync(authentication, revision);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<FindResultInfo[]>> FindTableItemAsync(Guid authenticationToken, string itemPath, string text, FindOptions options)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<FindResultInfo[]>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            result.Value = await tableItem.FindAsync(authentication, text, options);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<TableInfo[]>> CopyTableAsync(Guid authenticationToken, string tableName, string newTableName, string categoryPath, bool copyXml)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<TableInfo[]>();
            var table = await this.GetTableAsync(tableName);
            var newTable = await table.CopyAsync(authentication, newTableName, categoryPath, copyXml);
            result.TaskID = GuidUtility.FromName(categoryPath + newTableName);
            result.Value = await table.Dispatcher.InvokeAsync(() => EnumerableUtility.FamilyTree(newTable, item => item.Childs).Select(item => item.TableInfo).ToArray());
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<TableInfo[]>> InheritTableAsync(Guid authenticationToken, string tableName, string newTableName, string categoryPath, bool copyXml)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<TableInfo[]>();
            var table = await this.GetTableAsync(tableName);
            var newTable = await table.InheritAsync(authentication, newTableName, categoryPath, copyXml);
            result.TaskID = GuidUtility.FromName(categoryPath + newTableName);
            result.Value = await table.Dispatcher.InvokeAsync(() => EnumerableUtility.FamilyTree(newTable, item => item.Childs).Select(item => item.TableInfo).ToArray());
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainMetaData>> EnterTableContentEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DomainMetaData>();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            var content = domain.Host as ITableContentGroup;
            await content.EnterEditAsync(authentication);
            result.Value = await domain.GetMetaDataAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainMetaData>> LeaveTableContentEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DomainMetaData>();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            var content = domain.Host as ITableContentGroup;
            await content.LeaveEditAsync(authentication);
            result.Value = await domain.GetMetaDataAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainMetaData>> BeginTableContentEditAsync(Guid authenticationToken, string tableName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DomainMetaData>();
            var table = await this.GetTableAsync(tableName);
            var content = table.Content;
            await content.BeginEditAsync(authentication);
            var domain = content.Domain;
            result.Value = await domain.GetMetaDataAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<TableInfo[]>> EndTableContentEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<TableInfo[]>();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            var content = domain.Host as ITableContentGroup;
            var tables = content.Tables;
            await content.EndEditAsync(authentication);
            result.Value = domain.Result as TableInfo[];
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> CancelTableContentEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            var content = domain.Host as ITableContentGroup;
            await content.CancelEditAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainMetaData>> BeginTableTemplateEditAsync(Guid authenticationToken, string tableName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DomainMetaData>();
            var table = await this.GetTableAsync(tableName);
            var template = table.Template;
            await template.BeginEditAsync(authentication);
            var domain = template.Domain;
            result.Value = await domain.GetMetaDataAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainMetaData>> BeginNewTableAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DomainMetaData>();
            var tableItem = await this.GetTableItemAsync(itemPath);
            if (tableItem is ITableCategory category)
            {
                var template = await category.NewTableAsync(authentication);
                var domain = template.Domain;
                result.Value = await domain.GetMetaDataAsync(authentication);
                result.SignatureDate = authentication.SignatureDate;
            }
            else if (tableItem is ITable table)
            {
                var template = await table.NewTableAsync(authentication);
                var domain = template.Domain;
                result.Value = await domain.GetMetaDataAsync(authentication);
                result.SignatureDate = authentication.SignatureDate;
            }
            else
            {
                throw new NotImplementedException();
            }
            return result;
        }

        public async Task<ResultBase<TableInfo[]>> EndTableTemplateEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<TableInfo[]>();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            var template = domain.Host as ITableTemplate;
            await template.EndEditAsync(authentication);
            if (template.Target is ITable table)
            {
                result.Value = domain.Result as TableInfo[];
                result.SignatureDate = authentication.SignatureDate;
            }
            else if (template.Target is ITable[] tables)
            {
                result.Value = await template.Dispatcher.InvokeAsync(() => tables.Select(item => item.TableInfo).ToArray());
                result.SignatureDate = authentication.SignatureDate;
            }
            else
            {
                throw new NotImplementedException();
            }
            return result;
        }

        public async Task<ResultBase> CancelTableTemplateEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            if (domain == null)
                throw new DomainNotFoundException(domainID);
            var template = domain.Host as ITableTemplate;
            await template.CancelEditAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> NewTypeCategoryAsync(Guid authenticationToken, string categoryPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var categoryName = new JSSoft.Library.ObjectModel.CategoryName(categoryPath);
            var category = await this.GetTypeCategoryAsync(categoryName.ParentPath);
            await category.AddNewCategoryAsync(authentication, categoryName.Name);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<CremaDataSet>> GetTypeItemDataSetAsync(Guid authenticationToken, string itemPath, string revision)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<CremaDataSet>();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.Value = await typeItem.GetDataSetAsync(authentication, revision);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> RenameTypeItemAsync(Guid authenticationToken, string itemPath, string newName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.RenameAsync(authentication, newName);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> MoveTypeItemAsync(Guid authenticationToken, string itemPath, string parentPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.MoveAsync(authentication, parentPath);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> DeleteTypeItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.DeleteAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<TypeInfo>> CopyTypeAsync(Guid authenticationToken, string typeName, string newTypeName, string categoryPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<TypeInfo>();
            var type = await this.GetTypeAsync(typeName);
            var newType = await type.CopyAsync(authentication, newTypeName, categoryPath);
            result.TaskID = GuidUtility.FromName(categoryPath + newTypeName);
            result.Value = await type.Dispatcher.InvokeAsync(() => newType.TypeInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainMetaData>> BeginTypeTemplateEditAsync(Guid authenticationToken, string typeName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DomainMetaData>();
            var type = await this.GetTypeAsync(typeName);
            var template = type.Template;
            await template.BeginEditAsync(authentication);
            var domain = template.Domain;
            result.Value = await domain.GetMetaDataAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DomainMetaData>> BeginNewTypeAsync(Guid authenticationToken, string categoryPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DomainMetaData>();
            var category = await this.GetTypeCategoryAsync(categoryPath);
            var template = await category.NewTypeAsync(authentication);
            var domain = template.Domain;
            result.Value = await domain.GetMetaDataAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<TypeInfo[]>> EndTypeTemplateEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<TypeInfo[]>();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            var template = domain.Host as ITypeTemplate;
            await template.EndEditAsync(authentication);
            result.Value = domain.Result as TypeInfo[];
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> CancelTypeTemplateEditAsync(Guid authenticationToken, Guid domainID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var domain = await this.DomainContext.Dispatcher.InvokeAsync(() => this.DomainContext.Domains[domainID]);
            if (domain == null)
                throw new DomainNotFoundException(domainID);
            var template = domain.Host as ITypeTemplate;
            await template.CancelEditAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> SetPublicTypeItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.SetPublicAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessInfo>> SetPrivateTypeItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessInfo>();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.SetPrivateAsync(authentication);
            result.Value = await typeItem.Dispatcher.InvokeAsync(() => typeItem.AccessInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> AddAccessMemberTypeItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessMemberInfo>();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.AddAccessMemberAsync(authentication, memberID, accessType);
            var accessInfo = await typeItem.Dispatcher.InvokeAsync(() => typeItem.AccessInfo);
            result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> SetAccessMemberTypeItemAsync(Guid authenticationToken, string itemPath, string memberID, AccessType accessType)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessMemberInfo>();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.SetAccessMemberAsync(authentication, memberID, accessType);
            var accessInfo = await typeItem.Dispatcher.InvokeAsync(() => typeItem.AccessInfo);
            result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> RemoveAccessMemberTypeItemAsync(Guid authenticationToken, string itemPath, string memberID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.RemoveAccessMemberAsync(authentication, memberID);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<LockInfo>> LockTypeItemAsync(Guid authenticationToken, string itemPath, string comment)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<LockInfo>();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.LockAsync(authentication, comment);
            result.Value = await typeItem.Dispatcher.InvokeAsync(() => typeItem.LockInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> UnlockTypeItemAsync(Guid authenticationToken, string itemPath)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.TaskID = await (Task<Guid>)typeItem.UnlockAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<LogInfo[]>> GetTypeItemLogAsync(Guid authenticationToken, string itemPath, string revision)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<LogInfo[]>();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.Value = await typeItem.GetLogAsync(authentication, revision);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<FindResultInfo[]>> FindTypeItemAsync(Guid authenticationToken, string itemPath, string text, FindOptions options)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<FindResultInfo[]>();
            var typeItem = await this.GetTypeItemAsync(itemPath);
            result.Value = await typeItem.FindAsync(authentication, text, options);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public IDomainContext DomainContext { get; }

        public IUserCollection UserCollection { get; }

        public IDataBaseContext DataBasesContext { get; }

        private void Tables_TablesStateChanged(object sender, ItemsEventArgs<ITable> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var tableNames = e.Items.Select(item => item.Name).ToArray();
            var states = e.Items.Select(item => item.TableState).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTablesStateChanged(callbackInfo, tableNames, states));
        }

        private void Tables_TablesChanged(object sender, ItemsChangedEventArgs<ITable> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = e.Items.Select(item => item.TableInfo).ToArray();
            var itemType = e.ItemType;
            this.InvokeEvent(() => this.Callback?.OnTablesChanged(callbackInfo, values, itemType));
        }

        private void TableContext_ItemCreated(object sender, ItemsCreatedEventArgs<ITableItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var paths = e.Items.Select(item => item.Path).ToArray();
            var arguments = e.Arguments.Select(item => item is TableInfo tableInfo ? (TableInfo?)tableInfo : null).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTableItemsCreated(callbackInfo, paths, arguments));
        }

        private void TableContext_ItemRenamed(object sender, ItemsRenamedEventArgs<ITableItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTableItemsRenamed(callbackInfo, oldPaths, itemNames));
        }

        private void TableContext_ItemMoved(object sender, ItemsMovedEventArgs<ITableItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var parentPaths = e.Items.Select(item =>
            {
                if (NameValidator.VerifyCategoryPath(item.Path) == true)
                    return new CategoryName(item.Path).ParentPath;
                return new ItemName(item.Path).CategoryPath;
            }).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTableItemsMoved(callbackInfo, oldPaths, parentPaths));
        }

        private void TableContext_ItemDeleted(object sender, ItemsDeletedEventArgs<ITableItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.ItemPaths;
            this.InvokeEvent(() => this.Callback?.OnTableItemsDeleted(callbackInfo, itemPaths));
        }

        private void TableContext_ItemsAccessChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = new AccessInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var accessInfo = item.AccessInfo;
                if (item.AccessInfo.Path != item.Path)
                {
                    accessInfo = AccessInfo.Empty;
                    accessInfo.Path = item.Path;
                }
                values[i] = accessInfo;
            }
            var metaData = e.MetaData as object[];
            var changeType = (AccessChangeType)metaData[0];
            var memberIDs = metaData[1] as string[];
            var accessTypes = metaData[2] as AccessType[];

            this.InvokeEvent(() => this.Callback?.OnTableItemsAccessChanged(callbackInfo, changeType, values, memberIDs, accessTypes));
        }

        private void TableContext_ItemsLockChanged(object sender, ItemsEventArgs<ITableItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = new LockInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var lockInfo = item.LockInfo;
                if (item.LockInfo.Path != item.Path)
                {
                    lockInfo = LockInfo.Empty;
                    lockInfo.Path = item.Path;
                }
                values[i] = lockInfo;
            }
            var metaData = e.MetaData as object[];
            var changeType = (LockChangeType)metaData[0];
            var comments = metaData[1] as string[];

            this.InvokeEvent(() => this.Callback?.OnTableItemsLockChanged(callbackInfo, changeType, values, comments));
        }

        private void Types_TypesStateChanged(object sender, ItemsEventArgs<IType> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var typeNames = e.Items.Select(item => item.Name).ToArray();
            var states = e.Items.Select(item => item.TypeState).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTypesStateChanged(callbackInfo, typeNames, states));
        }

        private void Types_TypesChanged(object sender, ItemsEventArgs<IType> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = e.Items.Select(item => item.TypeInfo).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTypesChanged(callbackInfo, values));
        }

        private void TypeContext_ItemCreated(object sender, ItemsCreatedEventArgs<ITypeItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.Items.Select(item => item.Path).ToArray();
            var arguments = e.Arguments.Select(item => item is TypeInfo typeInfo ? (TypeInfo?)typeInfo : null).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTypeItemsCreated(callbackInfo, itemPaths, arguments));
        }

        private void TypeContext_ItemRenamed(object sender, ItemsRenamedEventArgs<ITypeItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTypeItemsRenamed(callbackInfo, oldPaths, itemNames));
        }

        private void TypeContext_ItemMoved(object sender, ItemsMovedEventArgs<ITypeItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldPaths = e.OldPaths;
            var parentPaths = e.Items.Select(item => item.Parent.Path).ToArray();
            this.InvokeEvent(() => this.Callback?.OnTypeItemsMoved(callbackInfo, oldPaths, parentPaths));
        }

        private void TypeContext_ItemDeleted(object sender, ItemsDeletedEventArgs<ITypeItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.ItemPaths;
            this.InvokeEvent(() => this.Callback?.OnTypeItemsDeleted(callbackInfo, itemPaths));
        }

        private void TypeContext_ItemsAccessChanged(object sender, ItemsEventArgs<ITypeItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = new AccessInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var accessInfo = item.AccessInfo;
                if (item.AccessInfo.Path != item.Path)
                {
                    accessInfo = AccessInfo.Empty;
                    accessInfo.Path = item.Path;
                }
                values[i] = accessInfo;
            }
            var metaData = e.MetaData as object[];
            var changeType = (AccessChangeType)metaData[0];
            var memberIDs = metaData[1] as string[];
            var accessTypes = metaData[2] as AccessType[];

            this.InvokeEvent(() => this.Callback?.OnTypeItemsAccessChanged(callbackInfo, changeType, values, memberIDs, accessTypes));
        }

        private void TypeContext_ItemsLockChanged(object sender, ItemsEventArgs<ITypeItem> e)
        {
            var exceptionUserID = e.InvokeID;
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var values = new LockInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var lockInfo = item.LockInfo;
                if (item.LockInfo.Path != item.Path)
                {
                    lockInfo = LockInfo.Empty;
                    lockInfo.Path = item.Path;
                }
                values[i] = lockInfo;
            }
            var metaData = e.MetaData as object[];
            var changeType = (LockChangeType)metaData[0];
            var comments = metaData[1] as string[];
            this.InvokeEvent(() => this.Callback?.OnTypeItemsLockChanged(callbackInfo, changeType, values, comments));
        }

        private void DataBase_TaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var taskIDs = e.TaskIDs;
            this.InvokeEvent(() => this.Callback?.OnTaskCompleted(callbackInfo, taskIDs));
        }

        private void DataBase_Unloaded(object sender, EventArgs e)
        {
            this.dataBase = null;
        }

        private async Task<DataBaseMetaData> AttachEventHandlersAsync(Guid token)
        {
            var metaData = await this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                this.TableContext.Tables.TablesStateChanged += Tables_TablesStateChanged;
                this.TableContext.Tables.TablesChanged += Tables_TablesChanged;
                this.TableContext.ItemsCreated += TableContext_ItemCreated;
                this.TableContext.ItemsRenamed += TableContext_ItemRenamed;
                this.TableContext.ItemsMoved += TableContext_ItemMoved;
                this.TableContext.ItemsDeleted += TableContext_ItemDeleted;
                this.TableContext.ItemsAccessChanged += TableContext_ItemsAccessChanged;
                this.TableContext.ItemsLockChanged += TableContext_ItemsLockChanged;

                this.TypeContext.Types.TypesStateChanged += Types_TypesStateChanged;
                this.TypeContext.Types.TypesChanged += Types_TypesChanged;
                this.TypeContext.ItemsCreated += TypeContext_ItemCreated;
                this.TypeContext.ItemsRenamed += TypeContext_ItemRenamed;
                this.TypeContext.ItemsMoved += TypeContext_ItemMoved;
                this.TypeContext.ItemsDeleted += TypeContext_ItemDeleted;
                this.TypeContext.ItemsAccessChanged += TypeContext_ItemsAccessChanged;
                this.TypeContext.ItemsLockChanged += TypeContext_ItemsLockChanged;

                this.dataBase.TaskCompleted += DataBase_TaskCompleted;
                this.dataBase.Unloaded += DataBase_Unloaded;
                return this.dataBase.GetMetaData();
            });
            this.LogService.Debug($"[{token}] {nameof(DataBaseService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }

        private async Task DetachEventHandlersAsync(Guid token)
        {
            if (this.dataBase != null)
            {
                await this.dataBase.Dispatcher.InvokeAsync(() =>
                {
                    this.TableContext.Tables.TablesStateChanged -= Tables_TablesStateChanged;
                    this.TableContext.Tables.TablesChanged -= Tables_TablesChanged;
                    this.TableContext.ItemsCreated -= TableContext_ItemCreated;
                    this.TableContext.ItemsRenamed -= TableContext_ItemRenamed;
                    this.TableContext.ItemsMoved -= TableContext_ItemMoved;
                    this.TableContext.ItemsDeleted -= TableContext_ItemDeleted;
                    this.TableContext.ItemsAccessChanged -= TableContext_ItemsAccessChanged;
                    this.TableContext.ItemsLockChanged -= TableContext_ItemsLockChanged;

                    this.TypeContext.Types.TypesStateChanged -= Types_TypesStateChanged;
                    this.TypeContext.Types.TypesChanged -= Types_TypesChanged;
                    this.TypeContext.ItemsCreated -= TypeContext_ItemCreated;
                    this.TypeContext.ItemsRenamed -= TypeContext_ItemRenamed;
                    this.TypeContext.ItemsMoved -= TypeContext_ItemMoved;
                    this.TypeContext.ItemsDeleted -= TypeContext_ItemDeleted;
                    this.TypeContext.ItemsAccessChanged -= TypeContext_ItemsAccessChanged;
                    this.TypeContext.ItemsLockChanged -= TypeContext_ItemsLockChanged;

                    this.dataBase.TaskCompleted -= DataBase_TaskCompleted;
                    this.dataBase.Unloaded -= DataBase_Unloaded;
                });
            }
            this.LogService.Debug($"[{token}] {nameof(DataBaseService)} {nameof(DetachEventHandlersAsync)}");
        }

        private Task<ITypeItem> GetTypeItemAsync(string itemPath)
        {
            return this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                var item = this.TypeContext[itemPath];
                if (item == null)
                    throw new ItemNotFoundException(itemPath);
                return item;
            });
        }

        private Task<IType> GetTypeAsync(string typeName)
        {
            return this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                var type = this.TypeContext.Types[typeName];
                if (type == null)
                    throw new TypeNotFoundException(typeName);
                return type;
            });
        }

        private Task<ITypeCategory> GetTypeCategoryAsync(string categoryPath)
        {
            return this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                var category = this.TypeContext.Categories[categoryPath];
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                return category;
            });
        }

        private Task<ITableItem> GetTableItemAsync(string itemPath)
        {
            return this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                var item = this.TableContext[itemPath];
                if (item == null)
                    throw new ItemNotFoundException(itemPath);
                return item;
            });
        }

        private Task<ITable> GetTableAsync(string tableName)
        {
            return this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                var table = this.TableContext.Tables[tableName];
                if (table == null)
                    throw new TableNotFoundException(tableName);
                return table;
            });
        }

        private Task<ITableCategory> GetTableCategoryAsync(string categoryPath)
        {
            return this.dataBase.Dispatcher.InvokeAsync(() =>
            {
                var category = this.TableContext.Categories[categoryPath];
                if (category == null)
                    throw new CategoryNotFoundException(categoryPath);
                return category;
            });
        }

        private ITableContext TableContext => this.dataBase.GetService(typeof(ITableContext)) as ITableContext;

        private ITypeContext TypeContext => this.dataBase.GetService(typeof(ITypeContext)) as ITypeContext;
    }
}

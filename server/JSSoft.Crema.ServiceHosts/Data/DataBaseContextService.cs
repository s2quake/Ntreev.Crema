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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.ServiceHosts.Data
{
    class DataBaseContextService : CremaServiceItemBase<IDataBaseContextEventCallback>, IDataBaseContextService
    {
        private readonly Dictionary<Guid, ITransaction> transactionByID = new();
        private long index = 0;
        private Peer peer;

        public DataBaseContextService(CremaService service, IDataBaseContextEventCallback callback)
            : base(service, callback)
        {
            this.DataBaseContext = this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            this.UserContext = this.CremaHost.GetService(typeof(IUserContext)) as IUserContext;
            this.UserCollection = this.CremaHost.GetService(typeof(IUserCollection)) as IUserCollection;

            this.LogService.Debug($"{nameof(DataBaseContextService)} Constructor");
        }

        public async Task DisposeAsync()
        {
            if (this.peer != null)
            {
                await sthis.DetachEventHandlersAsync(this.peer.ID);
                this.peer = null;
            }
        }

        public async Task<ResultBase<DataBaseContextMetaData>> SubscribeAsync(Guid token)
        {
            if (peer is not null)
                throw new InvalidOperationException();
            var value = await this.AttachEventHandlersAsync(token);
            this.peer = Peer.GetPeer(token);
            this.LogService.Debug($"[{token}] {nameof(DataBaseContextService)} {nameof(SubscribeAsync)}");
            return new ResultBase<DataBaseContextMetaData>()
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
            this.peer = null;
            this.LogService.Debug($"[{token}] {nameof(DataBaseContextService)} {nameof(UnsubscribeAsync)}");
            return new ResultBase()
            {
                SignatureDate = new SignatureDateProvider($"{token}")
            };
        }

        public async Task<ResultBase<CremaDataSet>> GetDataSetAsync(Guid authenticationToken, string dataBaseName, DataSetType dataSetType, string filterExpression, string revision)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<CremaDataSet>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.Value = await dataBase.GetDataSetAsync(authentication, dataSetType, filterExpression, revision);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> SetPublicAsync(Guid authenticationToken, string dataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.SetPublicAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessInfo>> SetPrivateAsync(Guid authenticationToken, string dataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessInfo>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.SetPrivateAsync(authentication);
            result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> AddAccessMemberAsync(Guid authenticationToken, string dataBaseName, string memberID, AccessType accessType)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessMemberInfo>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.AddAccessMemberAsync(authentication, memberID, accessType);
            var accessInfo = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
            result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> SetAccessMemberAsync(Guid authenticationToken, string dataBaseName, string memberID, AccessType accessType)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<AccessMemberInfo>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.SetAccessMemberAsync(authentication, memberID, accessType);
            var accessInfo = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
            result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> RemoveAccessMemberAsync(Guid authenticationToken, string dataBaseName, string memberID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.RemoveAccessMemberAsync(authentication, memberID);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<LockInfo>> LockAsync(Guid authenticationToken, string dataBaseName, string comment)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<LockInfo>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.LockAsync(authentication, comment);
            result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.LockInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> UnlockAsync(Guid authenticationToken, string dataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.UnlockAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> LoadAsync(Guid authenticationToken, string dataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.LoadAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> UnloadAsync(Guid authenticationToken, string dataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.UnloadAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> CreateAsync(Guid authenticationToken, string dataBaseName, string comment)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DataBaseInfo>();
            var dataBase = await this.DataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
            result.TaskID = GuidUtility.FromName(dataBaseName + comment);
            result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> RenameAsync(Guid authenticationToken, string dataBaseName, string newDataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.RenameAsync(authentication, newDataBaseName);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase> DeleteAsync(Guid authenticationToken, string dataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.DeleteAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> CopyAsync(Guid authenticationToken, string dataBaseName, string newDataBaseName, string comment, bool force)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DataBaseInfo>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            var newDataBase = await dataBase.CopyAsync(authentication, newDataBaseName, comment, force);
            result.TaskID = GuidUtility.FromName(newDataBaseName + comment);
            result.Value = await newDataBase.Dispatcher.InvokeAsync(() => newDataBase.DataBaseInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<LogInfo[]>> GetLogAsync(Guid authenticationToken, string dataBaseName, string revision)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<LogInfo[]>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.Value = await dataBase.GetLogAsync(authentication, revision);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> RevertAsync(Guid authenticationToken, string dataBaseName, string revision)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DataBaseInfo>();
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            result.TaskID = await (Task<Guid>)dataBase.RevertAsync(authentication, revision);
            result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo);
            result.SignatureDate = authentication.SignatureDate;
            return result;
        }

        public async Task<ResultBase<Guid>> BeginTransactionAsync(Guid authenticationToken, string dataBaseName)
        {
            var authentication = this.peer[authenticationToken];
            var dataBase = await this.GetDataBaseAsync(dataBaseName);
            var transaction = await dataBase.BeginTransactionAsync(authentication);
            this.transactionByID[transaction.ID] = transaction;
            return new ResultBase<Guid>()
            {
                Value = transaction.ID,
                SignatureDate = authentication.SignatureDate
            };
        }

        public async Task<ResultBase> EndTransactionAsync(Guid authenticationToken, Guid transactionID)
        {
            var authentication = this.peer[authenticationToken];
            var transaction = this.transactionByID[transactionID];
            await transaction.CommitAsync(authentication);
            this.transactionByID.Remove(transactionID);
            return new ResultBase()
            {
                SignatureDate = authentication.SignatureDate
            };
        }

        public async Task<ResultBase<DataBaseMetaData>> CancelTransactionAsync(Guid authenticationToken, Guid transactionID)
        {
            var authentication = this.peer[authenticationToken];
            var result = new ResultBase<DataBaseMetaData>();
            var transaction = this.transactionByID[transactionID];
            result.Value = await transaction.RollbackAsync(authentication);
            result.SignatureDate = authentication.SignatureDate;
            this.transactionByID.Remove(transactionID);
            return result;
        }

        public IDataBaseContext DataBaseContext { get; }

        public IUserContext UserContext { get; }

        public IUserCollection UserCollection { get; }

        private void DataBaseContext_ItemsCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var dataBaseNames = e.Items.Select(item => item.Name).ToArray();
            var dataBaseInfos = e.Arguments.Select(item => (DataBaseInfo)item).ToArray();
            var comment = e.MetaData as string;
            this.InvokeEvent(() => this.Callback?.OnDataBasesCreated(callbackInfo, dataBaseNames, dataBaseInfos, comment));
        }

        private void DataBaseContext_ItemsRenamed(object sender, ItemsRenamedEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var oldNames = e.OldNames;
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnDataBasesRenamed(callbackInfo, oldNames, itemNames));
        }

        private void DataBaseContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemPaths = e.ItemPaths;
            this.InvokeEvent(() => this.Callback?.OnDataBasesDeleted(callbackInfo, itemPaths));
        }

        private void DataBaseContext_ItemsLoaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnDataBasesLoaded(callbackInfo, itemNames));
        }

        private void DataBaseContext_ItemsUnloaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnDataBasesUnloaded(callbackInfo, itemNames));
        }

        private void DataBaseContext_ItemsResetting(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            this.InvokeEvent(() => this.Callback?.OnDataBasesResetting(callbackInfo, itemNames));
        }

        private void DataBaseContext_ItemsReset(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var metaDatas = e.MetaData as DataBaseMetaData[];
            this.InvokeEvent(() => this.Callback?.OnDataBasesReset(callbackInfo, itemNames, metaDatas));
        }

        private void DataBaseContext_ItemsAuthenticationEntered(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var authenticationInfo = (AuthenticationInfo)e.MetaData;
            this.InvokeEvent(() => this.Callback?.OnDataBasesAuthenticationEntered(callbackInfo, itemNames, authenticationInfo));
        }

        private void DataBaseContext_ItemsAuthenticationLeft(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var authenticationInfo = (AuthenticationInfo)e.MetaData;
            this.InvokeEvent(() => this.Callback?.OnDataBasesAuthenticationLeft(callbackInfo, itemNames, authenticationInfo));
        }

        private void DataBaseContext_ItemsInfoChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var dataBaseInfos = e.Items.Select(item => item.DataBaseInfo).ToArray();

            this.InvokeEvent(() => this.Callback?.OnDataBasesInfoChanged(callbackInfo, dataBaseInfos));
        }

        private void DataBaseContext_ItemsStateChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var itemNames = e.Items.Select(item => item.Name).ToArray();
            var dataBaseStates = e.Items.Select(item => item.DataBaseState).ToArray();
            this.InvokeEvent(() => this.Callback?.OnDataBasesStateChanged(callbackInfo, itemNames, dataBaseStates));
        }

        private void DataBaseContext_ItemsAccessChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var metaData = e.MetaData as object[];
            var changeType = (AccessChangeType)metaData[0];
            var accessInfos = new AccessInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var accessInfo = item.AccessInfo;
                if (changeType == AccessChangeType.Public)
                {
                    accessInfo.Path = item.Name;
                }
                accessInfos[i] = accessInfo;
            }
            var memberIDs = metaData[1] as string[];
            var accessTypes = metaData[2] as AccessType[];
            this.InvokeEvent(() => this.Callback?.OnDataBasesAccessChanged(callbackInfo, changeType, accessInfos, memberIDs, accessTypes));
        }

        private void DataBaseContext_ItemsLockChanged(object sender, ItemsEventArgs<IDataBase> e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var metaData = e.MetaData as object[];
            var changeType = (LockChangeType)metaData[0];
            var lockInfos = new LockInfo[e.Items.Length];
            for (var i = 0; i < e.Items.Length; i++)
            {
                var item = e.Items[i];
                var lockInfo = item.LockInfo;
                if (changeType == LockChangeType.Unlock)
                {
                    lockInfo.Path = item.Name;
                }
                lockInfos[i] = lockInfo;
            }
            var comments = metaData[1] as string[];
            this.InvokeEvent(() => this.Callback?.OnDataBasesLockChanged(callbackInfo, changeType, lockInfos, comments));
        }

        private void DataBaseContext_TaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = e.SignatureDate };
            var taskIDs = e.TaskIDs;
            this.InvokeEvent(() => this.Callback?.OnTaskCompleted(callbackInfo, taskIDs));
        }

        private async Task<DataBaseContextMetaData> AttachEventHandlersAsync(Guid token)
        {
            var metaData = await this.DataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                this.DataBaseContext.ItemsCreated += DataBaseContext_ItemsCreated;
                this.DataBaseContext.ItemsRenamed += DataBaseContext_ItemsRenamed;
                this.DataBaseContext.ItemsDeleted += DataBaseContext_ItemsDeleted;
                this.DataBaseContext.ItemsLoaded += DataBaseContext_ItemsLoaded;
                this.DataBaseContext.ItemsUnloaded += DataBaseContext_ItemsUnloaded;
                this.DataBaseContext.ItemsResetting += DataBaseContext_ItemsResetting;
                this.DataBaseContext.ItemsReset += DataBaseContext_ItemsReset;
                this.DataBaseContext.ItemsAuthenticationEntered += DataBaseContext_ItemsAuthenticationEntered;
                this.DataBaseContext.ItemsAuthenticationLeft += DataBaseContext_ItemsAuthenticationLeft;
                this.DataBaseContext.ItemsInfoChanged += DataBaseContext_ItemsInfoChanged;
                this.DataBaseContext.ItemsStateChanged += DataBaseContext_ItemsStateChanged;
                this.DataBaseContext.ItemsAccessChanged += DataBaseContext_ItemsAccessChanged;
                this.DataBaseContext.ItemsLockChanged += DataBaseContext_ItemsLockChanged;
                this.DataBaseContext.TaskCompleted += DataBaseContext_TaskCompleted;
                return this.DataBaseContext.GetMetaData();
            });
            this.LogService.Debug($"[{token}] {nameof(DataBaseContextService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }

        private async Task DetachEventHandlersAsync(Guid token)
        {
            await this.DataBaseContext.Dispatcher.InvokeAsync(() =>
            {
                this.DataBaseContext.ItemsCreated -= DataBaseContext_ItemsCreated;
                this.DataBaseContext.ItemsRenamed -= DataBaseContext_ItemsRenamed;
                this.DataBaseContext.ItemsDeleted -= DataBaseContext_ItemsDeleted;
                this.DataBaseContext.ItemsLoaded -= DataBaseContext_ItemsLoaded;
                this.DataBaseContext.ItemsUnloaded -= DataBaseContext_ItemsUnloaded;
                this.DataBaseContext.ItemsResetting -= DataBaseContext_ItemsResetting;
                this.DataBaseContext.ItemsReset -= DataBaseContext_ItemsReset;
                this.DataBaseContext.ItemsAuthenticationEntered -= DataBaseContext_ItemsAuthenticationEntered;
                this.DataBaseContext.ItemsAuthenticationLeft -= DataBaseContext_ItemsAuthenticationLeft;
                this.DataBaseContext.ItemsInfoChanged -= DataBaseContext_ItemsInfoChanged;
                this.DataBaseContext.ItemsStateChanged -= DataBaseContext_ItemsStateChanged;
                this.DataBaseContext.ItemsAccessChanged -= DataBaseContext_ItemsAccessChanged;
                this.DataBaseContext.ItemsLockChanged -= DataBaseContext_ItemsLockChanged;
                this.DataBaseContext.TaskCompleted -= DataBaseContext_TaskCompleted;
            });
            this.LogService.Debug($"[{token}] {nameof(DataBaseContextService)} {nameof(DetachEventHandlersAsync)}");
        }

        private async Task<IDataBase> GetDataBaseAsync(string dataBaseName)
        {
            var dataBase = await this.CremaHost.Dispatcher.InvokeAsync(() => this.DataBaseContext[dataBaseName]);
            if (dataBase == null)
                throw new DataBaseNotFoundException(dataBaseName);
            return dataBase;
        }
    }
}

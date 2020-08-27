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
        private Authentication authentication;
        private readonly Dictionary<Guid, ITransaction> transactionByID = new Dictionary<Guid, ITransaction>();
        private long index = 0;

        public DataBaseContextService(CremaService service, IDataBaseContextEventCallback callback)
            : base(service, callback)
        {
            this.DataBaseContext = this.CremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            this.UserContext = this.CremaHost.GetService(typeof(IUserContext)) as IUserContext;

            this.LogService.Debug($"{nameof(DataBaseContextService)} Constructor");
        }

        public async Task<ResultBase<DataBaseContextMetaData>> SubscribeAsync(Guid authenticationToken)
        {
            var result = new ResultBase<DataBaseContextMetaData>();
            try
            {
                this.authentication = await this.UserContext.AuthenticateAsync(authenticationToken);
                this.OwnerID = this.authentication.ID;
                result.Value = await this.AttachEventHandlersAsync();
                result.SignatureDate = this.authentication.SignatureDate;
                this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseContextService)} {nameof(SubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnsubscribeAsync()
        {
            var result = new ResultBase();
            try
            {
                await this.DetachEventHandlersAsync();
                await this.UserContext.Dispatcher.InvokeAsync(() =>
                {
                    this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;
                });
                this.authentication = null;
                result.SignatureDate = new SignatureDateProvider(this.OwnerID).Provide();
                this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseContextService)} {nameof(UnsubscribeAsync)}");
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<CremaDataSet>> GetDataSetAsync(string dataBaseName, DataSetType dataSetType, string filterExpression, string revision)
        {
            var result = new ResultBase<CremaDataSet>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.Value = await dataBase.GetDataSetAsync(this.authentication, dataSetType, filterExpression, revision);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault() { ExceptionType = e.GetType().Name, Message = e.Message };
            }
            return result;
        }

        public async Task<ResultBase> SetPublicAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.SetPublicAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<AccessInfo>> SetPrivateAsync(string dataBaseName)
        {
            var result = new ResultBase<AccessInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.SetPrivateAsync(this.authentication);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> AddAccessMemberAsync(string dataBaseName, string memberID, AccessType accessType)
        {
            var result = new ResultBase<AccessMemberInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.AddAccessMemberAsync(this.authentication, memberID, accessType);
                var accessInfo = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
                result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<AccessMemberInfo>> SetAccessMemberAsync(string dataBaseName, string memberID, AccessType accessType)
        {
            var result = new ResultBase<AccessMemberInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.SetAccessMemberAsync(this.authentication, memberID, accessType);
                var accessInfo = await dataBase.Dispatcher.InvokeAsync(() => dataBase.AccessInfo);
                result.Value = accessInfo.Members.Where(item => item.UserID == memberID).First();
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> RemoveAccessMemberAsync(string dataBaseName, string memberID)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.RemoveAccessMemberAsync(this.authentication, memberID);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<LockInfo>> LockAsync(string dataBaseName, string comment)
        {
            var result = new ResultBase<LockInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.LockAsync(this.authentication, comment);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.LockInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnlockAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.UnlockAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> LoadAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.LoadAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> UnloadAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.UnloadAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> CreateAsync(string dataBaseName, string comment)
        {
            var result = new ResultBase<DataBaseInfo>();
            try
            {
                var dataBase = await this.DataBaseContext.AddNewDataBaseAsync(this.authentication, dataBaseName, comment);
                result.TaskID = GuidUtility.FromName(dataBaseName + comment);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> RenameAsync(string dataBaseName, string newDataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.RenameAsync(this.authentication, newDataBaseName);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> DeleteAsync(string dataBaseName)
        {
            var result = new ResultBase();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.DeleteAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> CopyAsync(string dataBaseName, string newDataBaseName, string comment, bool force)
        {
            var result = new ResultBase<DataBaseInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                var newDataBase = await dataBase.CopyAsync(this.authentication, newDataBaseName, comment, force);
                result.TaskID = GuidUtility.FromName(newDataBaseName + comment);
                result.Value = await newDataBase.Dispatcher.InvokeAsync(() => newDataBase.DataBaseInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<LogInfo[]>> GetLogAsync(string dataBaseName, string revision)
        {
            var result = new ResultBase<LogInfo[]>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.Value = await dataBase.GetLogAsync(this.authentication, revision);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DataBaseInfo>> RevertAsync(string dataBaseName, string revision)
        {
            var result = new ResultBase<DataBaseInfo>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                result.TaskID = await (Task<Guid>)dataBase.RevertAsync(this.authentication, revision);
                result.Value = await dataBase.Dispatcher.InvokeAsync(() => dataBase.DataBaseInfo);
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<Guid>> BeginTransactionAsync(string dataBaseName)
        {
            var result = new ResultBase<Guid>();
            try
            {
                var dataBase = await this.GetDataBaseAsync(dataBaseName);
                var transaction = await dataBase.BeginTransactionAsync(this.authentication);
                this.transactionByID[transaction.ID] = transaction;
                result.Value = transaction.ID;
                result.SignatureDate = this.authentication.SignatureDate;
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase> EndTransactionAsync(Guid transactionID)
        {
            var result = new ResultBase();
            try
            {
                var transaction = this.transactionByID[transactionID];
                await transaction.CommitAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
                this.transactionByID.Remove(transactionID);
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<ResultBase<DataBaseMetaData>> CancelTransactionAsync(Guid transactionID)
        {
            var result = new ResultBase<DataBaseMetaData>();
            try
            {
                var transaction = this.transactionByID[transactionID];
                result.Value = await transaction.RollbackAsync(this.authentication);
                result.SignatureDate = this.authentication.SignatureDate;
                this.transactionByID.Remove(transactionID);
            }
            catch (Exception e)
            {
                result.Fault = new CremaFault(e);
            }
            return result;
        }

        public async Task<bool> IsAliveAsync()
        {
            if (this.authentication == null)
                return false;
            this.LogService.Debug($"[{this.authentication}] {nameof(DataBaseContextService)}.{nameof(IsAliveAsync)} : {DateTime.Now}");
            await Task.Delay(1);
            return true;
        }

        public IDataBaseContext DataBaseContext { get; }

        public IUserContext UserContext { get; }

        protected override void OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            var callbackInfo = new CallbackInfo() { Index = this.index++, SignatureDate = signatureDate };
            this.Callback?.OnServiceClosed(callbackInfo, closeInfo);
        }

        protected override async Task OnCloseAsync(bool disconnect)
        {
            if (this.authentication != null)
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
            }
        }

        private async void Users_UsersLoggedOut(object sender, ItemsEventArgs<IUser> e)
        {
            var actionUserID = e.UserID;
            var contains = e.Items.Any(item => item.ID == this.authentication.ID);
            var closeInfo = (CloseInfo)e.MetaData;
            if (actionUserID != this.authentication.ID && contains == true)
            {
                await this.DetachEventHandlersAsync();
                this.authentication = null;
                // this.Channel.Abort();
            }
        }

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

        private async Task<DataBaseContextMetaData> AttachEventHandlersAsync()
        {
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut += Users_UsersLoggedOut;
            });
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
                return this.DataBaseContext.GetMetaData(this.authentication);
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseContextService)} {nameof(AttachEventHandlersAsync)}");
            return metaData;
        }

        private async Task DetachEventHandlersAsync()
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
            await this.UserContext.Dispatcher.InvokeAsync(() =>
            {
                this.UserContext.Users.UsersLoggedOut -= Users_UsersLoggedOut;
            });
            this.LogService.Debug($"[{this.OwnerID}] {nameof(DataBaseContextService)} {nameof(DetachEventHandlersAsync)}");
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

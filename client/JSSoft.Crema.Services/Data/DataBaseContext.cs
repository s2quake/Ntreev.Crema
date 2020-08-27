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

using Ntreev.Crema.ServiceHosts.Data;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseContext : ContainerBase<DataBase>, IDataBaseContext, IDataBaseContextEventCallback
    {
        private bool isDisposed;

        private ItemsCreatedEventHandler<IDataBase> itemsCreated;
        private ItemsRenamedEventHandler<IDataBase> itemsRenamed;
        private ItemsDeletedEventHandler<IDataBase> itemsDeleted;
        private ItemsEventHandler<IDataBase> itemsLoaded;
        private ItemsEventHandler<IDataBase> itemsUnloaded;
        private ItemsEventHandler<IDataBase> itemsResetting;
        private ItemsEventHandler<IDataBase> itemsReset;
        private ItemsEventHandler<IDataBase> itemsAuthenticationEntered;
        private ItemsEventHandler<IDataBase> itemsAuthenticationLeft;
        private ItemsEventHandler<IDataBase> itemsInfoChanged;
        private ItemsEventHandler<IDataBase> itemsStateChanged;
        private ItemsEventHandler<IDataBase> itemsAccessChanged;
        private ItemsEventHandler<IDataBase> itemsLockChanged;
        private TaskCompletedEventHandler taskCompleted;

        private readonly TaskResetEvent<Guid> taskEvent;
        private readonly IndexedDispatcher callbackEvent;

        public DataBaseContext(CremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = new CremaDispatcher(this);
            this.taskEvent = new TaskResetEvent<Guid>(this.Dispatcher);
            this.callbackEvent = new IndexedDispatcher(this);
        }

        public void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            this.callbackEvent.Dispose();
        }

        public Task WaitAsync(Guid taskID)
        {
            return this.taskEvent.WaitAsync(taskID);
        }

        public async Task InitializeAsync(Guid authenticationToken)
        {
            var result = await this.Service.SubscribeAsync(authenticationToken);
            await this.Dispatcher.InvokeAsync(() =>
            {
                var metaData = result.Value;
                this.Initialize(metaData);
            });
        }

        public async Task<LockInfo> InvokeDataBaseLock(Authentication authentication, DataBase dataBase, string comment)
        {
            this.CremaHost.DebugMethod(authentication, this, nameof(InvokeDataBaseLock), dataBase, comment);
            var result = await this.Service.LockAsync(dataBase.Name, comment);
            result.Validate(authentication);
            return result.Value;
        }

        public async Task InvokeDataBaseUnlock(Authentication authentication, DataBase dataBase)
        {
            this.CremaHost.DebugMethod(authentication, this, nameof(InvokeDataBaseUnlock), dataBase);
            var result = await this.Service.UnlockAsync(dataBase.Name);
            result.Validate(authentication);
        }

        public async Task<AccessInfo> InvokeDataBaseSetPrivate(Authentication authentication, DataBase dataBase)
        {
            this.CremaHost.DebugMethod(authentication, this, nameof(InvokeDataBaseSetPrivate), dataBase);
            var result = await this.Service.SetPrivateAsync(dataBase.Name);
            result.Validate(authentication);
            return result.Value;
        }

        public async Task InvokeDataBaseSetPublic(Authentication authentication, DataBase dataBase)
        {
            this.CremaHost.DebugMethod(authentication, this, nameof(InvokeDataBaseSetPrivate), dataBase);
            var result = await this.Service.SetPublicAsync(dataBase.Name);
            result.Validate(authentication);
        }

        public async Task<AccessMemberInfo> InvokeDataBaseAddAccessMember(Authentication authentication, DataBase dataBase, string memberID, AccessType accessType)
        {
            this.CremaHost.DebugMethod(authentication, this, nameof(InvokeDataBaseAddAccessMember), dataBase, memberID, accessType);
            var result = await this.Service.AddAccessMemberAsync(dataBase.Name, memberID, accessType);
            result.Validate(authentication);
            return result.Value;
        }

        public async Task<AccessMemberInfo> InvokeDataBaseSetAccessMember(Authentication authentication, DataBase dataBase, string memberID, AccessType accessType)
        {
            this.CremaHost.DebugMethod(authentication, this, nameof(InvokeDataBaseSetAccessMember), dataBase, memberID, accessType);
            var result = await this.Service.SetAccessMemberAsync(dataBase.Name, memberID, accessType);
            result.Validate(authentication);
            return result.Value;
        }

        public async Task InvokeDataBaseRemoveAccessMember(Authentication authentication, DataBase dataBase, string memberID)
        {
            this.CremaHost.DebugMethod(authentication, this, nameof(InvokeDataBaseRemoveAccessMember), dataBase, memberID);
            var result = await this.Service.RemoveAccessMemberAsync(dataBase.Name, memberID);
            result.Validate(authentication);
        }

        public async Task<DataBase> AddNewDataBaseAsync(Authentication authentication, string dataBaseName, string comment)
        {
            try
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewDataBaseAsync), dataBaseName, comment);
                });
                var taskID = GuidUtility.FromName(dataBaseName + comment);
                var result = await this.Service.CreateAsync(dataBaseName, comment);
                await this.WaitAsync(taskID);
                return await this.Dispatcher.InvokeAsync(() => this[dataBaseName]);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DataBase> CopyDataBaseAsync(Authentication authentication, DataBase dataBase, string newDataBaseName, string comment, bool force)
        {
            try
            {
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CopyDataBaseAsync), dataBase, newDataBaseName, comment, force);
                    return dataBase.Name;
                });
                var taskID = GuidUtility.FromName(newDataBaseName + comment);
                var result = await this.Service.CopyAsync(name, newDataBaseName, comment, force);
                await this.WaitAsync(taskID);
                return await this.Dispatcher.InvokeAsync(() => this[newDataBaseName]);
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<LogInfo[]> GetLog(Authentication authentication, DataBase dataBase, string revision)
        {
            this.Dispatcher.VerifyAccess();
            this.CremaHost.DebugMethod(authentication, this, nameof(GetLog), dataBase);

            var result = await this.Service.GetLogAsync(dataBase.Name, revision);
            result.Validate(authentication);
            return result.Value ?? new LogInfo[] { };
        }

        public async Task Revert(Authentication authentication, DataBase dataBase, string revision)
        {
            this.Dispatcher.VerifyAccess();
            this.CremaHost.DebugMethod(authentication, this, nameof(Revert), dataBase);

            var result = await this.Service.RevertAsync(dataBase.Name, revision);
            result.Validate(authentication);
            dataBase.SetDataBaseInfo(result.Value);
            this.InvokeItemsRevertedEvent(authentication, new DataBase[] { dataBase }, new string[] { revision });
        }

        public DataBaseContextMetaData GetMetaData(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));

            var dataBases = this.ToArray<DataBase>();
            var metaList = new List<DataBaseMetaData>(this.Count);
            foreach (var item in dataBases)
            {
                var metaData = item.Dispatcher.Invoke(() => item.GetMetaData(authentication));
                metaList.Add(metaData);
            }
            return new DataBaseContextMetaData()
            {
                DataBases = metaList.ToArray(),
            };
        }

        public async Task<DataBaseContextMetaData> GetMetaDataAsync(Authentication authentication)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));

            var dataBases = await this.Dispatcher.InvokeAsync(() => (from DataBase item in this select item).ToArray());
            var metaList = new List<DataBaseMetaData>(this.Count);
            foreach (var item in dataBases)
            {
                metaList.Add(await item.GetMetaDataAsync(authentication));
            }
            return new DataBaseContextMetaData()
            {
                DataBases = metaList.ToArray(),
            };
        }

        public void InvokeItemsCreateEvent(Authentication authentication, IDataBase[] items, string comment)
        {
            var args = items.Select(item => (object)item.DataBaseInfo).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsCreateEvent), items);
            var commentMessage = EventMessageBuilder.CreateDataBase(authentication, items) + Environment.NewLine + comment;
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(commentMessage);
            this.OnItemsCreated(new ItemsCreatedEventArgs<IDataBase>(authentication, items, args, comment));
        }

        public void InvokeItemsRenamedEvent(Authentication authentication, IDataBase[] items, string[] oldNames)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsRenamedEvent), items, oldNames);
            var message = EventMessageBuilder.RenameDataBase(authentication, items, oldNames);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsRenamed(new ItemsRenamedEventArgs<IDataBase>(authentication, items, oldNames, oldNames));
        }

        public void InvokeItemsDeletedEvent(Authentication authentication, IDataBase[] items, string[] paths)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsDeletedEvent), paths);
            var message = EventMessageBuilder.DeleteDataBase(authentication, items);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsDeleted(new ItemsDeletedEventArgs<IDataBase>(authentication, items, paths));
        }

        public void InvokeItemsRevertedEvent(Authentication authentication, IDataBase[] items, string[] revisions)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsRevertedEvent), items, revisions);
            var message = EventMessageBuilder.RevertDataBase(authentication, items, revisions);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsInfoChanged(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsLoadedEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsLoadedEvent), items);
            this.CremaHost.Info(EventMessageBuilder.LoadDataBase(authentication, items));
            this.OnItemsLoaded(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsAuthenticationEnteredEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsAuthenticationEnteredEvent), items);
            this.CremaHost.Info(EventMessageBuilder.EnterDataBase(authentication, items));
            this.OnItemsEntered(new ItemsEventArgs<IDataBase>(authentication, items, authentication.AuthenticationInfo));
        }

        public void InvokeItemsAuthenticationLeftEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsAuthenticationLeftEvent), items);
            this.CremaHost.Info(EventMessageBuilder.LeaveDataBase(authentication, items));
            this.OnItemsLeft(new ItemsEventArgs<IDataBase>(authentication, items, authentication.AuthenticationInfo));
        }

        public void InvokeItemsUnloadedEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsUnloadedEvent), items);
            this.CremaHost.Info(EventMessageBuilder.UnloadDataBase(authentication, items));
            this.OnItemsUnloaded(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsResettingEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsResettingEvent), items);
            this.CremaHost.Info(EventMessageBuilder.ResettingDataBase(authentication, items));
            this.OnItemsResetting(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsResetEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsResetEvent), items);
            this.CremaHost.Info(EventMessageBuilder.ResetDataBase(authentication, items));
            this.OnItemsReset(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsInfoChangedEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsInfoChangedEvent), items);
            this.OnItemsInfoChanged(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsStateChangedEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsStateChangedEvent), items);
            this.OnItemsStateChanged(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsSetPublicEvent(Authentication authentication, IDataBase[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPublicEvent), items);
            var message = EventMessageBuilder.SetPublicDataBase(authentication, items);
            var metaData = new object[] { AccessChangeType.Public, new string[] { string.Empty, }, new AccessType[] { AccessType.None, }, };
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsSetPrivateEvent(Authentication authentication, IDataBase[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPrivateEvent), items);
            var message = EventMessageBuilder.SetPrivateDataBase(authentication, items);
            var metaData = new object[] { AccessChangeType.Private, new string[] { string.Empty, }, new AccessType[] { AccessType.None, }, };
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsAddAccessMemberEvent(Authentication authentication, IDataBase[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsAddAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.AddAccessMemberToDataBase(authentication, items, memberIDs, accessTypes);
            var metaData = new object[] { AccessChangeType.Add, memberIDs, accessTypes, };
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsSetAccessMemberEvent(Authentication authentication, IDataBase[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.SetAccessMemberOfDataBase(authentication, items, memberIDs, accessTypes);
            var metaData = new object[] { AccessChangeType.Set, memberIDs, accessTypes, };
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsRemoveAccessMemberEvent(Authentication authentication, IDataBase[] items, string[] memberIDs)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsRemoveAccessMemberEvent), items, memberIDs);
            var message = EventMessageBuilder.RemoveAccessMemberFromDataBase(authentication, items, memberIDs);
            var metaData = new object[] { AccessChangeType.Remove, memberIDs, new AccessType[] { AccessType.None, }, };
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, new object[] { AccessChangeType.Remove, memberIDs, }));
        }

        public void InvokeItemsLockedEvent(Authentication authentication, IDataBase[] items, string[] comments)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsLockedEvent), items, comments);
            var message = EventMessageBuilder.LockDataBase(authentication, items, comments);
            var metaData = new object[] { LockChangeType.Lock, new string[] { message, }, };
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsUnlockedEvent(Authentication authentication, IDataBase[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsUnlockedEvent), items);
            var metaData = new object[] { LockChangeType.Unlock, new string[] { string.Empty, }, };
            this.CremaHost.Debug(eventLog);
            this.OnItemsLockChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeTaskCompletedEvent(Authentication authentication, Guid taskID)
        {
            this.OnTaskCompleted(new TaskCompletedEventArgs(authentication, taskID));
        }

        public async Task CloseAsync(CloseInfo closeInfo)
        {
            var result = await this.CremaHost.Dispatcher.InvokeAsync(() =>
            {
                if (this.isDisposed == true)
                    return false;
                this.isDisposed = true;
                return true;
            });
            if (result == false)
                return;

            var dataBases = await this.Dispatcher.InvokeAsync(() => this.ToArray<DataBase>());
            foreach (var item in dataBases)
            {
                await item.CloseAsync(closeInfo);
            }

            if (this.Service == null)
                return;
            await this.Service.UnsubscribeAsync();
            await Task.Delay(100);
            await this.callbackEvent.DisposeAsync();
            await this.Dispatcher.DisposeAsync();
            this.Service = null;
            this.Dispatcher = null;
        }

        public Task<ResultBase> LoadDataBase(DataBase dataBase)
        {
            return this.Service.LoadAsync(dataBase.Name);
        }

        public Task<ResultBase> UnloadDataBase(DataBase dataBase)
        {
            return this.Service.UnloadAsync(dataBase.Name);
        }

        public new DataBase this[string dataBaseName] => base[dataBaseName];

        public DataBase this[Guid dataBaseID] => this.FirstOrDefault<DataBase>(item => item.ID == dataBaseID);

        public CremaDispatcher Dispatcher { get; set; }

        public CremaHost CremaHost { get; }

        public IDataBaseContextService Service { get; set; }

        public UserContext UserContext => this.CremaHost.UserContext;

        public new int Count => base.Count;

        public event ItemsCreatedEventHandler<IDataBase> ItemsCreated
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsCreated += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsCreated -= value;
            }
        }

        public event ItemsRenamedEventHandler<IDataBase> ItemsRenamed
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsRenamed += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsRenamed -= value;
            }
        }

        public event ItemsDeletedEventHandler<IDataBase> ItemsDeleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsDeleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsDeleted -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsLoaded
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsLoaded += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsLoaded -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsUnloaded
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsUnloaded += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsUnloaded -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsResetting
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsResetting += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsResetting -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsReset
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsReset += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsReset -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsAuthenticationEntered
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsAuthenticationEntered += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsAuthenticationEntered -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsAuthenticationLeft
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsAuthenticationLeft += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsAuthenticationLeft -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsInfoChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsInfoChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsInfoChanged -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsStateChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsStateChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsStateChanged -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsAccessChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsAccessChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsAccessChanged -= value;
            }
        }

        public event ItemsEventHandler<IDataBase> ItemsLockChanged
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.itemsLockChanged += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.itemsLockChanged -= value;
            }
        }

        public event TaskCompletedEventHandler TaskCompleted
        {
            add
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted += value;
            }
            remove
            {
                this.Dispatcher.VerifyAccess();
                this.taskCompleted -= value;
            }
        }

        protected virtual void OnItemsCreated(ItemsCreatedEventArgs<IDataBase> e)
        {
            this.itemsCreated?.Invoke(this, e);
        }

        protected virtual void OnItemsRenamed(ItemsRenamedEventArgs<IDataBase> e)
        {
            this.itemsRenamed?.Invoke(this, e);
        }

        protected virtual void OnItemsDeleted(ItemsDeletedEventArgs<IDataBase> e)
        {
            this.itemsDeleted?.Invoke(this, e);
        }

        protected virtual void OnItemsLoaded(ItemsEventArgs<IDataBase> e)
        {
            this.itemsLoaded?.Invoke(this, e);
        }

        protected virtual void OnItemsUnloaded(ItemsEventArgs<IDataBase> e)
        {
            this.itemsUnloaded?.Invoke(this, e);
        }

        protected virtual void OnItemsResetting(ItemsEventArgs<IDataBase> e)
        {
            this.itemsResetting?.Invoke(this, e);
        }

        protected virtual void OnItemsReset(ItemsEventArgs<IDataBase> e)
        {
            this.itemsReset?.Invoke(this, e);
        }

        protected virtual void OnItemsEntered(ItemsEventArgs<IDataBase> e)
        {
            this.itemsAuthenticationEntered?.Invoke(this, e);
        }

        protected virtual void OnItemsLeft(ItemsEventArgs<IDataBase> e)
        {
            this.itemsAuthenticationLeft?.Invoke(this, e);
        }

        protected virtual void OnItemsInfoChanged(ItemsEventArgs<IDataBase> e)
        {
            this.itemsInfoChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsStateChanged(ItemsEventArgs<IDataBase> e)
        {
            this.itemsStateChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsAccessChanged(ItemsEventArgs<IDataBase> e)
        {
            this.itemsAccessChanged?.Invoke(this, e);
        }

        protected virtual void OnItemsLockChanged(ItemsEventArgs<IDataBase> e)
        {
            this.itemsLockChanged?.Invoke(this, e);
        }

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs e)
        {
            this.taskCompleted?.Invoke(this, e);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher?.VerifyAccess();
            base.OnCollectionChanged(e);
        }

        private void Initialize(DataBaseContextMetaData metaData)
        {
            for (var i = 0; i < metaData.DataBases.Length; i++)
            {
                var dataBaseInfo = metaData.DataBases[i];
                var dataBase = new DataBase(this, dataBaseInfo);
                this.AddBase(dataBase.Name, dataBase);
            }
        }

        #region IDataBaseContextEventCallback

        async void IDataBaseContextEventCallback.OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo)
        {
            await this.CloseAsync(closeInfo);
        }

        async void IDataBaseContextEventCallback.OnDataBasesCreated(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseInfo[] dataBaseInfos, string comment)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBaseInfo = dataBaseInfos[i];
                            var dataBase = new DataBase(this, dataBaseInfo);
                            this.AddBase(dataBase.Name, dataBase);
                            dataBases[i] = dataBase;
                        }
                        this.InvokeItemsCreateEvent(authentication, dataBases, comment);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesRenamed(CallbackInfo callbackInfo, string[] dataBaseNames, string[] newDataBaseNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var newDataBaseName = newDataBaseNames[i];
                            var dataBase = this[dataBaseName];
                            this.ReplaceKeyBase(dataBaseName, newDataBaseName);
                            dataBase.Name = newDataBaseName;
                            dataBases[i] = dataBase;
                        }
                        this.InvokeItemsRenamedEvent(authentication, dataBases, dataBaseNames);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesDeleted(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBase = this[dataBaseName];
                            this.RemoveBase(dataBaseName);
                            dataBases[i] = dataBase;
                        }
                        this.InvokeItemsDeletedEvent(authentication, dataBases, dataBaseNames);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesLoaded(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBase = this[dataBaseName];
                            dataBase.SetLoaded(authentication);
                            dataBases[i] = dataBase;
                        }
                        this.InvokeItemsLoadedEvent(authentication, dataBases);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesUnloaded(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBase = this[dataBaseName];
                            dataBases[i] = dataBase;
                            dataBase.SetUnloadedAsync(authentication).Wait();
                        }
                        this.InvokeItemsUnloadedEvent(authentication, dataBases);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesResetting(CallbackInfo callbackInfo, string[] dataBaseNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBase = this[dataBaseName];
                            dataBases[i] = dataBase;
                            dataBase.SetResetting(authentication);
                        }
                        this.InvokeItemsResettingEvent(authentication, dataBases);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesReset(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseMetaData[] metaDatas)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var metaData = metaDatas[i];
                            var dataBase = this[dataBaseName];
                            dataBases[i] = dataBase;
                            dataBase.SetReset(authentication, metaData);
                        }
                        //this.InvokeItemsResetEvent(authentication, dataBases, metaData);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesAuthenticationEntered(CallbackInfo callbackInfo, string[] dataBaseNames, AuthenticationInfo authenticationInfo)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Dispatcher.Invoke(() => this.UserContext.Authenticate(callbackInfo.SignatureDate));
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBase = this[dataBaseName];
                            dataBase.SetAuthenticationEntered(authentication);
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesAuthenticationLeft(CallbackInfo callbackInfo, string[] dataBaseNames, AuthenticationInfo authenticationInfo)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBase = this[dataBaseName];
                            dataBases[i] = dataBase;
                            dataBase.SetAuthenticationLeft(authentication);
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesInfoChanged(CallbackInfo callbackInfo, DataBaseInfo[] dataBaseInfos)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseInfos.Length];
                        for (var i = 0; i < dataBaseInfos.Length; i++)
                        {
                            var dataBaseInfo = dataBaseInfos[i];
                            var dataBase = this[dataBaseInfo.Name];
                            dataBase.SetDataBaseInfo(dataBaseInfo);
                            dataBases[i] = dataBase;
                        }
                        this.InvokeItemsInfoChangedEvent(authentication, dataBases);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesStateChanged(CallbackInfo callbackInfo, string[] dataBaseNames, DataBaseState[] dataBaseStates)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[dataBaseNames.Length];
                        for (var i = 0; i < dataBaseNames.Length; i++)
                        {
                            var dataBaseName = dataBaseNames[i];
                            var dataBaseState = dataBaseStates[i];
                            var dataBase = this[dataBaseName];
                            dataBase.SetDataBaseState(dataBaseState);
                            dataBases[i] = dataBase;
                        }
                        this.InvokeItemsStateChangedEvent(authentication, dataBases);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesAccessChanged(CallbackInfo callbackInfo, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[accessInfos.Length];
                        for (var i = 0; i < accessInfos.Length; i++)
                        {
                            var accessInfo = accessInfos[i];
                            var dataBase = this[accessInfo.Path];
                            if (changeType == AccessChangeType.Public)
                                accessInfo.Path = string.Empty;
                            dataBase.SetAccessInfo(accessInfo);
                            dataBases[i] = dataBase;
                        }
                        switch (changeType)
                        {
                            case AccessChangeType.Public:
                                this.InvokeItemsSetPublicEvent(authentication, dataBases);
                                break;
                            case AccessChangeType.Private:
                                this.InvokeItemsSetPrivateEvent(authentication, dataBases);
                                break;
                            case AccessChangeType.Add:
                                this.InvokeItemsAddAccessMemberEvent(authentication, dataBases, memberIDs, accessTypes);
                                break;
                            case AccessChangeType.Set:
                                this.InvokeItemsSetAccessMemberEvent(authentication, dataBases, memberIDs, accessTypes);
                                break;
                            case AccessChangeType.Remove:
                                this.InvokeItemsRemoveAccessMemberEvent(authentication, dataBases, memberIDs);
                                break;
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnDataBasesLockChanged(CallbackInfo callbackInfo, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var dataBases = new DataBase[lockInfos.Length];
                        for (var i = 0; i < lockInfos.Length; i++)
                        {
                            var lockInfo = lockInfos[i];
                            var dataBase = this[lockInfo.Path];
                            if (changeType == LockChangeType.Unlock)
                                lockInfo.Path = string.Empty;
                            dataBase.SetLockInfo(lockInfo);
                            dataBases[i] = dataBase;
                        }
                        switch (changeType)
                        {
                            case LockChangeType.Lock:
                                this.InvokeItemsLockedEvent(authentication, dataBases, comments);
                                break;
                            case LockChangeType.Unlock:
                                this.InvokeItemsUnlockedEvent(authentication, dataBases);
                                break;
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseContextEventCallback.OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.taskEvent.Set(taskIDs);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        #endregion

        #region IDataBaseContext

        async Task<IDataBase> IDataBaseContext.AddNewDataBaseAsync(Authentication authentication, string dataBaseName, string comment)
        {
            return await this.AddNewDataBaseAsync(authentication, dataBaseName, comment);
        }

        bool IDataBaseContext.Contains(string dataBaseName)
        {
            return this.ContainsKey(dataBaseName);
        }

        IDataBase IDataBaseContext.this[string dataBaseName] => this[dataBaseName];

        IDataBase IDataBaseContext.this[Guid dataBaseID] => this[dataBaseID];

        #endregion

        #region IEnumerable

        IEnumerator<IDataBase> IEnumerable<IDataBase>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}

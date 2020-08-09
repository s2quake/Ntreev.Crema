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

using Ntreev.Crema.Data;
using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.Data.Serializations;
using Ntreev.Crema.Services.Properties;
using Ntreev.Library;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseContext : ContainerBase<DataBase>, IDataBaseContext
    {
        internal const string DataBasesString = "databases";

        private readonly IRepositoryProvider repositoryProvider;
        private readonly string cachePath;
        private readonly string basePath;
        private readonly CremaDispatcher repositoryDispatcher;

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

        public DataBaseContext(CremaHost cremaHost)
        {
            this.CremaHost = cremaHost;
            this.Dispatcher = new CremaDispatcher(this);
            this.cachePath = cremaHost.GetPath(CremaPath.Caches, DataBasesString);
            this.repositoryProvider = cremaHost.RepositoryProvider;
            this.RemotePath = cremaHost.GetPath(CremaPath.RepositoryDataBases);
            this.basePath = cremaHost.GetPath(CremaPath.DataBases);
            this.repositoryDispatcher = new CremaDispatcher($"Repository: {this.GetType().Name}");
        }

        public async Task RestoreStateAsync(CremaSettings settings)
        {
            var dataBaseList = new List<DataBase>(this.Count);
            if (settings.NoCache == false)
            {
                foreach (var item in this)
                {
                    if (Directory.Exists(item.BasePath) == true)
                    {
                        dataBaseList.Add(item);
                    }
                }
            }
            foreach (var item in settings.DataBases)
            {
                if (this.ContainsKey(item) == true)
                {
                    dataBaseList.Add(this[item]);
                }
                else
                {
                    CremaLog.Error(new DataBaseNotFoundException(item));
                }
            }
            var dataBases = dataBaseList.Distinct().ToArray();
            var tasks = dataBases.Select(item => item.LoadAsync(Authentication.System)).ToArray();
            await Task.WhenAll(tasks);
        }

        public async Task<DataBase> AddNewDataBaseAsync(Authentication authentication, string dataBaseName, string comment)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddNewDataBaseAsync), dataBaseName, comment);
                    this.ValidateCreateDataBase(authentication, dataBaseName);
                });
                var taskID = GuidUtility.FromName(dataBaseName + comment);
                var dataSet = new CremaDataSet();
                var tempPath = PathUtility.GetTempPath(true);
                var dataBasePath = Path.Combine(tempPath, dataBaseName);
                var message = EventMessageBuilder.CreateDataBase(authentication, dataBaseName) + ": " + comment;
                await this.repositoryDispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        FileUtility.WriteAllText($"{CremaSchema.MajorVersion}.{CremaSchema.MinorVersion}", dataBasePath, ".version");
                        dataSet.WriteToDirectory(dataBasePath);
                        this.repositoryProvider.CreateRepository(authentication, this.RemotePath, dataBasePath, comment);
                    }
                    finally
                    {
                        DirectoryUtility.Delete(tempPath);
                    }
                });
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    var dataBase = new DataBase(this, dataBaseName);
                    this.CremaHost.Sign(authentication);
                    this.AddBase(dataBase.Name, dataBase);
                    this.InvokeItemsCreateEvent(authentication, new DataBase[] { dataBase }, comment);
                    this.InvokeTaskCompletedEvent(authentication, taskID);
                    return dataBase;
                });
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
                this.ValidateExpired();
                var dataBaseName = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(CopyDataBaseAsync), dataBase, newDataBaseName, comment);
                    this.ValidateCopyDataBase(authentication, dataBase, newDataBaseName, force);
                    this.CremaHost.Sign(authentication);
                    return dataBase.Name;
                });
                var taskID = GuidUtility.FromName(newDataBaseName + comment);
                await this.RepositoryDispatcher.InvokeAsync(() =>
                {
                    var message = EventMessageBuilder.CreateDataBase(authentication, newDataBaseName) + ": " + comment;
                    this.repositoryProvider.CopyRepository(authentication, this.RemotePath, dataBaseName, newDataBaseName, comment);
                });
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    var newDataBase = new DataBase(this, newDataBaseName);
                    this.AddBase(newDataBase.Name, newDataBase);
                    this.InvokeItemsCreateEvent(authentication, new DataBase[] { newDataBase }, comment);
                    this.InvokeTaskCompletedEvent(authentication, taskID);
                    return newDataBase;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task InvokeDataBaseRenameAsync(Authentication authentication, DataBaseInfo dataBaseInfo, string newDataBaseName)
        {
            var message = EventMessageBuilder.RenameDataBase(authentication, dataBaseInfo.Name, newDataBaseName);
            var dataBasePath = this.CremaHost.GetPath(CremaPath.DataBases, $"{dataBaseInfo.ID}");
            var remotesPath = this.RemotePath;
            await this.repositoryDispatcher.InvokeAsync(() =>
            {
                if (Directory.Exists(dataBasePath) == true)
                {
                    this.repositoryProvider.RenameRepository(authentication, dataBasePath, dataBaseInfo.Name, newDataBaseName, message);
                }
                this.repositoryProvider.RenameRepository(authentication, remotesPath, dataBaseInfo.Name, newDataBaseName, message);
            });
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.ReplaceKeyBase(dataBaseInfo.Name, newDataBaseName);
            });
        }

        public async Task InvokeDataBaseDeleteAsync(Authentication authentication, DataBaseInfo dataBaseInfo)
        {
            var message = EventMessageBuilder.DeleteDataBase(authentication, dataBaseInfo.Name);
            await this.repositoryDispatcher.InvokeAsync(() =>
            {
                this.repositoryProvider.DeleteRepository(authentication, this.RemotePath, dataBaseInfo.Name, message);
            });
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.DeleteCaches(dataBaseInfo);
                this.RemoveBase(dataBaseInfo.Name);
            });
        }

        public Task<RepositoryInfo> InvokeDataBaseRevertAsync(Authentication authentication, string dataBaseName, string revision)
        {
            var comment = $"revert to {revision}";
            return this.repositoryDispatcher.InvokeAsync(() =>
            {
                var signatureDate = authentication.Sign();
                this.repositoryProvider.RevertRepository(authentication.ID, this.RemotePath, dataBaseName, revision, comment);
                return this.repositoryProvider.GetRepositoryInfo(this.CremaHost.GetPath(CremaPath.RepositoryDataBases), dataBaseName);
            });
        }

        public DataBaseContextMetaData GetMetaData(Authentication authentication)
        {
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
                var metaData = await item.Dispatcher.InvokeAsync(() => item.GetMetaData(authentication));
                metaList.Add(metaData);
            }
            return new DataBaseContextMetaData()
            {
                DataBases = metaList.ToArray(),
            };
        }

        public void InvokeItemsCreateEvent(Authentication authentication, DataBase[] items, string comment)
        {
            var args = items.Select(item => (object)item.DataBaseInfo).ToArray();
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsCreateEvent), items);
            var message = EventMessageBuilder.CreateDataBase(authentication, items) + ": " + comment;
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsCreated(new ItemsCreatedEventArgs<IDataBase>(authentication, items, args, null));
        }

        public void InvokeItemsRenamedEvent(Authentication authentication, DataBase[] items, string[] oldNames)
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

        public void InvokeItemsResetEvent(Authentication authentication, IDataBase[] items, DataBaseMetaData[] metaDatas)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsResetEvent), items);
            this.CremaHost.Info(EventMessageBuilder.ResetDataBase(authentication, items));
            this.OnItemsReset(new ItemsEventArgs<IDataBase>(authentication, items, metaDatas));
        }

        public void InvokeItemsAuthenticationEnteredEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsAuthenticationEnteredEvent), items);
            this.CremaHost.Info(EventMessageBuilder.EnterDataBase(authentication, items));
            this.OnItemsAuthenticationEntered(new ItemsEventArgs<IDataBase>(authentication, items, authentication.AuthenticationInfo));
        }

        public void InvokeItemsAuthenticationLeftEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsAuthenticationLeftEvent), items);
            this.CremaHost.Info(EventMessageBuilder.LeaveDataBase(authentication, items));
            this.OnItemsAuthenticationLeft(new ItemsEventArgs<IDataBase>(authentication, items, authentication.AuthenticationInfo));
        }

        public void InvokeItemsChangedEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsChangedEvent), items);
            this.OnItemsInfoChanged(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsStateChangedEvent(Authentication authentication, IDataBase[] items)
        {
            this.CremaHost.DebugMethodMany(authentication, this, nameof(InvokeItemsStateChangedEvent), items);
            this.OnItemsStateChanged(new ItemsEventArgs<IDataBase>(authentication, items));
        }

        public void InvokeItemsSetPublicEvent(Authentication authentication, string basePath, IDataBase[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPublicEvent), items);
            var message = EventMessageBuilder.SetPublicDataBase(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Public);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsSetPrivateEvent(Authentication authentication, string basePath, IDataBase[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetPrivateEvent), items);
            var message = EventMessageBuilder.SetPrivateDataBase(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Private);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsAddAccessMemberEvent(Authentication authentication, string basePath, IDataBase[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsAddAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.AddAccessMemberToDataBase(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Add, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsSetAccessMemberEvent(Authentication authentication, string basePath, IDataBase[] items, string[] memberIDs, AccessType[] accessTypes)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsSetAccessMemberEvent), items, memberIDs, accessTypes);
            var message = EventMessageBuilder.SetAccessMemberOfDataBase(authentication, items, memberIDs, accessTypes);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Set, memberIDs, accessTypes);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsRemoveAccessMemberEvent(Authentication authentication, string basePath, IDataBase[] items, string[] memberIDs)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsRemoveAccessMemberEvent), items, memberIDs);
            var message = EventMessageBuilder.RemoveAccessMemberFromDataBase(authentication, items, memberIDs);
            var metaData = EventMetaDataBuilder.Build(items, AccessChangeType.Remove, memberIDs);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsAccessChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsLockedEvent(Authentication authentication, IDataBase[] items, string[] comments)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsLockedEvent), items, comments);
            var message = EventMessageBuilder.LockDataBase(authentication, items, comments);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Lock, comments);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeItemsUnlockedEvent(Authentication authentication, IDataBase[] items)
        {
            var eventLog = EventLogBuilder.BuildMany(authentication, this, nameof(InvokeItemsUnlockedEvent), items);
            var message = EventMessageBuilder.UnlockDataBase(authentication, items);
            var metaData = EventMetaDataBuilder.Build(items, LockChangeType.Unlock);
            this.CremaHost.Debug(eventLog);
            this.CremaHost.Info(message);
            this.OnItemsLockChanged(new ItemsEventArgs<IDataBase>(authentication, items, metaData));
        }

        public void InvokeTaskCompletedEvent(Authentication authentication, Guid taskID)
        {
            this.OnTaskCompleted(new TaskCompletedEventArgs(authentication, taskID));
        }

        public async Task InitializeAsync()
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var caches = this.CremaHost.NoCache == true ? new Dictionary<string, DataBaseSerializationInfo>() : this.ReadCaches();
                var dataBases = this.repositoryProvider.GetRepositories(this.RemotePath);

                foreach (var item in dataBases)
                {
                    if (caches.ContainsKey(item) == false)
                        this.AddBase(item, new DataBase(this, item));
                    else
                        this.AddBase(item, new DataBase(this, item, caches[item]));
                }
                this.CremaHost.Info($"{nameof(DataBaseContext)} Initialized");
            });
        }

        public async Task DisposeAsync()
        {
            var dataBases = await this.Dispatcher.InvokeAsync(() => this.ToArray<DataBase>());
            foreach (var item in dataBases)
            {
                var dataBaseInfo = (DataBaseSerializationInfo)item.DataBaseInfo;
                var filename = FileUtility.Prepare(this.cachePath, $"{item.ID}");
                this.Serializer.Serialize(filename, dataBaseInfo, DataBaseSerializationInfo.Settings);
                await item.DisposeAsync();
            }
            await this.repositoryDispatcher.DisposeAsync();
            await this.Dispatcher.DisposeAsync();
            this.CremaHost.Info($"{nameof(DataBaseContext)} Disposed");
        }

        public new DataBase this[string dataBaseName] => base[dataBaseName];

        public DataBase this[Guid dataBaseID] => this.FirstOrDefault<DataBase>(item => item.ID == dataBaseID);

        public DataBase AddFromPath(string path)
        {
            var dataBase = new DataBase(this, Path.GetFileName(path));
            this.AddBase(dataBase.Name, dataBase);
            return dataBase;
        }

        public CremaDispatcher Dispatcher { get; }

        public CremaDispatcher RepositoryDispatcher => this.CremaHost.RepositoryDispatcher;

        public CremaHost CremaHost { get; }

        public IObjectSerializer Serializer => this.CremaHost.Serializer;

        public string RemotePath { get; }

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

        protected virtual void OnItemsAuthenticationEntered(ItemsEventArgs<IDataBase> e)
        {
            this.itemsAuthenticationEntered?.Invoke(this, e);
        }

        protected virtual void OnItemsAuthenticationLeft(ItemsEventArgs<IDataBase> e)
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

        private void ValidateCopyDataBase(Authentication authentication, DataBase dataBase, string newDataBaseName, bool force)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (this.ContainsKey(newDataBaseName) == true)
                throw new ArgumentException(string.Format(Resources.Exception_DataBaseIsAlreadyExisted_Format, newDataBaseName), nameof(newDataBaseName));

            if (force == true)
                return;

            if (dataBase.IsLoaded == true)
                throw new InvalidOperationException(Resources.Exception_DataBaseHasBeenLoaded);
        }

        private void ValidateCreateDataBase(Authentication authentication, string dataBaseName)
        {
            if (authentication.Types.HasFlag(AuthenticationType.Administrator) == false)
                throw new PermissionDeniedException();

            if (this.ContainsKey(dataBaseName) == true)
                throw new ArgumentException(string.Format(Resources.Exception_DataBaseIsAlreadyExisted_Format, dataBaseName), nameof(dataBaseName));
        }

        private Dictionary<string, DataBaseSerializationInfo> ReadCaches()
        {
            var caches = new Dictionary<string, DataBaseSerializationInfo>();
            if (Directory.Exists(this.cachePath) == true)
            {
                var itemPaths = this.Serializer.GetItemPaths(cachePath, typeof(DataBaseSerializationInfo), DataBaseSerializationInfo.Settings);
                foreach (var item in itemPaths)
                {
                    try
                    {
                        var dataBaseInfo = (DataBaseSerializationInfo)this.Serializer.Deserialize(item, typeof(DataBaseSerializationInfo), DataBaseSerializationInfo.Settings);
                        caches.Add(dataBaseInfo.Name, dataBaseInfo);
                    }
                    catch (Exception e)
                    {
                        this.CremaHost.Error(e);
                    }
                }
            }
            return caches;
        }

        private void DeleteCaches(DataBaseInfo dataBaseInfo)
        {
            var directoryName = Path.GetDirectoryName(this.cachePath);
            var name = $"{dataBaseInfo.ID}";
            var files = Directory.GetFiles(directoryName, $"{name}.*").Where(item => Path.GetFileNameWithoutExtension(item) == name).ToArray();
            FileUtility.Delete(files);
        }

        #region IDataBaseCollection

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

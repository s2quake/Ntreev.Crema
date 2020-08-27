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
using JSSoft.Crema.ServiceHosts.Data;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Domains;
using JSSoft.Crema.Services.Properties;
using JSSoft.Crema.Services.Users;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class DataBase : DataBaseBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext, Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        IDataBaseEventCallback, IDataBase, IInfoProvider, IStateProvider
    {
        private bool isDisposed;
        private DataBaseMetaData metaData;
        //private PingTimer pingTimer;

        private EventHandler<AuthenticationEventArgs> authenticationEntered;
        private EventHandler<AuthenticationEventArgs> authenticationLeft;
        private TaskCompletedEventHandler taskCompleted;

        private readonly HashSet<AuthenticationToken> authentications = new HashSet<AuthenticationToken>();
        private readonly HashSet<AuthenticationToken> authenticationInternals = new HashSet<AuthenticationToken>();
        private TaskResetEvent<Guid> taskEvent;
        private IndexedDispatcher callbackEvent;

        private DataBaseClientContext clientContext;
        private Guid serviceToken;
        private DataBaseServiceHost host;

        public DataBase(DataBaseContext dataBases, DataBaseInfo dataBaseInfo)
        {
            this.DataBaseContext = dataBases;
            this.Dispatcher = dataBases.Dispatcher;
            base.Name = dataBaseInfo.Name;
            base.DataBaseInfo = dataBaseInfo;
        }

        public DataBase(DataBaseContext dataBases, DataBaseMetaData metaData)
        {
            this.DataBaseContext = dataBases;
            this.Dispatcher = dataBases.Dispatcher;
            base.Name = metaData.DataBaseInfo.Name;
            base.DataBaseInfo = metaData.DataBaseInfo;
            base.DataBaseState = metaData.DataBaseState;
            base.AccessInfo = metaData.AccessInfo;
            base.LockInfo = metaData.LockInfo;

            foreach (var item in metaData.Authentications)
            {
                if (this.UserContext.Authenticate(item) is Authentication authentication)
                {
                    this.authenticationInternals.Add(authentication);
                }
            }
        }

        public override string ToString()
        {
            return base.Name;
        }

        public void InvokeTaskCompletedEvent(Authentication authentication, Guid taskID)
        {
            this.OnTaskCompleted(new TaskCompletedEventArgs(authentication, taskID));
        }

        public Task WaitAsync(Guid taskID)
        {
            return this.taskEvent.WaitAsync(taskID);
        }

        public AccessType GetAccessType(Authentication authentication)
        {
            this.ValidateExpired();
            return base.GetAccessType(authentication);
        }

        public async Task<Guid> SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.SetPublicAsync(name);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetPrivateAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.SetPrivateAsync(name);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMember), this, memberID, accessType);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.AddAccessMemberAsync(name, memberID, accessType);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.SetAccessMemberAsync(name, memberID, accessType);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.RemoveAccessMemberAsync(name, memberID);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LockAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.LockAsync(name, comment);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> UnlockAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.UnlockAsync(name);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> LoadAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LoadAsync), this);
                });
                var result = await this.DataBaseContext.Service.LoadAsync(base.Name);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> UnloadAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnloadAsync), this);
                });
                var result = await this.DataBaseContext.Service.UnloadAsync(base.Name);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task EnterAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var value = await this.Dispatcher.InvokeAsync(() =>
                {
                    if (this.IsLoaded == false)
                        throw new InvalidOperationException(Resources.Exception_CannotEnter);
                    if (this.authentications.Contains(authentication) == true)
                        throw new ArgumentException("AlreadyInDataBase", nameof(authentication));
                    this.authentications.Add(authentication);
                    return this.authentications.Any() && this.Dispatcher.Owner is DataBase == false;
                });

                if (value == true)
                {
                    this.Dispatcher = new CremaDispatcher(this);
                    this.taskEvent = new TaskResetEvent<Guid>(this.Dispatcher);
                    this.callbackEvent = new IndexedDispatcher(this);
                    this.host = new DataBaseServiceHost(this);
                    this.clientContext = new DataBaseClientContext(this.host);
                    this.serviceToken = await this.clientContext.OpenAsync();
                    var result = await this.Service.SubscribeAsync(this.CremaHost.AuthenticationToken, base.Name);
                    var taskID = await this.Dispatcher.InvokeAsync(() =>
                    {
                        var metaData = result.Value;
                        this.CremaHost.Sign(authentication, result);
                        this.TypeContext = new TypeContext(this, metaData);
                        this.TableContext = new TableContext(this, metaData);
                        base.UpdateAccessParent();
                        base.UpdateLockParent();
                        this.AttachDomainHost();
                        return result.TaskID;
                    });
                    await this.DataBaseContext.WaitAsync(taskID);
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task LeaveAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var value = await this.Dispatcher.InvokeAsync(() =>
                {
                    if (authentication == null)
                        throw new ArgumentNullException(nameof(authentication));
                    this.authentications.Remove(authentication);
                    return this.authentications.Any() == false && this.Dispatcher.Owner is DataBase;
                });
                if (value == true)
                {
                    var result = await this.Service.UnsubscribeAsync();
                    var taskID = await this.Dispatcher.InvokeAsync(() =>
                    {
                        authentication.SignatureDate = result.SignatureDate;
                        this.DetachDomainHost();
                        this.TypeContext.Dispose();
                        this.TypeContext = null;
                        this.TableContext.Dispose();
                        this.TableContext = null;
                        this.callbackEvent.Dispose();
                        this.callbackEvent = null;
                        this.Dispatcher.Dispose();
                        this.Dispatcher = this.DataBaseContext.Dispatcher;
                        return result.TaskID;
                    });
                    await this.DataBaseContext.WaitAsync(taskID);
                    await this.clientContext.CloseAsync(this.serviceToken);
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RenameAsync(Authentication authentication, string name)
        {
            try
            {
                this.ValidateExpired();
                var oldName = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.RenameAsync(oldName, name);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> DeleteAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var dataBaseContext = this.DataBaseContext;
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.DeleteAsync(name);
                await dataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public bool Contains(Authentication authentication)
        {
            return this.authentications.Contains(authentication);
        }

        public async Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetLogAsync), this);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.GetLogAsync(name, revision);
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    return result.Value;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> RevertAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RevertAsync), this, revision);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.RevertAsync(name, revision);
                await this.DataBaseContext.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<Guid> ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ImportAsync), this, comment);
                });
                var result = await this.Service.ImportDataSetAsync(dataSet, comment);
                await this.WaitAsync(result.TaskID);
                return result.TaskID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<CremaDataSet> GetDataSetAsync(Authentication authentication, DataSetType dataSetType, string filterExpression, string revision)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, dataSetType, filterExpression, revision);
                    return base.Name;
                });
                var result = await this.DataBaseContext.Service.GetDataSetAsync(name, dataSetType, filterExpression, revision);
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    return result.Value;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void LockForTransaction(Authentication authentication, Guid transactionID)
        {
            if (this.IsLocked == false)
            {
                this.Lock(authentication, $"{transactionID}");
                this.DataBaseContext.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { $"{transactionID}", });
            }
        }

        public void UnlockForTransaction(Authentication authentication, Guid transactionID)
        {
            if (this.LockInfo.Comment == $"{transactionID}" && this.IsLocked == true)
            {
                this.Unlock(authentication);
                this.DataBaseContext.InvokeItemsUnlockedEvent(authentication, new IDataBase[] { this });
            }
        }

        public async Task<DataBaseTransaction> BeginTransactionAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var transaction = await this.Dispatcher.InvokeAsync(() =>
                {
                    return new DataBaseTransaction(this, this.DataBaseContext.Service);
                });
                await transaction.BeginAsync(authentication);
                return transaction;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void ValidateGetDataSet(Authentication authentication)
        {
            if (this.IsLoaded == false)
                throw new NotImplementedException();
            this.VerifyAccessType(authentication, AccessType.Guest);
        }

        public bool VerifyAccess(Authentication authentication)
        {
            return this.authentications.Contains(authentication);
        }

        public DataBaseMetaData GetMetaData(Authentication authentication)
        {
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            return this.metaData;
        }

        public Task<DataBaseMetaData> GetMetaDataAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (authentication == null)
                    throw new ArgumentNullException(nameof(authentication));
                return this.metaData;
            });
        }

        public void SetLoaded(Authentication authentication)
        {
            base.DataBaseState = DataBaseState.Loaded;
            base.Load(authentication);
        }

        public async Task SetUnloadedAsync(Authentication authentication)
        {
            if (this.Dispatcher.Owner is DataBase)
            {
                var result = await this.Service.UnsubscribeAsync();
                authentication.SignatureDate = result.SignatureDate;
                this.DetachDomainHost();
                this.TypeContext.Dispose();
                this.TypeContext = null;
                this.TableContext.Dispose();
                this.TableContext = null;
                this.callbackEvent.Dispose();
                this.callbackEvent = null;
                this.Dispatcher.Dispose();
                this.Dispatcher = this.DataBaseContext.Dispatcher;
            }
            this.authenticationInternals.Clear();
            this.authentications.Clear();
            base.DataBaseState = DataBaseState.None;
            base.Unload(authentication);
#pragma warning disable CS4014 // 이 호출이 대기되지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다. 호출 결과에 'await' 연산자를 적용해 보세요.
            this.clientContext.CloseAsync(this.serviceToken);
#pragma warning restore CS4014 // 이 호출이 대기되지 않으므로 호출이 완료되기 전에 현재 메서드가 계속 실행됩니다. 호출 결과에 'await' 연산자를 적용해 보세요.
        }

        public void SetResetting(Authentication authentication)
        {
            if (this.Dispatcher.Owner is DataBase)
            {
                //this.DetachDomainHost();
                this.TypeContext?.Dispose();
                this.TypeContext = null;
                this.TableContext?.Dispose();
                this.TypeContext = null;
            }

            this.IsResetting = true;
            base.ResettingDataBase(authentication);
            this.DataBaseContext.InvokeItemsResettingEvent(authentication, new IDataBase[] { this, });
        }

        // TODO: SetReset
        public void SetReset(Authentication authentication, DataBaseMetaData metaData)
        {
            //throw new NotImplementedException();
            //var domains = metaDatas.Where(item => item.DomainInfo.DataBaseID == this.ID).ToArray();
            //await this.CremaHost.DomainContext.AddDomainsAsync(domains);\

            if (this.Dispatcher.Owner is DataBase)
            {
                this.Dispatcher.InvokeAsync(() =>
                {
                    this.TypeContext = new TypeContext(this, metaData);
                    this.TableContext = new TableContext(this, metaData);
                    this.AttachDomainHost();
                    base.UpdateLockParent();
                    base.UpdateAccessParent();
                    base.ResetDataBase(authentication);
                    this.DataBaseContext.InvokeItemsResetEvent(authentication, new IDataBase[] { this, });
                });
            }
            else
            {
                base.ResetDataBase(authentication);
                this.DataBaseContext.InvokeItemsResetEvent(authentication, new IDataBase[] { this, });
            }

            //this.IsResetting = false;

        }

        public void SetReset2(Authentication authentication, DataBaseMetaData metaData)
        {
            this.TypeContext = new TypeContext(this, metaData);
            this.TableContext = new TableContext(this, metaData);
            this.AttachDomainHost();
            base.UpdateLockParent();
            base.UpdateAccessParent();
            base.ResetDataBase(authentication);
            this.DataBaseContext.InvokeItemsResetEvent(authentication, new IDataBase[] { this, });
        }

        public void SetAuthenticationEntered(Authentication authentication)
        {
            if (this.Dispatcher != null && this.Dispatcher.CheckAccess() == false)
            {
                this.Dispatcher.Invoke(Action);
            }
            else
            {
                Action();
            }

            void Action()
            {
                this.authenticationInternals.Add(authentication);
                this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                this.DataBaseContext.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
            }
        }

        public void SetAuthenticationLeft(Authentication authentication)
        {
            if (this.Dispatcher != null && this.Dispatcher.CheckAccess() == false)
            {
                this.Dispatcher.Invoke(Action);
            }
            else
            {
                Action();
            }

            void Action()
            {
                this.authenticationInternals.Remove(authentication);
                this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                this.DataBaseContext.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
            }
        }

        public void SetDataBaseInfo(DataBaseInfo dataBaseInfo)
        {
            base.DataBaseInfo = dataBaseInfo;
        }

        public void SetDataBaseState(DataBaseState dataBaseState)
        {
            base.DataBaseState = dataBaseState;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            base.AccessInfo = accessInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetLockInfo(LockInfo lockInfo)
        {
            base.LockInfo = lockInfo;
        }

        public void Invoke(Action action)
        {
            this.Dispatcher.Invoke(action);
        }

        public async Task CloseAsync(CloseInfo closeInfo)
        {
            var result = await this.CremaHost.Dispatcher.InvokeAsync(() =>
            {
                if (this.isDisposed == true)
                    return false;
                this.isDisposed = true;
                return this.Dispatcher.Owner is DataBase;
            });
            if (result == false)
                return;

            await this.Service.UnsubscribeAsync();
            await Task.Delay(100);
            await this.callbackEvent.DisposeAsync();
            await this.Dispatcher.DisposeAsync();
            this.Dispatcher = null;
        }

        public void Delete()
        {
            this.Dispatcher = null;
            base.DataBaseState = DataBaseState.None;
            this.TableContext = null;
            this.TypeContext = null;
            this.OnDeleted(EventArgs.Empty);
        }

        private void AttachDomainHost()
        {
            var domains = this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.GetDomains(this.ID));
            var authentications = this.authenticationInternals.Select(item => (Authentication)item).ToArray();
            var domainHostByDomain = this.FindDomainHosts(domains);
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
                domainHost.Attach(domain);
            }
            this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.AttachDomainHost(authentications, domainHostByDomain));
        }

        public void AttachDomainHost(Domain[] domains)
        {
            this.Dispatcher.VerifyAccess();
            var authentications = this.authenticationInternals.Select(item => (Authentication)item).ToArray();
            var domainHostByDomain = this.FindDomainHosts(domains);
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
                domainHost.Attach(domain);
            }
            this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.AttachDomainHost(authentications, domainHostByDomain));
        }

        private void DetachDomainHost()
        {
            var domains = this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.GetDomains(this.ID));
            var authentications = this.authenticationInternals.Select(item => (Authentication)item).ToArray();
            var domainHostByDomain = domains.ToDictionary(item => item, item => item.Host);
            this.DomainContext.Dispatcher.Invoke(() => this.DomainContext.DetachDomainHost(authentications, domainHostByDomain));
            foreach (var item in domainHostByDomain)
            {
                var domain = item.Key;
                var domainHost = item.Value;
                domainHost.Detach();
            }
        }

        public IDictionary<Domain, IDomainHost> FindDomainHosts(Domain[] domains)
        {
            var dictionary = new Dictionary<Domain, IDomainHost>(domains.Length);
            foreach (var item in domains)
            {
                dictionary.Add(item, this.FindDomainHost(item));
            }
            return dictionary;
        }

        public IDomainHost FindDomainHost(Domain domain)
        {
            var domainInfo = domain.DomainInfo;
            var itemPath = domainInfo.ItemPath;
            var itemType = domainInfo.ItemType;

            if (itemType == nameof(TableContent))
            {
                return new TableContent.TableContentGroup(this.TableContext.Tables, domain, itemPath);
            }
            else if (itemType == nameof(NewTableTemplate))
            {
                if (this.TableContext[itemPath] is TableCategory category)
                {
                    return new NewTableTemplate(category);
                }
                else if (this.TableContext[itemPath] is Table table)
                {
                    return new NewTableTemplate(table);
                }
                throw new NotImplementedException();
            }
            else if (itemType == nameof(TableTemplate))
            {
                var table = this.TableContext[itemPath] as Table;
                return table.Template;
            }
            else if (itemType == nameof(NewTypeTemplate))
            {
                var category = this.TypeContext[itemPath] as TypeCategory;
                return new NewTypeTemplate(category);
            }
            else if (itemType == nameof(TypeTemplate))
            {
                var type = this.TypeContext[itemPath] as Type;
                return type.Template;
            }

            return null;
        }

        public Task<IDomainHost> FindDomainHostAsync(Domain domain)
        {
            var domainInfo = domain.DomainInfo;
            var itemPath = domainInfo.ItemPath;
            var itemType = domainInfo.ItemType;

            return this.Dispatcher.InvokeAsync<IDomainHost>(() =>
            {
                if (itemType == nameof(TableContent))
                {
                    return new TableContent.TableContentGroup(this.TableContext.Tables, domain, itemPath);
                }
                else if (itemType == nameof(NewTableTemplate))
                {
                    if (this.TableContext[itemPath] is TableCategory category)
                    {
                        return new NewTableTemplate(category);
                    }
                    else if (this.TableContext[itemPath] is Table table)
                    {
                        return new NewTableTemplate(table);
                    }
                    throw new NotImplementedException();
                }
                else if (itemType == nameof(TableTemplate))
                {
                    var table = this.TableContext[itemPath] as Table;
                    return table.Template;
                }
                else if (itemType == nameof(NewTypeTemplate))
                {
                    var category = this.TypeContext[itemPath] as TypeCategory;
                    return new NewTypeTemplate(category);
                }
                else if (itemType == nameof(TypeTemplate))
                {
                    var type = this.TypeContext[itemPath] as Type;
                    return type.Template;
                }

                return null;
            });
        }

        public object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(TableContext))
                return this.TableContext;
            else if (serviceType == typeof(TableCategoryCollection))
                return this.TableContext.Categories;
            else if (serviceType == typeof(TableCollection))
                return this.TableContext.Tables;
            else if (serviceType == typeof(TypeContext))
                return this.TypeContext;
            else if (serviceType == typeof(TypeCategoryCollection))
                return this.TypeContext.Categories;
            else if (serviceType == typeof(TypeCollection))
                return this.TypeContext.Types;
            else if (serviceType == typeof(DomainContext))
                return this.CremaHost.DomainContext;
            else
                return this.CremaHost.GetService(serviceType);
        }

        public CremaHost CremaHost => this.DataBaseContext.CremaHost;

        public DataBaseContext DataBaseContext { get; }

        public TableContext TableContext { get; private set; }

        public TypeContext TypeContext { get; private set; }

        public UserContext UserContext => this.CremaHost.UserContext;

        public CremaDispatcher Dispatcher { get; private set; }

        public IDataBaseService Service { get; set; }

        public new DataBaseInfo DataBaseInfo => base.DataBaseInfo;

        public new DataBaseState DataBaseState => base.DataBaseState;

        public AuthenticationInfo[] AuthenticationInfos => this.authenticationInternals.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();

        public override TypeCategoryBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext> TypeCategory => this.TypeContext?.Root;

        public override TableCategoryBase<Table, TableCategory, TableCollection, TableCategoryCollection, TableContext> TableCategory => this.TableContext?.Root;

        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }

        public bool IsLoaded => this.DataBaseState.HasFlag(DataBaseState.Loaded);

        public bool IsResetting { get; private set; }

        public new bool IsLocked => base.IsLocked;

        public new bool IsPrivate => base.IsPrivate;

        public Guid ID => base.DataBaseInfo.ID;

        public DomainContext DomainContext => this.CremaHost.DomainContext;

        public new event EventHandler Renamed
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Renamed -= value;
            }
        }

        public new event EventHandler Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Deleted -= value;
            }
        }

        public new event EventHandler Loaded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Loaded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Loaded -= value;
            }
        }

        public new event EventHandler Unloaded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Unloaded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Unloaded -= value;
            }
        }

        public event EventHandler<AuthenticationEventArgs> AuthenticationEntered
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationEntered += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationEntered -= value;
            }
        }

        public event EventHandler<AuthenticationEventArgs> AuthenticationLeft
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationLeft += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.authenticationLeft -= value;
            }
        }

        public new event EventHandler Resetting
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Resetting += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Resetting -= value;
            }
        }

        public new event EventHandler Reset
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.Reset += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.Reset -= value;
            }
        }

        public new event EventHandler DataBaseInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseInfoChanged -= value;
            }
        }

        public new event EventHandler DataBaseStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DataBaseStateChanged -= value;
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

        protected override void OnDataBaseStateChanged(EventArgs e)
        {
            this.metaData.DataBaseState = base.DataBaseState;
            base.OnDataBaseStateChanged(e);
        }

        protected virtual void OnTaskCompleted(TaskCompletedEventArgs e)
        {
            this.taskCompleted?.Invoke(this, e);
        }

        //private ResultBase ReleaseService()
        //{
        //    var result = this.CremaHost.InvokeService(() => this.service.Unsubscribe());
        //    this.service.CloseService(CloseReason.None);
        //    this.pingTimer.Dispose();
        //    this.pingTimer = null;
        //    this.service = null;
        //    return result;
        //}

        #region IDataBaseEventCallback

        async void IDataBaseEventCallback.OnServiceClosed(CallbackInfo callbackInfo, CloseInfo closeInfo)
        {
            await this.CloseAsync(closeInfo);
        }

        async void IDataBaseEventCallback.OnTablesChanged(CallbackInfo callbackInfo, TableInfo[] tableInfos, string itemType)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var tables = new Table[tableInfos.Length];
                        for (var i = 0; i < tableInfos.Length; i++)
                        {
                            var tableInfo = tableInfos[i];
                            var table = this.TableContext.Tables[tableInfo.Name];
                            table.SetTableInfo(tableInfo);
                            tables[i] = table;
                        }
                        if (itemType == DomainItemType.TableTemplate)
                        {
                            this.TableContext.Tables.InvokeTablesTemplateChangedEvent(authentication, tables);
                        }
                        else
                        {
                            this.TableContext.Tables.InvokeTablesContentChangedEvent(authentication, tables);
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTablesStateChanged(CallbackInfo callbackInfo, string[] tableNames, TableState[] states)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var tables = new Table[tableNames.Length];
                        for (var i = 0; i < tableNames.Length; i++)
                        {
                            var table = this.TableContext.Tables[tableNames[i]];
                            var state = states[i];
                            table.TableState = state;
                            tables[i] = table;
                        }
                        this.TableContext.Tables.InvokeTablesStateChangedEvent(authentication, tables);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTableItemsCreated(CallbackInfo callbackInfo, string[] itemPaths, TableInfo?[] args)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var tableItems = new ITableItem[itemPaths.Length];
                        var categories = new List<TableCategory>();
                        var tables = new List<Table>();

                        for (var i = 0; i < itemPaths.Length; i++)
                        {
                            var itemPath = itemPaths[i];
                            if (NameValidator.VerifyCategoryPath(itemPath) == true)
                            {
                                var categoryName = new CategoryName(itemPath);
                                var category = this.TableContext.Categories.Prepare(itemPath);
                                categories.Add(category);
                                tableItems[i] = category;
                            }
                            else
                            {
                                var tableInfo = (TableInfo)args[i];
                                var table = this.TableContext.Tables.AddNew(authentication, tableInfo.Name, tableInfo.CategoryPath);
                                table.Initialize(tableInfo);
                                tables.Add(table);
                                tableItems[i] = table;
                            }
                        }

                        if (categories.Any() == true)
                        {
                            this.TableContext.Categories.InvokeCategoriesCreatedEvent(authentication, categories.ToArray());
                        }

                        if (tables.Any() == true)
                        {
                            this.TableContext.Tables.InvokeTablesCreatedEvent(authentication, tables.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTableItemsRenamed(CallbackInfo callbackInfo, string[] itemPaths, string[] newNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        {
                            var items = new List<TableCategory>(itemPaths.Length);
                            var oldNames = new List<string>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is TableCategory == false)
                                    continue;

                                var category = tableItem as TableCategory;
                                items.Add(category);
                                oldNames.Add(category.Name);
                                oldPaths.Add(category.Path);
                            }

                            if (items.Any() == true)
                            {
                                for (var i = 0; i < itemPaths.Length; i++)
                                {
                                    var tableItem = this.TableContext[itemPaths[i]];
                                    if (tableItem is TableCategory == false)
                                        continue;

                                    var category = tableItem as TableCategory;
                                    var categoryName = newNames[i];
                                    category.SetName(categoryName);
                                }

                                this.TableContext.Categories.InvokeCategoriesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                            }
                        }

                        {
                            var items = new List<Table>(itemPaths.Length);
                            var oldNames = new List<string>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is Table == false)
                                    continue;

                                var table = tableItem as Table;
                                items.Add(table);
                                oldNames.Add(table.Name);
                                oldPaths.Add(table.Path);
                            }

                            if (items.Any() == true)
                            {
                                for (var i = 0; i < itemPaths.Length; i++)
                                {
                                    var tableItem = this.TableContext[itemPaths[i]];
                                    if (tableItem is Table == false)
                                        continue;

                                    var table = tableItem as Table;
                                    var tableName = newNames[i];
                                    table.SetName(tableName);
                                }

                                this.TableContext.Tables.InvokeTablesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                            }
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTableItemsMoved(CallbackInfo callbackInfo, string[] itemPaths, string[] parentPaths)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        {
                            var items = new List<TableCategory>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);
                            var oldParentPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is TableCategory == false)
                                    continue;

                                var category = tableItem as TableCategory;
                                items.Add(category);
                                oldPaths.Add(category.Path);
                                oldParentPaths.Add(category.Parent.Path);
                            }

                            if (items.Any() == true)
                            {
                                for (var i = 0; i < itemPaths.Length; i++)
                                {
                                    var tableItem = this.TableContext[itemPaths[i]];
                                    if (tableItem is TableCategory == false)
                                        continue;

                                    var category = tableItem as TableCategory;
                                    var parent = this.TableContext.Categories[parentPaths[i]];
                                    category.SetParent(parent);
                                }

                                this.TableContext.Categories.InvokeCategoriesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                            }
                        }

                        {
                            var items = new List<Table>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);
                            var oldParentPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is Table == false)
                                    continue;

                                var table = tableItem as Table;
                                items.Add(table);
                                oldPaths.Add(table.Path);
                                oldParentPaths.Add(table.Category.Path);
                            }

                            if (items.Any() == true)
                            {
                                for (var i = 0; i < itemPaths.Length; i++)
                                {
                                    var tableItem = this.TableContext[itemPaths[i]];
                                    if (tableItem is Table == false)
                                        continue;

                                    var table = tableItem as Table;
                                    var parent = this.TableContext.Categories[parentPaths[i]];
                                    table.SetParent(parent);
                                }

                                this.TableContext.Tables.InvokeTablesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                            }
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTableItemsDeleted(CallbackInfo callbackInfo, string[] itemPaths)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        {
                            var items = new List<TableCategory>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is TableCategory == false)
                                    continue;

                                var category = tableItem as TableCategory;
                                items.Add(category);
                                oldPaths.Add(category.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is TableCategory == false)
                                    continue;

                                var category = tableItem as TableCategory;
                                category.Dispose();
                            }

                            this.TableContext.Categories.InvokeCategoriesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                        }

                        {
                            var items = new List<Table>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is Table == false)
                                    continue;

                                var table = tableItem as Table;
                                items.Add(table);
                                oldPaths.Add(table.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var tableItem = this.TableContext[itemPaths[i]];
                                if (tableItem is Table == false)
                                    continue;

                                var table = tableItem as Table;
                                table.Dispose();
                            }

                            this.TableContext.Tables.InvokeTablesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTableItemsAccessChanged(CallbackInfo callbackInfo, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var tableItems = new ITableItem[accessInfos.Length];
                        for (var i = 0; i < accessInfos.Length; i++)
                        {
                            var accessInfo = accessInfos[i];
                            var tableItem = this.TableContext[accessInfo.Path];
                            if (tableItem is Table table)
                            {
                                table.SetAccessInfo(changeType, accessInfo);
                            }
                            else if (tableItem is TableCategory category)
                            {
                                category.SetAccessInfo(changeType, accessInfo);
                            }
                            else
                            {
                                throw new NotImplementedException(accessInfo.Path);
                            }
                            tableItems[i] = tableItem as ITableItem;
                        }
                        switch (changeType)
                        {
                            case AccessChangeType.Public:
                                this.TableContext.InvokeItemsSetPublicEvent(authentication, tableItems);
                                break;
                            case AccessChangeType.Private:
                                this.TableContext.InvokeItemsSetPrivateEvent(authentication, tableItems);
                                break;
                            case AccessChangeType.Add:
                                this.TableContext.InvokeItemsAddAccessMemberEvent(authentication, tableItems, memberIDs, accessTypes);
                                break;
                            case AccessChangeType.Set:
                                this.TableContext.InvokeItemsSetAccessMemberEvent(authentication, tableItems, memberIDs, accessTypes);
                                break;
                            case AccessChangeType.Remove:
                                this.TableContext.InvokeItemsRemoveAccessMemberEvent(authentication, tableItems, memberIDs);
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

        async void IDataBaseEventCallback.OnTableItemsLockChanged(CallbackInfo callbackInfo, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var tableItems = new ITableItem[lockInfos.Length];
                        for (var i = 0; i < lockInfos.Length; i++)
                        {
                            var lockInfo = lockInfos[i];
                            var tableItem = this.TableContext[lockInfo.Path];
                            if (tableItem is Table table)
                            {
                                table.SetLockInfo(changeType, lockInfo);
                            }
                            else if (tableItem is TableCategory category)
                            {
                                category.SetLockInfo(changeType, lockInfo);
                            }
                            else
                            {
                                throw new NotImplementedException(lockInfo.Path);
                            }
                            tableItems[i] = tableItem as ITableItem;
                        }
                        switch (changeType)
                        {
                            case LockChangeType.Lock:
                                this.TableContext.InvokeItemsLockedEvent(authentication, tableItems, comments);
                                break;
                            case LockChangeType.Unlock:
                                this.TableContext.InvokeItemsUnlockedEvent(authentication, tableItems);
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

        async void IDataBaseEventCallback.OnTypesChanged(CallbackInfo callbackInfo, TypeInfo[] typeInfos)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var types = new Type[typeInfos.Length];
                        for (var i = 0; i < typeInfos.Length; i++)
                        {
                            var typeInfo = typeInfos[i];
                            var type = this.TypeContext.Types[typeInfo.Name];
                            type.SetTypeInfo(typeInfo);
                            types[i] = type;
                        }
                        this.TypeContext.Types.InvokeTypesChangedEvent(authentication, types);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTypesStateChanged(CallbackInfo callbackInfo, string[] typeNames, TypeState[] states)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var types = new Type[typeNames.Length];
                        for (var i = 0; i < typeNames.Length; i++)
                        {
                            var type = this.TypeContext.Types[typeNames[i]];
                            var state = states[i];
                            type.SetTypeState(state);
                            types[i] = type;
                        }
                        this.TypeContext.Types.InvokeTypesStateChangedEvent(authentication, types);
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTypeItemsCreated(CallbackInfo callbackInfo, string[] itemPaths, TypeInfo?[] args)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var typeItems = new ITypeItem[itemPaths.Length];
                        var categories = new List<TypeCategory>();
                        var types = new List<Type>();

                        for (var i = 0; i < itemPaths.Length; i++)
                        {
                            var itemPath = itemPaths[i];
                            if (NameValidator.VerifyCategoryPath(itemPath) == true)
                            {
                                var categoryName = new CategoryName(itemPath);
                                var category = this.TypeContext.Categories.Prepare(itemPath);
                                categories.Add(category);
                                typeItems[i] = category;
                            }
                            else
                            {
                                var typeInfo = (TypeInfo)args[i];
                                var type = this.TypeContext.Types.AddNew(authentication, typeInfo.Name, typeInfo.CategoryPath);
                                type.Initialize(typeInfo);
                                types.Add(type);
                                typeItems[i] = type;
                            }
                        }

                        if (categories.Any() == true)
                        {
                            this.TypeContext.Categories.InvokeCategoriesCreatedEvent(authentication, categories.ToArray());
                        }

                        if (types.Any() == true)
                        {
                            this.TypeContext.Types.InvokeTypesCreatedEvent(authentication, types.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTypeItemsRenamed(CallbackInfo callbackInfo, string[] itemPaths, string[] newNames)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        {
                            var items = new List<TypeCategory>(itemPaths.Length);
                            var oldNames = new List<string>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is TypeCategory == false)
                                    continue;

                                var category = typeItem as TypeCategory;
                                items.Add(category);
                                oldNames.Add(category.Name);
                                oldPaths.Add(category.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is TypeCategory == false)
                                    continue;

                                var category = typeItem as TypeCategory;
                                category.SetName(newNames[i]);
                            }

                            this.TypeContext.Categories.InvokeCategoriesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                        }

                        {
                            var items = new List<Type>(itemPaths.Length);
                            var oldNames = new List<string>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is Type == false)
                                    continue;

                                var type = typeItem as Type;
                                items.Add(type);
                                oldNames.Add(type.Name);
                                oldPaths.Add(type.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is Type == false)
                                    continue;

                                var type = typeItem as Type;
                                type.SetName(newNames[i]);
                            }

                            this.TypeContext.Types.InvokeTypesRenamedEvent(authentication, items.ToArray(), oldNames.ToArray(), oldPaths.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTypeItemsMoved(CallbackInfo callbackInfo, string[] itemPaths, string[] parentPaths)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        {
                            var items = new List<TypeCategory>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);
                            var oldParentPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is TypeCategory == false)
                                    continue;

                                var category = typeItem as TypeCategory;
                                items.Add(category);
                                oldPaths.Add(category.Path);
                                oldParentPaths.Add(category.Parent.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is TypeCategory == false)
                                    continue;

                                var category = typeItem as TypeCategory;
                                var parent = this.TypeContext.Categories[parentPaths[i]];
                                category.SetParent(parent);
                            }

                            this.TypeContext.Categories.InvokeCategoriesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                        }

                        {
                            var items = new List<Type>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);
                            var oldParentPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is Type == false)
                                    continue;

                                var type = typeItem as Type;
                                items.Add(type);
                                oldPaths.Add(type.Path);
                                oldParentPaths.Add(type.Category.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is Type == false)
                                    continue;

                                var type = typeItem as Type;
                                var parent = this.TypeContext.Categories[parentPaths[i]];
                                type.SetParent(parent);
                            }

                            this.TypeContext.Types.InvokeTypesMovedEvent(authentication, items.ToArray(), oldPaths.ToArray(), oldParentPaths.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTypeItemsDeleted(CallbackInfo callbackInfo, string[] itemPaths)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        {
                            var items = new List<TypeCategory>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is TypeCategory == false)
                                    continue;

                                var category = typeItem as TypeCategory;
                                items.Add(category);
                                oldPaths.Add(category.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is TypeCategory == false)
                                    continue;

                                var category = typeItem as TypeCategory;
                                category.Dispose();
                            }

                            this.TypeContext.Categories.InvokeCategoriesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                        }

                        {
                            var items = new List<Type>(itemPaths.Length);
                            var oldPaths = new List<string>(itemPaths.Length);

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is Type == false)
                                    continue;

                                var type = typeItem as Type;
                                items.Add(type);
                                oldPaths.Add(type.Path);
                            }

                            for (var i = 0; i < itemPaths.Length; i++)
                            {
                                var typeItem = this.TypeContext[itemPaths[i]];
                                if (typeItem is Type == false)
                                    continue;

                                var type = typeItem as Type;
                                type.Dispose();
                            }

                            this.TypeContext.Types.InvokeTypesDeletedEvent(authentication, items.ToArray(), oldPaths.ToArray());
                        }
                    });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        async void IDataBaseEventCallback.OnTypeItemsAccessChanged(CallbackInfo callbackInfo, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var typeItems = new ITypeItem[accessInfos.Length];
                        for (var i = 0; i < accessInfos.Length; i++)
                        {
                            var accessInfo = accessInfos[i];
                            var typeItem = this.TypeContext[accessInfo.Path];
                            if (typeItem is Type type)
                            {
                                type.SetAccessInfo(changeType, accessInfo);
                            }
                            else if (typeItem is TypeCategory category)
                            {
                                category.SetAccessInfo(changeType, accessInfo);
                            }
                            else
                            {
                                throw new NotImplementedException(accessInfo.Path);
                            }
                            typeItems[i] = typeItem as ITypeItem;
                        }
                        switch (changeType)
                        {
                            case AccessChangeType.Public:
                                this.TypeContext.InvokeItemsSetPublicEvent(authentication, typeItems);
                                break;
                            case AccessChangeType.Private:
                                this.TypeContext.InvokeItemsSetPrivateEvent(authentication, typeItems);
                                break;
                            case AccessChangeType.Add:
                                this.TypeContext.InvokeItemsAddAccessMemberEvent(authentication, typeItems, memberIDs, accessTypes);
                                break;
                            case AccessChangeType.Set:
                                this.TypeContext.InvokeItemsSetAccessMemberEvent(authentication, typeItems, memberIDs, accessTypes);
                                break;
                            case AccessChangeType.Remove:
                                this.TypeContext.InvokeItemsRemoveAccessMemberEvent(authentication, typeItems, memberIDs);
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

        async void IDataBaseEventCallback.OnTypeItemsLockChanged(CallbackInfo callbackInfo, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            try
            {
                await this.callbackEvent.InvokeAsync(callbackInfo.Index, () =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var authentication = this.UserContext.Authenticate(callbackInfo.SignatureDate);
                        var typeItems = new ITypeItem[lockInfos.Length];
                        for (var i = 0; i < lockInfos.Length; i++)
                        {
                            var lockInfo = lockInfos[i];
                            var typeItem = this.TypeContext[lockInfo.Path];
                            if (typeItem is Type type)
                            {
                                type.SetLockInfo(changeType, lockInfo);
                            }
                            else if (typeItem is TypeCategory category)
                            {
                                category.SetLockInfo(changeType, lockInfo);
                            }
                            else
                            {
                                throw new NotImplementedException(lockInfo.Path);
                            }
                            typeItems[i] = typeItem as ITypeItem;
                        }
                        switch (changeType)
                        {
                            case LockChangeType.Lock:
                                this.TypeContext.InvokeItemsLockedEvent(authentication, typeItems, comments);
                                break;
                            case LockChangeType.Unlock:
                                this.TypeContext.InvokeItemsUnlockedEvent(authentication, typeItems);
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

        async void IDataBaseEventCallback.OnTaskCompleted(CallbackInfo callbackInfo, Guid[] taskIDs)
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

        #region IDataBase

        Task IDataBase.LoadAsync(Authentication authentication)
        {
            return this.LoadAsync(authentication);
        }

        Task IDataBase.UnloadAsync(Authentication authentication)
        {
            return this.UnloadAsync(authentication);
        }

        Task IDataBase.EnterAsync(Authentication authentication)
        {
            return this.EnterAsync(authentication);
        }

        Task IDataBase.LeaveAsync(Authentication authentication)
        {
            return this.LeaveAsync(authentication);
        }

        Task IDataBase.RenameAsync(Authentication authentication, string name)
        {
            return this.RenameAsync(authentication, name);
        }

        Task IDataBase.DeleteAsync(Authentication authentication)
        {
            return this.DeleteAsync(authentication);
        }

        Task IDataBase.RevertAsync(Authentication authentication, string revision)
        {
            return this.RevertAsync(authentication, revision);
        }

        Task IDataBase.ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            return this.ImportAsync(authentication, dataSet, comment);
        }

        async Task<IDataBase> IDataBase.CopyAsync(Authentication authentication, string newDataBaseName, string comment, bool force)
        {
            return await this.DataBaseContext.CopyDataBaseAsync(authentication, this, newDataBaseName, comment, force);
        }

        async Task<ITransaction> IDataBase.BeginTransactionAsync(Authentication authentication)
        {
            return await this.BeginTransactionAsync(authentication);
        }

        ITableContext IDataBase.TableContext => this.TableContext;

        ITypeContext IDataBase.TypeContext => this.TypeContext;

        #endregion

        #region IAccessible

        Task IAccessible.SetPublicAsync(Authentication authentication)
        {
            return this.SetPublicAsync(authentication);
        }

        Task IAccessible.SetPrivateAsync(Authentication authentication)
        {
            return this.SetPrivateAsync(authentication);
        }

        Task IAccessible.AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            return this.AddAccessMemberAsync(authentication, memberID, accessType);
        }

        Task IAccessible.SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            return this.SetAccessMemberAsync(authentication, memberID, accessType);
        }

        Task IAccessible.RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            return this.RemoveAccessMemberAsync(authentication, memberID);
        }

        #endregion

        #region ILockable

        Task ILockable.LockAsync(Authentication authentication, string comment)
        {
            return this.LockAsync(authentication, comment);
        }

        Task ILockable.UnlockAsync(Authentication authentication)
        {
            return this.UnlockAsync(authentication);
        }

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IDataBase))
                return this;

            if (base.DataBaseState.HasFlag(DataBaseState.Loaded) == true && this.Dispatcher.Owner is DataBase)
            {
                if (serviceType == typeof(ITableContext))
                    return this.TableContext;
                else if (serviceType == typeof(ITableCategoryCollection))
                    return this.TableContext.Categories;
                else if (serviceType == typeof(ITableCollection))
                    return this.TableContext.Tables;
                else if (serviceType == typeof(ITypeContext))
                    return this.TypeContext;
                else if (serviceType == typeof(ITypeCategoryCollection))
                    return this.TypeContext.Categories;
                else if (serviceType == typeof(ITypeCollection))
                    return this.TypeContext.Types;
            }

            return this.CremaHost.GetService(serviceType);
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => this.DataBaseInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.DataBaseState;

        #endregion
    }
}

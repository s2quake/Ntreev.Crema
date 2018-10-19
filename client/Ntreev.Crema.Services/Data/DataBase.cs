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
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services.DataBaseService;
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;

namespace Ntreev.Crema.Services.Data
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class DataBase : DataBaseBase<Type, TypeCategory, TypeCollection, TypeCategoryCollection, TypeContext, Table, TableCategory, TableCollection, TableCategoryCollection, TableContext>,
        IDataBaseServiceCallback, IDataBase, ICremaService, IInfoProvider, IStateProvider
    {
        private DataBaseServiceClient service;
        private DataBaseMetaData metaData;
        private Timer timer;

        private EventHandler<AuthenticationEventArgs> authenticationEntered;
        private EventHandler<AuthenticationEventArgs> authenticationLeft;

        private readonly HashSet<AuthenticationToken> authentications = new HashSet<AuthenticationToken>();

        public DataBase(DataBaseCollection dataBases, DataBaseInfo dataBaseInfo)
        {
            this.DataBases = dataBases;
            this.Dispatcher = dataBases.Dispatcher;
            base.Name = dataBaseInfo.Name;
            base.DataBaseInfo = dataBaseInfo;
        }

        public DataBase(DataBaseCollection dataBases, DataBaseMetaData metaData)
        {
            this.DataBases = dataBases;
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
                    this.authentications.Add(authentication);
                }
            }
        }

        public override string ToString()
        {
            return base.Name;
        }

        public AccessType GetAccessType(Authentication authentication)
        {
            this.ValidateExpired();
            return base.GetAccessType(authentication);
        }

        public async Task SetPublicAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPublicAsync), this);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.SetPublic(name));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.SetPublic(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetPublicEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetPrivateAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPrivateAsync), this);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.SetPrivate(name));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.SetPrivate(authentication);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetPrivateEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task AddAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(AddAccessMember), this, memberID, accessType);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.AddAccessMember(name, memberID, accessType));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.AddAccessMember(authentication, memberID, accessType);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsAddAccessMemberEvent(authentication, new IDataBase[] { this }, new string[] { memberID, }, new AccessType[] { accessType, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetAccessMemberAsync(Authentication authentication, string memberID, AccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetAccessMemberAsync), this, memberID, accessType);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.SetAccessMember(name, memberID, accessType));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.SetAccessMember(authentication, memberID, accessType);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsSetAccessMemberEvent(authentication, new IDataBase[] { this }, new string[] { memberID, }, new AccessType[] { accessType, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RemoveAccessMemberAsync(Authentication authentication, string memberID)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
               {
                   this.CremaHost.DebugMethod(authentication, this, nameof(RemoveAccessMemberAsync), this, memberID);
                   return base.Name;
               });
                var result = await Task.Run(() => this.DataBases.Service.RemoveAccessMember(name, memberID));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.RemoveAccessMember(authentication, memberID);
                    this.metaData.AccessInfo = base.AccessInfo;
                    this.DataBases.InvokeItemsRemoveAccessMemberEvent(authentication, new IDataBase[] { this }, new string[] { memberID, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task LockAsync(Authentication authentication, string comment)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LockAsync), this);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.Lock(name, comment));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Lock(authentication, comment);
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBases.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { comment, });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task UnlockAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnlockAsync), this);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.Unlock(name));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Unlock(authentication);
                    this.metaData.LockInfo = base.LockInfo;
                    this.DataBases.InvokeItemsUnlockedEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task LoadAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LoadAsync), this);
                });
                var result = await Task.Run(() => this.DataBases.Service.Load(base.Name));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.metaData.DataBaseState = DataBaseState.Loaded;
                    base.DataBaseState = DataBaseState.Loaded;
                    base.Load(authentication);
                    this.DataBases.InvokeItemsLoadedEvent(authentication, new IDataBase[] { this });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task UnloadAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(UnloadAsync), this);
                });
                var result = await Task.Run(() => this.DataBases.Service.Unload(base.Name));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.authentications.Clear();
                    this.TableContext?.Dispose();
                    this.TableContext = null;
                    this.TypeContext?.Dispose();
                    this.TypeContext = null;
                    this.metaData.DataBaseState = DataBaseState.None;
                    base.DataBaseState = DataBaseState.None;
                    base.Unload(authentication);
                    this.DataBases.InvokeItemsUnloadedEvent(authentication, new IDataBase[] { this });
                });
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
                   return this.authentications.Any(item => ((Authentication)item).ID == authentication.ID) && this.Dispatcher.Owner is DataBase == false;
               });

                if (value == true)
                {
                    this.Dispatcher = new CremaDispatcher(this);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.service = DataServiceFactory.CreateServiceClient(this.CremaHost.IPAddress, this.CremaHost.ServiceInfos[nameof(DataBaseService)], this);
                        this.service.Open();
                        if (this.service is ICommunicationObject service)
                        {
                            service.Faulted += Service_Faulted;
                        }
                        var result = this.service.Subscribe(this.CremaHost.AuthenticationToken, base.Name);
#if !DEBUG
                        this.timer = new Timer(30000);
                        this.timer.Elapsed += Timer_Elapsed;
                        this.timer.Start();
#endif
                        var metaData = result.GetValue();
                        this.CremaHost.Sign(authentication, result);
                        this.TypeContext = new TypeContext(this, metaData);
                        this.TableContext = new TableContext(this, metaData);
                        base.UpdateAccessParent();
                        base.UpdateLockParent();
                        this.AttachDomainHost();
                        this.CremaHost.AddService(this);
                        this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                        this.DataBases.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
                        return true;
                    });
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
                    return this.authentications.Any(item => ((Authentication)item).ID == authentication.ID) == false && this.Dispatcher.Owner is DataBase;
                });
                if (value == true)
                {
                    var signatureDate = await this.ReleaseServiceAsync();
                    authentication.SignatureDate = signatureDate;
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.DetachDomainHost();
                        this.TypeContext.Dispose();
                        this.TypeContext = null;
                        this.TableContext.Dispose();
                        this.TableContext = null;
                        this.Dispatcher.Dispose();
                        this.Dispatcher = this.DataBases.Dispatcher;
                        this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                        this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
                    });
                }
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RenameAsync(Authentication authentication, string name)
        {
            try
            {
                this.ValidateExpired();
                var oldName = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RenameAsync), this, name);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.Rename(oldName, name));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.Name = name;
                    this.metaData.DataBaseInfo = base.DataBaseInfo;
                    this.DataBases.InvokeItemsRenamedEvent(authentication, new DataBase[] { this }, new string[] { oldName });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task DeleteAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), this);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.Delete(name));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    base.DataBaseState = DataBaseState.None;
                    this.DataBases.InvokeItemsDeletedEvent(authentication, new DataBase[] { this }, new string[] { name });
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<bool> ContainsAsync(Authentication authentication)
        {
            return await this.Dispatcher.InvokeAsync(() => this.authentications.Contains(authentication));
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
                var result = await Task.Run(() => this.DataBases.Service.GetLog(name, revision));
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    return result.GetValue();
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task RevertAsync(Authentication authentication, string revision)
        {
            try
            {
                this.ValidateExpired();
                var name = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RevertAsync), this, revision);
                    return base.Name;
                });
                var result = await Task.Run(() => this.DataBases.Service.Revert(name, revision));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(ImportAsync), this, comment);
                });
                var result = await Task.Run(() => this.Service.ImportDataSet(dataSet, comment));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                });
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(GetDataSetAsync), this, dataSetType, filterExpression, revision);
                });
                var result = await Task.Run(() => this.Service.GetDataSet(dataSetType, filterExpression, revision));
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    return result.GetValue();
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DataBaseTransaction> BeginTransactionAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                return await await this.Dispatcher.InvokeAsync(async () =>
                {
                    var result = await Task.Run(() => this.DataBases.Service.BeginTransaction(base.Name));
                    this.CremaHost.Sign(authentication, result);
                    if (this.IsLocked == false)
                    {
                        base.Lock(authentication, $"{this.ID}");
                        this.DataBases.InvokeItemsLockedEvent(authentication, new IDataBase[] { this }, new string[] { $"{this.ID}", });
                    }
                    var transaction = new DataBaseTransaction(authentication, this, this.DataBases.Service);
                    transaction.Disposed += (s, e) =>
                    {
                        this.Dispatcher.InvokeAsync(() =>
                        {
                            if (this.LockInfo.Comment == $"{this.ID}" && this.IsLocked == true)
                            {
                                base.Unlock(authentication);
                                this.DataBases.InvokeItemsUnlockedEvent(authentication, new IDataBase[] { this });
                            }
                        });
                    };
                    return transaction;
                });
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

        public void SetUnloaded(Authentication authentication)
        {
            if (this.Dispatcher.Owner is DataBase)
            {
                this.DetachDomainHost();
                this.ReleaseServiceAsync();
            }
            this.authentications.Clear();
            this.TableContext?.Dispose();
            this.TableContext = null;
            this.TypeContext?.Dispose();
            this.TypeContext = null;
            base.DataBaseState = DataBaseState.None;
            base.Unload(authentication);
        }

        public void SetResetting(Authentication authentication)
        {
            this.TypeContext?.Dispose();
            this.TableContext?.Dispose();
            this.IsResetting = true;
            base.ResettingDataBase(authentication);
            this.DataBases.InvokeItemsResettingEvent(authentication, new IDataBase[] { this, });

            if (this.Dispatcher.Owner is DataBase)
            {
                this.DetachDomainHost();
            }

            this.DomainContext.DeleteDomains(authentication, this.ID);
        }

        // TODO: SetReset
        public void SetReset(Authentication authentication, DomainMetaData[] metaDatas)
        {
            throw new NotImplementedException();
            //var domains = metaDatas.Where(item => item.DomainInfo.DataBaseID == this.ID).ToArray();
            //await this.CremaHost.DomainContext.AddDomainsAsync(domains);
            //if (this.Dispatcher.Owner is DataBase)
            //{
            //    var result = await Task.Run(() => this.service.GetMetaData());
            //    this.TypeContext = new TypeContext(this, result.GetValue());
            //    this.TableContext = new TableContext(this, result.GetValue());
            //    this.AttachDomainHost();
            //    base.UpdateLockParent();
            //    base.UpdateAccessParent();
            //}

            //this.IsResetting = false;
            //base.ResetDataBase(authentication);
            //this.DataBases.InvokeItemsResetEvent(authentication, new IDataBase[] { this, });
        }

        public void SetAuthenticationEntered(Authentication authentication)
        {
            this.ValidateExpired();
            this.Dispatcher.Invoke(() =>
            {
                this.authentications.Add(authentication);
                this.authenticationEntered?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                this.DataBases.InvokeItemsAuthenticationEnteredEvent(authentication, new IDataBase[] { this });
            });
        }

        public void SetAuthenticationLeft(Authentication authentication)
        {
            this.ValidateExpired();
            this.Dispatcher.Invoke(() =>
            {
                this.authentications.Remove(authentication);
                this.authenticationLeft?.Invoke(this, new AuthenticationEventArgs(authentication.AuthenticationInfo));
                this.DataBases.InvokeItemsAuthenticationLeftEvent(authentication, new IDataBase[] { this });
            });
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
        public void SetAccessInfo(AccessChangeType changeType, AccessInfo accessInfo)
        {
            if (changeType != AccessChangeType.Public)
                base.AccessInfo = accessInfo;
            else
                base.AccessInfo = AccessInfo.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetAccessInfo(AccessInfo accessInfo)
        {
            base.AccessInfo = accessInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetLockInfo(LockChangeType changeType, LockInfo lockInfo)
        {
            if (this.IsResetting == true)
                return;
            if (changeType == LockChangeType.Lock)
                base.LockInfo = lockInfo;
            else
                base.LockInfo = LockInfo.Empty;
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
            if (this.Dispatcher.Owner is DataBase == false)
                return;
            await this.Dispatcher.DisposeAsync();
            this.service.Unsubscribe();
            this.service.Close();
            this.service = null;
            this.timer?.Dispose();
            this.timer = null;
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
            this.DomainContext.Dispatcher.Invoke(() =>
            {
                var domains = this.DomainContext.GetDomains(this.ID);
                var authentications = this.authentications.Select(item => (Authentication)item).ToArray();
                var domainHostByDomain = this.FindDomainHosts(domains);
                foreach (var item in domainHostByDomain)
                {
                    var domain = item.Key;
                    var domainHost = item.Value;
                    domainHost.Attach(domain);
                }
                this.DomainContext.AttachDomainHost(authentications, domainHostByDomain);
            });
        }

        private void DetachDomainHost()
        {
            this.DomainContext.Dispatcher.Invoke(() =>
            {
                var domains = this.DomainContext.GetDomains(this.ID);
                var authentications = this.authentications.Select(item => (Authentication)item).ToArray();
                var domainHostByDomain = domains.ToDictionary(item => item, item => item.Host);
                this.DomainContext.DetachDomainHost(authentications, domainHostByDomain);
                foreach (var item in domainHostByDomain)
                {
                    var domain = item.Key;
                    var domainHost = item.Value;
                    domainHost.Detach();
                }
            });
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
                return new TableContent.TableContentDomainHost(this.TableContext.Tables, domain, itemPath);
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
                    return new TableContent.TableContentDomainHost(this.TableContext.Tables, domain, itemPath);
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

        public CremaHost CremaHost => this.DataBases.CremaHost;

        public DataBaseCollection DataBases { get; }

        public TableContext TableContext { get; private set; }

        public TypeContext TypeContext { get; private set; }

        public UserContext UserContext => this.CremaHost.UserContext;

        public CremaDispatcher Dispatcher { get; private set; }

        public IDataBaseService Service => this.service;

        public new DataBaseInfo DataBaseInfo => base.DataBaseInfo;

        public new DataBaseState DataBaseState => base.DataBaseState;

        public AuthenticationInfo[] AuthenticationInfos => this.authentications.Select(item => ((Authentication)item).AuthenticationInfo).ToArray();

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

        protected override void OnDataBaseStateChanged(EventArgs e)
        {
            this.metaData.DataBaseState = base.DataBaseState;
            base.OnDataBaseStateChanged(e);
        }

        private async Task<SignatureDate> ReleaseServiceAsync()
        {
            var result = this.service.Unsubscribe();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                this.service.Close();
            else
                this.service.Abort();
            this.timer?.Dispose();
            this.timer = null;
            this.service = null;

            result.Validate();
            await this.CremaHost.RemoveServiceAsync(this);
            return result.SignatureDate;
        }

        private async void Service_Faulted(object sender, EventArgs e)
        {
            this.service.Abort();
            this.service = null;
            this.timer?.Dispose();
            this.timer = null;
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            await this.CremaHost.RemoveServiceAsync(this);
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.timer?.Stop();
            try
            {
                await this.Dispatcher.InvokeAsync(() => this.service.IsAlive());
                this.timer?.Start();
            }
            catch
            {

            }
        }

        #region IDataBaseServiceCallback

        async void IDataBaseServiceCallback.OnServiceClosed(SignatureDate signatureDate, CloseInfo closeInfo)
        {
            this.service.Close();
            this.service = null;
            this.timer?.Dispose();
            this.timer = null;
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            await this.CremaHost.RemoveServiceAsync(this);
        }

        void IDataBaseServiceCallback.OnTablesChanged(SignatureDate signatureDate, TableInfo[] tableInfos)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
                    var tables = new Table[tableInfos.Length];
                    for (var i = 0; i < tableInfos.Length; i++)
                    {
                        var tableInfo = tableInfos[i];
                        var table = this.TableContext.Tables[tableInfo.Name];
                        table.SetTableInfo(tableInfo);
                        tables[i] = table;
                    }
                    this.TableContext.Tables.InvokeTablesTemplateChangedEvent(authentication, tables);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTablesStateChanged(SignatureDate signatureDate, string[] tableNames, TableState[] states)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.ValidateExpired();
                this.Dispatcher.Invoke(() =>
                {
                    var tables = new Table[tableNames.Length];
                    for (var i = 0; i < tableNames.Length; i++)
                    {
                        var table = this.TableContext.Tables[tableNames[i]];
                        var state = states[i];
                        table.SetTableState(state);
                        tables[i] = table;
                    }
                    this.TableContext.Tables.InvokeTablesStateChangedEvent(authentication, tables);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTableItemsCreated(SignatureDate signatureDate, string[] itemPaths, TableInfo?[] args)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int er = 0;
                }
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTableItemsRenamed(SignatureDate signatureDate, string[] itemPaths, string[] newNames)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTableItemsMoved(SignatureDate signatureDate, string[] itemPaths, string[] parentPaths)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTableItemsDeleted(SignatureDate signatureDate, string[] itemPaths)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTableItemsAccessChanged(SignatureDate signatureDate, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTableItemsLockChanged(SignatureDate signatureDate, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypesChanged(SignatureDate signatureDate, TypeInfo[] typeInfos)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypesStateChanged(SignatureDate signatureDate, string[] typeNames, TypeState[] states)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypeItemsCreated(SignatureDate signatureDate, string[] itemPaths, TypeInfo?[] args)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int er = 0;
                }
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypeItemsRenamed(SignatureDate signatureDate, string[] itemPaths, string[] newNames)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypeItemsMoved(SignatureDate signatureDate, string[] itemPaths, string[] parentPaths)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypeItemsDeleted(SignatureDate signatureDate, string[] itemPaths)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypeItemsAccessChanged(SignatureDate signatureDate, AccessChangeType changeType, AccessInfo[] accessInfos, string[] memberIDs, AccessType[] accessTypes)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        void IDataBaseServiceCallback.OnTypeItemsLockChanged(SignatureDate signatureDate, LockChangeType changeType, LockInfo[] lockInfos, string[] comments)
        {
            try
            {
                var authentication = this.UserContext.Authenticate(signatureDate);
                this.Dispatcher.Invoke(() =>
                {
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
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
            }
        }

        #endregion

        #region IDataBase

        async Task<IDataBase> IDataBase.CopyAsync(Authentication authentication, string newDataBaseName, string comment, bool force)
        {
            return await this.DataBases.CopyDataBaseAsync(authentication, this, newDataBaseName, comment, force);
        }

        async Task<ITransaction> IDataBase.BeginTransactionAsync(Authentication authentication)
        {
            return await this.BeginTransactionAsync(authentication);
        }

        ITableContext IDataBase.TableContext => this.TableContext;

        ITypeContext IDataBase.TypeContext => this.TypeContext;

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

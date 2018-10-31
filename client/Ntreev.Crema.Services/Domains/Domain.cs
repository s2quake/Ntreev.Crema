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
using Ntreev.Crema.Services.DomainService;
using Ntreev.Crema.Services.Properties;
using Ntreev.Crema.Services.Users;
using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains
{
    abstract class Domain : DomainBase<Domain, DomainCategory, DomainCollection, DomainCategoryCollection, DomainContext>,
        IDomain, IDomainItem, IInfoProvider, IStateProvider
    {
        private bool initialized;
        private readonly HashSet<string> modifiedTableList = new HashSet<string>();

        private EventHandler<DomainUserEventArgs> userAdded;
        private EventHandler<DomainUserRemovedEventArgs> userRemoved;
        private EventHandler<DomainUserLocationEventArgs> userLocationChanged;
        private EventHandler<DomainUserEventArgs> userStateChanged;
        private EventHandler<DomainUserLocationEventArgs> userEditBegun;
        private EventHandler<DomainUserEventArgs> userEditEnded;
        private EventHandler<DomainUserEventArgs> ownerChanged;
        private EventHandler<DomainRowEventArgs> rowAdded;
        private EventHandler<DomainRowEventArgs> rowChanged;
        private EventHandler<DomainRowEventArgs> rowRemoved;
        private EventHandler<DomainPropertyEventArgs> propertyChanged;

        private EventHandler<DomainDeletedEventArgs> deleted;

        public CremaResetEvent<long> taskEvent;
        public CremaResetEvent<string> enterEvent;
        public CremaResetEvent<string> leaveEvent;
        private List<long> postList = new List<long>();
        private List<long> postList2 = new List<long>();
        private List<long> completionList = new List<long>();
        private DomainMetaData metaData;

        protected Domain(DomainInfo domainInfo)
        {
            this.Initialize(domainInfo);
            this.Name = domainInfo.DomainID.ToString();
            this.Users = new DomainUserCollection(this);
        }

        public async Task WaitUserEnterAsync(Authentication authentication)
        {
            await this.enterEvent.WaitAsync(authentication.ID);
        }

        public async Task WaitUserLeaveAsync(Authentication authentication)
        {
            await this.leaveEvent.WaitAsync(authentication.ID);
        }

        public async Task<object> DeleteAsync(Authentication authentication, bool isCanceled)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(DeleteAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, isCanceled);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.DeleteDomain(this.ID, isCanceled));
                if (this.Host != null)
                {
                    await this.Host.DeleteAsync(authentication, isCanceled, result.Value);
                    return result.Value;
                }
                else
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        var container = this.Container;
                        this.CremaHost.Sign(authentication, result);
                        this.Dispose();
                        this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled, result.Value));
                        container.InvokeDomainDeletedEvent(authentication, new Domain[] { this }, new bool[] { isCanceled }, new object[] { result.Value });
                    });
                    return null;
                }

            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<long> EnterAsync(Authentication authentication, DomainAccessType accessType)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EnterAsync), this, accessType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.Enter(this.ID, accessType));
                await this.taskEvent.WaitAsync(result.ID);
                return result.ID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<long> LeaveAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(LeaveAsync), this);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.Leave(this.ID));
                await this.taskEvent.WaitAsync(result.ID);
                return result.ID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<long> BeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.BeginUserEdit(this.ID, location));
                await this.taskEvent.WaitAsync(result.ID);
                return result.ID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<long> EndUserEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.EndUserEdit(this.ID));
                await this.taskEvent.WaitAsync(result.ID);
                return result.ID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        //public async Task WaitEventAsync(long id)
        //{
        //    await await this.Dispatcher.InvokeAsync(async () =>
        //    {
        //        if (this.Logger.CompletionID < id)
        //        {
        //            this.setsByID.Add(id, new ManualResetEvent(false));
        //            var set = this.setsByID[id];
        //            await Task.Run(() => set.WaitOne());
        //        }
        //    });
        //}
        //private Task SetEventAsync(long id)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        if (this.setsByID.ContainsKey(id) == true)
        //        {
        //            this.setsByID[id].Set();
        //            this.setsByID.Remove(id);
        //        }
        //    });
        //}

        public async Task<DomainResultInfo<DomainRowInfo[]>> NewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(NewRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.NewRow(this.ID, rows));
                await this.taskEvent.WaitAsync(result.ID);
                return new DomainResultInfo<DomainRowInfo[]>() { ID = result.ID, Value = result.Value };
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }



        public async Task<DomainResultInfo<DomainRowInfo[]>> SetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetRow(this.ID, rows));
                await this.taskEvent.WaitAsync(result.ID);
                return new DomainResultInfo<DomainRowInfo[]>() { ID = result.ID, Value = result.Value };
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DomainResultInfo<DomainRowInfo[]>> RemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(RemoveRowAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.RemoveRow(this.ID, rows));
                await this.taskEvent.WaitAsync(result.ID);
                return new DomainResultInfo<DomainRowInfo[]>() { ID = result.ID, Value = result.Value };
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<long> SetPropertyAsync(Authentication authentication, string propertyName, object value)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetPropertyAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, propertyName, value);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetProperty(this.ID, propertyName, value));
                await this.taskEvent.WaitAsync(result.ID);
                return result.ID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task SetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                var domainUser = await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserLocationAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                    return this.GetDomainUser(authentication);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetUserLocation(this.ID, location));
                await this.Dispatcher.InvokeAsync(() =>
                {
                    domainUser.DomainLocationInfo = location;
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<long> KickAsync(Authentication authentication, string userID, string comment)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(KickAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID, comment);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.Kick(this.ID, userID, comment));
                await this.taskEvent.WaitAsync(result.ID);
                return result.ID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<long> SetOwnerAsync(Authentication authentication, string userID)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetOwnerAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType, userID);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetOwner(this.ID, userID));
                await this.taskEvent.WaitAsync(result.ID);
                return result.ID;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public DomainMetaData GetMetaData(Authentication authentication)
        {
            this.Dispatcher.VerifyAccess();
            if (authentication == null)
                throw new ArgumentNullException(nameof(authentication));
            var metaData = new DomainMetaData()
            {
                DomainID = Guid.Parse(this.Name),
                DomainInfo = base.DomainInfo,
                Users = this.Users.Select<DomainUser, DomainUserMetaData>(item => item.GetMetaData(authentication)).ToArray(),
                DomainState = base.DomainState,
                ModifiedTables = this.modifiedTableList.ToArray(),
            };
            if (this.Users.ContainsKey(authentication.ID) == true)
            {
                metaData.Data = this.SerializeSource();
            }
            return metaData;
        }

        public async Task<DomainMetaData> GetMetaDataAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return await this.Dispatcher.InvokeAsync(() =>
            {
                var metaData = new DomainMetaData()
                {
                    DomainID = Guid.Parse(this.Name),
                    DomainInfo = base.DomainInfo,
                    Users = this.Users.Select<DomainUser, DomainUserMetaData>(item => item.GetMetaData(authentication)).ToArray(),
                    DomainState = this.DomainState,
                    ModifiedTables = this.modifiedTableList.ToArray(),
                };

                if (this.Users.ContainsKey(authentication.ID) == true)
                {
                    metaData.Data = this.SerializeSource();
                }

                return metaData;
            });
        }

        public void Initialize(Authentication authentication, DomainMetaData metaData)
        {
            this.Dispatcher.VerifyAccess();
            if (this.enterEvent != null)
                throw new Exception("123123");

            this.taskEvent = new CremaResetEvent<long>(this.Dispatcher);
            this.enterEvent = new CremaResetEvent<string>(this.Dispatcher);
            this.leaveEvent = new CremaResetEvent<string>(this.Dispatcher);
            base.DomainState = metaData.DomainState;
            this.metaData = metaData;
            this.modifiedTableList.Clear();
            foreach (var item in metaData.ModifiedTables)
            {
                this.modifiedTableList.Add(item);
            }

            foreach (var item in metaData.Users)
            {
                if (item.DomainUserState.HasFlag(DomainUserState.Detached) == true)
                    continue;
                var signatureDate = new SignatureDate(item.DomainUserInfo.UserID, authentication.SignatureDate.DateTime);
                var userAuthentication = this.UserContext.Authenticate(signatureDate);
                var domainUser = new DomainUser(this, item.DomainUserInfo, item.DomainUserState, false);
                this.Users.Add(domainUser);
                this.enterEvent.Set(domainUser.ID);
            }

            if (metaData.Data == null)
            {

            }
            else
            {
                if (metaData.CompetionID != 0 && metaData.PostID != metaData.CompetionID)
                {
                    int qwer = 0;
                }
                this.OnInitialize(metaData.Data);
                if (this.DataDispatcher != null)
                {
                    int qwer = 0;
                }
                this.Logger = new DomainLogger(this)
                {
                    PostID = metaData.PostID,
                    CompletionID = metaData.CompetionID,
                };
                this.initialized = true;
                //foreach (var item in metaData.Users)
                //{
                //    var userInfo = item.DomainUserInfo;
                //    if (this.Users.ContainsKey(item.DomainUserInfo.UserID) == false)
                //    {
                //        var signatureDate = new SignatureDate(item.DomainUserInfo.UserID, authentication.SignatureDate.DateTime);
                //        var userAuthentication = this.UserContext.Authenticate(signatureDate);
                //        this.Users.Add(new DomainUser(this, item.DomainUserInfo, item.DomainUserState, false));
                //    }
                //}
            }
        }

        public async Task InitializeAsync(Authentication authentication, DomainMetaData metaData)
        {
            await this.taskEvent.WaitAsync(metaData.CompetionID);
            //return this.Dispatcher.InvokeAsync(() => this.Initialize(authentication, metaData));
        }

        public UserContext UserContext => this.CremaHost.UserContext;

        public async Task ReleaseAsync(Authentication authentication, DomainMetaData metaData)
        {
            await this.taskEvent.WaitAsync(metaData.CompetionID);
            await this.Dispatcher.InvokeAsync(() =>
            {
                //foreach (var item in this.Users.ToArray<DomainUser>())
                //{
                //    if (metaData.Users.Any(i => i.DomainUserInfo.UserID == item.DomainUserInfo.UserID) == false)
                //    {
                //        this.Users.Remove(item.DomainUserInfo.UserID);
                //        //this.InvokeUserRemoved(authentication, item.DomainUserInfo, RemoveInfo.Empty);
                //    }
                //}
                //foreach (var item in metaData.Users)
                //{
                //    if (item.DomainUserState.HasFlag(DomainUserState.IsOwner) == true)
                //    {
                //        var master = this.Users[item.DomainUserInfo.UserID];
                //        this.Users.Owner = master;
                //        //this.InvokeUserChanged(authentication, item.DomainUserInfo, item.DomainUserState);
                //    }
                //}
                this.Logger?.Dispose();
                this.Logger = null;
                this.OnRelease();
                this.initialized = false;
            });
        }

        public void Dispose(Authentication authentication, bool isCanceled, object result)
        {
            this.Logger?.Dispose();
            this.Logger = null;
            this.Dispose();
            this.OnDeleted(new DomainDeletedEventArgs(authentication, this, isCanceled, result));
        }

        private void Log(long id, string name)
        {
            //this.Dispatcher.Invoke(() =>
            //{
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "debug", this.CremaHost.UserID, "ClientDomainLog.txt");
            Ntreev.Library.IO.FileUtility.Prepare(path);
            System.IO.File.AppendAllText(path, $"{id}\t{DateTime.Now}\t{name}{Environment.NewLine}");
            //});
        }

        //private Task WaitEventAsync(long id)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        if (this.setsByID.ContainsKey(id) == true)
        //        {
        //            this.setsByID[id].Set();
        //            this.setsByID.Remove(id);
        //        }
        //    });
        //}



        //public Task AttachUserAsync()
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        if (this.Users.ContainsKey(this.CremaHost.UserID) == true)
        //        {
        //            var domainUser = this.Users[this.CremaHost.UserID];
        //            domainUser.IsOnline = true;
        //        }
        //    });
        //}

        //public Task DetachUserAsync()
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        if (this.Users.ContainsKey(this.CremaHost.UserID) == true)
        //        {
        //            var domainUser = this.Users[this.CremaHost.UserID];
        //            domainUser.IsOnline = false;
        //        }
        //    });
        //}

        public object GetService(System.Type serviceType)
        {
            return this.CremaHost.GetService(serviceType);
        }

        public void InvokeDomainInfoChanged(Authentication authentication, DomainInfo domainInfo)
        {
            base.UpdateDomainInfo(domainInfo);
            this.Container.InvokeDomainInfoChangedEvent(authentication, this);
        }

        public void InvokeDomainStateChanged(Authentication authentication, DomainState domainState)
        {
            base.DomainState = domainState;
            this.Container.InvokeDomainStateChangedEvent(authentication, this);
        }

        public async void InvokeUserAddedAsync(Authentication authentication, DomainUserInfo domainUserInfo, DomainUserState domainUserState, byte[] data, long id)
        {
            Log(id, $"{nameof(InvokeUserAddedAsync)}: {authentication.ID}");
            //if (id <= this.metaData.CompetionID)
            //    return;
            this.postList.Add(id);
            var domainUser = new DomainUser(this, domainUserInfo, domainUserState, false);
            this.Users.Add(domainUser);
            if (data != null)
            {
                this.Logger = new DomainLogger(this);
                await this.DataDispatcher.InvokeAsync(() =>
                {
                    this.OnInitialize(data);
                    this.initialized = true;
                });
            }
            this.OnUserAdded(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserAddedEvent(authentication, this, domainUser, id);
            this.taskEvent.Set(id);
            this.enterEvent.Set(authentication.ID);
            this.leaveEvent.Reset(authentication.ID);
            this.completionList.Add(id);
        }

        public async void InvokeUserRemovedAsync(Authentication authentication, DomainUser domainUser, DomainUser ownerUser, RemoveInfo removeInfo, long id)
        {
            Log(id, $"{nameof(InvokeUserRemovedAsync)}: {authentication.ID}");
            //if (id <= this.metaData.CompetionID)
            //    return;
            this.postList.Add(id);
            this.Users.Remove(domainUser.ID);
            this.Users.Owner = ownerUser;
            if (domainUser.ID == this.CremaHost.UserID)
            {
                await this.DataDispatcher.InvokeAsync(() =>
                {
                    this.Logger.Dispose();
                    this.Logger = null;
                    this.initialized = false;
                });
            }
            this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, removeInfo));
            this.Container?.InvokeDomainUserRemovedEvent(authentication, this, domainUser, removeInfo, id);
            this.taskEvent.Set(id);
            this.leaveEvent.Set(domainUser.ID);
            this.enterEvent.Reset(authentication.ID);
            this.completionList.Add(id);
        }

        public void InvokeUserLocationChanged(Authentication authentication, DomainUser domainUser, DomainLocationInfo domainLocationInfo)
        {
            domainUser.DomainLocationInfo = domainLocationInfo;
            //this.CremaHost.Sign(authentication);
            this.OnUserLocationChanged(new DomainUserLocationEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserLocationChangedEvent(authentication, this, domainUser);
        }

        public void InvokeUserStateChanged(Authentication authentication, DomainUser domainUser, DomainUserState domainUserState)
        {
            domainUser.DomainUserState = domainUserState;
            //this.CremaHost.Sign(authentication);
            this.OnUserStateChanged(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserStateChangedEvent(authentication, this, domainUser);
        }

        public async void InvokeUserEditBegunAsync(Authentication authentication, DomainUser domainUser, DomainLocationInfo domainLocationInfo, long id)
        {
            Log(id, $"{nameof(InvokeUserEditBegunAsync)}: {authentication.ID}");
            if (id <= this.metaData.CompetionID)
                return;
            this.postList.Add(id);
            await this.DataDispatcher.InvokeAsync(() =>
            {
                //if (id < this.Logger.CompletionID)
                //    return false;
                this.OnBeginUserEdit(domainUser, domainLocationInfo);
                //this.Logger.BeginUserEdit(authentication, domainLocationInfo, id);
                //return true;
            });
            domainUser.DomainLocationInfo = domainLocationInfo;
            domainUser.IsBeingEdited = true;
            //this.CremaHost.Sign(authentication);
            this.OnUserEditBegun(new DomainUserLocationEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserEditBegunEvent(authentication, this, domainUser, id);
            this.taskEvent.Set(id);
            this.completionList.Add(id);
        }

        public async void InvokeUserEditEndedAsync(Authentication authentication, DomainUser domainUser, long id)
        {
            Log(id, $"{nameof(InvokeUserEditEndedAsync)}: {authentication.ID}");
            if (id <= this.metaData.CompetionID)
                return;
            this.postList.Add(id);
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.OnEndUserEdit(domainUser);
            });
            domainUser.IsBeingEdited = false;
            //this.CremaHost.Sign(authentication);
            this.OnUserEditEnded(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainUserEditEndedEvent(authentication, this, domainUser, id);
            this.taskEvent.Set(id);
            this.completionList.Add(id);
        }

        public void InvokeOwnerChangedAsync(Authentication authentication, DomainUser domainUser, long id)
        {
            Log(id, $"{nameof(InvokeOwnerChangedAsync)}: {authentication.ID}");
            if (id <= this.metaData.CompetionID)
                return;
            this.postList.Add(id);
            this.Users.Owner = domainUser;
            this.OnOwnerChanged(new DomainUserEventArgs(authentication, this, domainUser));
            this.Container?.InvokeDomainOwnerChangedEvent(authentication, this, domainUser, id);
            this.taskEvent.Set(id);
            this.completionList.Add(id);
        }

        public async void InvokeRowAddedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, long id)
        {
            Log(id, $"{nameof(InvokeRowAddedAsync)}: {authentication.ID}");
            if (id <= this.metaData.CompetionID)
                return;
            this.postList.Add(id);
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.postList2.Add(id);
                this.OnNewRow(domainUser, rows, authentication.SignatureDate);
            });
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnRowAdded(new DomainRowEventArgs(authentication, this, rows));
            this.Container?.InvokeDomainRowAddedEvent(authentication, this, id, rows);
            this.taskEvent.Set(id);
            this.completionList.Add(id);
        }

        public async void InvokeRowChangedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, long id)
        {
            Log(id, $"{nameof(InvokeRowChangedAsync)}: {authentication.ID}");
            if (id <= this.metaData.CompetionID)
                return;
            this.postList.Add(id);
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.postList2.Add(id);
                var taskID = id;
                this.OnSetRow(domainUser, rows, authentication.SignatureDate);
            });
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnRowChanged(new DomainRowEventArgs(authentication, this, rows));
            this.Container?.InvokeDomainRowChangedEvent(authentication, this, id, rows);
            this.taskEvent.Set(id);
            this.completionList.Add(id);
        }

        public async void InvokeRowRemovedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, long id)
        {
            Log(id, $"{nameof(InvokeRowRemovedAsync)}: {authentication.ID}");
            if (id <= this.metaData.CompetionID)
                return;
            this.postList.Add(id);
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.postList2.Add(id);
                var taskID = id;
                this.OnRemoveRow(domainUser, rows, authentication.SignatureDate);
            });
            foreach (var item in rows)
            {
                this.modifiedTableList.Add(item.TableName);
            }
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnRowRemoved(new DomainRowEventArgs(authentication, this, rows));
            this.Container?.InvokeDomainRowRemovedEvent(authentication, this, id, rows);
            this.taskEvent.Set(id);
            this.completionList.Add(id);
        }

        public async void InvokePropertyChangedAsync(Authentication authentication, DomainUser domainUser, string propertyName, object value, long id)
        {
            Log(id, $"{nameof(InvokePropertyChangedAsync)}: {authentication.ID}");
            this.postList.Add(id);
            await this.DataDispatcher.InvokeAsync(() =>
            {
                this.postList2.Add(id);
                var taskID = id;
                this.OnSetProperty(domainUser, propertyName, value, authentication.SignatureDate);
            });
            domainUser.IsModified = true;
            this.IsModified = true;
            //this.CremaHost.Sign(authentication);
            base.UpdateModificationInfo(authentication.SignatureDate);
            this.OnPropertyChanged(new DomainPropertyEventArgs(authentication, this, propertyName, value));
            this.Container?.InvokeDomainPropertyChangedEvent(authentication, this, id, propertyName, value);
            this.taskEvent.Set(id);
            this.completionList.Add(id);
        }

        public void SetDomainHost(Authentication authentication, IDomainHost host)
        {
            this.Dispatcher.VerifyAccess();
            this.Host = host;
            if (this.Host != null)
            {
                base.DomainState |= DomainState.IsActivated;
            }
            else
            {
                base.DomainState &= ~DomainState.IsActivated;
            }
            this.OnDomainStateChanged(new DomainEventArgs(authentication, this));
            this.Container.InvokeDomainStateChangedEvent(authentication, this);
        }

        public void Attach(params Authentication[] authentications)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in authentications)
            {
                if (this.Users.ContainsKey(item.ID) == true)
                {
                    var domainUser = this.Users[item.ID];
                    domainUser.IsOnline = true;
                    this.OnUserStateChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainUserStateChangedEvent(item, this, domainUser);
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
        }

        //public Task AttachAsync(params Authentication[] authentications)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        foreach (var item in authentications)
        //        {
        //            if (this.Users.ContainsKey(item.ID) == true)
        //            {
        //                this.Sign(item, true);
        //                this.InvokeAttach(item, out var domainUser);
        //                this.OnUserChanged(new DomainUserEventArgs(item, this, domainUser));
        //                this.OnDomainStateChanged(new DomainEventArgs(item, this));
        //                this.Container.InvokeDomainUserChangedEvent(item, this, domainUser);
        //                this.Container.InvokeDomainStateChangedEvent(item, this);
        //            }
        //        }
        //    });
        //}

        public void Detach(params Authentication[] authentications)
        {
            foreach (var item in authentications)
            {
                if (this.Users[item.ID] is DomainUser domainUser && domainUser.IsOnline == true)
                {
                    //this.Sign(item, true);
                    //this.InvokeDetach(item, out var domainUser);
                    this.OnUserStateChanged(new DomainUserEventArgs(item, this, domainUser));
                    this.OnDomainStateChanged(new DomainEventArgs(item, this));
                    this.Container.InvokeDomainUserStateChangedEvent(item, this, domainUser);
                    this.Container.InvokeDomainStateChangedEvent(item, this);
                }
            }
        }

        public abstract object Source { get; }

        public IDomainHost Host { get; set; }

        public CremaHost CremaHost => this.Context.CremaHost;

        public Guid ID => Guid.Parse(this.Name);

        public Guid DataBaseID => base.DomainInfo.DataBaseID;

        public string DomainName => base.DomainInfo.ItemType;

        public DomainUserCollection Users { get; }

        public CremaDispatcher Dispatcher => this.Context?.Dispatcher;

        public CremaDispatcher DataDispatcher => this.Logger?.Dispatcher;

        public DomainLogger Logger { get; set; }

        public string[] ModifiedTables => this.modifiedTableList.OrderBy(item => item).ToArray();

        public event EventHandler<DomainUserEventArgs> UserAdded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userAdded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userAdded -= value;
            }
        }

        public event EventHandler<DomainUserRemovedEventArgs> UserRemoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userRemoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userRemoved -= value;
            }
        }

        public event EventHandler<DomainUserLocationEventArgs> UserLocationChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userLocationChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userLocationChanged -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> UserStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userStateChanged -= value;
            }
        }

        public event EventHandler<DomainUserLocationEventArgs> UserEditBegun
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditBegun += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditBegun -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> UserEditEnded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditEnded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.userEditEnded -= value;
            }
        }

        public event EventHandler<DomainUserEventArgs> OwnerChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.ownerChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.ownerChanged -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowAdded
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowAdded += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowAdded -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowChanged -= value;
            }
        }

        public event EventHandler<DomainRowEventArgs> RowRemoved
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.rowRemoved += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.rowRemoved -= value;
            }
        }

        public event EventHandler<DomainPropertyEventArgs> PropertyChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.propertyChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.propertyChanged -= value;
            }
        }

        public new event EventHandler<DomainDeletedEventArgs> Deleted
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                this.deleted += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                this.deleted -= value;
            }
        }

        public new event EventHandler DomainInfoChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainInfoChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainInfoChanged -= value;
            }
        }

        public new event EventHandler DomainStateChanged
        {
            add
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainStateChanged += value;
            }
            remove
            {
                this.Dispatcher?.VerifyAccess();
                base.DomainStateChanged -= value;
            }
        }

        protected virtual void OnInitialize(byte[] data)
        {

        }

        protected virtual void OnRelease()
        {

        }

        protected virtual void OnNewRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {

        }

        protected virtual void OnSetRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {

        }

        protected virtual void OnRemoveRow(DomainUser domainUser, DomainRowInfo[] rows, SignatureDate signatureDate)
        {

        }

        protected virtual void OnSetProperty(DomainUser domainUser, string propertyName, object value, SignatureDate signatureDate)
        {

        }

        protected virtual void OnBeginUserEdit(DomainUser domainUser, DomainLocationInfo location)
        {

        }

        protected virtual void OnEndUserEdit(DomainUser domainUser)
        {

        }

        protected abstract byte[] SerializeSource();

        protected abstract void DerializeSource(byte[] data);

        protected virtual void OnUserAdded(DomainUserEventArgs e)
        {
            this.userAdded?.Invoke(this, e);
        }

        protected virtual void OnUserRemoved(DomainUserRemovedEventArgs e)
        {
            this.userRemoved?.Invoke(this, e);
        }

        protected virtual void OnUserLocationChanged(DomainUserLocationEventArgs e)
        {
            this.userLocationChanged?.Invoke(this, e);
        }

        protected virtual void OnUserStateChanged(DomainUserEventArgs e)
        {
            this.userStateChanged?.Invoke(this, e);
        }

        protected virtual void OnUserEditBegun(DomainUserLocationEventArgs e)
        {
            this.userEditBegun?.Invoke(this, e);
        }

        protected virtual void OnUserEditEnded(DomainUserEventArgs e)
        {
            this.userEditEnded?.Invoke(this, e);
        }

        protected virtual void OnOwnerChanged(DomainUserEventArgs e)
        {
            this.ownerChanged?.Invoke(this, e);
        }

        protected virtual void OnRowAdded(DomainRowEventArgs e)
        {
            this.rowAdded?.Invoke(this, e);
        }

        protected virtual void OnRowChanged(DomainRowEventArgs e)
        {
            this.rowChanged?.Invoke(this, e);
        }

        protected virtual void OnRowRemoved(DomainRowEventArgs e)
        {
            this.rowRemoved?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(DomainPropertyEventArgs e)
        {
            this.propertyChanged?.Invoke(this, e);
        }

        protected virtual void OnDeleted(DomainDeletedEventArgs e)
        {
            this.deleted?.Invoke(this, e);
        }

        //private async Task<DomainUser> InvokeBeginUserEditAsync(Authentication authentication, DomainUser domainUser, DomainLocationInfo location)
        //{
        //    await this.DataDispatcher.InvokeAsync(() =>
        //    {
        //        this.OnBeginUserEdit(domainUser, location);
        //    });
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        domainUser.Location = location;
        //        domainUser.IsBeingEdited = true;
        //    });
        //    return domainUser;
        //}

        //private async Task<DomainUser> InvokeEndUserEditAsync(Authentication authentication, DomainUser domainUser)
        //{
        //    await this.DataDispatcher.InvokeAsync(() =>
        //    {
        //        this.OnEndUserEdit(domainUser);
        //    });
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        domainUser.IsBeingEdited = false;
        //    });
        //    return domainUser;
        //}

        //private async Task InvokeNewRowAsync(Authentication authentication, DomainResultInfo info)
        //{
        //    var domainUser = await this.GetDomainUserAsync(authentication);
        //    await this.OnNewRowAsync(domainUser, info, authentication.SignatureDate);
        //    await this.Logger.NewRowAsync(authentication, info);
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        foreach (var item in info.Rows)
        //        {
        //            this.modifiedTableList.Add(item.TableName);
        //        }
        //        domainUser.IsModified = true;
        //    });
        //}

        //private async Task InvokeSetRowAsync(Authentication authentication, DomainResultInfo info)
        //{
        //    var domainUser = await this.GetDomainUserAsync(authentication);
        //    await this.OnSetRowAsync(domainUser, info, authentication.SignatureDate);
        //    await this.Logger.SetRowAsync(authentication, info);
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        foreach (var item in info.Rows)
        //        {
        //            this.modifiedTableList.Add(item.TableName);
        //        }
        //        domainUser.IsModified = true;
        //    });
        //}

        //private async Task InvokeRemoveRowAsync(Authentication authentication, DomainResultInfo info)
        //{
        //    var domainUser = await this.GetDomainUserAsync(authentication);
        //    await this.OnRemoveRowAsync(domainUser, info, authentication.SignatureDate);
        //    await this.Logger.RemoveRowAsync(authentication, info);
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        foreach (var item in info.Rows)
        //        {
        //            this.modifiedTableList.Add(item.TableName);
        //        }
        //        domainUser.IsModified = true;
        //    });
        //}

        //private async Task InvokeSetPropertyAsync(Authentication authentication, string propertyName, object value)
        //{
        //    var domainUser = await this.GetDomainUserAsync(authentication);
        //    await this.OnSetPropertyAsync(domainUser, propertyName, value, authentication.SignatureDate);
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        domainUser.IsModified = true;
        //    });
        //}

        //private async Task<DomainUser> InvokeSetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        //{
        //    var domainUser = await this.GetDomainUserAsync(authentication);
        //    await this.OnSetLocationAsync(domainUser, location);
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        domainUser.Location = location;
        //    });
        //    return domainUser;
        //}

        //private async Task<(DomainUser, RemoveInfo)> InvokeKickAsync(Authentication authentication, string userID, string comment)
        //{
        //    var removeInfo = new RemoveInfo(RemoveReason.Kick, comment);
        //    var domainUser = await this.Dispatcher.InvokeAsync(() => this.Users[userID]);
        //    return (domainUser, removeInfo);
        //}

        //private async Task<(DomainUser, DomainUser)> InvokeSetOwnerAsync(Authentication authentication, string userID)
        //{
        //    return await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        var oldOwner = this.Users.Owner;
        //        var newOwner = this.Users[userID];
        //        return (oldOwner, newOwner);
        //    });
        //}

        public DomainUser GetDomainUser(string userID)
        {

            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
            if (this.Users.ContainsKey(userID) == false)
                throw new UserNotFoundException(userID);

            return this.Users[userID];
        }

        public DomainUser GetDomainUser(Authentication authentication)
        {

            if (this.Dispatcher == null)
                throw new InvalidOperationException(Resources.Exception_InvalidObject);
            this.Dispatcher.VerifyAccess();
            if (this.Users.ContainsKey(authentication.ID) == false)
                throw new UserNotFoundException(authentication.ID);

            return this.Users[authentication.ID];
        }

        private Task<DomainUser> GetDomainUserAsync(Authentication authentication)
        {
            this.ValidateExpired();
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.Users.ContainsKey(authentication.ID) == false)
                    throw new UserNotFoundException(authentication.ID);
                return this.Users[authentication.ID];
            });
        }

        private void Sign(Authentication authentication, ResultBase result)
        {
            result.Validate(authentication);
        }

        private void Sign<T>(Authentication authentication, ResultBase<T> result)
        {
            result.Validate(authentication);
        }

        private IDomainService Service => this.Context.Service;

        #region IDomain

        Task IDomain.EnterAsync(Authentication authentication, DomainAccessType accessType)
        {
            return this.EnterAsync(authentication, accessType);
        }

        Task IDomain.LeaveAsync(Authentication authentication)
        {
            return this.LeaveAsync(authentication);
        }

        Task IDomain.BeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            return this.BeginUserEditAsync(authentication, location);
        }

        Task IDomain.EndUserEditAsync(Authentication authentication)
        {
            return this.EndUserEditAsync(authentication);
        }

        Task IDomain.NewRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.NewRowAsync(authentication, rows);
        }

        Task IDomain.SetRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.SetRowAsync(authentication, rows);
        }

        Task IDomain.RemoveRowAsync(Authentication authentication, DomainRowInfo[] rows)
        {
            return this.RemoveRowAsync(authentication, rows);
        }

        Task IDomain.SetPropertyAsync(Authentication authentication, string propertyName, object value)
        {
            return this.SetPropertyAsync(authentication, propertyName, value);
        }

        Task IDomain.SetUserLocationAsync(Authentication authentication, DomainLocationInfo location)
        {
            return this.SetUserLocationAsync(authentication, location);
        }

        Task IDomain.KickAsync(Authentication authentication, string userID, string comment)
        {
            return this.KickAsync(authentication, userID, comment);
        }

        Task IDomain.SetOwnerAsync(Authentication authentication, string userID)
        {
            return this.SetOwnerAsync(authentication, userID);
        }

        IDomainUserCollection IDomain.Users => this.Users;

        DomainInfo IDomain.DomainInfo => base.DomainInfo;

        object IDomain.Host => this.Host;

        #endregion

        #region IDomainItem

        IDomainItem IDomainItem.Parent => this.Category;

        IEnumerable<IDomainItem> IDomainItem.Childs => Enumerable.Empty<IDomainItem>();

        #endregion

        #region IServiceProvider

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IDataBase) && this.Category != null)
            {
                return this.Category.DataBase;
            }
            return (this.Context as IServiceProvider).GetService(serviceType);
        }

        #endregion

        #region InternalSignatureDateProvider

        protected class InternalSignatureDateProvider : SignatureDateProvider
        {
            private readonly DateTime dateTime;

            public InternalSignatureDateProvider(SignatureDate signatureDate)
                : base(signatureDate.ID)
            {
                this.dateTime = signatureDate.DateTime;
            }

            public DateTime DateTime
            {
                get { return this.dateTime; }
            }

            protected override DateTime GetTime()
            {
                return this.dateTime;
            }
        }

        #endregion

        #region IInfoProvider

        IDictionary<string, object> IInfoProvider.Info => base.DomainInfo.ToDictionary();

        #endregion

        #region IStateProvider

        object IStateProvider.State => this.DomainState;

        #endregion
    }
}

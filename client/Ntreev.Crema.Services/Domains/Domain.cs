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

        private Dictionary<long, ManualResetEvent> setsByID = new Dictionary<long, ManualResetEvent>();

        protected Domain(DomainInfo domainInfo)
        {
            this.Initialize(domainInfo);
            this.Name = domainInfo.DomainID.ToString();
            this.Users = new DomainUserCollection(this);
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

        public async Task BeginUserEditAsync(Authentication authentication, DomainLocationInfo location)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(BeginUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.BeginUserEdit(this.ID, location));
                //var domainUser = await this.InvokeBeginUserEditAsync(authentication, location);
                //await this.Dispatcher.InvokeAsync(() =>
                //{
                //    this.CremaHost.Sign(authentication, result);
                //    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                //    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                //});
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task EndUserEditAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(EndUserEditAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.EndUserEdit(this.ID));
                //var domainUser = await this.InvokeEndUserEditAsync(authentication);
                //await this.Dispatcher.InvokeAsync(() =>
                //{
                //    this.CremaHost.Sign(authentication, result);
                //    this.OnUserChanged(new DomainUserEventArgs(authentication, this, domainUser));
                //    this.Container.InvokeDomainUserChangedEvent(authentication, this, domainUser);
                //});
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

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
                await await this.DataDispatcher.InvokeAsync(async () =>
                {
                    if (this.Logger.CompletionID < result.ID)
                    {
                        this.setsByID.Add(result.ID, new ManualResetEvent(false));
                        var set = this.setsByID[result.ID];
                        await Task.Run(() => set.WaitOne());
                    }
                });
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
                await await this.DataDispatcher.InvokeAsync(async () =>
                {
                    if (this.Logger.CompletionID < result.ID)
                    {
                        this.setsByID.Add(result.ID, new ManualResetEvent(false));
                        var set = this.setsByID[result.ID];
                        await Task.Run(() => set.WaitOne());
                    }
                });
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
                await await this.DataDispatcher.InvokeAsync(async () =>
                {
                    if (this.Logger.CompletionID < result.ID)
                    {
                        this.setsByID.Add(result.ID, new ManualResetEvent(false));
                        var set = this.setsByID[result.ID];
                        await Task.Run(() => set.WaitOne());
                    }
                });
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
                await await this.DataDispatcher.InvokeAsync(async () =>
                {
                    if (this.Logger.CompletionID < result.ID)
                    {
                        this.setsByID.Add(result.ID, new ManualResetEvent(false));
                        var set = this.setsByID[result.ID];
                        await Task.Run(() => set.WaitOne());
                    }
                });
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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.DebugMethod(authentication, this, nameof(SetUserLocationAsync), base.DomainInfo.ItemPath, base.DomainInfo.ItemType);
                });
                var result = await this.CremaHost.InvokeServiceAsync(() => this.Service.SetUserLocation(this.ID, location));
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
                await await this.DataDispatcher.InvokeAsync(async () =>
                {
                    if (this.Logger.CompletionID < result.ID)
                    {
                        this.setsByID.Add(result.ID, new ManualResetEvent(false));
                        var set = this.setsByID[result.ID];
                        await Task.Run(() => set.WaitOne());
                    }
                });
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
                await await this.DataDispatcher.InvokeAsync(async () =>
                {
                    if (this.Logger.CompletionID < result.ID)
                    {
                        this.setsByID.Add(result.ID, new ManualResetEvent(false));
                        var set = this.setsByID[result.ID];
                        await Task.Run(() => set.WaitOne());
                    }
                });
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
            base.DomainState = metaData.DomainState;
            this.modifiedTableList.Clear();
            foreach (var item in metaData.ModifiedTables)
            {
                this.modifiedTableList.Add(item);
            }

            if (metaData.Data == null)
            {
                foreach (var item in metaData.Users)
                {
                    var signatureDate = new SignatureDate(item.DomainUserInfo.UserID, authentication.SignatureDate.DateTime);
                    var userAuthentication = this.UserContext.Authenticate(signatureDate);
                    this.Users.Add(new DomainUser(this, item.DomainUserInfo, item.DomainUserState, false));
                }
            }
            else
            {
                if (metaData.CompetionID != 0 && metaData.PostID != metaData.CompetionID)
                {
                    int qwer = 0;
                }
                this.OnInitialize(metaData);
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
                foreach (var item in metaData.Users)
                {
                    var userInfo = item.DomainUserInfo;
                    if (this.Users.ContainsKey(item.DomainUserInfo.UserID) == false)
                    {
                        var signatureDate = new SignatureDate(item.DomainUserInfo.UserID, authentication.SignatureDate.DateTime);
                        var userAuthentication = this.UserContext.Authenticate(signatureDate);
                        this.Users.Add(new DomainUser(this, item.DomainUserInfo, item.DomainUserState, false));
                    }
                }
            }
        }

        public Task InitializeAsync(Authentication authentication, DomainMetaData metaData)
        {
            return this.Dispatcher.InvokeAsync(() => this.Initialize(authentication, metaData));
        }

        public UserContext UserContext => this.CremaHost.UserContext;

        public async Task ReleaseAsync(Authentication authentication, DomainMetaData metaData)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {

                foreach (var item in this.Users.ToArray<DomainUser>())
                {
                    if (metaData.Users.Any(i => i.DomainUserInfo.UserID == item.DomainUserInfo.UserID) == false)
                    {
                        this.Users.Remove(item.DomainUserInfo.UserID);
                        //this.InvokeUserRemoved(authentication, item.DomainUserInfo, RemoveInfo.Empty);
                    }
                }
                foreach (var item in metaData.Users)
                {
                    if (item.DomainUserState.HasFlag(DomainUserState.IsOwner) == true)
                    {
                        var master = this.Users[item.DomainUserInfo.UserID];
                        this.Users.Owner = master;
                        //this.InvokeUserChanged(authentication, item.DomainUserInfo, item.DomainUserState);
                    }
                }
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

        public async Task InvokeUserAddedAsync(Authentication authentication, DomainUserInfo domainUserInfo, DomainUserState domainUserState, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }

            var domainUser = new DomainUser(this, domainUserInfo, domainUserState, false);

            var proceed = await await this.DataDispatcher.InvokeAsync(async () =>
                {
                    if (id < this.Logger.CompletionID)
                        return false;
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.Users.Add(domainUser);
                    });
                    this.Logger.Join(authentication, domainUser.DomainUserInfo.AccessType, id);
                    return true;
                });
            if (proceed == false)
                return;
            await this.Dispatcher.InvokeAsync(() =>
            {
                //domainUser.DomainUserState &= ~DomainUserState.Detached;
                //this.CremaHost.Sign(authentication);
                this.OnUserAdded(new DomainUserEventArgs(authentication, this, domainUser));
                this.Container?.InvokeDomainUserAddedEvent(authentication, this, domainUser, id);
                this.SetEvent(id);
            });

        }


        public async Task InvokeUserRemovedAsync(Authentication authentication, DomainUser domainUser, DomainUser ownerUser, RemoveInfo removeInfo, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }
            var proceed = await await this.DataDispatcher.InvokeAsync(async () =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Users.Remove(domainUser.ID);
                    this.Users.Owner = ownerUser;
                });
                if (removeInfo.Reason == RemoveReason.Kick)
                    this.Logger.Kick(authentication, domainUser.ID, removeInfo.Message, id);
                else
                    this.Logger.Disjoin(authentication, removeInfo, id);
                return true;
            });
            if (proceed == false)
                return;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnUserRemoved(new DomainUserRemovedEventArgs(authentication, this, domainUser, removeInfo));
                this.Container?.InvokeDomainUserRemovedEvent(authentication, this, domainUser, removeInfo, id);
                this.SetEvent(id);
            });
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

        public async Task InvokeUserEditBegunAsync(Authentication authentication, DomainUser domainUser, DomainLocationInfo domainLocationInfo, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }

            var proceed = await this.DataDispatcher.InvokeAsync(() =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                this.OnBeginUserEdit(domainUser, domainLocationInfo);
                this.Logger.BeginUserEdit(authentication, domainLocationInfo, id);
                return true;
            });
            if (proceed == false)
                return;
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.DomainLocationInfo = domainLocationInfo;
                domainUser.IsBeingEdited = true;
                //this.CremaHost.Sign(authentication);
                this.OnUserEditBegun(new DomainUserLocationEventArgs(authentication, this, domainUser));
                this.Container?.InvokeDomainUserEditBegunEvent(authentication, this, domainUser, id);
                this.SetEvent(id);
            });
        }

        public async Task InvokeUserEditEndedAsync(Authentication authentication, DomainUser domainUser, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }

            var proceed = await this.DataDispatcher.InvokeAsync(() =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                this.OnEndUserEdit(domainUser);
                this.Logger.EndUserEdit(authentication, id);
                return true;
            });
            if (proceed == false)
                return;
            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.IsBeingEdited = false;
                //this.CremaHost.Sign(authentication);
                this.OnUserEditEnded(new DomainUserEventArgs(authentication, this, domainUser));
                this.Container?.InvokeDomainUserEditEndedEvent(authentication, this, domainUser, id);
                this.SetEvent(id);
            });
        }

        public async Task InvokeOwnerChangedAsync(Authentication authentication, DomainUser domainUser, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }

            var proceed = await await this.DataDispatcher.InvokeAsync(async () =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Users.Owner = domainUser;
                });
                this.Logger.SetOwner(authentication, domainUser.ID, id);
                return true;
            });
            if (proceed == false)
                return;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnOwnerChanged(new DomainUserEventArgs(authentication, this, domainUser));
                this.Container?.InvokeDomainOwnerChangedEvent(authentication, this, domainUser, id);
                this.SetEvent(id);
            });
        }

        public async Task InvokeRowAddedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }

            var proceed = await this.DataDispatcher.InvokeAsync(() =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                this.OnNewRow(domainUser, rows, authentication.SignatureDate);
                this.Logger.NewRow(authentication, rows, id);
                return true;
            });
            if (proceed == false)
                return;
            await this.Dispatcher.InvokeAsync(() =>
            {
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
                this.SetEvent(id);
            });
        }

        private void SetEvent(long id)
        {
            if (this.setsByID.ContainsKey(id) == true)
            {
                this.setsByID[id].Set();
                this.setsByID.Remove(id);
            }
        }

        public async Task InvokeRowChangedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }

            var proceed = await this.DataDispatcher.InvokeAsync(() =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                this.OnSetRow(domainUser, rows, authentication.SignatureDate);
                this.Logger.SetRow(authentication, rows, id);
                return true;
            });
            if (proceed == false)
                return;

            await this.Dispatcher.InvokeAsync(() =>
            {
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
                this.SetEvent(id);
            });
        }

        public async Task InvokeRowRemovedAsync(Authentication authentication, DomainUser domainUser, DomainRowInfo[] rows, long id)
        {
            if (this.initialized == false)
            {
                if (authentication.ID == this.CremaHost.UserID)
                {
                    int wer = 0;
                }
                return;
            }

            var proceed = await this.DataDispatcher.InvokeAsync(() =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                this.OnRemoveRow(domainUser, rows, authentication.SignatureDate);
                this.Logger.RemoveRow(authentication, rows, id);
                return true;
            });
            if (proceed == false)
                return;

            await this.Dispatcher.InvokeAsync(() =>
            {
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
                this.SetEvent(id);
            });
        }

        public async Task InvokePropertyChangedAsync(Authentication authentication, DomainUser domainUser, string propertyName, object value, long id)
        {
            if (this.initialized == false)
                return;

            var proceed = await this.DataDispatcher.InvokeAsync(() =>
            {
                if (id < this.Logger.CompletionID)
                    return false;
                this.OnSetProperty(domainUser, propertyName, value, authentication.SignatureDate);
                this.Logger.SetProperty(authentication, propertyName, value, id);
                return true;
            });
            if (proceed == false)
                return;

            await this.Dispatcher.InvokeAsync(() =>
            {
                domainUser.IsModified = true;
                this.IsModified = true;
                //this.CremaHost.Sign(authentication);
                base.UpdateModificationInfo(authentication.SignatureDate);
                this.OnPropertyChanged(new DomainPropertyEventArgs(authentication, this, propertyName, value));
                this.Container?.InvokeDomainPropertyChangedEvent(authentication, this, id, propertyName, value);
                this.SetEvent(id);
            });
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

        protected virtual void OnInitialize(DomainMetaData metaData)
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
            this.userLocationChanged(this, e);
        }

        protected virtual void OnUserStateChanged(DomainUserEventArgs e)
        {
            this.userStateChanged(this, e);
        }

        protected virtual void OnUserEditBegun(DomainUserLocationEventArgs e)
        {
            this.userEditBegun(this, e);
        }

        protected virtual void OnUserEditEnded(DomainUserEventArgs e)
        {
            this.userEditEnded(this, e);
        }

        protected virtual void OnOwnerChanged(DomainUserEventArgs e)
        {
            this.ownerChanged(this, e);
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

        IDomainUserCollection IDomain.Users => this.Users;

        DomainInfo IDomain.DomainInfo => base.DomainInfo;

        object IDomain.Host => this.Host;

        #endregion

        #region IDomainItem

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

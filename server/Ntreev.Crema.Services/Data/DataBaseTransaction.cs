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
using Ntreev.Crema.Services.Domains;
using Ntreev.Crema.Services.Properties;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    class DataBaseTransaction : ITransaction
    {
        private Authentication authentication;
        private readonly DataBase dataBase;
        private readonly DataBaseRepositoryHost repository;
        private readonly TypeInfo[] typeInfos;
        private readonly TableInfo[] tableInfos;
        private readonly string transactionPath;
        private readonly string domainPath;

        public DataBaseTransaction(Authentication authentication, DataBase dataBase, DataBaseRepositoryHost repository)
        {
            this.authentication = authentication;
            this.dataBase = dataBase;
            this.dataBase.Dispatcher.VerifyAccess();
            this.repository = repository;
            this.typeInfos = dataBase.TypeContext.Types.Select((Type item) => item.TypeInfo).ToArray();
            this.tableInfos = dataBase.TableContext.Tables.Select((Table item) => item.TableInfo).ToArray();
            this.transactionPath = this.CremaHost.GetPath(CremaPath.Transactions, $"{dataBase.ID}");
            this.domainPath = this.CremaHost.GetPath(CremaPath.Domains, $"{dataBase.ID}");
        }

        public async void BeginAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.dataBase.DataBaseState = DataBaseState.Progressing;
                });
                await this.DomainContext.BeginTransactionAsync(authentication, this.domainPath, this.transactionPath);
                await this.repository.BeginTransactionAsync(authentication.ID, dataBase.Name);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ID = Guid.NewGuid();
                    this.dataBase.LockForTransaction(authentication, this.ID);
                    this.dataBase.DataBaseState = DataBaseState.Loaded;
                    this.CremaHost.Sign(authentication);
                    this.authentication.Expired += Authentication_Expired;
                });
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() => this.dataBase.DataBaseState = DataBaseState.Loaded);
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task CommitAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateCommit(authentication);
                    this.dataBase.DataBaseState = DataBaseState.Progressing;
                });
                await this.repository.EndTransactionAsync();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.authentication.Expired -= Authentication_Expired;
                    this.authentication = null;
                    this.dataBase.DataBaseState = DataBaseState.Loaded;
                    this.dataBase.UnlockForTransaction(authentication, this.ID);
                });
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() => this.dataBase.DataBaseState = DataBaseState.Loaded);
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DataBaseMetaData> RollbackAsync(Authentication authentication)
        {
            try
            {
                this.ValidateExpired();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.ValidateRollback(authentication);
                    this.dataBase.DataBaseState = DataBaseState.Progressing;
                });
                await this.dataBase.ResettingDataBaseAsync(authentication);
                await this.DomainContext.DeleteDomainsAsync(Authentication.System, this.dataBase.ID);
                await this.repository.CancelTransactionAsync();
                await this.DomainContext.CancelTransactionAsync(this.domainPath, this.transactionPath);
                await this.DomainContext.RestoreAsync(this.dataBase);
                await this.dataBase.ResetDataBaseAsync(authentication, this.typeInfos, this.tableInfos);
                return await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication);
                    this.authentication.Expired -= Authentication_Expired;
                    this.dataBase.DataBaseState = DataBaseState.Loaded;
                    this.dataBase.UnlockForTransaction(authentication, this.ID);
                    return this.dataBase.GetMetaData(authentication);
                });
            }
            catch (Exception e)
            {
                await this.Dispatcher.InvokeAsync(() => this.dataBase.DataBaseState = DataBaseState.Loaded);
                this.CremaHost.Error(e);
                throw;
            }
        }

        public CremaDispatcher Dispatcher => this.dataBase.Dispatcher;

        public CremaHost CremaHost => this.dataBase.CremaHost;

        public DomainContext DomainContext => this.CremaHost.DomainContext;

        public Guid ID { get; private set; }

        private async void Authentication_Expired(object sender, EventArgs e)
        {
            this.authentication.Expired -= Authentication_Expired;
            this.authentication = null;
            await this.RollbackAsync(this.authentication);
        }

        private void ValidateCommit(Authentication authentication)
        {
            if (this.authentication == null)
                throw new InvalidOperationException(Resources.Exception_Expired);
        }

        private void ValidateRollback(Authentication authentication)
        {
            if (this.authentication == null)
                throw new InvalidOperationException(Resources.Exception_Expired);
        }
    }
}

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

using JSSoft.Crema.ServiceHosts.Data;
using JSSoft.Crema.ServiceModel;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Data
{
    class DataBaseTransaction : ITransaction
    {
        private readonly DataBase dataBase;
        private readonly IDataBaseContextService service;

        public DataBaseTransaction(DataBase dataBase, IDataBaseContextService service)
        {
            this.dataBase = dataBase;
            this.service = service;
        }

        public async Task BeginAsync(Authentication authentication)
        {
            if (authentication is null)
                throw new ArgumentNullException(nameof(authentication));

            var result = await this.service.BeginTransactionAsync(this.dataBase.Name);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.CremaHost.Sign(authentication, result);
                this.ID = result.Value;
                this.dataBase.LockForTransaction(authentication, this.ID);
            });
        }

        public async Task CommitAsync(Authentication authentication)
        {
            if (authentication is null)
                throw new ArgumentNullException(nameof(authentication));

            try
            {
                this.ValidateExpired();
                var result = await this.service.EndTransactionAsync(this.ID);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.dataBase.UnlockForTransaction(authentication, this.ID);
                    this.OnDisposed(EventArgs.Empty);
                });
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public async Task<DataBaseMetaData> RollbackAsync(Authentication authentication)
        {
            if (authentication is null)
                throw new ArgumentNullException(nameof(authentication));

            try
            {
                this.ValidateExpired();
                var result = await this.service.CancelTransactionAsync(this.ID);

                await this.dataBase.Dispatcher.InvokeAsync(() =>
                {
                    this.dataBase.SetResetting(authentication);
                    this.dataBase.SetReset2(authentication, result.Value);
                });

                //await this.DomainContext.DeleteDomainsAsync(authentication, this.dataBase.ID);
                //await this.DomainContext.RestoreAsync(authentication, this.dataBase.ID);

                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.CremaHost.Sign(authentication, result);
                    this.dataBase.UnlockForTransaction(authentication, this.ID);
                    this.OnDisposed(EventArgs.Empty);
                });
                return result.Value;
            }
            catch (Exception e)
            {
                this.CremaHost.Error(e);
                throw;
            }
        }

        public void Dispose()
        {
        }

        public CremaDispatcher Dispatcher => this.dataBase.Dispatcher;

        public CremaHost CremaHost => this.dataBase.CremaHost;

        public Guid ID { get; private set; }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }
    }
}

using Ntreev.Crema.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    sealed class CremaResetEvent<T>
    {
        private readonly Dictionary<T, ManualResetEvent> setsByID = new Dictionary<T, ManualResetEvent>();

        public CremaResetEvent(CremaDispatcher dispatcher)
        {
            this.Dispatcher = dispatcher;
        }

        public async Task WaitAsync(T id)
        {
            await await this.Dispatcher.InvokeAsync(async () =>
            {
                if (this.setsByID.ContainsKey(id) == false)
                {
                    this.setsByID.Add(id, new ManualResetEvent(false));
                }
                else
                {
                    //this.setsByID[id].Reset();
                }
                var set = this.setsByID[id];
                await Task.Run(() => set.WaitOne());
            });
        }

        public Task SetAsync(T id)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.setsByID.ContainsKey(id) == true)
                {
                    this.setsByID[id].Set();
                }
                else
                {
                    this.setsByID.Add(id, new ManualResetEvent(true));
                }
            });
        }

        public void Set(T id)
        {
            this.Dispatcher.VerifyAccess();
            if (this.setsByID.ContainsKey(id) == true)
            {
                this.setsByID[id].Set();
            }
            else
            {
                this.setsByID.Add(id, new ManualResetEvent(true));
            }
        }

        public Task ResetAsync(T id)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.setsByID.ContainsKey(id) == true)
                {
                    this.setsByID[id].Reset();
                }
            });
        }

        private CremaDispatcher Dispatcher { get; }
    }
}

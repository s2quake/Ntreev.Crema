using Ntreev.Crema.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    sealed class CallbackEvent
    {
        private readonly Dictionary<long, ManualResetEvent> setsByID = new Dictionary<long, ManualResetEvent>();
        private long currentID;

        private object owner;


        

        public CallbackEvent(object owner)
        {
            this.owner = owner;
            this.Dispatcher = new CremaDispatcher(this);
            this.setsByID.Add(-1, new ManualResetEvent(true));
        }

        public async Task BeginAsync(long id)
        {
            var set = await this.Dispatcher.InvokeAsync(() =>
            {
                var prevID = id - 1;
                this.currentID = id;
                if (this.setsByID.ContainsKey(prevID) == false)
                {
                    this.setsByID.Add(prevID, new ManualResetEvent(false));
                }
                return this.setsByID[prevID];

            });
            await Task.Run(() => set.WaitOne());
        }

        public Task EndAsync(long id)
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

        //public void Set(long id)
        //{
        //    this.Dispatcher.VerifyAccess();
        //    if (this.setsByID.ContainsKey(id) == true)
        //    {
        //        this.setsByID[id].Set();
        //    }
        //    else
        //    {
        //        this.setsByID.Add(id, new ManualResetEvent(true));
        //    }
        //}

        //public Task ResetAsync(long id)
        //{
        //    return this.Dispatcher.InvokeAsync(() =>
        //    {
        //        if (this.setsByID.ContainsKey(id) == true)
        //        {
        //            this.setsByID[id].Reset();
        //        }
        //    });
        //}

        //public void Reset(long id)
        //{
        //    this.Dispatcher.VerifyAccess();
        //    if (this.setsByID.ContainsKey(id) == true)
        //    {
        //        this.setsByID[id].Reset();
        //    }
        //}

        //public void Dispose()
        //{
        //    this.Dispatcher.Dispose();
        //}

        public async Task DisposeAsync()
        {
            var set = await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.setsByID.ContainsKey(this.currentID) == true)
                {
                    return this.setsByID[this.currentID];
                }
                else
                {
                    return null;
                }
            });
            if (set != null)
                await Task.Run(() => set.WaitOne());
            await this.Dispatcher.DisposeAsync();
        }

        private CremaDispatcher Dispatcher { get; }
    }
}

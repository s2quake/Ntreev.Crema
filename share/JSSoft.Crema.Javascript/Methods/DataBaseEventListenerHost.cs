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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Javascript.Methods
{
    abstract class DataBaseEventListenerHost
    {
        private readonly Dictionary<IDataBase, DataBaseEventListenerCollection> dataBaseToListeners = new();

        protected DataBaseEventListenerHost(DataBaseEvents eventName)
        {
            this.EventName = eventName;
        }

        public async Task DisposeAsync()
        {
            foreach (var item in this.dataBaseToListeners)
            {
                await this.OnUnsubscribeAsync(item.Key);
            }
            this.dataBaseToListeners.Clear();
        }

        public async Task SubscribeAsync(IDataBase dataBase, DataBaseEventListener listener)
        {
            if (this.dataBaseToListeners.ContainsKey(dataBase) == false)
            {
                this.dataBaseToListeners[dataBase] = new DataBaseEventListenerCollection();
            }
            var listeners = this.dataBaseToListeners[dataBase];
            if (listeners.Any() == false)
            {
                await this.OnSubscribeAsync(dataBase);
            }
            listeners.Add(listener);
        }

        public async Task UnsubscribeAsync(IDataBase dataBase, DataBaseEventListener listener)
        {
            var listeners = this.dataBaseToListeners[dataBase];
            listeners.Remove(listener);
            if (listeners.Any() == false)
            {
                await this.OnUnsubscribeAsync(dataBase);
                this.dataBaseToListeners.Remove(dataBase);
            }
        }

        public async Task SubscribeAsync(IDataBase dataBase, IEnumerable<DataBaseEventListener> listeners)
        {
            this.dataBaseToListeners[dataBase] = new DataBaseEventListenerCollection(listeners);
            await this.OnSubscribeAsync(dataBase);
        }

        public async Task UnsubscribeAsync(IDataBase dataBase)
        {
            await this.OnUnsubscribeAsync(dataBase);
            this.dataBaseToListeners.Remove(dataBase);
        }

        public CremaDispatcher Dispatcher
        {
            get; set;
        }

        public DataBaseEvents EventName { get; }

        protected async void InvokeAsync(IDataBase dataBase, IDictionary<string, object> properties)
        {
            var dataBaseName = dataBase.Name;
            var listeners = this.dataBaseToListeners[dataBase].ToArray();
            await this.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in listeners)
                {
                    item.Invoke(dataBaseName, properties);
                }
            });
        }

        protected abstract Task OnSubscribeAsync(IDataBase dataBase);

        protected abstract Task OnUnsubscribeAsync(IDataBase dataBase);


    }
}

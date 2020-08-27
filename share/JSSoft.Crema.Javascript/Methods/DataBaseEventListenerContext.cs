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

using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JSSoft.Crema.Javascript.Methods
{
    class DataBaseEventListenerContext
    {
        private readonly ICremaHost cremaHost;
        private readonly IDictionary<DataBaseEvents, DataBaseEventListenerHost> listenerHosts;
        private readonly Dictionary<DataBaseEvents, DataBaseEventListenerCollection> listeners = new Dictionary<DataBaseEvents, DataBaseEventListenerCollection>();
        private readonly CremaDispatcher dispatcher;

        public DataBaseEventListenerContext(ICremaHost cremaHost, DataBaseEventListenerHost[] eventListener)
        {
            this.cremaHost = cremaHost;

            this.listenerHosts = eventListener.ToDictionary(item => item.EventName);
            this.dispatcher = new CremaDispatcher(this);
            foreach (var item in eventListener)
            {
                item.Dispatcher = this.dispatcher;
            }
            this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext.ItemsLoaded += DataBaseContext_ItemsLoaded);
            this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext.ItemsUnloaded += DataBaseContext_ItemsUnloaded);
        }

        public void Dispose()
        {
            this.dispatcher.Invoke(() =>
            {
                foreach (var item in this.listenerHosts)
                {
                    item.Value.Dispose();
                }
            });
            this.dispatcher.Dispose();
        }

        public void AddEventListener(DataBaseEvents eventName, DataBaseEventListener listener)
        {
            this.dispatcher.Invoke(() =>
            {
                if (this.listenerHosts.ContainsKey(eventName) == true)
                {
                    if (this.listeners.ContainsKey(eventName) == false)
                    {
                        this.listeners[eventName] = new DataBaseEventListenerCollection();
                    }

                    this.listeners[eventName].Add(listener);

                    var dataBases = this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext.Where(item => item.IsLoaded).ToArray());
                    var listenerHost = this.listenerHosts[eventName];
                    foreach (var item in dataBases)
                    {
                        listenerHost.Subscribe(item, listener);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            });
        }

        public void RemoveEventListener(DataBaseEvents eventName, DataBaseEventListener listener)
        {
            this.dispatcher.Invoke(() =>
            {
                if (this.listenerHosts.ContainsKey(eventName) == true)
                {
                    var dataBases = this.DataBaseContext.Dispatcher.Invoke(() => this.DataBaseContext.Where(item => item.IsLoaded).ToArray());
                    var listenerHost = this.listenerHosts[eventName];
                    foreach (var item in dataBases)
                    {
                        listenerHost.Unsubscribe(item, listener);
                    }
                    var listeners = this.listeners[eventName];
                    listeners.Remove(listener);
                }
                else
                {
                    throw new NotImplementedException();
                }
            });
        }

        private void DataBaseContext_ItemsLoaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            if (sender is IDataBase dataBase)
            {
                this.dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.listenerHosts)
                    {
                        var eventName = item.Key;
                        var host = item.Value;
                        var listeners = this.listeners[eventName];
                        host.Subscribe(dataBase, listeners);
                    }
                });
            }
        }

        private void DataBaseContext_ItemsUnloaded(object sender, ItemsEventArgs<IDataBase> e)
        {
            if (sender is IDataBase dataBase)
            {
                this.dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in this.listenerHosts.Values)
                    {
                        item.Unsubscribe(dataBase);
                    }
                });
            }
        }

        private IDataBaseContext DataBaseContext => this.cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;

        #region classes

        class ListenerItem
        {
            public DataBaseEvents EventName { get; set; }

            public DataBaseEventListener Listener { get; set; }
        }

        #endregion
    }
}

﻿// Released under the MIT License.
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
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace JSSoft.Crema.Services.Data
{
    [Export(typeof(IPlugin))]
    [Export(typeof(DataFindService))]
    class DataFindService : IPlugin
    {
        public const string ServiceID = "44343501-B6B7-444D-8A5E-7CAE32F054A4";
        private readonly ICremaHost cremaHost;
        private Authentication authentication;

        private readonly Dictionary<Guid, DataFindServiceItem> items = new();

        [ImportingConstructor]
        public DataFindService(ICremaHost cremaHost)
        {
            this.cremaHost = cremaHost;
            this.cremaHost.CloseRequested += CremaHost_CloseRequested;
            this.cremaHost.Closed += CremaHost_Closed;
            this.cremaHost.Opened += CremaHost_Opened;
        }

        public async void Initialize(Authentication authentication)
        {
            this.Dispatcher = new CremaDispatcher(this);
            this.authentication = authentication;

            if (this.cremaHost.GetService(typeof(IDataBaseContext)) is IDataBaseContext dataBaseContext)
            {
                await dataBaseContext.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in dataBaseContext)
                    {
                        var serviceItem = new DataFindServiceItem(item, this.Dispatcher, authentication);
                        this.items.Add(item.ID, serviceItem);
                    }
                });
            }
        }

        public void Release()
        {

        }

        public string Name => this.GetType().Name;

        public FindResultInfo[] FindFromTable(Guid dataBaseID, string[] itemPaths, string text, FindOptions options)
        {
            this.Dispatcher.VerifyAccess();

            return this.items[dataBaseID].FindFromTable(itemPaths, text, options);
        }

        public FindResultInfo[] FindFromType(Guid dataBaseID, string[] itemPaths, string text, FindOptions options)
        {
            this.Dispatcher.VerifyAccess();

            return this.items[dataBaseID].FindFromType(itemPaths, text, options);
        }

        public CremaDispatcher Dispatcher { get; private set; }

        public Guid ID => Guid.Parse(ServiceID);

        private async void CremaHost_Opened(object sender, EventArgs e)
        {
            if (this.cremaHost.GetService(typeof(IDataBaseContext)) is IDataBaseContext dataBaseContext)
            {
                await dataBaseContext.Dispatcher.InvokeAsync(() =>
                {
                    dataBaseContext.ItemsCreated += DataBaseContext_ItemCreated;
                    dataBaseContext.ItemsDeleted += DataBaseContext_ItemDeleted;
                });
            }
        }

        private void CremaHost_CloseRequested(object sender, CloseRequestedEventArgs e)
        {
            e.AddTask(this.Dispatcher.DisposeAsync());
        }

        private void CremaHost_Closed(object sender, EventArgs e)
        {
            foreach (var item in this.items)
            {
                item.Value.Commit();
            }
            this.items.Clear();
        }

        private void DataBaseContext_ItemCreated(object sender, ItemsCreatedEventArgs<IDataBase> e)
        {
            foreach (var item in e.Items)
            {
                var serviceItem = new DataFindServiceItem(item, this.Dispatcher, authentication);
                this.items.Add(item.ID, serviceItem);
            }
        }

        private void DataBaseContext_ItemDeleted(object sender, ItemsDeletedEventArgs<IDataBase> e)
        {
            foreach (var item in e.Items)
            {
                var value = this.items[item.ID];
                value.Dispose();
                this.items.Remove(item.ID);
            }
        }
    }
}

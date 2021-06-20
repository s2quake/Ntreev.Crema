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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Library.Random;
using JSSoft.Crema.Services.Test.Extensions;
using JSSoft.Crema.Services.Random;

namespace JSSoft.Crema.Services.Test
{
    class TestServerConfigurator
    {
        private readonly TestApplication app;

        public TestServerConfigurator(TestApplication app)
        {
            this.app = app;
        }

        public async Task<IDataBase[]> GenerateDataBasesAsync(int count)
        {
            var cremaHost = this.app.GetService(typeof(ICremaHost)) as ICremaHost;
            var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
            var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var itemList = new List<IDataBase>(count);
            for (var i = 0; i < count; i++)
            {
                var dataBaseName = await dataBaseContext.GenerateNewDataBaseNameAsync(RandomUtility.NextName());
                var comment = RandomUtility.NextString();
                var item = await dataBaseContext.AddNewDataBaseAsync(authentication, dataBaseName, comment);
                itemList.Add(item);
            }
            await cremaHost.LogoutAsync(authentication);
            return itemList.ToArray();
        }

        public async Task LoginRandomManyAsync()
        {
            var cremaHost = this.app.GetService(typeof(ICremaHost)) as ICremaHost;
            var userCollcection = cremaHost.GetService(typeof(IUserCollection)) as IUserCollection;
            var count = await userCollcection.Dispatcher.InvokeAsync(() => userCollcection.Count);
            await cremaHost.LoginRandomManyAsync((int)(count * 0.25));
        }

        public async Task LoadRandomDataBasesAsync()
        {
            var cremaHost = this.app.GetService(typeof(ICremaHost)) as ICremaHost;
            var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var dataBases = await dataBaseContext.GetDataBasesAsync();
            for (var i = 0; i < dataBases.Length; i++)
            {
                if (i % 2 == 0)
                {
                    var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
                    var dataBase = dataBases[i];
                    await dataBase.LoadAsync(authentication);
                    await cremaHost.LogoutAsync(authentication);
                }
            }
        }

        public async Task SetPrivateRandomDataBasesAsync()
        {
            var cremaHost = this.app.GetService(typeof(ICremaHost)) as ICremaHost;
            var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var dataBases = await dataBaseContext.GetDataBasesAsync();
            for (var i = 0; i < dataBases.Length; i++)
            {
                if (i % 3 == 0)
                {
                    var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
                    var dataBase = dataBases[i];
                    var userCollection = dataBaseContext.GetService(typeof(IUserCollection)) as IUserCollection;
                    var admins = new Queue<IUser>(await userCollection.GetRandomUsersAsync(UserFlags.Admin, item => item.ID != authentication.ID));
                    var members = new Queue<IUser>(await userCollection.GetRandomUsersAsync(UserFlags.Member));
                    var guests = new Queue<IUser>(await userCollection.GetRandomUsersAsync(UserFlags.Guest));
                    var isLoaded = dataBase.IsLoaded;
                    if (isLoaded == false)
                        await dataBase.LoadAsync(authentication);
                    await dataBase.SetPrivateAsync(authentication);
                    for (var j = 0; j < 3; j++)
                    {
                        await dataBase.AddAccessMemberAsync(authentication, admins.Dequeue().ID, AccessType.Master);
                        await dataBase.AddAccessMemberAsync(authentication, admins.Dequeue().ID, AccessType.Developer);
                        await dataBase.AddAccessMemberAsync(authentication, admins.Dequeue().ID, AccessType.Editor);
                        await dataBase.AddAccessMemberAsync(authentication, admins.Dequeue().ID, AccessType.Guest);
                        await dataBase.AddAccessMemberAsync(authentication, members.Dequeue().ID, AccessType.Master);
                        await dataBase.AddAccessMemberAsync(authentication, members.Dequeue().ID, AccessType.Developer);
                        await dataBase.AddAccessMemberAsync(authentication, members.Dequeue().ID, AccessType.Editor);
                        await dataBase.AddAccessMemberAsync(authentication, members.Dequeue().ID, AccessType.Guest);
                        await dataBase.AddAccessMemberAsync(authentication, guests.Dequeue().ID, AccessType.Guest);
                    }
                    if (isLoaded == false)
                        await dataBase.UnloadAsync(authentication);
                    await cremaHost.LogoutAsync(authentication);
                }
            }
        }

        public async Task LockRandomDataBasesAsync()
        {
            var cremaHost = this.app.GetService(typeof(ICremaHost)) as ICremaHost;
            var dataBaseContext = cremaHost.GetService(typeof(IDataBaseContext)) as IDataBaseContext;
            var dataBases = await dataBaseContext.GetDataBasesAsync();
            for (var i = 0; i < dataBases.Length; i++)
            {
                if (i % 4 == 0)
                {
                    var authentication = await cremaHost.LoginRandomAsync(Authority.Admin);
                    var dataBase = dataBases[i];
                    await dataBase.LockAsync(authentication, RandomUtility.NextString());
                    await cremaHost.LogoutAsync(authentication);
                }
            }
        }
    }
}

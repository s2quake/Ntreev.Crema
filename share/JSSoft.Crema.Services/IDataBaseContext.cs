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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    [DispatcherMethod(typeof(IDataBaseContext), DeclaringType = typeof(IEnumerable<IDataBase>), MethodName = nameof(IEnumerable<IDataBase>.GetEnumerator))]
    [DispatcherMethod(typeof(IDataBaseContext), DeclaringType = typeof(IEnumerable), MethodName = nameof(IEnumerable.GetEnumerator))]
    [DispatcherProperty(typeof(IDataBaseContext), DeclaringType = typeof(IReadOnlyCollection<IDataBase>), PropertyName = nameof(IReadOnlyCollection<IDataBase>.Count))]
    public interface IDataBaseContext : IReadOnlyCollection<IDataBase>, IEnumerable<IDataBase>, IServiceProvider, IDispatcherObject
    {
        [DispatcherMethod(typeof(IDataBaseContext))]
        DataBaseContextMetaData GetMetaData(Authentication authentication);

        Task<IDataBase> AddNewDataBaseAsync(Authentication authentication, string dataBaseName, string comment);

        [DispatcherMethod(typeof(IDataBaseContext))]
        bool Contains(string dataBaseName);

        [DispatcherProperty(typeof(IDataBaseContext))]
        IDataBase this[string dataBaseName] { get; }

        [DispatcherProperty(typeof(IDataBaseContext))]
        IDataBase this[Guid dataBaseID] { get; }

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsCreatedEventHandler<IDataBase> ItemsCreated;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsRenamedEventHandler<IDataBase> ItemsRenamed;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsDeletedEventHandler<IDataBase> ItemsDeleted;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsLoaded;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsUnloaded;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsResetting;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsReset;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsAuthenticationEntered;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsAuthenticationLeft;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsInfoChanged;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsStateChanged;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsAccessChanged;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event ItemsEventHandler<IDataBase> ItemsLockChanged;

        [DispatcherEvent(typeof(IDataBaseContext))]
        event TaskCompletedEventHandler TaskCompleted;
    }
}

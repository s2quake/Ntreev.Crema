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

using JSSoft.Crema.Data;
using JSSoft.Crema.ServiceModel;
using System;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public interface IDataBase : IAccessible, IPermission, ILockable, IServiceProvider, IDispatcherObject, IExtendedProperties
    {
        [DispatcherMethod(typeof(IDataBase))]
        DataBaseMetaData GetMetaData();

        Task LoadAsync(Authentication authentication);

        Task UnloadAsync(Authentication authentication);

        Task EnterAsync(Authentication authentication);

        Task LeaveAsync(Authentication authentication);

        Task RenameAsync(Authentication authentication, string name);

        Task DeleteAsync(Authentication authentication);

        Task RevertAsync(Authentication authentication, string revision);

        Task ImportAsync(Authentication authentication, CremaDataSet dataSet, string comment);

        [DispatcherMethod(typeof(IDataBase))]
        bool Contains(Authentication authentication);

        Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision);

        Task<CremaDataSet> GetDataSetAsync(Authentication authentication, CremaDataSetFilter filter, string revision);

        Task<ITransaction> BeginTransactionAsync(Authentication authentication);

        Task<IDataBase> CopyAsync(Authentication authentication, string newDataBaseName, string comment, bool force);

        string Name { get; }

        bool IsLoaded { get; }

        bool IsLocked { get; }

        bool IsPrivate { get; }

        Guid ID { get; }

        DataBaseInfo DataBaseInfo { get; }

        DataBaseState DataBaseState { get; }

        DataBaseFlags DataBaseFlags { get; }

        [DispatcherProperty(typeof(IDataBase))]
        AuthenticationInfo[] AuthenticationInfos { get; }

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler Renamed;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler Deleted;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler Loaded;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler Unloaded;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler Resetting;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler Reset;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler<AuthenticationEventArgs> AuthenticationEntered;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler<AuthenticationEventArgs> AuthenticationLeft;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler DataBaseInfoChanged;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler DataBaseStateChanged;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler LockChanged;

        [DispatcherEvent(typeof(IDataBase))]
        event EventHandler AccessChanged;

        [DispatcherEvent(typeof(IDataBase))]
        event TaskCompletedEventHandler TaskCompleted;
    }
}

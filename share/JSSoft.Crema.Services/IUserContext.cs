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
    [DispatcherMethod(typeof(IUserContext), DeclaringType = typeof(IEnumerable<IUserItem>), MethodName = nameof(IEnumerable<IUserItem>.GetEnumerator))]
    [DispatcherMethod(typeof(IUserContext), DeclaringType = typeof(IEnumerable), MethodName = nameof(IEnumerable.GetEnumerator))]
    public interface IUserContext : IEnumerable<IUserItem>, IServiceProvider, IDispatcherObject
    {
        [DispatcherMethod(typeof(IUserContext))]
        UserContextMetaData GetMetaData(Authentication authentication);

        Task NotifyMessageAsync(Authentication authentication, string[] userIDs, string message);

        [DispatcherMethod(typeof(IUserContext))]
        bool Contains(string itemPath);

        IUserCollection Users { get; }

        IUserCategoryCollection Categories { get; }

        IUserCategory Root { get; }

        [DispatcherProperty(typeof(IUserContext))]
        IUserItem this[string itemPath] { get; }

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsCreatedEventHandler<IUserItem> ItemsCreated;

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsRenamedEventHandler<IUserItem> ItemsRenamed;

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsMovedEventHandler<IUserItem> ItemsMoved;

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsDeletedEventHandler<IUserItem> ItemsDeleted;

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsEventHandler<IUserItem> ItemsChanged;

        [DispatcherEvent(typeof(IUserContext))]
        event TaskCompletedEventHandler TaskCompleted;
    }
}

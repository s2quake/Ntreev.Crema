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

using System;
using System.Collections;
using System.Collections.Generic;
using JSSoft.Crema.ServiceModel;

namespace JSSoft.Crema.Services
{
    [DispatcherMethod(typeof(IUserContext), DeclaringType = typeof(IEnumerable<IUserCategory>), MethodName = nameof(IEnumerable<IUserCategory>.GetEnumerator))]
    [DispatcherMethod(typeof(IUserContext), DeclaringType = typeof(IEnumerable), MethodName = nameof(IEnumerable.GetEnumerator))]
    [DispatcherProperty(typeof(IUserContext), DeclaringType = typeof(IReadOnlyCollection<IUserCategory>), PropertyName = nameof(IReadOnlyCollection<IUserCategory>.Count))]
    public interface IUserCategoryCollection : IReadOnlyCollection<IUserCategory>, IEnumerable<IUserCategory>, IServiceProvider, IDispatcherObject
    {
        [DispatcherMethod(typeof(IUserContext))]
        bool Contains(string categoryPath);

        IUserCategory Root { get; }

        [DispatcherProperty(typeof(IUserContext))]
        IUserCategory this[string categoryPath] { get; }

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsCreatedEventHandler<IUserCategory> CategoriesCreated;

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsRenamedEventHandler<IUserCategory> CategoriesRenamed;

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsMovedEventHandler<IUserCategory> CategoriesMoved;

        [DispatcherEvent(typeof(IUserContext))]
        event ItemsDeletedEventHandler<IUserCategory> CategoriesDeleted;
    }
}

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

using JSSoft.Library;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public interface ITypeTemplate : IEnumerable<ITypeMember>, IDispatcherObject, IPermission
    {
        Task BeginEditAsync(Authentication authentication);

        Task EndEditAsync(Authentication authentication);

        Task CancelEditAsync(Authentication authentication);

        Task SetTypeNameAsync(Authentication authentication, string value);

        Task SetIsFlagAsync(Authentication authentication, bool value);

        Task SetTagsAsync(Authentication authentication, TagInfo tags);

        Task SetCommentAsync(Authentication authentication, string value);

        Task<ITypeMember> AddNewAsync(Authentication authentication);

        Task EndNewAsync(Authentication authentication, ITypeMember member);

        bool Contains(string memberName);

        IDomain Domain { get; }

        IType Type { get; }

        int Count { get; }

        ITypeMember this[string memberName] { get; }

        string TypeName { get; }

        bool IsFlag { get; }

        string Comment { get; }

        bool IsNew { get; }

        bool IsModified { get; }

        string Editor { get; }

        event EventHandler EditBegun;

        event EventHandler EditEnded;

        event EventHandler EditCanceled;

        event EventHandler Changed;
    }
}

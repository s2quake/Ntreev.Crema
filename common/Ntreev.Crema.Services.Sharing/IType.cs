﻿//Released under the MIT License.
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

using Ntreev.Crema.Data;
using Ntreev.Crema.ServiceModel;
using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    public interface IType : IAccessible, ILockable, IPermission, IServiceProvider, IDispatcherObject, IExtendedProperties
    {
        Task RenameAsync(Authentication authentication, string newName);

        Task MoveAsync(Authentication authentication, string categoryPath);

        Task DeleteAsync(Authentication authentication);

        Task<IType> CopyAsync(Authentication authentication, string newTypeName, string categoryPath);

        Task<CremaDataSet> GetDataSetAsync(Authentication authentication, string revision);

        Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision);

        Task<FindResultInfo[]> FindAsync(Authentication authentication, string text, FindOptions options);

        string Name { get; }

        string Path { get; }

        bool IsLocked { get; }

        bool IsPrivate { get; }

        TypeInfo TypeInfo { get; }

        TypeState TypeState { get; }

        ITypeCategory Category { get; }

        ITypeTemplate Template { get; }

        event EventHandler Renamed;

        event EventHandler Moved;

        event EventHandler Deleted;

        event EventHandler LockChanged;

        event EventHandler AccessChanged;

        event EventHandler TypeInfoChanged;

        event EventHandler TypeStateChanged;
    }
}

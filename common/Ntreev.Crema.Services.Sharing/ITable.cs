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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Ntreev.Crema.Data;
using Ntreev.Library.ObjectModel;
using Ntreev.Crema.ServiceModel;
using Ntreev.Library;

namespace Ntreev.Crema.Services
{
    public interface ITable : IAccessible, ILockable, IPermission, IServiceProvider, IDispatcherObject, IExtendedProperties
    {
        Task RenameAsync(Authentication authentication, string newName);

        Task MoveAsync(Authentication authentication, string categoryPath);

        Task DeleteAsync(Authentication authentication);

        Task<ITable> CopyAsync(Authentication authentication, string newTableName, string categoryPath, bool copyContent);

        Task<ITable> InheritAsync(Authentication authentication, string newTableName, string categoryPath, bool copyContent);

        Task<ITableTemplate> NewTableAsync(Authentication authentication);

        Task<CremaDataSet> GetDataSetAsync(Authentication authentication, string revision);

        Task<LogInfo[]> GetLogAsync(Authentication authentication, string revision);

        Task<FindResultInfo[]> FindAsync(Authentication authentication, string text, FindOptions options);

        ITable Parent { get; }

        string Name { get; }

        string TableName { get; }

        string Path { get; }

        bool IsLocked { get; }

        bool IsPrivate { get; }

        TableInfo TableInfo { get; }

        TableState TableState { get; }

        ITableCategory Category { get; }

        IContainer<ITable> Childs { get; }

        IContainer<ITable> DerivedTables { get; }

        ITable TemplatedParent { get; }

        ITableTemplate Template { get; }

        ITableContent Content { get; }

        event EventHandler Renamed;

        event EventHandler Moved;

        event EventHandler Deleted;

        event EventHandler LockChanged;

        event EventHandler AccessChanged;

        event EventHandler TableInfoChanged;

        event EventHandler TableStateChanged;
    }
}

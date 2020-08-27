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

using JSSoft.Library;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services
{
    public interface ITableColumn : IDispatcherObject
    {
        Task DeleteAsync(Authentication authentication);

        Task SetIndexAsync(Authentication authentication, int value);

        Task SetIsKeyAsync(Authentication authentication, bool value);

        Task SetIsUniqueAsync(Authentication authentication, bool value);

        Task SetNameAsync(Authentication authentication, string value);

        Task SetDataTypeAsync(Authentication authentication, string value);

        Task SetDefaultValueAsync(Authentication authentication, string value);

        Task SetCommentAsync(Authentication authentication, string value);

        Task SetAutoIncrementAsync(Authentication authentication, bool value);

        Task SetTagsAsync(Authentication authentication, TagInfo value);

        Task SetIsReadOnlyAsync(Authentication authentication, bool value);

        Task SetAllowNullAsync(Authentication authentication, bool value);

        int Index { get; }

        bool IsKey { get; }

        bool IsUnique { get; }

        string Name { get; }

        string DataType { get; }

        string DefaultValue { get; }

        string Comment { get; }

        bool AutoIncrement { get; }

        TagInfo Tags { get; }

        bool IsReadOnly { get; }

        bool AllowNull { get; }

        ITableTemplate Template { get; }
    }
}

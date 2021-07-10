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

using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class TypeCollectionExtensions
    {
        public static Task<bool> ContainsAsync(this ITypeCollection typeCollection, string typeName)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.Contains(typeName));
        }

        public static Task<IType> GetTypeAsync(this ITypeCollection typeCollection, string typeName)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection[typeName]);
        }

        public static Task<IType[]> GetTypesAsync(this ITypeCollection typeCollection)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.ToArray());
        }

        public static Task<int> GetCountAsync(this ITypeCollection typeCollection)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.Count);
        }

        public static Task AddTypesStateChangedAsync(this ITypeCollection typeCollection, ItemsEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesStateChanged += handler);
        }

        public static Task AddTypesChangedAsync(this ITypeCollection typeCollection, ItemsChangedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesChanged += handler);
        }

        public static Task AddTypesCreatedAsync(this ITypeCollection typeCollection, ItemsCreatedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesCreated += handler);
        }

        public static Task AddTypesMovedAsync(this ITypeCollection typeCollection, ItemsMovedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesMoved += handler);
        }

        public static Task AddTypesRenamedAsync(this ITypeCollection typeCollection, ItemsRenamedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesRenamed += handler);
        }

        public static Task AddTypesDeletedAsync(this ITypeCollection typeCollection, ItemsDeletedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesDeleted += handler);
        }

        public static Task RemoveTypesStateChangedAsync(this ITypeCollection typeCollection, ItemsEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesStateChanged -= handler);
        }

        public static Task RemoveTypesChangedAsync(this ITypeCollection typeCollection, ItemsChangedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesChanged -= handler);
        }

        public static Task RemoveTypesCreatedAsync(this ITypeCollection typeCollection, ItemsCreatedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesCreated -= handler);
        }

        public static Task RemoveTypesMovedAsync(this ITypeCollection typeCollection, ItemsMovedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesMoved -= handler);
        }

        public static Task RemoveTypesRenamedAsync(this ITypeCollection typeCollection, ItemsRenamedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesRenamed -= handler);
        }

        public static Task RemoveTypesDeletedAsync(this ITypeCollection typeCollection, ItemsDeletedEventHandler<IType> handler)
        {
            return typeCollection.Dispatcher.InvokeAsync(() => typeCollection.TypesDeleted -= handler);
        }
    }
}

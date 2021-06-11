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
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class TypeContextExtensions
    {
        public static Task<bool> ContainsAsync(this ITypeContext typeContext, string itemPath)
        {
            return typeContext.Dispatcher.InvokeAsync(() => typeContext.Contains(itemPath));
        }

        public static Task<bool> ContainsTypeAsync(this ITypeContext typeContext, string typeName)
        {
            if (typeContext.GetService(typeof(ITypeCollection)) is ITypeCollection typeCollection)
            {
                return typeCollection.ContainsAsync(typeName);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsTypeCategoryAsync(this ITypeContext typeContext, string categoryPath)
        {
            if (typeContext.GetService(typeof(ITypeCategoryCollection)) is ITypeCategoryCollection typeCategoryCollection)
            {
                return typeCategoryCollection.ContainsAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeItem> GetTypeItemAsync(this ITypeContext typeContext, string itemPath)
        {
            return typeContext.Dispatcher.InvokeAsync(() => typeContext[itemPath]);
        }

        public static Task<ITypeItem[]> GetTypeItemsAsync(this ITypeContext typeContext)
        {
            return typeContext.Dispatcher.InvokeAsync(() => typeContext.ToArray());
        }

        public static Task<IType> GetTypeAsync(this ITypeContext typeContext, string typeName)
        {
            if (typeContext.GetService(typeof(ITypeCollection)) is ITypeCollection typeCollection)
            {
                return typeCollection.GetTypeAsync(typeName);
            }
            throw new NotImplementedException();
        }

        public static Task<IType[]> GetTypesAsync(this ITypeContext typeContext)
        {
            if (typeContext.GetService(typeof(ITypeCollection)) is ITypeCollection typeCollection)
            {
                return typeCollection.GetTypesAsync();
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeCategory> GetTypeCategoryAsync(this ITypeContext typeContext, string categoryPath)
        {
            if (typeContext.GetService(typeof(ITypeCategoryCollection)) is ITypeCategoryCollection typeCategoryCollection)
            {
                return typeCategoryCollection.GetCategoryAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeCategory[]> GetTypeCategoriesAsync(this ITypeContext typeContext)
        {
            if (typeContext.GetService(typeof(ITypeCategoryCollection)) is ITypeCategoryCollection typeCategoryCollection)
            {
                return typeCategoryCollection.GetCategoriesAsync();
            }
            throw new NotImplementedException();
        }
    }
}

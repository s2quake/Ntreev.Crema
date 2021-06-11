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
    public static class DataBaseExtensions
    {
        public static Task<bool> ContainsAsync(this IDataBase dataBase, Authentication authentication)
        {
            return dataBase.Dispatcher.InvokeAsync(() => dataBase.Contains(authentication));
        }

        public static Task<bool> ContainsTableItemAsync(this IDataBase dataBase, string itemPath)
        {
            if (dataBase.GetService(typeof(ITableContext)) is ITableContext tableContext)
            {
                return tableContext.ContainsAsync(itemPath);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsTableAsync(this IDataBase dataBase, string tableName)
        {
            if (dataBase.GetService(typeof(ITableCollection)) is ITableCollection tableCollection)
            {
                return tableCollection.ContainsAsync(tableName);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsTableCategoryAsync(this IDataBase dataBase, string categoryPath)
        {
            if (dataBase.GetService(typeof(ITableCategoryCollection)) is ITableCategoryCollection tableCategoryCollection)
            {
                return tableCategoryCollection.ContainsAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsTypeItemAsync(this IDataBase dataBase, string itemPath)
        {
            if (dataBase.GetService(typeof(ITypeContext)) is ITypeContext typeContext)
            {
                return typeContext.ContainsAsync(itemPath);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsTypeAsync(this IDataBase dataBase, string typeName)
        {
            if (dataBase.GetService(typeof(ITypeCollection)) is ITypeCollection typeCollection)
            {
                return typeCollection.ContainsAsync(typeName);
            }
            throw new NotImplementedException();
        }

        public static Task<bool> ContainsTypeCategoryAsync(this IDataBase dataBase, string categoryPath)
        {
            if (dataBase.GetService(typeof(ITypeCategoryCollection)) is ITypeCategoryCollection typeCategoryCollection)
            {
                return typeCategoryCollection.ContainsAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITableItem> GetTableItemAsync(this IDataBase dataBase, string itemPath)
        {
            if (dataBase.GetService(typeof(ITableContext)) is ITableContext tableContext)
            {
                return tableContext.GetTableItemAsync(itemPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITableItem[]> GetTableItemsAsync(this IDataBase dataBase)
        {
            if (dataBase.GetService(typeof(ITableContext)) is ITableContext tableContext)
            {
                return tableContext.GetTableItemsAsync();
            }
            throw new NotImplementedException();
        }

        public static Task<ITable> GetTableAsync(this IDataBase dataBase, string tableName)
        {
            if (dataBase.GetService(typeof(ITableCollection)) is ITableCollection tableCollection)
            {
                return tableCollection.GetTableAsync(tableName);
            }
            throw new NotImplementedException();
        }

        public static Task<ITable[]> GetTablesAsync(this IDataBase dataBase)
        {
            if (dataBase.GetService(typeof(ITableCollection)) is ITableCollection tableCollection)
            {
                return tableCollection.GetTablesAsync();
            }
            throw new NotImplementedException();
        }

        public static Task<ITableCategory> GetTableCategoryAsync(this IDataBase dataBase, string categoryPath)
        {
            if (dataBase.GetService(typeof(ITableCategoryCollection)) is ITableCategoryCollection tableCategoryCollection)
            {
                return tableCategoryCollection.GetCategoryAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITableCategory[]> GetTableCategoriesAsync(this IDataBase dataBase)
        {
            if (dataBase.GetService(typeof(ITableCategoryCollection)) is ITableCategoryCollection tableCategoryCollection)
            {
                return tableCategoryCollection.GetCategoriesAsync();
            }
            throw new NotImplementedException();
        }
        
        public static Task<ITypeItem> GetTypeItemAsync(this IDataBase dataBase, string itemPath)
        {
            if (dataBase.GetService(typeof(ITypeContext)) is ITypeContext typeContext)
            {
                return typeContext.GetTypeItemAsync(itemPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeItem[]> GetTypeItemsAsync(this IDataBase dataBase)
        {
            if (dataBase.GetService(typeof(ITypeContext)) is ITypeContext typeContext)
            {
                return typeContext.GetTypeItemsAsync();
            }
            throw new NotImplementedException();
        }

        public static Task<IType> GetTypeAsync(this IDataBase dataBase, string typeName)
        {
            if (dataBase.GetService(typeof(ITypeCollection)) is ITypeCollection typeCollection)
            {
                return typeCollection.GetTypeAsync(typeName);
            }
            throw new NotImplementedException();
        }

        public static Task<IType[]> GetTypesAsync(this IDataBase dataBase)
        {
            if (dataBase.GetService(typeof(ITypeCollection)) is ITypeCollection typeCollection)
            {
                return typeCollection.GetTypesAsync();
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeCategory> GetTypeCategoryAsync(this IDataBase dataBase, string categoryPath)
        {
            if (dataBase.GetService(typeof(ITypeCategoryCollection)) is ITypeCategoryCollection typeCategoryCollection)
            {
                return typeCategoryCollection.GetCategoryAsync(categoryPath);
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeCategory[]> GetTypeCategoriesAsync(this IDataBase dataBase)
        {
            if (dataBase.GetService(typeof(ITypeCategoryCollection)) is ITypeCategoryCollection typeCategoryCollection)
            {
                return typeCategoryCollection.GetCategoriesAsync();
            }
            throw new NotImplementedException();
        }
    }
}

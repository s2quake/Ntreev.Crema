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
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Random
{
    public static class DataBaseExtensions
    {
        static DataBaseExtensions()
        {

        }

        public static Task InitializeRandomItemsAsync(this IDataBase dataBase, Authentication authentication)
        {
            return InitializeRandomItemsAsync(dataBase, authentication, false);
        }

        public static async Task InitializeRandomItemsAsync(this IDataBase dataBase, Authentication authentication, bool transaction)
        {
            if (transaction == true)
                await InitializeRandomItemsTransactionAsync(dataBase, authentication);
            else
                await InitializeRandomItemsStandardAsync(dataBase, authentication);
        }

        public static Task GenerateTableAsync(this IDataBase dataBase, Authentication authentication)
        {
            if (dataBase.GetService(typeof(ITableContext)) is ITableContext tableContext)
            {
                return tableContext.GenerateAsync(authentication);
            }
            throw new NotImplementedException();
        }

        public static Task GenerateTypeAsync(this IDataBase dataBase, Authentication authentication)
        {
            if (dataBase.GetService(typeof(ITypeContext)) is ITypeContext typeContext)
            {
                return typeContext.GenerateAsync(authentication);
            }
            throw new NotImplementedException();
        }

        public static Task<ITableItem> GetRandomTableItemAsync(this IDataBase dataBase)
        {
            return GetRandomTableItemAsync(dataBase, DefaultPredicate);
        }

        public static Task<ITableItem> GetRandomTableItemAsync(this IDataBase dataBase, Func<ITableItem, bool> predicate)
        {
            if (dataBase.GetService(typeof(ITableContext)) is ITableContext tableContext)
            {
                return tableContext.GetRandomTableItemAsync(predicate);
            }
            throw new NotImplementedException();
        }

        public static Task<ITable> GetRandomTableAsync(this IDataBase dataBase)
        {
            return GetRandomTableAsync(dataBase, DefaultPredicate);
        }

        public static Task<ITable> GetRandomTableAsync(this IDataBase dataBase, Func<ITable, bool> predicate)
        {
            if (dataBase.GetService(typeof(ITableCollection)) is ITableCollection tableCollection)
            {
                return tableCollection.GetRandomTableAsync(predicate);
            }
            throw new NotImplementedException();
        }

        public static Task<ITableCategory> GetRandomTableCategoryAsync(this IDataBase dataBase)
        {
            return GetRandomTableCategoryAsync(dataBase, DefaultPredicate);
        }

        public static Task<ITableCategory> GetRandomTableCategoryAsync(this IDataBase dataBase, Func<ITableCategory, bool> predicate)
        {
            if (dataBase.GetService(typeof(ITableCategoryCollection)) is ITableCategoryCollection tableCategoryCollection)
            {
                return tableCategoryCollection.GetRandomTableCategoryAsync(predicate);
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeItem> GetRandomTypeItemAsync(this IDataBase dataBase)
        {
            return GetRandomTypeItemAsync(dataBase, DefaultPredicate);
        }

        public static Task<ITypeItem> GetRandomTypeItemAsync(this IDataBase dataBase, Func<ITypeItem, bool> predicate)
        {
            if (dataBase.GetService(typeof(ITypeContext)) is ITypeContext typeContext)
            {
                return typeContext.GetRandomTypeItemAsync(predicate);
            }
            throw new NotImplementedException();
        }
    
        public static Task<IType> GetRandomTypeAsync(this IDataBase dataBase)
        {
            return GetRandomTypeAsync(dataBase, DefaultPredicate);
        }

        public static Task<IType> GetRandomTypeAsync(this IDataBase dataBase, Func<IType, bool> predicate)
        {
            if (dataBase.GetService(typeof(ITypeCollection)) is ITypeCollection typeCollection)
            {
                return typeCollection.GetRandomTypeAsync(predicate);
            }
            throw new NotImplementedException();
        }

        public static Task<ITypeCategory> GetRandomTypeCategoryAsync(this IDataBase dataBase)
        {
            return GetRandomTypeCategoryAsync(dataBase, DefaultPredicate);
        }

        public static Task<ITypeCategory> GetRandomTypeCategoryAsync(this IDataBase dataBase, Func<ITypeCategory, bool> predicate)
        {
            if (dataBase.GetService(typeof(ITypeCategoryCollection)) is ITypeCategoryCollection typeCategoryCollection)
            {
                return typeCategoryCollection.GetRandomTypeCategoryAsync(predicate);
            }
            throw new NotImplementedException();
        }

        private static async Task InitializeRandomItemsTransactionAsync(this IDataBase dataBase, Authentication authentication)
        {
            var trans = await dataBase.BeginTransactionAsync(authentication);
            var typeContext = dataBase.GetService(typeof(ITypeContext)) as ITypeContext;
            var tableContext = dataBase.GetService(typeof(ITableContext)) as ITableContext;
            await typeContext.AddRandomItemsAsync(authentication);
            await tableContext.AddRandomItemsAsync(authentication);
            await trans.CommitAsync(authentication);
        }

        private static async Task InitializeRandomItemsStandardAsync(this IDataBase dataBase, Authentication authentication)
        {
            var typeContext = dataBase.GetService(typeof(ITypeContext)) as ITypeContext;
            var tableContext = dataBase.GetService(typeof(ITableContext)) as ITableContext;
            await typeContext.AddRandomItemsAsync(authentication);
            await tableContext.AddRandomItemsAsync(authentication);
        }

        private static bool DefaultPredicate(ITableItem _) => true;

        private static bool DefaultPredicate(ITable _) => true;

        private static bool DefaultPredicate(ITableCategory _) => true;

        private static bool DefaultPredicate(ITypeItem _) => true;

        private static bool DefaultPredicate(IType _) => true;

        private static bool DefaultPredicate(ITypeCategory _) => true;
    }
}
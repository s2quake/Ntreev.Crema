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

using JSSoft.Library;
using JSSoft.Library.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Extensions
{
    public static class TypeCategoryExtensions
    {
        public static Task<IType[]> GetAllTypesAsync(this ITypeCategory category, Func<IType, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var query = from item in EnumerableUtility.FamilyTree<ITypeItem, IType>(category as ITypeItem, item => item.Childs)
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }

        //public static Task<ITable[]> GetAllRelationTablesAsync(this ITableCategory category, Func<ITable, bool> predicate)
        //{
        //    return category.Dispatcher.InvokeAsync(() =>
        //    {
        //        var tables = from item in EnumerableUtility.FamilyTree<ITableItem, ITable>(category as ITableItem, item => item.Childs)
        //                     select item;
        //        var allTables = tables.SelectMany(item => TableExtensions.GetAllRelationTables(item)).Distinct().OrderBy(item => item.Name);
        //        var query = from item in allTables
        //                    where predicate(item)
        //                    select item;
        //        return query.ToArray();
        //    });
        //}

        public static Task<ITable[]> GetAllUsingTablesAsync(this ITypeCategory category, Func<ITable, bool> predicate)
        {
            return category.Dispatcher.InvokeAsync(() =>
            {
                var types = EnumerableUtility.FamilyTree<ITypeItem, IType>(category as ITypeItem, item => item.Childs);
                var tables = types.SelectMany(item => TypeExtensions.GetTables(item)).Distinct();
                var query = from item in tables
                            where predicate(item)
                            select item;
                return query.ToArray();
            });
        }

        public static async Task<ITypeCategory> AddNewCategoryAsync(this ITypeCategory category, Authentication authentication)
        {
            var newName = NameUtility.GenerateNewName("Folder", category.Categories.Select(item => item.Name));
            return await category.AddNewCategoryAsync(authentication, newName);
        }
    }
}
